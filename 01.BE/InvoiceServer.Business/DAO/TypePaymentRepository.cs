using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class TypePaymentRepository : GenericRepository<TYPEPAYMENT>, ITypePaymentRepository
    {
        public TypePaymentRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<TYPEPAYMENT> GetList()
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false));
        }

        public TYPEPAYMENT GetPaymentByCode(string code)
        {
            return this.dbSet.FirstOrDefault(p => !(p.DELETED ?? false) && p.CODE.Equals(code));
        }

        public TYPEPAYMENT GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => !(p.DELETED ?? false) && p.ID == id);
        }
    }
}
