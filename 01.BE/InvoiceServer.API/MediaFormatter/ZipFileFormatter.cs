using InvoiceServer.Business.Models;
using System.Net.Http.Headers;

namespace InvoiceServer.API.MediaFormatter
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ZipFileFormatter'
    public class ZipFileFormatter : FileFormatter<InvoiceFile>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ZipFileFormatter'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ZipFileFormatter.ZipFileFormatter()'
        public ZipFileFormatter()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ZipFileFormatter.ZipFileFormatter()'
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/zip"));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ZipFileFormatter.GetFilePath(InvoiceFile)'
        protected override string GetFilePath(InvoiceFile t)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ZipFileFormatter.GetFilePath(InvoiceFile)'
        {
            return t != null ? t.FilePath : string.Empty;
        }
    }
}