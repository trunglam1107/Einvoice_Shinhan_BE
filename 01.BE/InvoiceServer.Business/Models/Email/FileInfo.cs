namespace InvoiceServer.Business.Models
{
    public class File_Info
    {
        public string FileName { get; set; }

        public string FullPathFileattached { get; set; }

        public File_Info(string fileName, string fullPathFileattached)
        {
            this.FileName = fileName;
            this.FullPathFileattached = fullPathFileattached;
        }
    }
}
