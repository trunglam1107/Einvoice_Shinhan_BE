using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreBusiness'
    public class KeyStoreBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreBusiness'
    {
        #region Fields, Properties

        private readonly IKeyStoreBO keyStoreBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreBusiness.KeyStoreBusiness(IBOFactory)'
        public KeyStoreBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreBusiness.KeyStoreBusiness(IBOFactory)'
        {
            this.keyStoreBO = boFactory.GetBO<IKeyStoreBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreBusiness.GetList(long)'
        public IEnumerable<KeyStoreOfCompany> GetList(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreBusiness.GetList(long)'
        {
            var keyStores = this.keyStoreBO.GetList(companyId);
            return keyStores;
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreBusiness.Create(KeyStoreOfCompany)'
        public ResultCode Create(KeyStoreOfCompany keyStoreInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreBusiness.Create(KeyStoreOfCompany)'
        {
            if (keyStoreInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            keyStoreInfo.CompanyId = this.CurrentUser.Company.Id.Value;
            ResultCode result = this.keyStoreBO.Create(keyStoreInfo);
            return result;
        }

        #endregion
    }
}