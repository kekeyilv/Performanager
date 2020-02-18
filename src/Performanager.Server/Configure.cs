using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Performanager.Server
{
    public class Configure
    {
        public Dictionary<string, string> Bind { get; set; } = new Dictionary<string, string>();
        public List<Dictionary<string,string>> Clients { get; set; } = new List<Dictionary<string, string>>();
        public string Log { get; set; }
        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this));
        }
    }
}
