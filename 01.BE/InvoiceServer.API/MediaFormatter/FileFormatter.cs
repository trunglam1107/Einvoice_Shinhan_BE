using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvoiceServer.API.MediaFormatter
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileFormatter<T>'
    public abstract class FileFormatter<T> : BaseFormatter<T>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileFormatter<T>'
        where T : class
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileFormatter<T>.GetFilePath(T)'
        protected abstract string GetFilePath(T t);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileFormatter<T>.GetFilePath(T)'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileFormatter<T>.WriteToStreamAsync(Type, object, Stream, HttpContent, TransportContext)'
        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileFormatter<T>.WriteToStreamAsync(Type, object, Stream, HttpContent, TransportContext)'
                                               TransportContext transportContext)
        {
            var file = value as T;
            if (file == null || !File.Exists(GetFilePath(file)))
            {
                await base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
                return;
            }

            using (var stream = new FileStream(GetFilePath(file), FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(writeStream);
            }
        }
    }
}