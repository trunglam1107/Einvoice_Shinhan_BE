using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness'
    public class ReleaseInvoiceBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness'
    {
        #region Fields, Properties
        private static readonly Logger logger = new Logger();
        private readonly IReleaseInvoiceBO releaseInvoiceBO;
        private readonly IReleaseInvoiceDetailBO releaseInvoiceDetailBO;
        private readonly IInvoiceBO invoiceBO;
        private readonly PrintConfig config;
        private readonly IQuarztJobBO quarztJobBO;
        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ReleaseInvoiceBusiness(IBOFactory)'
        public ReleaseInvoiceBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ReleaseInvoiceBusiness(IBOFactory)'
        {
            config = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };
            this.releaseInvoiceBO = boFactory.GetBO<IReleaseInvoiceBO>(config, this.CurrentUser, emailConfig);
            this.invoiceBO = boFactory.GetBO<IInvoiceBO>(config, new EmailConfig());
            this.releaseInvoiceDetailBO = boFactory.GetBO<IReleaseInvoiceDetailBO>();
            this.quarztJobBO = boFactory.GetBO<IQuarztJobBO>();
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ReleaseInvoiceBusiness(IBOFactory, bool)'
        public ReleaseInvoiceBusiness(IBOFactory boFactory, bool byPass)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ReleaseInvoiceBusiness(IBOFactory, bool)'
        {
            if (byPass)
                config = GetPrintConfigbyPass();
            else
                config = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };
            this.releaseInvoiceBO = boFactory.GetBO<IReleaseInvoiceBO>(config, this.CurrentUser, emailConfig);
            this.invoiceBO = boFactory.GetBO<IInvoiceBO>(config, new EmailConfig());
            this.releaseInvoiceDetailBO = boFactory.GetBO<IReleaseInvoiceDetailBO>();
        }
        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ReleaseGroupInvoice(ReleaseGroupInvoice)'
        public Result ReleaseGroupInvoice(ReleaseGroupInvoice info)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ReleaseGroupInvoice(ReleaseGroupInvoice)'
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            info.CompanyId = GetCompanyIdOfUser();
            info.UserActionId = this.CurrentUser.Id;
            Result result = this.releaseInvoiceBO.ReleaseInvoice(info);
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.Create(ReleaseInvoiceMaster)'
        public Result Create(ReleaseInvoiceMaster releaseInvoice)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.Create(ReleaseInvoiceMaster)'
        {
            if (releaseInvoice == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            releaseInvoice.CompanyId = this.CurrentUser.Company.Id;
            releaseInvoice.UserAction = this.CurrentUser.Id;
            releaseInvoice.UserActionId = this.CurrentUser.Id;
            return this.releaseInvoiceBO.Create(releaseInvoice);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.UpdataStatusReleaseDetailInvoice(long, StatusReleaseInvoice)'
        public ResultCode UpdataStatusReleaseDetailInvoice(long releaseId, StatusReleaseInvoice statusReleaseInvoice)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.UpdataStatusReleaseDetailInvoice(long, StatusReleaseInvoice)'
        {
            if (statusReleaseInvoice == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.releaseInvoiceBO.UpdateStatusReleaseInvoiceDetail(releaseId, statusReleaseInvoice, this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.UpdataReleaseStatus(long, string)'
        public ResultCode UpdataReleaseStatus(long releaseId, string fullPathFile)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.UpdataReleaseStatus(long, string)'
        {
            StatusRelease statuRelase = new StatusRelease();
            if (!fullPathFile.IsNullOrEmpty())
            {
                statuRelase.Message = "Hóa đơn đã xác thực";
                statuRelase.Status = (int)ReleaseInvoiceStatus.ProcessSendEmail;
            }
            else
            {
                statuRelase.Message = "Lỗi trong quá trình xử lý lưu file trên server";
                statuRelase.Status = (int)ReleaseInvoiceStatus.SignError;
            }

            statuRelase.LoginId = this.CurrentUser.Id;
            long companyId = GetCompanyIdOfUser();
            return this.releaseInvoiceBO.Update(releaseId, companyId, statuRelase);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.Fillter(ConditionSearchReleaseInvoice)'
        public IEnumerable<ReleaseInvoices> Fillter(ConditionSearchReleaseInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.Fillter(ConditionSearchReleaseInvoice)'
        {
            condition.TotalRecords = this.releaseInvoiceBO.CountReleaseInvoice(condition);
            return this.releaseInvoiceBO.FilterReleaseInvoice(condition);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.FilterReleaseInvoice(ConditionSearchReleaseList)'
        public IEnumerable<ReleaseListInvoice> FilterReleaseInvoice(ConditionSearchReleaseList condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.FilterReleaseInvoice(ConditionSearchReleaseList)'
        {
            condition.TotalRecords = this.releaseInvoiceBO.CountFilterReleaseInvoice(condition);
            return this.releaseInvoiceBO.FilterReleaseInvoice(condition);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ReleaseInvoiceDetail(out long, long?, string, string, int, int)'
        public IEnumerable<InvoicesReleaseDetail> ReleaseInvoiceDetail(out long totalRecords, long? releaseId = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ReleaseInvoiceDetail(out long, long?, string, string, int, int)'
        {
            var codition = new ConditionSearchReleaseListDetail(this.CurrentUser, releaseId, orderBy, orderType);
            totalRecords = this.releaseInvoiceBO.CountReleaseInvoiceDetail(codition);
            return this.releaseInvoiceBO.ReleaseInvoiceDetail(codition, skip, take);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.FileInvoices(long)'
        public ExportFileInfo FileInvoices(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.FileInvoices(long)'
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            if (!InvoiceReleased(id))
            {
                return fileInfo;
            }

            PrintConfig myconfig = GetPrintConfig();
            fileInfo.FileName = string.Format("{0}.zip", id);
            fileInfo.FullPathFileName = Path.Combine(myconfig.FullPathFileReleaseExport, fileInfo.FileName);
            return fileInfo;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.GetReleaseInvoices(long)'
        public IEnumerable<InvoicesRelease> GetReleaseInvoices(long releaseId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.GetReleaseInvoices(long)'
        {
            long companyId = GetCompanyIdOfUser();
            List<int> releaseStatus = new List<int>() { (int)ReleaseInvoiceStatus.New, (int)ReleaseInvoiceStatus.SignError };
            return this.releaseInvoiceDetailBO.FilterReleaseInvoice(releaseId, companyId, releaseStatus);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.GetCompanyOfInvoice(long)'
        public RELEASEINVOICE GetCompanyOfInvoice(long invoiceId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.GetCompanyOfInvoice(long)'
        {
            var releaseInvoice = releaseInvoiceBO.GetReleaseInvoiceByInvoiceId(invoiceId);
            if (releaseInvoice == null || !releaseInvoice.COMPANYID.HasValue)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This release id [{0}] not found ", invoiceId));
            }

            return releaseInvoice;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.DeleteFileAfterSign(long)'
        public void DeleteFileAfterSign(long releaseId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.DeleteFileAfterSign(long)'
        {
            try
            {
                long companyId = GetCompanyIdOfUser();
                string pathFile = Path.Combine(Config.ApplicationSetting.Instance.FolderInvoiceFile, companyId.ToString(), AssetSignInvoice.Invoice);
                var releaseInvoice = this.releaseInvoiceBO.GetReleaseInvoiceById(releaseId);
                if (releaseInvoice.RELEASEINVOICEDETAILs.Count > 0)
                {
                    foreach (var item in releaseInvoice.RELEASEINVOICEDETAILs)
                    {
                        string pathPdf = Path.Combine(pathFile, item.INVOICEID + ".pdf");
                        string pathXml = Path.Combine(pathFile, item.INVOICEID + ".xml");
                        if (File.Exists(pathPdf))
                        {
                            File.Delete(pathPdf);
                        }
                        if (File.Exists(pathXml))
                        {
                            File.Delete(pathXml);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.GetInvoiceByReleaseId(long)'
        public IEnumerable<InvoicesReleaseDetail> GetInvoiceByReleaseId(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.GetInvoiceByReleaseId(long)'
        {
            return this.releaseInvoiceBO.GetInvoiceByReleaseId(id);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.GetInvoiceById(long)'
        public InvoiceInfo GetInvoiceById(long invoiceId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.GetInvoiceById(long)'
        {
            return this.invoiceBO.GetById(invoiceId);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.GetInvoicenfo(long)'
        public InvoiceInfo GetInvoicenfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.GetInvoicenfo(long)'
        {

            long companyId = GetCompanyIdOfUser();
            return this.invoiceBO.GetInvoiceInfo(id, companyId);

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.getNameXml(long)'
        public string getNameXml(long invoiceId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.getNameXml(long)'
        {
            long companyId = GetCompanyIdOfUser();
            var invoicePrint = GetInvoicenfo(invoiceId);
            string folderTree = DateTime.Now.Year.ToString() + "\\" + DateTime.Now.ToString("MM") + "\\" + DateTime.Now.ToString("dd");
            if (invoicePrint.ReleasedDate != null)
            {
                folderTree = invoicePrint.ReleasedDate.Value.Year.ToString() + "\\" + invoicePrint.ReleasedDate.Value.ToString("MM") + "\\" + invoicePrint.ReleasedDate.Value.ToString("dd");
            }

            string fileNameSymbol = invoicePrint.Id.ToString();

            string pathFile = Path.Combine(Config.ApplicationSetting.Instance.FolderInvoiceFile, companyId.ToString(), AssetSignInvoice.Release, AssetSignInvoice.SignFile, folderTree);

            string pathXml = Path.Combine(pathFile, fileNameSymbol + "_sign" + ".xml");
            return pathXml;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.UpdataInvoiceStatus(InvoiceInfo, int)'
        public void UpdataInvoiceStatus(InvoiceInfo invoice, int status)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.UpdataInvoiceStatus(InvoiceInfo, int)'
        {
            this.invoiceBO.UpdataInvoiceStatus(invoice, status);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.IscheckResponeCQT(long)'
        public InvoiceMaster IscheckResponeCQT(long Id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.IscheckResponeCQT(long)'
        {
            return this.releaseInvoiceBO.IscheckResponeCQT(Id);
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.IscheckResponeCode(long)'
        public ServerSignResult IscheckResponeCode(long Id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.IscheckResponeCode(long)'
        {
            return this.releaseInvoiceBO.IscheckCodeCQT(Id);
        }



#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.IscheckRepsoneTwanWithNotCode(long)'
        public ServerSignResult IscheckRepsoneTwanWithNotCode(long Id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.IscheckRepsoneTwanWithNotCode(long)'
        {
            return this.releaseInvoiceBO.IscheckRepsoneTwanWithNotCode(Id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.DeleteFileAfterSignTwanSucces(ReleaseInvoiceMaster)'
        public void DeleteFileAfterSignTwanSucces(ReleaseInvoiceMaster releaseInvoice)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.DeleteFileAfterSignTwanSucces(ReleaseInvoiceMaster)'
        {
            try
            {
                long companyId = GetCompanyIdOfUser();
                string pathFile = Path.Combine(Config.ApplicationSetting.Instance.FolderInvoiceFile, companyId.ToString(), AssetSignInvoice.Release, AssetSignInvoice.SignFile);
                string pathFileInvoice = Path.Combine(Config.ApplicationSetting.Instance.FolderInvoiceFile, companyId.ToString(), AssetSignInvoice.Invoice);

                if (releaseInvoice.ReleaseInvoiceInfos.Count > 0)
                {
                    foreach (var item in releaseInvoice.ReleaseInvoiceInfos)
                    {
                        string pathPdf = Path.Combine(pathFile, item.InvoiceId + "_sign" + ".pdf");
                        string pathXml = Path.Combine(pathFile, item.InvoiceId + "_sign" + ".xml");
                        string pathPdfInvoice = Path.Combine(pathFile, item.InvoiceId + ".pdf");
                        string pathXmlInvoice = Path.Combine(pathFile, item.InvoiceId + ".xml");
                        if (File.Exists(pathPdf))
                        {
                            File.Delete(pathPdf);
                        }
                        if (File.Exists(pathXml))
                        {
                            File.Delete(pathXml);
                        }
                        if (File.Exists(pathPdfInvoice))
                        {
                            File.Delete(pathPdfInvoice);
                        }
                        if (File.Exists(pathXmlInvoice))
                        {
                            File.Delete(pathXmlInvoice);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger logger = new Logger();
                logger.Error(this.CurrentUser.UserId, ex);
            }
        }
        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig myconfig = new PrintConfig(fullPathTemplateFolder, fullPathFileInvoice);
            myconfig.BuildAssetByCompany(this.CurrentUser.Company);
            myconfig.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            return myconfig;
        }
        private PrintConfig GetPrintConfigbyPass()
        {
            string fullPathFileAsset = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig myconfig = new PrintConfig(fullPathFileAsset, fullPathFileInvoice);
            myconfig.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            return myconfig;
        }
        private bool InvoiceReleased(long id)
        {
            bool isReleased = true;
            List<int> releaseStatus = new List<int>() { (int)ReleaseInvoiceStatus.New, (int)ReleaseInvoiceStatus.SignError };
            var releaseInvoice = this.releaseInvoiceBO.FilterInvoice(id, releaseStatus);
            if (releaseInvoice == null)
            {
                isReleased = false;
            }

            return isReleased;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ServiceSendMailByMonth()'
        public ResultCode ServiceSendMailByMonth()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ServiceSendMailByMonth()'
        {
            try
            {
                EmailServerInfo emailServerInfo;
                emailServerInfo = this.releaseInvoiceBO.GetEmailServerInfo();
                this.releaseInvoiceBO.SendMailByMonth(emailServerInfo);
                return ResultCode.NoError;
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return ResultCode.UnknownError;
            }

        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.SignFile(ReleaseInvoiceMaster)'
        public ServerSignResult SignFile(ReleaseInvoiceMaster releaseInvoice)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.SignFile(ReleaseInvoiceMaster)'
        {
            //check job sign có đang chạy k
            var quarzt = this.quarztJobBO.findByName("ReleaseInvoice");
            if (quarzt.PROCESSING == true)
            {
                throw new BusinessLogicException(ResultCode.JobSignProcess, MsgApiResponse.DataInvalid);
            }
            releaseInvoice.CompanyId = this.CurrentUser.Company.Id;
            releaseInvoice.UserAction = this.CurrentUser.Id;
            releaseInvoice.UserActionId = this.CurrentUser.Id;
            return this.releaseInvoiceBO.SignFileManual(releaseInvoice, this.CurrentUser);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.CheckSignedInvoice(ReleaseInvoiceMaster)'
        public void CheckSignedInvoice(ReleaseInvoiceMaster releaseInvoice)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.CheckSignedInvoice(ReleaseInvoiceMaster)'
        {
            List<long> invoiceIDs = new List<long>();
            foreach (var item in releaseInvoice.ReleaseInvoiceInfos)
            {
                invoiceIDs.Add(item.InvoiceId);
            }
            foreach (var item in releaseInvoice.ReleaseInvoiceInfos)
            {
                INVOICE invoice = this.releaseInvoiceBO.GetInvoiceCheckSign(item.InvoiceId);

                bool isSign = this.releaseInvoiceBO.CheckSignedInvoice(invoice, invoiceIDs);
                if (!isSign)
                {
                    throw new BusinessLogicException(ResultCode.CheckSignInvalid, "Bạn chưa phát hành hết hóa đơn của ngày trước.");
                }
            }
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ImportInvoiceXml(string)'
        public string ImportInvoiceXml(string invoice)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceBusiness.ImportInvoiceXml(string)'
        {
            return this.invoiceBO.ImportXmlInvoice(invoice);
        }

        #endregion
    }
}