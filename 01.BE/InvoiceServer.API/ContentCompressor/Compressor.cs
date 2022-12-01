using System.IO;
using System.Threading.Tasks;

namespace InvoiceServer.API.ContentCompressor
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Compressor'
    public abstract class Compressor : ICompressor
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Compressor'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Compressor.EncodingType'
        public abstract string EncodingType { get; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Compressor.EncodingType'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Compressor.CreateCompressionStream(Stream)'
        public abstract Stream CreateCompressionStream(Stream output);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Compressor.CreateCompressionStream(Stream)'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Compressor.CreateDecompressionStream(Stream)'
        public abstract Stream CreateDecompressionStream(Stream input);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Compressor.CreateDecompressionStream(Stream)'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Compressor.Compress(Stream, Stream)'
        public virtual Task Compress(Stream source, Stream destination)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Compressor.Compress(Stream, Stream)'
        {
            var compressed = CreateCompressionStream(destination);

            return Pump(source, compressed)
                .ContinueWith(task => compressed.Dispose());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Compressor.Decompress(Stream, Stream)'
        public virtual Task Decompress(Stream source, Stream destination)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Compressor.Decompress(Stream, Stream)'
        {
            var decompressed = CreateDecompressionStream(source);

            return Pump(decompressed, destination)
                .ContinueWith(task => decompressed.Dispose());
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Compressor.Pump(Stream, Stream)'
        protected virtual Task Pump(Stream input, Stream output)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Compressor.Pump(Stream, Stream)'
        {
            return input.CopyToAsync(output);
        }
    }
}