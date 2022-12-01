using System;
using System.IO;
using System.Web.Configuration;

namespace InvoiceServer.Common
{
    public static class CommonUtil
    {
        public static string GetBase64StringImage(string Path)
        {
            if (!File.Exists(Path)) return string.Empty;
            var bytes = File.ReadAllBytes(Path);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Get value of config and return a String, return null if key not exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConfig(string key)
        {
            string res = WebConfigurationManager.AppSettings[key];
            return res;
        }

        /// <summary>
        /// Get value of config and return value with type T, return defaultValue if key not exists
        /// </summary>
        /// <typeparam name="T">Type of result</typeparam>
        /// <param name="key">Key of config</param>
        /// <param name="defaultValue">Default value if key not exists</param>
        /// <returns></returns>
        public static T GetConfig<T>(string key, T defaultValue)
        {
            T res = defaultValue;
            try
            {
                object configOriginValue = WebConfigurationManager.AppSettings[key];
                try
                {
                    res = (T)Convert.ChangeType(configOriginValue, typeof(T));
                }
                catch
                {
                    try
                    {
                        res = (T)configOriginValue;
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            return res;
        }

        public static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}
