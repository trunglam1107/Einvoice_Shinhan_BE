using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IReleaseInvoiceDetailBO
    {
        Result Create(ReleaseInvoiceMaster info);

        ResultCode Update(long releaseId, StatusRelease status);

        IEnumerable<InvoicesRelease> FilterReleaseInvoice(long releaseId, long companyId, List<int> releaseStatus);
    }
}