namespace InvoiceServer.Business.Models
{
    public class QueueCreateFile
    {
        public long InvoiceId { get; set; }
        public QueueCreateFile(long? invoiceid)
        {
            this.InvoiceId = (invoiceid ?? 0);
        }

    }
}
