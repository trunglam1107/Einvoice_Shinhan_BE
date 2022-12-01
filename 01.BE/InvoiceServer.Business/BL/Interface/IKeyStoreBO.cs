using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IKeyStoreBO
    {

        /// <summary>
        /// Search DataPermission by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<KeyStoreOfCompany> GetList(long companyId);

        KeyStoreOfCompany GetKeyStoreUse(long companyId);

        ResultCode Create(KeyStoreOfCompany keyStoreInfo);

        ResultCode Update(KeyStoreOfCompany keyStoreInfo);
    }
}
