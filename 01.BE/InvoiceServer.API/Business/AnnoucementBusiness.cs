using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Annoucement;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness'
    public class AnnoucementBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness'
    {
        #region Fields, Properties

        private readonly IAnnouncementBO announcementBO;
        private readonly PrintConfig printConfig;
        private readonly IEmailActiveBO emailActiveBO;
        private readonly INoticeUseInvoiceDetailBO noticeUseInvoiceDetailBO;
        private readonly IAutoNumberBO autoNumberBO;
        private readonly IMyCompanyBO myCompanyBO;
        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.AnnoucementBusiness(IBOFactory)'
        public AnnoucementBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.AnnoucementBusiness(IBOFactory)'
        {
            printConfig = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };
            this.announcementBO = boFactory.GetBO<IAnnouncementBO>(printConfig, emailConfig, this.CurrentUser);
            this.emailActiveBO = boFactory.GetBO<IEmailActiveBO>(printConfig);
            this.noticeUseInvoiceDetailBO = boFactory.GetBO<INoticeUseInvoiceDetailBO>();
            this.autoNumberBO = boFactory.GetBO<IAutoNumberBO>();
            var uploadImageConfig = new UpdateloadImageConfig
            {
                MaxSizeImage = Config.ApplicationSetting.Instance.MaxSizeImage,
                RootFolderUpload = DefaultFields.ASSET_FOLDER
            };
            this.myCompanyBO = boFactory.GetBO<IMyCompanyBO>(uploadImageConfig);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.AnnoucementBusiness(IBOFactory, bool)'
        public AnnoucementBusiness(IBOFactory boFactory, bool byPass)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.AnnoucementBusiness(IBOFactory, bool)'
        {
            if (byPass)
                printConfig = GetPrintConfigbyPass();
            else
                printConfig = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };
            this.announcementBO = boFactory.GetBO<IAnnouncementBO>(printConfig, emailConfig, this.CurrentUser);
            this.emailActiveBO = boFactory.GetBO<IEmailActiveBO>(printConfig);
            this.noticeUseInvoiceDetailBO = boFactory.GetBO<INoticeUseInvoiceDetailBO>();

        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.Fillter(ConditionSearchAnnouncement)'
        public IEnumerable<AnnouncementMaster> Fillter(ConditionSearchAnnouncement condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.Fillter(ConditionSearchAnnouncement)'
        {
            condition.TotalRecords = this.announcementBO.Count(condition);
            return this.announcementBO.Filter(condition);
        }

        private PrintConfig GetPrintConfig()
        {
            string fullPathFileAsset = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            string fullPathFileExportData = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.FolderExportDataOfCompany);
            PrintConfig config = new PrintConfig(fullPathFileAsset, fullPathFileInvoice, fullPathFileExportData);
            if (this.CurrentUser != null)
            {
                config.BuildAssetByCompany(this.CurrentUser.Company);
            }
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            return config;
        }

        private PrintConfig GetPrintConfigbyPass()
        {
            string fullPathFileAsset = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig config = new PrintConfig(fullPathFileAsset, fullPathFileInvoice);
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            return config;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.ExportExcel(ConditionSearchAnnouncement)'
        public ExportFileInfo ExportExcel(ConditionSearchAnnouncement condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.ExportExcel(ConditionSearchAnnouncement)'
        {
            ExportFileInfo fileInfo = this.announcementBO.ExportExcel(condition);
            return fileInfo;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.PrintView(long)'
        public ExportFileInfo PrintView(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.PrintView(long)'
        {
            long companyId = GetCompanyIdOfUser();
            ExportFileInfo fileInfo = this.announcementBO.PrintView(id, companyId, true);
            return fileInfo;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.PrintViewPdf(long)'
        public ExportFileInfo PrintViewPdf(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.PrintViewPdf(long)'
        {
            long companyId = GetCompanyIdOfUser();
            ExportFileInfo fileInfoPdf = this.announcementBO.PrintViewPdf(id, companyId);
            return fileInfoPdf;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.PrintViewPdfPortal(long, long)'
        public ExportFileInfo PrintViewPdfPortal(long id, long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.PrintViewPdfPortal(long, long)'
        {
            ExportFileInfo fileInfoPdf = this.announcementBO.PrintViewPdf(id, companyId);
            return fileInfoPdf;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.ApproveAnnouncement(ApprovedAnnouncement)'
        public ResultApproved ApproveAnnouncement(ApprovedAnnouncement approveInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.ApproveAnnouncement(ApprovedAnnouncement)'
        {
            if (approveInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            approveInfo.CompanyId = GetCompanyIdOfUser();
            approveInfo.UserActionId = this.CurrentUser.Id;
            return this.announcementBO.ApproveAnnouncement(approveInfo, this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.CancelApproveAnnouncement(ApprovedAnnouncement)'
        public ResultApproved CancelApproveAnnouncement(ApprovedAnnouncement approveInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.CancelApproveAnnouncement(ApprovedAnnouncement)'
        {
            if (approveInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            approveInfo.CompanyId = GetCompanyIdOfUser();
            approveInfo.UserActionId = this.CurrentUser.Id;
            return this.announcementBO.CancelApproveAnnouncement(approveInfo, this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.Delete(long)'
        {
            return this.announcementBO.Delete(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.GetAnnouncementInfo(long)'
        public AnnouncementInfo GetAnnouncementInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.GetAnnouncementInfo(long)'
        {
            long companyId = GetCompanyIdOfUser();
            return this.announcementBO.GetAnnouncementInfo(id, companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.GetAnnouncementOfClient(long)'
        public AnnouncementsRelease GetAnnouncementOfClient(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.GetAnnouncementOfClient(long)'
        {
            return this.announcementBO.GetAnnouncementOfClient(id);
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.GetAnnouncementByInvoiceId(long, int)'
        public AnnouncementInfo GetAnnouncementByInvoiceId(long invoiceId, int announType)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.GetAnnouncementByInvoiceId(long, int)'
        {
            long companyId = GetCompanyIdOfUser();
            return this.announcementBO.GetAnnouncementByInvoiceId(invoiceId, announType, companyId);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.CheckExistAnnoun(long, bool, int)'
        public AnnouncementInfo CheckExistAnnoun(long invoiceId, bool invoiceType, int announType)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.CheckExistAnnoun(long, bool, int)'
        {
            return this.announcementBO.CheckExistAnnoun(invoiceId, invoiceType, announType);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.CheckAnnount(long, int)'
        public bool CheckAnnount(long invoiceId, int type)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.CheckAnnount(long, int)'
        {
            return this.announcementBO.CheckAnnount(invoiceId, type);
        }
        //Nghia

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.SearchCode(string, string, DateTime, string)'
        public ResultCode SearchCode(string ms, string kh, DateTime date, string num)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.SearchCode(string, string, DateTime, string)'
        {
            return this.announcementBO.SearchCode(ms, kh, date, num);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.SaveMD(AnnouncementMD)'
        public ANNOUNCEMENT SaveMD(AnnouncementMD anInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.SaveMD(AnnouncementMD)'
        {
            string announcementId = "BBHHD";
            if (anInfo.AnnouncementType == 1)
                announcementId = "BBDCHD";
            if (anInfo.AnnouncementType == 2)
                announcementId = "BBTTHD";
            var company = this.myCompanyBO.GetCompanyInfo(anInfo.CompanyId);
            AutoNumberViewModel find = this.autoNumberBO.GetByID(announcementId);
            if (find.ACTIVED)
            {
                anInfo.MinutesNo = this.autoNumberBO.CreateAutoNumber(anInfo.CompanyId, announcementId, company.BranchId, true);
            }
            return this.announcementBO.SaveMD(anInfo);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.checkExist(string, long?, long)'
        public bool checkExist(string code, long? id, long companyid)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.checkExist(string, long?, long)'
        {
            return this.announcementBO.checkExist(code, id, companyid);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.Update(AnnouncementMD)'
        public ResultCode Update(AnnouncementMD anInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.Update(AnnouncementMD)'
        {
            return this.announcementBO.Update(anInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.UpdateAnnouncementStatus(AnnouncementMD)'
        public string UpdateAnnouncementStatus(AnnouncementMD anInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.UpdateAnnouncementStatus(AnnouncementMD)'
        {
            return this.announcementBO.UpdateAnnouncementStatus(anInfo, this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.LoadList(long, int, bool)'
        public AnnouncementMD LoadList(long ID, int actiontype, bool isAnnounId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.LoadList(long, int, bool)'
        {
            return this.announcementBO.LoadList(ID, actiontype, isAnnounId, this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.GetInvoiceData(long)'
        public InvoicePrintInfo GetInvoiceData(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.GetInvoiceData(long)'
        {
            return this.announcementBO.GetInvoiceData(id);
        }

        public InvoicePrintInfo GetInvoiceData_Replace(long id)
        {
            return this.announcementBO.GetInvoiceData_Replace(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.Remove(long, long)'
        public ResultCode Remove(long announcementID, long updatedBy)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.Remove(long, long)'
        {
            return this.announcementBO.Remove(announcementID, updatedBy);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.IsExit(long, int)'
        public string IsExit(long id, int announcementType)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.IsExit(long, int)'
        {
            return this.announcementBO.IsExit(id, announcementType);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.IsExistAll(long)'
        public string IsExistAll(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnoucementBusiness.IsExistAll(long)'
        {
            return this.announcementBO.IsExistAll(id);
        }

        public ResultCode SendVerificationCode(AnnouncementVerificationCode announcementVerificationCode)
        {
            if (announcementVerificationCode == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            announcementVerificationCode.NumberFormat = this.CurrentUser.Company.NumberFormat.IsNullOrEmpty() ? "{0:#,##0.###}" : this.CurrentUser.Company.NumberFormat;
            announcementVerificationCode.CompanyId = GetCompanyIdOfUser();
            long emailNoticeId = 0;
            SendEmailVerificationCodeAnnoun sendEmailVerificationCodeAnnoun = this.announcementBO.GetVerificationCode(announcementVerificationCode);
            emailNoticeId = this.announcementBO.callCreateEmailActive(announcementVerificationCode.CompanyId, sendEmailVerificationCodeAnnoun); //update create emailactive
            return this.emailActiveBO.SendEmailAnoumentDaily(emailNoticeId, sendEmailVerificationCodeAnnoun);
        }

        public IEnumerable<string> FilterInvoiceSymbol(long invoiceTemplateId, long companyId)
        {
            return this.noticeUseInvoiceDetailBO.FilterInvoiceSymbol(companyId, invoiceTemplateId, false);
        }
        #endregion
    }
}