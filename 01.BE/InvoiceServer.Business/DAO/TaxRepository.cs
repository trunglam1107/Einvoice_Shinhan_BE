using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class TaxRepository : GenericRepository<TAX>, ITaxRepository
    {
        public TaxRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<TAX> GetList()
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false));
        }

        public TAX GetTax(decimal value)
        {
            return this.dbSet.FirstOrDefault(p => !(p.DELETED ?? false) && p.TAX1 == value);
        }

        public TAX GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => !(p.DELETED ?? false) && p.ID == id);
        }

        public TAX GetTax(string name)
        {
            return this.dbSet.FirstOrDefault(p => !(p.DELETED ?? false) && p.NAME.Equals(name));
        }

        public TAX GetTaxBYCODE(string code)
        {
            return this.dbSet.FirstOrDefault(p => !(p.DELETED ?? false) && p.CODE.Equals(code));
        }

    }
}
