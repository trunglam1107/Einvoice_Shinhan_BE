using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IQueueCreateFileInvoiceRepository : IRepository<QUEUECREATEFILEINVOICE>
    {
        IEnumerable<QUEUECREATEFILEINVOICE> FilterByStatus(List<long> status);

        IEnumerable<QUEUECREATEFILEINVOICE> FilterByInvoicesId(List<long> invoicesId);
    }
}

