using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class TaxDepartmentRepository : GenericRepository<TAXDEPARTMENT>, ITaxDepartmentRepository
    {
        public TaxDepartmentRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<TAXDEPARTMENT> GetList()
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false));
        }

        public IEnumerable<TAXDEPARTMENT> FilterTaxDeparment(long cityId)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false) && p.CITYID == cityId);
        }

        public IEnumerable<TAXDEPARTMENT> FilterTaxDeparment(long cityId, string taxDepartmentName)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false) && p.CITYID == cityId && p.NAME.ToUpper() == taxDepartmentName.ToUpper());
        }

        public TAXDEPARTMENT GetById(long id)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false) && p.ID == id).FirstOrDefault();
        }
    }
}
