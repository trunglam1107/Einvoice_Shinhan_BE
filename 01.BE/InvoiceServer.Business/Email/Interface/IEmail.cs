namespace InvoiceServer.Business.Email
{
    public interface IEmail
    {
        bool Send();
        bool SendByMonth();
        bool Test();
    }
}
