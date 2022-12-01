namespace InvoiceServer.Business.BL
{
    public interface IBOFactory
    {
        T GetBO<T>(params object[] args) where T : class;
    }
}
