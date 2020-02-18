using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Performanager.Server
{
    public class Logger
    {
        public enum Level
        {
            Info,
            Warning,
            Error
        }
        public Logger(Configure configure)
        {
            if(string.IsNullOrEmpty(configure.Log))
            {
                Writer = Console.Out;
            }
            else
            {
                Writer = new StreamWriter(new FileStream(configure.Log, FileMode.OpenOrCreate));
            }
        }
        public TextWriter Writer { get; }
        public void Write(Level level,string content)
        {
            Writer.WriteLine(DateTime.Now+" [ "+ level +" ] "+ content+"\r\n");
        }
    }
}
