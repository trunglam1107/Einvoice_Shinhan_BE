namespace InvoiceServer.Business.BL
{
    public abstract class BaseBO
    {
        private const string AuthorizationToken = "X-Authorization-Token";

        private string _currentUserId = string.Empty;
    }
}
