using InvoiceServer.Common.Constants;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class AgenciesBO : IAgenciesBO
    {
        #region Fields, Properties

        private readonly IRepositoryFactory repoFactory;
        private readonly IContractRepository contractRepository;
        private readonly IMyCompanyRepository myCompanyRepository;
        private const int MaxLevelAgencies = 3;
        #endregion

        #region Contructor

        public AgenciesBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.NotNull(repoFactory, "repoFactory");

            this.repoFactory = repoFactory;
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.contractRepository = repoFactory.GetRepository<IContractRepository>();
        }

        #endregion

        #region Methods
      
        public IEnumerable<MYCOMPANY> GetList()
        {
            return this.myCompanyRepository.GetList();
        }

        #endregion

        public IEnumerable<AgenciesInfo> FilterAgencies(ConditionSearchAgencies condition, int skip = int.MinValue, int take = int.MaxValue)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var sellers = this.myCompanyRepository.FilterSeller(condition).AsQueryable()
                .OrderBy(condition.Order_By, condition.Order_Type.Equals(OrderType.Desc)).Skip(skip).Take(take).ToList();

            return sellers.Select(p => new AgenciesInfo(p));
        }

        public int CountFillterAgencies(ConditionSearchAgencies condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.myCompanyRepository.FilterSeller(condition).Count();
        }

        public ResultCode Create(AgenciesInfo sellerInfo)
        {
            return CreateCompany(sellerInfo);
        }

        public ResultCode Update(int companyId, AgenciesInfo sellerInfo)
        {

            CompanyUpdate(companyId, sellerInfo);
            return ResultCode.NoError;
        }

        public ResultCode Delete(int id, int companyId, string userLevel)
        {
            bool customerHasContract = CustomerHasContractActive(id, companyId);
            if (customerHasContract)
            {
                throw new BusinessLogicException(ResultCode.AgenciesHasContract, "Cannot delete record because this Agency has contract approved");
            }

            MYCOMPANY currentCompany = GetCompany(id);
            currentCompany.DELETED = true;
            currentCompany.ACTIVE = false;
            return myCompanyRepository.Update(currentCompany) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public AgenciesInfo GetAgenciesInfo(int companyId, string userLevel)
        {
            var companyInfo = GetCompany(companyId);
            var agenciesInfo = new AgenciesInfo(companyInfo);
            return agenciesInfo;
        }

        private ResultCode CreateCompany(AgenciesInfo agenciesInfo)
        {
            if (agenciesInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!agenciesInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            bool isExitTaxCode = this.myCompanyRepository.ContainTaxCode(agenciesInfo.CompanyId, agenciesInfo.TaxCode, CustomerLevel.Sellers);
            if (isExitTaxCode)
            {
                throw new BusinessLogicException(ResultCode.CompanyNameIsExitsTaxCode,
                                  string.Format("Create Company failed because company with TaxCode = [{0}] is exist.", agenciesInfo.TaxCode));
            }

            bool isExitEmail = this.myCompanyRepository.ContainEmail(agenciesInfo.CompanyId, agenciesInfo.Email, CustomerLevel.Sellers);
            if (isExitEmail)
            {
                throw new BusinessLogicException(ResultCode.CompanyNameIsExitsEmail,
                                  string.Format("Create Company failed because company with Email = [{0}] is exist.", agenciesInfo.Email));
            }

            if (agenciesInfo.Level_Agencies > MaxLevelAgencies)
            {
                throw new BusinessLogicException(ResultCode.CompanyNameCannotCreated, "Create Company failed because this company is level three");
            }

            var myCompany = new MYCOMPANY();
            myCompany.CopyData(agenciesInfo);
            myCompany.LEVELCUSTOMER = CustomerLevel.Sellers;
             
            if (agenciesInfo.SessionInfo.Company.Id.HasValue)
            {
                myCompany.COMPANYID = agenciesInfo.SessionInfo.Company.Id;
            }

            return this.myCompanyRepository.Insert(myCompany) ? ResultCode.NoError : ResultCode.UnknownError;

        }

        private ResultCode CompanyUpdate(int id, AgenciesInfo agenciesInfo)
        {
            if (agenciesInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!agenciesInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            MYCOMPANY currentCompny = GetCompany(id);
            if (!currentCompny.TAXCODE.IsEquals(agenciesInfo.TaxCode)
                && this.myCompanyRepository.ContainTaxCode(agenciesInfo.CompanyId, agenciesInfo.TaxCode, CustomerLevel.Customer))
            {
                throw new BusinessLogicException(ResultCode.CompanyNameIsExitsTaxCode,
                                    string.Format("Create Company failed because company with TaxCode = [{0}] is exist.", agenciesInfo.TaxCode));
            }

            if (agenciesInfo.Email.IsNotNullOrEmpty() && !currentCompny.EMAIL.IsEquals(agenciesInfo.Email)        // Email is changed
               && this.myCompanyRepository.ContainEmail(agenciesInfo.CompanyId, agenciesInfo.Email, CustomerLevel.Sellers)) // New Email is existed
            {
                throw new BusinessLogicException(ResultCode.CompanyNameIsExitsEmail,
                                  string.Format("Create Company failed because company with Email = [{0}] is exist.", agenciesInfo.Email));
            }

            currentCompny.CopyData(agenciesInfo);
            return this.myCompanyRepository.Update(currentCompny) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        private MYCOMPANY GetCompany(int id)
        {
            var company = this.myCompanyRepository.GetById(id);
            if (company == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.DataNotFound);
            }

            return company;
        }

        private bool CustomerHasContractActive(int customerId, int companyId)
        {
            return this.contractRepository.CustomerHasContract(companyId, customerId);
        }
    }
}