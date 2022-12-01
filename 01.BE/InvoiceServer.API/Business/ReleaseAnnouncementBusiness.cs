using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness'
    public class ReleaseAnnouncementBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness'
    {
        #region Fields, Properties

        private readonly IReleaseAnnouncementBO releaseAnnouncementBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.ReleaseAnnouncementBusiness(IBOFactory)'
        public ReleaseAnnouncementBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.ReleaseAnnouncementBusiness(IBOFactory)'
        {
            PrintConfig config = GetPrintConfig();
            this.releaseAnnouncementBO = boFactory.GetBO<IReleaseAnnouncementBO>(config, this.CurrentUser);
        }

        #endregion Contructor

        #region Methods

        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder, fullPathFileInvoice);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.SignAnnounFile(ReleaseAnnouncementMaster)'
        public ServerSignAnnounResult SignAnnounFile(ReleaseAnnouncementMaster releaseAnnouncement)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.SignAnnounFile(ReleaseAnnouncementMaster)'
        {
            //releaseAnnouncement.CompanyId = releaseAnnouncement.CompanyId;
            releaseAnnouncement.UserAction = this.CurrentUser.Id;
            releaseAnnouncement.UserActionId = this.CurrentUser.Id;
            return this.releaseAnnouncementBO.SignAnnounFile(releaseAnnouncement, this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.Create(ReleaseAnnouncementMaster)'
        public Result Create(ReleaseAnnouncementMaster releaseAnnouncement)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.Create(ReleaseAnnouncementMaster)'
        {
            if (releaseAnnouncement == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            releaseAnnouncement.CompanyId = this.CurrentUser.Company.Id;
            releaseAnnouncement.UserAction = this.CurrentUser.Id;
            releaseAnnouncement.UserActionId = this.CurrentUser.Id;
            return this.releaseAnnouncementBO.Create(releaseAnnouncement);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.GetReleaseAnnouncements(long)'
        public IEnumerable<AnnouncementsRelease> GetReleaseAnnouncements(long releaseId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.GetReleaseAnnouncements(long)'
        {
            long companyId = GetCompanyIdOfUser();
            List<int> releaseStatus = new List<int>() { (int)ReleaseAnnouncementStatus.New, (int)ReleaseAnnouncementStatus.SignError };
            return this.releaseAnnouncementBO.FilterReleaseAnnouncement(releaseId, companyId, releaseStatus);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.GetReleaseAnnouncementByAnnouncementId(long)'
        public RELEASEANNOUNCEMENT GetReleaseAnnouncementByAnnouncementId(long AnnouncementId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.GetReleaseAnnouncementByAnnouncementId(long)'
        {
            var releaseAnnouncement = releaseAnnouncementBO.GetReleaseAnnouncementByAnnouncementId(AnnouncementId);
            if (releaseAnnouncement == null || !releaseAnnouncement.COMPANYID.HasValue)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This release id [{0}] not found ", AnnouncementId));
            }

            return releaseAnnouncement;

        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.UpdataStatusReleaseDetailAnnouncement(long, StatusReleaseAnnouncement)'
        public ResultCode UpdataStatusReleaseDetailAnnouncement(long releaseId, StatusReleaseAnnouncement statusReleaseAnnouncement)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.UpdataStatusReleaseDetailAnnouncement(long, StatusReleaseAnnouncement)'
        {
            if (statusReleaseAnnouncement == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.releaseAnnouncementBO.UpdateStatusReleaseAnnouncementDetail(releaseId, statusReleaseAnnouncement, this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.UpdataReleaseStatus(long, string)'
        public ResultCode UpdataReleaseStatus(long releaseId, string fullPathFile)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementBusiness.UpdataReleaseStatus(long, string)'
        {
            StatusRelease statuRelase = new StatusRelease();
            if (!fullPathFile.IsNullOrEmpty())
            {
                statuRelase.Message = "Biên bản đã xác thực";
                statuRelase.Status = (int)ReleaseInvoiceStatus.ProcessSendEmail;
            }
            else
            {
                statuRelase.Message = "Lỗi trong quá trình xử lý lưu file trên server";
                statuRelase.Status = (int)ReleaseInvoiceStatus.SignError;
            }

            statuRelase.LoginId = this.CurrentUser.Id;
            long companyId = GetCompanyIdOfUser();
            return this.releaseAnnouncementBO.Update(releaseId, companyId, statuRelase);
        }

        #endregion
    }
}