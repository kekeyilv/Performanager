using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Performanager.Client
{
    public static class Program
    {
        public static Logger logger = null;
        static void Main()
        {
            if(Platform.Platformtype==Platform.PlatformType.Other)
            {
                Console.WriteLine("Unsupport platform.");
                return;
            }
            else
            {
                Console.WriteLine("Performanager on " + Platform.Platformtype + ". Version " + Assembly.GetExecutingAssembly().GetName().Version);
            }
            const string config = "Config.json";
            Configure configure = new Configure();
            logger = new Logger(configure);
            if(File.Exists(config))
            {
                configure = JsonConvert.DeserializeObject<Configure>(File.ReadAllText(config));
            }
            Listen.StartListen(configure);
        }
    }
}
