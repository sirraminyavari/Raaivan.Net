using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Web
{
    public class RedisSessionStateProviderSettings
    {
        public static string getConnectionString()
        {
            return string.IsNullOrEmpty(RaaiVanSettings.Redis.Hosts) ? string.Empty : RaaiVanSettings.Redis.Hosts + 
                (string.IsNullOrEmpty(RaaiVanSettings.Redis.Password) ? string.Empty : ",password=" + RaaiVanSettings.Redis.Password);
        }
    }
}