
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IInvoiceStatisticalRepository : IRepository<STATISTICAL>
    {
        IEnumerable<STATISTICAL> FilterInvoiceStatistical(long invoiceId);
        List<STATISTICAL> GetSTATISTICALs(List<long> invoiceIds);
    }
}
