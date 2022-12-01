namespace InvoiceServer.Business.Models
{
    public class ExportOption
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string PageSize { get; set; }

        public string PageOrientation { get; set; }
    }
}
