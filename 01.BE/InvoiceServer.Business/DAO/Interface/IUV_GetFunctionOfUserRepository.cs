using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IUVGetFunctionOfUserRepository : IRepository<UV_GETFUNCTIONOFUSER>
    {
        IEnumerable<UV_GETFUNCTIONOFUSER> GetByUserSID(long UserSID, long? companySID);
    }
}
