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
            return RaaiVanSettings.RedisHosts;
        }
    }
}