using System.IO;
using System.IO.Compression;

namespace InvoiceServer.API.ContentCompressor
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeflateCompressor'
    public class DeflateCompressor : Compressor
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeflateCompressor'
    {
        private const string DeflateEncoding = "deflate";

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeflateCompressor.EncodingType'
        public override string EncodingType
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeflateCompressor.EncodingType'
        {
            get { return DeflateEncoding; }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeflateCompressor.CreateCompressionStream(Stream)'
        public override Stream CreateCompressionStream(Stream output)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeflateCompressor.CreateCompressionStream(Stream)'
        {
            return new DeflateStream(output, CompressionMode.Compress, leaveOpen: true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeflateCompressor.CreateDecompressionStream(Stream)'
        public override Stream CreateDecompressionStream(Stream input)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeflateCompressor.CreateDecompressionStream(Stream)'
        {
            return new DeflateStream(input, CompressionMode.Decompress, leaveOpen: true);
        }
    }
}