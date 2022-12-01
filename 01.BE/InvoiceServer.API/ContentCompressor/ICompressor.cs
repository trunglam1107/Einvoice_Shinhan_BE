using System.IO;
using System.Threading.Tasks;

namespace InvoiceServer.API.ContentCompressor
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ICompressor'
    public interface ICompressor
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ICompressor'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ICompressor.EncodingType'
        string EncodingType { get; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ICompressor.EncodingType'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ICompressor.Compress(Stream, Stream)'
        Task Compress(Stream source, Stream destination);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ICompressor.Compress(Stream, Stream)'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ICompressor.Decompress(Stream, Stream)'
        Task Decompress(Stream source, Stream destination);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ICompressor.Decompress(Stream, Stream)'
    }
}
