using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness'
    public class RegisterTemplateBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness'
    {
        #region Fields, Properties
        private readonly IRegisterTemplatesBO registerTemplateBO;
        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.RegisterTemplateBusiness(IBOFactory)'
        public RegisterTemplateBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.RegisterTemplateBusiness(IBOFactory)'
        {
            var printConfig = GetPrintConfig();
            this.registerTemplateBO = boFactory.GetBO<IRegisterTemplatesBO>(printConfig, this.CurrentUser);
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.Fillter(ConditionSearchRegisterTemplate)'
        public IEnumerable<RegisterTemplateMaster> Fillter(ConditionSearchRegisterTemplate condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.Fillter(ConditionSearchRegisterTemplate)'
        {
            condition.TotalRecords = this.registerTemplateBO.Count(condition);
            return this.registerTemplateBO.Filter(condition);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.GetTemplateCompanyUseInfo(long)'
        public RegisterTemplateInfo GetTemplateCompanyUseInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.GetTemplateCompanyUseInfo(long)'
        {
            return this.registerTemplateBO.GetById(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.Create(RegisterTemplateInfo)'
        public ResultCode Create(RegisterTemplateInfo invoiceReleasesInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.Create(RegisterTemplateInfo)'
        {
            if (invoiceReleasesInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            if (invoiceReleasesInfo.CompanyId == null)
            {
                invoiceReleasesInfo.CompanyId = this.CurrentUser.Company.Id;
            }
            this.registerTemplateBO.Create(invoiceReleasesInfo);
            return ResultCode.NoError;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.Update(long, RegisterTemplateInfo)'
        public ResultCode Update(long id, RegisterTemplateInfo registerTemplateInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.Update(long, RegisterTemplateInfo)'
        {
            if (registerTemplateInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            long idCompany = GetCompanyIdOfUser();
            if (registerTemplateInfo.CompanyId == null)
            {
                registerTemplateInfo.CompanyId = idCompany;
            }
            return this.registerTemplateBO.Update(id, registerTemplateInfo);

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.Delete(long)'
        {
            return this.registerTemplateBO.Delete(id);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.PrintView(string)'
        public ExportFileInfo PrintView(string parttern)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.PrintView(string)'
        {
            long companyId = GetCompanyIdOfUser();
            string partternOfTemplate = FilterParameterInString(parttern, string.Empty, true);
            string prefix = FilterParameterInString(parttern, "prefix=");
            string suffix = FilterParameterInString(parttern, "suffix=");
            string symbol = string.Format("{0}/{1}E", prefix, suffix);
            string numberFrom = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
            ExportFileInfo fileInfo = this.registerTemplateBO.PrintView(companyId, partternOfTemplate, symbol, numberFrom);
            return fileInfo;
        }

        private PrintConfig GetPrintConfig()
        {
            string fullPathFolderAsset = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathFolderAsset);
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }


        private string FilterParameterInString(string parameter, string key, bool pattern = false)
        {
            string vaues = string.Empty;
            string[] parameters = parameter.Replace("undefined", "").Split('&');
            if (parameters.Length == 0)
            {
                return vaues;
            }
            if (pattern)
            {
                return parameters[0].Trim();
            }

            foreach (var item in parameters)
            {
                if (item.IndexOf(key) > -1)
                {
                    vaues = item.Replace(key, "").Trim();
                }
            }

            return vaues;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.GetRegisterTemplateSampleOfCompany(int)'
        public IEnumerable<RegisterTemplateSample> GetRegisterTemplateSampleOfCompany(int companyID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateBusiness.GetRegisterTemplateSampleOfCompany(int)'
        {
            return this.registerTemplateBO.GetRegisterTemplateSampleOfCompany(companyID);
        }
        #endregion
    }
}