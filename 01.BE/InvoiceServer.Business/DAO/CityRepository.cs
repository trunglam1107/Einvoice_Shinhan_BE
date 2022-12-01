using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class CityRepository : GenericRepository<CITY>, ICityRepository
    {
        public CityRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<CITY> GetList()
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false));
        }

        public CITY GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => !(p.DELETED ?? false) && p.ID == id);
        }
        public CITY GetByName(string Name)
        {
            return this.dbSet.FirstOrDefault(p => !(p.DELETED ?? false) && p.NAME.ToUpper() == Name.ToUpper());
        }
    }
}
