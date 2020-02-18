using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Performanager.Server
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
                Task.Run(async () =>
                    {
                        try
                        {
                            context.Response.Headers.Add("Server", "");
                            Program.logger.Write(Logger.Level.Info, "Request " + context.Request.Url + " from " + context.Request.RemoteEndPoint.Address);
                            var urls = context.Request.RawUrl.Split('/');
                            if (context.Request.RawUrl == "/")
                            {
                                WritePage("Index.html", context);
                                return;
                            }
                            switch (context.Request.HttpMethod.ToLower())
                            {
                                case "get":
                                    string path = context.Request.RawUrl.Split('?')[0].Remove(0, 1);
                                    if (!File.Exists("wwwroot/" + path))
                                    {
                                        context.Response.StatusCode = 404;
                                        Program.logger.Write(Logger.Level.Error, "Can not find wwwroot/" + path + ". It will redirect to wwwroot/404.html.");
                                        WritePage("404.html", context);
                                        return;
                                    }
                                    if (path.EndsWith(".html"))
                                    {
                                        if (path.StartsWith("Manage", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            WriteManagePage(path, context, configure);
                                            return;
                                        }
                                        WritePage(path, context);
                                    }
                                    else
                                    {
                                        if (path.EndsWith(".css"))
                                        {
                                            context.Response.ContentType = "text/css";
                                        }
                                        if (path.EndsWith(".svg"))
                                        {
                                            context.Response.ContentType = "image/svg+xml";
                                        }
                                        else if (path.EndsWith(".ico"))
                                        {
                                            context.Response.ContentType = "image/x-icon";
                                        }
                                        var buffer = File.ReadAllBytes("wwwroot/" + path);
                                        context.Response.ContentLength64 = buffer.LongLength;
                                        using var stream = context.Response.OutputStream;
                                        stream.Write(buffer);
                                        buffer = null;
                                        GC.Collect();
                                    }
                                    break;
                                case "post":
                                    if (urls[1] == "all")
                                    {
                                        using var stream = context.Response.OutputStream;
                                        var clients = new List<Dictionary<string, string>>();
                                        foreach (var before in configure.Clients)
                                        {
                                            var dictionary = new Dictionary<string, string>();
                                            foreach (var pair in before)
                                            {
                                                dictionary.Add(pair.Key, pair.Value);
                                            }
                                            clients.Add(dictionary);
                                        }
                                        HttpClient httpclient = new HttpClient();
                                        for (int i = 0; i < clients.Count; i++)
                                        {
                                            var client = clients[i];
                                            try
                                            {
                                                client.Add("Platform", await httpclient.GetStringAsync("http://" + client["IP"] + ":" + client["Port"] + "/platform"));
                                            }
                                            catch (Exception)
                                            {
                                                clients.Remove(client);
                                                Program.logger.Write(Logger.Level.Error, "Can not connect to " + client["IP"]);
                                                i -= 1;
                                            }
                                        }
                                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(clients)));
                                    }
                                    else if (urls[1] == "add")
                                    {
                                        var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(new StreamReader(context.Request.InputStream).ReadToEnd());
                                        foreach (var client in configure.Clients)
                                        {
                                            if (client["IP"] == data["IP"] || client["Name"] == data["Name"] || string.IsNullOrEmpty(data["IP"]) || string.IsNullOrEmpty(data["Port"]) || string.IsNullOrEmpty(data["Name"]))
                                            {
                                                context.Response.StatusCode = 500;
                                                context.Response.OutputStream.Dispose();
                                                return;
                                            }
                                        }
                                        configure.Clients.Add(data);
                                        configure.Save("Config.json");
                                        context.Response.OutputStream.Dispose();
                                    }
                                    else if (urls[1].Split('?')[0] == "info")
                                    {
                                        string response = "";
                                        foreach (var client in configure.Clients)
                                        {
                                            if (client["Name"] == context.Request.QueryString["Name"])
                                            {
                                                HttpClient httpclient = new HttpClient();
                                                try
                                                {
                                                    response += "Platform: " + await httpclient.GetStringAsync("http://" + client["IP"] + ":" + client["Port"] + "/platform" )+ "\r\n";
                                                    response += "Version: " + await httpclient.GetStringAsync("http://" + client["IP"] + ":" + client["Port"] + "/version");
                                                    using var stream = context.Response.OutputStream;
                                                    stream.Write(Encoding.UTF8.GetBytes(response));
                                                    return;
                                                }
                                                catch (Exception)
                                                {
                                                    Program.logger.Write(Logger.Level.Error, "Can not connect to " + client["IP"]);
                                                    context.Response.StatusCode = 500;
                                                    context.Response.OutputStream.Dispose();
                                                    return;
                                                }
                                            }
                                        }
                                        context.Response.StatusCode = 500;
                                        context.Response.OutputStream.Dispose();
                                    }
                                    else if(urls[1] == "performance")
                                    {
                                        HttpClient httpclient = new HttpClient();
                                        foreach (var client in configure.Clients)
                                        {
                                            if (client["Name"] == context.Request.QueryString["Name"])
                                            {
                                                using var stream = context.Response.OutputStream;
                                                stream.Write(Encoding.UTF8.GetBytes(await httpclient.GetStringAsync("http://" + client["IP"] + ":" + client["Port"] + "/performance/"+urls[2])));
                                                return;
                                            }
                                        }
                                    }
                                    else if (urls[1] == "process")
                                    {
                                        HttpClient httpclient = new HttpClient();
                                        foreach (var client in configure.Clients)
                                        {
                                            if (client["Name"] == context.Request.QueryString["Name"])
                                            {
                                                using var stream = context.Response.OutputStream;
                                                stream.Write(Encoding.UTF8.GetBytes(await httpclient.GetStringAsync("http://" + client["IP"] + ":" + client["Port"] + "/process/" + urls[2])));
                                                return;
                                            }
                                        }
                                    }
                                    else if (urls[1] == "file")
                                    {
                                        HttpClient httpclient = new HttpClient();
                                        foreach (var client in configure.Clients)
                                        {
                                            if (client["Name"] == context.Request.QueryString["Name"])
                                            {
                                                using var stream = context.Response.OutputStream;
                                                stream.Write(Encoding.UTF8.GetBytes(await httpclient.GetStringAsync("http://" + client["IP"] + ":" + client["Port"] + "/file/" + urls[2])));
                                                return;
                                            }
                                        }
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
        static void WritePage(string path, HttpListenerContext context)
        {
            using var stream = context.Response.OutputStream;
            stream.Write(Encoding.UTF8.GetBytes(string.Format(File.ReadAllText("wwwroot/Model.html"), File.ReadAllText("wwwroot/" + path))));
        }
        static void WriteManagePage(string path, HttpListenerContext context, Configure configure)
        {
            foreach (var client in configure.Clients)
            {
                if (client["Name"] == context.Request.QueryString["Name"])
                {
                    using var stream = context.Response.OutputStream;
                    stream.Write(Encoding.UTF8.GetBytes(string.Format(string.Format(File.ReadAllText("wwwroot/Model.html"), File.ReadAllText("wwwroot/ManageModel.html")), context.Request.QueryString["Name"], client["IP"], File.ReadAllText("wwwroot/" + path))));
                    return;
                }
            }
            context.Response.StatusCode = 404;
            WritePage("404.html", context);
        }
    }
}
