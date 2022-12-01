namespace InvoiceServer.Business.Models
{
    public class ResultRelease
    {
        public long ReleaseId { get; private set; }
        public long ReleaseDeatailId { get; private set; }
        public string ErrorMessage { get; set; }
        public bool IsSuccessful { get; set; }

        public ResultRelease()
        {
        }

        public ResultRelease(long releaseId, long releaseDetailId)
        {
            this.ReleaseDeatailId = releaseDetailId;
            this.ReleaseId = releaseId;
        }
    }
}
