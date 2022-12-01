using System;
using System.Web;

namespace InvoiceServer.Business.Models
{
    public class FileUploadInfo
    {
        public HttpPostedFile File { get; set; }
        public string FileName { get; set; }
        public int Chunk { get; set; }
        public int TotalChunk { get; set; }

        public static FileUploadInfo FromRequest(HttpRequest request)
        {
            if (request.Files == null || request.Files.Count == 0)
            {
                return null;
            }

            return new FileUploadInfo
            {
                File = request.Files[0],
                FileName = request.Headers["name"],
                Chunk = Convert.ToInt32(request.Headers["chunk"]),
                TotalChunk = Convert.ToInt32(request.Headers["chunks"])
            };
        }
    }
}
