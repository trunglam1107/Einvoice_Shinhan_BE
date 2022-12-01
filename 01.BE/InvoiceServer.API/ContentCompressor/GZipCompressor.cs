using System.IO;
using System.IO.Compression;

namespace InvoiceServer.API.ContentCompressor
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GZipCompressor'
    public class GZipCompressor : Compressor
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GZipCompressor'
    {
        private const string GZipEncoding = "gzip";

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GZipCompressor.EncodingType'
        public override string EncodingType
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GZipCompressor.EncodingType'
        {
            get { return GZipEncoding; }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GZipCompressor.CreateCompressionStream(Stream)'
        public override Stream CreateCompressionStream(Stream output)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GZipCompressor.CreateCompressionStream(Stream)'
        {
            return new GZipStream(output, CompressionMode.Compress, leaveOpen: true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GZipCompressor.CreateDecompressionStream(Stream)'
        public override Stream CreateDecompressionStream(Stream input)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GZipCompressor.CreateDecompressionStream(Stream)'
        {
            return new GZipStream(input, CompressionMode.Decompress, leaveOpen: true);
        }
    }
}