using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using System;
using System.Collections.Generic;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness'
    public class NoticeUseInvoiceBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness'
    {
        #region Fields, Properties

        private readonly INoticeUseInvoiceBO noticeUseInvoiceReleaseBO;
        private readonly IRegisterTemplatesBO templateCompanyUseReleaseBO;
        private readonly INoticeUseInvoiceDetailBO noticeUseInvoiceDetailBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.NoticeUseInvoiceBusiness(IBOFactory)'
        public NoticeUseInvoiceBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.NoticeUseInvoiceBusiness(IBOFactory)'
        {
            var printConfig = GetPrintConfig();
            this.noticeUseInvoiceReleaseBO = boFactory.GetBO<INoticeUseInvoiceBO>(printConfig, this.CurrentUser);
            this.templateCompanyUseReleaseBO = boFactory.GetBO<IRegisterTemplatesBO>(printConfig);
            this.noticeUseInvoiceDetailBO = boFactory.GetBO<INoticeUseInvoiceDetailBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.Fillter(ConditionSearchUseInvoice)'
        public IEnumerable<NotificeUseInvoiceMaster> Fillter(ConditionSearchUseInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.Fillter(ConditionSearchUseInvoice)'
        {
            condition.TotalRecords = this.noticeUseInvoiceReleaseBO.Count(condition);
            return this.noticeUseInvoiceReleaseBO.Filter(condition);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.GetInvoiceSample()'
        public IEnumerable<RegisterTemplateMaster> GetInvoiceSample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.GetInvoiceSample()'
        {
            long companyId = GetCompanyIdOfUser();
            return templateCompanyUseReleaseBO.GetList(companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.GetNoticeUseInvoiceInfo(long)'
        public UseInvoiceInfo GetNoticeUseInvoiceInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.GetNoticeUseInvoiceInfo(long)'
        {
            long companyId = GetCompanyIdOfUser();

            return this.noticeUseInvoiceReleaseBO.GetNoticeUseInvoiceInfo(id, companyId);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.ViewGetNoticeUseInvoiceInfo(long)'
        public UseInvoiceInfo ViewGetNoticeUseInvoiceInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.ViewGetNoticeUseInvoiceInfo(long)'
        {
            long companyId = GetCompanyIdOfUser();

            return this.noticeUseInvoiceReleaseBO.GetNoticeUseInvoiceInfo(id, companyId, true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.Create(UseInvoiceInfo)'
        public ResultCode Create(UseInvoiceInfo useInvoiceInfoInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.Create(UseInvoiceInfo)'
        {
            if (useInvoiceInfoInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            ResultCode result = this.noticeUseInvoiceReleaseBO.Create(useInvoiceInfoInfo);
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.Update(long, UseInvoiceInfo)'
        public ResultCode Update(long id, UseInvoiceInfo useInvoiceInfoInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.Update(long, UseInvoiceInfo)'
        {
            if (useInvoiceInfoInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long idCompany = GetCompanyIdOfUser();
            return this.noticeUseInvoiceReleaseBO.Update(id, idCompany, useInvoiceInfoInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.UpdateStatus(long, InvoiceReleasesStatus, bool)'
        public ResultCode UpdateStatus(long id, InvoiceReleasesStatus invoiceStatus, bool isUnApproved = false)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.UpdateStatus(long, InvoiceReleasesStatus, bool)'
        {
            if (invoiceStatus == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            if (!Enum.IsDefined(typeof(RecordStatus), invoiceStatus.Status))
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            if (isUnApproved && this.noticeUseInvoiceDetailBO.UsedByInvoices(id))
            {
                throw new BusinessLogicException(ResultCode.NoiticeUseInvoiceHasBeenUsedNotCancel, "Symbol has been use!");
            }

            long idCompany = GetCompanyIdOfUser();
            invoiceStatus.ActionUserId = this.CurrentUser.Id;
            return this.noticeUseInvoiceReleaseBO.UpdateStatus(id, idCompany, invoiceStatus, isUnApproved);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.Delete(long)'
        {
            long companyId = GetCompanyIdOfUser();
            return this.noticeUseInvoiceReleaseBO.Delete(id, companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.GetNumumberInvoiceStart(long, string, string, long)'
        public InvoiceNumber GetNumumberInvoiceStart(long registerTemplateId, string prefix, string suffix, long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.GetNumumberInvoiceStart(long, string, string, long)'
        {
            if (companyId == 0) companyId = GetCompanyIdOfUser();
            string symbol = string.Format("{0}{1}E", prefix, suffix);
            decimal numberInvoiceStart = this.noticeUseInvoiceReleaseBO.GetStartNumberInvoiceCompanyUse(companyId, registerTemplateId, symbol);
            return new InvoiceNumber(numberInvoiceStart);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.DownloadNotificationInvoiceXml(long)'
        public ExportFileInfo DownloadNotificationInvoiceXml(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.DownloadNotificationInvoiceXml(long)'
        {
            return this.noticeUseInvoiceReleaseBO.DownloadNotificationXML(id, this.CurrentUser.Company);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.DownloadNotificationInvoicePdf(long)'
        public ExportFileInfo DownloadNotificationInvoicePdf(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.DownloadNotificationInvoicePdf(long)'
        {
            long companyId = GetCompanyIdOfUser();
            ExportFileInfo fileInfoPdf = this.noticeUseInvoiceReleaseBO.DownloadNotificationPDF(id, this.CurrentUser.Company);
            return fileInfoPdf;
        }
        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            string fullPathFileExportData = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.FolderExportDataOfCompany);
            PrintConfig config = new PrintConfig(fullPathTemplateFolder, fullPathFileInvoice, fullPathFileExportData);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.PrintView(long)'
        public ExportFileInfo PrintView(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceBusiness.PrintView(long)'
        {
            return this.noticeUseInvoiceReleaseBO.DownloadNotificationPDF(id, this.CurrentUser.Company);
        }
        #endregion
    }
}