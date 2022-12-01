using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness'
    public class AutoNumberBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness'
    {
        private readonly IAutoNumberBO autoNumberBO;
        private readonly IInvoiceBO invoiceBO;
        private readonly IMyCompanyBO myCompanyBO;
        private readonly PrintConfig printConfig;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.AutoNumberBusiness(IBOFactory)'
        public AutoNumberBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.AutoNumberBusiness(IBOFactory)'
        {
            printConfig = GetPrintConfig();
            this.autoNumberBO = boFactory.GetBO<IAutoNumberBO>();
            this.invoiceBO = boFactory.GetBO<IInvoiceBO>(printConfig);
            var uploadImageConfig = new UpdateloadImageConfig
            {
                MaxSizeImage = Config.ApplicationSetting.Instance.MaxSizeImage,
                RootFolderUpload = DefaultFields.ASSET_FOLDER
            };
            this.myCompanyBO = boFactory.GetBO<IMyCompanyBO>(uploadImageConfig);
        }
        private PrintConfig GetPrintConfig()
        {
            var fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder, fullPathFileInvoice);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.GetAll(ConditionSearchAutoNumber)'
        public IEnumerable<AutoNumberViewModel> GetAll(ConditionSearchAutoNumber condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.GetAll(ConditionSearchAutoNumber)'
        {
            return this.autoNumberBO.GetAll(condition);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.checkMorethanOneMinus(string)'
        public int checkMorethanOneMinus(string id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.checkMorethanOneMinus(string)'
        {
            var condition = new ConditionSearchAutoNumber(null, null)
            {
                CurrentUser = this.CurrentUser,
            };
            IEnumerable<AutoNumberViewModel> result = this.GetAll(condition);
            int count = 0;
            foreach (AutoNumberViewModel loop in result)
            {
                if (loop.ACTIVED && loop.TYPEID.Equals(id))
                    count = count + 1;
            }
            if (count > 1)
            {
                return 2;
            }
            else if (count == 1)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.GetById(string)'
        public AutoNumberViewModel GetById(string id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.GetById(string)'
        {
            return this.autoNumberBO.GetByID(id);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.Update(string, AutoNumberViewModel)'
        public ResultCode Update(string code, AutoNumberViewModel autoNumberViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.Update(string, AutoNumberViewModel)'
        {
            if (autoNumberViewModel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            autoNumberViewModel.UPDATEUSERSID = this.CurrentUser.ClientId;
            return this.autoNumberBO.Update(code, autoNumberViewModel);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.Create(AutoNumberViewModel)'
        public ResultCode Create(AutoNumberViewModel autoNumberViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.Create(AutoNumberViewModel)'
        {
            if (autoNumberViewModel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            autoNumberViewModel.CREATEUSERSID = this.CurrentUser.Company.Id ?? 0;
            return ResultCode.NoError;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.Delete(string)'
        public ResultCode Delete(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.Delete(string)'
        {
            return this.autoNumberBO.Delete(code);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.loadAnnoucement(long, string)'
        public string loadAnnoucement(long invoiceId, string TYPEID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.loadAnnoucement(long, string)'
        {
            string result = null;
            bool enable = this.autoNumberBO.checkEnable(TYPEID);
            if (invoiceId.Equals(0))
            {
                long company = this.CurrentUser.Company.Id ?? 0;
                if (enable)
                {
                    result = this.autoNumberBO.CreateAutoNumber(company, TYPEID, this.CurrentUser.Company.BranchId, false);
                }
            }
            else
            {
                InvoiceInfo invoiceInfo = invoiceBO.GetById(invoiceId);
                var company = myCompanyBO.GetCompanyInfo(invoiceInfo.CompanyId ?? 0);
                if (enable)
                {
                    result = this.autoNumberBO.CreateAutoNumber(invoiceInfo.CompanyId ?? 0, TYPEID, company.BranchId, false);
                }
            }
            return result;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.loadAnnoucementLenth(string)'
        public int loadAnnoucementLenth(string TYPEID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberBusiness.loadAnnoucementLenth(string)'
        {
            return this.autoNumberBO.Length(TYPEID);
        }
    }
}