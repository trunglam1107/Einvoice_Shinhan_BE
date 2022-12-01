using InvoiceServer.Business.Models;
using System.Net.Http.Headers;

namespace InvoiceServer.API.MediaFormatter
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'PdfFileFormatter'
    public class PdfFileFormatter : FileFormatter<InvoiceFile>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'PdfFileFormatter'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'PdfFileFormatter.PdfFileFormatter()'
        public PdfFileFormatter()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'PdfFileFormatter.PdfFileFormatter()'
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/pdf"));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'PdfFileFormatter.GetFilePath(InvoiceFile)'
        protected override string GetFilePath(InvoiceFile t)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'PdfFileFormatter.GetFilePath(InvoiceFile)'
        {
            return t != null ? t.FilePath : string.Empty;
        }
    }
}