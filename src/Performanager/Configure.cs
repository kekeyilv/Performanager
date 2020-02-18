using System;
using System.Collections.Generic;
using System.Text;

namespace Performanager.Client
{
    public class Configure
    {
        public Dictionary<string, string> Bind { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Server { get; set; } = new Dictionary<string, string>();
        public string Log { get; set; }
    }
}
