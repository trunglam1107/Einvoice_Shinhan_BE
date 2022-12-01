using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceTemplateRepository : GenericRepository<INVOICETEMPLATE>, IInvoiceTemplateRepository
    {
        public InvoiceTemplateRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<INVOICETEMPLATE> GetAll(long companySID)
        {
            return base.GetAll();
        }

        public IEnumerable<INVOICETEMPLATE> GetList()
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false));
        }

        public INVOICETEMPLATE GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => !(p.DELETED ?? false) && p.ID == id);
        }

        public INVOICETEMPLATE GetByIdNoTracking(long id)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false) && p.ID == id).AsNoTracking().FirstOrDefault();
        }

        public long CreateInvoiceTemplate(INVOICETEMPLATE invoiceTemplate)
        {
            this.dbSet.AddOrUpdate(invoiceTemplate);
            context.SaveChanges();
            return invoiceTemplate.ID;
        }

        public INVOICETEMPLATE GetByInvoiceSampleCode(string invoiceSampleCode, long companyId)
        {
            var invoiceTemplate = from template in this.dbSet
                                  join invoiceSample in this.context.Set<INVOICESAMPLE>()
                                        on template.INVOICESAMPLEID equals invoiceSample.ID
                                  where (invoiceSample.CODE == invoiceSampleCode)
                                  select template;
            return invoiceTemplate.FirstOrDefault();
        }

        public INVOICETEMPLATE GetByInvoiceSampleId(long invoiceSampleId, long companyId)
        {
            var invoiceTemplateQuery = this.dbSet.Where(x => x.INVOICESAMPLEID == invoiceSampleId);
            return invoiceTemplateQuery.FirstOrDefault();
        }

        public IEnumerable<INVOICETEMPLATE> Filter(long invoiceSampleId, long companySID)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false) && p.INVOICESAMPLEID == invoiceSampleId);
        }
    }
}
