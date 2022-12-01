using InvoiceServer.Business.BL;
using InvoiceServer.Business.Constants;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace InvoiceServer.API.Business
{
    public class InvoiceCompanyUseBusiness : BaseBusiness
    {
        #region Fields, Properties
        private static readonly string TempalateInvoice = "Template";
        private IInvoiceCompanyUseBO invoiceCompanyUseReleaseBO;

        #endregion Fields, Properties

        #region Contructor

        public InvoiceCompanyUseBusiness(IBOFactory boFactory)
        {
            var printConfig = GetPrintConfig();
            this.invoiceCompanyUseReleaseBO = boFactory.GetBO<IInvoiceCompanyUseBO>(printConfig);
        }

        #endregion Contructor

        #region Methods

        public IEnumerable<InvoiceCompanyUseMaster> Fillter(out int totalRecords, string key = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
        {
            var condition = new ConditionSearchInvoiCompanyUse(this.CurrentUser, key, orderType, orderby);
            totalRecords = this.invoiceCompanyUseReleaseBO.Count(condition);
            return this.invoiceCompanyUseReleaseBO.Filter(condition, skip, take);
        }

        public InvoiceCompanyUseInfo GetInvoiceCompanyUseInfo(int id)
        {
            int companyId = GetCompanyIdOfUser();
            return this.invoiceCompanyUseReleaseBO.GetById(id);
        }

        public ResultCode Create(InvoiceCompanyUseInfo invoiceReleasesInfo)
        {
            if (invoiceReleasesInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            invoiceReleasesInfo.CompanyId = this.CurrentUser.Company.Id;
            invoiceReleasesInfo.FileName = TempalateInvoice;
            int invoiceCompanyUseId = this.invoiceCompanyUseReleaseBO.Create(invoiceReleasesInfo);
            if (invoiceReleasesInfo.TemplateInvoice.IsNotNullOrEmpty())
            {
                SaveFileImport(invoiceReleasesInfo, invoiceCompanyUseId);
            }
            
            return invoiceCompanyUseId == 0 ? ResultCode.NoError : ResultCode.NoError;
        }

        public ResultCode Update(int id, InvoiceCompanyUseInfo invoiceReleasesInfo)
        {
            if (invoiceReleasesInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            int idCompany = GetCompanyIdOfUser();
            invoiceReleasesInfo.CompanyId = idCompany;
            invoiceReleasesInfo.FileName = TempalateInvoice;
            if (invoiceReleasesInfo.TemplateInvoice.IsNotNullOrEmpty())
            {
                SaveFileImport(invoiceReleasesInfo, id);
            }
            return this.invoiceCompanyUseReleaseBO.Update(id, invoiceReleasesInfo);
        }

        public ResultCode Delete(int id)
        {
            int companyId = GetCompanyIdOfUser();
            return this.invoiceCompanyUseReleaseBO.Delete(id, companyId);
        }
        
        private int GetCompanyIdOfUser()
        {
            int companyId = 0;
            if (this.CurrentUser.Company.Id.HasValue)
            {
                companyId = this.CurrentUser.Company.Id.Value;
            }

            return companyId;
        }

        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.UploadFolderImage);
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

        private void SaveFileImport(InvoiceCompanyUseInfo fileImport,int configReportId)
        {
            string FolderPathConfig = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.UploadFolderImage);
            int companyId = GetCompanyIdOfUser();
            string folder = string.Format(@"{0}\{1}\{2}", FolderPathConfig, companyId, configReportId);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fileName = string.Format(@"{0}\{1}.xlsx", folder, fileImport.FileName);

            try
            {
                File.WriteAllBytes(fileName, Convert.FromBase64String(fileImport.TemplateInvoice));
            }
            catch (Exception ex)
            {

                throw new BusinessLogicException(ResultCode.FileNotFound, ex.Message);
            }
        }

        #endregion
    }
}