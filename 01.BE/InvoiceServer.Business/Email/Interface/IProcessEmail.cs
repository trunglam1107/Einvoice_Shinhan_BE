namespace InvoiceServer.Business.Email
{
    public interface IProcessEmail
    {
        bool SendEmail(IEmail email);
    }
}
