using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using RaaiVan.Modules.GlobalUtilities;
using StackExchange.Redis;

namespace RaaiVan.Web.API
{
    public class RedisAPI
    {
        private static ConnectionMultiplexer Redis = null;

        private static bool init()
        {
            string hosts = RaaiVanSettings.RedisHosts.Trim();
            if (!string.IsNullOrEmpty(hosts) && Redis == null) Redis = ConnectionMultiplexer.Connect(hosts);
            return Redis != null;
        }

        private static IDatabase get_database()
        {
            return !init() ? null : Redis.GetDatabase();
        }

        public static bool Enabled
        {
            get { return init(); }
        }

        public static void set_value<T>(string key, T value)
        {
            IDatabase db = get_database();

            if (db != null && value != null && value.GetType().IsSerializable && !string.IsNullOrEmpty(key))
            {
                string str = JsonConvert.SerializeObject(value);
                if (!string.IsNullOrEmpty(str)) db.StringSet(key, str);
            }
        }

        public static void set_value(string key, string value)
        {
            set_value<string>(key, value);
        }

        public static T get_value<T>(string key)
        {
            IDatabase db = get_database();

            if (db != null && !string.IsNullOrEmpty(key))
            {
                RedisValue value = db.StringGet(key);

                if (!value.IsNullOrEmpty)
                    return JsonConvert.DeserializeObject<T>(value);
            }

            return default(T);
        }

        public static string get_value(string key)
        {
            return get_value<string>(key);
        }

        public static bool remove_key(string key)
        {
            IDatabase db = get_database();
            return db != null && !string.IsNullOrEmpty(key) && db.KeyDelete(new RedisKey(key));
        }
    }
}