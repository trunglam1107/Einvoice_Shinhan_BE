using InvoiceServer.Business.DAO;
using InvoiceServer.Business.ExportXml;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class NoticeUseInvoiceBO : INoticeUseInvoiceBO
    {
        #region Fields, Properties

        private readonly INotificationUseInvoiceRepository notificationUseRepository;
        private readonly INotificationUseInvoiceDetailRepository notificationUseDetailRepository;
        private readonly IInvoiceConcludeRepository invoiceConcludeRepository;
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly IDbTransactionManager transaction;
        private readonly PrintConfig config;
        private readonly UserSessionInfo currentUser;
        private readonly ISystemSettingRepository systemSettingRepository;
        private readonly ITaxDepartmentRepository taxDepartmentRepository;
        private readonly ICityRepository cityRepository;
        private readonly IRegisterTemplatesRepository registerTemplatesRepository;
        #endregion

        #region Contructor

        public NoticeUseInvoiceBO(IRepositoryFactory repoFactory, PrintConfig config)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");

            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.notificationUseRepository = repoFactory.GetRepository<INotificationUseInvoiceRepository>();
            this.notificationUseDetailRepository = repoFactory.GetRepository<INotificationUseInvoiceDetailRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.invoiceConcludeRepository = repoFactory.GetRepository<IInvoiceConcludeRepository>();
            this.taxDepartmentRepository = repoFactory.GetRepository<ITaxDepartmentRepository>();
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
            this.cityRepository = repoFactory.GetRepository<ICityRepository>();
            this.registerTemplatesRepository = repoFactory.GetRepository<IRegisterTemplatesRepository>();
            this.config = config;
        }
        public NoticeUseInvoiceBO(IRepositoryFactory repoFactory, PrintConfig config, UserSessionInfo userSessionInfo)
            : this(repoFactory, config)
        {
            this.currentUser = userSessionInfo;
        }
        #endregion

        #region Methods

        public IEnumerable<NotificeUseInvoiceMaster> Filter(ConditionSearchUseInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceConcludes = this.notificationUseRepository.Filter(condition);
            var res = invoiceConcludes;
            return res;
        }

        public long Count(ConditionSearchUseInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.notificationUseRepository.Count(condition);
        }

        public ResultCode Create(UseInvoiceInfo info)
        {
            // check so luong duoc phep dang ky phat hanh theo packager(= -1 -> unlimited)
            if (this.currentUser.NumberInvoicePackage != -1 && !CheckValidateNumberOfPackage(info.UseInvoiceDetails, false))
            {
                throw new BusinessLogicException(ResultCode.NumberNoiticeUseExceedNumberOfPackage, "number invoice use is exceed number invoice of package");
            }
            // Update data
            try
            {
                transaction.BeginTransaction();
                NOTIFICATIONUSEINVOICE notificationUse = InsertInvoiUse(info);
                InsertNotificationUseDetail(notificationUse.ID, info.UseInvoiceDetails);
                transaction.Commit();
            }
            catch (BusinessLogicException ex)
            {
                transaction.Rollback();
                throw new BusinessLogicException(ex.ErrorCode, ex.Message);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        public ResultCode Update(long id, long? companyId, UseInvoiceInfo info)
        {
            // check so luong duoc phep dang ky phat hanh theo packager(= -1 -> unlimited)
            if (this.currentUser.NumberInvoicePackage != -1 && !CheckValidateNumberOfPackage(info.UseInvoiceDetails, true, id))
            {
                throw new BusinessLogicException(ResultCode.NumberNoiticeUseExceedNumberOfPackage, "number invoice use is exceed number invoice of package");
            }
            // Update data
            try
            {
                transaction.BeginTransaction();
                UpdateInvoiUser(id, companyId, info);
                DeleteNoticeUseInvoiceDetail(id);
                InsertNotificationUseDetail(id, info.UseInvoiceDetails);
                transaction.Commit();
            }
            catch (BusinessLogicException ex)
            {
                transaction.Rollback();
                throw new BusinessLogicException(ex.ErrorCode, ex.Message);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        public UseInvoiceInfo GetNoticeUseInvoiceInfo(long id, long? companyId, bool isView = false)
        {
            var currentNoticeUseInvoice = this.notificationUseRepository.GetNoticeUseInvoiceInfo(id);

            if (!companyId.HasValue)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var HO = myCompanyRepository.GetHO();
            if (currentNoticeUseInvoice.CompanyId != companyId && HO.COMPANYSID != companyId && !isView)
            {
                throw new BusinessLogicException(ResultCode.NotPermisionData, ClientManagementInfo.NotPermissionData);
            }

            return currentNoticeUseInvoice;
        }

        public ResultCode Delete(long id, long? companyId)
        {
            try
            {
                transaction.BeginTransaction();
                DeleteNoticeUseInvoiceDetail(id);
                DeleteNoticeUseInvoice(id, companyId);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        public decimal GetStartNumberInvoiceCompanyUse(long companyId, long registerTemplateId, string symbol)
        {

            var notificationUseDetails = this.notificationUseDetailRepository.GetNotificationUseInvoiceDetails(companyId, registerTemplateId, symbol);
            var maxNotificationUseDetail = notificationUseDetails.OrderByDescending(p => p.NUMBERTO).FirstOrDefault();
            if (maxNotificationUseDetail != null)
            {
                return maxNotificationUseDetail.NUMBERTO.ToDecimal(0);
            }

            return 0M;
        }

        public ExportFileInfo DownloadNotificationXML(long id, CompanyInfo company)
        {
            ReporNotificationInvoice report = new ReporNotificationInvoice(company);
            report.NoticeInvoice = GetNoticeUseInvoiceInfo(id, company.Id);
            NotificationInvoiceXmL export = new NotificationInvoiceXmL(report, config);
            return export.ExportReport();
        }

        public ExportFileInfo DownloadNotificationPDF(long id, CompanyInfo company)
        {
            ExportFileInfo fileInfo = GetNoticeUseInvoiceFile(id);
            if (fileInfo == null)
            {
                NOTIFICATIONUSEINVOICE notificationUse = this.notificationUseRepository.GetById(id);

                if (notificationUse != null)
                {
                    var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(notificationUse.COMPANYID ?? 0);
                    var companyInfo = this.myCompanyRepository.GetById(company.Id ?? 0);
                    // truong hop la PGD thi get thong tin cua chi nhanh
                    if (companyInfo.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
                    {
                        companyInfo = this.myCompanyRepository.GetById((long)companyInfo.COMPANYID);
                    }
                    ReporNotificationInvoice report = new ReporNotificationInvoice(company);
                    report.NoticeInvoice = GetNoticeUseInvoiceInfo(id, company.Id);
                    NotificationDocx docx = new NotificationDocx(report.NoticeInvoice, config, systemSettingInfo, true);
                    bool isWord = false;
                    return docx.ExportFileInfo(isWord);
                }
            }

            return fileInfo;
        }
        private ExportFileInfo GetNoticeUseInvoiceFile(long id)
        {
            NOTIFICATIONUSEINVOICE notificationUse = this.notificationUseRepository.GetById(id);

            return FilterNoticeUseInvoiceFile(notificationUse);
        }
        private ExportFileInfo FilterNoticeUseInvoiceFile(NOTIFICATIONUSEINVOICE NoticeInvoice)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            string fullPathFile = null;
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, NoticeInvoice.COMPANYID.ToString());

            var fileNameSign = string.Format("{0}_sign.pdf", NoticeInvoice.ID);

            fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignNotification.Release, AssetSignNotification.SignFile, fileNameSign);
            fileInfo.FullPathFileName = fullPathFile;
            fileInfo.FullPathFileInvoice = Path.Combine(folderInvoiceOfCompany, AssetSignNotification.Contracts, fileNameSign);

            if (!File.Exists(fullPathFile))
            {
                return null;
            }

            fileInfo.FileName = "Notification.pdf";
            fileInfo.FullPathFileName = fullPathFile;
            return fileInfo;
        }
        public IEnumerable<long> GetListCompany()
        {
            return this.notificationUseRepository.GetListCompanyUse();
        }
        public List<MYCOMPANY> GetListCompanyShinhan()
        {
            return this.notificationUseRepository.GetListCompanyUseShinhan();
        }
        /// <summary>
        /// Lấy danh sách companies cần tạo files
        /// </summary>
        /// <returns></returns>
        public List<MYCOMPANY> GetListCompanyNeedCreateFiles()
        {
            return this.notificationUseRepository.GetListCompanyNeedCreateFiles();
        }
        public List<CompanySymbolInfo> GetSymbolCompanyForSignJob()
        {
            return this.notificationUseRepository.GetSymbolCompanyForSign();
        }
        /// <summary>
        /// Lấy danh sách companies cần ký
        /// </summary>
        /// <returns></returns>
        public List<MYCOMPANY> GetListCompanyNeedSignFiles()
        {
            return this.notificationUseRepository.GetListCompanyCreatedFiles();
        }

        /// <summary>
        /// Lấy danh sách công ty có hóa đơn cần duyệt
        /// </summary>
        /// <returns></returns>
        public List<MYCOMPANY> GetListCompanyNeedApproved()
        {
            return this.notificationUseRepository.GetListCompanyNeedApproved();
        }
        public List<MYCOMPANY> GetListCompanySendMails()
        {
            return this.notificationUseRepository.GetListCompanySendMails();   
        }
        #endregion

        #region Private Methods
        private NOTIFICATIONUSEINVOICE InsertInvoiUse(UseInvoiceInfo info)
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!info.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            var company = this.myCompanyRepository.GetById(info.CompanyId ?? 0);
            var taxDepartment = this.taxDepartmentRepository.GetById(info.TaxDepartmentId ?? 0);
            var city = this.cityRepository.GetById(info.CityId ?? 0);

            NOTIFICATIONUSEINVOICE notificationUse = new NOTIFICATIONUSEINVOICE();
            notificationUse.CopyData(info);
            notificationUse.STATUS = (int)RecordStatus.Created;
            notificationUse.CREATEDDATE = DateTime.Now;
            notificationUse.CREATEDBY = info.UserActionId;
            notificationUse.COMPANYNAME = company.COMPANYNAME;
            notificationUse.COMPANYADDRESS = company.ADDRESS;
            notificationUse.DELEGATE = company.DELEGATE;
            notificationUse.COMPANYTAXCODE = company.TAXCODE;
            notificationUse.BANKACCOUNT = company.BANKACCOUNT;
            notificationUse.COMPANYPHONE = company.TEL1;
            notificationUse.TAXDEPARTMENTNAME = taxDepartment?.NAME;
            notificationUse.CITYNAME = city.NAME;
            this.notificationUseRepository.Insert(notificationUse);
            return notificationUse;
        }

        private void UpdateInvoiUser(long id, long? companyId, UseInvoiceInfo info)
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!info.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            var company = this.myCompanyRepository.GetById(info.CompanyId ?? 0);
            var taxDepartment = this.taxDepartmentRepository.GetById(info.TaxDepartmentId ?? 0);
            var city = this.cityRepository.GetById(info.CityId ?? 0);

            NOTIFICATIONUSEINVOICE notificationUse = GetNotificationUse(id, companyId);

            notificationUse.CopyData(info);
            notificationUse.UPDATEDDATE = DateTime.Now;
            notificationUse.UPDATEDBY = info.UserActionId;
            notificationUse.COMPANYNAME = company.COMPANYNAME;
            notificationUse.COMPANYADDRESS = company.ADDRESS;
            notificationUse.DELEGATE = company.DELEGATE;
            notificationUse.COMPANYTAXCODE = company.TAXCODE;
            notificationUse.BANKACCOUNT = company.BANKACCOUNT;
            notificationUse.COMPANYPHONE = company.TEL1;
            notificationUse.TAXDEPARTMENTNAME = taxDepartment?.NAME;
            notificationUse.CITYNAME = city.NAME;
            this.notificationUseRepository.Update(notificationUse);
        }

        private void InsertNotificationUseDetail(long notificationUseId, List<UseInvoiceDetailInfo> notificationUseDetailInfo)
        {
            ResultCode errorCode;
            string errorMessage;
            notificationUseDetailInfo.ForEach(p =>
            {
                if (!p.IsValid(out errorCode, out errorMessage))
                {
                    throw new BusinessLogicException(errorCode, errorMessage);
                }

                p.Code = string.Format("{0}/{1}{2}", p.Prefix, p.Suffix, "E");
                //uypdate TT68

                var registerTemplate = this.registerTemplatesRepository.GetById(p.RegisterTemplateId ?? 0);
                var invoiceSample = registerTemplate.INVOICESAMPLE;
                var invoiceType = invoiceSample.INVOICETYPE;

                NOTIFICATIONUSEINVOICEDETAIL notificationUseDetail = new NOTIFICATIONUSEINVOICEDETAIL();
                notificationUseDetail.CopyData(p);
                notificationUseDetail.INVOICETYPENAME = invoiceType.NAME;
                notificationUseDetail.REGISTERTEMPLATECODE = registerTemplate.CODE;
                notificationUseDetail.INVOICESAMPLENAME = invoiceSample.NAME;
                notificationUseDetail.NOTIFICATIONUSEINVOICEID = notificationUseId;
                // khi tao moi gia tri mac dinh cua ISRECEIVEDWARNING = false
                notificationUseDetail.ISRECEIVEDWARNING = false;
                this.notificationUseDetailRepository.Insert(notificationUseDetail);
            });
        }

        public IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetListNotificationUseDetail(long notificationUseId)
        {
            return this.notificationUseDetailRepository.Filter(notificationUseId);
        }

        private NOTIFICATIONUSEINVOICE GetNotificationUse(long id, long? companyId, bool isview = false)
        {
            if (!companyId.HasValue)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            NOTIFICATIONUSEINVOICE notificationUse = GetNotificationUse(id);
            var tamp = myCompanyRepository.GetHO();
            if (notificationUse.COMPANYID != companyId && tamp.COMPANYSID != companyId && isview)
            {
                throw new BusinessLogicException(ResultCode.NotPermisionData, ClientManagementInfo.NotPermissionData);
            }

            return notificationUse;
        }

        private NOTIFICATIONUSEINVOICE GetNotificationUse(long id)
        {
            NOTIFICATIONUSEINVOICE notificationUse = this.notificationUseRepository.GetById(id);
            if (notificationUse == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.ResouceIdNotFound);
            }

            return notificationUse;
        }

        private void DeleteNoticeUseInvoice(long id, long? companyId)
        {
            var notificationUseInvoice = GetNotificationUse(id, companyId);

            notificationUseInvoice.DELETED = true;
            notificationUseInvoice.UPDATEDDATE = DateTime.Now;
            this.notificationUseRepository.Delete(notificationUseInvoice);
        }

        private void DeleteNoticeUseInvoiceDetail(long invoiceConclude)
        {
            var invoiceConcludeDetails = this.GetListNotificationUseDetail(invoiceConclude);
            invoiceConcludeDetails.ForEach(p =>
            {
                this.notificationUseDetailRepository.Delete(p);
            });
        }

        public ResultCode UpdateStatus(long id, long? companyId, InvoiceReleasesStatus status, bool isUnApproved = false)
        {
            NOTIFICATIONUSEINVOICE notificationUseInvoice = this.GetNotificationUse(id, companyId);
            if (notificationUseInvoice.STATUS.HasValue && notificationUseInvoice.STATUS.Value == (int)RecordStatus.Approved && !isUnApproved)
            {
                throw new BusinessLogicException(ResultCode.NoiticeUseInvoiceApprovedNotUpdate, "Notify the approved bill is not editable");
            }
            if (!isUnApproved)
            {
                var invoiceRelease = this.invoiceConcludeRepository.GetByNotification(id).ToList();
                if (invoiceRelease.Count > 0)
                {
                    throw new BusinessLogicException(ResultCode.InvoiceReleaseNotApproved, "Invoice release not approved");
                }
                // check so luong duoc phep dang ky phat hanh theo packager(= -1 -> unlimited)
                if (this.currentUser.NumberInvoicePackage != -1)
                {
                    long numberRegisted = 0;
                    var listSuffix = notificationUseInvoice.NOTIFICATIONUSEINVOICEDETAILs.GroupBy(p => new { p.SUFFIX })
                        .Select(g => new
                        {
                            suffix = g.Key.SUFFIX,
                            numberRegis = g.Sum(x => x.NUMBERUSE)
                        });
                    foreach (var item in listSuffix)
                    {
                        numberRegisted = this.notificationUseRepository.GetTotalRegisterBySuffixExceptCurrentNotice(item.suffix, id);
                        if (numberRegisted + item.numberRegis > this.currentUser.NumberInvoicePackage)
                        {
                            throw new BusinessLogicException(ResultCode.NumberNoiticeUseExceedNumberOfPackage, "number invoice use is exceed number invoice of package");
                        }
                    }
                }
                // Update data
                var company = notificationUseInvoice.MYCOMPANY;
                var taxDepartment = notificationUseInvoice.TAXDEPARTMENT;
                var city = notificationUseInvoice.CITY;

                notificationUseInvoice.COMPANYNAME = company.COMPANYNAME;
                notificationUseInvoice.COMPANYADDRESS = company.ADDRESS;
                notificationUseInvoice.DELEGATE = company.DELEGATE;
                notificationUseInvoice.COMPANYTAXCODE = company.TAXCODE;
                notificationUseInvoice.BANKACCOUNT = company.BANKACCOUNT;
                notificationUseInvoice.COMPANYPHONE = company.TEL1;
                notificationUseInvoice.TAXDEPARTMENTNAME = taxDepartment?.NAME;
                notificationUseInvoice.CITYNAME = city.NAME;

                notificationUseInvoice.NOTIFICATIONUSEINVOICEDETAILs.ForEach(p =>
                {
                    var registerTemplate = p.REGISTERTEMPLATE;
                    var invoiceSample = registerTemplate.INVOICESAMPLE;
                    var invoiceType = invoiceSample.INVOICETYPE;

                    p.INVOICETYPENAME = invoiceType.NAME;
                    p.REGISTERTEMPLATECODE = registerTemplate.CODE;
                    p.INVOICESAMPLENAME = invoiceSample.NAME;
                });
            }
            notificationUseInvoice.STATUS = status.Status;
            notificationUseInvoice.UPDATEDBY = status.ActionUserId;
            notificationUseInvoice.UPDATEDDATE = DateTime.Now;
            return this.notificationUseRepository.Update(notificationUseInvoice) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        private bool CheckValidateNumberOfPackage(List<UseInvoiceDetailInfo> UseInvoiceDetails, bool isUpdate, long noticeId = 0)
        {
            long numberRegisted = 0;
            var listSuffix = UseInvoiceDetails.GroupBy(p => new { p.Suffix })
                .Select(g => new
                {
                    suffix = g.Key.Suffix,
                    numberRegis = g.Sum(x => x.NumberUse)
                });
            foreach (var item in listSuffix)
            {
                if (isUpdate)
                {
                    numberRegisted = this.notificationUseRepository.GetTotalRegisterBySuffixExceptCurrentNotice(item.suffix, noticeId);
                }
                else
                {
                    numberRegisted = this.notificationUseRepository.GetTotalNumberRegisterBySuffix(item.suffix);
                }
                if (numberRegisted + item.numberRegis > this.currentUser.NumberInvoicePackage)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}