namespace InvoiceServer.Business.Models
{
    public class ExportFileInfo
    {
        public string FullPathFileName { get; set; }
        public string FileName { get; set; }

        public string FullPathFileInvoice { get; set; }
        public ExportFileInfo()
        {
        }
        public ExportFileInfo(string fullPathFileName) : this()
        {
            this.FullPathFileName = fullPathFileName;
        }
    }
}
