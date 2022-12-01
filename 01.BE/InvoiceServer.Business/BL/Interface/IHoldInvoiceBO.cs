using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IHoldInvoiceBO
    {
        IEnumerable<HoldInvoiceViewModel> Filter(ConditionSearchHoldInvoice condition, int skip = 0, int take = int.MaxValue);

        long Count(ConditionSearchHoldInvoice condition);

        ResultCode Create(HoldInvoiceViewModel holdInvoiceViewModel);

        HoldInvoiceViewModel GetById(long id);

        ResultCode Update(HoldInvoiceViewModel holdInvoiceViewModel);

        ResultCode Delete(long id);

    }
}


