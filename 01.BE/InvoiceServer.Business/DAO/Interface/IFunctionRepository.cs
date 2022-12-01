using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public interface IFunctionRepository : IRepository<FUNCTION>
    {
        FUNCTION GetById(int id);
    }
}
