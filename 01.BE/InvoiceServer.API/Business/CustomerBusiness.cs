using InvoiceServer.Business.BL;
using InvoiceServer.Business.ExternalData;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness'
    public class CustomerBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness'
    {
        #region Fields, Properties

        private readonly ICustomerBO customerBO;
        private readonly IMyCompanyUpgradeBO myCompanyUpgradeBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.CustomerBusiness(IBOFactory)'
        public CustomerBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.CustomerBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderExportTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.ExportTemplateFilePath),
            };
            this.customerBO = boFactory.GetBO<ICustomerBO>(printConfig, emailConfig);
            this.myCompanyUpgradeBO = boFactory.GetBO<IMyCompanyUpgradeBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.FillterCustomer(ConditionSearchCustomer)'
        public IEnumerable<CustomerInfo> FillterCustomer(ConditionSearchCustomer condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.FillterCustomer(ConditionSearchCustomer)'
        {
            condition.TotalRecords = this.customerBO.CountFillterCustomer(condition);
            var customers = this.customerBO.FilterCustomer(condition);
            return customers;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetCustomerInfo(long)'
        public CustomerInfo GetCustomerInfo(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetCustomerInfo(long)'
        {
            return this.customerBO.GetCustomerInfo(companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetCustomerInfoForBB(long)'
        public CustomerInfo GetCustomerInfoForBB(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetCustomerInfoForBB(long)'
        {
            return this.customerBO.GetCustomerInfoForBB(companyId);

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.Create(CustomerInfo)'
        public ResultCode Create(CustomerInfo customerInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.Create(CustomerInfo)'
        {
            if (customerInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            if (!LevelCustomerInfo.LevelCustomer.Contains(customerInfo.LevelCustomer))
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            customerInfo.SessionInfo = this.CurrentUser;
            ResultCode result = this.customerBO.Create(customerInfo);
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.Update(long, CustomerInfo)'
        public ResultCode Update(long companyId, CustomerInfo customerInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.Update(long, CustomerInfo)'
        {
            if (customerInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            customerInfo.SessionInfo = this.CurrentUser;
            ResultCode result = this.customerBO.Update(companyId, customerInfo);
            //add  update upgrade customer
            if (result == ResultCode.NoError)
            {
                customerInfo.MyCompanyUpgrade.CompanySid = customerInfo.Id ?? 0;
                result = this.myCompanyUpgradeBO.Create(customerInfo.MyCompanyUpgrade, this.CurrentUser);
            }
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.Delete(long)'
        {
            string roleOfCurrentUser = this.CurrentUser.RoleUser.Level;
            long companyId = GetCompanyIdOfUser();
            return this.customerBO.Delete(id, companyId, roleOfCurrentUser);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetCustomerByTaxCode(string)'
        public ExternalCustomer GetCustomerByTaxCode(string taxCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetCustomerByTaxCode(string)'
        {
            ClientApiService clientService = new ClientApiService();
            return clientService.GetCustomerInfo(taxCode);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetBranchs()'
        public IEnumerable<Branch> GetBranchs()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetBranchs()'
        {
            return this.customerBO.GetBranchs();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetListBranchs(int)'
        public IEnumerable<BranchLevel> GetListBranchs(int isGetAll)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetListBranchs(int)'
        {
            long companyId = GetCompanyIdOfUser();
            return this.customerBO.GetListBranchs(companyId, isGetAll);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetListBranchs_V2(int)'
        public IEnumerable<BranchLevel> GetListBranchs_V2(int isGetAll)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetListBranchs_V2(int)'
        {
            long companyId = GetCompanyIdOfUser();
            return this.customerBO.GetListBranchs_V2(companyId, isGetAll);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetListBranchs_2(long)'
        public IEnumerable<BranchLevel> GetListBranchs_2(long companyID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetListBranchs_2(long)'
        {
            long companyId = GetCompanyIdOfUser();
            return this.customerBO.GetListBranchs_2(companyID);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetListAllBranchs()'
        public IEnumerable<BranchLevel> GetListAllBranchs()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetListAllBranchs()'
        {
            return this.customerBO.GetListAllBranchs();
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetListBranchsOnly(int)'
        public IEnumerable<BranchLevel> GetListBranchsOnly(int isGetAll)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.GetListBranchsOnly(int)'
        {
            long companyId = GetCompanyIdOfUser();
            return this.customerBO.GetListBranchsOnly(companyId, isGetAll);
        }
        //add
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.ImportData(FileImport)'
        public ResultImportSheet ImportData(FileImport fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.ImportData(FileImport)'
        {
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }

            string fullPathfile = SaveFileImport(fileImport);
            long companyId = GetCompanyIdOfUser();
            return this.customerBO.ImportData(fullPathfile, companyId);
        }

        private static string SaveFileImport(FileImport fileImport)
        {
            string FolderPath = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.UploadFolderImport);
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string fileName = string.Format(@"{0}\{1}.xlsx", FolderPath, DateTime.Now.ToString("ddMMyyyy"));
            try
            {
                File.WriteAllBytes(fileName, Convert.FromBase64String(fileImport.Data));
            }
            catch (Exception ex)
            {

                throw new BusinessLogicException(ResultCode.FileNotFound, ex.Message);
            }

            return fileName;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.DownloadDataCustomer(ConditionSearchCustomer)'
        public ExportFileInfo DownloadDataCustomer(ConditionSearchCustomer condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerBusiness.DownloadDataCustomer(ConditionSearchCustomer)'
        {
            ExportFileInfo fileInfo = this.customerBO.DownloadDataCustomer(condition);
            return fileInfo;
        }
        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }
        #endregion
    }
}