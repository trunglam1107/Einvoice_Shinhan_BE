using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public interface IInvoiceReleasesTemplateRepository : IRepository<INVOICERELEASESTEMPLATE>
    {
        INVOICERELEASESTEMPLATE GetTemplateReleaseInvoice();
    }
}