using InvoiceServer.Business.Models;
using System.Net.Http.Headers;

namespace InvoiceServer.API.MediaFormatter
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DocxFileFormatter'
    public class DocxFileFormatter : FileFormatter<InvoiceFile>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DocxFileFormatter'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DocxFileFormatter.DocxFileFormatter()'
        public DocxFileFormatter()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DocxFileFormatter.DocxFileFormatter()'
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.ms-word"));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DocxFileFormatter.GetFilePath(InvoiceFile)'
        protected override string GetFilePath(InvoiceFile t)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DocxFileFormatter.GetFilePath(InvoiceFile)'
        {
            return t != null ? t.FilePath : string.Empty;
        }
    }
}