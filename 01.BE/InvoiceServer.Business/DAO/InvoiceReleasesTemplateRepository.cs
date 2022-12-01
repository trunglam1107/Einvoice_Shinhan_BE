using InvoiceServer.Data.DBAccessor;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceReleasesTemplateRepository : GenericRepository<INVOICERELEASESTEMPLATE>, IInvoiceReleasesTemplateRepository
    {
        public InvoiceReleasesTemplateRepository(IDbContext context)
            : base(context)
        {
        }

        public INVOICERELEASESTEMPLATE GetTemplateReleaseInvoice()
        {
            return this.dbSet.FirstOrDefault();
        }
    }
}
