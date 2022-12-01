using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceTemplateSampleRepository : GenericRepository<INVOICETEMPLATESAMPLE>, IInvoiceTemplateSampleRepository
    {
        public InvoiceTemplateSampleRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<INVOICETEMPLATESAMPLE> GetList()
        {
            return this.dbSet;
        }

        public INVOICETEMPLATESAMPLE GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }

        public IEnumerable<INVOICETEMPLATESAMPLE> GetListByInvoiceTypeId(long invoiceTypeId)
        {
            var listInvoiceTemplateSampleActive = this.GetList();
            return listInvoiceTemplateSampleActive.OrderBy(p => p.ID);
        }
    }
}
