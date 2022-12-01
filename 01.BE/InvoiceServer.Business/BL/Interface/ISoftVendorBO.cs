using InvoiceServer.Business.Models;

namespace InvoiceServer.Business.BL
{
    public interface ISoftVendorBO
    {

        /// <summary>
        /// Search DataPermission by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        VendorInfo GetSoftVendorInfo();
    }
}
