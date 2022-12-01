namespace InvoiceServer.Business.Models
{
    public class FileInformation
    {
        public string FileName { get; set; }

        public string FullPathFileattached { get; set; }

        public FileInformation(string fileName, string fullPathFileattached)
        {
            this.FileName = fileName;
            this.FullPathFileattached = fullPathFileattached;
        }
    }
}
