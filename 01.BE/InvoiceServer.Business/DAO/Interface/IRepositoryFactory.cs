namespace InvoiceServer.Business.DAO
{
    public interface IRepositoryFactory
    {
        T GetRepository<T>(params object[] args) where T : class;
        IDbTransactionManager GetTransactionManager();
    }
}
