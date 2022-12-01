using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public class MyCompanyUpgradeBO : IMyCompanyUpgradeBO
    {
        #region Fields, Properties
        private readonly IMyCompanyUpgradeRepository myCompanyUpgradeRepository;
        private readonly IDbTransactionManager transaction;
        #endregion

        #region Contructor

        public MyCompanyUpgradeBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.myCompanyUpgradeRepository = repoFactory.GetRepository<IMyCompanyUpgradeRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
        }

        #endregion

        #region Methods
        public CompanyUpgradeInfo getByID(long id)
        {
            var company = this.myCompanyUpgradeRepository.GetByCompanySId(id);
            if (company == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.DataNotFound);
            }
            var companyInfo = new CompanyUpgradeInfo(company);
            return companyInfo;
        }
        public IEnumerable<MYCOMPANYUPGRADE> GetList()
        {
            return this.myCompanyUpgradeRepository.GetList();
        }
        #endregion

        public ResultCode Create(CompanyUpgradeInfo companyInfo, UserSessionInfo currentUser)
        {
            try
            {
                transaction.BeginTransaction();
                if (companyInfo.Id.HasValue)
                {
                    if (companyInfo.Type.HasValue)
                    {
                        UpdateCompanyUpgrade(companyInfo, currentUser);
                    }
                    else
                    {
                        DeleteCompanyUpgrade(companyInfo);
                    }
                }
                else
                {
                    CreateCompanyUpgrade(companyInfo, currentUser);
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return ResultCode.NoError;
        }

        #region Private Method
        private void CreateCompanyUpgrade(CompanyUpgradeInfo companyInfo, UserSessionInfo currentUser)
        {
            if (companyInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var myCompany = new MYCOMPANYUPGRADE();
            myCompany.CopyData(companyInfo);
            myCompany.CREATEDDATE = DateTime.Now;
            myCompany.UPDATEDBY = currentUser.Id;
            this.myCompanyUpgradeRepository.Insert(myCompany);
        }

        private void UpdateCompanyUpgrade(CompanyUpgradeInfo companyInfo, UserSessionInfo currentUser)
        {
            if (companyInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            MYCOMPANYUPGRADE myCompany = this.myCompanyUpgradeRepository.GetByCompanySId(companyInfo.CompanySid);
            myCompany.CopyData(companyInfo);
            myCompany.UPDATEDDATE = DateTime.Now;
            myCompany.UPDATEDBY = currentUser.Id;
            this.myCompanyUpgradeRepository.Update(myCompany);
        }
        private void DeleteCompanyUpgrade(CompanyUpgradeInfo companyInfo)
        {
            if (companyInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            MYCOMPANYUPGRADE myCompany = this.myCompanyUpgradeRepository.GetById((long)companyInfo.Id);
            this.myCompanyUpgradeRepository.Delete(myCompany);
        }
        #endregion
    }
}