using InvoiceServer.Common;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvoiceServer.API.ContentCompressor
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompressedContent'
    public class CompressedContent : HttpContent
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompressedContent'
    {
        private readonly HttpContent content;
        private readonly ICompressor compressor;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompressedContent.CompressedContent(HttpContent, ICompressor)'
        public CompressedContent(HttpContent content, ICompressor compressor)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompressedContent.CompressedContent(HttpContent, ICompressor)'
        {
            Ensure.Argument.ArgumentNotNull(content, "content");
            Ensure.Argument.ArgumentNotNull(compressor, "compressor");

            this.content = content;
            this.compressor = compressor;

            AddHeaders();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompressedContent.TryComputeLength(out long)'
        protected override bool TryComputeLength(out long length)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompressedContent.TryComputeLength(out long)'
        {
            length = -1;
            return false;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompressedContent.SerializeToStreamAsync(Stream, TransportContext)'
        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompressedContent.SerializeToStreamAsync(Stream, TransportContext)'
        {
            Ensure.Argument.ArgumentNotNull(stream, "stream");

            using (content)
            {
                var contentStream = await content.ReadAsStreamAsync();
                await compressor.Compress(contentStream, stream);
            }
        }

        private void AddHeaders()
        {
            foreach (var header in content.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            Headers.ContentEncoding.Add(compressor.EncodingType);
        }
    }
}