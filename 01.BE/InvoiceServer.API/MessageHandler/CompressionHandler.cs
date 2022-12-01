using InvoiceServer.API.ContentCompressor;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace InvoiceServer.API.MessageHandler
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompressionHandler'
    public class CompressionHandler : DelegatingHandler
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompressionHandler'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompressionHandler.Compressors'
        public Collection<ICompressor> Compressors { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompressionHandler.Compressors'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompressionHandler.CompressionHandler()'
        public CompressionHandler()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompressionHandler.CompressionHandler()'
        {
            Compressors = new Collection<ICompressor>();
            Compressors.Add(new DeflateCompressor());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompressionHandler.SendAsync(HttpRequestMessage, CancellationToken)'
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompressionHandler.SendAsync(HttpRequestMessage, CancellationToken)'
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.Content == null
                || request.Headers.AcceptEncoding.IsNullOrEmpty())
            {
                return response;
            }

            var encoding = request.Headers.AcceptEncoding.First();

            var compressor = Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding.Value, StringComparison.InvariantCultureIgnoreCase));

            if (compressor != null)
            {
                response.Content = new CompressedContent(response.Content, compressor);
            }

            return response;
        }
    }
}