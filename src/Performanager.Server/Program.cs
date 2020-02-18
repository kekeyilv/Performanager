using Newtonsoft.Json;
using System;
using System.IO;

namespace Performanager.Server
{
    public static class Program
    {
        public static Logger logger = null;
        static void Main()
        {
            const string config = "Config.json";
            Configure configure = new Configure();
            logger = new Logger(configure);
            if (File.Exists(config))
            {
                configure = JsonConvert.DeserializeObject<Configure>(File.ReadAllText(config));
            }
            Listen.StartListen(configure);
        }
    }
}
