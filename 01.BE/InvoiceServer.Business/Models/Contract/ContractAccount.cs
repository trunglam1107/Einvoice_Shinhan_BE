namespace InvoiceServer.Business.Models
{
    public class ContractAccount
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public long? CompanyId { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Level_Role { get; set; }

        public long? ClientId { get; set; }

        public int? PassWordStatus { get; set; }

        public ContractAccount()
        {

        }
    }
}
