using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public interface ILastKeyRepository : IRepository<LASTKEY>
    {
        int GetLastKey(long companyId, string keyString);
        void InsertLastKey(long companyId, string keyString, int lastKey);
        void UpdateLastKey(long companyId, string keyString, int lastKey);

    }
}
