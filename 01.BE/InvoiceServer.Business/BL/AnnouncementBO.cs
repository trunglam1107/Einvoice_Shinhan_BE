using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Email;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Annoucement;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace InvoiceServer.Business.BL
{
    public partial class AnnouncementBO : IAnnouncementBO
    {
        #region Fields, Properties

        private readonly EmailConfig emailConfig;
        private readonly Email email;
        private readonly IAnnouncementRepository announcementRepository;
        private readonly IEmailActiveFileAttachRepository emailActiveFileAttachRepository;
        private readonly ISystemSettingRepository systemSettingRepository;
        private readonly IDbTransactionManager transaction;
        private readonly PrintConfig config;
        private readonly DataClassesDataContext _dbSet = new DataClassesDataContext();
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly IInvoiceRepository invoiceRepository;
        private readonly Logger logger = new Logger();
        #endregion

        public AnnouncementBO(IRepositoryFactory repoFactory, PrintConfig printConfig, UserSessionInfo userSessionInfo)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");

            this.announcementRepository = repoFactory.GetRepository<IAnnouncementRepository>();
            this.emailActiveFileAttachRepository = repoFactory.GetRepository<IEmailActiveFileAttachRepository>();
            this.config = printConfig;
            this.email = new Email(repoFactory);
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
        }

        public AnnouncementBO(IRepositoryFactory repoFactory, PrintConfig printConfig, EmailConfig emailConfig, UserSessionInfo userSessionInfo)
            : this(repoFactory, printConfig, userSessionInfo)
        {
            this.emailConfig = emailConfig;
        }

        public IEnumerable<AnnouncementMaster> Filter(ConditionSearchAnnouncement condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var annoucementMasters = this.announcementRepository.FilterAnnouncement(condition).OrderByDescending(x => x.AnnouncementDate).Skip(condition.Skip).Take(condition.Take).ToList();
            return annoucementMasters;
        }

        public long Count(ConditionSearchAnnouncement condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.announcementRepository.FilterAnnouncement(condition).Count();
        }
        public ExportFileInfo PrintView(long id, long companyId, bool isWord)
        {
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(companyId);
            var companyInfo = this.myCompanyRepository.GetById(companyId);
            // truong hop la PGD thi get thong tin cua chi nhanh
            if (companyInfo.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
            {
                companyInfo = this.myCompanyRepository.GetById((long)companyInfo.COMPANYID);
            }
            AnnouncementDownload announcementMasteres = announcementRepository.GetAnnouncementPrintInfo(id, companyInfo);
            AnnouncementDocx docx = new AnnouncementDocx(announcementMasteres, config, systemSettingInfo, false);
            return docx.ExportFileInfo(isWord);
        }

        public ExportFileInfo PrintViewPdf(long id, long companyId)
        {

            ExportFileInfo fileInfo = GetAnnouncementFile(id);
            if (fileInfo == null)
            {
                ANNOUNCEMENT announcement = GetAnnouncement(id);
                if (announcement != null)
                {
                    var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(announcement.COMPANYID);
                    var companyInfo = this.myCompanyRepository.GetById(companyId);
                    // truong hop la PGD thi get thong tin cua chi nhanh
                    if (companyInfo.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
                    {
                        companyInfo = this.myCompanyRepository.GetById((long)companyInfo.COMPANYID);
                    }
                    AnnouncementDownload announcementMasteres = announcementRepository.GetAnnouncementPrintInfo(id, companyInfo);
                    AnnouncementDocx docx = new AnnouncementDocx(announcementMasteres, config, systemSettingInfo, true);
                    bool isWord = false;
                    return docx.ExportFileInfo(isWord);
                }
            }

            return fileInfo;
        }

        private ExportFileInfo GetAnnouncementFile(long announcementId)
        {
            ANNOUNCEMENT announcement = GetAnnouncement(announcementId);
            return FilterAnnouncementFile(announcement);
        }

        private ExportFileInfo FilterAnnouncementFile(ANNOUNCEMENT announcement)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            string fullPathFile = null;
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, announcement.COMPANYID.ToString());
            if (announcement.ANNOUNCEMENTSTATUS == (int)AnnouncementStatus.Released || announcement.ANNOUNCEMENTSTATUS == (int)AnnouncementStatus.Successfull)
            {
                var fileNameSign = "";
                if (announcement.CLIENTSIGN == true)
                {
                    fileNameSign = string.Format("{0}_sign_sign.pdf", announcement.ID);
                }
                else
                {
                    fileNameSign = string.Format("{0}_sign.pdf", announcement.ID);
                }
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignAnnoucement.Release, AssetSignAnnoucement.SignFile, PathUtil.FolderTree(announcement.ANNOUNCEMENTDATE), fileNameSign);
                fileInfo.FullPathFileName = fullPathFile;
                fileInfo.FullPathFileInvoice = Path.Combine(folderInvoiceOfCompany, AssetSignAnnoucement.Contracts, fileNameSign);
            }

            if (!File.Exists(fullPathFile))
            {
                return null;
            }

            fileInfo.FileName = "Announcement.pdf";
            fileInfo.FullPathFileName = fullPathFile;
            return fileInfo;
        }

        public ResultApproved ApproveAnnouncement(ApprovedAnnouncement approveAnnouncement, UserSessionInfo currentUser)
        {
            if (approveAnnouncement == null || approveAnnouncement.ReleaseAnnouncementInfos.Count == 0)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            ResultApproved result = new ResultApproved();
            foreach (var p in approveAnnouncement.ReleaseAnnouncementInfos)
            {
                //check is exist another announcement(thay the/huy) approved
                var announApproved = this.announcementRepository.GetAnnounApprovedByInvoiceId(p.InvoiceId);
                if (announApproved != null && (announApproved.ANNOUNCEMENTTYPE == 2 || announApproved.ANNOUNCEMENTTYPE == 3))
                {
                    result.RowError.Add(new ApprovedRowError(ResultCode.InvoiceExistAnnouncementAdjustmentOrCancel, announApproved.INVOICENO));
                    continue;
                }

                var currentAnnouncement = GetAnnouncement(p.AnnouncementId);
                var company = this.myCompanyRepository.GetById(currentAnnouncement.COMPANYID);
                var invoice = this.invoiceRepository.GetById(currentAnnouncement.INVOICEID);
                var client = invoice?.CLIENT ?? new CLIENT();

                currentAnnouncement.ANNOUNCEMENTSTATUS = (int)AnnouncementStatus.Approved;
                currentAnnouncement.APPROVEDBY = currentUser.Id;
                currentAnnouncement.APPROVEDDATE = DateTime.Now;
                currentAnnouncement.COMPANYNAME = company.COMPANYNAME;
                currentAnnouncement.COMPANYADDRESS = company.ADDRESS;
                currentAnnouncement.COMPANYTEL = company.TEL1;
                currentAnnouncement.COMPANYTAXCODE = company.TAXCODE;
                if (!OtherExtensions.IsCurrentClient(client.CUSTOMERCODE))
                {
                    currentAnnouncement.CLIENTNAME = client.ISORG == false ? client.PERSONCONTACT : client.CUSTOMERNAME;
                    currentAnnouncement.CLIENTADDRESS = client.ADDRESS;
                    currentAnnouncement.CLIENTTEL = client.MOBILE;
                    currentAnnouncement.CLIENTTAXCODE = client.TAXCODE;
                }
                //currentAnnouncement.COMPANYNAME = currentUser.
                this.announcementRepository.Update(currentAnnouncement);
            }
            if (result.RowError.Count > 0)
            {
                result.ErrorCode = ResultCode.ImportFileFormatInvalid;
                result.Message = MsgApiResponse.DataInvalid;
            }

            return result;
        }

        public ResultApproved CancelApproveAnnouncement(ApprovedAnnouncement approveAnnouncement, UserSessionInfo currentUser)
        {
            if (approveAnnouncement == null || approveAnnouncement.ReleaseAnnouncementInfos.Count == 0)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            ResultApproved result = new ResultApproved();
            foreach (var p in approveAnnouncement.ReleaseAnnouncementInfos)
            {
                var currentAnnouncement = GetAnnouncement(p.AnnouncementId);
                currentAnnouncement.ANNOUNCEMENTSTATUS = (int)AnnouncementStatus.New;
                currentAnnouncement.APPROVEDBY = currentUser.Id;
                currentAnnouncement.APPROVEDDATE = DateTime.Now;
                this.announcementRepository.Update(currentAnnouncement);
            }
            if (result.RowError.Count > 0)
            {
                result.ErrorCode = ResultCode.ImportFileFormatInvalid;
                result.Message = MsgApiResponse.DataInvalid;
            }

            return result;
        }

        private ANNOUNCEMENT GetAnnouncement(long id)
        {
            ANNOUNCEMENT announcements = this.announcementRepository.GetByOnlyId(id);
            if (announcements == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This AnnouncementId [{0}] not found in data of client", id));
            }

            return announcements;
        }

        public ResultCode Delete(long id)
        {
            return DeleteAnnouncement(id);
        }

        private ResultCode DeleteAnnouncement(long id)
        {
            ANNOUNCEMENT announcement = GetAnnouncement(id);
            List<int> announcementStatusCanDelete = new List<int>() { (int)AnnouncementStatus.New };
            if (!announcementStatusCanDelete.Contains((announcement.ANNOUNCEMENTSTATUS)))
            {
                throw new BusinessLogicException(ResultCode.AnnouncementReleasedNotDelete, "This Announcement released can not delete");
            }
            return this.announcementRepository.Delete(announcement) ? ResultCode.NoError : ResultCode.UnknownError;
        }
        public bool checkExist(string code, long? id, long company)
        {
            return this.announcementRepository.getMinus(code, id, company);
        }
        public AnnouncementInfo GetAnnouncementInfo(long id, long companyId)
        {
            ANNOUNCEMENT announcements = this.announcementRepository.GetById(id, companyId);
            if (announcements != null)
            {
                return new AnnouncementInfo()
                {
                    Id = announcements.ID,
                    Denominator = announcements.DENOMINATOR,
                };
            }
            return new AnnouncementInfo();
        }

        public AnnouncementsRelease GetAnnouncementOfClient(long id)
        {
            ANNOUNCEMENT announcements = this.announcementRepository.GetByOnlyId(id);
            if (announcements != null)
            {
                return new AnnouncementsRelease()
                {
                    AnnouncementId = announcements.ID,
                    //Denominator = announcements.DENOMINATOR,
                    //Symbol = announcements.SYMBOL,
                    InvoiceId = announcements.INVOICEID,
                    //AnnouncementDate = announcements.ANNOUNCEMENTDATE,
                    ClientSign = announcements.CLIENTSIGN,
                    CompanyId = announcements.COMPANYID,
                };
            }
            return new AnnouncementsRelease();
        }

        public AnnouncementInfo GetAnnouncementByInvoiceId(long invoiceId, int announType, long companyId)
        {
            ANNOUNCEMENT announcements = this.announcementRepository.GetByInvoiceId(invoiceId, announType, companyId);
            if (announcements != null)
            {
                return new AnnouncementInfo()
                {
                    Id = announcements.ID,
                    AnnouncementType = announcements.ANNOUNCEMENTTYPE,
                    AnnouncementStatus = announcements.ANNOUNCEMENTSTATUS,
                    CompanySign = announcements.COMPANYSIGN,
                    ClientSign = announcements.CLIENTSIGN,
                };
            }
            return new AnnouncementInfo();
        }
        public AnnouncementInfo CheckExistAnnoun(long invoiceId, bool invoiceType, int announType)
        {
            return this.announcementRepository.CheckExistAnnoun(invoiceId, invoiceType, announType);
        }
        public bool CheckAnnount(long invoiceId, int type)
        {
            return this.announcementRepository.CheckAnnount(invoiceId, type);
        }
        public ANNOUNCEMENT SaveMD(AnnouncementMD anInfo)
        {
            var md = new ANNOUNCEMENT();
            var getDataInvoice = this.invoiceRepository.GetById(anInfo.InvoiceId);
            if (anInfo.UpdateStatus == AnnouncementStatus.New)
            {
                try
                {
                    md.LEGALGROUNDS = anInfo.LegalGrounds;
                    md.TITLE = anInfo.Title;
                    md.ANNOUNCEMENTDATE = anInfo.AnnouncementDate;

                    md.COMPANYID = anInfo.CompanyId;
                    md.COMPANYNAME = anInfo.CompanyName; 
                    md.COMPANYADDRESS = anInfo.CompanyAddress;
                    md.COMPANYTEL = anInfo.CompanyTel;
                    md.COMPANYTAXCODE = anInfo.CompanyTaxCode;
                    md.COMPANYREPRESENTATIVE = anInfo.CompanyRepresentative;
                    md.COMPANYPOSITION = anInfo.CompanyPosition;
                    md.CLIENTNAME = anInfo.ClientName;
                    md.CLIENTADDRESS = anInfo.ClientAddress;
                    md.CLIENTTEL = anInfo.ClientTel;
                    md.CLIENTTAXCODE = anInfo.ClientTaxCode;
                    md.CLIENTREPRESENTATIVE = anInfo.ClientRepresentative;
                    md.CLIENTPOSITION = anInfo.ClientPosition;
                    md.INVOICEID = anInfo.InvoiceId;
                    md.REGISTERTEMPLATEID = anInfo.RegisterTemplateId;
                    //md.DENOMINATOR = anInfo.Denominator;
                    md.SYMBOL = getDataInvoice.SYMBOL;
                    md.INVOICENO = anInfo.InvoiceNo;
                    md.INVOICEDATE = anInfo.InvoiceDate;
                    md.TOTALAMOUNT = anInfo.TotalAmount;
                    md.SERVICENAME = anInfo.ServiceName;
                    md.REASION = anInfo.Reasion;
                    md.BEFORECONTAIN = anInfo.BeforeContain;
                    md.CHANGECONTAIN = anInfo.ChangeContain;
                    md.ANNOUNCEMENTTYPE = anInfo.AnnouncementType;
                    md.ANNOUNCEMENTSTATUS = anInfo.AnnouncementStatus;
                    md.CREATEDBY = anInfo.CreatedBy;
                    md.MINUTESNO = anInfo.MinutesNo;

                    md.CREATEDDATE = md.UPDATEDDATE = DateTime.Now;
                    md.UPDATEDBY = anInfo.UpdatedBy;
                    md.UPDATEDDATE = md.CREATEDDATE;
                    md.APPROVEDBY = anInfo.ApprovedBy;
                    md.PROCESSBY = anInfo.ProcessBy;
                    _dbSet.ANNOUNCEMENTs.Add(md);
                    _dbSet.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    Logger logger = new Logger();
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            logger.Error("Property: {0} Error: {1}", new Exception($"validationError.PropertyName={validationError.PropertyName} validationError.ErrorMessage={validationError.ErrorMessage}"));
                            Trace.TraceInformation("Property: {0} Error: {1}",
                                                    validationError.PropertyName,
                                                    validationError.ErrorMessage);
                        }
                    }
                    throw;

                }
            }
            return md;
        }

        public ResultCode Update(AnnouncementMD anInfo)
        {

            var an = _dbSet.ANNOUNCEMENTs.Where(x => x.ID == anInfo.id).FirstOrDefault();
            an.BEFORECONTAIN = anInfo.BeforeContain;
            an.CHANGECONTAIN = anInfo.ChangeContain;
            an.REASION = anInfo.Reasion;
            an.UPDATEDDATE = DateTime.Now;
            an.UPDATEDBY = anInfo.UpdatedBy;
            an.SERVICENAME = anInfo.ServiceName;
            an.COMPANYREPRESENTATIVE = anInfo.CompanyRepresentative;
            an.COMPANYPOSITION = anInfo.CompanyPosition;
            an.CLIENTPOSITION = anInfo.ClientPosition;
            an.CLIENTREPRESENTATIVE = anInfo.ClientRepresentative;
            an.LEGALGROUNDS = anInfo.LegalGrounds;
            an.MINUTESNO = anInfo.MinutesNo;
            _dbSet.SaveChanges();

            return ResultCode.NoError;
        }

        public ResultCode UpdateAnnounceAfterInvoiceCancel(AnnouncementMD anInfo)
        {
            var an = _dbSet.ANNOUNCEMENTs.Where(x => x.ID == anInfo.id).FirstOrDefault();
            an.ANNOUNCEMENTSTATUS = anInfo.AnnouncementStatus;
            _dbSet.SaveChanges();
            return ResultCode.NoError;
        }

        public string UpdateAnnouncementStatus(AnnouncementMD anInfo, UserSessionInfo userSessionInfo)
        {
            //check is exist another announcement(thay the/huy) approved, chi check khi approved, cancel approved k check
            if (anInfo.AnnouncementStatus == (int)AnnouncementStatus.Approved)
            {
                var announApproved = this.announcementRepository.GetAnnounApprovedByInvoiceId(anInfo.InvoiceId);
                if (announApproved != null && (announApproved.ANNOUNCEMENTTYPE == 2 || announApproved.ANNOUNCEMENTTYPE == 3))
                {
                    throw new BusinessLogicException(ResultCode.InvoiceExistAnnouncementAdjustmentOrCancel, MsgApiResponse.DataInvalid);
                }
            }

            var an = _dbSet.ANNOUNCEMENTs.Where(x => x.ID == anInfo.id).FirstOrDefault();
            var company = this.myCompanyRepository.GetById(an.COMPANYID);
            if (company.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
            {
                company = this.myCompanyRepository.GetById(company.MYCOMPANY2.COMPANYSID);
            }
            var invoice = this.invoiceRepository.GetById(an.INVOICEID);
            var client = invoice?.CLIENT ?? new CLIENT();

            an.ANNOUNCEMENTSTATUS = anInfo.AnnouncementStatus;
            an.APPROVEDBY = anInfo.ApprovedBy;
            an.APPROVEDDATE = DateTime.Now;

            if (anInfo.AnnouncementStatus == (int)AnnouncementStatus.Approved)
            {
                an.COMPANYNAME = company.COMPANYNAME;
                an.COMPANYADDRESS = company.ADDRESS;
                an.COMPANYTEL = company.TEL1;
                an.COMPANYTAXCODE = company.TAXCODE;
                if (!OtherExtensions.IsCurrentClient(client.CUSTOMERCODE))
                {
                    an.CLIENTNAME = client.ISORG == false ? client.PERSONCONTACT : client.CUSTOMERNAME;
                    an.CLIENTADDRESS = client.ADDRESS;
                    an.CLIENTTEL = client.MOBILE;
                    an.CLIENTTAXCODE = client.TAXCODE;
                }
            }

            _dbSet.SaveChanges();

            return "OK";
        }

        private string Choose(int annStatus, string value1, string value2)
        {
            if (annStatus == (int)AnnouncementStatus.New)
                return value1;
            return value2;
        }
        public AnnouncementMD LoadList(long id, int actiontype, bool isAnounId, UserSessionInfo userSessionInfo)
        {
            IQueryable<AnnouncementMD> an = LoadListAn();

            if (isAnounId)
            {
                an = an.Where(p => p.id == id);
            }
            else
            {
                an = an.Where(p => p.InvoiceId == id && p.AnnouncementType == actiontype);
                // Note: When searching the announcement with the invoiceId, there must be the AnnouncementType of record attached
            }

            var announ = an.FirstOrDefault();
            announ.CompanyName = Choose(announ.AnnouncementStatus, announ.CompanyNameMyCompany, announ.CompanyNameAnnouncement);
            announ.CompanyAddress = Choose(announ.AnnouncementStatus, announ.CompanyAddressMyCompany, announ.CompanyAddressAnnouncement);
            announ.CompanyTel = Choose(announ.AnnouncementStatus, announ.CompanyTelMyCompany, announ.CompanyTelAnnouncement);
            announ.CompanyTaxCode = Choose(announ.AnnouncementStatus, announ.CompanyTaxCodeMyCompany, announ.CompanyTaxCodeAnnouncement);

            announ.DateCompanySign = announ.CompanySignDate.HasValue ? announ.CompanySignDate.Value.ToString("dd/MM/yyy") : String.Empty;
            announ.DateClientSign = announ.ClientSignDate.HasValue ? announ.ClientSignDate.Value.ToString("dd/MM/yyy") : String.Empty;
            var signDetailClient = this._dbSet.SIGNDETAILs.Where(x => announ.ClientSign == true && (x.ANNOUNCEMENTID ?? 0) == announ.id && x.ISCLIENTSIGN == true && x.TYPESIGN == (int)SignDetailTypeSign.Announcement)
                .OrderByDescending(x => x.CREATEDDATE)
                .FirstOrDefault();
            announ.ClientSignature = signDetailClient?.NAME;

            var signDetail = this._dbSet.SIGNDETAILs.Where(x => announ.CompanySign == true && (x.ANNOUNCEMENTID ?? 0) == announ.id && !(x.ISCLIENTSIGN ?? false) && x.TYPESIGN == (int)SignDetailTypeSign.Announcement)
                    .OrderByDescending(x => x.CREATEDDATE)
                    .FirstOrDefault();
            announ.CompanySignature = signDetail?.NAME ?? Choose(announ.AnnouncementStatus, announ.CompanyNameMyCompany, announ.CompanyNameAnnouncement);

            // truong hop la phong giao dich
            if (announ.AnnouncementStatus == (int)AnnouncementStatus.New)
            {
                var company = this.myCompanyRepository.GetById(announ.CompanyId);
                if (company.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
                {
                    announ.CompanyName = company.MYCOMPANY2.COMPANYNAME;
                    announ.CompanyAddress = company.MYCOMPANY2.ADDRESS;
                    announ.CompanyTel = company.MYCOMPANY2.TEL1;
                    announ.CompanyRepresentative = company.MYCOMPANY2.DELEGATE;
                    announ.CompanyPosition = company.MYCOMPANY2.POSITION;
                }
            }


            var systemSetting = this._dbSet.SYSTEMSETTINGs.FirstOrDefault();
            var systemSettingInfo = new SystemSettingInfo(systemSetting);
            if (systemSetting != null)
            {
                systemSettingInfo.EmailReceiveWarning = "";
                systemSettingInfo.IsWarningMinimum = false;
                systemSettingInfo.NumberMinimum = 0;
                systemSettingInfo.ServerSign = false;
                systemSettingInfo.SignAfterApprove = false;
                systemSettingInfo.StepToCreateInvoiceNo = 0;
            }
            announ.SystemSetting = systemSettingInfo;

            return announ;
        }

        public IQueryable<AnnouncementMD> LoadListAn()
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            IQueryable<AnnouncementMD> an =
                from ANNOUNCEMENT in _dbSet.ANNOUNCEMENTs
                join releaseAnn in _dbSet.RELEASEANNOUNCEMENTDETAILs.Where(p => p.SIGNED == true)
                   on ANNOUNCEMENT.ID equals releaseAnn.ANNOUNCEMENTID into releaseDetail
                join invoice in _dbSet.INVOICEs on ANNOUNCEMENT.INVOICEID equals invoice.ID
                join client in _dbSet.CLIENTs on invoice.CLIENTID equals client.ID
                from releaseAnnounDetail in releaseDetail.DefaultIfEmpty()
                join company in _dbSet.MYCOMPANies on ANNOUNCEMENT.COMPANYID equals company.COMPANYSID into companyDetail
                from company in companyDetail.DefaultIfEmpty()
                join currency in _dbSet.CURRENCies on invoice.CURRENCYID equals currency.ID into currencyTemp
                from currency in currencyTemp.DefaultIfEmpty()
                let announcementNew = ANNOUNCEMENT.ANNOUNCEMENTSTATUS == (int)AnnouncementStatus.New
                let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
                select new AnnouncementMD
                {
                    LegalGrounds = ANNOUNCEMENT.LEGALGROUNDS,
                    Title = ANNOUNCEMENT.TITLE,
                    AnnouncementDate = ANNOUNCEMENT.ANNOUNCEMENTDATE,
                    CompanyId = ANNOUNCEMENT.COMPANYID,
                    //CompanyName = ANNOUNCEMENT.ANNOUNCEMENTSTATUS == (int)AnnouncementStatus.New ? company.COMPANYNAME : ANNOUNCEMENT.COMPANYNAME,
                    CompanyNameAnnouncement = ANNOUNCEMENT.COMPANYNAME,
                    CompanyNameMyCompany = company.COMPANYNAME,
                    BranchId = company.BRANCHID,
                    //CompanyAddress = ANNOUNCEMENT.ANNOUNCEMENTSTATUS == (int)AnnouncementStatus.New ? company.ADDRESS : ANNOUNCEMENT.COMPANYADDRESS,
                    CompanyAddressAnnouncement = ANNOUNCEMENT.COMPANYADDRESS,
                    CompanyAddressMyCompany = company.ADDRESS,
                    //CompanyTel = ANNOUNCEMENT.ANNOUNCEMENTSTATUS == (int)AnnouncementStatus.New ? company.TEL1 : ANNOUNCEMENT.COMPANYTEL,
                    CompanyTelAnnouncement = ANNOUNCEMENT.COMPANYTEL,
                    CompanyTelMyCompany = company.TEL1,
                    //CompanyTaxCode = ANNOUNCEMENT.ANNOUNCEMENTSTATUS == (int)AnnouncementStatus.New ? company.TAXCODE : ANNOUNCEMENT.COMPANYTAXCODE,
                    CompanyTaxCodeAnnouncement = ANNOUNCEMENT.COMPANYTAXCODE,
                    CompanyTaxCodeMyCompany = company.TAXCODE,
                    CompanyRepresentative = ANNOUNCEMENT.COMPANYREPRESENTATIVE,
                    CompanyPosition = ANNOUNCEMENT.COMPANYPOSITION,
                    ClientName = announcementNew ? (isOrgCurrentClient == false ? (notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT) : (notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME)) : ANNOUNCEMENT.CLIENTNAME,
                    ClientAddress = announcementNew ? (notCurrentClient ? client.ADDRESS : invoice.CUSTOMERADDRESS) : ANNOUNCEMENT.CLIENTADDRESS,
                    ClientTel = announcementNew ? client.MOBILE : ANNOUNCEMENT.CLIENTTEL,
                    ClientTaxCode = announcementNew ? (notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE) : ANNOUNCEMENT.CLIENTTAXCODE,
                    ClientRepresentative = ANNOUNCEMENT.CLIENTREPRESENTATIVE,
                    ClientPosition = ANNOUNCEMENT.CLIENTPOSITION,
                    InvoiceId = ANNOUNCEMENT.INVOICEID,
                    RegisterTemplateId = ANNOUNCEMENT.REGISTERTEMPLATEID,
                    Denominator = ANNOUNCEMENT.ID.ToString(),
                    Symbol = ANNOUNCEMENT.SYMBOL,
                    InvoiceNo = ANNOUNCEMENT.INVOICENO,
                    InvoiceDate = invoice.RELEASEDDATE ?? ANNOUNCEMENT.INVOICEDATE,//DateTime.ParseExact(annoucement.InvoiceDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    TotalAmount = ANNOUNCEMENT.TOTALAMOUNT,
                    ServiceName = ANNOUNCEMENT.SERVICENAME,
                    Reasion = ANNOUNCEMENT.REASION,
                    BeforeContain = ANNOUNCEMENT.BEFORECONTAIN,
                    ChangeContain = ANNOUNCEMENT.CHANGECONTAIN,
                    AnnouncementType = ANNOUNCEMENT.ANNOUNCEMENTTYPE,
                    AnnouncementStatus = ANNOUNCEMENT.ANNOUNCEMENTSTATUS,
                    CreatedBy = ANNOUNCEMENT.CREATEDBY,
                    //CreatedDate = annoucement.CreatedDate.ToString("yyyy-MM-dd"),
                    UpdatedBy = ANNOUNCEMENT.UPDATEDBY,
                    ApprovedBy = ANNOUNCEMENT.APPROVEDBY,
                    ProcessBy = ANNOUNCEMENT.PROCESSBY,
                    //ProcessDate = annoucement.CreatedDate.ToString("yyyy-MM-dd"),
                    id = ANNOUNCEMENT.ID,
                    VerificationCode = releaseAnnounDetail.VERIFICATIONCODE,
                    CompanySign = ANNOUNCEMENT.COMPANYSIGN,
                    CompanySignDate = ANNOUNCEMENT.COMPANYSIGNDATE,
                    ClientSign = ANNOUNCEMENT.CLIENTSIGN,
                    ClientSignDate = ANNOUNCEMENT.CLIENTSIGNDATE,
                    MinutesNo = ANNOUNCEMENT.MINUTESNO,
                    EmailClient = client.EMAIL,
                    CurrencyCode = currency.CODE,
                };
            return an;
        }

        public string getInfoCertificate(string SerialNumber)
        {
            // co mot ham giong o InvoiceBO.cs
            string NameCA = String.Empty;
            var certStore = new X509Store("ShinhanBank", StoreLocation.CurrentUser);//StoreName.My
            certStore.Open(OpenFlags.ReadOnly);
            X509CertificateCollection certCollection = certStore.Certificates.Find(X509FindType.FindBySerialNumber, SerialNumber, false);
            foreach (X509Certificate2 x509Certificate2 in certCollection.OfType<X509Certificate2>())
            {
                if (x509Certificate2.SerialNumber == SerialNumber)
                {
                    NameCA = x509Certificate2.GetNameInfo(X509NameType.SimpleName, true);
                }
            }
            certStore.Close();
            return NameCA;
        }

        public InvoicePrintInfo GetInvoiceData_Replace(long id)
        {
            var getInvoice = new InvoicePrintInfo();

            getInvoice = this.invoiceRepository.SearchInvoiceReplace_Info(id)[0];

            return getInvoice;

        }
        public InvoicePrintInfo GetInvoiceData(long id)
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var iv = from invoice in _dbSet.INVOICEs
                     join company in this._dbSet.MYCOMPANies on invoice.COMPANYID equals company.COMPANYSID
                     join template in this._dbSet.REGISTERTEMPLATES on invoice.REGISTERTEMPLATEID equals template.ID
                     join client in this._dbSet.CLIENTs on invoice.CLIENTID equals client.ID
                     join annou in _dbSet.ANNOUNCEMENTs on invoice.ID equals annou.INVOICEID into leftannou
                     from a in leftannou.DefaultIfEmpty()
                     join currency in this._dbSet.CURRENCies on invoice.CURRENCYID equals currency.ID into currencyTemp
                     from currency in currencyTemp.DefaultIfEmpty()
                     where invoice.ID == id && invoice.INVOICESTATUS == (int)InvoiceStatus.Released //&& a==null
                     let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                     let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
                     select new InvoicePrintInfo()
                     {

                         CustomerName = isOrgCurrentClient == false ? (notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT) : (notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME),
                         CompanyName = company.COMPANYNAME,
                         CustomerCompanyName = notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                         CustomerAddress = notCurrentClient ? client.ADDRESS : invoice.CUSTOMERADDRESS,
                         Tel = client.MOBILE,
                         CustomerTaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                         InvoiceCode = template.CODE,
                         Id = invoice.ID,
                         InvoiceNo = invoice.NO,
                         Total = invoice.TOTAL,
                         Sum = invoice.SUM,
                         Delegate = client.DELEGATE,
                         DateRelease = invoice.RELEASEDDATE,
                         DateClientSign = invoice.CREATEDDATE,
                         PersonContact = isOrgCurrentClient == false ? null : (notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT),
                         CurrencyCode = currency.CODE,
                         Symbol = invoice.SYMBOL,
                     };
            return iv.FirstOrDefault();


        }

        public ResultCode Remove(long announcementID, long updatedBy)
        {
            var an = _dbSet.ANNOUNCEMENTs.Where(x => x.ID == announcementID).FirstOrDefault();
            an.ANNOUNCEMENTTYPE = (int)EnumAnnouncementActionType.CANCEL;
            an.ANNOUNCEMENTSTATUS = (int)AnnouncementStatus.Successfull;
            an.UPDATEDDATE = DateTime.Now;
            an.UPDATEDBY = updatedBy;
            _dbSet.SaveChanges();
            return ResultCode.NoError;
        }

        public ResultCode SearchCode(string ms, string kh, DateTime date, string num)
        {
            throw new NotImplementedException();
        }

        public string IsExit(long id, int announcementType)
        {
            if (_dbSet.ANNOUNCEMENTs.Where(x => x.INVOICEID == id && x.ANNOUNCEMENTTYPE == announcementType).Count() > 0)
                return "OK";
            return "";
        }

        public string IsExistAll(long id)
        {
            if (_dbSet.ANNOUNCEMENTs.Where(x => x.INVOICEID == id && (x.ANNOUNCEMENTTYPE == (int)EnumAnnouncementActionType.CANCEL
                                                                      //|| x.ANNOUNCEMENTTYPE == (int)EnumAnnouncementActionType.ADJUSTMENT
                                                                      //|| x.ANNOUNCEMENTTYPE == (int)EnumAnnouncementActionType.SUBSTITUTE
                                                                      )).Count() > 0)
                return "OK";
            return "";
        }
        public SendEmailVerificationCodeAnnoun GetVerificationCode(AnnouncementVerificationCode announcementVerificationCode)
        {
            if (announcementVerificationCode == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            SendEmailVerificationCodeAnnoun emailVerificationCodeInfo;
            try
            {
                emailVerificationCodeInfo = this.announcementRepository.GetVerificationCodeInfomation(announcementVerificationCode);
                if (!emailVerificationCodeInfo.IsOrg)
                {
                    emailVerificationCodeInfo.CustomerName = emailVerificationCodeInfo.PersonContact;
                }
                emailVerificationCodeInfo.EmailTo = announcementVerificationCode.Email;
                emailVerificationCodeInfo.NumberFormat = announcementVerificationCode.NumberFormat;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return emailVerificationCodeInfo;
        }
        public long CreateVerificationCode(AnnouncementVerificationCode announcementVerificationCode)
        {
            if (announcementVerificationCode == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long result = 0;
            try
            {
                transaction.BeginTransaction();
                SendEmailVerificationCodeAnnoun emailVerificationCodeInfo = this.announcementRepository.GetVerificationCodeInfomation(announcementVerificationCode);
                if (!emailVerificationCodeInfo.IsOrg)
                {
                    emailVerificationCodeInfo.CustomerName = emailVerificationCodeInfo.PersonContact;
                }
                emailVerificationCodeInfo.EmailTo = announcementVerificationCode.Email;
                emailVerificationCodeInfo.NumberFormat = announcementVerificationCode.NumberFormat;
                result = CreateEmailActive(announcementVerificationCode.CompanyId, emailVerificationCodeInfo);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return result;
        }

        public long callCreateEmailActive(long companyId, SendEmailVerificationCodeAnnoun receiverInfo)
        {
            long result = 0;
            try
            {
                transaction.BeginTransaction();
                result = CreateEmailActive(companyId, receiverInfo);
                transaction.Commit();
            }
            catch(Exception ex)
            {

                result = 0;
                transaction.Rollback();
                throw;
            }
            return result;
        }

        public long CreateEmailActive(long companyId, SendEmailVerificationCodeAnnoun receiverInfo)
        {
            logger.Error("Create Email Active For Announce ", new Exception(""));
            int emailType = 0;
            if (receiverInfo.AnnouncementType == 1)
            {
                emailType = (int)SendEmailType.SendVerificationCodeAnnouncementAdjust;
            }
            if (receiverInfo.AnnouncementType == 2)
            {
                emailType = (int)SendEmailType.SendVerificationCodeAnnouncementReplace;
            }
            if (receiverInfo.AnnouncementType == 3)
            {
                emailType = (int)SendEmailType.SendVerificationCodeAnnouncementCancel;
            }

            EmailInfo emailInfo = EmailTemplate.GetEmail(this.emailConfig, emailType, receiverInfo);
            emailInfo.RefIds.Add(receiverInfo.AnnouncementId);   // Task #30077
            emailInfo.CompanyId = receiverInfo.CompanyId;
            long emailActiveId = this.email.CreateEmailActive(emailInfo, (int)TypeEmailActive.AnnouncementEmail, companyId);
            InsertFileAttachByAnnouncement(receiverInfo.AnnouncementId, companyId, emailActiveId);
            return emailActiveId;
        }
        private void InsertFileAttachByAnnouncement(long AnnouncementId, long companyId, long emailActiveId)
        {
            ExportFileInfo fileInfo = GetFileAnnouncementSign(AnnouncementId, companyId);
            if (fileInfo == null)
            {
                throw new BusinessLogicException(ResultCode.FileNotFound, MsgApiResponse.FileNotFound);
            }
            ANNOUNCEMENT announcement = GetAnnouncement(AnnouncementId);
            InserEmailFileAttachFile(fileInfo, emailActiveId, announcement);
        }

        private ExportFileInfo GetFileAnnouncementSign(long AnnouncementId, long companyId)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            ANNOUNCEMENT announcement = GetAnnouncement(AnnouncementId);
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, companyId.ToString());
            string fileNameSign = "";
            if (announcement.CLIENTSIGN == true)
            {
                fileNameSign = string.Format("{0}_sign_sign.pdf", announcement.ID);
            }
            else
            {
                fileNameSign = string.Format("{0}_sign.pdf", announcement.ID);
            }
            fileInfo.FullPathFileName = Path.Combine(folderInvoiceOfCompany, AssetSignAnnoucement.Release, AssetSignAnnoucement.SignFile, PathUtil.FolderTree(announcement.ANNOUNCEMENTDATE), fileNameSign);
            fileInfo.FullPathFileInvoice = Path.Combine(folderInvoiceOfCompany, AssetSignAnnoucement.Release, AssetSignAnnoucement.SignFile, PathUtil.FolderTree(announcement.ANNOUNCEMENTDATE), fileNameSign);
            fileInfo.FileName = fileNameSign;
            return fileInfo;
        }

        private void InserEmailFileAttachFile(ExportFileInfo fileInfo, long emailActiveId, ANNOUNCEMENT announcement = null)
        {
            if (fileInfo == null || !File.Exists(fileInfo.FullPathFileName.ConvertToString()))
            {
                return;
            }
            //ID_Số biên bản_(Nếu không sử dụng số tăng tự động thì sử dụng BBDCHD/BBTTHD/BBHHD)_InvoiceNo
            if (announcement != null)
            {
                string[] formatFile = fileInfo.FileName.Split('.');
                if (announcement.MINUTESNO.IsNotNullOrEmpty() && announcement.MINUTESNO.Contains("BBDCHD") || fileInfo.FileName.Contains("BBTTHD") || fileInfo.FileName.Contains("BBHHD"))
                {
                    fileInfo.FileName = string.Format("{0}_{1}_{2}.{3}", announcement.INVOICEID, announcement.MINUTESNO, announcement.INVOICENO, formatFile[1]).Replace("/", "");
                }
                else
                {
                    string type = "";
                    switch (announcement.ANNOUNCEMENTTYPE)
                    {
                        case 1:
                            type = "BBDCHD";
                            break;
                        case 2:
                            type = "BBTTHD";
                            break;
                        case 3:
                            type = "BBHHD";
                            break;
                        default:
                            break;
                    }
                    fileInfo.FileName = string.Format("{0}_{1}_{2}_{3}.{4}", announcement.INVOICEID, announcement.MINUTESNO, type, announcement.INVOICENO, formatFile[1]).Replace("/", "");
                }
            }
            EMAILACTIVEFILEATTACH emailactiveFileAttach = new EMAILACTIVEFILEATTACH();
            emailactiveFileAttach.CopyData(fileInfo);
            emailactiveFileAttach.EMAILACTIVEID = emailActiveId;
            this.emailActiveFileAttachRepository.Insert(emailactiveFileAttach);
        }

        public ExportFileInfo ExportExcel(ConditionSearchAnnouncement condition)
        {
            var dataRport = new AnnouncementExport(condition.CurrentUser?.Company);
            dataRport.Items = announcementRepository.FilterAnnouncement(condition).OrderByDescending(x => x.AnnouncementDate).ToList();
            ReportInvoiceMintuesView excel = new ReportInvoiceMintuesView(dataRport, config, condition.Language);
            return excel.ExportFile();
        }
    }
}