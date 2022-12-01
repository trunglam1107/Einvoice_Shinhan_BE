using System;

namespace InvoiceServer.Data.Utils
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DataConvertAttribute : Attribute
    {
        public string Source { get; set; }
        public object DefaultValue { get; set; }
        public bool ThrowExceptionIfSourceNotExist { get; set; }

        public DataConvertAttribute(string source,
                                      object defaultValue = null,
                                      bool throwExceptionIfSourceNotExist = true
                                    )
        {
            this.Source = source;
            this.DefaultValue = defaultValue;
            this.ThrowExceptionIfSourceNotExist = throwExceptionIfSourceNotExist;
        }
    }
}
