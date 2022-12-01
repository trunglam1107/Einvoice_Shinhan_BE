using InvoiceServer.Business.Models;

namespace InvoiceServer.Business.BL
{
    public interface IInvoiceReleasesDetailBO
    {

        /// <summary>
        /// Get the client info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        InvoiceReleasesDetailInfo GetInvoiceReleasesDetailInfo(long id);

    }
}
