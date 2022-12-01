using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public class DeletingInvoiceRepository : GenericRepository<DELETINGINVOICE>, IDeletingInvoiceRepository
    {
        public DeletingInvoiceRepository(IDbContext context)
            : base(context)
        {
        }
    }
}
