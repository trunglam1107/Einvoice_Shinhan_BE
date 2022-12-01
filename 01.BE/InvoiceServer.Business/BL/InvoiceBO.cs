using Dapper;
using HiQPdf;
using InvoiceServer.Business.Cache;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.DAO.Interface;
using InvoiceServer.Business.Email;
using InvoiceServer.Business.ExportData;
using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.ExportXml;
using InvoiceServer.Business.Helper;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common.Utils;
using InvoiceServer.Data.DBAccessor;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace InvoiceServer.Business.BL
{
    public partial class InvoiceBO : IInvoiceBO
    {
        #region Fields, Properties
        protected readonly static object lockObject = new object();
        protected readonly static object lockImport = new object();
        private readonly IInvoiceRepository invoiceRepository;
        private readonly IInvoiceDetailRepository invoiceDetailRepository;
        private readonly IClientRepository clientRepository;
        private readonly ITaxRepository taxRepository;
        private readonly IProductRepository productRepository;
        private readonly IDbTransactionManager transaction;
        private readonly INotificationUseInvoiceDetailRepository noticeInvoiceDetailRepository;
        private readonly IRegisterTemplatesRepository registerTemplates;
        private readonly PrintConfig config;
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly ITypePaymentRepository typePaymentRepository;
        private readonly IQueueCreateFileInvoiceRepository queueRepository;
        private readonly EmailConfig emailConfig;
        private readonly Email email;
        private readonly Account account;
        private readonly IInvoiceTemplateRepository invoiceTemplateRepository;
        private readonly IReleaseInvoiceDetaiRepository releaseInvoiceDetaiRepository;
        private readonly IEmailActiveFileAttachRepository emailActiveFileAttachRepository;
        private readonly IReportCancellingDetailRepository reportCancellingDetailRepository;
        private readonly ISystemSettingRepository systemSettingRepository;
        private readonly IReleaseInvoiceBO releaseInvoiceBO;
        private readonly IInvoiceSampleRepository invoiceSample;
        private readonly IUnitListRepository unitListRepository;
        private InvoicePrintInfo invoiceInfo;
        private readonly UserSessionInfo currentUser;
        private readonly IAnnouncementRepository announcementRepository;
        private readonly IInvoiceStatisticalRepository invoiceStatisticalRepository;
        private readonly ICurrencyRepository currencyRepository;
        private readonly IInvoiceGiftRepository invoiceGiftRepository;
        private readonly IReleaseInvoiceDetaiRepository releseInvoiceDetailRepository;
        private readonly Logger logger = new Logger();
        private readonly IImportFileDAO importFileDAO;
        private Func<string, string> GetConfig = key => WebConfigurationManager.AppSettings[key];
        private readonly IHistoryReportGeneralRepository historyReportGeneralRepository;
        private readonly IHistoryReportGeneralBO historyReportGeneralBO;
        private readonly IEmailActiveBO emailActiveBO;
        private readonly IQuarztJobBO quarztJobBO;
        private readonly IDeclarationRepository declarationRepository;
        private readonly IQuarztJobRepository quarztRespository;
        #endregion

        #region Contructor


        public InvoiceBO(IRepositoryFactory repoFactory, PrintConfig config)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.invoiceDetailRepository = repoFactory.GetRepository<IInvoiceDetailRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.productRepository = repoFactory.GetRepository<IProductRepository>();
            this.clientRepository = repoFactory.GetRepository<IClientRepository>();
            this.noticeInvoiceDetailRepository = repoFactory.GetRepository<INotificationUseInvoiceDetailRepository>();
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.registerTemplates = repoFactory.GetRepository<IRegisterTemplatesRepository>();
            this.taxRepository = repoFactory.GetRepository<ITaxRepository>();
            this.typePaymentRepository = repoFactory.GetRepository<ITypePaymentRepository>();
            this.queueRepository = repoFactory.GetRepository<IQueueCreateFileInvoiceRepository>();
            this.email = new Email(repoFactory);
            this.invoiceTemplateRepository = repoFactory.GetRepository<IInvoiceTemplateRepository>();
            this.releaseInvoiceDetaiRepository = repoFactory.GetRepository<IReleaseInvoiceDetaiRepository>();
            this.emailActiveFileAttachRepository = repoFactory.GetRepository<IEmailActiveFileAttachRepository>();
            this.reportCancellingDetailRepository = repoFactory.GetRepository<IReportCancellingDetailRepository>();
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
            this.invoiceSample = repoFactory.GetRepository<IInvoiceSampleRepository>();
            this.unitListRepository = repoFactory.GetRepository<IUnitListRepository>();
            this.invoiceInfo = new InvoicePrintInfo();
            this.announcementRepository = repoFactory.GetRepository<IAnnouncementRepository>();
            this.account = new Account(repoFactory);
            this.config = config;
            this.invoiceStatisticalRepository = repoFactory.GetRepository<IInvoiceStatisticalRepository>();
            this.currencyRepository = repoFactory.GetRepository<ICurrencyRepository>();
            this.invoiceGiftRepository = repoFactory.GetRepository<IInvoiceGiftRepository>();
            this.releseInvoiceDetailRepository = repoFactory.GetRepository<IReleaseInvoiceDetaiRepository>();
            this.importFileDAO = repoFactory.GetRepository<IImportFileDAO>(repoFactory, this.currentUser);
            this.historyReportGeneralRepository = repoFactory.GetRepository<IHistoryReportGeneralRepository>();
            this.historyReportGeneralBO = new HistoryReportGeneralBO(repoFactory);
            this.emailActiveBO = new EmailActiveBO(repoFactory, config);
            this.quarztJobBO = new QuarztJobBO(repoFactory);
            this.declarationRepository = repoFactory.GetRepository<IDeclarationRepository>();
            this.quarztRespository = repoFactory.GetRepository<IQuarztJobRepository>();
        }

        public InvoiceBO(IRepositoryFactory repoFactory, PrintConfig config, EmailConfig emailConfig)
            : this(repoFactory, config)
        {
            this.emailConfig = emailConfig;
        }

        public InvoiceBO(IRepositoryFactory repoFactory, PrintConfig config, EmailConfig emailConfig, UserSessionInfo userSessionInfo)
            : this(repoFactory, config, emailConfig)
        {
            this.currentUser = userSessionInfo;
            this.releaseInvoiceBO = new ReleaseInvoiceBO(repoFactory, config, userSessionInfo);
            this.importFileDAO = repoFactory.GetRepository<IImportFileDAO>(repoFactory, this.currentUser);
        }

        public InvoiceBO(IRepositoryFactory repoFactory, PrintConfig config, UserSessionInfo userSessionInfo)
            : this(repoFactory, config)
        {
            this.currentUser = userSessionInfo;
        }
        #endregion

        #region Methods

        public IEnumerable<InvoiceMaster> Filter(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceMasters = this.invoiceRepository.FilterInvoice(condition);

            var systemSetting = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            foreach (var invoiceMaster in invoiceMasters)
            {
                FormatBySystemSetting(invoiceMaster, systemSetting);
            }

            return invoiceMasters;
        }
        public IEnumerable<InvoiceMaster> FilterSP(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceMasters = this.invoiceRepository.FilterInvoiceSP(condition);

            var systemSetting = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            foreach (var invoiceMaster in invoiceMasters)
            {
                FormatBySystemSetting(invoiceMaster, systemSetting);
            }

            return invoiceMasters;
        }

        public IEnumerable<InvoiceMaster> FilterSP_Replace(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceMasters = this.invoiceRepository.FilterInvoiceSP_Replace(condition);

            var systemSetting = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            foreach (var invoiceMaster in invoiceMasters)
            {
                FormatBySystemSetting(invoiceMaster, systemSetting);
            }

            return invoiceMasters;
        }
        public IEnumerable<InvoiceMaster> FilterAnnoun(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceMasters = this.invoiceRepository.FilterInvoiceAnnoun(condition).OrderByDescending(p => p.Updated).Skip(condition.Skip).Take(condition.Take).ToList();
            if (condition.InvoiceStatus.Count == 0 || condition.ColumnOrder != "Id")
                invoiceMasters = invoiceMasters.AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
            return invoiceMasters;
        }

        public IEnumerable<ReleaseInvoiceInfo> GetListInvoiceNo(List<ReleaseInvoiceInfo> releaseInvoiceInfo)
        {
            if (releaseInvoiceInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoices = this.invoiceRepository.GetListInvoiceNo(releaseInvoiceInfo).ToList();
            return invoices;
        }

        public IEnumerable<ReleaseInvoiceInfo> GetListInvoiceApproved(long branch)
        {
            if (branch != 0)
            {
                return this.invoiceRepository.GetInvoiceApprovedByBranch(branch);

            }
            else
            {
                return this.invoiceRepository.GetAllInvoiceApproved();
            }
        }
        public IQueryable<INVOICE> GetListInvoiceCreatedFileShinhan(long companyId, bool isOrg)
        {
            return this.invoiceRepository.GetAllInvoiceCreatedFileShinhan(companyId, isOrg);
        }

        public List<InvoicePrintModel> GetInvoicePrintCreateFile(long companyid)
        {
            return this.invoiceRepository.GetInvoicePrintCreateFile(companyid);
        }
        public List<InvoicePrintModel> GetInvoicePrintPersonal(long companyid)
        {
            return this.invoiceRepository.GetInvoicePrintPersonal(companyid);
        }
        public void UpdateStatusAfterSign(List<InvoicePrintModel> invoiceModels)
        {
            //bool result = UpdateStatusAfterSign(invoiceModels, this.config.PathInvoiceFile);
            bool result = UpdateAfterSign(invoiceModels);
            if (result)
            {
                logger.Error("UpdateStatusAfterSign", new Exception("Sign success!"));
            }
        }
        private bool UpdateAfterSign(List<InvoicePrintModel> invoiceModels)
        {
            logger.Error("Start UpdateAfterSign: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), new Exception("VanSynthesis"));
            try
            {
                var take = (invoiceModels.Count() / 6) + 1;
                var skip = 0;
                string updateDate = DateTime.Now.ToString("dd/MM/yyyy");
                for (int i = 1; i <= 6; i++)
                {
                    var listProcess = new List<decimal>(invoiceModels.Skip(skip).Take(take).Select(p => p.ID));
                    if (listProcess.Count > 0)
                    {
                        string listId = string.Join(",", listProcess);
                        string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
                        string sqlResult = @"UPDATE INVOICE SET INVOICESTATUS = 4, UPDATEDDATE = TO_DATE('{date}','dd/mm/yyyy') WHERE ID IN({ids})";
                        sqlResult = sqlResult.Replace("{date}", updateDate).Replace("{ids}", listId);
                        using (var connection = new OracleConnection(connectionString))
                        {
                            connection.Open();
                            var result = connection.Execute(sqlResult);
                            connection.Close();
                        }
                        skip = take * i;
                    }

                }
                logger.Error("Done UpdateAfterSign: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), new Exception(""));
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateAfterSign", ex);
                return false;
            }

        }
        private bool UpdateStatusAfterSign(List<InvoicePrintModel> invoiceModels, string pathSign)
        {
            var lstInv = new List<INVOICE>();
            string pathFilePdf = String.Empty;
            string pathXml = String.Empty;
            foreach (var item in invoiceModels)
            {
                pathFilePdf = Path.Combine(pathSign, item.COMPANYID.ToString(), "Releases", "Sign", item.RELEASEDDATE.Value.ToString("yyyy"), item.RELEASEDDATE.Value.ToString("MM"), item.RELEASEDDATE.Value.ToString("dd"), item.ID.ToString() + "_sign.pdf");
                pathXml = pathFilePdf.Replace(".pdf", ".xml");
                if (File.Exists(pathFilePdf) && File.Exists(pathXml))
                {
                    var inv = this.invoiceRepository.GetById(Convert.ToInt64(item.ID));
                    inv.INVOICESTATUS = (int)InvoiceStatus.Released;
                    inv.UPDATEDDATE = DateTime.Now;
                    lstInv.Add(inv);
                }

            }
            logger.Error("Commit status: " + lstInv.Count(), new Exception("UpdateStatusAfterSign"));
            //using (var ct = new Context)
            if (lstInv.Count > 0)
            {
                //lock (ReleaseInvoiceLock.LockObject)
                //{
                return this.invoiceRepository.Update(lstInv[0]);
                //}
            }
            return false;
        }
        public bool checkBeforeSign(long companyId, string symbol)
        {
            return this.invoiceRepository.checkBeforeSign(companyId, symbol);
        }
        public IQueryable<INVOICE> GetListInvoiceNewShinhan(long companyId, bool isOrg)
        {
            return this.invoiceRepository.GetAllInvoiceNewShinhan(companyId, isOrg);
        }
        public IEnumerable<InvoiceMaster> FilterCreated(ConditionSearchInvoice condition, int? flgCreate = 0)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceMasters = this.invoiceRepository.FilterInvoiceCreated(condition, flgCreate);
            return invoiceMasters;
        }
        public long CountV2(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.invoiceRepository.CountV2(condition);
        }
        public long Count(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.invoiceRepository.Count(condition);
        }
        public IEnumerable<InvoiceMaster> FilterV2(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceMasters = this.invoiceRepository.FilterInvoiceV2(condition);
            return invoiceMasters;
        }
        public long CountCreated(ConditionSearchInvoice condition, int? flgCreate = 0)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.invoiceRepository.CountCreated(condition, flgCreate);
        }
        public long CountInvoiceCreated(ConditionSearchInvoice condition, int? flgCreate = 0)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.invoiceRepository.CountInvoiceCreated(condition, flgCreate);
        }
        public IEnumerable<ReleaseInvoiceInfo> FilterListCreated(ConditionSearchInvoice condition, int? flgCreate = 0)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceInfos = this.invoiceRepository.ListInvoiceCreated(condition, flgCreate);
            return invoiceInfos;
        }
        public IEnumerable<InvoiceMaster> FillterInvoiceConverted(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceMasters = this.invoiceRepository.FillterInvoiceConverted(condition).Skip(condition.Skip).Take(condition.Take).ToList();
            return invoiceMasters;
        }

        public long CountInvoiceConverted(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.invoiceRepository.CountConverted(condition);
        }

        public ResultCode Create(InvoiceInfo info)
        {
            try
            {
                transaction.BeginTransaction();
                CreateOutTransaction(info);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        private void CreateOutTransaction(InvoiceInfo info, bool insertSubtitute = false)
        {
            FormatBySystemSetting(info);
            CLIENT client = ProcessClient(info);
            if (info.ClientId == client.ID)
            {
                UpdateClient(info);
            }
            else if (client.CUSTOMERTYPE == (int)CustomerType.IsAccounting)
            {
                info.EmailActiveId = this.CreateEmailAccountInfo(client);
            }
            info.ClientId = client.ID;
            INVOICE invoice;
            if (insertSubtitute)
                invoice = InsertInvoiceSubstitute(info, (int)InvoiceStatus.New);
            else
                invoice = InsertInvoice(info);

            info.Id = invoice.ID;
            if (!insertSubtitute)
                InsertInvoiDetail(info, info.InvoiceDetail);    // Hóa đơn hủy sẽ được insert detail sau

            //InsertStatistical(info);
            //if (info.InvoiceGift.Count > 0)
            //{
            //    InsertInvoiceGift(info);
            //}
            InsertQueueCreateFile(new List<long>() { info.Id });
        }

        public INVOICE CreateFromXml(InvoiceInfo info)
        {
            INVOICE invoice;
            try
            {
                transaction.BeginTransaction();
                invoice = InsertInvoice(info);
                info.Id = invoice.ID;
                InsertInvoiDetail(info, info.InvoiceDetail);
                InsertQueueCreateFile(new List<long>() { info.Id });
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return invoice;
        }
        public void InsertStatistical(InvoiceInfo invoiceInfo)
        {
            if (invoiceInfo.InvoiceStatistical != null)
            {
                invoiceInfo.InvoiceStatistical.ForEach(item =>
                {
                    STATISTICAL statistical = new STATISTICAL();
                    statistical.INVOICEID = invoiceInfo.Id;
                    DateTime? traddate = item.TradDate.DecodeUrl().ConvertDateTime();
                    statistical.TRADDATE = traddate;
                    statistical.CURRENCY = item.Currency.ToString();
                    statistical.AMOUNT = item.Amount;
                    statistical.AMOUNTTAX = item.TaxAmount;
                    statistical.EXCHANGERATE = item.ExchangeRate;
                    statistical.DESCRIPTION = item.Description;
                    this.invoiceStatisticalRepository.Insert(statistical);
                });

            }
        }
        public void InsertInvoiceGift(InvoiceInfo invoiceInfo)
        {
            invoiceInfo.InvoiceGift.ForEach(item =>
            {
                INVOICEGIFT invoiceGift = new INVOICEGIFT();
                invoiceGift.COMPANYID = item.CompanyId ?? 0;
                invoiceGift.INVOICEID = invoiceInfo.Id;
                DateTime? traddate = item.TradDate.DecodeUrl().ConvertDateTime();
                invoiceGift.CREATEDATE = traddate;
                invoiceGift.CURRENCY = item.Currency;
                invoiceGift.PRETAXAMOUNT = item.Amount;
                invoiceGift.AMOUNTTAX = item.TaxAmount;
                invoiceGift.GIFTNAME = item.GiftName;
                invoiceGift.REFNO = item.RefNo;
                invoiceGift.SEQUENCE = item.Sequence;
                invoiceGift.PERIODTAX = item.PeriodTax;
                invoiceGift.REMARK = item.Remark;
                invoiceGift.TAX = item.Tax;
                invoiceGift.TYPEGIFT = item.TypeGift;
                this.invoiceGiftRepository.Insert(invoiceGift);
            });
        }
        public ReleaseInvoiceMaster Substitute(InvoiceInfo info)
        {
            ReleaseInvoiceMaster releaseInvoice = new ReleaseInvoiceMaster();
            try
            {
                transaction.BeginTransaction();
                CLIENT client = ProcessClient(info);
                info.ClientId = client.ID;
                info.InvoiceType = (int)InvoiceType.Substitute;
                long oldId = info.Id;
                INVOICE invoice = InsertOrUpdateSubstitute(info);
                info.Id = invoice.ID;
                info.FileReceipt = SaveFileImport(info, oldId);
                UpdateFileReceipt(info);
                InsertInvoiDetail(info, info.InvoiceDetail);

                releaseInvoice.SetDataForReleaseInvoice(invoice, null, info.CompanyId, info.UserAction, "Phát hành thay thế hóa đơn");
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return releaseInvoice;
        }
        public ResultCode UpdateAdjustFileRecipt(InvoiceInfo info)
        {
            INVOICE invoiceCurrent = GetInvoice(info.Id);
            string pathFileOld = invoiceCurrent.FILERECEIPT;
            if (File.Exists(pathFileOld))
            {
                File.Delete(pathFileOld);
            }
            if (string.IsNullOrEmpty(pathFileOld))
            {
                info.FolderPath = DefaultFields.FILE_RECEPT_FOLDER;

                string filePath = SaveFileImport(info, invoiceCurrent.PARENTID ?? 0);

                invoiceCurrent.FILERECEIPT = filePath;

                this.invoiceRepository.Update(invoiceCurrent);
            }
            return ResultCode.NoError;
        }
        public ResultCode UpdateCancelFileRecipt(InvoiceInfo info)
        {
            INVOICE invoiceCurrent = GetInvoice(info.Id);
            string pathFileOld = invoiceCurrent.FILERECEIPT;
            if (File.Exists(pathFileOld))
            {
                File.Delete(pathFileOld);
            }
            if (string.IsNullOrEmpty(pathFileOld))
            {
                info.FolderPath = DefaultFields.FILE_RECEPT_FOLDER;

                string filePath = SaveFileImport(info, invoiceCurrent.ID.ToInt() ?? 0, (int)InvoiceStatus.Cancel);

                invoiceCurrent.FILERECEIPT = filePath;
                this.invoiceRepository.Update(invoiceCurrent);
            }
            return ResultCode.NoError;
        }
        public ReleaseInvoiceMaster Adjust(InvoiceInfo info)
        {
            ReleaseInvoiceMaster releaseInvoice = new ReleaseInvoiceMaster();
            try
            {
                transaction.BeginTransaction();
                if (info.InvoiceType == (int)InvoiceType.AdjustmentInfomation)
                {
                    if (info.ClientId == null)
                    {
                        CLIENT client = ProcessClient(info);
                        info.ClientId = client.ID;
                    }
                    else
                    {
                        UpdateInvoiceClient(info);
                    }
                    if (info.FileReceipt != null)
                    {
                        info.FileReceipt = SaveFileImport(info, info.Id);
                        UpdateFileReceipt(info);
                    }
                    UpdateInvoice(info.Id, info, 1);
                    INVOICE currenInvoice = GetInvoice(info.Id);
                    releaseInvoice.SetDataForReleaseInvoice(currenInvoice, info.CompanyId, info.UserAction, "Phát hành điều chỉnh hóa đơn");
                    transaction.Commit();
                }
                else
                {

                    var conditionSearchInvoice = new ConditionSearchInvoice()
                    {
                        CurrentUser = this.currentUser,
                        InvoiceSampleId = info.RegisterTemplateId,
                        Symbol = info.Symbol,
                        InvoiceNo = info.No,
                    };
                    CLIENT client = ProcessClient(info);
                    info.ClientId = client.ID;
                    INVOICE invoice = InsertInvoiceSubstitute(info, (int)InvoiceStatus.New);
                    long oldId = info.Id;
                    info.Id = invoice.ID;
                    if (info.FileReceipt != null)
                    {
                        info.FileReceipt = SaveFileImport(info, oldId);
                        UpdateFileReceipt(info);
                    }

                    InsertInvoiDetail(info, info.InvoiceDetail);
                    releaseInvoice.SetDataForReleaseInvoice(invoice, info.CompanyId, info.UserAction, "Phát hành điều chỉnh hóa đơn");

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

        public ReleaseInvoiceMaster AdjustJob(InvoiceInfo info, DateTime? releaseddate)
        {
            ReleaseInvoiceMaster releaseInvoice = new ReleaseInvoiceMaster();
            try
            {
                transaction.BeginTransaction();
                if (info.InvoiceType == (int)InvoiceType.AdjustmentInfomation)
                {
                    if (info.ClientId == null)
                    {
                        CLIENT client = ProcessClient(info);
                        info.ClientId = client.ID;
                    }
                    else
                    {
                        UpdateInvoiceClient(info);
                    }
                    if (info.FileReceipt != null)
                    {
                        info.FileReceipt = SaveFileImport(info, info.Id);
                        UpdateFileReceipt(info);
                    }
                    UpdateInvoice(info.Id, info, 1);
                    INVOICE currenInvoice = GetInvoice(info.Id);
                    releaseInvoice.SetDataForReleaseInvoice(currenInvoice, info.CompanyId, info.UserAction, "Phát hành điều chỉnh hóa đơn");
                    transaction.Commit();
                }
                else
                {

                    var conditionSearchInvoice = new ConditionSearchInvoice()
                    {
                        CurrentUser = this.currentUser,
                        InvoiceSampleId = info.RegisterTemplateId,
                        Symbol = info.Symbol,
                        InvoiceNo = info.No,
                    };
                    CLIENT client = ProcessClient(info);
                    info.ClientId = client.ID;
                    INVOICE invoice = InsertInvoiceSubstitute(info, (int)InvoiceStatus.New, releaseddate);
                    long oldId = info.Id;
                    info.Id = invoice.ID;
                    if (info.FileReceipt != null)
                    {
                        info.FileReceipt = SaveFileImport(info, oldId);
                        UpdateFileReceipt(info);
                    }

                    InsertInvoiDetail(info, info.InvoiceDetail);
                    releaseInvoice.SetDataForReleaseInvoice(invoice, info.CompanyId, info.UserAction, "Phát hành điều chỉnh hóa đơn");

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
        private INVOICE UpdateInvoiceDraft(long Id, InvoiceInfo invoiceInfo)
        {
            INVOICE invoiceDraft = GetCurrentInvoice(Id, (long)invoiceInfo.CompanyId);
            invoiceDraft.CopyData(invoiceInfo);
            this.InsertInvoiceSubstituteCopyMoreData(invoiceDraft, invoiceInfo, (int)InvoiceStatus.New);
            this.invoiceRepository.Update(invoiceDraft);
            return invoiceDraft;
        }

        private string SaveFileImport(InvoiceInfo fileImport, long oldId, int? status = 0)
        {
            if (fileImport.InvoiceType == (int)InvoiceType.Substitute || string.IsNullOrEmpty(fileImport.FileReceipt))  // Task #30215
                return null;

            string fullPathFile = this.config.PathInvoiceFile;
            string filePathFileName = null;
            string fileName;
            if (!Directory.Exists(fileImport.FolderPath))
            {
                Directory.CreateDirectory(fileImport.FolderPath);
            }
            fileName = string.Format(@"{0}\BienBanDieuChinhHoaDon_{1}.pdf", fileImport.FolderPath, fileImport.Id);
            ANNOUNCEMENT currentAnnouncement = new ANNOUNCEMENT();
            if (status.Equals((int)InvoiceStatus.Cancel))
            {
                currentAnnouncement = GetAnnouncement(oldId, 3, fileImport.CompanyId);
            }
            else
            {
                currentAnnouncement = GetAnnouncement(oldId, 1, fileImport.CompanyId);
            }
            filePathFileName = string.Format("{0}\\{1}\\Releases\\SignAnnouncement\\{2}_sign.pdf", fullPathFile, currentAnnouncement.COMPANYID, currentAnnouncement.ID);
            try
            {
                if (fileImport.FileReceipt == null)
                {
                    fileName = filePathFileName;
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
        public void UpdateClient(InvoiceInfo info)
        {
            CLIENT client = this.clientRepository.GetById((long)info.ClientId);
            client.UPDATEDDATE = DateTime.Now;
            client.CUSTOMERNAME = info.CompanyName;
            client.ADDRESS = info.Address;
            client.MOBILE = info.Mobile;
            if (client.USEREGISTEREMAIL != true)
            {
                client.RECEIVEDINVOICEEMAIL = info.Email;
            }
            client.BANKACCOUNT = info.BankAccount;
            client.BANKNAME = info.BankName;
            client.TAXCODE = info.TaxCode;
            client.ISORG = info.IsOrg;
            this.clientRepository.Update(client);
        }
        public ResultCode Update(long id, long? companyId, InvoiceInfo info)
        {
            try
            {
                transaction.BeginTransaction();
                this.UpdateOutTransaction(id, companyId, info);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        private void UpdateOutTransaction(long id, long? companyId, InvoiceInfo info, bool insertSubtitute = false)
        {
            FormatBySystemSetting(info);
            INVOICE currentInvoice;
            if (info.ClientId == null)
            {
                currentInvoice = GetInvoice(id);
                info.ClientId = currentInvoice.CLIENTID;
            }
            UpdateClient(info);
            UpdateInvoice(id, info, 2);
            DeleteInvoiceDetail(id);
            DeleteInvoiceStatistical(id);
            DeleteInvoiceGift(id);
            if (!insertSubtitute)
                InsertInvoiDetail(info, info.InvoiceDetail);    // Hóa đơn hủy sẽ được insert detail sau
            InsertStatistical(info);
            InsertInvoiceGift(info);
            DeleteInvoiceFile(id, companyId.Value);
            InsertQueueCreateFile(new List<long>() { id });
        }

        public string getInfoCertificate(string SerialNumber)
        {
            // co mot ham giong o AnnouncementBO.cs
            string NameCA = String.Empty;
            var certStore = new X509Store("ShinhanBank", StoreLocation.CurrentUser);//StoreName.My
            certStore.Open(OpenFlags.ReadOnly);
            X509CertificateCollection certCollection = certStore.Certificates.Find(X509FindType.FindBySerialNumber, SerialNumber, false);
            foreach (X509Certificate2 x509Certificate in certCollection.OfType<X509Certificate2>())
            {
                if (x509Certificate.SerialNumber == SerialNumber)
                {
                    NameCA = x509Certificate.GetNameInfo(X509NameType.SimpleName, true);
                }
            }
            certStore.Close();
            return NameCA;
        }
        public List<InvoiceStatistical> GetListStatisticals(long id)
        {
            return this.invoiceStatisticalRepository.FilterInvoiceStatistical(id).Select(p => new InvoiceStatistical(p)).ToList();
        }
        public List<InvoiceGift> GetListGift(long id)
        {
            return this.invoiceGiftRepository.FilterInvoiceGift(id).Select(p => new InvoiceGift(p)).ToList();
        }
        public InvoicePrintInfo GetInvoiceData(long id)
        {
            InvoicePrintInfo invoicePrint = this.invoiceRepository.GetInvoicePrintInfo(id);
            var systemSetting = this.systemSettingRepository.FilterSystemSetting(invoicePrint.CompanyId);
            this.FormatBySystemSetting(invoicePrint, systemSetting);

            invoicePrint.IsInvoiceSwitch = this.invoiceRepository.GetInvoiceReleaseInfo(id).Converted;
            invoicePrint.Day = invoicePrint.DataInvoice.Value.Day.ToString().PadLeft(2, '0');
            invoicePrint.Month = invoicePrint.DataInvoice.Value.Month.ToString().PadLeft(2, '0');
            invoicePrint.Year = invoicePrint.DataInvoice.Value.Year.ToString();
            var invoiceDetails = GetInvoiceDetail(invoicePrint.Id).Select(p => new InvoiceItem(p)).ToList();
            InvoiceItem oldDetail = invoiceDetails.FirstOrDefault();
            InvoiceItem newDetail = new InvoiceItem();
            newDetail.ProductCode = oldDetail.ProductCode;
            newDetail.Unit = oldDetail.Unit;
            newDetail.Quantity = oldDetail.Quantity;
            if (invoicePrint.ImportType != 2)
            {
                if (invoicePrint.Vat_invoice_type != null)
                {
                    if (invoicePrint.Vat_invoice_type.Contains("3"))
                    {
                        if (invoicePrint.Report_class != null || !string.IsNullOrEmpty(invoicePrint.Report_class))
                        {
                            if (invoicePrint.Report_class.Contains("6"))
                            {
                                newDetail.ProductName = "Tiền lãi ngân hàng(interest collection)";
                                newDetail.ProductNameInvoice = "Tiền lãi ngân hàng(interest collection)";
                            }
                            else
                            {
                                newDetail.ProductName = "Phí dịch vụ ngân hàng ( Fee commission)";
                                newDetail.ProductNameInvoice = "Phí dịch vụ ngân hàng ( Fee commission)";
                            }

                            newDetail.Total = invoicePrint.Total;
                            newDetail.FirstRow = true;
                            invoiceDetails.Add(newDetail);
                            invoiceDetails.Reverse();
                        }
                    }
                }
            }

            invoicePrint.InvoiceItems = invoiceDetails;
            //invoicePrint.InvoiceStatisticals = this.invoiceStatisticalRepository.FilterInvoiceStatistical(invoicePrint.Id).Select(p => new InvoiceStatistical(p)).ToList();
            //invoicePrint.InvoiceGifts = this.invoiceGiftRepository.FilterInvoiceGift(invoicePrint.Id).Select(p => new InvoiceGift(p)).ToList();
            if (invoicePrint.DateRelease != null)
            {
                invoicePrint.InvoiceDate = invoicePrint.DateRelease.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                invoicePrint.InvoiceDate = invoicePrint.DataInvoice.Value.ToString("dd/MM/yyyy");
            }

            // Get VerificationCode
            //var releaseInvoiceDetail = this.releaseInvoiceDetaiRepository.FilteReleaseInvoiceDetail(invoicePrint.Id);
            //invoicePrint.VerificationCode = releaseInvoiceDetail?.VERIFICATIONCODE ?? "";

            invoicePrint.TaxAmout5 = 0;
            invoicePrint.TaxAmout10 = 0;
            foreach (var item in invoicePrint.InvoiceItems)
            {
                item.ProductName = GetProductName(invoicePrint, item);
                if (item.TaxId == 2)
                {
                    invoicePrint.TaxAmout5 += item.AmountTax;
                }
                if (item.TaxId == 3)
                {
                    invoicePrint.TaxAmout10 += item.AmountTax;
                }
            }
            //invoicePrint.StatisticalSumAmount = 0;
            //invoicePrint.StatisticalSumTaxAmount = 0;
            //invoicePrint.InvoiceStatisticals.ForEach(p =>
            //{
            //    invoicePrint.StatisticalSumAmount += p.Amount;
            //    invoicePrint.StatisticalSumTaxAmount += p.TaxAmount;
            //});
            //invoicePrint.GiftSumAmount = 0;
            //invoicePrint.GiftSumTaxAmount = 0;
            //invoicePrint.InvoiceGifts.ForEach(p =>
            //{
            //    invoicePrint.GiftSumAmount += p.PreTaxAmount;
            //    invoicePrint.GiftSumTaxAmount += p.TaxAmount;
            //});

            return invoicePrint;
        }

        private string GetProductName(InvoicePrintInfo invoicePrint, InvoiceItem item)
        {
            if (invoicePrint.Invoice_Status != (int)InvoiceStatus.New || invoicePrint.InvoiceSample != 0)
            {
                item.ProductName = item.ProductNameInvoice;
            }
            if (item.Discount == true)
            {
                item.ProductName = item.DiscountDescription;
            }
            return item.ProductName;
        }

        public InvoiceInfo GetInvoiceInfo(long id, long? companyId, bool isInvoice = true)
        {
            INVOICE currentInvoice = GetInvoice(id);

            // Set parent symbol 
            currentInvoice.PARENTSYMBOL = currentInvoice.SYMBOL;
            long? oldTemplateId = currentInvoice.REGISTERTEMPLATEID;
            var currentDetails = currentInvoice.INVOICEDETAILs.Where(p => !(p.DELETED ?? false)).Select(p => new InvoiceDetailInfo(p)).ToList();
            List<InvoiceDetailInfo> invoiceDetails = new List<InvoiceDetailInfo>();
            if (currentInvoice.IMPORTTYPE != 2)
            {
                if (currentInvoice.REPORT_CLASS != null)
                {
                    InvoiceDetailInfo invoiceDetailInfo = new InvoiceDetailInfo();
                    invoiceDetailInfo.Price = currentInvoice.TOTAL;
                    invoiceDetailInfo.Total = currentInvoice.TOTAL;
                    invoiceDetailInfo.AmountTax = currentInvoice.TOTALTAX;
                    invoiceDetailInfo.Sum = (currentInvoice.TOTAL + currentInvoice.TOTALTAX);
                    if (currentInvoice.REF_CTT != null)
                    {
                        invoiceDetailInfo.ProductName = currentInvoice.REF_CTT;
                    }
                    else
                    {
                        invoiceDetailInfo.ProductName = currentInvoice.REPORT_CLASS != null ?
                            currentInvoice.REPORT_CLASS.Contains("6") ? "Tiền lãi ngân hàng ( interest collection)" + " Ref:" + currentInvoice.REFNUMBER :
                            "Phí dịch vụ Ngân hàng ( Fee commission)" + " Ref:" + currentInvoice.REFNUMBER : "";
                    }

                    var taxInfo = this.taxRepository.GetTax(Convert.ToDecimal(currentInvoice.VAT_RATE));
                    if (taxInfo != null)
                    {
                        invoiceDetailInfo.TaxId = taxInfo.ID;
                    }
                    else
                    {
                        invoiceDetailInfo.TaxId = null;
                    }

                    invoiceDetails.Add(invoiceDetailInfo);
                }
                else
                {
                    invoiceDetails = currentDetails;
                }
            }
            else
            {

                invoiceDetails = currentDetails;
            }

            invoiceDetails = invoiceDetails.OrderBy(p => p.Id).ToList();
            InvoiceInfo invoiceInfo_ = new InvoiceInfo(currentInvoice, invoiceDetails, null, null);
            var ProductFirst = currentInvoice?.INVOICEDETAILs.OrderByDescending(p => p.ID).FirstOrDefault().TAXID;
            var client = this.clientRepository.GetById((long)invoiceInfo_.ClientId);
            invoiceInfo_.Email = client.USEREGISTEREMAIL == true ? client.EMAIL : client.RECEIVEDINVOICEEMAIL;
            invoiceInfo_.TaxId = ProductFirst;
            var company = this.myCompanyRepository.GetById(companyId ?? 0);
            invoiceInfo_.Delegate = company.DELEGATE;

            invoiceInfo_.FileReceipt = Path.GetFileName(invoiceInfo_.FileReceipt);
            if (invoiceInfo_.Sum.HasValue)
            {
                invoiceInfo_.AmountInWords = ReadNuberToText.ConvertNumberToString(invoiceInfo_.Sum.Value.ToString());
            }
            if (!isInvoice || currentInvoice.INVOICETYPE != null)
            {
                invoiceInfo_.CompanyName = currentInvoice.CUSTOMERNAME;
                invoiceInfo_.PersonContact = currentInvoice.PERSONCONTACT;
                invoiceInfo_.CustomerName = currentInvoice.PERSONCONTACT;
                invoiceInfo_.Address = currentInvoice.CUSTOMERADDRESS;
                invoiceInfo_.BankAccount = currentInvoice.CUSTOMERBANKACC;
                invoiceInfo_.TaxCode = currentInvoice.CUSTOMERTAXCODE;
                invoiceInfo_.ParentReleaseDate = currentInvoice?.INVOICE2?.RELEASEDDATE;
                invoiceInfo_.OldTemplateId = oldTemplateId;
            }
            invoiceInfo_.CustomerCode = currentInvoice?.CLIENT.CUSTOMERCODE;
            invoiceInfo_.CompanyId = currentInvoice.COMPANYID;
            invoiceInfo_.BranchId = company.BRANCHID;
            invoiceInfo_.VatInvoiceType = currentInvoice.VAT_INVOICE_TYPE;
            return invoiceInfo_;
        }

        public ResultCode Delete(long id, long? companyId)
        {
            return DeleteInvoice(id);
        }

        public ResultCode UpdateStatus(long id, long? companyId, int status)
        {
            if (!companyId.HasValue)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            INVOICE currentInvoice = GetInvoice(id);
            currentInvoice.INVOICESTATUS = status;
            currentInvoice.UPDATEDDATE = DateTime.Now;
            return this.invoiceRepository.Update(currentInvoice) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public ResultCode UpdateNote(long id, InvoiceNote invoiceNote)
        {
            if (invoiceNote == null || !invoiceNote.CompanyId.HasValue)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            DeleteInvoiceFile(id, invoiceNote.CompanyId.Value);
            INVOICE currentInvoice = GetInvoice(id);
            currentInvoice.NOTE = invoiceNote.Note;
            currentInvoice.UPDATEDDATE = DateTime.Now;
            this.invoiceRepository.Update(currentInvoice);
            InsertQueueCreateFile(new List<long>() { id });
            return ResultCode.NoError;
        }

        public ExportFileInfo PrintView(long id)
        {
            ExportFileInfo fileInfo = GetInvoiceFile(id);
            if (fileInfo == null)
            {
                fileInfo = ExportInvoiceToPdf(id);
            }

            return fileInfo;
        }
        public void SetinvoiceInfo(InvoicePrintInfo invoicePrint)
        {
            this.invoiceInfo = invoicePrint;
        }

        public ExportFileInfo GetXmlFile(long id)
        {
            ExportFileInfo fileInfo = GetInvoiceXmlFile(id);
            if (fileInfo == null)
            {
                fileInfo = DownloadInvoiceXML(id);
            }
            return fileInfo;
        }
        private string callApiServerSign(SignListModel invoiceSigns, bool isPer = false)
        {
            string response = String.Empty;
            var json = JsonConvert.SerializeObject(invoiceSigns);
            string DATA = json.ToString();
            string hostSign = isPer ? WebConfigurationManager.AppSettings["UrlServerSignPer"] : WebConfigurationManager.AppSettings["UrlServerSign"];
            string UrlServerSign = PathUtil.UrlCombine(hostSign, ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionSignInvoiceMultipe);
            string UrlServerSignTestHSM = PathUtil.UrlCombine(hostSign, ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionSignInvoiceMultipeTestHSM);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlServerSign);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(DATA);
            request.ContentLength = byteArray.Length;
            request.Timeout = 18000000;//tăng timeout gọi api
            using (Stream webStream = request.GetRequestStream())
            {
                webStream.Write(byteArray, 0, byteArray.Length);
                webStream.Close();
            }
            using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
            {
                using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
                {
                    StreamReader responseReader = new StreamReader(webStream);
                    response = responseReader.ReadToEnd().ToString();
                    response = JsonConvert.DeserializeObject(response).ToString();
                }
            }

            return response.ToString();
        }
        private SignListModel ProcessSignXmls(List<InvoicePrintModel> invoicePrintModel)
        {
            SignListModel invoiceSign = new SignListModel();
            var invoiceModel = invoicePrintModel.FirstOrDefault();
            invoiceSign.Password = invoiceModel.PASSWORD;
            invoiceSign.SerialNumber = invoiceModel.SERIALNUMBER;
            invoiceSign.Slot = invoiceModel.SLOTS.ToInt();
            invoiceSign.SignHSM = true;
           
            string pathFileXmlSign = string.Empty;
            bool overwriteSign = bool.Parse(this.GetConfig("overwriteSign") ?? "false");
            List<InvoiceFor> invoiceFors = new List<InvoiceFor>();
            foreach (var item in invoicePrintModel)
            {
                try
                {
                    if (!overwriteSign)
                    {
                        pathFileXmlSign = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Releases", "Sign", item.RELEASEDDATE.Value.ToString("yyyy"), item.RELEASEDDATE.Value.ToString("MM"), item.RELEASEDDATE.Value.ToString("dd"), item.ID.ToString() + "_sign.xml");
                        if (File.Exists(pathFileXmlSign))//check file đã được ký hay chưa
                        {
                            continue;
                        }
                    }

                    InvoiceFor invoiceFor = new InvoiceFor();
                    invoiceFor.InvoiceId = Convert.ToInt64(item.ID);
                    //get filepath xml
                    string pathFileXml = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Invoice", item.ID.ToString() + ".xml");
                    if (!File.Exists(pathFileXml))//check file đã được tạo hay chưa
                    {
                        continue;
                    }

                    invoiceFor.PathFileXml = pathFileXml;
                    invoiceFor.DateFolder = PathUtil.FolderTree(item.RELEASEDDATE);
                    invoiceSign.DateSignXml = item.RELEASEDDATE.Value.ToString("dd/MM/yyyy");
                    invoiceFors.Add(invoiceFor);
                }
                catch (Exception ex)
                {
                    logger.Error("SignJobInvoice", ex);
                }

            }
            invoiceSign.invoiceFors = invoiceFors;

            return invoiceSign;
        }

        public void SignXmls(List<InvoicePrintModel> invoiceModels, bool isPer = false)
        {
            var signData = ProcessSignXmls(invoiceModels);
            string result = callApiServerSign(signData, isPer);
            logger.Error("SignXmls", new Exception(result));
        }
        public void ProcessXmls(List<InvoiceExportXmlModel> invoiceExportXmlModels)
        {
            //logger.Error("ProcessXml task child", new Exception(invoiceExportXmlModels.Count().ToString()));
            string xml = string.Empty;
            string pathFile = string.Empty;
            foreach (var item in invoiceExportXmlModels)
            {
                try
                {
                    //logger.Error("start item: " + item.InvoicePrintModel.ID, new Exception(DateTime.Now.ToString()));
                    pathFile = Path.Combine(this.config.PathInvoiceFile, item.InvoicePrintModel.COMPANYID.ToString(), "Invoice", item.InvoicePrintModel.ID.ToString() + ".xml");
                    //if (!File.Exists(pathFile))
                    //{
                    // tạo nội dung file XML
                    xml = ExportXmlsJob(item);

                    File.WriteAllText(pathFile, xml);
                    //string result = callApiServerSign(pathFile, item.InvoicePrintModel);
                    //logger.Error("end item: " + item.InvoicePrintModel.ID, new Exception(DateTime.Now.ToString()));
                    //}

                }
                catch (Exception ex)
                {
                    logger.Error("JobCreateFile", ex);
                }

            }
        }
        public void ProcessPdfs(List<InvoiceExportModel> invoiceExportPdfModels)
        {
            string html = string.Empty;
            string fullPathFile = string.Empty;
            string fileName = string.Empty;
            foreach (var item in invoiceExportPdfModels)
            {
                try
                {
                    fullPathFile = Path.Combine(this.config.PathInvoiceFile, item.InvoicePrintModel.COMPANYID.ToString(), "Releases", "Sign", item.InvoicePrintModel.RELEASEDDATE.Value.ToString("yyyy"), item.InvoicePrintModel.RELEASEDDATE.Value.ToString("MM"), item.InvoicePrintModel.RELEASEDDATE.Value.ToString("dd"));
                    fileName = item.InvoicePrintModel.ID.ToString() + "_sign.pdf";
                    //if (!File.Exists(pathFile))
                    //{
                    // Tạo file nội dung file pdf
                    //html = ExportInvoiceToPdfCreateFile(item);
                    //html = File.ReadAllText(@"C:\PhuongDD\eInvoice\05.SourceCode\trunk\TT78\01.Shinhan\01.BE\InvoiceServer.API\Data\InvoiceTemplate\test_shinhan.html");                    
                    //PdfSharpConvert(html, pathFile);
                    ExportInvoiceToPdfCreateFile(item, fullPathFile, fileName);
                    //}

                }
                catch (Exception ex)
                {
                    logger.Error("JobCreateFile", ex);
                }

            }
        }
        public void PdfSharpConvert(string html, string path)
        {
            HtmlToPdf htmlToPdfConverter = new HtmlToPdf();
            htmlToPdfConverter.Document.PageSize = PdfPageSize.A4;
            htmlToPdfConverter.Document.PageOrientation = PdfPageOrientation.Portrait;
            // set PDF page margins
            htmlToPdfConverter.Document.Margins = new PdfMargins(0);
            //htmlToPdfConverter.Document.PageSize.Width = 824;
            //htmlToPdfConverter.Document.PageSize.Height = 595;
            string baseUrl = "";
            byte[] pdfBuffer = null;
            // convert HTML code to a PDF memory buffer
            pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, baseUrl);
            File.WriteAllBytes(path, pdfBuffer);

        }
        public ExportModelSumary ProcessInvoicePrint_Old(List<InvoicePrintModel> invoicePrints, MYCOMPANY myconpany)
        {
            ExportModelSumary result = new ExportModelSumary();
            var pdfExport = new List<InvoiceExportModel>();
            var xmlExport = new List<InvoiceExportXmlModel>();
            this.config.BuildAssetByCompany(new CompanyInfo(myconpany));
            string pathFile = string.Empty;
            bool overwriteSign = bool.Parse(this.GetConfig("overwriteSign") ?? "false");
            var settings = this.systemSettingRepository.FilterSystemSetting(myconpany.COMPANYSID);
            //logger.Error("settings amount: " + settings.Amount, new Exception("ProcessInvoicePrint"));
            //connection.Query<SystemSettingInfo>("SELECT ID, QUANTITY, PRICE, EXCHANGERATE, AMOUNT, CONVERTEDAMOUNT FROM SYSTEMSETTING").FirstOrDefault();
            INVOICETEMPLATE template = GetTemplateNoTracking(1);
            foreach (var item in invoicePrints)
            {
                if (!overwriteSign)
                {
                    pathFile = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Releases", "Sign", item.RELEASEDDATE.Value.ToString("yyyy"), item.RELEASEDDATE.Value.ToString("MM"), item.RELEASEDDATE.Value.ToString("dd"), item.ID.ToString() + "_sign.pdf");
                    if (File.Exists(pathFile) && File.Exists(pathFile.Replace("_sign.pdf", "_sign.xml")))
                    {
                        continue;
                    }
                }

                FormatBySystemSetting(item, settings);
                //string scriptDetail = @"SELECT DT.*, T.DISPLAYINVOICE FROM INVOICEDETAIL DT
                //                            INNER JOIN TAX T ON T.ID = DT.TAXID WHERE DT.INVOICEID = " + item.ID;
                //var details = connection.Query<INVOICEDETAIL>(scriptDetail).Select(p => new InvoiceItem(p)).ToList();
                //logger.Error("Start GetInvoiceDetail: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff"), new Exception("ProcessInvoicePrint"));
                var details = GetInvoiceDetailJob(Convert.ToInt64(item.ID)).Select(p => new InvoiceItem(p)).ToList();
                TaxCodes.ListTax.TryGetValue(details[0].TaxCode, out string tax);
                item.TAXNAME = tax;
                item.InvoiceItems = details;
                item.Option.Height = template.HEIGHT;
                item.Option.Width = template.WIDTH;
                item.Option.PageSize = template.PAGESIZE;
                item.Option.PageOrientation = template.PAGEORIENTATION;
                this.config.FullPathFileTemplateInvoice = Path.Combine(this.config.FullPathFolderTemplateInvoice, template.URLFILE);
                var itemName = item.InvoiceItems;
                itemName.ForEach(p =>
                {
                    //if (item.INVOICESTATUS != (int)InvoiceStatus.New || item.INV != 0)
                    //{
                    //    p.ProductName = p.ProductNameInvoice;
                    //}
                    if (p.TaxId == 4)
                    {
                        p.AmountTax = null;
                    }
                    this.FormatBySystemSetting(p, settings);
                });
                var itemsVAT = itemName.Where(p => p.TaxId != 4).ToList();
                if (itemsVAT.Count == 0)
                {
                    item.TOTALTAX = null;
                }
                int numberLineOfPage = template.NUMBERLINEOFPAGE;

                int numberCharacterOfLine = (int)template.NUMBERCHARACTERINLINE;
                string folderSign = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Releases", "Sign", item.RELEASEDDATE.Value.ToString("yyyy"), item.RELEASEDDATE.Value.ToString("MM"), item.RELEASEDDATE.Value.ToString("dd"));
                if (!Directory.Exists(folderSign))
                {
                    Directory.CreateDirectory(folderSign);
                }
                string folderInvoice = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Invoice");
                if (!Directory.Exists(folderInvoice))
                {
                    Directory.CreateDirectory(folderInvoice);
                }
                // Get VerificationCode
                var invoiceExportModel = new InvoiceExportModel()
                {
                    InvoicePrintModel = item,
                    //InvoiceStatisticals = objStatiscals,
                    //InvoiceGift = objInvocieGifts,
                    PrintConfig = config,
                    NumberLineOfPage = numberLineOfPage,
                    NumberRecord = template.NUMBERRECORD ?? 0,
                    DetailInvoice = template.DETAILINVOICE,
                    LocationSignLeft = template.LOCATIONSIGNLEFT,
                    LocationSignButton = template.LOCATIONSIGNBUTTON,
                    NumberCharacterOfLine = numberCharacterOfLine,
                    IsShowOrder = template.ISSHOWORDER,
                    HeaderInvoice = template.HEADERINVOICE,
                    FooterInvoice = template.FOOTERINVOICE,
                    UserSessionInfo = currentUser,
                    TemplateFileName = template.URLFILE,
                    SystemSetting = settings,
                };
                pdfExport.Add(invoiceExportModel);
                InvoiceExportXmlModel invoiceExportXmlModel = new InvoiceExportXmlModel()
                {
                    InvoicePrintModel = item,
                    PrintConfig = this.config,
                    SystemSettingInfo = settings,
                };
                xmlExport.Add(invoiceExportXmlModel);
            }
            //logger.Error("End for ProcessInvoicePrint: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff"), new Exception("ProcessInvoicePrint"));
            result.invoiceExportPdfModels = pdfExport;
            result.invoiceExportXmlModels = xmlExport;
            return result;
        }
        public ExportModelSumary ProcessInvoicePrint(List<InvoicePrintModel> invoicePrints, MYCOMPANY myconpany, INVOICETEMPLATE template)
        {
            ExportModelSumary result = new ExportModelSumary();
            try
            {
                var pdfExport = new List<InvoiceExportModel>();
                var xmlExport = new List<InvoiceExportXmlModel>();
                this.config.BuildAssetByCompany(new CompanyInfo(myconpany));
                string pathFile = string.Empty;
                bool overwriteSign = bool.Parse(this.GetConfig("overwriteSign") ?? "false");
                //var settings = this.systemSettingRepository.FilterSystemSetting(myconpany.COMPANYSID);
                var settings = this.systemSettingRepository.FilterSystemSettingJob();
                var listDetail = this.invoiceDetailRepository.GetDetail(invoicePrints);
                foreach (var item in invoicePrints)
                {
                    //logger.Error("Invoice error for before Currency : SYMBOL " + item.SYMBOL + " INVOICENO : " + item.INVOICENO 
                    //    + "setting amount : " + settings.Amount + "Currency now : " + item.CURRENCYCODE, new Exception("Invoice before for Currency"));

                    if (item.CURRENCYCODE != DefaultFields.CURRENCY_VND && settings.Amount == 0)
                    {
                        logger.Error("Invoice error for error Currency : SYMBOL " + item.SYMBOL + " INVOICENO : " + item.INVOICENO
                        + "setting amount : " + settings.Amount + "Currency now : " + item.CURRENCYCODE, new Exception("Invoice error for Currency"));
                        continue;
                    }
                    if (!overwriteSign)
                    {
                        pathFile = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Releases", "Sign", item.RELEASEDDATE.Value.ToString("yyyy"), item.RELEASEDDATE.Value.ToString("MM"), item.RELEASEDDATE.Value.ToString("dd"), item.ID.ToString() + "_sign.pdf");
                        if (File.Exists(pathFile) && File.Exists(pathFile.Replace("_sign.pdf", "_sign.xml")))
                        {
                            continue;
                        }
                    }


                    FormatBySystemSetting(item, settings);
                    //logger.Error("Start GetInvoiceDetail: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff"), new Exception("ProcessInvoicePrint"));
                    //var details = GetInvoiceDetailJob(Convert.ToInt64(item.ID)).Select(p => new InvoiceItem(p)).ToList();
                    var details = listDetail.Where(p => p.InvoiceId == item.ID).Select(p => new InvoiceItem(p)).ToList();
                    TaxCodes.ListTax.TryGetValue(details[0].TaxCode, out string tax);
                    item.TAXNAME = tax;
                    item.InvoiceItems = details;
                    item.Option.Height = template.HEIGHT;
                    item.Option.Width = template.WIDTH;
                    item.Option.PageSize = template.PAGESIZE;
                    item.Option.PageOrientation = template.PAGEORIENTATION;
                    this.config.FullPathFileTemplateInvoice = Path.Combine(this.config.FullPathFolderTemplateInvoice, template.URLFILE);
                    var itemName = item.InvoiceItems;
                    itemName.ForEach(p =>
                    {
                        //if (item.INVOICESTATUS != (int)InvoiceStatus.New || item.INV != 0)
                        //{
                        //    p.ProductName = p.ProductNameInvoice;
                        //}
                        if (p.TaxId == 4)
                        {
                            p.AmountTax = null;
                        }
                        this.FormatBySystemSettingJob(p, settings, item.CURRENCYCODE);
                    });
                    var itemsVAT = itemName.Where(p => p.TaxId != 4).ToList();
                    if (itemsVAT.Count == 0)
                    {
                        item.TOTALTAX = null;
                    }
                    int numberLineOfPage = template.NUMBERLINEOFPAGE;

                    int numberCharacterOfLine = (int)template.NUMBERCHARACTERINLINE;
                    string folderSign = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Releases", "Sign", item.RELEASEDDATE.Value.ToString("yyyy"), item.RELEASEDDATE.Value.ToString("MM"), item.RELEASEDDATE.Value.ToString("dd"));
                    if (!Directory.Exists(folderSign))
                    {
                        Directory.CreateDirectory(folderSign);
                    }
                    string folderInvoice = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Invoice");
                    if (!Directory.Exists(folderInvoice))
                    {
                        Directory.CreateDirectory(folderInvoice);
                    }
                    // Get VerificationCode
                    var invoiceExportModel = new InvoiceExportModel()
                    {
                        InvoicePrintModel = item,
                        //InvoiceStatisticals = objStatiscals,
                        //InvoiceGift = objInvocieGifts,
                        PrintConfig = config,
                        NumberLineOfPage = numberLineOfPage,
                        NumberRecord = template.NUMBERRECORD ?? 0,
                        DetailInvoice = template.DETAILINVOICE,
                        LocationSignLeft = template.LOCATIONSIGNLEFT,
                        LocationSignButton = template.LOCATIONSIGNBUTTON,
                        NumberCharacterOfLine = numberCharacterOfLine,
                        IsShowOrder = template.ISSHOWORDER,
                        HeaderInvoice = template.HEADERINVOICE,
                        FooterInvoice = template.FOOTERINVOICE,
                        UserSessionInfo = currentUser,
                        TemplateFileName = template.URLFILE,
                        SystemSetting = settings,
                    };
                    pdfExport.Add(invoiceExportModel);
                    InvoiceExportXmlModel invoiceExportXmlModel = new InvoiceExportXmlModel()
                    {
                        InvoicePrintModel = item,
                        PrintConfig = this.config,
                        SystemSettingInfo = settings,
                    };
                    xmlExport.Add(invoiceExportXmlModel);
                }
                //logger.Error("End for ProcessInvoicePrint: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff"), new Exception("ProcessInvoicePrint"));
                result.invoiceExportPdfModels = pdfExport;
                result.invoiceExportXmlModels = xmlExport;
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }
        public ExportModelSumary ProcessInvoicePrint(InvoicePrintModel invoicePrint)
        {
            ExportModelSumary result = new ExportModelSumary();
            var pdfExport = new List<InvoiceExportModel>();
            var xmlExport = new List<InvoiceExportXmlModel>();
            var mycompany = this.myCompanyRepository.GetById(Convert.ToInt64(invoicePrint.COMPANYID));
            this.config.BuildAssetByCompany(new CompanyInfo(mycompany));
            string pathFile = string.Empty;
            bool overwriteSign = bool.Parse(this.GetConfig("overwriteSign") ?? "false");
            var settings = this.systemSettingRepository.FilterSystemSetting(mycompany.COMPANYSID);
            //if (!overwriteSign)
            //{
            //    pathFile = Path.Combine(this.config.PathInvoiceFile, invoicePrint.COMPANYID.ToString(), "Releases", "Sign", invoicePrint.RELEASEDDATE.Value.ToString("yyyy"), invoicePrint.RELEASEDDATE.Value.ToString("MM"), invoicePrint.RELEASEDDATE.Value.ToString("dd"), invoicePrint.ID.ToString() + "_sign.pdf");
            //    if (File.Exists(pathFile) && File.Exists(pathFile.Replace("_sign.pdf", "_sign.xml")))
            //    {
            //        continue;
            //    }
            //}
            FormatBySystemSetting(invoicePrint, settings);
            //string scriptDetail = @"SELECT DT.*, T.DISPLAYINVOICE FROM INVOICEDETAIL DT
            //                            INNER JOIN TAX T ON T.ID = DT.TAXID WHERE DT.INVOICEID = " + item.ID;
            //var details = connection.Query<INVOICEDETAIL>(scriptDetail).Select(p => new InvoiceItem(p)).ToList();
            var details = GetInvoiceDetail(Convert.ToInt64(invoicePrint.ID)).Select(p => new InvoiceItem(p)).ToList();
            TaxCodes.ListTax.TryGetValue(details[0].TaxCode, out string tax);
            invoicePrint.TAXNAME = tax;
            invoicePrint.InvoiceItems = details;
            INVOICETEMPLATE template = GetTemplateNoTracking(1);
            //var template = connection.Query<INVOICETEMPLATE>("SELECT * FROM INVOICETEMPLATE WHERE ID = " + item.REGISTERTEMPLATEID).FirstOrDefault();
            invoicePrint.Option.Height = template.HEIGHT;
            invoicePrint.Option.Width = template.WIDTH;
            invoicePrint.Option.PageSize = template.PAGESIZE;
            invoicePrint.Option.PageOrientation = template.PAGEORIENTATION;
            this.config.FullPathFileTemplateInvoice = Path.Combine(this.config.FullPathFolderTemplateInvoice, template.URLFILE);
            var itemName = invoicePrint.InvoiceItems;
            var settings_2 = this.systemSettingRepository.FilterSystemSetting(mycompany.COMPANYSID);
            itemName.ForEach(p =>
            {
                //if (item.INVOICESTATUS != (int)InvoiceStatus.New || item.INV != 0)
                //{
                //    p.ProductName = p.ProductNameInvoice;
                //}
                if (p.TaxId == 4)
                {
                    p.AmountTax = null;
                }
                //this.FormatBySystemSetting(p, settings);
                this.FormatBySystemSettingJob(p, settings_2, invoicePrint.CURRENCYCODE);
            });
            var itemsVAT = itemName.Where(p => p.TaxId != 4).ToList();
            if (itemsVAT.Count == 0)
            {
                invoicePrint.TOTALTAX = null;
            }
            int numberLineOfPage = template.NUMBERLINEOFPAGE;

            int numberCharacterOfLine = (int)template.NUMBERCHARACTERINLINE;
            string folderSign = Path.Combine(this.config.PathInvoiceFile, invoicePrint.COMPANYID.ToString(), "Releases", "Sign", invoicePrint.RELEASEDDATE.Value.ToString("yyyy"), invoicePrint.RELEASEDDATE.Value.ToString("MM"), invoicePrint.RELEASEDDATE.Value.ToString("dd"));
            if (!Directory.Exists(folderSign))
            {
                Directory.CreateDirectory(folderSign);
            }
            string folderInvoice = Path.Combine(this.config.PathInvoiceFile, invoicePrint.COMPANYID.ToString(), "Invoice");
            if (!Directory.Exists(folderInvoice))
            {
                Directory.CreateDirectory(folderInvoice);
            }
            // Get VerificationCode
            var invoiceExportModel = new InvoiceExportModel()
            {
                InvoicePrintModel = invoicePrint,
                //InvoiceStatisticals = objStatiscals,
                //InvoiceGift = objInvocieGifts,
                PrintConfig = config,
                NumberLineOfPage = numberLineOfPage,
                NumberRecord = template.NUMBERRECORD ?? 0,
                DetailInvoice = template.DETAILINVOICE,
                LocationSignLeft = template.LOCATIONSIGNLEFT,
                LocationSignButton = template.LOCATIONSIGNBUTTON,
                NumberCharacterOfLine = numberCharacterOfLine,
                IsShowOrder = template.ISSHOWORDER,
                HeaderInvoice = template.HEADERINVOICE,
                FooterInvoice = template.FOOTERINVOICE,
                UserSessionInfo = currentUser,
                TemplateFileName = template.URLFILE,
                SystemSetting = settings,
            };
            pdfExport.Add(invoiceExportModel);
            InvoiceExportXmlModel invoiceExportXmlModel = new InvoiceExportXmlModel()
            {
                InvoicePrintModel = invoicePrint,
                PrintConfig = this.config,
                SystemSettingInfo = settings,
            };
            xmlExport.Add(invoiceExportXmlModel);
            result.invoiceExportPdfModels = pdfExport;
            result.invoiceExportXmlModels = xmlExport;
            return result;
        }
        public void ExportInvoiceToPdfCreateFile(InvoiceExportModel invoiceExportModel, string fullPathFile, string fileName)
        {
            HQExportFileJob export = new HQExportFileJob(invoiceExportModel);
            export.ExportFileJob(fullPathFile, fileName);
        }
        private RELEASEINVOICE CreateRelease()
        {
            ReleaseInvoiceMaster releaseInvoiceMaster = new ReleaseInvoiceMaster();
            releaseInvoiceMaster.CompanyId = this.currentUser.Company.Id;
            releaseInvoiceMaster.ReleasedDate = DateTime.Now;
            releaseInvoiceMaster.Description = "";
            return this.releaseInvoiceBO.InsertReleaseInvoice(releaseInvoiceMaster, ReleaseInvoiceStatus.New);
        }
        private void GenInvoiceNoCreateFile(long invoiceId)
        {
            var currentInvoice = invoiceRepository.GetById(invoiceId);
            // Sinh số hóa đơn
            currentInvoice.NO = this.releaseInvoiceBO.GeneratorInvoiceNoFromBD(currentInvoice.COMPANYID, currentInvoice.REGISTERTEMPLATEID.Value, currentInvoice.SYMBOL.Replace("/", ""), currentInvoice.INVOICESTATUS);
            currentInvoice.INVOICENO = currentInvoice.NO.ToDecimal(0);
            currentInvoice.RELEASEDDATE = DateTime.Now;
            this.invoiceRepository.Update(currentInvoice);
        }

        private void CreateReleaseDetail(INVOICE invoices, long ReleaseId)
        {
            ReleaseInvoiceInfo releaseInvoiceInfo = new ReleaseInvoiceInfo()
            {
                InvoiceId = invoices.ID,
                InvoiceNo = invoices.NO
            };
            InsertReleaseInvoiceDetail(ReleaseId, invoices.COMPANYID, releaseInvoiceInfo);
        }

        private void InsertReleaseInvoiceDetail(long releaseInvoiceId, long companyId, ReleaseInvoiceInfo releaseDetail)
        {
            INVOICE currentInvoice = GetInvoice(releaseDetail.InvoiceId);
            this.CheckInvoice(currentInvoice);

            List<string> symbols = GetSymbolsInReportCancellingInvoice(companyId, (long)currentInvoice.REGISTERTEMPLATEID);
            if (symbols.Count > 0)
            {
                foreach (var item in symbols)
                {
                    if (currentInvoice.SYMBOL == item)
                    {
                        throw new BusinessLogicException(ResultCode.InvoiceNotApproveSymbol, "Dãy hóa đơn đã bị hủy,không thể thực hiện được chức năng phê duyệt/ký hóa đơn.");
                    }
                }
            }
            RELEASEINVOICEDETAIL releaseInvoiceDetail = new RELEASEINVOICEDETAIL();
            releaseInvoiceDetail.INVOICEID = currentInvoice.ID;
            releaseInvoiceDetail.RELEASEINVOICEID = releaseInvoiceId;
            releaseInvoiceDetail.MESSAGES = "Hệ thống đang phát hành hóa đơn";
            releaseInvoiceDetail.VERIFICATIONCODE = GetVerificationCode(currentInvoice.ID);
            this.releseInvoiceDetailRepository.Insert(releaseInvoiceDetail);
        }

        private void CheckInvoice(INVOICE invoice)
        {
            if (invoice.PARENTID != null && invoice.INVOICESTATUS != (int)InvoiceStatus.Cancel)
            {
                var invoiceParent = GetInvoice((long)invoice.PARENTID);
                if (invoice.INVOICETYPE == (int)InvoiceType.Substitute)
                {
                    if (invoiceParent.INVOICESTATUS != (int)InvoiceStatus.Cancel)
                    {
                        throw new BusinessLogicException(ResultCode.InvoiceNotInCanceledStatus, "Hóa đơn đang xử lý chưa được hủy. Bạn không thể phát hành hóa đơn này.");
                    }
                }
                else
                {
                    if (invoiceParent.INVOICESTATUS == (int)InvoiceStatus.Cancel)
                    {
                        throw new BusinessLogicException(ResultCode.InvoiceTypeChanged, "Hóa đơn đang xử lý đã bị thay đổi trạng thái điều chỉnh. Bạn không thể phát hành hóa đơn này.");
                    }
                }
            }
        }

        private RELEASEINVOICEDETAIL CreateReleaseDetailShinhan(INVOICE invoices, long ReleaseId)
        {
            ReleaseInvoiceInfo releaseInvoiceInfo = new ReleaseInvoiceInfo()
            {
                InvoiceId = invoices.ID,
                InvoiceNo = invoices.NO
            };
            return InsertReleaseInvoiceDetailShinhan(ReleaseId, invoices.COMPANYID, releaseInvoiceInfo);
        }
        private RELEASEINVOICEDETAIL InsertReleaseInvoiceDetailShinhan(long releaseInvoiceId, long companyId, ReleaseInvoiceInfo releaseDetail)
        {
            INVOICE currentInvoice = GetInvoice(releaseDetail.InvoiceId);
            if (currentInvoice.PARENTID != null && currentInvoice.INVOICESTATUS != (int)InvoiceStatus.Cancel)
            {
                var invoiceParent = GetInvoice((long)currentInvoice.PARENTID);
                if (currentInvoice.INVOICETYPE == (int)InvoiceType.Substitute)
                {
                    if (invoiceParent.INVOICESTATUS != (int)InvoiceStatus.Cancel)
                    {
                        throw new BusinessLogicException(ResultCode.InvoiceNotInCanceledStatus, "Hóa đơn đang xử lý chưa được hủy. Bạn không thể phát hành hóa đơn này.");
                    }
                }
                else
                {
                    if (invoiceParent.INVOICESTATUS == (int)InvoiceStatus.Cancel)
                    {
                        throw new BusinessLogicException(ResultCode.InvoiceTypeChanged, "Hóa đơn đang xử lý đã bị thay đổi trạng thái điều chỉnh. Bạn không thể phát hành hóa đơn này.");
                    }
                }
            }

            this.LogSymbols(currentInvoice, companyId);

            RELEASEINVOICEDETAIL releaseInvoiceDetail = new RELEASEINVOICEDETAIL();
            releaseInvoiceDetail.INVOICEID = currentInvoice.ID;
            releaseInvoiceDetail.RELEASEINVOICEID = releaseInvoiceId;
            releaseInvoiceDetail.MESSAGES = "Hệ thống đang phát hành hóa đơn";
            releaseInvoiceDetail.VERIFICATIONCODE = GetVerificationCode(currentInvoice.ID);
            return releaseInvoiceDetail;
        }

        private void LogSymbols(INVOICE currentInvoice, long companyId)
        {
            List<string> symbols = GetSymbolsInReportCancellingInvoice(companyId, (long)currentInvoice.REGISTERTEMPLATEID);
            if (symbols.Count > 0)
            {
                foreach (var item in symbols)
                {
                    if (currentInvoice.SYMBOL == item)
                    {
                        throw new BusinessLogicException(ResultCode.InvoiceNotApproveSymbol, "Dãy hóa đơn đã bị hủy,không thể thực hiện được chức năng phê duyệt/ký hóa đơn.");
                    }
                }
            }
        }

        private string GetVerificationCode(long invoiceId)
        {
            string verificationCode = string.Format("{0}{1}", DateTime.Now.ToString(), invoiceId).GetHashCode().ToString("x");
            return verificationCode.ToUpper();
        }
        //end job tạo file
        public ExportFileInfo DownloadStatistical(long id)
        {
            ExportFileInfo fileInfo = GetInvoiceStaticalFile(id);
            return fileInfo;
        }
        public ExportFileInfo DownloadGift(long id)
        {
            ExportFileInfo fileInfo = GetInvoiceGiftFile(id);
            return fileInfo;
        }
        public ExportFileInfo ResearchInvoice(string verificationCode)
        {
            INVOICE currentInvoice = GetInvoice(verificationCode);
            ExportFileInfo fileInfo = FilterInvoiceFile(currentInvoice);
            return fileInfo;
        }

        public InvoiceNo RegistInvoiceNo(long companyId, long templateId, string symbol)
        {
            string invoiceNo = GeneratorInvoiceNo(companyId, templateId, symbol);
            return new InvoiceNo(invoiceNo);
        }

        public IEnumerable<ReplacedInvoice> FilterReplacedInvoice(SearchReplacedInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var replacedInvoiceds = this.invoiceRepository.FilterReplacedInvoice(condition).ToList();
            return replacedInvoiceds;
        }

        public long CountReplacedInvoice(SearchReplacedInvoice condition)
        {
            return this.invoiceRepository.CountReplace(condition);
        }

        private List<string> GetSymbolsInReportCancellingInvoice(long companyId, long invoiceTemplateId)
        {
            IEnumerable<REPORTCANCELLINGDETAIL> reportCancelling = this.reportCancellingDetailRepository.Filter(companyId, invoiceTemplateId);
            return reportCancelling.Select(p => p.SYMBOL).ToList();

        }
        public Boolean CheckPermission(ApprovedInvoice approveInvoice, UserSessionInfo currentUser)
        {
            var Company = myCompanyRepository.GetById(currentUser.Company.Id.Value);
            foreach (var p in approveInvoice.ReleaseInvoiceInfos)
            {
                var currentInvoice = invoiceRepository.GetById(p.InvoiceId);
                if (Company.LEVELCUSTOMER != "HO" && currentInvoice.COMPANYID != currentUser.Company.Id.Value)
                {
                    return false;
                }
            }
            return true;
        }
        public ResultApproved ApproveInvoiceNew(ApprovedInvoice approveInvoice, UserSessionInfo currentUser)
        {
            ResultApproved result = new ResultApproved();
            if (approveInvoice == null || approveInvoice.ReleaseInvoiceInfos.Count == 0)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            foreach (var p in approveInvoice.ReleaseInvoiceInfos)
            {
                var currentInvoice = invoiceRepository.GetById(p.InvoiceId);
                if (currentInvoice.INVOICESTATUS == (int)InvoiceStatus.Approved)
                {
                    continue;
                }
                var checkApprove = this.invoiceRepository.Approve_Invoice(DateTime.Now.ToString("yyyyMMdd"), currentInvoice.COMPANYID, currentInvoice.SYMBOL.Substring(5, 2), currentInvoice.ID);
                if (checkApprove == false)
                {
                    result.RowError.Add(new ApprovedRowError(ResultCode.InvoiceApproveInvalidDay, currentInvoice.ID.ToString()));
                }
            }
            if (result.RowError.Count > 0)
            {
                result.ErrorCode = ResultCode.InvoiceApproveInvalidDay;
                result.Message = MsgApiResponse.DataInvalid;
            }
            return result;

        }
        public ResultApproved ApproveInvoice(ApprovedInvoice approveInvoice, UserSessionInfo currentUser)
        {
            ResultApproved result = new ResultApproved();
            //var Company = myCompanyRepository.GetById(currentUser.Company.Id.Value);
            //check job approved có đang chạy k
            var quarzt = this.quarztRespository.findByName("ApproveInvoice");
            if (quarzt.PROCESSING == true)
            {
                throw new BusinessLogicException(ResultCode.JobApprovedProcess, MsgApiResponse.DataInvalid);
            }
            if (approveInvoice == null || approveInvoice.ReleaseInvoiceInfos.Count == 0)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            //if (currentUser.SystemSetting.StepToCreateInvoiceNo == StepToCreateInvoiceNo.WhenApprove)
            //{
            //    if (!ValidDateSign(approveInvoice.ReleaseInvoiceInfos))
            //    {
            //        throw new BusinessLogicException(ResultCode.ReleaseDateInvalid, MsgApiResponse.DataInvalid);
            //    }
            //}

            foreach (var p in approveInvoice.ReleaseInvoiceInfos)
            {
                var currentInvoice = invoiceRepository.GetById(p.InvoiceId);
                if (currentInvoice.INVOICESTATUS == (int)InvoiceStatus.Approved)
                {
                    continue;
                }
                //this.invoiceRepository.Approve_Invoice(DateTime.Now.ToString("yyyyMMdd"), currentInvoice.COMPANYID, currentInvoice.SYMBOL.Substring(5, 2), currentInvoice.ID);
                var checkApprove = this.invoiceRepository.Approve_Invoice(DateTime.Now.ToString("yyyyMMdd"), currentInvoice.COMPANYID, currentInvoice.SYMBOL.Substring(5, 2), currentInvoice.ID);
                if (checkApprove == false)
                {
                    result.RowError.Add(new ApprovedRowError(ResultCode.InvoiceApproveInvalidDay, currentInvoice.ID.ToString()));
                }

            }
            if (result.RowError.Count > 0)
            {
                result.ErrorCode = ResultCode.InvoiceApproveInvalidDay;
                result.Message = MsgApiResponse.DataInvalid;
            }

            return result;
        }
        private void ApproveInvoiceItem(INVOICE currentInvoice, ApprovedInvoice approveInvoice, ResultApproved result, MYCOMPANY Company, bool isJob = false)
        {
            if (!this.ValidateErrorApproveInvoiceItem(result, currentInvoice))
            {
                return;
            }
            currentInvoice.INVOICESTATUS = (int)InvoiceStatus.Approved;
            currentInvoice.APPROVEDBY = approveInvoice.UserActionId;
            currentInvoice.APPROVEDDATE = DateTime.Now;
            currentInvoice.NO = this.releaseInvoiceBO.GeneratorInvoiceNoFromBD(currentInvoice.COMPANYID, currentInvoice.REGISTERTEMPLATEID.Value, currentInvoice.SYMBOL, currentInvoice.INVOICESTATUS);
            currentInvoice.INVOICENO = currentInvoice.NO.ToDecimal(0);

            currentInvoice.VERIFICATIONCODE = currentInvoice.INVOICENO + this.RandomVerificationCode();
            CLIENT client = this.clientRepository.GetById(currentInvoice.CLIENTID ?? 0);
            var oldInvoiceNo = currentInvoice.INVOICENO ?? 0;
            if ((currentInvoice.INVOICETYPE ?? 0) == 0 && oldInvoiceNo == 0) // enum InvoiceType, InvoiceNo = 0: chưa có số hóa đơn mới update theo client (Bug #29620)
            {
                if (!OtherExtensions.IsCurrentClient(client.CUSTOMERCODE))
                {
                    if (client.ISORG != true)
                    {
                        currentInvoice.PERSONCONTACT = client.PERSONCONTACT;
                    }
                    currentInvoice.CUSTOMERNAME = client.CUSTOMERNAME;
                    currentInvoice.CUSTOMERTAXCODE = client.TAXCODE;
                    currentInvoice.CUSTOMERADDRESS = client.ADDRESS;
                    currentInvoice.CUSTOMERBANKACC = client.BANKACCOUNT;
                }

                if (Company.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
                {
                    var parrentCompany = myCompanyRepository.GetById((long)Company.COMPANYID);
                    currentInvoice.COMPANYNAME = parrentCompany.COMPANYNAME;
                    currentInvoice.COMPANYADDRESS = parrentCompany.ADDRESS;
                    currentInvoice.COMPANYTAXCODE = parrentCompany.TAXCODE;
                }
                else
                {
                    var mycompany = myCompanyRepository.GetById(currentInvoice.COMPANYID);
                    currentInvoice.COMPANYNAME = mycompany.COMPANYNAME;
                    currentInvoice.COMPANYADDRESS = mycompany.ADDRESS;
                    currentInvoice.COMPANYTAXCODE = mycompany.TAXCODE;
                }
            }

            this.invoiceRepository.Update(currentInvoice);
        }
        public string RandomVerificationCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[5];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }

        /// <summary>
        /// Duyệt danh sách hóa đơn
        /// </summary>
        /// <param name="invoices"></param>
        /// <param name="Company"></param>
        public void JobApproveItemShinhan(List<INVOICE> objinvoices)
        {
            // tạo một hóa đơn release
            var invoiceConclude = CreateRelease();
            var invoices = this.invoiceRepository.GetByIds(objinvoices.Select(f => f.ID).ToList());

            var objCompanies = this.myCompanyRepository.GetByIds(objinvoices.Select(f => (long)f.COMPANYID).Distinct().ToList());

            // Lấy danh sách client
            var objClients = this.clientRepository.GetByIds(objinvoices.Select(f => f.CLIENTID).Distinct().ToList());

            // danh sách detail của realease
            List<RELEASEINVOICEDETAIL> releaseDetails = new List<RELEASEINVOICEDETAIL>();
            foreach (var invoice in invoices)
            {
                try
                {
                    MYCOMPANY Company = objCompanies.FirstOrDefault(f => f.COMPANYSID == invoice.COMPANYID);
                    var client = objClients.FirstOrDefault(f => f.ID == invoice.CLIENTID);
                    ApproveInvoiceItemShinhan(invoice, Company, client);

                    // Tạo mã hóa đơn
                    invoice.NO = this.releaseInvoiceBO.GeneratorInvoiceNoFromBD(invoice.COMPANYID, invoice.REGISTERTEMPLATEID.Value, invoice.SYMBOL.Replace("/", ""), invoice.INVOICESTATUS);

                    invoice.INVOICENO = invoice.NO.ToDecimal(0);
                    if (invoice.RELEASEDDATE == null)
                    {
                        invoice.RELEASEDDATE = DateTime.Now;
                    }

                    lock (ReleaseInvoiceLock.LockObject)
                    {
                        this.invoiceRepository.Update(invoice);
                    }

                    releaseDetails.Add(CreateReleaseDetailShinhan(invoice, invoiceConclude.ID));
                }
                catch (Exception ex)
                {
                    logger.Error("ApproveItem", ex);
                }

            }
            this.releaseInvoiceDetaiRepository.InsertMultipe(releaseDetails);
        }
        /// <summary>
        /// Duyệt hóa đơn
        /// </summary>
        /// <param name="item"></param>
        /// <param name="Company"></param>
        /// <param name="client"></param>
        private void ApproveInvoiceItemShinhan(INVOICE item, MYCOMPANY Company, CLIENT client)
        {
            //check approve permission
            item.INVOICESTATUS = (int)InvoiceStatus.Approved;
            item.APPROVEDBY = this.currentUser.Id;
            item.APPROVEDDATE = DateTime.Now;
            var oldInvoiceNo = item.INVOICENO ?? 0;
            if ((item.INVOICETYPE ?? 0) == 0 && oldInvoiceNo == 0) // enum InvoiceType, InvoiceNo = 0: chưa có số hóa đơn mới update theo client (Bug #29620)
            {
                if (!OtherExtensions.IsCurrentClient(client.CUSTOMERCODE))
                {
                    if (client.ISORG != true)
                    {
                        item.PERSONCONTACT = client.PERSONCONTACT;
                    }
                    item.CUSTOMERNAME = client.CUSTOMERNAME;
                    item.CUSTOMERTAXCODE = client.TAXCODE;
                    item.CUSTOMERADDRESS = client.ADDRESS;
                    item.CUSTOMERBANKACC = client.BANKACCOUNT;
                }

                if (Company.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
                {
                    //var parrentCompany = Company;
                    item.COMPANYNAME = Company.COMPANYNAME;
                    item.COMPANYADDRESS = Company.ADDRESS;
                    item.COMPANYTAXCODE = Company.TAXCODE;
                }
                else
                {
                    //var mycompany = myCompanyRepository.GetById(item.COMPANYID);
                    item.COMPANYNAME = Company.COMPANYNAME;
                    item.COMPANYADDRESS = Company.ADDRESS;
                    item.COMPANYTAXCODE = Company.TAXCODE;
                }
            }
        }
        private INVOICE GenInvoiceNoAfterSign(INVOICE currentInvoice)
        {
            if (currentUser.SystemSetting.StepToCreateInvoiceNo == StepToCreateInvoiceNo.WhenApprove)
            {
                currentInvoice.NO = this.releaseInvoiceBO.GeneratorInvoiceNoFromBD(currentInvoice.COMPANYID, currentInvoice.REGISTERTEMPLATEID.Value, currentInvoice.SYMBOL.Replace("/", ""), currentInvoice.INVOICESTATUS);
                if (currentInvoice.NO == DefaultFields.INVOICE_NO_DEFAULT_VALUE)
                {
                    currentInvoice.INVOICESTATUS = (int)InvoiceStatus.New;
                    throw new BusinessLogicException(ResultCode.NumberInvoiceOverloadRegister, "The number invoice is exceed number invoice register.");
                }
                currentInvoice.INVOICENO = currentInvoice.NO.ToDecimal(0);
                // BIDC thì đã có ngày hóa đơn sẵn
                if (!currentInvoice.RELEASEDDATE.HasValue)
                {
                    currentInvoice.RELEASEDDATE = DateTime.Now;
                }
            }
            return currentInvoice;
        }
        private void ValidateExeptionApproveInvoiceItem(INVOICE currentInvoice, ApprovedInvoice approveInvoice, MYCOMPANY Company)
        {
            //check approve permission
            if (Company.LEVELCUSTOMER != "HO" && currentInvoice.COMPANYID != currentUser.Company.Id.Value)
            {
                throw new BusinessLogicException(ResultCode.InvoiceHaveNoPermissionData, "Không có quyền duyệt hóa đơn");
            }

            //check err total number invoice notice with number invoice of contract
            var currentSymbol = currentInvoice.SYMBOL.Replace("/", "");
            double nextInvoiceNo = GetNextInvoiceNo(currentInvoice.COMPANYID, currentInvoice.REGISTERTEMPLATEID ?? 0, currentSymbol);
            //double maxInvoiceNoCompanyRegister = GetMaxNumberInvoiceCompanyRegister(currentInvoice.COMPANYID, currentInvoice.REGISTERTEMPLATEID ?? 0, currentSymbol);
            //if (nextInvoiceNo > maxInvoiceNoCompanyRegister)
            //{
            //    throw new BusinessLogicException(ResultCode.NumberInvoiceOverloadRegister, string.Format("The number invoice no [{0}] is exceed number invoice register ", nextInvoiceNo));
            //}

            if (currentInvoice.PARENTID != null)
            {
                var invoiceParent = GetInvoice((long)currentInvoice.PARENTID);
                if (currentInvoice.INVOICETYPE == (int)InvoiceType.Substitute)
                {
                    if (invoiceParent.INVOICESTATUS != (int)InvoiceStatus.Cancel)
                    {
                        throw new BusinessLogicException(ResultCode.InvoiceNotInCanceledStatus, "Hóa đơn đang xử lý chưa được hủy. Bạn không thể phát hành hóa đơn này.");
                    }
                }
                else
                {
                    if (invoiceParent.INVOICESTATUS == (int)InvoiceStatus.Cancel)
                    {
                        throw new BusinessLogicException(ResultCode.InvoiceTypeChanged, "Hóa đơn đang xử lý đã bị thay đổi trạng thái điều chỉnh. Bạn không thể phát hành hóa đơn này.");
                    }
                }
            }

            this.LogSymbols(currentInvoice, approveInvoice.CompanyId);
        }
        private bool ValidateErrorApproveInvoiceItem(ResultApproved result, INVOICE currentInvoice)
        {

            if (!currentInvoice.CLIENTID.HasValue)
            {
                result.RowError.Add(new ApprovedRowError(ResultCode.InvoiceApprovedNotHasCustomer, currentInvoice.NO));
                return false;
            }
            if (currentInvoice.INVOICEDETAILs.Count == 0)
            {
                result.RowError.Add(new ApprovedRowError(ResultCode.InvoiceApprovedNotHasItem, currentInvoice.NO));
                return false;
            }
            return true;
        }
        private bool ValidDateSign(List<ReleaseInvoiceInfo> ReleaseInvoiceInfos, bool isJob = false)
        {

            ReleaseInvoiceInfo invoicesReleaseMinDate = isJob ? ReleaseInvoiceInfos.Where(p => p.DateRelease != null).OrderBy(p => p.DateRelease).First() : ReleaseInvoiceInfos.Where(p => p.ReleaseDate != null).OrderBy(p => p.ReleaseDate).First();
            DateTime dateTime = isJob ? (invoicesReleaseMinDate.DateRelease ?? DateTime.Now) : (invoicesReleaseMinDate.ReleaseDate.DecodeUrl().ConvertDateTime() ?? DateTime.Now);// DateTime.Parse(invoicesReleaseMinDate.ReleaseDate);
            var InvoiceNotRelease = this.invoiceRepository.GetInvoiceNotReleaseForApprove(dateTime);
            if (InvoiceNotRelease.Count() > 0)
            {
                foreach (var inv in InvoiceNotRelease)
                {
                    JobLog.AddFail(inv.ID, inv.NO, inv.RELEASEDDATE.Value.ToString("yyyy/MM/dd"), "Ngày hóa đơn không hợp lệ");
                }
                return false;
            }
            return true;
        }
        public ResultApproved JobApproveInvoice(ApprovedInvoice approveInvoice, UserSessionInfo currentUser)
        {
            ResultApproved result = new ResultApproved();
            var Company = myCompanyRepository.GetById(currentUser.Company.Id.Value);

            if (approveInvoice == null || approveInvoice.ReleaseInvoiceInfos.Count == 0)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            if (currentUser.SystemSetting.StepToCreateInvoiceNo == StepToCreateInvoiceNo.WhenApprove)
            {
                if (!ValidDateSign(approveInvoice.ReleaseInvoiceInfos, true))
                {
                    logger.Error("ApprovedInvoice", new Exception("Can not approved invoice, because the invoice date is invalid"));
                    result.ErrorCode = ResultCode.ImportFileFormatInvalid;
                    result.Message = MsgApiResponse.DataInvalid;
                    return result;
                }
            }
            RELEASEINVOICE invoiceConclude = new RELEASEINVOICE();
            invoiceConclude = CreateRelease();

            foreach (var p in approveInvoice.ReleaseInvoiceInfos)
            {
                try
                {
                    var currentInvoice = invoiceRepository.GetById(p.InvoiceId);
                    if (currentInvoice.INVOICESTATUS == (int)InvoiceStatus.Approved)
                    {
                        continue;
                    }
                    // BIDC thì chỉ approve tự động khi hóa đơn được import từ job
                    ApproveInvoiceItem(currentInvoice, approveInvoice, result, Company, true);
                    INVOICE invoice = GetInvoice(p.InvoiceId);
                    CreateReleaseDetail(invoice, invoiceConclude.ID);
                    GenInvoiceNoCreateFile(invoice.ID);
                }
                catch (Exception ex)
                {
                    logger.Error(this.currentUser.UserId, ex);
                }
            }
            if (result.RowError.Count > 0)
            {
                result.ErrorCode = ResultCode.ImportFileFormatInvalid;
                result.Message = MsgApiResponse.DataInvalid;
            }

            return result;
        }
        public SendEmailVerificationCode GetInvoiceByVerificationCode(InvoiceVerificationCode VerificationCodeInfo)
        {
            return this.invoiceRepository.GetVerificationCodeInfomation(VerificationCodeInfo);
        }
        public long CreateVerificationCode(InvoiceVerificationCode invoiceVerificationCode)
        {
            if (invoiceVerificationCode == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long result = 0;
            try
            {
                transaction.BeginTransaction();
                SendEmailVerificationCode emailVerificationCodeInfo = this.invoiceRepository.GetVerificationCodeInfomation(invoiceVerificationCode);
                if (!emailVerificationCodeInfo.IsOrg)
                {
                    emailVerificationCodeInfo.CustomerName = emailVerificationCodeInfo.PersonContact;
                }
                emailVerificationCodeInfo.EmailTo = invoiceVerificationCode.Email;
                emailVerificationCodeInfo.NumberFormat = invoiceVerificationCode.NumberFormat;
                emailVerificationCodeInfo.SystemSetting = this.currentUser.SystemSetting;
                result = CreateEmailActive(invoiceVerificationCode.CompanyId, emailVerificationCodeInfo);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return result;
        }
        public SendEmailVerificationCode GetEmailVerificationCodeInfo(InvoiceVerificationCode invoiceVerificationCode)
        {
            if (invoiceVerificationCode == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            SendEmailVerificationCode emailVerificationCodeInfo = this.invoiceRepository.GetVerificationCodeInfomation(invoiceVerificationCode);
            if (!emailVerificationCodeInfo.IsOrg)
            {
                emailVerificationCodeInfo.CustomerName = emailVerificationCodeInfo.PersonContact;
            }
            emailVerificationCodeInfo.EmailTo = invoiceVerificationCode.Email;
            emailVerificationCodeInfo.NumberFormat = invoiceVerificationCode.NumberFormat;
            emailVerificationCodeInfo.SystemSetting = this.currentUser.SystemSetting;
            return emailVerificationCodeInfo;
        }

        public long CreateVerificationCodeForSendMonth(InvoiceVerificationCode invoiceVerificationCode, CLIENT client, List<string> listCode)
        {
            if (invoiceVerificationCode == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            long result = 0;
            try
            {
                transaction.BeginTransaction();
                List<InvoiceSendByMonth> invoiceSends = new List<InvoiceSendByMonth>();
                foreach (var item in listCode)
                {
                    SendEmailVerificationCode emailVerificationCodeInfo = this.invoiceRepository.GetVerificationCodeInfomation(item, invoiceVerificationCode.CompanyId);
                    invoiceSends.Add(new InvoiceSendByMonth()
                    {
                        InvoiceTemplate = emailVerificationCodeInfo.InvoiceCode,
                        InvoiceNo = emailVerificationCodeInfo.InvoiceNo,
                        InvoiceSymbol = emailVerificationCodeInfo.Symbol,
                        InvoiceDate = emailVerificationCodeInfo.InvoiceDate.ToString("dd/MM/yyyy"),
                        VerificationCode = item,
                        InvoiceId = emailVerificationCodeInfo.InvoiceId,
                    });
                }
                result = CreateEmailActiveForSendMonth(client, invoiceSends);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return result;
        }
        public List<SendEmailVerificationCode> GetVerificationCodeForSendMonth(InvoiceVerificationCode invoiceVerificationCode, CLIENT client, List<string> listCode)
        {
            if (invoiceVerificationCode == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            List<SendEmailVerificationCode> listEmailVerificationCode = new List<SendEmailVerificationCode>();
            try
            {
                foreach (var item in listCode)
                {
                    SendEmailVerificationCode emailVerificationCodeInfo = this.invoiceRepository.GetVerificationCodeInfomation(item, invoiceVerificationCode.CompanyId);
                    listEmailVerificationCode.Add(emailVerificationCodeInfo);
                }
            }
            catch (Exception ex)
            {
                logger.Error(this.currentUser.UserId, ex);
            }

            return listEmailVerificationCode;
        }
        public long CreateInvoiceEmailActiveSendByMonth(InvoiceVerificationCode invoiceVerificationCode, CLIENT client, List<string> listCode)
        {
            long result = 0;
            try
            {
                List<InvoiceSendByMonth> invoiceSends = new List<InvoiceSendByMonth>();
                foreach (var item in listCode)
                {
                    SendEmailVerificationCode emailVerificationCodeInfo = this.invoiceRepository.GetVerificationCodeInfomation(item, invoiceVerificationCode.CompanyId);
                    invoiceSends.Add(new InvoiceSendByMonth()
                    {
                        InvoiceTemplate = emailVerificationCodeInfo.InvoiceCode,
                        InvoiceNo = emailVerificationCodeInfo.InvoiceNo,
                        InvoiceSymbol = emailVerificationCodeInfo.Symbol,
                        InvoiceDate = emailVerificationCodeInfo.InvoiceDate.ToString("dd/MM/yyyy"),
                        VerificationCode = item,
                        InvoiceId = emailVerificationCodeInfo.InvoiceId
                    });
                }
                result = CreateEmailActiveForSendMonth(client, invoiceSends);
            }
            catch (Exception ex)
            {
                logger.Error(this.currentUser.UserId, ex);
            }
            return result;
        }
        private long CreateEmailActiveForSendMonth(CLIENT client, List<InvoiceSendByMonth> invoiceSends)
        {
            EmailInfo emailInfo = EmailTemplate.GetEmail(this.emailConfig, (int)SendEmailType.SendVerificationCodeSendMonth, client, invoiceSends);
            emailInfo.CompanyId = invoiceRepository.GetById(invoiceSends[0].InvoiceId)?.COMPANYID ?? 0;
            long emailActiveId = this.email.CreateEmailActive(emailInfo, (int)TypeEmailActive.MonthlyEmail);
            InsertFileAttachSendByMonth(client.CUSTOMERCODE, emailActiveId);
            return emailActiveId;
        }
        public long CreateEmailActive(long companyId, SendEmailVerificationCode receiverInfo)
        {
            var typeSendMail = (int)SendEmailType.SendVerificationCode;
            EmailInfo emailInfo = EmailTemplate.GetEmail(this.emailConfig, typeSendMail, receiverInfo);
            emailInfo.CompanyId = receiverInfo.CompanyId;
            long emailActiveId = this.email.CreateEmailActive(emailInfo, (int)TypeEmailActive.DailyEmail, receiverInfo.CompanyId);
            //InsertFileAttachByInvoice(receiverInfo.InvoiceId, receiverInfo.CompanyId, emailActiveId);
            return emailActiveId;
        }

        public void UpdateStatusEmailActive(List<SendEmailVerificationCode> emailVerificationCodes)
        {
            emailVerificationCodes.ForEach(p =>
            {
                this.email.UpdateStatusEmail(p.EmailActiveId);
            });
        }
        public ExportFileInfo SwitchInvoice(long id)
        {
            //RELEASEINVOICEDETAIL releaseInvoiceDetail = GetReleaseInvoiceDetail(id);
            //if (releaseInvoiceDetail.CONVERTED.HasValue && releaseInvoiceDetail.CONVERTED.Value)
            //{
            //   
            //}

            INVOICE invoice = GetInvoice(id);
            if (invoice.CONVERTED.HasValue && invoice.CONVERTED.Value)
            {
                throw new BusinessLogicException(ResultCode.InvoiceIsConverted, string.Format("This invoice Id [{0}] converted", id));
            }

            ExportFileInfo fileInfo = FilterInvoiceFile(invoice);
            if (fileInfo == null || !File.Exists(fileInfo.FullPathFileName))
            {
                fileInfo = ExportInvoiceToPdf(id, true);
            }
            else
            {
                AddFileSwitch(fileInfo);
            }

            SetInvoiceConverted(id);
            invoice.UPDATEDDATE = DateTime.Now;
            this.invoiceRepository.Update(invoice);
            return fileInfo;
        }
        public void UpdateStatusSendMail(InvoiceVerificationCode invoiceVerificationCode)
        {
            SendEmailVerificationCode emailVerificationCodeInfo = this.invoiceRepository.GetVerificationCodeInfomation(invoiceVerificationCode);
            INVOICE current = this.invoiceRepository.GetById(emailVerificationCodeInfo.InvoiceId);
            current.SENDMAILSTATUS = true;
            this.invoiceRepository.Update(current);
        }
        public INVOICE GetCurrentInvoice(long id, long companyId)
        {
            return this.invoiceRepository.GetById(id, companyId);
        }
        public InvoiceInfo GetById(long id)
        {
            var invoice = this.invoiceRepository.GetById(id);
            var invoiceInformation = new InvoiceInfo(invoice);
            return invoiceInformation;
        }
        public IEnumerable<InvoiceMaster> FilterInvoice(ConditionSearchInvoiceOfClient condition, int skip = 0, int take = int.MaxValue)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceMasters = this.invoiceRepository.FilterInvoiceOfClient(condition).Skip(skip).Take(take).ToList();
            foreach (var item in invoiceMasters)
            {
                if (item.InvoiceStatus.Value == (int)InvoiceStatus.Cancel)
                {
                    item.IsClientSign = true;
                }
            }

            return invoiceMasters;
        }

        public long CountInvoice(ConditionSearchInvoiceOfClient condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.invoiceRepository.FilterInvoiceOfClient(condition).Count();
        }

        public ExportFileInfo ExportInvoiceConverted(ConditionSearchInvoice condition)
        {
            var invoiceMasteres = invoiceRepository.FillterInvoiceConverted(condition);
            var report = new ReportInvoiceConvertedBoard();
            var listReportInvoiceConverted = invoiceMasteres.Select(i => new ReportInvoiceConverted()
            {
                InvoiceCode = i.InvoiceCode,
                InvoiceSymbol = i.InvoiceSymbol,
                CustomerCode = i.CustomerCode,
                CustomerName = i.CustomerName ?? i.PersonContact,
                PersonContact = i.PersonContact,
                InvoiceNo = i.InvoiceNo,
                InvoiceDate = i.InvoiceDate,
            });
            var tmpListInvoiceConverted = listReportInvoiceConverted.OrderBy(x => x.InvoiceNo).ToList();
            foreach (var item in tmpListInvoiceConverted)
            {
                if (item.CustomerName == "")
                {
                    item.CustomerName = item.PersonContact;
                }
            }
            report.items = tmpListInvoiceConverted;
            var CurrentCompany = this.myCompanyRepository.GetById(condition.Branch ?? 0);
            var company = condition.CurrentUser.Company.DeepCopy();
            company.CompanyName = CurrentCompany.COMPANYNAME;
            company.TaxCode = CurrentCompany.TAXCODE;
            report.Company = company;
            ReportInvoiceConvertedView export = new ReportInvoiceConvertedView(report, config, condition.Language);
            return export.ExportFile();
        }
        public List<InvoiceGift> ReadGiftFile(string pathFile)
        {
            var rowError = new ListError<ImportRowError>();
            ImportExcel importExcel = new ImportExcel(pathFile, GiftFile.SheetNames);
            DataTable dtGift = importExcel.GetBySheetName(GiftFile.SheetName, GiftFile.ColumnImport, ref rowError);
            var result = (from rw in dtGift.AsEnumerable()
                          select new InvoiceGift()
                          {
                              CompanyId = Convert.ToInt32(rw["BranchID"]),
                              TradDate = Convert.ToString(rw["NgayHachToan"]),
                              RefNo = Convert.ToString(rw["SoRef"]),
                              Sequence = Convert.ToString(rw["Sequence"]),
                              PeriodTax = Convert.ToString(rw["KyThue"]),
                              GiftName = Convert.ToString(rw["TenQuaTang"]),
                              Remark = Convert.ToString(rw["Remark"]),
                              Currency = Convert.ToString(rw["DonViTien"]),
                              Amount = Convert.ToDecimal(rw["TienTruocThue"]),
                              Tax = Convert.ToString(rw["ThueSuat"]),
                              TaxAmount = Convert.ToDecimal(rw["ThueGTGT"]),
                              TypeGift = Convert.ToString(rw["PhanLoaiQuaTang"])
                          }).ToList();
            return result;
        }

        public List<InvoiceDetailInfo> ReadDetailFile(string pathFile)
        {
            var rowError = new ListError<ImportRowError>();
            ImportExcel importExcel = new ImportExcel(pathFile, DetailFile.SheetNames);
            DataTable dtDetailInvoice = importExcel.GetBySheetName(DetailFile.SheetName, DetailFile.ColumnImport, ref rowError);
            if (dtDetailInvoice == null)
            {
                throw new BusinessLogicException(ResultCode.ImportDataIsNotTemplate, "File upload chưa đúng template");
            }
            var result = new List<InvoiceDetailInfo>();
            int startIndex = 0;
            foreach (DataRow row in dtDetailInvoice.AsEnumerable())
            {
                startIndex++;
                if (startIndex > 1)
                {
                    result.Add(new InvoiceDetailInfo
                    {
                        ProductName = Convert.ToString(row["Nội dung chi phí"]),
                        Total = Convert.ToDecimal(row["Số tiền"])
                    });
                }
            }

            //var result = (from rw in dtDetailInvoice.AsEnumerable()
            //              select new InvoiceDetailInfo()
            //              {
            //                  ProductName = Convert.ToString(rw["HangHoaDichVu"]+1),
            //                 

            //              }).ToList();
            return result;
        }
        public bool Verify(string file)
        {
            // Create a new XML document.
            XmlDocument xmlDocument = new XmlDocument();
            // Format using white spaces.
            xmlDocument.PreserveWhitespace = true;
            // Load the passed XML file into the document. 
            xmlDocument.Load(file);
            // Create a new SignedXml object and pass it
            // the XML document class.
            SignedXml signedXml = new SignedXml(xmlDocument);
            string cert = String.Empty;
            XmlNodeList nodes = xmlDocument.GetElementsByTagName("X509Data");
            foreach (XmlNode node in nodes)
            {
                cert = node["X509Certificate"].InnerText;
            }
            byte[] data = Convert.FromBase64String(cert);
            var x509 = new X509Certificate2(data);
            // Find the "Signature" node and create a new
            // XmlNodeList object.
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature");
            // Load the signature node.
            signedXml.LoadXml((XmlElement)nodeList[0]);
            RSA publicKey = x509.PublicKey.Key as RSA;
            return signedXml.CheckSignature(publicKey);
        }

        #endregion

        #region Private Methods

        private ResultCode DeleteInvoice(long id)
        {
            INVOICE invoice = GetInvoice(id);
            List<int> invoiceStatusCanDelete = new List<int>() { (int)InvoiceStatus.New, (int)InvoiceStatus.Approved };
            if (!invoiceStatusCanDelete.Contains((invoice.INVOICESTATUS ?? 0)))
            {
                throw new BusinessLogicException(ResultCode.InvoiceReleasedNotDelete, "This Invoice released can not delete");
            }
            invoice.DELETED = true;
            var parentId = invoice.PARENTID;
            invoice.PARENTID = null;
            invoice.NOTE = $"ParentId: {parentId}";
            return this.invoiceRepository.Update(invoice) ? ResultCode.NoError : ResultCode.UnknownError;
        }
        private INVOICE InsertInvoice(InvoiceInfo info)
        {
            if (info == null || info.Symbol.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!info.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            INVOICE invoice = new INVOICE();
            // truong hop la PGD thi get thong tin cua Chi nhanh de Update
            if (info.TaxId == 1)
            {
                info.Report_Class = "4-VAT FEE ZERO";
            }
            else if (info.TaxId == 2 || info.TaxId == 3)
            {
                info.Report_Class = "1-VAT FEE";
            }
            else
            {
                info.Report_Class = "5-NON VAT FEE";
            }

            info.Vat_invoice_type = info.VatInvoiceType;
            updateCompanyInfo(info);
            invoice.CopyData(info);
            invoice.RELEASEDDATE = info.ReleasedDate;
            invoice.NO = info.No;
            invoice.INVOICENO = invoice.NO.ToDecimal(0);
            invoice.INVOICESTATUS = (int)InvoiceStatus.New;
            invoice.CREATEDDATE = info.Created.HasValue ? info.Created.Value : DateTime.Now;
            invoice.CREATEDBY = info.UserAction;
            invoice.IMPORTTYPE = 2;//Cột nguồn tạo hóa đơn : 2: Tạo tay
            this.invoiceRepository.Insert(invoice);
            return invoice;
        }

        private void updateCompanyInfo(InvoiceInfo info)
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            MYCOMPANY company = myCompanyRepository.GetById((long)info.CompanyId);
            if (company.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
            {
                MYCOMPANY parentCompany = myCompanyRepository.GetById((long)company.COMPANYID);
                info.CompanyAddress = parentCompany.ADDRESS;
                info.CompanyNameInvoice = parentCompany.COMPANYNAME;
            }
        }

        private INVOICE InsertInvoiceSubstitute(InvoiceInfo info, int invoiceStatus)
        {
            if (info == null || info.Symbol.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!info.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            INVOICE invoice = new INVOICE();
            var company = this.myCompanyRepository.GetById(info.CompanyId ?? 0);

            if (company == null)
            {
                bool parseSuccess = int.TryParse(info.ParentId, out int parentId);
                if (parseSuccess)
                {
                    var parentInvoice = this.invoiceRepository.GetById(parentId);
                    invoice.COMPANYADDRESS = parentInvoice.COMPANYADDRESS;
                    invoice.COMPANYTAXCODE = parentInvoice.COMPANYTAXCODE;
                    invoice.COMPANYNAME = parentInvoice.COMPANYNAME;
                }
            }
            else
            {
                invoice.COMPANYADDRESS = company.ADDRESS;
                invoice.COMPANYTAXCODE = company.TAXCODE;
                invoice.COMPANYNAME = company.COMPANYNAME;
            }
            if (info.TaxId == 1)
            {
                info.Report_Class = "4-VAT FEE ZERO";
            }
            else if (info.TaxId == 2 || info.TaxId == 3)
            {
                info.Report_Class = "1-VAT FEE";
            }
            else
            {
                info.Report_Class = "5-NON VAT FEE";
            }

            info.Vat_invoice_type = info.VatInvoiceType;

            if (info.InvoiceType == 6)
            {
                info.Total = 0;
                info.TotalTax = 0;
                info.Sum = 0;
            }

            invoice.CopyData(info);
            invoice.NO = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
            invoice.INVOICENO = invoice.NO.ToDecimal(0);
            //invoice.RELEASEDDATE = GetPreviousWorkingDate(DateTime.Today);
            invoice.RELEASEDDATE = info.ReleasedDate;
            invoice.NO = info.No;
            invoice.INVOICENO = invoice.NO.ToDecimal(0);
            invoice.INVOICESTATUS = (int)InvoiceStatus.New;
            invoice.CREATEDDATE = info.Created.HasValue ? info.Created.Value : DateTime.Now;
            invoice.CREATEDBY = info.UserAction;
            invoice.IMPORTTYPE = 2;//Cột nguồn tạo hóa đơn : 2: Tạo tay
            this.InsertInvoiceSubstituteCopyMoreData(invoice, info, invoiceStatus);
            this.invoiceRepository.Insert(invoice);
            return invoice;
        }


        private INVOICE InsertInvoiceSubstitute(InvoiceInfo info, int invoiceStatus, DateTime? releaseddate)
        {
            if (info == null || info.Symbol.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!info.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            INVOICE invoice = new INVOICE();
            var company = this.myCompanyRepository.GetById(info.CompanyId ?? 0);

            if (company == null)
            {
                bool parseSuccess = int.TryParse(info.ParentId, out int parentId);
                if (parseSuccess)
                {
                    var parentInvoice = this.invoiceRepository.GetById(parentId);
                    invoice.COMPANYADDRESS = parentInvoice.COMPANYADDRESS;
                    invoice.COMPANYTAXCODE = parentInvoice.COMPANYTAXCODE;
                    invoice.COMPANYNAME = parentInvoice.COMPANYNAME;
                }
            }
            else
            {
                invoice.COMPANYADDRESS = company.ADDRESS;
                invoice.COMPANYTAXCODE = company.TAXCODE;
                invoice.COMPANYNAME = company.COMPANYNAME;
            }

            invoice.CopyData(info);
            invoice.NO = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
            invoice.INVOICENO = invoice.NO.ToDecimal(0);
            invoice.RELEASEDDATE = releaseddate;
            this.InsertInvoiceSubstituteCopyMoreData(invoice, info, invoiceStatus);
            this.invoiceRepository.Insert(invoice);
            return invoice;
        }

        private void InsertInvoiceSubstituteCopyMoreData(INVOICE invoice, InvoiceInfo info, int invoiceStatus)
        {
            invoice.ORIGINNO = info.OriginNo;
            invoice.PARENTCODE = info.ParentCode;
            invoice.PARENTSYMBOL = info.ParentSymbol;
            invoice.PARENTID = info.Id;
            invoice.INVOICETYPE = info.InvoiceType;
            invoice.INVOICESTATUS = invoiceStatus;
            invoice.CREATEDDATE = DateTime.Now;
            invoice.CREATEDBY = info.UserAction;
            invoice.FILERECEIPT = info.FileReceipt;
        }

        private void UpdateInvoiceClient(InvoiceInfo info)
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            INVOICE currenInvoice = GetInvoice(info.Id);
            CLIENT client = this.clientRepository.GetById((long)currenInvoice.CLIENTID);
            if (client.ID == info.ClientId)
            {
                client.UPDATEDDATE = DateTime.Now;
                client.CUSTOMERNAME = info.CompanyName;
                client.ADDRESS = info.Address;
                if (client.ISORG == false)
                {
                    client.PERSONCONTACT = info.CustomerName;
                }
                this.clientRepository.Update(client);
                info.ClientId = client.ID;
            }
            else
            {
                CLIENT orderClient = this.clientRepository.GetById((long)info.ClientId);
                orderClient.UPDATEDDATE = DateTime.Now;
                orderClient.CUSTOMERNAME = info.CompanyName;
                orderClient.ADDRESS = info.Address;
                if (orderClient.ISORG == false)
                {
                    orderClient.PERSONCONTACT = info.CustomerName;
                }
                this.clientRepository.Update(orderClient);
                info.ClientId = orderClient.ID;
            }
            currenInvoice.CLIENTID = info.ClientId;
            currenInvoice.CUSTOMERNAME = info.CustomerName;
            this.invoiceRepository.Update(currenInvoice);

        }
        private void UpdateFileReceipt(InvoiceInfo info)
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            INVOICE currenInvoice = GetInvoice(info.Id);
            currenInvoice.FILERECEIPT = info.FileReceipt;
            this.invoiceRepository.Update(currenInvoice);
        }
        private void UpdateInvoice(long id, InvoiceInfo info, int change)
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

            INVOICE currenInvoice = GetInvoice(id);
            info.CompanyNameInvoice = info.InvoiceType == (int)InvoiceType.AdjustmentInfomation ? currenInvoice.COMPANYNAME : this.currentUser.Company.CompanyName;
            info.CompanyAddress = info.InvoiceType == (int)InvoiceType.AdjustmentInfomation ? currenInvoice.COMPANYADDRESS : this.currentUser.Company.Address;
            info.CompanyTaxCode = info.InvoiceType == (int)InvoiceType.AdjustmentInfomation ? currenInvoice.COMPANYTAXCODE : this.currentUser.Company.TaxCode;
            if (info.InvoiceType != (int)InvoiceType.AdjustmentInfomation)
            {
                // truong hop la PGD thi get thong tin cua Chi nhanh de Update
                updateCompanyInfo(info);
            }
            currenInvoice.CopyData(info);
            currenInvoice.CREATEDDATE = info.Created.HasValue ? info.Created.Value : DateTime.Now;
            currenInvoice.UPDATEDDATE = DateTime.Now;
            currenInvoice.UPDATEDBY = info.UserAction;
            currenInvoice.FILERECEIPT = info.FileReceipt;
            currenInvoice.MESSAGECODE = null;
            this.invoiceRepository.Update(currenInvoice);
            //change = 1 la thay the, =2 la update hoa don
            if (change == 1)
            {
                this.UpdateAnnouncementStatus(id, info.CompanyId.Value, info.UserAction);
            }
        }

        public INVOICE GetInvoice(long id)
        {
            INVOICE invoices = this.invoiceRepository.GetById(id);
            if (invoices == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This InvoiceId [{0}] not found in data of client", id));
            }

            return invoices;
        }

        private void InsertInvoiDetail(InvoiceInfo invoice, List<InvoiceDetailInfo> invoiceDetails)
        {
            ResultCode errorCode;
            string errorMessage;
            invoiceDetails.ForEach(p =>
            {
                if (!p.IsValid(out errorCode, out errorMessage))
                {
                    throw new BusinessLogicException(errorCode, errorMessage);
                }

                INVOICEDETAIL invoiceDetail = new INVOICEDETAIL();
                p.Quantity = 1;
                p.Price = p.Total;
                invoiceDetail.CopyData(p);
                PRODUCT product = ProcessProduct(invoice, p);
                invoiceDetail.PRODUCTID = product.ID;
                if (invoiceDetail.PRODUCTID == 0)
                {
                    invoiceDetail.PRODUCTID = null;
                }
                invoiceDetail.INVOICEID = invoice.Id;
                if (invoiceDetail.TAXID == 0)
                {
                    invoiceDetail.TAXID = null;
                }
                if (invoice.InvoiceType == 6)
                {
                    invoiceDetail.PRICE = 0;
                    invoiceDetail.TOTAL = 0;
                    invoiceDetail.AMOUNTTAX = 0;
                    invoiceDetail.SUM = 0;
                    invoiceDetail.TAXID = 1;
                }
                this.invoiceDetailRepository.Insert(invoiceDetail);
            });
        }

        private void DeleteInvoiceDetail(long invoiceId)
        {
            IEnumerable<INVOICEDETAIL> invoiceDetails = this.invoiceDetailRepository.FilterInvoiceDetail(invoiceId).ToList();
            invoiceDetails.ForEach(p =>
            {
                this.invoiceDetailRepository.Delete(p);
            });
        }
        private void DeleteInvoiceStatistical(long invoiceId)
        {
            IEnumerable<STATISTICAL> statistical = this.invoiceStatisticalRepository.FilterInvoiceStatistical(invoiceId).ToList();
            statistical.ForEach(p =>
            {
                this.invoiceStatisticalRepository.Delete(p);
            });
        }
        private void DeleteInvoiceGift(long invoiceId)
        {
            IEnumerable<INVOICEGIFT> gift = this.invoiceGiftRepository.FilterInvoiceGift(invoiceId).ToList();
            gift.ForEach(p =>
            {
                this.invoiceGiftRepository.Delete(p);
            });
        }
        private string GeneratorInvoiceNo(long companyId, long templateId, string symbol)
        {

            string numberFromUse = GetNumberForm(companyId, templateId, symbol);
            double nextInvoiceNo = GetNextInvoiceNo(companyId, templateId, symbol);
            double lastInvoiceNo = UserSessionCache.Instance.GetMaxInvoiceNo(companyId, templateId, symbol);
            if (nextInvoiceNo <= lastInvoiceNo)
            {
                nextInvoiceNo = lastInvoiceNo + 1;
            }

            UserSessionCache.Instance.SaveMaxInvoiceNo(companyId, templateId, symbol, nextInvoiceNo);

            return nextInvoiceNo.ToString().PadLeft(numberFromUse.Length, '0');
        }
        private string GeneratorInvoiceNoFromBD(long companyId, long templateId, string symbol)
        {

            string numberFromUse = GetNumberForm(companyId, templateId, symbol);
            double nextInvoiceNo = GetNextInvoiceNo(companyId, templateId, symbol);
            //double maxInvoiceNoCompanyRegister = GetMaxNumberInvoiceCompanyRegister(companyId, templateId, symbol);
            //if (nextInvoiceNo > maxInvoiceNoCompanyRegister)
            //{
            //    throw new BusinessLogicException(ResultCode.NumberInvoiceOverloadRegister, string.Format("The number invoice no [{0}] is exceed number invoice register ", nextInvoiceNo));
            //}

            UserSessionCache.Instance.SaveMaxInvoiceNo(companyId, templateId, symbol, nextInvoiceNo);
            return nextInvoiceNo.ToString().PadLeft(numberFromUse.Length, '0');
        }

        private double GetNextInvoiceNo(long companyId, long templateid, string symbol)
        {
            lock (lockObject)
            {
                double maxInvoiceNo = 0;
                var maxInvoice = GetMaxInvoice(companyId, templateid, symbol);
                double.TryParse(maxInvoice, out maxInvoiceNo);
                return ++maxInvoiceNo;
            }
        }

        private string GetMaxInvoice(long companyId, long templateid, string symbol)
        {
            var invoicesNo = this.invoiceRepository.GetMaxInvoiceNo(companyId, templateid, symbol);

            string maxInvoice = "0";
            if (invoicesNo != null && invoicesNo.IsNotNullOrEmpty())
            {
                maxInvoice = invoicesNo;
            }

            return maxInvoice;
        }

        private string GetNumberForm(long companyId, long registerTemplateId, string symbol)
        {
            string numberFrom = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
            NOTIFICATIONUSEINVOICEDETAIL noticeInvoice = GetNotificationUseInvoiceDetail(companyId, registerTemplateId, symbol);
            if (noticeInvoice != null && noticeInvoice.NUMBERFROM.IsNullOrEmpty())
            {
                numberFrom = noticeInvoice.NUMBERFROM;
            }

            return numberFrom;
        }

        private NOTIFICATIONUSEINVOICEDETAIL GetNotificationUseInvoiceDetail(long companyId, long registerTemplateId, string symbol)
        {
            return this.noticeInvoiceDetailRepository.GetNotificationUseInvoiceDetails(companyId, registerTemplateId, symbol).OrderBy(p => p.NUMBERFROM).FirstOrDefault();
        }
        private void FormatBySystemSetting(InvoicePrintModel invoicePrintInfo, SystemSettingInfo systemSetting)
        {
            //logger.Error("CURRENCYCODE: " + invoicePrintInfo.CURRENCYCODE, new Exception("FormatBySystemSetting"));
            //StandardSystemSettingIfVND(systemSetting, invoicePrintInfo.CURRENCYCODE);
            invoicePrintInfo.TOTAL = FormatNumber.SetFractionDigit(invoicePrintInfo.TOTAL, invoicePrintInfo.CURRENCYCODE == DefaultFields.CURRENCY_VND ? 0 : systemSetting.Amount);
            //invoicePrintInfo.TOTAL = FormatNumber.SetFractionDigit(invoicePrintInfo.TOTAL, systemSetting.Amount);
            invoicePrintInfo.TOTALTAX = FormatNumber.SetFractionDigit(invoicePrintInfo.TOTALTAX, invoicePrintInfo.CURRENCYCODE == DefaultFields.CURRENCY_VND ? 0 : systemSetting.Amount);
            //invoicePrintInfo.TA = FormatNumber.SetFractionDigit(invoicePrintInfo.TaxAmout5, systemSetting.Amount);
            //invoicePrintInfo.TaxAmout10 = FormatNumber.SetFractionDigit(invoicePrintInfo.TaxAmout10, systemSetting.Amount);
            invoicePrintInfo.SUM = FormatNumber.SetFractionDigit(invoicePrintInfo.SUM, invoicePrintInfo.CURRENCYCODE == DefaultFields.CURRENCY_VND ? 0 : systemSetting.Amount);
            invoicePrintInfo.TOTALDISCOUNT = FormatNumber.SetFractionDigit(invoicePrintInfo.TOTALDISCOUNT, invoicePrintInfo.CURRENCYCODE == DefaultFields.CURRENCY_VND ? 0 : systemSetting.Amount);
            invoicePrintInfo.TOTALDISCOUNTTAX = FormatNumber.SetFractionDigit(invoicePrintInfo.TOTALDISCOUNTTAX, invoicePrintInfo.CURRENCYCODE == DefaultFields.CURRENCY_VND ? 0 : systemSetting.Amount);
            invoicePrintInfo.CURRENCYEXCHANGERATE = FormatNumber.SetFractionDigit(invoicePrintInfo.CURRENCYEXCHANGERATE, invoicePrintInfo.CURRENCYCODE == DefaultFields.CURRENCY_VND ? 0 : systemSetting.ExchangeRate);
        }
        private void FormatBySystemSetting(InvoicePrintInfo invoicePrintInfo, SystemSettingInfo systemSetting)
        {
            StandardSystemSettingIfVND(systemSetting, invoicePrintInfo.CurrencyCode);
            invoicePrintInfo.Total = FormatNumber.SetFractionDigit(invoicePrintInfo.Total, systemSetting.Amount);
            invoicePrintInfo.Total = FormatNumber.SetFractionDigit(invoicePrintInfo.Total, systemSetting.Amount);
            invoicePrintInfo.TaxAmout = FormatNumber.SetFractionDigit(invoicePrintInfo.TaxAmout, systemSetting.Amount);
            invoicePrintInfo.TaxAmout5 = FormatNumber.SetFractionDigit(invoicePrintInfo.TaxAmout5, systemSetting.Amount);
            invoicePrintInfo.TaxAmout10 = FormatNumber.SetFractionDigit(invoicePrintInfo.TaxAmout10, systemSetting.Amount);
            invoicePrintInfo.Sum = FormatNumber.SetFractionDigit(invoicePrintInfo.Sum, systemSetting.Amount);
            invoicePrintInfo.DiscountAmount = FormatNumber.SetFractionDigit(invoicePrintInfo.DiscountAmount, systemSetting.Amount);
            invoicePrintInfo.TotalDiscountTax = FormatNumber.SetFractionDigit(invoicePrintInfo.TotalDiscountTax, systemSetting.Amount);
        }

        private void FormatBySystemSetting(InvoiceItem invoiceItem, SystemSettingInfo systemSetting)
        {
            invoiceItem.Quantity = FormatNumber.SetFractionDigit(invoiceItem.Quantity, systemSetting.Quantity);
            invoiceItem.Price = FormatNumber.SetFractionDigit(invoiceItem.Price, systemSetting.Price);
            invoiceItem.Total = FormatNumber.SetFractionDigit(invoiceItem.Total, systemSetting.Amount);
            invoiceItem.AmountDiscount = FormatNumber.SetFractionDigit(invoiceItem.AmountDiscount, systemSetting.Amount);
            invoiceItem.AmountTax = FormatNumber.SetFractionDigit(invoiceItem.AmountTax, systemSetting.Amount);
        }
        private void FormatBySystemSettingJob(InvoiceItem invoiceItem, SystemSettingInfo systemSetting, string currencyCode)
        {
            //StandardSystemSettingIfVND(systemSetting, currencyCode);
            invoiceItem.Quantity = FormatNumber.SetFractionDigit(invoiceItem.Quantity, currencyCode == DefaultFields.CURRENCY_VND ? 0 : systemSetting.Quantity);
            invoiceItem.Price = FormatNumber.SetFractionDigit(invoiceItem.Price, currencyCode == DefaultFields.CURRENCY_VND ? 0 : systemSetting.Price);
            invoiceItem.Total = FormatNumber.SetFractionDigit(invoiceItem.Total, currencyCode == DefaultFields.CURRENCY_VND ? 0 : systemSetting.Amount);
            invoiceItem.Sum = FormatNumber.SetFractionDigit(invoiceItem.Sum, currencyCode == DefaultFields.CURRENCY_VND ? 0 : systemSetting.Amount);
            invoiceItem.AmountDiscount = FormatNumber.SetFractionDigit(invoiceItem.AmountDiscount, currencyCode == DefaultFields.CURRENCY_VND ? 0 : systemSetting.Amount);
            invoiceItem.AmountTax = FormatNumber.SetFractionDigit(invoiceItem.AmountTax, currencyCode == DefaultFields.CURRENCY_VND ? 0 : systemSetting.Amount);
        }
        private void FormatBySystemSetting(InvoiceInfo invoiceInfo)
        {
            // Format lúc tạo mới, edit, và chỉnh theo nếu tiền = VND thì không có số thập phân
            var systemSetting = this.systemSettingRepository.FilterSystemSetting(invoiceInfo.CompanyId ?? 0);
            StandardSystemSettingIfVND(systemSetting, invoiceInfo.CurrencyCode);

            invoiceInfo.Total = FormatNumber.SetFractionDigit(invoiceInfo.Total, systemSetting.Amount);
            invoiceInfo.TotalTax = FormatNumber.SetFractionDigit(invoiceInfo.TotalTax, systemSetting.Amount);
            invoiceInfo.TotalTax5 = FormatNumber.SetFractionDigit(invoiceInfo.TotalTax5, systemSetting.Amount);
            invoiceInfo.TotalTax10 = FormatNumber.SetFractionDigit(invoiceInfo.TotalTax10, systemSetting.Amount);
            invoiceInfo.TotalDiscount = FormatNumber.SetFractionDigit(invoiceInfo.TotalDiscount, systemSetting.Amount);
            invoiceInfo.TotalDiscountTax = FormatNumber.SetFractionDigit(invoiceInfo.TotalDiscountTax, systemSetting.Amount);
            invoiceInfo.Sum = FormatNumber.SetFractionDigit(invoiceInfo.Sum, systemSetting.Amount);
            invoiceInfo.StatisticalSumAmount = FormatNumber.SetFractionDigit(invoiceInfo.StatisticalSumAmount, systemSetting.Amount);
            invoiceInfo.StatisticalSumTaxAmount = FormatNumber.SetFractionDigit(invoiceInfo.StatisticalSumTaxAmount, systemSetting.Amount);
            invoiceInfo.GiftSumAmount = FormatNumber.SetFractionDigit(invoiceInfo.GiftSumAmount, systemSetting.Amount);
            invoiceInfo.GiftSumTaxAmount = FormatNumber.SetFractionDigit(invoiceInfo.GiftSumTaxAmount, systemSetting.Amount);

            foreach (var invoiceDetail in invoiceInfo.InvoiceDetail)
            {
                invoiceDetail.Price = FormatNumber.SetFractionDigit(invoiceDetail.Price, systemSetting.Price);
                invoiceDetail.AmountTax = FormatNumber.SetFractionDigit(invoiceDetail.AmountTax, systemSetting.Amount);
                invoiceDetail.AmountDiscount = FormatNumber.SetFractionDigit(invoiceDetail.AmountDiscount, systemSetting.Amount);
                invoiceDetail.Total = FormatNumber.SetFractionDigit(invoiceDetail.Total, systemSetting.Amount);
            }
        }

        private void FormatBySystemSetting(InvoiceMaster invoiceMaster, SystemSettingInfo SSInfo)
        {
            var systemSetting = SSInfo.DeepCopy();
            StandardSystemSettingIfVND(systemSetting, invoiceMaster.CurrencyCode);

            invoiceMaster.TotalAmount = FormatNumber.SetFractionDigit(invoiceMaster.TotalAmount, systemSetting.Amount);
            invoiceMaster.Sum = FormatNumber.SetFractionDigit(invoiceMaster.Sum, systemSetting.Amount);
            invoiceMaster.Total = FormatNumber.SetFractionDigit(invoiceMaster.Total, systemSetting.Amount);
            invoiceMaster.TotalTax = FormatNumber.SetFractionDigit(invoiceMaster.TotalTax, systemSetting.Amount);
        }

        public ExportFileInfo ExportInvoiceToPdf(long id, bool isSwitch = false)
        {
            InvoicePrintInfo invoicePrint = new InvoicePrintInfo();
            if (this.invoiceInfo.Id != 0 && this.invoiceInfo.InvoiceNo != null)
            {
                invoicePrint = this.invoiceInfo;
            }
            else
            {
                invoicePrint = this.invoiceRepository.GetInvoicePrintInfo(id);
            }
            //var statistical = this.invoiceStatisticalRepository.FilterInvoiceStatistical(id);
            //var invoiceGift = this.invoiceGiftRepository.FilterInvoiceGift(id);
            var invoiceSample_ = this.invoiceSample.GetById((long)invoicePrint.InvoiceSampleId);
            invoicePrint.isMultiTax = invoiceSample_.ISMULTITAX ?? false;
            var systemSetting = this.systemSettingRepository.FilterSystemSetting(invoicePrint.CompanyId);
            FormatBySystemSetting(invoicePrint, systemSetting);
            invoicePrint.IsInvoiceSwitch = isSwitch;
            MYCOMPANY myconpany = GetCompany(invoicePrint.CompanyId);
            this.config.BuildAssetByCompany(new CompanyInfo(myconpany));
            var invoiceDetails = GetInvoiceDetail(invoicePrint.Id).Select(p => new InvoiceItem(p)).ToList();
            invoicePrint.InvoiceItems = invoiceDetails;

            INVOICETEMPLATE template = GetTemplateNoTracking(invoicePrint.TemplateId);
            string footerInvoice = template.FOOTERINVOICE;
            if (isSwitch && template != null)
            {
                footerInvoice = footerInvoice.Replace(InvoicePrintInfoConst.SignOfConverterId_2, InvoicePrintInfoConst.SignOfConverter_2);
            }

            invoicePrint.Option.Height = template.HEIGHT;
            invoicePrint.Option.Width = template.WIDTH;
            invoicePrint.Option.PageSize = template.PAGESIZE;
            invoicePrint.Option.PageOrientation = template.PAGEORIENTATION;
            this.config.FullPathFileTemplateInvoice = Path.Combine(this.config.FullPathFolderTemplateInvoice, template.URLFILE);
            var itemName = invoicePrint.InvoiceItems;
            int numberRow = 0;
            itemName.ForEach(p =>
            {
                if (invoicePrint.Invoice_Status != (int)InvoiceStatus.New || invoicePrint.InvoiceSample != 0)
                {
                    p.ProductName = p.ProductNameInvoice;
                }
                if (p.TaxId == 4)
                {
                    p.AmountTax = null;
                }
                this.FormatBySystemSetting(p, systemSetting);
                int numberLine = p.ProductName.NumberLine(template.NUMBERCHARACTERINLINE.Value, 1.1);
                numberRow = numberRow + numberLine;
            });
            var itemsVAT = itemName.Where(p => p.TaxId != 4).ToList();
            if (itemsVAT.Count == 0)
            {
                invoicePrint.TaxAmout = null;
            }
            int numberLineOfPage = template.NUMBERLINEOFPAGE;

            int numberCharacterOfLine = (int)template.NUMBERCHARACTERINLINE;
            // Get VerificationCode
            var invoiceExportModel = new InvoiceExportModel()
            {
                InvoicePrintInfo = invoicePrint,
                //InvoiceStatisticals = statistical.ToList(),
                //InvoiceGift = invoiceGift.ToList(),
                PrintConfig = config,
                NumberLineOfPage = numberLineOfPage,
                NumberRecord = template.NUMBERRECORD ?? 0,
                DetailInvoice = template.DETAILINVOICE,
                LocationSignLeft = template.LOCATIONSIGNLEFT,
                LocationSignButton = template.LOCATIONSIGNBUTTON,
                NumberCharacterOfLine = numberCharacterOfLine,
                IsShowOrder = template.ISSHOWORDER,
                HeaderInvoice = template.HEADERINVOICE,
                FooterInvoice = footerInvoice,
                UserSessionInfo = currentUser,
                TemplateFileName = template.URLFILE,
                SystemSetting = systemSetting,
            };
            HQExportFile export = new HQExportFile(invoiceExportModel);
            return export.ExportFile();
        }
        //public string ExportInvoiceToPdfJob(InvoicePrintInfo invoicePrint, List<INVOICESAMPLE> objSamples, List<INVOICETEMPLATE> objTemplates, MYCOMPANY myconpany, List<INVOICEDETAIL> objDetails, bool isSwitch = false)
        //{

        //    //var statistical = this.invoiceStatisticalRepository.FilterInvoiceStatistical(id);
        //    //var invoiceGift = this.invoiceGiftRepository.FilterInvoiceGift(id);

        //    // lấy thông tin hóa đơn mẫu
        //    var invoiceSample_ = objSamples.FirstOrDefault(f => f.ID == invoicePrint.InvoiceSampleId);
        //    //this.invoiceSample.GetById((long)invoicePrint.InvoiceSampleId);
        //    invoicePrint.isMultiTax = invoiceSample_.ISMULTITAX ?? false;
        //    var systemSetting = this.systemSettingRepository.FilterSystemSetting(invoicePrint.CompanyId);
        //    FormatBySystemSetting(invoicePrint, systemSetting);
        //    invoicePrint.IsInvoiceSwitch = isSwitch;

        //    // MYCOMPANY myconpany = GetCompany(invoicePrint.CompanyId);

        //    this.config.BuildAssetByCompany(new CompanyInfo(myconpany));
        //    var invoiceDetails = objDetails.Select(p => new InvoiceItem(p)).ToList();
        //    InvoiceItem oldDetail = invoiceDetails.FirstOrDefault();

        //    InvoiceItem newDetail = new InvoiceItem();
        //    newDetail.ProductCode = oldDetail.ProductCode;
        //    newDetail.Unit = oldDetail.Unit;
        //    newDetail.Quantity = oldDetail.Quantity;
        //    if (invoicePrint.Vat_invoice_type != null)
        //    {
        //        if (invoicePrint.Vat_invoice_type.Contains("3"))
        //        {
        //            if (invoicePrint.Report_class != null || !string.IsNullOrEmpty(invoicePrint.Report_class))
        //            {
        //                if (invoicePrint.Report_class.Contains("6"))
        //                {
        //                    newDetail.ProductName = "Tiền lãi ngân hàng(interest collection)";
        //                    newDetail.ProductNameInvoice = "Tiền lãi ngân hàng(interest collection)";
        //                }
        //                else
        //                {
        //                    newDetail.ProductName = "Phí dịch vụ ngân hàng ( Fee commission)";
        //                    newDetail.ProductNameInvoice = "Phí dịch vụ ngân hàng ( Fee commission)";
        //                }

        //                newDetail.Total = invoicePrint.Total;
        //                newDetail.FirstRow = true;
        //                invoiceDetails.Add(newDetail);
        //                invoiceDetails.Reverse();
        //            }
        //        }
        //    }

        //    invoicePrint.InvoiceItems = invoiceDetails;
        //    //invoicePrint.InvoiceItems = GetInvoiceDetail(invoicePrint.Id).Select(p => new InvoiceItem(p)).ToList();

        //    // Lấy invoice template
        //    INVOICETEMPLATE template = objTemplates.FirstOrDefault(f => f.ID == invoicePrint.TemplateId);
        //    //GetTemplateNoTracking(invoicePrint.TemplateId);
        //    string footerInvoice = template.FOOTERINVOICE;
        //    if (isSwitch && template != null)
        //    {
        //        footerInvoice = footerInvoice.Replace(InvoicePrintInfoConst.SignOfConverterId, InvoicePrintInfoConst.SignOfConverter);
        //    }

        //    invoicePrint.Option.Height = template.HEIGHT;
        //    invoicePrint.Option.Width = template.WIDTH;
        //    invoicePrint.Option.PageSize = template.PAGESIZE;
        //    invoicePrint.Option.PageOrientation = template.PAGEORIENTATION;
        //    this.config.FullPathFileTemplateInvoice = Path.Combine(this.config.FullPathFolderTemplateInvoice, template.URLFILE);
        //    var itemName = invoicePrint.InvoiceItems;
        //    int numberRow = 0;
        //    itemName.ForEach(p =>
        //    {
        //        if (invoicePrint.Invoice_Status != (int)InvoiceStatus.New || invoicePrint.InvoiceSample != 0)
        //        {
        //            p.ProductName = p.ProductNameInvoice;
        //        }
        //        if (p.TaxId == 4)
        //        {
        //            p.AmountTax = null;
        //        }
        //        this.FormatBySystemSetting(p, systemSetting);
        //        int numberLine = p.ProductName.NumberLine(template.NUMBERCHARACTERINLINE.Value, 1.1);
        //        numberRow = numberRow + numberLine;
        //    });
        //    var itemsVAT = itemName.Where(p => p.TaxId != 4).ToList();
        //    if (itemsVAT.Count == 0)
        //    {
        //        invoicePrint.TaxAmout = null;
        //    }
        //    int numberLineOfPage = template.NUMBERLINEOFPAGE;

        //    int numberCharacterOfLine = (int)template.NUMBERCHARACTERINLINE;
        //    // Get VerificationCode
        //    var invoiceExportModel = new InvoiceExportModel()
        //    {
        //        InvoicePrintInfo = invoicePrint,
        //        //InvoiceStatisticals = objStatiscals,
        //        //InvoiceGift = objInvocieGifts,
        //        PrintConfig = config,
        //        NumberLineOfPage = numberLineOfPage,
        //        NumberRecord = template.NUMBERRECORD ?? 0,
        //        DetailInvoice = template.DETAILINVOICE,
        //        LocationSignLeft = template.LOCATIONSIGNLEFT,
        //        LocationSignButton = template.LOCATIONSIGNBUTTON,
        //        NumberCharacterOfLine = numberCharacterOfLine,
        //        IsShowOrder = template.ISSHOWORDER,
        //        HeaderInvoice = template.HEADERINVOICE,
        //        FooterInvoice = footerInvoice,
        //        UserSessionInfo = currentUser,
        //        TemplateFileName = template.URLFILE,
        //        SystemSetting = systemSetting,
        //    };
        //    HQExportFile export = new HQExportFile(invoiceExportModel);
        //    return export.ExportFileJob();
        //}
        public string ExportInvoiceToPdfJob(long id, bool isSwitch = false)
        {
            InvoicePrintInfo invoicePrint = new InvoicePrintInfo();
            if (this.invoiceInfo.Id != 0 && this.invoiceInfo.InvoiceNo != null)
            {
                invoicePrint = this.invoiceInfo;
            }
            else
            {
                invoicePrint = this.invoiceRepository.GetInvoicePrintInfo(id);
            }
            //var statistical = this.invoiceStatisticalRepository.FilterInvoiceStatistical(id);
            //var invoiceGift = this.invoiceGiftRepository.FilterInvoiceGift(id);
            var invoiceSample_ = this.invoiceSample.GetById((long)invoicePrint.InvoiceSampleId);
            invoicePrint.isMultiTax = invoiceSample_.ISMULTITAX ?? false;
            var systemSetting = this.systemSettingRepository.FilterSystemSetting(invoicePrint.CompanyId);
            FormatBySystemSetting(invoicePrint, systemSetting);
            invoicePrint.IsInvoiceSwitch = isSwitch;
            MYCOMPANY myconpany = GetCompany(invoicePrint.CompanyId);
            this.config.BuildAssetByCompany(new CompanyInfo(myconpany));
            invoicePrint.InvoiceItems = GetInvoiceDetail(invoicePrint.Id).Select(p => new InvoiceItem(p)).ToList();

            INVOICETEMPLATE template = GetTemplateNoTracking(invoicePrint.TemplateId);
            string footerInvoice = template.FOOTERINVOICE;
            if (isSwitch && template != null)
            {
                footerInvoice = footerInvoice.Replace(InvoicePrintInfoConst.SignOfConverterId_2, InvoicePrintInfoConst.SignOfConverter_2);
            }

            invoicePrint.Option.Height = template.HEIGHT;
            invoicePrint.Option.Width = template.WIDTH;
            invoicePrint.Option.PageSize = template.PAGESIZE;
            invoicePrint.Option.PageOrientation = template.PAGEORIENTATION;
            this.config.FullPathFileTemplateInvoice = Path.Combine(this.config.FullPathFolderTemplateInvoice, template.URLFILE);
            var itemName = invoicePrint.InvoiceItems;
            int numberRow = 0;
            itemName.ForEach(p =>
            {
                if (invoicePrint.Invoice_Status != (int)InvoiceStatus.New || invoicePrint.InvoiceSample != 0)
                {
                    p.ProductName = p.ProductNameInvoice;
                }
                if (p.TaxId == 4)
                {
                    p.AmountTax = null;
                }
                this.FormatBySystemSetting(p, systemSetting);
                int numberLine = p.ProductName.NumberLine(template.NUMBERCHARACTERINLINE.Value, 1.1);
                numberRow = numberRow + numberLine;
            });
            var itemsVAT = itemName.Where(p => p.TaxId != 4).ToList();
            if (itemsVAT.Count == 0)
            {
                invoicePrint.TaxAmout = null;
            }
            int numberLineOfPage = template.NUMBERLINEOFPAGE;

            int numberCharacterOfLine = (int)template.NUMBERCHARACTERINLINE;
            // Get VerificationCode
            var invoiceExportModel = new InvoiceExportModel()
            {
                InvoicePrintInfo = invoicePrint,
                //InvoiceStatisticals = statistical,
                //InvoiceGift = invoiceGift,
                PrintConfig = config,
                NumberLineOfPage = numberLineOfPage,
                NumberRecord = template.NUMBERRECORD ?? 0,
                DetailInvoice = template.DETAILINVOICE,
                LocationSignLeft = template.LOCATIONSIGNLEFT,
                LocationSignButton = template.LOCATIONSIGNBUTTON,
                NumberCharacterOfLine = numberCharacterOfLine,
                IsShowOrder = template.ISSHOWORDER,
                HeaderInvoice = template.HEADERINVOICE,
                FooterInvoice = footerInvoice,
                UserSessionInfo = currentUser,
                TemplateFileName = template.URLFILE,
                SystemSetting = systemSetting,
            };
            HQExportFile export = new HQExportFile(invoiceExportModel);
            return export.ExportFileJob();
        }

        private MYCOMPANY GetCompany(long id)
        {
            return myCompanyRepository.GetById(id);
        }

        private ExportFileInfo GetInvoiceFile(long invoiceId)
        {
            INVOICE invoice = GetInvoice(invoiceId);
            if (invoice.INVOICESTATUS == (int)InvoiceStatus.Released || invoice.INVOICESTATUS == (int)InvoiceStatus.Cancel || invoice.INVOICESTATUS == (int)InvoiceStatus.Delete)
            {
                return FilterInvoiceFile(invoice);
            }
            else
            {
                return null;
            }

        }

        private ExportFileInfo FilterInvoiceFile(INVOICE invoice)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            string fullPathFile;
            string fileName;
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, invoice.COMPANYID.ToString());
            if (!invoice.INVOICESTATUS.HasValue || invoice.INVOICESTATUS.Value == (int)InvoiceStatus.Released)
            {
                if (invoice.CLIENTSIGN == true)
                {
                    fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), string.Format("{0}_sign_sign.pdf", invoice.ID));
                    fileInfo.FullPathFileName = fullPathFile;
                }
                else
                {
                    fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), string.Format("{0}_sign.pdf", invoice.ID));
                    fileInfo.FullPathFileName = fullPathFile;
                }
            }
            else if (invoice.INVOICESTATUS == (int)InvoiceStatus.Cancel)
            {
                this.FilterInvoiceFileCancel(invoice, folderInvoiceOfCompany, fileInfo, out fullPathFile, out fileName);
            }
            else if (invoice.INVOICESTATUS == (int)InvoiceStatus.Delete)
            {
                this.FilterInvoiceFileDelete(invoice, folderInvoiceOfCompany, fileInfo, out fullPathFile, out fileName);
            }
            else if (this.invoiceInfo.InvoiceNo != null)
            {
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), string.Format("{0}_sign.pdf", invoice.ID));
                fileInfo.FullPathFileName = fullPathFile;
            }
            else
            {
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Invoice, string.Format("{0}.pdf", invoice.ID));
                fileInfo.FullPathFileName = fullPathFile;
            }


            if (!File.Exists(fullPathFile))
            {
                return null;
            }

            fileInfo.FileName = "Invoice.pdf";
            return fileInfo;
        }
        private void FilterInvoiceFileCancel(INVOICE invoice, string folderInvoiceOfCompany, ExportFileInfo fileInfo, out string fullPathFile, out string fileName)
        {
            if (invoice.CLIENTSIGN == true)
            {
                fileName = string.Format("{0}_sign_signCancel.pdf", invoice.ID);
            }
            else
            {
                fileName = string.Format("{0}_signCancel.pdf", invoice.ID);
            }
            fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), fileName);
            if (!File.Exists(fullPathFile))
            {
                if (invoice.CLIENTSIGN == true)
                {
                    fileName = string.Format("{0}_sign_sign.pdf", invoice.ID);
                }
                else
                {
                    fileName = string.Format("{0}_sign.pdf", invoice.ID);
                }
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), fileName);
                string path_Image = Path.Combine(config.FullPathFileAsset, "hoadon-huy.png");
                fullPathFile = fileInfo.FullPathFileName = AddFileCancel(fullPathFile, path_Image);
            }
            else
            {
                fileInfo.FullPathFileName = fullPathFile;
            }
        }
        private void FilterInvoiceFileDelete(INVOICE invoice, string folderInvoiceOfCompany, ExportFileInfo fileInfo, out string fullPathFile, out string fileName)
        {
            if (invoice.CLIENTSIGN == true)
            {
                fileName = string.Format("{0}_sign_signDelete.pdf", invoice.ID);
            }
            else
            {
                fileName = string.Format("{0}_signDelete.pdf", invoice.ID);
            }
            fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), fileName);
            if (!File.Exists(fullPathFile))
            {
                if (invoice.CLIENTSIGN == true)
                {
                    fileName = string.Format("{0}_sign_sign.pdf", invoice.ID);
                }
                else
                {
                    fileName = string.Format("{0}_sign.pdf", invoice.ID);
                }
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), fileName);
                string path_Image = Path.Combine(config.FullPathFileAsset, "hoadon-xoa.png");
                fullPathFile = fileInfo.FullPathFileName = AddFileDelete(fullPathFile, path_Image);
            }
            else
            {
                fileInfo.FullPathFileName = fullPathFile;
            }
        }
        private ExportFileInfo GetInvoiceXmlFile(long invoiceId)
        {
            INVOICE invoice = GetInvoice(invoiceId);
            return FilterInvoiceXmlFile(invoice);
        }
        private ExportFileInfo GetInvoiceStaticalFile(long invoiceId)
        {
            INVOICE invoice = GetInvoice(invoiceId);
            return FilterStatisticalFile(invoice);
        }
        private ExportFileInfo GetInvoiceGiftFile(long invoiceId)
        {
            INVOICE invoice = GetInvoice(invoiceId);
            return FilterGiftFile(invoice);
        }
        private ExportFileInfo FilterStatisticalFile(INVOICE invoice)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            string fullPathFile = null;
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, invoice.COMPANYID.ToString());
            if (!invoice.INVOICESTATUS.HasValue || invoice.INVOICESTATUS.Value == (int)InvoiceStatus.Released)
            {
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), string.Format("{0}_statistical_sign.pdf", invoice.ID));
                fileInfo.FullPathFileName = fullPathFile;
            }
            else
            {
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Invoice, string.Format("{0}_statistical.pdf", invoice.ID));
                fileInfo.FullPathFileName = fullPathFile;
            }


            if (!File.Exists(fullPathFile))
            {
                return null;
            }

            fileInfo.FileName = "statistical.pdf";
            return fileInfo;
        }
        private ExportFileInfo FilterGiftFile(INVOICE invoice)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            string fullPathFile = null;
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, invoice.COMPANYID.ToString());
            if (!invoice.INVOICESTATUS.HasValue || invoice.INVOICESTATUS.Value == (int)InvoiceStatus.Released)
            {
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), string.Format("{0}_invoiceGift_sign.pdf", invoice.ID));
                fileInfo.FullPathFileName = fullPathFile;
            }
            else
            {
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Invoice, string.Format("{0}_invoiceGift.pdf", invoice.ID));
                fileInfo.FullPathFileName = fullPathFile;
            }


            if (!File.Exists(fullPathFile))
            {
                return null;
            }

            fileInfo.FileName = "invoiceGift.pdf";
            return fileInfo;
        }
        private ExportFileInfo FilterInvoiceXmlFile(INVOICE invoice)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            string fullPathFile = null;
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, invoice.COMPANYID.ToString());
            if (!invoice.INVOICESTATUS.HasValue || invoice.INVOICESTATUS.Value == (int)InvoiceStatus.Released)
            {
                if (invoice.CLIENTSIGN == true)
                {
                    fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), string.Format("{0}_sign_sign.xml", invoice.ID));
                    fileInfo.FullPathFileName = fullPathFile;
                }
                else
                {
                    fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), string.Format("{0}_sign.xml", invoice.ID));
                    fileInfo.FullPathFileName = fullPathFile;

                }

            }
            else if (invoice.INVOICESTATUS == (int)InvoiceStatus.Cancel)
            {
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), string.Format("{0}_signCancel.xml", invoice.ID));
                if (!File.Exists(fullPathFile))
                {
                    UpdateStatusXml(Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), string.Format("{0}_sign.xml", invoice.ID)), invoice.INVOICESTATUS ?? 0);
                    fileInfo.FullPathFileName = fullPathFile;
                }
                else
                {
                    fileInfo.FullPathFileName = fullPathFile;
                }

            }
            else if (invoice.INVOICESTATUS == (int)InvoiceStatus.Delete)
            {
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), string.Format("{0}_signDelete.xml", invoice.ID));
                if (!File.Exists(fullPathFile))
                {
                    UpdateStatusXml(Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), string.Format("{0}_sign.xml", invoice.ID)), invoice.INVOICESTATUS ?? 0);
                    fileInfo.FullPathFileName = fullPathFile;
                }
                else
                {
                    fileInfo.FullPathFileName = fullPathFile;
                }

            }
            else
            {
                fullPathFile = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Invoice, string.Format("{0}.xml", invoice.ID));
                fileInfo.FullPathFileName = fullPathFile;
            }
            if (!File.Exists(fullPathFile))
            {
                return null;
            }

            fileInfo.FileName = "Invoice.xml";
            return fileInfo;
        }
        private void UpdateStatusXml(string xmlPath, int status)
        {
            var xdoc = XDocument.Load(xmlPath);
            //xdoc.XPathSelectElement("//InvoiceData/status").Value = status.ToString();//tắt dòng này để tải được xml
            xdoc.Save(xmlPath.Replace("sign.xml", status == (int)InvoiceStatus.Cancel ? "signCancel.xml" : "signDelete.xml"));
            File.Delete(xmlPath);
        }
        private string AddFileCancel(string fullPathFileName, string imageDraft)
        {
            string fileName = fullPathFileName.Replace("sign.pdf", "signCancel.pdf");
            using (Stream inputPdfStream = new FileStream(fullPathFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream inputImageStream = new FileStream(imageDraft, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                var pdfContentByte = stamper.GetOverContent(1);
                var image = iTextSharp.text.Image.GetInstance(imageDraft);
                float signPositionX = pdfContentByte.PdfDocument.PageSize.Width - 430;
                image.SetAbsolutePosition(signPositionX, 350);
                image.ScalePercent(70);
                pdfContentByte.AddImage(image);
                stamper.Close();
                inputPdfStream.Close();
                inputImageStream.Close();
            }
            File.Delete(fullPathFileName);
            return fileName;
        }
        private string AddFileDelete(string fullPathFileName, string imageDraft)
        {
            string fileName = fullPathFileName.Replace("sign.pdf", "signDelete.pdf");
            using (Stream inputPdfStream = new FileStream(fullPathFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream inputImageStream = new FileStream(imageDraft, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                var pdfContentByte = stamper.GetOverContent(1);
                var image = iTextSharp.text.Image.GetInstance(imageDraft);
                float signPositionX = pdfContentByte.PdfDocument.PageSize.Width - 430;
                image.SetAbsolutePosition(signPositionX, 350);
                image.ScalePercent(70);
                pdfContentByte.AddImage(image);
                stamper.Close();
                inputPdfStream.Close();
                inputImageStream.Close();
            }
            File.Delete(fullPathFileName);
            return fileName;
        }
        private void AddFileSwitch(ExportFileInfo fileInfo)
        {
            string fileName = fileInfo.FullPathFileName.Replace("sign.pdf", "signSwitchTemp.pdf");
            using (Stream inputPdfStream = new FileStream(fileInfo.FullPathFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);

                stamper.Close();
                reader.Close();
            }

            fileInfo.FullPathFileName = fileName;
            AddPageNumberAndMessageSwitch(fileInfo.FullPathFileName);
            AddConverterSign(fileInfo);
        }
        protected void AddPageNumberAndMessageSwitch(string file)
        {
            var textDetect = "(VALUE ADDED TAX INVOICE)";
            var locationFound = PdfGetTextData.FindFirstLocationOfTextInPage(file, textDetect, 1);
            if (locationFound.X == 0 && locationFound.Y == 0)
            {
                locationFound = new PdfTextLocation()
                {
                    X = 143,
                    Y = 665,
                    Page = 1,
                };
            }

            byte[] bytes = File.ReadAllBytes(file);
            Font blackFont = FontFactory.GetFont(BaseFont.TIMES_ROMAN, 10, Font.NORMAL, BaseColor.BLACK);
            using (MemoryStream stream = new MemoryStream())
            {
                PdfReader reader = new PdfReader(bytes);
                using (PdfStamper stamper = new PdfStamper(reader, stream))
                {
                    int pages = reader.NumberOfPages;
                    if (pages > 1)
                    {
                        ColumnText.ShowTextAligned(stamper.GetOverContent(1), Element.ALIGN_RIGHT, new Phrase("(Trang " + 1 + "/" + pages + ")", blackFont), 590f, 830, 0);
                    }
                    for (int i = 2; i <= pages; i++)
                    {
                        ColumnText.ShowTextAligned(stamper.GetOverContent(i), Element.ALIGN_RIGHT, new Phrase("(Tiep theo trang " + i + "/" + pages + ")", blackFont), 590f, 830, 0);
                    }
                    for (int i = 1; i <= pages; i++)
                    {
                        var pdfContentByte = stamper.GetOverContent(locationFound.Page);
                        string font_Path = Path.Combine(config.FullPathFileAsset, "font-times-new-roman.ttf");
                        BaseFont bf = BaseFont.CreateFont(font_Path, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        string textSwitch = Common.Constants.InvoicePrintInfoConst.MessageSwitchInvoice;
                        var FontColour = new BaseColor(0, 0, 0);
                        Font font = new Font(bf, 8, Font.NORMAL, FontColour);
                        ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textSwitch, font), locationFound.X + 58, locationFound.Y - 25, 0);
                    }
                }
                bytes = stream.ToArray();
            }
            File.WriteAllBytes(file, bytes);
        }
        private void AddConverterSign(ExportFileInfo fileInfo)
        {
            var listTextDetect1 = TextDetectLocationConverterSign.Texts;
            var locationFound = PdfGetTextData.FindLastLocationOfListTextReverse(fileInfo.FullPathFileName, listTextDetect1, 10);

            if (locationFound == null || (locationFound.X == 0 && locationFound.Y == 0))
            {
                return;
            }

            AddConverterSignShinhanStyle(fileInfo, locationFound);
        }

        private void AddConverterSignShinhanStyle(ExportFileInfo fileInfo, PdfTextLocation locationFound)
        {
            string fileName = fileInfo.FullPathFileName.Replace("signSwitchTemp.pdf", "signSwitch.pdf");
            using (Stream inputPdfStream = new FileStream(fileInfo.FullPathFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                string fontPath = Path.Combine(config.FullPathFileAsset, "font-times-new-roman.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                string fontPathItalic = Path.Combine(config.FullPathFileAsset, "font-times-new-roman-italic.ttf");
                BaseFont bfItalic = BaseFont.CreateFont(fontPathItalic, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                // Thêm mục chữ ký người chuyển đổi vào page cuối
                var pdfContentByte = stamper.GetOverContent(locationFound.Page);
                string textConverter1 = "Người chuyển đổi";
                var fontConverter1 = new Font(bf, 10.5f, Font.NORMAL, new BaseColor(20, 20, 20));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter1, fontConverter1), 300 - 25, locationFound.Y - 10f, 0);
                string textConverter2 = "(Converter)";
                var fontConverter2 = new Font(bfItalic, 9.8f, Font.NORMAL, new BaseColor(20, 20, 20));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter2, fontConverter2), 300 + 38, locationFound.Y - 10f, 0);

                string textConverterName = this.currentUser.UserName;
                var fontConverterName = new Font(bf, 10f, Font.NORMAL, new BaseColor(0, 0, 0));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverterName, fontConverterName), 290, locationFound.Y - 70, 0);
                string textDateConvert = "Ngày chuyển đổi: " + DateTime.Today.ToString("dd/MM/yyyy");
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textDateConvert, fontConverterName), 285, locationFound.Y - 85, 0);

                pdfContentByte.SetLineWidth(0.8d);
                pdfContentByte.MoveTo(300 - 70, locationFound.Y - 17.9);
                pdfContentByte.LineTo(300 + 70, locationFound.Y - 17.9);
                pdfContentByte.ClosePathStroke();

                stamper.Close();
                reader.Close();
            }
            File.Delete(fileInfo.FullPathFileName);
            fileInfo.FullPathFileName = fileName;
        }

        private void AddConverterSignHuananStyle(ExportFileInfo fileInfo, PdfTextLocation locationFound)
        {
            string fileName = fileInfo.FullPathFileName.Replace("signSwitchTemp.pdf", "signSwitch.pdf");
            using (Stream inputPdfStream = new FileStream(fileInfo.FullPathFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                string fontPathBold = Path.Combine(config.FullPathFileAsset, "font-times-new-roman-bold.ttf");
                BaseFont bfBold = BaseFont.CreateFont(fontPathBold, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                string fontPath = Path.Combine(config.FullPathFileAsset, "font-times-new-roman.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                // Thêm mục chữ ký người chuyển đổi vào page cuối
                var pdfContentByte = stamper.GetOverContent(locationFound.Page);
                string textConverter1 = "NGƯỜI CHUYỂN ĐỔI";
                var fontConverter1 = new Font(bfBold, 9f, Font.NORMAL, new BaseColor(20, 20, 20));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter1, fontConverter1), 300, locationFound.Y - 8f, 0);
                string textConverter2 = "(Converter)";
                var fontConverter2 = new Font(bfBold, 9f, Font.NORMAL, new BaseColor(20, 20, 20));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter2, fontConverter2), 300, locationFound.Y - 25f, 0);

                string textConverterName = this.currentUser.UserName;
                var fontConverterName = new Font(bf, 10f, Font.NORMAL, new BaseColor(0, 0, 0));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverterName, fontConverterName), 300, locationFound.Y - 90, 0);
                string textDateConvert = "Ngày chuyển đổi: " + DateTime.Today.ToString("dd/MM/yyyy");
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textDateConvert, fontConverterName), 295, locationFound.Y - 105, 0);

                pdfContentByte.SetLineWidth(0.8d);
                pdfContentByte.MoveTo(300 - 70, locationFound.Y - 32);
                pdfContentByte.LineTo(300 + 70, locationFound.Y - 32);
                pdfContentByte.ClosePathStroke();

                stamper.Close();
                reader.Close();
            }
            File.Delete(fileInfo.FullPathFileName);
            fileInfo.FullPathFileName = fileName;
        }

        private void AddConverterSignBIDCStyle(ExportFileInfo fileInfo, PdfTextLocation locationFound)
        {
            string fileName = fileInfo.FullPathFileName.Replace("signSwitchTemp.pdf", "signSwitch.pdf");
            using (Stream inputPdfStream = new FileStream(fileInfo.FullPathFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                string fontPath = Path.Combine(config.FullPathFileAsset, "font-times-new-roman.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                string fontPathBold = Path.Combine(config.FullPathFileAsset, "font-times-new-roman-bold.ttf");
                BaseFont bfBold = BaseFont.CreateFont(fontPathBold, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                string fontPathItalic = Path.Combine(config.FullPathFileAsset, "font-times-new-roman-italic.ttf");
                BaseFont bfItalic = BaseFont.CreateFont(fontPathItalic, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                // Thêm mục chữ ký người chuyển đổi vào page cuối
                var pdfContentByte = stamper.GetOverContent(locationFound.Page);
                string textConverter1 = "Người chuyển đổi / (Converter)";
                var fontConverter1 = new Font(bfBold, 9f, Font.NORMAL, new BaseColor(20, 20, 20));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter1, fontConverter1), 300, locationFound.Y - 8f, 0);
                string textConverter2 = "(Ký, ghi rõ họ tên)";
                var fontConverter2 = new Font(bfItalic, 9f, Font.NORMAL, new BaseColor(20, 20, 20));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter2, fontConverter2), 300, locationFound.Y - 25f, 0);
                string textConverter3 = "(Sign & fullname)";
                var fontConverter3 = new Font(bfItalic, 8f, Font.NORMAL, new BaseColor(20, 20, 20));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter3, fontConverter3), 300, locationFound.Y - 42f, 0);

                string textConverterName = this.currentUser.UserName;
                var fontConverterName = new Font(bf, 10f, Font.NORMAL, new BaseColor(0, 0, 0));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverterName, fontConverterName), 300, locationFound.Y - 90, 0);
                string textDateConvert = "Ngày chuyển đổi: " + DateTime.Today.ToString("dd/MM/yyyy");
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textDateConvert, fontConverterName), 295, locationFound.Y - 105, 0);

                stamper.Close();
                reader.Close();
            }
            File.Delete(fileInfo.FullPathFileName);
            fileInfo.FullPathFileName = fileName;
        }

        private void AddConverterSignSinoStyle(ExportFileInfo fileInfo, PdfTextLocation locationFound)
        {
            string fileName = fileInfo.FullPathFileName.Replace("signSwitchTemp.pdf", "signSwitch.pdf");
            using (Stream inputPdfStream = new FileStream(fileInfo.FullPathFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                string fontPath = Path.Combine(config.FullPathFileAsset, "font-times-new-roman.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                // Thêm mục chữ ký người chuyển đổi vào page cuối
                var pdfContentByte = stamper.GetOverContent(locationFound.Page);
                string textConverter1 = "Người chuyển đổi";
                var fontConverter1 = new Font(bf, 9f, Font.NORMAL, new BaseColor(20, 20, 20));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter1, fontConverter1), 300 - 21, locationFound.Y - 8.5f, 0);
                string textConverter2 = "(Converter)";
                var fontConverter2 = new Font(bf, 9f, Font.NORMAL, new BaseColor(20, 20, 20));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter2, fontConverter2), 300 + 34, locationFound.Y - 8.5f, 0);

                string textConverterName = this.currentUser.UserName;
                var fontConverterName = new Font(bf, 10f, Font.NORMAL, new BaseColor(0, 0, 0));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverterName, fontConverterName), 300, locationFound.Y - 90, 0);
                string textDateConvert = "Ngày chuyển đổi: " + DateTime.Today.ToString("dd/MM/yyyy");
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textDateConvert, fontConverterName), 295, locationFound.Y - 105, 0);


                pdfContentByte.SetLineWidth(0.8d);
                pdfContentByte.MoveTo(300 - 70, locationFound.Y - 16f);
                pdfContentByte.LineTo(300 + 70, locationFound.Y - 16f);
                pdfContentByte.ClosePathStroke();

                stamper.Close();
                reader.Close();
            }
            File.Delete(fileInfo.FullPathFileName);
            fileInfo.FullPathFileName = fileName;
        }

        private void AddConverterSignMegaStyle(ExportFileInfo fileInfo, PdfTextLocation locationFound)
        {
            string fileName = fileInfo.FullPathFileName.Replace("signSwitchTemp.pdf", "signSwitch.pdf");
            using (Stream inputPdfStream = new FileStream(fileInfo.FullPathFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                string fontPathBold = Path.Combine(config.FullPathFileAsset, "font-times-new-roman-bold.ttf");
                BaseFont bfBold = BaseFont.CreateFont(fontPathBold, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                string fontPath = Path.Combine(config.FullPathFileAsset, "font-times-new-roman.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                // Thêm mục chữ ký người chuyển đổi vào page cuối
                var pdfContentByte = stamper.GetOverContent(locationFound.Page);
                string textConverter1 = "NGƯỜI CHUYỂN ĐỔI";
                var fontConverter1 = new Font(bfBold, 9f, Font.NORMAL, new BaseColor(20, 20, 20));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter1, fontConverter1), 300, locationFound.Y - 8.5f, 0);
                string textConverter2 = "(Converter Signature and full name)";
                var fontConverter2 = new Font(bfBold, 9f, Font.NORMAL, new BaseColor(20, 20, 20));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter2, fontConverter2), 300, locationFound.Y - 25f, 0);

                string textConverterName = this.currentUser.UserName;
                var fontConverterName = new Font(bf, 10f, Font.NORMAL, new BaseColor(0, 0, 0));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverterName, fontConverterName), 300, locationFound.Y - 90, 0);
                string textDateConvert = "Ngày chuyển đổi: " + DateTime.Today.ToString("dd/MM/yyyy");
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textDateConvert, fontConverterName), 295, locationFound.Y - 105, 0);


                pdfContentByte.SetLineWidth(0.8d);
                pdfContentByte.MoveTo(300 - 70, locationFound.Y - 32.8f);
                pdfContentByte.LineTo(300 + 70, locationFound.Y - 32.8f);
                pdfContentByte.ClosePathStroke();

                stamper.Close();
                reader.Close();
            }
            File.Delete(fileInfo.FullPathFileName);
            fileInfo.FullPathFileName = fileName;
        }

        #endregion
        private void InsertQueueCreateFile(List<long> invoicesId)
        {
            foreach (var item in invoicesId)
            {
                QUEUECREATEFILEINVOICE queue = new QUEUECREATEFILEINVOICE();
                queue.INVOICEID = item;
                queue.PROCESSSTATUS = (int)QueueCreateFileStatus.New;
                queue.CREATEDFILE = false;
                this.queueRepository.Insert(queue);
            }
        }

        public decimal GetTotalInvoiceUse()
        {
            var totalInvoiceOfCompany = this.invoiceRepository.CountInvoice(1);
            return totalInvoiceOfCompany;
        }

        public decimal GetTotalInvoiceOfCompanyUse(long companyId)
        {
            var totalInvoiceOfCompany = this.invoiceRepository.CountInvoice(0, companyId);
            return totalInvoiceOfCompany;
        }
        public decimal GetTotalInvoiceOfCancelling(long companyId)
        {
            var totalInvoiceOfCancelling = this.invoiceRepository.CountInvoice(0, companyId);
            return totalInvoiceOfCancelling;
        }

        private double GetMaxNumberInvoiceCompanyRegister(long companyId, long registerTemplateId, string symbol)
        {
            var invoiceUserDetails = this.noticeInvoiceDetailRepository.GetNotificationUseInvoiceDetails(companyId, registerTemplateId, symbol);
            var maxInvoiceUseDetail = invoiceUserDetails.OrderByDescending(p => p.NUMBERTO).FirstOrDefault();
            if (maxInvoiceUseDetail != null)
            {
                return maxInvoiceUseDetail.NUMBERTO.ToDouble(0);
            }

            return 0;
        }
        public INVOICETEMPLATE GetTemplateNoTracking(long id)
        {
            return this.invoiceTemplateRepository.GetByIdNoTracking(id);
        }

        private List<INVOICEDETAIL> GetInvoiceDetail(long invoiceId)
        {
            return this.invoiceDetailRepository.FilterInvoiceDetail(invoiceId).ToList();
        }
        private List<INVOICEDETAIL> GetInvoiceDetailJob(long invoiceId)
        {
            return this.invoiceDetailRepository.FilterInvoiceDetailJob(invoiceId).ToList();
        }
        private RELEASEINVOICEDETAIL GetReleaseInvoiceDetail(long invoiceId)
        {
            RELEASEINVOICEDETAIL releaseInvoiceDetail = this.releaseInvoiceDetaiRepository.FilteReleaseInvoiceDetail(invoiceId);
            if (releaseInvoiceDetail == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This TaxId [{0}] not found in data of client", invoiceId));
            }

            return releaseInvoiceDetail;
        }

        private void SetInvoiceConverted(long invoiceId)
        {
            //RELEASEINVOICEDETAIL currentReleaseInvoiceDetail = GetReleaseInvoiceDetail(invoiceId);
            //currentReleaseInvoiceDetail.CONVERTED = true;
            //currentReleaseInvoiceDetail.CONVERTER = currentUser.UserName;
            //currentReleaseInvoiceDetail.CONVERTDATE = DateTime.Today;

            INVOICE currentInvoice = GetInvoice(invoiceId);
            currentInvoice.CONVERTED = true;
            currentInvoice.CONVERTER = currentUser.UserName;
            currentInvoice.CONVERTDATE = DateTime.Today;

            this.invoiceRepository.Update(currentInvoice);

            //this.releaseInvoiceDetaiRepository.Update(currentReleaseInvoiceDetail);
        }

        private void DeleteInvoiceFile(long invoiceId, long companyId)
        {
            ExportFileInfo fileInfo = GetFileInvoice(invoiceId, companyId);
            if (File.Exists(fileInfo.FullPathFileName))
            {
                File.Delete(fileInfo.FullPathFileName);
            }

            if (File.Exists(fileInfo.FullPathFileInvoice))
            {
                File.Delete(fileInfo.FullPathFileInvoice);
            }
        }

        private ExportFileInfo GetFileInvoiceSign(long invoiceId, long companyId)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            INVOICE invoice = GetInvoice(invoiceId);
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, companyId.ToString());
            string fileNameSign = "";
            if (invoice.CLIENTSIGN == true)
            {
                fileNameSign = string.Format("{0}_sign_sign.pdf", invoice.ID);
            }
            else
            {
                fileNameSign = string.Format("{0}_sign.pdf", invoice.ID);
            }
            string fileNameInvoice = string.Format("{0}.pdf", invoice.ID);
            fileInfo.FullPathFileName = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, PathUtil.FolderTree(invoice.RELEASEDDATE), fileNameSign);
            fileInfo.FullPathFileInvoice = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Invoice, fileNameInvoice);
            fileInfo.FileName = fileNameSign;
            return fileInfo;
        }
        private ExportFileInfo GetFileInvoice(long invoiceId, long companyId)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, companyId.ToString());
            string fileNameInvoice = string.Format("{0}.pdf", invoiceId);
            fileInfo.FullPathFileName = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Invoice, fileNameInvoice);
            fileInfo.FullPathFileInvoice = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Invoice, fileNameInvoice);
            fileInfo.FileName = fileNameInvoice;
            return fileInfo;
        }
        private ExportFileInfo GetFileStatisticalSign(long invoiceId, long companyId)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            INVOICE invoice = GetInvoice(invoiceId);
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, companyId.ToString());
            string fileNameSign = string.Format("{0}_statistical_sign.pdf", invoice.ID);
            string fileNameStatistical = string.Format("{0}_statistical.pdf", invoice.ID);
            fileInfo.FullPathFileName = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, invoice.RELEASEDDATE.Value.ToString("yyyyMMdd"), fileNameSign);
            fileInfo.FullPathFileInvoice = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Invoice, fileNameStatistical);
            fileInfo.FileName = fileNameSign;
            return fileInfo;
        }
        private ExportFileInfo GetFileInvoiceGiftSign(long invoiceId, long companyId)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            INVOICE invoice = GetInvoice(invoiceId);
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, companyId.ToString());
            string fileNameSign = string.Format("{0}_invoiceGift_sign.pdf", invoice.ID);
            string fileNameGift = string.Format("{0}_invoiceGift.pdf", invoice.ID);
            fileInfo.FullPathFileName = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, invoice.RELEASEDDATE.Value.ToString("yyyyMMdd"), fileNameSign);
            fileInfo.FullPathFileInvoice = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Invoice, fileNameGift);
            fileInfo.FileName = fileNameSign;
            return fileInfo;
        }
        private void InsertFileAttachByInvoice(long invoiceId, long companyId, long emailActiveId)
        {
            ExportFileInfo fileInfoXML;
            fileInfoXML = GetInvoiceXmlFile(invoiceId);
            logger.Error("fileInfoXML", new Exception(fileInfoXML.FullPathFileName));
            if (fileInfoXML == null)
            {

                throw new BusinessLogicException(ResultCode.FileNotFound, MsgApiResponse.FileNotFound);
            }
            InserEmailFileAttachFile(fileInfoXML, emailActiveId, invoiceId);
            ExportFileInfo fileInfoInvoice = GetFileInvoiceSign(invoiceId, companyId);
            logger.Trace("fileInfoInvoice + " + fileInfoInvoice.FileName.ToString() + fileInfoInvoice.FullPathFileInvoice.ToString() + fileInfoInvoice.FullPathFileName.ToString());
            InserEmailFileAttachFile(fileInfoInvoice, emailActiveId, invoiceId);
            //logger.Trace("fileInfoInvoice + " + fileInfoInvoice.FileName.ToString() + fileInfoInvoice.FullPathFileInvoice.ToString() + fileInfoInvoice.FullPathFileName.ToString());
            //ExportFileInfo fileInfoStatistical = GetFileStatisticalSign(invoiceId, companyId);
            //InserEmailFileAttachFile(fileInfoStatistical, emailActiveId, invoiceId, 1);
            //ExportFileInfo fileInfoInvoiceGift = GetFileInvoiceGiftSign(invoiceId, companyId);
            //InserEmailFileAttachFile(fileInfoInvoiceGift, emailActiveId, invoiceId, 2);
        }
        private void InsertFileAttachSendByMonth(string CustomerCode, long emailActiveId)
        {
            ExportFileInfo fileAttach = new ExportFileInfo();
            var buildingFileName = CustomerCode + "_" + DateTime.Today.Month.ToString() + DateTime.Today.Year.ToString() + ".zip";
            fileAttach.FileName = "ShinhanBank_Inv_" + buildingFileName;
            fileAttach.FullPathFileName = Path.Combine(WebConfigurationManager.AppSettings["FolderInvoiceFile"] + "\\FileSendByMonth") + "\\" + fileAttach.FileName;
            InserEmailFileAttachFile(fileAttach, emailActiveId);
        }
        private void InserEmailFileAttachFile(ExportFileInfo fileInfo, long emailActiveId, long? invoiceId = null, int? flagTye = null)
        {
            if (fileInfo == null || !File.Exists(fileInfo.FullPathFileName.ConvertToString()))
            {
                return;
            }
            //Dinh dang name file attach ID_InvocieNo_Signed
            if (invoiceId != null)
            {
                INVOICE invoice = GetInvoice((long)invoiceId);
                //string date = invoice.RELEASEDDATE.Value.ToString("ddMMyyy");
                string extension = Path.GetExtension(fileInfo.FullPathFileName);
                //fileInfo.FileName = string.Format("{0}_sign.{4}", invoiceId, extension);
                fileInfo.FileName = Path.GetFileName(fileInfo.FullPathFileName);
                logger.Error("filename", new Exception(fileInfo.FileName));
                //switch (flagTye)
                //{
                //    case 1:
                //        fileInfo.FileName = string.Format("{0}_{1}_statistical_Signed.pdf", invoiceId, invoice.NO);
                //        break;
                //    case 2:
                //        fileInfo.FileName = string.Format("{0}_{1}_invoiceGift_Signed.pdf", invoiceId, invoice.NO);
                //        break;
                //    default:
                //        string date = invoice.RELEASEDDATE.Value.ToString("ddMMyyy");
                //        fileInfo.FileName = string.Format("{0}_{1}_{2}_{3}_Signed.{4}", invoiceId, invoice.SYMBOL.ToString().Replace("/", ""), invoice.NO, date, formatFile[1]);
                //        break;
                //}


            }

            EMAILACTIVEFILEATTACH emailactiveFileAttach = new EMAILACTIVEFILEATTACH();
            emailactiveFileAttach.CopyData(fileInfo);
            emailactiveFileAttach.EMAILACTIVEID = emailActiveId;
            this.emailActiveFileAttachRepository.Insert(emailactiveFileAttach);
        }
        public ResultValidDate DateInvoiceCanUse(InvoiceDateInfo invoiceDateInfo, bool isNew = true)
        {
            ResultValidDate result = new ResultValidDate();
            if (invoiceDateInfo == null || !invoiceDateInfo.DateOfInvoice.HasValue)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            if (isNew)
            {
                invoiceDateInfo.InvoiceNo = GeneratorInvoiceNoFromBD(invoiceDateInfo.Branch, invoiceDateInfo.RegisterTemplateId, invoiceDateInfo.Symbol);
            }
            result.Result = CheckDateOfInvoice(invoiceDateInfo);
            return result;
        }

        private int CheckDateOfInvoice(InvoiceDateInfo invoiceDateInfo)
        {
            int canUse = 0;
            List<DateTime> dateOfInvoiceCanUse = this.invoiceRepository.DateOfInvoiceCanUse(invoiceDateInfo).OrderBy(p => p).ToList();
            if (!(invoiceDateInfo.DateOfInvoice.Value >= dateOfInvoiceCanUse[0] && invoiceDateInfo.DateOfInvoice.Value <= dateOfInvoiceCanUse[1]))
            {
                canUse = 1;
            }

            return canUse;
        }

        private INVOICE GetInvoice(string verificationCode)
        {
            if (verificationCode.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            INVOICE invoice = this.invoiceRepository.ResearchInvoice(verificationCode);
            if (invoice == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, string.Format("Not found invoice by verification code {0}", verificationCode));
            }

            return invoice;
        }

        public InvoiceClientSign GetInvoiceOfClient(long invoiceId)
        {
            InvoiceClientSign invoice = this.invoiceRepository.GetInvoiceOfClient(invoiceId);
            if (invoice == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, string.Format("Not found invoice by id code {0}", invoiceId));
            }

            return invoice;
        }
        public ANNOUNCEMENT UpdateAnnouncementStatus(long id, long companyId, long userAction)
        {
            ANNOUNCEMENT currentAnnouncement = GetAnnouncement(id, 1, companyId);
            if (currentAnnouncement == null)
            {
                return null;
            }

            currentAnnouncement.ANNOUNCEMENTSTATUS = (int)AnnouncementStatus.Successfull;
            currentAnnouncement.PROCESSDATE = DateTime.Now;
            currentAnnouncement.PROCESSBY = userAction;
            this.announcementRepository.Update(currentAnnouncement);
            return currentAnnouncement;
        }

        public ANNOUNCEMENT GetAnnouncement(long id, int announcementType, long? companyId)
        {
            ANNOUNCEMENT announcements = this.announcementRepository.GetByInvoiceId(id, announcementType, companyId);
            if (announcements == null)
            {
                throw new BusinessLogicException(ResultCode.AnnouncementNotExist, string.Format("The announcement does not exist for this invoice [{0}]", id));
            }

            return announcements;
        }

        public InvoiceReleaseDate MaxReleaseDate(InvoiceInfo invoiceInfo)
        {
            if (invoiceInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var maxReleaseDate = this.invoiceRepository.MaxReleaseDate(invoiceInfo)?.Date;

            return new InvoiceReleaseDate(maxReleaseDate ?? new DateTime());
        }

        public InvoiceNo MinInvoiceNo(InvoiceInfo invoiceInfo)
        {
            if (invoiceInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var minInvoiceNo = this.invoiceRepository.MinInvoiceNo(invoiceInfo) ?? "0".PadLeft(7, '0');

            return new InvoiceNo(minInvoiceNo);
        }

        public InvoiceNo MaxInvoiceNo(InvoiceInfo invoiceInfo)
        {
            if (invoiceInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var notificationUseInvoiceDetails = this.noticeInvoiceDetailRepository.GetNotificationUseInvoiceDetails(invoiceInfo.CompanyId ?? 0, invoiceInfo.RegisterTemplateId, invoiceInfo.Symbol.Replace("/", ""));
            var maxNotificationUseInvoiceDetail = notificationUseInvoiceDetails.OrderByDescending(p => p.NUMBERTO).FirstOrDefault();

            if (maxNotificationUseInvoiceDetail == null)
            {
                return new InvoiceNo("0".PadLeft(7, '0'));
            }

            return new InvoiceNo(maxNotificationUseInvoiceDetail.NUMBERTO.PadLeft(7, '0'));
        }

        public ResultApproved RevertApprovedInvoice(ApprovedInvoice approvedInvoice, UserSessionInfo currentUser)
        {
            if (approvedInvoice == null || approvedInvoice.ReleaseInvoiceInfos.Count == 0)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            ResultApproved result = new ResultApproved();
            foreach (var p in approvedInvoice.ReleaseInvoiceInfos)
            {
                var currentInvoice = GetInvoice(p.InvoiceId);

                this.ValidateRevertApprovedInvoiceItem(currentInvoice, approvedInvoice);

                if (!currentInvoice.CLIENTID.HasValue)
                {
                    result.RowError.Add(new ApprovedRowError(ResultCode.InvoiceApprovedNotHasCustomer, currentInvoice.NO));
                    continue;
                }
                if (currentInvoice.INVOICEDETAILs.Count == 0)
                {
                    result.RowError.Add(new ApprovedRowError(ResultCode.InvoiceApprovedNotHasItem, currentInvoice.NO));
                    continue;
                }
                currentInvoice.INVOICESTATUS = (int)InvoiceStatus.New;
                currentInvoice.UPDATEDBY = currentUser.Id;
                currentInvoice.UPDATEDDATE = DateTime.Now;

                this.invoiceRepository.Update(currentInvoice);
            }
            if (result.RowError.Count > 0)
            {
                result.ErrorCode = ResultCode.DataInvalid;
                result.Message = MsgApiResponse.DataInvalid;
            }

            return result;
        }
        private void ValidateRevertApprovedInvoiceItem(INVOICE currentInvoice, ApprovedInvoice approvedInvoice)
        {
            if (currentInvoice == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, "Invoice not exists!");
            }

            if (currentInvoice.NO.Trim('0') != "")
            {
                throw new BusinessLogicException(ResultCode.InvoiceHadNumber, "Hóa đơn đã có số hóa đơn, không thể hủy duyệt!");
            }

            if (currentInvoice.PARENTID != null)
            {
                var invoiceParent = GetInvoice((long)currentInvoice.PARENTID);
                if (invoiceParent.INVOICESTATUS == (int)InvoiceStatus.Cancel)
                {
                    throw new BusinessLogicException(ResultCode.InvoiceTypeChanged, "Hóa đơn đang xử lý đã bị thay đổi trạng thái điều chỉnh. Bạn không thể hủy duyệt hóa đơn này.");
                }
            }

            List<string> symbols = GetSymbolsInReportCancellingInvoice(approvedInvoice.CompanyId, (long)currentInvoice.REGISTERTEMPLATEID);
            if (symbols.Count > 0)
            {
                foreach (var item in symbols)
                {
                    if (currentInvoice.SYMBOL == item)
                    {
                        throw new BusinessLogicException(ResultCode.InvoiceNotApproveSymbol, "Dãy hóa đơn đã bị hủy,không thể thực hiện được chức năng hủy duyệt hóa đơn.");
                    }
                }
            }
        }

        public string ImportXmlInvoice(string invoice)
        {
            string result = String.Empty;
            string FileName = "invoiceXmlTemp_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss_fff") + ".xml";
            string folderInvoiceXml = Path.Combine(config.FullPathFileAsset, "XmlImportInvoice\\" + FileName);
            File.WriteAllText(folderInvoiceXml, invoice);
            InvoiceInfo invInfo;
            using (StreamReader reader = new StreamReader(folderInvoiceXml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(InvoiceInfo));
                invInfo = (InvoiceInfo)serializer.Deserialize(reader);
            }
            File.Delete(folderInvoiceXml);
            //process data
            var company = myCompanyRepository.GetByTaxCode(invInfo.SellerTaxCode);
            if (company != null)
            {
                invInfo.CompanyId = company.COMPANYSID;
                var template = registerTemplates.GetByCompanyId(company.COMPANYSID, invInfo.InvoiceTemplateCode);
                if (template != null)
                {
                    invInfo.RegisterTemplateId = template.ID;
                }
            }
            //process client
            invInfo.ClientId = ProcessDataClient(invInfo).ID;
            invInfo.InvoiceDetail.ForEach(item =>
            {
                item.Total = item.Quantity * item.Price;
            });
            invInfo.No = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
            var curency = currencyRepository.GetByCode(invInfo.CurrencyCode ?? DefaultFields.CURRENCY_VND);
            invInfo.CurrencyId = curency.ID;
            INVOICE invoiceNew = CreateFromXml(invInfo);
            if (invoiceNew.ID != 0)
            {
                result = "<InvoiceInfo><InvoiceNo>" + invInfo.No + "</InvoiceNo>"
                    + "<InvoiceSample>" + invInfo.InvoiceTemplateCode + "</InvoiceSample>"
                    + "<InvoiceSymbol>" + invInfo.Symbol + "</InvoiceSymbol>"
                    + "<InvoiceDate>" + DateTime.Now.ToString("dd/MM/yyyy") + "</InvoiceDate></InvoiceInfo>";
            }
            return result;
        }
        public CLIENT ProcessDataClient(InvoiceInfo invoice)
        {
            CLIENT clientCurrent = this.clientRepository.GetByPersonalContactAndAddress(invoice.CustomerName, invoice.Address);
            if (clientCurrent == null)
            {
                CLIENT client = new CLIENT();
                client.TAXCODE = invoice.TaxCode;
                if (client.TAXCODE != null || client.TAXCODE != "")
                {
                    client.CUSTOMERNAME = invoice.CustomerName;
                    client.ISORG = true;
                }
                else
                {
                    client.PERSONCONTACT = invoice.CustomerName;
                    client.ISORG = false;
                }
                client.ADDRESS = invoice.Address;
                client.EMAIL = invoice.Email;
                //25384 update email vao Email nhận hóa đơn, do email tai hoa don dang load data tu client.RECEIVEDINVOICEEMAIL
                client.RECEIVEDINVOICEEMAIL = invoice.Email;
                client.CREATEDDATE = DateTime.Now;
                this.clientRepository.Insert(client);
                var customerCode = client.ID.ToString();
                var maxCustomerCode = this.clientRepository.GetMaxCustomerCode(client.ID);
                if (maxCustomerCode != 0)
                {
                    customerCode += $"_{maxCustomerCode.ToString()}";
                }
                client.CUSTOMERCODE = customerCode;
                this.clientRepository.Update(client);
                return client;
            }
            else
            {
                return clientCurrent;
            }

        }
        public PRODUCT ProcessDataProduct(string productName, string unitName, decimal price)
        {
            PRODUCT productCurent = productRepository.FilterProductNameUnitCode(productName, unitName);
            if (productCurent == null)
            {
                UNITLIST unitcurent = unitListRepository.GetByName(unitName);
                if (unitcurent == null)
                {
                    UNITLIST unit = new UNITLIST();
                    unit.NAME = unitName;
                    unit.CODE = convertToUnSign3(unitName).ToUpper();
                    this.unitListRepository.Insert(unit);
                    unitcurent = unit;
                }
                PRODUCT product = new PRODUCT();
                product.PRODUCTNAME = productName;
                product.PRICE = price;
                product.CREATEDDATE = DateTime.Now;
                product.UNIT = unitcurent.NAME;
                product.UNITID = unitcurent.ID;
                this.productRepository.Insert(product);
                return product;
            }
            else
            {
                return productCurent;
            }
        }
        private string convertToUnSign3(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public long CreateEmailAccountInfo(CLIENT client)
        {
            long emailNoticeId = 0;
            if (client == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            emailNoticeId = CreateEmailActive(client);
            return emailNoticeId;
        }

        private long CreateEmailActive(CLIENT receiver)
        {
            var clientId = receiver.ID;
            ReceiverAccountActiveInfo NoticeAccountInfo = this.clientRepository.GetClientAccountInfo(clientId);
            EmailInfo emailInfo = EmailTemplate.GetEmail(this.emailConfig, (int)SendEmailType.NoticeAccountCustomer, NoticeAccountInfo);
            long emailActiveId = this.email.CreateEmailActive(emailInfo, 1);
            return emailActiveId;
        }

        public ResultCode ImportInvoiceFile(bool isJob)
        {
            lock (lockImport)
            {
                importFileDAO.ImportFileAuto_BIDC(isJob);
            }

            return ResultCode.ImportInvoiceFileSuccess;
        }

        public BIDCImportFromAPIOutput BIDCImportInvoiceExternalApi(XmlDocument xmlDocument)
        {
            lock (lockImport)
            {
                return importFileDAO.BIDCImportInvoiceExternalApi(xmlDocument);
            }
        }
        private void CancelInvoiceSino()
        {
            ReleaseInvoiceMaster releaseInvoice = new ReleaseInvoiceMaster();
            releaseInvoice.CompanyId = this.currentUser.Company.Id;
            releaseInvoice.UserAction = this.currentUser.Id;
            releaseInvoice.UserActionId = this.currentUser.Id;
            releaseInvoice.ReleasedDate = DateTime.Now;
            releaseInvoice.ReleaseInvoiceInfos = this.invoiceRepository.GetListInvoiceNeedDelete().ToList();
            if (releaseInvoice.ReleaseInvoiceInfos.Count > 0)
            {
                this.releaseInvoiceBO.JobSignFile(releaseInvoice, this.currentUser);
            }
        }

        public IEnumerable<InvoiceMaster> FilterSubstitute(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceMasters = this.invoiceRepository.FilterSubstitute(condition);

            return invoiceMasters;
        }
        public long CountSubstitute(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.invoiceRepository.CountSubstitute(condition);
        }

        public InvoiceInfo GetByInvoiceNo(ConditionSearchInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.invoiceRepository.GetByInvoiceNo(condition);
        }

        private INVOICE InsertOrUpdateSubstitute(InvoiceInfo info)
        {
            if (info == null || info.Symbol.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!info.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            this.CreateOutTransaction(info, true);
            return invoiceRepository.GetById(info.Id);
        }

        /// <summary>
        /// Làm tròn các field Amount thành 0 chữ số thập phân nếu là VND
        /// </summary>
        /// <param name="systemSetting"></param>
        /// <param name="currencyCode"></param>
        private void StandardSystemSettingIfVND(SystemSettingInfo systemSetting, string currencyCode)
        {
            //&& systemSetting.UseSystemSettingVND == false
            if (currencyCode == DefaultFields.CURRENCY_VND)
            {
                systemSetting.Amount = 0;
                systemSetting.ConvertedAmount = 0;
                systemSetting.Price = 0;
            }
        }

        private void StandardSystemSettingIfVNDBTH(SystemSettingInfo systemSetting, string currencyCode)
        {
            systemSetting.Amount = 0;
            systemSetting.ConvertedAmount = 0;
        }
        public ExportFileInfo ExportExcel(ConditionSearchInvoice condition)
        {
            var dataRport = new InvoiceExport(condition.CurrentUser?.Company);
            dataRport.Items = invoiceRepository.FilterInvoice(condition).ToList();
            ExportFileInfo file = new ExportFileInfo();
            if (condition.InvoiceStatus.FirstOrDefault() == (int)InvoiceStatus.Cancel)
            {
                ReportInvoiceCancelledView excel = new ReportInvoiceCancelledView(dataRport, config, condition.Language);
                file = excel.ExportFile();
            }
            else if (condition.InvoiceStatus.FirstOrDefault() == (int)InvoiceStatus.Delete)
            {
                ReportInvoiceDeletedView excel = new ReportInvoiceDeletedView(dataRport, config, condition.Language);
                file = excel.ExportFile();
            }
            return file;
        }

        public ExportFileInfo ExportExcelAdjust(SearchReplacedInvoice condition, int status)
        {
            var dataRport = new InvoiceExport(condition.CurrentUser?.Company);
            dataRport.Replaced = invoiceRepository.FilterReplacedInvoice(condition).ToList();
            ExportFileInfo file = new ExportFileInfo();
            if (status == 1)
            {
                ReportInvoiceAdjustmentView excel = new ReportInvoiceAdjustmentView(dataRport, config, condition.Language);
                file = excel.ExportFile();
            }
            else if (status == 2)
            {
                ReportInvoiceReplacedView excel = new ReportInvoiceReplacedView(dataRport, config, condition.Language);
                file = excel.ExportFile();
            }
            return file;
        }

        public ExportFileInfo ExportExcelInvoices(ConditionSearchInvoice condition)
        {
            var dataRport = new InvoiceExport(condition.CurrentUser?.Company);
            dataRport.Items = invoiceRepository.FilterInvoiceSP(condition).ToList();
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            foreach (var item in dataRport.Items)
            {
                FormatBySystemSetting(item, systemSettingInfo);
            }
            ReportInvoicesView excel = new ReportInvoicesView(dataRport, config, systemSettingInfo, condition.Language);
            return excel.ExportFile();
        }

        public ExportFileInfo ExportExcelCkInvoiceNumber(ConditionSearchCheckNumberInvoice conditionSrc)
        {
            var dataRport = new InvoiceCkNumberExport(conditionSrc.CurrentUser?.Company);
            dataRport.Items = invoiceRepository.FilterInvoiceCheckDaily(conditionSrc).ToList();
            ReportInvoicesCkNumberView excel = new ReportInvoicesCkNumberView(dataRport, config);
            return excel.ExportFile();
        }
        public FooterConfig GetConfigInfo()
        {
            FooterConfig configInfor = new FooterConfig();

            configInfor.CompanyNameConfig = this.GetConfig("CompanyName");
            configInfor.TaxCodeConfig = this.GetConfig("TaxCode");
            configInfor.PhoneConfig = this.GetConfig("Phone");
            configInfor.CompanyLinkConfig = this.GetConfig("CompanyLink");
            configInfor.CompanyLinkConfigShow = this.GetConfig("CompanyLinkShow");
            configInfor.PortalCompanyLinkConfig = this.GetConfig("PortalCompanyLink");
            configInfor.PortalCompanyLinkConfigShow = this.GetConfig("PortalCompanyLinkShow");
            configInfor.LinkAgencyLogin = this.GetConfig("LinkAgencyLogin");

            return configInfor;
        }

        public List<ReportHistoryXml> ListReportHistoryXml(ConditionReportDetailUse condition, SIGNATURE signature)
        {
            ExportFileInfo fileInfo = null;
            var companyInfo = this.myCompanyRepository.GetById((long)condition.Branch);
            var myCompanyInfo = new CompanyInfo(companyInfo);
            var dataRport = new ReportCombineExport(myCompanyInfo);
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(1);
            List<ReportHistoryXml> lst = new List<ReportHistoryXml>();
            var globallist = this.invoiceRepository.FillterListInvoiceSummaryViewReport(condition).ToList().Select(x =>
            {
                var exChangeRate = x.Currency == DefaultFields.CURRENCY_VND ? 1 : x.CurrencyExchangeRate;
                x.Total *= exChangeRate;
                x.TotalTax *= exChangeRate;
                x.SumAmountInvoice *= exChangeRate;
                return x;
            }).ToList();
            logger.Error("Total count:  " + globallist.Count.ToString(), new Exception(""));
            var listDate = new List<ReportInvoiceDetail>(globallist.GroupBy(x => x.ReleasedDate).Select(x => new ReportInvoiceDetail
            {
                ReleasedDate = x.FirstOrDefault().ReleasedDate
            }).OrderBy(x => x.ReleasedDate));

            var currList = new List<ReportInvoiceDetail>();
            foreach (var dateRelease in listDate)
            {
                logger.Error("Releaseddate: " + dateRelease.ReleasedDate.ToString(), new Exception("Check releaseddate"));

                currList = new List<ReportInvoiceDetail>(globallist.Where(x => x.ReleasedDate == dateRelease.ReleasedDate).ToList());

                logger.Error("Current count:  " + currList.Count.ToString(), new Exception(""));

                var count = currList.Count() / 3000;
                logger.Error("Loop number:  " + count.ToString(), new Exception(""));
                var history = new HISTORYREPORTGENERAL();

                var date = this.declarationRepository.ExpireTokenDate(condition.CompanyId);
                var countDate = (date - DateTime.Now).TotalDays;
                if (countDate < 0)
                {
                    throw new BusinessLogicException(ResultCode.ExpireDateToken, "Token đã hết hạn");
                }

                List<ReportInvoiceDetail> newList = new List<ReportInvoiceDetail>();

                ReportHistoryXml reportHistoryXml = null;
                ReportGeneralMonthlyXml export = null;
                var SBTHDulieuCurrent = this.historyReportGeneralRepository.GetSBTHDLIEU(condition.Branch);

                for (int i = 0; i <= count; i++)
                {
                    SBTHDulieuCurrent = SBTHDulieuCurrent + 1;

                    newList = new List<ReportInvoiceDetail>(currList.Skip(i * 3000).Take(3000).ToList());

                    if (newList.Count() > 0)
                    {
                        dataRport.Items = newList;

                        condition.listInvoiceId = newList.Select(x => x.Id).ToList();

                        Parallel.ForEach(dataRport.Items, data =>
                        {
                            FormatBySystemSettingBTH(data, systemSettingInfo);
                        });

                        dataRport.Month = condition.Month;
                        dataRport.Year = condition.Year;
                        dataRport.Time = condition.Time == 0 ? 1 : 0;
                        dataRport.TimeNo = condition.TimeNo;
                        LKDLieu.ListLKDLieu.TryGetValue(condition.IsMonth, out string periodsGeneral);
                        dataRport.periods = periodsGeneral;
                        dataRport.IsMonth = condition.IsMonth;
                        dataRport.SBTHDulieu = (decimal)SBTHDulieuCurrent;//this.historyReportGeneralRepository.GetSBTHDLIEU(condition.IsMonth, condition.Year, condition.Month, null, condition.Branch);
                        condition.SBTHDulieu = (int)dataRport.SBTHDulieu;
                        logger.Error("SBTHDulieu: " + condition.SBTHDulieu, new Exception("SBTHDulieu"));
                        export = new ReportGeneralMonthlyXml(dataRport, config);

                        var XMLContextob = export.WriteFileXML();
                        string XMLContext = "";
                        if (XMLContextob != null)
                        {
                            XMLContext = XMLContextob.ToString();
                        }

                        //historyId = history.ID;

                        fileInfo = new ExportFileInfo();

                        fileInfo.FileName = string.Format("Bao-cao-tong-tong-hop-du-lieu-hoa-don_{0}_{1}_{2}.xml", myCompanyInfo.BranchId,
                            dataRport.SBTHDulieu.ToString(), dateRelease.ReleasedDate.Value.ToString("yyyyMMdd") + DateTime.Now.ToString("HHmmssfff"));

                        fileInfo.FullPathFileName = Path.Combine(this.folderStore, condition.Branch.ToString(), "Report", fileInfo.FileName);

                        string tempFolder = Path.GetDirectoryName(fileInfo.FullPathFileName);

                        if (!Directory.Exists(tempFolder))
                        {
                            Directory.CreateDirectory(tempFolder);
                        }
                        if (!File.Exists(fileInfo.FullPathFileName))
                        {
                            File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
                        }
                        else
                        {
                            File.Delete(fileInfo.FullPathFileName);
                            File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
                        }

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        string path = CallApiSignReport(fileInfo.FullPathFileName, signature);

                        try
                        {
                            if (File.Exists(path))
                            {
                                fileInfo.FullPathFileName = path;

                                history = this.historyReportGeneralBO.CreateHistoryGeneral(condition, fileInfo.FileName);

                                reportHistoryXml = new ReportHistoryXml(fileInfo, history.ID, condition.listInvoiceId);
                                reportHistoryXml.CompanyTaxCode = myCompanyInfo.TaxCode;

                                lst.Add(reportHistoryXml);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error sign", ex);
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }



            //Console.WriteLine(lst);

            return lst;
        }

        public List<ReportHistoryXml> ListReportHistoryXml_Auto(ConditionReportDetailUse condition, SIGNATURE signature)
        {
            ExportFileInfo fileInfo = null;
            var companyInfo = this.myCompanyRepository.GetById((long)condition.Branch);
            var myCompanyInfo = new CompanyInfo(companyInfo);
            var dataRport = new ReportCombineExport(myCompanyInfo);
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(1);
            List<ReportHistoryXml> lst = new List<ReportHistoryXml>();
            var globallist = this.invoiceRepository.FillterListInvoiceSummaryViewReport_Auto(condition).ToList().Select(x =>
            {
                var exChangeRate = x.Currency == DefaultFields.CURRENCY_VND ? 1 : x.CurrencyExchangeRate;
                x.Total *= exChangeRate;
                x.TotalTax *= exChangeRate;
                x.SumAmountInvoice *= exChangeRate;
                return x;
            }).ToList();
            logger.Error("Total count:  " + globallist.Count.ToString(), new Exception(""));
            var listDate = new List<ReportInvoiceDetail>(globallist.GroupBy(x => x.ReleasedDate).Select(x => new ReportInvoiceDetail
            {
                ReleasedDate = x.FirstOrDefault().ReleasedDate
            }).OrderBy(x => x.ReleasedDate));

            var currList = new List<ReportInvoiceDetail>();
            foreach (var dateRelease in listDate)
            {
                logger.Error("Releaseddate: " + dateRelease.ReleasedDate.ToString(), new Exception("Check releaseddate"));

                currList = new List<ReportInvoiceDetail>(globallist.Where(x => x.ReleasedDate == dateRelease.ReleasedDate).ToList());

                logger.Error("Current count:  " + currList.Count.ToString(), new Exception(""));

                var count = currList.Count() / 3000;
                logger.Error("Loop number:  " + count.ToString(), new Exception(""));
                var history = new HISTORYREPORTGENERAL();

                List<ReportInvoiceDetail> newList = new List<ReportInvoiceDetail>();

                ReportHistoryXml reportHistoryXml = null;
                ReportGeneralMonthlyXml export = null;
                var SBTHDulieuCurrent = this.historyReportGeneralRepository.GetSBTHDLIEU(condition.Branch);

                for (int i = 0; i <= count; i++)
                {
                    SBTHDulieuCurrent = SBTHDulieuCurrent + 1;

                    newList = new List<ReportInvoiceDetail>(currList.Skip(i * 3000).Take(3000).ToList());

                    if (newList.Count() > 0)
                    {
                        dataRport.Items = newList;

                        condition.listInvoiceId = newList.Select(x => x.Id).ToList();

                        Parallel.ForEach(dataRport.Items, data =>
                        {
                            FormatBySystemSettingBTH(data, systemSettingInfo);
                        });

                        dataRport.Month = condition.Month;
                        dataRport.Year = condition.Year;
                        dataRport.Time = condition.Time == 0 ? 1 : 0;
                        dataRport.TimeNo = condition.TimeNo;
                        LKDLieu.ListLKDLieu.TryGetValue(condition.IsMonth, out string periodsGeneral);
                        dataRport.periods = periodsGeneral;
                        dataRport.IsMonth = condition.IsMonth;
                        dataRport.SBTHDulieu = (decimal)SBTHDulieuCurrent;//this.historyReportGeneralRepository.GetSBTHDLIEU(condition.IsMonth, condition.Year, condition.Month, null, condition.Branch);
                        condition.SBTHDulieu = (int)dataRport.SBTHDulieu;
                        logger.Error("SBTHDulieu: " + condition.SBTHDulieu, new Exception("SBTHDulieu"));
                        export = new ReportGeneralMonthlyXml(dataRport, config);

                        try
                        {
                            var XMLContextob = export.WriteFileXML();

                            string XMLContext = "";
                            if (XMLContextob != null)
                            {
                                XMLContext = XMLContextob.ToString();
                            }

                            fileInfo = new ExportFileInfo();

                            fileInfo.FileName = string.Format("Bao-cao-tong-tong-hop-du-lieu-hoa-don_{0}_{1}_{2}.xml", myCompanyInfo.BranchId,
                                dataRport.SBTHDulieu.ToString(), dateRelease.ReleasedDate.Value.ToString("yyyyMMdd") + DateTime.Now.ToString("HHmmssfff"));

                            fileInfo.FullPathFileName = Path.Combine(this.folderStore, condition.Branch.ToString(), "Report", fileInfo.FileName);

                            string tempFolder = Path.GetDirectoryName(fileInfo.FullPathFileName);

                            if (!Directory.Exists(tempFolder))
                            {
                                Directory.CreateDirectory(tempFolder);
                            }
                            if (!File.Exists(fileInfo.FullPathFileName))
                            {
                                File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
                            }
                            else
                            {
                                File.Delete(fileInfo.FullPathFileName);
                                File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
                            }

                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error File XML", ex);
                            continue;

                        }
                        
                        //historyId = history.ID;

                        

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        string path = CallApiSignReport(fileInfo.FullPathFileName, signature);

                        try
                        {
                            if (File.Exists(path))
                            {
                                fileInfo.FullPathFileName = path;

                                history = this.historyReportGeneralBO.CreateHistoryGeneral(condition, fileInfo.FileName);

                                reportHistoryXml = new ReportHistoryXml(fileInfo, history.ID, condition.listInvoiceId);
                                reportHistoryXml.CompanyTaxCode = myCompanyInfo.TaxCode;

                                lst.Add(reportHistoryXml);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error sign", ex);
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }



            //Console.WriteLine(lst);

            return lst;
        }

        public ReportHistoryXml SendMQTXml(ConditionReportDetailUse condition, HISTORYREPORTGENERAL history, SIGNATURE singnature)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            var dataRport = new ReportCombineExport(condition.CurrentUser?.Company);
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            dataRport.Items = this.invoiceRepository.FillterListInvoiceSummary(condition).ToList();
            foreach (var item in dataRport.Items)
            {
                FormatBySystemSetting(item, systemSettingInfo);
            }
            dataRport.Month = condition.Month;
            dataRport.Year = condition.Year;
            dataRport.Time = condition.Time;
            dataRport.TimeNo = condition.TimeNo;
            LKDLieu.ListLKDLieu.TryGetValue(condition.IsMonth, out string periodsGeneral);
            dataRport.periods = periodsGeneral;
            dataRport.IsMonth = condition.IsMonth;
            ReportGeneralMonthlyXml export = new ReportGeneralMonthlyXml(dataRport, config);

            var XMLContextob = export.WriteFileXML();
            string XMLContext = "";
            if (XMLContextob != null)
            {
                XMLContext = XMLContextob.ToString();
            }


            fileInfo.FileName = string.Format("Bao-cao-tong-tong-hop-du-lieu-hoa-don_{0}_{1}.xml", history.ID, DateTime.Now.ToString("yyyyMMdd"));
            fileInfo.FullPathFileName = Path.Combine(this.folderStore, condition.CompanyId.ToString(), "Report", fileInfo.FileName);
            //string newFolder = Path.Combine(@"D:\store\unit\",condition.CompanyId.ToString(), "Report", fileInfo.FileName);
            string tempFolder = Path.GetDirectoryName(fileInfo.FullPathFileName);

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            if (!File.Exists(fileInfo.FullPathFileName))
            {
                File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
            }
            else
            {
                File.Delete(fileInfo.FullPathFileName);
                File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //string path = CallApiSignReport(tempFolder);

            string path = CallApiSignReport(fileInfo.FullPathFileName, singnature);
            //string pathFolder = Path.Combine(@"C:\store\unit", condition.CompanyId.ToString(), @"Report\Sign", fileInfo.FileName);
            if (File.Exists(path))
                fileInfo.FullPathFileName = path;

            ReportHistoryXml reportHistoryXml = new ReportHistoryXml(fileInfo, history.ID, null);


            return reportHistoryXml;

        }
        private string CallApiSignReport(string pathXml, SIGNATURE signature, string tagKey = null)
        {
            bool SignHsm = bool.Parse(WebConfigurationManager.AppSettings["SignHSM"] ?? "false");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string json = new JavaScriptSerializer().Serialize(new
            {
                Password = signature.PASSWORD,
                TagKey = tagKey == null ? "DLBTHop" : tagKey,
                TagSign = "NNT",
                SerialNumber = signature.SERIALNUMBER,
                PathFileXml = pathXml,
                Slot = signature.SLOTS,
                SignHSM = SignHsm
                //Certificatepath= certificatepath,
            });
            string response = String.Empty;
            string DATA = json.ToString();
            string UrlServerSign = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSign"], ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionServerSignXml);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlServerSign);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(DATA);
            request.ContentLength = byteArray.Length;
            request.Timeout = 18000000;//tăng timeout gọi api
            using (Stream webStream = request.GetRequestStream())
            //using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
            {
                webStream.Write(byteArray, 0, byteArray.Length);
                webStream.Close();
            }
            WebResponse webResponse = request.GetResponse();
            using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
            {
                StreamReader responseReader = new StreamReader(webStream);
                response = responseReader.ReadToEnd().ToString();
                response = JsonConvert.DeserializeObject(response).ToString();
            }
            return response.ToString();
        }

        public HISTORYREPORTGENERAL CreateHistoryGeneral(ConditionReportDetailUse condition)
        {

            HISTORYREPORTGENERAL history = new HISTORYREPORTGENERAL();
            history.PERIODSREPORT = condition.IsMonth;
            history.MONTH = condition.Month;
            history.YEAR = condition.Year;
            if (condition.TimeNo == 0)
            {
                history.ADDITIONALTIMES = 0;
            }
            else
            {
                history.ADDITIONALTIMES = condition.TimeNo;
            }
            this.historyReportGeneralRepository.Insert(history);
            return history;

        }
        //private double GetNextInvoiceNo(long companyId, long templateid, string symbol)
        //{
        //    lock (lockObject)
        //    {
        //        double maxInvoiceNo = 0;
        //        var maxInvoice = GetMaxInvoice(companyId, templateid, symbol);
        //        double.TryParse(maxInvoice, out maxInvoiceNo);
        //        return ++maxInvoiceNo;
        //    }
        //}
        #region sendMail
        public void SendMailJob(long companyId)
        {
            var listInv = this.invoiceRepository.GetInvoiceSendMails(companyId);
            var total = listInv.Count();
            if (total == 0)
            {
                //logger.Error("Invoice not found for send mail:  " + companyId, new Exception("job send mail"));
                return;
            }
            int thread = 10;
            var skip = 0;
            var take = (total / thread) + (thread - 1);
            List<Task> TaskList = new List<Task>();
            for (int i = 1; i <= thread; i++)
            {
                //task
                var invoices = new List<InvoicePrintModel>(listInv.Skip(skip).Take(take));
                //logger.Error("Start task zip file" + i, new Exception(invoices.Count().ToString()));
                var task = Task.Run(() => CreateZipFile(invoices));
                skip = take * i;
                TaskList.Add(task);
            }
            Task.WaitAll(TaskList.ToArray());
            logger.Error("Start copy zip to SFTP: " + companyId, new Exception("job send mail"));
            CopyFileToSFTP(listInv);
            logger.Error("Start insert email: " + companyId, new Exception("job send mail"));
            var lstmail = SendMailsJob(listInv);
            logger.Error("Success sendmail branchs: " + companyId + ", " + lstmail.Count + " invoices", new Exception("job send mail"));
            var taskDel = Task.Run(() => DeleteFolderZipFile(listInv));
            //CreateEmailActiveAuto(lstmail);
        }
        private void DeleteFolderZipFile(List<InvoicePrintModel> invoices)
        {
            foreach (var invoice in invoices)
            {
                try
                {
                    string folderName = "ShinhanBank_Inv_" + invoice.NO + "_" + invoice.COMPANYID + "_" + invoice.RELEASEDDATE.Value.ToString("ddMMyyy");
                    string folderPath = Path.Combine(this.config.PathInvoiceFile, invoice.COMPANYID.ToString(), "Releases", "Sign", invoice.RELEASEDDATE.Value.Year.ToString(), invoice.RELEASEDDATE.Value.ToString("MM"), invoice.RELEASEDDATE.Value.ToString("dd"), folderName);

                    if (Directory.Exists(folderPath))
                    {
                        Directory.Delete(folderPath, true);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("DeleteFolderZipFile", ex);
                }
            }

        }
        private void CreateZipFile(List<InvoicePrintModel> invoices)
        {
            foreach (var invoice in invoices)
            {
                try
                {
                    string folderName = "ShinhanBank_Inv_" + invoice.NO + "_" + invoice.COMPANYID + "_" + invoice.RELEASEDDATE.Value.ToString("ddMMyyy");
                    string folderPath = Path.Combine(this.config.PathInvoiceFile, invoice.COMPANYID.ToString(), "Releases", "Sign", invoice.RELEASEDDATE.Value.Year.ToString(), invoice.RELEASEDDATE.Value.ToString("MM"), invoice.RELEASEDDATE.Value.ToString("dd"), folderName);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string pathFileInvoice = Path.Combine(this.config.PathInvoiceFile, invoice.COMPANYID.ToString(), "Releases", "Sign", invoice.RELEASEDDATE.Value.Year.ToString(), invoice.RELEASEDDATE.Value.ToString("MM"), invoice.RELEASEDDATE.Value.ToString("dd"));
                    string signedPdfPath = Path.Combine(pathFileInvoice, invoice.ID + "_sign.pdf");
                    string signedXmlPath = Path.Combine(pathFileInvoice, invoice.ID + "_sign.xml");
                    if (!File.Exists(signedPdfPath) || !File.Exists(signedXmlPath))
                    {
                        continue;
                    }
                    List<string> listFiles = new List<string>();
                    listFiles.Add(signedPdfPath);
                    listFiles.Add(signedXmlPath);
                    string pathFileTo = String.Empty;
                    foreach (var file in listFiles)
                    {
                        pathFileTo = Path.Combine(folderPath, Path.GetFileName(file));
                        File.Copy(file, pathFileTo);
                    }
                    zipFile(invoice.CUSTOMERTAXCODE, folderPath, folderPath + ".zip");
                    //if (Directory.Exists(folderPath))
                    //{
                    //    Directory.Delete(folderPath, true);
                    //}

                }
                catch (Exception ex)
                {
                    logger.Error("zipFile", ex);
                }
            }

        }
        private void zipFile(string password, string folderTemp, string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (ZipFile zip = new ZipFile())
            {
                zip.Password = password;
                zip.AddDirectory(folderTemp);
                zip.Save(fileName);
            }
        }
        private bool CopyFileToSFTP(List<InvoicePrintModel> invoices)
        {
            try
            {
                string host = WebConfigurationManager.AppSettings["Host"];
                string userName = WebConfigurationManager.AppSettings["UserName"];
                string passWord = WebConfigurationManager.AppSettings["Password"];
                string directory = WebConfigurationManager.AppSettings["Path"];
                int port = Int32.Parse(WebConfigurationManager.AppSettings["Port"].ToString());
                using (SftpClient client = new SftpClient(host, port, userName, passWord))
                {
                    client.Connect();
                    client.ChangeDirectory(directory);

                    foreach (var item in invoices)
                    {

                        string folderName = "ShinhanBank_Inv_" + item.NO + "_" + item.COMPANYID + "_" + item.RELEASEDDATE.Value.ToString("ddMMyyy") + ".zip";
                        string PathFileSource = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Releases", "Sign", item.RELEASEDDATE.Value.Year.ToString(), item.RELEASEDDATE.Value.ToString("MM"), item.RELEASEDDATE.Value.ToString("dd"), folderName);
                        if (File.Exists(PathFileSource))
                        {
                            using (FileStream fs = new FileStream(PathFileSource, FileMode.Open))
                            {
                                client.BufferSize = 4 * 1024;
                                client.UploadFile(fs, Path.GetFileName(PathFileSource));
                            }
                        }
                    }
                }
                logger.Error("CopyFileToSFTP successfull", new Exception(""));
            }
            catch (Exception ex)
            {
                logger.Error("CopyFileToSFTP error", ex);
            }
            return true;
        }
        private void ToSftp(List<InvoicePrintModel> invoices)
        {
            string host = WebConfigurationManager.AppSettings["Host"];
            string userName = WebConfigurationManager.AppSettings["UserName"];
            string passWord = WebConfigurationManager.AppSettings["Password"];
            string directory = WebConfigurationManager.AppSettings["Path"];
            int port = Int32.Parse(WebConfigurationManager.AppSettings["Port"].ToString());
            using (SftpClient client = new SftpClient(host, port, userName, passWord))
            {
                client.Connect();
                client.ChangeDirectory(directory);
                foreach (var item in invoices)
                {

                    string folderName = "ShinhanBank_Inv_" + item.NO + "_" + item.COMPANYID + "_" + item.RELEASEDDATE.Value.ToString("ddMMyyy") + ".zip";
                    string PathFileSource = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Releases", "Sign", item.RELEASEDDATE.Value.Year.ToString(), item.RELEASEDDATE.Value.ToString("MM"), item.RELEASEDDATE.Value.ToString("dd"), folderName);
                    if (File.Exists(PathFileSource))
                    {
                        using (FileStream fs = new FileStream(PathFileSource, FileMode.Open))
                        {
                            client.BufferSize = 4 * 1024;
                            client.UploadFile(fs, Path.GetFileName(PathFileSource));
                        }
                    }
                }
            }

        }

        private List<InvoicePrintModel> SendMailsJob(List<InvoicePrintModel> listInvoice)
        {
            var invoices = new List<INVOICE>();
            string folderName = String.Empty;
            string pathZip = String.Empty;
            var listSend = new List<InvoicePrintModel>();
            foreach (var invoice in listInvoice)
            {
                try
                {
                    folderName = "ShinhanBank_Inv_" + invoice.NO + "_" + invoice.COMPANYID + "_" + invoice.RELEASEDDATE.Value.ToString("ddMMyyy") + ".zip";
                    pathZip = Path.Combine(this.config.PathInvoiceFile, invoice.COMPANYID.ToString(), "Releases", "Sign", invoice.RELEASEDDATE.Value.Year.ToString(), invoice.RELEASEDDATE.Value.ToString("MM"), invoice.RELEASEDDATE.Value.ToString("dd"), folderName);
                    if (File.Exists(pathZip))
                    {
                        var invoiceCur = this.invoiceRepository.GetById(Convert.ToInt64(invoice.ID));
                        long emailActiveid = CreateEmailActiveAuto(invoice);
                        bool result = this.emailActiveBO.JobSendMails(invoice, pathZip, emailActiveid);
                        if (!result)
                        {
                            continue;
                        }
                        invoiceCur.SENDMAILSTATUS = result;
                        invoices.Add(invoiceCur);
                        listSend.Add(invoice);
                    }
                    else
                    {
                        logger.Error("SendMailsJob", new Exception("File not found: " + pathZip));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("SendMailsJob", ex);
                }

            }
            if (invoices.Count() > 0)
            {
                this.invoiceRepository.Update(invoices[0]);
            }
            return listSend;
        }
        private long CreateEmailActiveAuto(InvoicePrintModel invoicePrintModels)
        {
            long emailActiveId = 0;
            try
            {
                SendEmailVerificationCode emailInfo = new SendEmailVerificationCode();//GetEmailVerificationCodeInfo(invoiceVerificationCode);
                emailInfo.VerificationCode = invoicePrintModels.VERIFICATIONCODE;
                emailInfo.CompanyId = Convert.ToInt64(invoicePrintModels.COMPANYID);
                emailInfo.CustomerCode = invoicePrintModels.CUSTOMERCODE;
                emailInfo.CustomerName = invoicePrintModels.CUSTOMERNAME;
                emailInfo.EmailTo = invoicePrintModels.USEREGISTEREMAIL == "1" ? invoicePrintModels.EMAIL : invoicePrintModels.RECEIVEDINVOICEEMAIL;
                emailInfo.InvoiceId = Convert.ToInt64(invoicePrintModels.ID);
                emailInfo.InvoiceNo = invoicePrintModels.NO;
                emailInfo.Symbol = invoicePrintModels.SYMBOL;
                emailInfo.TaxCode = invoicePrintModels.CUSTOMERTAXCODE;
                emailInfo.InvoiceDate = invoicePrintModels.RELEASEDDATE.Value;
                emailInfo.Total = invoicePrintModels.TOTAL ?? 0;
                emailInfo.TotalTax = invoicePrintModels.TOTALTAX ?? 0;
                emailInfo.Sum = invoicePrintModels.SUM ?? 0;
                emailInfo.NumberFormat = this.currentUser.Company.NumberFormat.IsNullOrEmpty() ? "{0:#,##0.###}" : this.currentUser.Company.NumberFormat;
                emailInfo.SystemSetting = this.currentUser.SystemSetting;
                emailInfo.IsOrg = invoicePrintModels.ISORG == "1";
                emailActiveId = CreateEmailActive(emailInfo.CompanyId, emailInfo);
            }
            catch (Exception ex)
            {
                logger.Error("CreateEmailActiveAuto invoiceID: " + invoicePrintModels.ID, ex);
                throw;
            }
            return emailActiveId;

        }

        #endregion
        public decimal? GetNextInvoiceNo2(long companyId, long templateid, string symbol)
        {
            lock (lockObject)
            {
                decimal? maxInvoiceNo = 0;
                var maxInvoice = GetMaxInvoiceNo(companyId, templateid, symbol);
                //double.TryParse(maxInvoice, out maxInvoiceNo);
                return ++maxInvoice;
            }
        }
        private decimal? GetMaxInvoiceNo(long companyId, long templateid, string symbol)
        {
            var invoicesNo = this.invoiceRepository.GetMaxInvoiceNo2(companyId, templateid, symbol);

            decimal? maxInvoice = 0;
            if (invoicesNo != null)
            {
                maxInvoice = invoicesNo;
            }

            return maxInvoice;
        }

        #region JobApproveItemShinhan_Old
        public void UpdateNoForCompany(long? companyId)
        {
            this.invoiceRepository.UpdateNoForCompany(companyId);
        }
        public void JobApproveItemShinhan_Old(List<INVOICE> invoices, MYCOMPANY Company)
        {
            //var invoiceConclude = CreateRelease();

            //List<RELEASEINVOICEDETAIL> releaseDetails = new List<RELEASEINVOICEDETAIL>();
            List<INVOICE> newList = new List<INVOICE>();
            //decimal? i = 1;
            foreach (var invoice in invoices)
            {
                try
                {
                    //ApproveInvoiceItemShinhan_old(invoice, Company);

                    invoice.INVOICESTATUS = (int)InvoiceStatus.Approved;
                    invoice.APPROVEDBY = this.currentUser.Id;
                    invoice.APPROVEDDATE = DateTime.Now;
                    invoice.NO = this.releaseInvoiceBO.GeneratorInvoiceNoFromBD(invoice.COMPANYID, invoice.REGISTERTEMPLATEID.Value, invoice.SYMBOL, invoice.INVOICESTATUS);
                    invoice.INVOICENO = invoice.NO.ToDecimal(0);

                    //logger.QuarztJob(false, $"Get invoice need approve by company done {string.Format("{0}{1}", DateTime.Now.ToString(), invoice.ID).GetHashCode().ToString("x").ToUpper()}");

                    //invoice.VERIFICATIONCODE = GetVerificationCode(invoice.ID);//tạo mã tra cứu : năm, ký hiệu (2 kí tự) + số hóa đơn   
                    invoice.VERIFICATIONCODE = DateTime.Now.Year.ToString().Substring(2, 2) + invoice.SYMBOL.Substring(5) + invoice.INVOICENO;
                    if (invoice.RELEASEDDATE == null)
                    {
                        invoice.RELEASEDDATE = DateTime.Now;
                    }

                    lock (ReleaseInvoiceLock.LockObject)
                    {
                        this.invoiceRepository.Update(invoice);
                    }

                    newList.Add(invoice);

                    //releaseDetails.Add(CreateReleaseDetailShinhan(invoice, invoiceConclude.ID));
                }
                catch (Exception ex)
                {
                    logger.Error("ApproveItem", ex);
                }
                //i = i + 1;
            }


            //this.releaseInvoiceDetaiRepository.InsertMultipe(releaseDetails);
        }

        private void ApproveInvoiceItemShinhan_old(INVOICE item, MYCOMPANY Company)
        {
            //check approve permission
            item.INVOICESTATUS = (int)InvoiceStatus.Approved;
            item.APPROVEDBY = this.currentUser.Id;
            item.APPROVEDDATE = DateTime.Now;
            CLIENT client = this.clientRepository.GetById(item.CLIENTID ?? 0);
            var oldInvoiceNo = item.INVOICENO ?? 0;
            if ((item.INVOICETYPE ?? 0) == 0 && oldInvoiceNo == 0) // enum InvoiceType, InvoiceNo = 0: chưa có số hóa đơn mới update theo client (Bug #29620)
            {
                if (!OtherExtensions.IsCurrentClient(client.CUSTOMERCODE))
                {
                    if (client.ISORG != true)
                    {
                        item.PERSONCONTACT = client.PERSONCONTACT;
                    }
                    item.CUSTOMERNAME = client.CUSTOMERNAME;
                    item.CUSTOMERTAXCODE = client.TAXCODE;
                    item.CUSTOMERADDRESS = client.ADDRESS;
                    item.CUSTOMERBANKACC = client.BANKACCOUNT;
                }

                if (Company.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
                {
                    var parrentCompany = myCompanyRepository.GetById((long)Company.COMPANYID);
                    item.COMPANYNAME = parrentCompany.COMPANYNAME;
                    item.COMPANYADDRESS = parrentCompany.ADDRESS;
                    item.COMPANYTAXCODE = parrentCompany.TAXCODE;
                }
                else
                {
                    var mycompany = myCompanyRepository.GetById(item.COMPANYID);
                    item.COMPANYNAME = mycompany.COMPANYNAME;
                    item.COMPANYADDRESS = mycompany.ADDRESS;
                    item.COMPANYTAXCODE = mycompany.TAXCODE;
                }
            }
        }

        #endregion

        public void JobApproveNew(string day, long? companyId, string symbol, long? invoiceid)
        {
            this.invoiceRepository.Approve_Invoice(day, companyId, symbol, invoiceid);
        }

        public List<CompanySymbolInfo> listSymbol()
        {
            return this.invoiceRepository.GetAllSymbol();
        }

        public DateTime GetPreviousWorkingDate(DateTime currentDate)
        {
            var date = currentDate.AddDays(-1);
            var calendar = this.quarztJobBO.GetWorkingCalendar(date.Year);

            while (true)
            {
                var workingDateFirst = calendar.Where(x => x.Day == date).FirstOrDefault();

                if ((workingDateFirst != null && workingDateFirst.Type == "")
                    || (workingDateFirst == null && date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
                {
                    break;
                }
                else
                {
                    date = date.AddDays(-1);
                    continue;
                }
            }

            return date;
        }

        public ReportGeneralInvoice GetBSLThu(ConditionReportDetailUse condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ReportGeneralInvoice report = new ReportGeneralInvoice(condition.CurrentUser?.Company);
            report.BSLThu = this.invoiceRepository.GetBSLThu(condition);

            return report;
        }
        public ExportFileInfo DownloadHisItem(long hisId)
        {
            var dataRport = this.invoiceRepository.GetInvoiceInfoBTH(hisId);
            ReportHistoryItemView reportExportAllHGRView = new ReportHistoryItemView(dataRport);
            return reportExportAllHGRView.ExportFile();

        }
        public ResultCode processXmlError(string messageCode)
        {
            var minvoice = this.invoiceRepository.ListErrorMinvoice(messageCode);
            foreach (var item in minvoice)
            {
                UpdateXml(item.XML);
            }
            return ResultCode.NoError;
        }
        public void UpdateXml(string xmlString)
        {
            try
            {
                XDocument xdoc = XDocument.Parse(xmlString);

                var listHdonFailed = xdoc.Descendants().Where(x => x.Name.LocalName == "HDon").ToList();
                List<INVOICE> listInv = new List<INVOICE>();
                foreach (var bth in listHdonFailed)
                {
                    var MS = bth.Descendants().FirstOrDefault(x => x.Name.LocalName == "KHMSHDon")?.Value;
                    var KHHDon = bth.Descendants().FirstOrDefault(x => x.Name.LocalName == "KHHDon")?.Value;
                    var SHDon = bth.Descendants().FirstOrDefault(x => x.Name.LocalName == "SHDon")?.Value;
                    var error = bth.Descendants().FirstOrDefault(x => x.Name.LocalName == "MTLoi")?.Value;
                    var errorCode = bth.Descendants().FirstOrDefault(x => x.Name.LocalName == "MLoi")?.Value;
                    string symbol = MS + KHHDon;
                    var InvoiceNo = Decimal.Parse(SHDon);
                    var invoice = this.invoiceRepository.GetByNoSymbol(symbol, InvoiceNo);

                    invoice.BTHERROR = error;//40051
                    invoice.BTHERRORSTATUS = Decimal.Parse(errorCode);

                    listInv.Add(invoice);
                }

                this.invoiceRepository.Update(listInv[0]);
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
