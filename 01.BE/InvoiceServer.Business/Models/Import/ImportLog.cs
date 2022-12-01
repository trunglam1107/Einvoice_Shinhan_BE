namespace InvoiceServer.Business.Models
{
    public class ImportLog
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Decription { get; set; }
        public string Date { get; set; }
        public bool IsSuccess { get; set; }
    }
}
