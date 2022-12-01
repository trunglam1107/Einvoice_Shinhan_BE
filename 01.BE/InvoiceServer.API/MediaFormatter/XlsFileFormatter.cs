using InvoiceServer.Business.Models;
using System.Net.Http.Headers;

namespace InvoiceServer.API.MediaFormatter
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'XlsFileFormatter'
    public class XlsFileFormatter : FileFormatter<InvoiceFile>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'XlsFileFormatter'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'XlsFileFormatter.XlsFileFormatter()'
        public XlsFileFormatter()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'XlsFileFormatter.XlsFileFormatter()'
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/ms-excel"));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'XlsFileFormatter.GetFilePath(InvoiceFile)'
        protected override string GetFilePath(InvoiceFile t)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'XlsFileFormatter.GetFilePath(InvoiceFile)'
        {
            return t != null ? t.FilePath : string.Empty;
        }
    }
}