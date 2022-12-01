using System;
using System.Net.Http.Formatting;

namespace InvoiceServer.API.MediaFormatter
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseFormatter<T>'
    public abstract class BaseFormatter<T> : MediaTypeFormatter
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseFormatter<T>'
        where T : class
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseFormatter<T>.CanReadType(Type)'
        public override bool CanReadType(Type type)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseFormatter<T>.CanReadType(Type)'
        {
            return false;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseFormatter<T>.CanWriteType(Type)'
        public override bool CanWriteType(Type type)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseFormatter<T>.CanWriteType(Type)'
        {
            return type == typeof(T);
        }
    }
}