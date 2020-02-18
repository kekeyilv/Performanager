using System;

namespace Performanager.Client
{
    public static class Platform
    {
        static Platform()
        {
            Platformtype = (Environment.OSVersion.Platform.ToString()) switch
            {
                "Win32NT" => PlatformType.Windows,
                "Unix" => PlatformType.Unix,
                _ => PlatformType.Other,
            };
        }
        public enum PlatformType
        {
            Other,
            Windows,
            Unix
        }
        public static PlatformType Platformtype { get; }
    }
}
