using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class CancellingInvoiceBO : ICancellingInvoiceBO
    {
        #region Fields, Properties

        private readonly ICancellingInvoiceRepository cancellingInvoiceRepository;
        private readonly IInvoiceRepository invoiceRepository;
        private readonly IAnnouncementRepository announcementRepository;
        private readonly IDbTransactionManager transaction;
        private readonly PrintConfig config;
        #endregion

        #region Contructor

        public CancellingInvoiceBO(IRepositoryFactory repoFactory, PrintConfig printConfig)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.cancellingInvoiceRepository = repoFactory.GetRepository<ICancellingInvoiceRepository>();
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.announcementRepository = repoFactory.GetRepository<IAnnouncementRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.config = printConfig;
        }

        #endregion

        #region Methods

        public ReleaseInvoiceMaster Create(CancellingInvoiceMaster cancellingInfo)
        {
            ReleaseInvoiceMaster releaseInvoice = new ReleaseInvoiceMaster();
            try
            {
                var ajusmentInvoice = this.invoiceRepository.GetListInvoiceChild(cancellingInfo.InvoiceId);
                if (ajusmentInvoice.Any())
                {
                    bool isCancel = false;
                    foreach (var item in ajusmentInvoice)
                    {
                        if ((item.INVOICETYPE == (int)InvoiceType.AdjustmentUpDown || item.INVOICETYPE == (int)InvoiceType.AdjustmentTax || item.INVOICETYPE == (int)InvoiceType.AdjustmentTaxCode)
                            && item.INVOICESTATUS == (int)InvoiceStatus.Released
                            && item.DELETED != true)
                        {
                            throw new BusinessLogicException(ResultCode.CannotCancel, "Hóa đơn đang có hóa đơn điều chỉnh, không được phép hủy");
                        }
                        else
                        {
                            isCancel = true;
                        }
                    }
                    if (isCancel)
                    {
                        transaction.BeginTransaction();
                        if (cancellingInfo.FileReceipt != null)
                        {
                            cancellingInfo.FileReceipt = SaveFileImport(cancellingInfo);
                        }
                        releaseInvoice = CancellingInvoice(cancellingInfo);
                        transaction.Commit();
                    }
                }
                else
                {
                    transaction.BeginTransaction();
                    if (cancellingInfo.FileReceipt != null)
                    {
                        cancellingInfo.FileReceipt = SaveFileImport(cancellingInfo);
                    }
                    releaseInvoice = CancellingInvoice(cancellingInfo);
                    transaction.Commit();
                }

            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return releaseInvoice;
        }
        private string SaveFileImport(CancellingInvoiceMaster fileImport)
        {
            string fullPathFile = this.config.PathInvoiceFile;
            string filePathFileName = null;

            string fileName;
            fileName = string.Format(@"{0}\BienBanHuyHoaDon_{1}.pdf", fileImport.FolderPath, fileImport.InvoiceId);
            ANNOUNCEMENT currentAnnouncement = GetAnnouncement(fileImport.InvoiceId, fileImport.CompanyId);
            // filePathFileName nay duoc su dung trong truong hop bien ban da ky client
            filePathFileName = string.Format("{0}\\{1}\\Releases\\SignAnnouncement\\{2}_sign_sign.pdf", fullPathFile, currentAnnouncement?.COMPANYID, currentAnnouncement?.ID);

            try
            {
                if (currentAnnouncement?.COMPANYSIGN == true && currentAnnouncement?.CLIENTSIGN == true)
                {
                    File.Copy(filePathFileName, fileName, true);
                }
                else
                {
                    File.WriteAllBytes(fileName, Convert.FromBase64String(fileImport.FileReceipt));
                }
            }
            catch (Exception ex)
            {

                throw new BusinessLogicException(ResultCode.FileNotFound, ex.Message);
            }

            return fileName;
        }
        public ReleaseInvoiceMaster CancellingInvoice(CancellingInvoiceMaster cancellingInvoiceinfo, bool isAutoSino = false)
        {
            ReleaseInvoiceMaster releaseInvoice = new ReleaseInvoiceMaster();
            if (cancellingInvoiceinfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            List<ReleaseInvoiceInfo> releaseInvoicesInfo = new List<ReleaseInvoiceInfo>();
            INVOICE invoiceCancelling = UpdateInvoiceStatus(cancellingInvoiceinfo.InvoiceId, cancellingInvoiceinfo.FileReceipt);
            CANCELLINGINVOICE cancellingInvoice = new CANCELLINGINVOICE();
            cancellingInvoice.CopyData(cancellingInvoiceinfo);
            cancellingInvoice.CANCELBY = cancellingInvoiceinfo.ActionUserId;
            cancellingInvoice.INVOICEID = invoiceCancelling.ID;
            releaseInvoicesInfo.Add(new ReleaseInvoiceInfo() { InvoiceId = invoiceCancelling.ID, InvoiceNo = invoiceCancelling.NO });
            DeleteInvoiceFile(invoiceCancelling);
            this.cancellingInvoiceRepository.Insert(cancellingInvoice);
            releaseInvoice.SetDataForReleaseInvoice(releaseInvoicesInfo, cancellingInvoiceinfo.CompanyId, cancellingInvoiceinfo.ActionUserId, cancellingInvoiceinfo.Description, cancellingInvoiceinfo.CancellingDate);
            if (!isAutoSino)
            {
                UpdateAnnouncementStatus(cancellingInvoiceinfo.InvoiceId, cancellingInvoiceinfo.CompanyId);
            }
            return releaseInvoice;
        }

        public ANNOUNCEMENT UpdateAnnouncementStatus(long id, long companyId)
        {
            ANNOUNCEMENT currentAnnouncement = GetAnnouncement(id, companyId);
            if (currentAnnouncement == null)
            {
                return null;
            }
            currentAnnouncement.ANNOUNCEMENTSTATUS = (int)AnnouncementStatus.Successfull;
            this.announcementRepository.Update(currentAnnouncement);
            return currentAnnouncement;
        }
        public ANNOUNCEMENT GetAnnouncement(long id, long companyId)
        {
            ANNOUNCEMENT announcements = this.announcementRepository.GetByInvoiceId(id, 3, companyId);
            if (announcements == null)
            {
                throw new BusinessLogicException(ResultCode.AnnouncementNotExist, string.Format("The announcement does not exist for this invoice [{0}]", id));
            }

            return announcements;
        }

        private INVOICE UpdateInvoiceStatus(long id, string fileReceipt)
        {
            INVOICE currentInvoice = GetInvoice(id);
            List<int> listInvoiceCanCelling = new List<int>() { (int)InvoiceStatus.Released, (int)InvoiceStatus.Releaseding };
            if (currentInvoice.INVOICESTATUS.HasValue && !listInvoiceCanCelling.Contains(currentInvoice.INVOICESTATUS.Value))
            {
                throw new BusinessLogicException(ResultCode.InvoiceIssuedNotUpdate, "The not yet issued invoice is not edited");
            }

            currentInvoice.INVOICESTATUS = (int)InvoiceStatus.Cancel;
            currentInvoice.FILERECEIPT = fileReceipt;
            currentInvoice.UPDATEDDATE = DateTime.Now;
            currentInvoice.MESSAGECODE = null;
            this.invoiceRepository.Update(currentInvoice);
            return currentInvoice;
        }

        private INVOICE GetInvoice(long id)
        {
            INVOICE invoices = this.invoiceRepository.GetById(id);
            if (invoices == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This InvoiceId [{0}] not found in data of client", id));
            }

            return invoices;
        }

        private void DeleteInvoiceFile(INVOICE invoice)
        {
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, invoice.COMPANYID.ToString());
            string fullPathFileInvoiceSign = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, string.Format("{0}_{1}_Sign.pdf", invoice.ID, invoice.NO));
            string fullPathFileInvoice = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Invoice, string.Format("{0}_{1}.pdf", invoice.ID, invoice.NO));
            if (File.Exists(fullPathFileInvoiceSign))
            {
                File.Delete(fullPathFileInvoiceSign);
            }

            if (File.Exists(fullPathFileInvoice))
            {
                File.Delete(fullPathFileInvoice);
            }

        }
        #endregion
    }
}