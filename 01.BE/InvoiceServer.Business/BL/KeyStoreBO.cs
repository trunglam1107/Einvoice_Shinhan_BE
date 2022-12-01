using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class KeyStoreBO : IKeyStoreBO
    {
        #region Fields, Properties

        private readonly IKeyStoreRepository keyStoreRepository;
        #endregion

        #region Contructor

        public KeyStoreBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.keyStoreRepository = repoFactory.GetRepository<IKeyStoreRepository>();
        }

        #endregion
        public IEnumerable<KeyStoreOfCompany> GetList(long companyId)
        {
            var keyStoreOfCompany = this.keyStoreRepository.GetList(companyId).OrderByDescending(k => k.ID);
            return keyStoreOfCompany.Select(p => new KeyStoreOfCompany(p));
        }

        #region Methods

        #endregion


        public KeyStoreOfCompany GetKeyStoreUse(long companyId)
        {
            KEYSTORE keyStore = this.keyStoreRepository.GetKeyStoreUse(companyId);
            if (keyStore == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This KeyStore [{0}] not found in data of client", companyId));
            }

            return new KeyStoreOfCompany(keyStore);
        }

        public ResultCode Create(KeyStoreOfCompany keyStoreInfo)
        {
            if (keyStoreInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            KEYSTORE keyStore = new KEYSTORE();
            keyStore.CopyData(keyStoreInfo);
            keyStore.CREATEDDATE = DateTime.Now;
            bool result = this.keyStoreRepository.Insert(keyStore);
            return result ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public ResultCode Update(KeyStoreOfCompany keyStoreInfo)
        {
            throw new NotImplementedException();
        }
    }
}