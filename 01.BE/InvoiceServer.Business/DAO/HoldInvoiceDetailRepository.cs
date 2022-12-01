using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public class HoldInvoiceDetailRepository : GenericRepository<HOLDINVOICEDETAIL>, IHoldInvoiceDetailRepository
    {
        public HoldInvoiceDetailRepository(IDbContext context)
            : base(context)
        {
        }

    }
}
