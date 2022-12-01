using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO.Interface
{
    public interface IServicesJobRepository : IRepository<INVOICE>
    {
        IEnumerable<JobApprovedInvoice> ListInvoiceApprove(int? flgCreate = 0);
    }
}

