namespace InvoiceServer.Business.Models
{
    public class RegisterTempleteUseReport
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public RegisterTempleteUseReport(long id, string code)
        {
            this.Id = id;
            this.Code = code;
        }

    }
}
