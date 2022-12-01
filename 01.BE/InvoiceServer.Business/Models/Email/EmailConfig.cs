namespace InvoiceServer.Business.Models
{
    public class EmailConfig
    {
        public string FolderEmailTemplate { get; set; }
        public string FolderExportTemplate { get; set; }
        public bool EnableSsl { get; set; }

        public int TypeEmail { get; set; }
    }
}
