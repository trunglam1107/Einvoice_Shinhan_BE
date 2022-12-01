using System;

namespace InvoiceServer.Common
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ConfigAttribute : Attribute
    {
        public string Key { get; private set; }
        public string DefaultValue { get; private set; }
        public bool ThrowExceptionIfSourceNotExist { get; private set; }

        public ConfigAttribute(string key)
        {
            this.Key = key;
            this.DefaultValue = string.Empty;
            this.ThrowExceptionIfSourceNotExist = true;
        }

        public ConfigAttribute(string key, string defaultValue, bool throwExceptionIfSourceNotExist)
        {
            this.Key = key;
            this.DefaultValue = defaultValue;
            this.ThrowExceptionIfSourceNotExist = throwExceptionIfSourceNotExist;
        }
    }
}