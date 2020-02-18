using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Performanager.Client
{
    public static class Processes
    {
        public static Dictionary<int, string> GetProcesses()
        {
            var result = new Dictionary<int, string>();
            foreach (Process process in Process.GetProcesses())
            {
                result.Add(process.Id, process.ProcessName);
            }
            return result;
        }
        public static long GetProcessMemory(int id)
        {
            return Process.GetProcessById(id).WorkingSet64;
        }
        public static double GetProcessCPU(int id)
        {
            var time = Process.GetProcessById(id).TotalProcessorTime;
            Thread.Sleep(1000);
            return (Process.GetProcessById(id).TotalProcessorTime - time).TotalMilliseconds / Environment.ProcessorCount / 10;
        }
        public static void Kill(int id)
        {
            Process.GetProcessById(id).Kill();
        }
    }
}
