using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading;

namespace Performanager.Client
{
    public static class Performance
    {
        public static long GetMemoryTotal()
        {
            switch (Platform.Platformtype)
            {
                case Platform.PlatformType.Windows:
                    ManagementClass management = new ManagementClass("Win32_PhysicalMemory");
                    ManagementObjectCollection managementObjects = management.GetInstances();
                    long capacity = 0;
                    foreach (ManagementObject managementObject in managementObjects)
                    {
                        capacity += long.Parse(managementObject.Properties["Capacity"].Value.ToString());
                    }
                    managementObjects.Dispose();
                    management.Dispose();
                    return capacity;
                case Platform.PlatformType.Unix:
                    string meminfo = File.ReadAllText("/proc/meminfo");
                    meminfo = meminfo.Replace(" ", "").Replace("kB", "");
                    foreach (string item in meminfo.Split('\n'))
                    {
                        string[] pair = item.Split(':');
                        if (pair[0] == "MemTotal")
                        {
                            return long.Parse(pair[1]) * 1024;
                        }
                    }
                    break;
            }
            return 0;
        }
        public static long GetMemoryFree()
        {
            switch (Platform.Platformtype)
            {
                case Platform.PlatformType.Windows:
                    ManagementClass management = new ManagementClass("Win32_PerfFormattedData_PerfOS_Memory");
                    ManagementObjectCollection managementObjects = management.GetInstances();
                    long capacity = 0;
                    foreach (ManagementObject managementObject in managementObjects)
                    {
                        capacity += long.Parse(managementObject.Properties["AvailableBytes"].Value.ToString());
                    }
                    managementObjects.Dispose();
                    management.Dispose();
                    return capacity;
                case Platform.PlatformType.Unix:
                    string meminfo = File.ReadAllText("/proc/meminfo");
                    meminfo = meminfo.Replace(" ", "").Replace("kB", "");
                    foreach (string item in meminfo.Split('\n'))
                    {
                        string[] pair = item.Split(':');
                        if (pair[0] == "MemFree")
                        {
                            return long.Parse(pair[1]) * 1024;
                        }
                    }
                    break;
            }
            return 0;
        }
        static readonly object performancelock = new object();
        public static double GetCPU()
        {
            switch (Platform.Platformtype)
            {
                case Platform.PlatformType.Windows:
                    lock (performancelock)
                    {
                        using PerformanceCounter counter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                        counter.NextValue();
                        Thread.Sleep(500);
                        return counter.NextValue() + 10;
                    }
                case Platform.PlatformType.Unix:
                    static double GetCPUTime()
                    {
                        var infos = File.ReadAllText("/proc/uptime").Split(' ');
                        var value0 = double.Parse(infos[0]);
                        var value1 = double.Parse(infos[1]);
                        return Math.Max(value0,value1)-Math.Min(value0,value1);
                    }
                    var time = GetCPUTime();
                    Thread.Sleep(500);
                    return 0.5 / (GetCPUTime() - time);
            }
            return 0;
        }
        public static double GetNetworkSent()
        {
            switch (Platform.Platformtype)
            {
                case Platform.PlatformType.Windows:
                    double result = 0;
                    lock (performancelock)
                    {
                        foreach (var category in PerformanceCounterCategory.GetCategories())
                        {
                            if (category.CategoryName == "Network Interface")
                            {
                                foreach (var name in category.GetInstanceNames())
                                {
                                    using PerformanceCounter counter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", name, true);
                                    counter.NextValue();
                                    Thread.Sleep(500);
                                    result += counter.NextValue();
                                }
                            }
                        }
                        return result;
                    }
                case Platform.PlatformType.Unix:
                    static long GetSentTotal()
                    {
                        long total = 0;
                        var infos = File.ReadAllLines("/proc/net/dev");
                        for (int i = 2; i < infos.Length; i++)
                        {
                            string value = infos[i].Split(':')[1];
                            while (value.IndexOf("  ") != -1)
                            {
                                value = value.Replace("  ", " ");
                            }
                            total += long.Parse(value.Split(' ')[9]);
                        }
                        return total;
                    }
                    var total = GetSentTotal();
                    Thread.Sleep(500);
                    return (GetSentTotal() - total)*2;
            }
            return 0;
        }
        public static double GetNetworkReceived()
        {
            switch (Platform.Platformtype)
            {
                case Platform.PlatformType.Windows:
                    double result = 0;
                    lock (performancelock)
                    {
                        foreach (var category in PerformanceCounterCategory.GetCategories())
                        {
                            if (category.CategoryName == "Network Interface")
                            {
                                foreach (var name in category.GetInstanceNames())
                                {
                                    using PerformanceCounter counter = new PerformanceCounter("Network Interface", "Bytes Received/sec", name, true);
                                    counter.NextValue();
                                    Thread.Sleep(500);
                                    result += counter.NextValue();
                                }
                            }
                        }
                        return result;
                    }
                case Platform.PlatformType.Unix:
                    static long GetReceivedTotal()
                    {
                        long total = 0;
                        var infos = File.ReadAllLines("/proc/net/dev");
                        for (int i = 2; i < infos.Length; i++)
                        {
                            string value = infos[i].Split(':')[1];
                            while (value.IndexOf("  ") != -1)
                            {
                                value = value.Replace("  ", " ");
                            }
                            total += long.Parse(value.Split(' ')[1]);

                        }
                        return total;
                    }
                    var total = GetReceivedTotal();
                    Thread.Sleep(500);
                    return (GetReceivedTotal() - total)*2;
            }
            return 0;
        }
        public static List<string> GetAdapters()
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            var names = new List<string>();
            foreach (NetworkInterface adapter in adapters)
            {
                names.Add(adapter.Description.Replace('(', '[').Replace(')', ']'));
            }
            return names;
        }
    }
}
