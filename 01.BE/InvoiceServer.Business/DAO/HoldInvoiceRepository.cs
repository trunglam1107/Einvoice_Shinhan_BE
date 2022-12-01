using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class HoldInvoiceRepository : GenericRepository<HOLDINVOICE>, IHoldInvoiceRepository
    {
        public HoldInvoiceRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<HOLDINVOICE> Filter(ConditionSearchHoldInvoice condition, int skip = 0, int take = Int32.MaxValue)
        {
            var res = this.dbSet.Where(h => h.COMPANYID == condition.CompanyId);
            if (condition.RegisterTemplateId.HasValue)
            {
                res = res.Where(h => h.REGISTERTEMPLATEID == condition.RegisterTemplateId);
            }
            if (condition.DateInvoice.HasValue)
            {
                res = res.Where(h => h.INVOICEDATE == condition.DateInvoice);
            }
            if (!string.IsNullOrEmpty(condition.Symbol))
            {
                res = res.Where(h => h.SYMBOL == condition.Symbol);
            }

            return res;
        }

        public HOLDINVOICE GetById(long id, long companySID)
        {
            return dbSet.FirstOrDefault(x => x.COMPANYID == companySID && x.ID == id);
        }
    }
}
