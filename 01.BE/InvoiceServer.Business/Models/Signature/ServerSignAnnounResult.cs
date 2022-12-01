namespace InvoiceServer.Business.Models
{
    public class ServerSignAnnounResult
    {
        public int? AnnouncementSuccess { get; set; }
        public int? AnnouncementFail { get; set; }
        public string Message { get; set; }
    }
}
