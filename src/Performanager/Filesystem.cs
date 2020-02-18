using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Performanager.Client
{
    public static class Filesystem
    {
        public static List<string> GetAll()
        {
            List<string> result = new List<string>();
            foreach(var drive in DriveInfo.GetDrives())
            {
                result.Add(drive.Name);
            }
            return result;
        }
        public static long GetFree(string drivename)
        {
            try
            {
                if(Platform.Platformtype==Platform.PlatformType.Unix)
                {
                    drivename = drivename.Replace("|", "/");
                }
                else
                {
                    drivename = drivename.Replace("|", "\\");
                }
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.Name == drivename)
                    {
                        return drive.AvailableFreeSpace;
                    }
                }  
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return 0;
        }
        public static long GetTotal(string drivename)
        {
            if (Platform.Platformtype == Platform.PlatformType.Unix)
            {
                drivename = drivename.Replace("|", "/");
            }
            else
            {
                drivename = drivename.Replace("|", "\\");
            }
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.Name == drivename)
                {
                    return drive.TotalSize;
                }
            }
            return 0;
        }
    }
}
