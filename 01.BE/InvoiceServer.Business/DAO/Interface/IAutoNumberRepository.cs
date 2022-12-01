using InvoiceServer.Data.DBAccessor;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public interface IAutoNumberRespository : IRepository<AUTONUMBERSETTING>
    {
        IQueryable<AUTONUMBERSETTING> GetList();
        AUTONUMBERSETTING GetById(string id);
        bool Update(string code, AUTONUMBERSETTING autoNumber);
        bool checkActive(string id);
        int Length(string id);
    }
}
