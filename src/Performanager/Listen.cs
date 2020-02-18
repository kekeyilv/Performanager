using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Performanager.Client
{
    public static class Listen
    {
        public static void StartListen(Configure configure)
        {
            HttpListener listener = new HttpListener();
            if (!configure.Bind.ContainsKey("IP"))
            {
                configure.Bind.Add("IP", "0.0.0.0");
            }
            if (!configure.Bind.ContainsKey("Port"))
            {
                configure.Bind.Add("Port", "2086");
            }
            listener.Prefixes.Add("http://" + configure.Bind["IP"] + ":" + configure.Bind["Port"] + "/");
            listener.Start();
            new Thread(() =>
                {
                    while (true)
                    {
                        Process(configure, listener);
                    }
                }
            ).Start();
        }
        public static void Process(Configure configure, HttpListener listener)
        {
            try
            {
                var context = listener.GetContext();
                Task.Run(() =>
                    {
                        try
                        {
                            context.Response.Headers["Server"] = "Performanager";
                            if (configure.Server.ContainsKey("IP"))
                            {
                                if (!string.IsNullOrWhiteSpace(configure.Server["IP"]))
                                {
                                    if (context.Request.RemoteEndPoint.Address.ToString() != configure.Server["IP"])
                                    {
                                        context.Response.StatusCode = 403;
                                        Program.logger.Write(Logger.Level.Warning, "Rejected request from " + context.Request.RemoteEndPoint.Address);
                                        return;
                                    }
                                }
                            }
                            Program.logger.Write(Logger.Level.Info, "Request " + context.Request.Url + " from " + context.Request.RemoteEndPoint.Address);
                            var urls = context.Request.RawUrl.Split('/');
                            switch (urls[1].ToLower())
                            {
                                case "performance":
                                    var method = typeof(Performance).GetMethod(urls[2].Split('?')[0]);
                                    object value = method.Invoke(null, ModelBinding(method,context).ToArray());
                                    if (!Equals(method.ReturnType, typeof(void)))
                                    {
                                        using var stream = context.Response.OutputStream;
                                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
                                    }
                                    else 
                                    {
                                        context.Response.OutputStream.Dispose();
                                    }
                                    break;
                                case "process":
                                    method = typeof(Processes).GetMethod(urls[2].Split('?')[0]);
                                    value = method.Invoke(null, ModelBinding(method, context).ToArray());
                                    if (!Equals(method.ReturnType, typeof(void)))
                                    {
                                        using var stream = context.Response.OutputStream;
                                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
                                    }
                                    else
                                    {
                                        context.Response.OutputStream.Dispose();
                                    }
                                    break;
                                case "file":
                                    method = typeof(Filesystem).GetMethod(urls[2].Split('?')[0]);
                                    value = method.Invoke(null, ModelBinding(method, context).ToArray());
                                    if (!Equals(method.ReturnType, typeof(void)))
                                    {
                                        using var stream = context.Response.OutputStream;
                                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
                                    }
                                    else
                                    {
                                        context.Response.OutputStream.Dispose();
                                    }
                                    break;
                                case "platform":
                                    using (var stream = context.Response.OutputStream)
                                    {
                                        stream.Write(Encoding.UTF8.GetBytes(Platform.Platformtype.ToString()));
                                    }
                                    break;
                                case "version":
                                    using (var stream = context.Response.OutputStream)
                                    {
                                        stream.Write(Encoding.UTF8.GetBytes(Assembly.GetExecutingAssembly().GetName().Version.ToString()));
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.logger.Write(Logger.Level.Error, ex.Message);
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                Program.logger.Write(Logger.Level.Error, ex.Message);
            }
        }
        public static List<object> ModelBinding(MethodInfo method,HttpListenerContext context)
        {
            List<object> parameters = new List<object>();
            foreach (var param in method.GetParameters())
            {
                foreach (var query in context.Request.QueryString.AllKeys)
                {
                    if (param.Name == query)
                    {
                        foreach (var convert in typeof(JsonConvert).GetMethods())
                        {
                            if (convert.Name == "DeserializeObject" && convert.GetParameters().Length == 1 && convert.ContainsGenericParameters)
                            {
                                parameters.Add(convert.MakeGenericMethod(param.ParameterType).Invoke(null, new object[] { context.Request.QueryString[query] }));
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            return parameters;
        }
    }
}
