using InvoiceServer.Data.DBAccessor;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class FunctionRepository : GenericRepository<FUNCTION>, IFunctionRepository
    {
        public FunctionRepository(IDbContext context)
            : base(context)
        {
        }

        public FUNCTION GetById(int id)
        {
            return this.dbSet.Where(r => r.ID == id)
                .FirstOrDefault();
        }
    }
}
