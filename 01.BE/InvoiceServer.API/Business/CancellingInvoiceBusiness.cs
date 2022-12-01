using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using System.IO;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CancellingInvoiceBusiness'
    public class CancellingInvoiceBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CancellingInvoiceBusiness'
    {
        #region Fields, Properties

        private readonly ICancellingInvoiceBO cancellingInvoiceBO;
        private readonly IReleaseInvoiceBO releaseInvoiceBO;
        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CancellingInvoiceBusiness.CancellingInvoiceBusiness(IBOFactory)'
        public CancellingInvoiceBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CancellingInvoiceBusiness.CancellingInvoiceBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };
            this.cancellingInvoiceBO = boFactory.GetBO<ICancellingInvoiceBO>(printConfig);
            this.releaseInvoiceBO = boFactory.GetBO<IReleaseInvoiceBO>(printConfig, this.CurrentUser, emailConfig);
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CancellingInvoiceBusiness.Create(CancellingInvoiceMaster)'
        public Result Create(CancellingInvoiceMaster info)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CancellingInvoiceBusiness.Create(CancellingInvoiceMaster)'
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            info.ActionUserId = this.CurrentUser.Id;
            info.CompanyId = GetCompanyIdOfUser();
            info.FolderPath = Config.ApplicationSetting.Instance.FolderInvoiceFile + "\\FileRecipt";

            // create folder FileRecipt for case it not exist
            if (!Directory.Exists(info.FolderPath))
            {
                Directory.CreateDirectory(info.FolderPath);
            }

            ReleaseInvoiceMaster releaseInfo = this.cancellingInvoiceBO.Create(info);
            return ReleaseInvoice(releaseInfo);
        }


        private Result ReleaseInvoice(ReleaseInvoiceMaster releaseInfo)
        {
            Result result = null;
            if (releaseInfo.ReleaseInvoiceInfos.Count < 1)
            {
                return result;
            }

            return releaseInvoiceBO.Create(releaseInfo, ReleaseInvoiceStatus.SendEmailSuccess);
        }

        private PrintConfig GetPrintConfig()
        {
            var fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder, fullPathFileInvoice);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

        #endregion
    }
}