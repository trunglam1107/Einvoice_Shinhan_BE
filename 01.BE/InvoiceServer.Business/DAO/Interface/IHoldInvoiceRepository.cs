using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IHoldInvoiceRepository : IRepository<HOLDINVOICE>
    {
        IEnumerable<HOLDINVOICE> Filter(ConditionSearchHoldInvoice condition, int skip = 0, int take = int.MaxValue);

        HOLDINVOICE GetById(long id, long companySID);
    }
}
