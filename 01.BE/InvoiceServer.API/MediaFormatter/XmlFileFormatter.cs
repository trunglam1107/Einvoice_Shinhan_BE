using InvoiceServer.Business.Models;
using System.Net.Http.Headers;

namespace InvoiceServer.API.MediaFormatter
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'XmlFileFormatter'
    public class XmlFileFormatter : FileFormatter<InvoiceFile>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'XmlFileFormatter'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'XmlFileFormatter.XmlFileFormatter()'
        public XmlFileFormatter()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'XmlFileFormatter.XmlFileFormatter()'
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'XmlFileFormatter.GetFilePath(InvoiceFile)'
        protected override string GetFilePath(InvoiceFile t)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'XmlFileFormatter.GetFilePath(InvoiceFile)'
        {
            return t != null ? t.FilePath : string.Empty;
        }
    }
}