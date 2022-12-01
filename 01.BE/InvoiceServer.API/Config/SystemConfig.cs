using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace InvoiceServer.API.Config
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemConfig'
    public class SystemConfig
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemConfig'
    {
        #region Web.app config keys

        /// <summary>
        /// Enable compressing response content
        /// </summary>
        [Config("Enable CacheResponse", "true", false)]
        public bool EnableCacheResponse { get; private set; }

        /// <summary>
        /// Cache timeout in minutes (default 1 day)
        /// </summary>
        [Config("CacheResponse Timeout", "120", false)]
        public int CacheResponseTimeout { get; private set; }

        /// <summary>
        /// Enable compressing response content
        /// </summary>
        [Config("Enable CompressResponse", "false", false)]
        public bool EnableCompressResponse { get; private set; }

        /// <summary>
        /// Enable Cross-origin resource sharing
        /// </summary>
        [Config("Enable CORS")]
        public bool EnableCORS { get; private set; }

        #endregion Web.app config keys

        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private static readonly SystemConfig instance = new SystemConfig();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemConfig.Instance'
        public static SystemConfig Instance
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemConfig.Instance'
        {
            get
            {
                return instance;
            }
        }

        #endregion Fields, Properties

        #region Methods

        /// <summary>
        /// Prevent new object of this class from outside
        /// </summary>
        private SystemConfig()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                // Get objects's properties from memory cache
                var configProperties = typeof(SystemConfig).GetProperties();

                foreach (var property in configProperties)
                {
                    var attr = property.GetCustomAttribute<ConfigAttribute>();
                    if (attr == null)
                    {
                        continue;
                    }

                    var configKey = attr.Key;
                    var configValueStr = attr.DefaultValue;

                    if (ConfigurationManager.AppSettings.AllKeys.Contains(configKey))
                    {
                        configValueStr = ConfigurationManager.AppSettings[configKey];
                    }

                    var configValue = configValueStr.ConvertDataToType(property.PropertyType);
                    property.SetValue(this, configValue);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Get configuration in Web.config failed.");
                throw;
            }
        }

        #endregion Methods
    }
}