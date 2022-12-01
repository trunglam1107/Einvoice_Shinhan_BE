using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public interface IRoleFunctionRepository : IRepository<ROLEFUNCTION>
    {
        ROLEFUNCTION GetById(long id);

        ROLEFUNCTION GetRoleOfUser(long roleFunctionId, long userSID);

    }
}
