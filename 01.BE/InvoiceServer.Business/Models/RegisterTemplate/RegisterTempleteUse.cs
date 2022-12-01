namespace InvoiceServer.Business.Models
{
    public class RegisterTempleteUse
    {
        public long Id { get; set; }
        public string Code { get; set; }

        public RegisterTempleteUse(long id, string code)
        {
            this.Id = id;
            this.Code = code;
        }

    }
}
