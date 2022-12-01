using BitMiracle.Docotic.Pdf;
using InvoiceServer.Business.Cache;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using Ionic.Zip;
using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace InvoiceServer.Business.BL
{
    public class ReleaseInvoiceBO : IReleaseInvoiceBO
    {
        #region Fields, Properties
        protected readonly static object lockObject = new object();
        private readonly IReleaseInvoiceRepository releaseInvoiceRepository;
        private readonly IReleaseInvoiceDetaiRepository releseInvoiceDetailRepository;
        private readonly IInvoiceRepository invoiceRepository;
        private readonly IDbTransactionManager transaction;
        private readonly PrintConfig config;
        private readonly INotificationUseInvoiceDetailRepository noticeInvoiceDetailRepository;
        private readonly IReportCancellingDetailRepository reportCancellingDetailRepository;
        private readonly IAnnouncementRepository announcementRepository;
        private readonly InvoiceBO invoiceBO;
        private static readonly Logger logger = new Logger();
        private readonly List<long> listInvoiceExceed = new List<long>();
        private readonly UserSessionInfo currentUser;
        private readonly ISystemSettingRepository systemSettingRepository;
        private readonly IEmailActiveBO emailActiveBO;
        private readonly IClientRepository clientRepository;
        private readonly IConfigEmailServerRepository configEmailServerRepository;
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly ISignatureRepository signatureRepository;
        private readonly ISignDetailRepository signDetailRepository;
        private readonly IInvoiceDetailRepository invoiceDetailRepository;
        private static readonly Func<string, string> GetConfig = key => WebConfigurationManager.AppSettings[key];
        private readonly InvoiceDeleteBO invoiceDeleteBO;
        private readonly IDeclarationRepository declarationRepository;
        #endregion

        #region Contructor

        public ReleaseInvoiceBO(IRepositoryFactory repoFactory, PrintConfig config)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");

            this.releaseInvoiceRepository = repoFactory.GetRepository<IReleaseInvoiceRepository>();
            this.releseInvoiceDetailRepository = repoFactory.GetRepository<IReleaseInvoiceDetaiRepository>();
            this.noticeInvoiceDetailRepository = repoFactory.GetRepository<INotificationUseInvoiceDetailRepository>();
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.reportCancellingDetailRepository = repoFactory.GetRepository<IReportCancellingDetailRepository>();
            this.announcementRepository = repoFactory.GetRepository<IAnnouncementRepository>();
            this.config = config;
            this.invoiceBO = new InvoiceBO(repoFactory, config);
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
            this.emailActiveBO = new EmailActiveBO(repoFactory, config);
            this.clientRepository = repoFactory.GetRepository<IClientRepository>();
            this.configEmailServerRepository = repoFactory.GetRepository<IConfigEmailServerRepository>();
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.signatureRepository = repoFactory.GetRepository<ISignatureRepository>();
            this.signDetailRepository = repoFactory.GetRepository<ISignDetailRepository>();
            this.invoiceDetailRepository = repoFactory.GetRepository<IInvoiceDetailRepository>();
            this.declarationRepository = repoFactory.GetRepository<IDeclarationRepository>();
        }

        public ReleaseInvoiceBO(IRepositoryFactory repoFactory, PrintConfig config, UserSessionInfo userSessionInfo, EmailConfig emailConfig)
            : this(repoFactory, config)
        {
            this.currentUser = userSessionInfo;
            this.invoiceBO = new InvoiceBO(repoFactory, config, emailConfig, this.currentUser);

        }
        public ReleaseInvoiceBO(IRepositoryFactory repoFactory, PrintConfig config, UserSessionInfo userSessionInfo)
            : this(repoFactory, config)
        {
            this.currentUser = userSessionInfo;
            this.invoiceDeleteBO = new InvoiceDeleteBO(repoFactory, this.currentUser);
        }
        #endregion

        #region Methods

        public Result ReleaseInvoice(ReleaseGroupInvoice info)
        {
            Result resultRelease = new Result();
            try
            {
                List<INVOICE> invoices = FillterInvoice(info).ToList();
                resultRelease.Count = invoices.Count;
                if (resultRelease.Count == 0)
                {
                    return resultRelease;
                }

                transaction.BeginTransaction();
                RELEASEINVOICE invoiceConclude = InsertReleaseInvoice(info);
                resultRelease.ReleaseId = invoiceConclude.ID;
                InsertReleaseInvoiceDetail(invoiceConclude.ID, invoices);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return resultRelease;
        }

        public Result Create(ReleaseInvoiceMaster info, ReleaseInvoiceStatus status = ReleaseInvoiceStatus.New)
        {
            Result resultRelease = new Result();

            info.ReleaseInvoiceInfos.ForEach(p =>
            {
                var currentInvoice = GetInvoice(p.InvoiceId);
                //check err total number invoice notice with number invoice of contract
                if (currentInvoice.NO == DefaultFields.INVOICE_NO_DEFAULT_VALUE && currentUser.SystemSetting.StepToCreateInvoiceNo != StepToCreateInvoiceNo.WhenApprove)
                {
                    var symbol = currentInvoice.SYMBOL.Replace("/", "");
                    double nextInvoiceNo = GetNextInvoiceNo(currentInvoice.COMPANYID, currentInvoice.REGISTERTEMPLATEID ?? 0, symbol);
                    double maxInvoiceNoCompanyRegister = GetMaxNumberInvoiceCompanyRegister(currentInvoice.COMPANYID, currentInvoice.REGISTERTEMPLATEID ?? 0, symbol);
                    //if (nextInvoiceNo > maxInvoiceNoCompanyRegister)
                    //{
                    //    throw new BusinessLogicException(ResultCode.NumberInvoiceOverloadRegister, string.Format("The invoice number {0} has exceeded the number of registrations", nextInvoiceNo));
                    //}
                }
                // end
            });
            try
            {
                transaction.BeginTransaction();
                RELEASEINVOICE invoiceConclude = InsertReleaseInvoice(info, status);
                InsertReleaseInvoiceDetail(invoiceConclude.ID, invoiceConclude.COMPANYID.Value, info.ReleaseInvoiceInfos);
                resultRelease.Count = info.ReleaseInvoiceInfos.Count;
                resultRelease.ReleaseId = invoiceConclude.ID;
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return resultRelease;
        }

        public Result JobCreate(ReleaseInvoiceMaster info, ReleaseInvoiceStatus status = ReleaseInvoiceStatus.New)
        {
            Result resultRelease = new Result();

            try
            {
                transaction.BeginTransaction();
                logger.Trace($"JobCreate() -- Start InsertReleaseInvoice()");
                RELEASEINVOICE releaseInvoice = InsertReleaseInvoice(info, status);
                logger.Trace($"JobCreate() -- Start JobInsertReleaseInvoiceDetail()");
                JobInsertReleaseInvoiceDetail(releaseInvoice.ID, releaseInvoice.COMPANYID.Value, info.ReleaseInvoiceInfos);
                logger.Trace($"JobCreate() -- JobInsertReleaseInvoiceDetail() done");
                resultRelease.Count = info.ReleaseInvoiceInfos.Count;
                resultRelease.ReleaseId = releaseInvoice.ID;
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return resultRelease;
        }

        public ResultCode UpdateStatusReleaseInvoiceDetail(long releaseId, StatusReleaseInvoice statusReleaseInvoice, UserSessionInfo currentUser)
        {
            RELEASEINVOICE releaseInvoice = GetReleaseInvoiceById(releaseId);
            if (releaseInvoice == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return UpdateStatusReleaseInvoiceDetail(releaseId, statusReleaseInvoice.InvoicesRelease, currentUser);
        }

        public ResultCode Update(long id, long companyId, StatusRelease statusRelease)
        {
            var currentReleaseInvoice = GetReleaseInvoiceById(id, companyId);
            if (currentReleaseInvoice == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            currentReleaseInvoice.STATUS = statusRelease.Status;
            currentReleaseInvoice.RELEASESTATUS = statusRelease.Message;
            currentReleaseInvoice.RELEASEBY = statusRelease.LoginId;
            this.releaseInvoiceRepository.Update(currentReleaseInvoice);
            return ResultCode.NoError;
        }

        public long CountReleaseInvoice(ConditionSearchReleaseInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.releaseInvoiceRepository.FilterReleaseInvoice(condition).Count();
        }

        public IEnumerable<SendEmailRelease> SendNotificeReleaseInvoice(IList<int> status)
        {
            var releaseInvoices = this.releaseInvoiceRepository.SendEmailNotifice(status).ToList();
            UpdateProcessSendEmail(releaseInvoices);
            List<SendEmailRelease> emailReleases = GetEmailRelease(releaseInvoices);
            return emailReleases;
        }
        public RELEASEINVOICE FilterInvoice(long id, IList<int> status)
        {
            return this.releaseInvoiceRepository.FilterInvoice(id, status);
        }

        public IEnumerable<ReleaseListInvoice> FilterReleaseInvoice(ConditionSearchReleaseList condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.releaseInvoiceRepository.FilterReleaseInvoice(condition).AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
        }

        public IEnumerable<ReleaseInvoices> FilterReleaseInvoice(ConditionSearchReleaseInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceMasters = this.releaseInvoiceRepository.FilterReleaseInvoice(condition).AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
            return invoiceMasters;
        }

        public long CountFilterReleaseInvoice(ConditionSearchReleaseList condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.releaseInvoiceRepository.FilterReleaseInvoice(condition).Count();
        }

        public IEnumerable<InvoicesReleaseDetail> ReleaseInvoiceDetail(ConditionSearchReleaseListDetail condition, int skip = int.MinValue, int take = int.MaxValue)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.releaseInvoiceRepository.ReleaseInvoiceDetail(condition).AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(skip).Take(take).ToList();
        }

        public long CountReleaseInvoiceDetail(ConditionSearchReleaseListDetail condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.releaseInvoiceRepository.ReleaseInvoiceDetail(condition).Count();
        }

        public string GeneratorInvoiceNoFromBD(long companyId, long templateId, string symbol, int? status)
        {

            //string numberFromUse = GetNumberForm(companyId, templateId, symbol);
            //double nextInvoiceNo = GetNextInvoiceNo(companyId, templateId, symbol);
            ////double maxInvoiceNoCompanyRegister = GetMaxNumberInvoiceCompanyRegister(companyId, templateId, symbol);
            ////if (nextInvoiceNo > maxInvoiceNoCompanyRegister)
            ////{
            ////    return DefaultFields.INVOICE_NO_DEFAULT_VALUE;
            ////}
            //if (status == (int)InvoiceStatus.Released)
            //{
            //    UserSessionCache.Instance.SaveMaxInvoiceNo(companyId, templateId, symbol, nextInvoiceNo);
            //}

            //return nextInvoiceNo.ToString();

            //double maxInvoiceNoCompanyRegister = GetMaxNumberInvoiceCompanyRegister(companyId, templateId, symbol);
            //if (nextInvoiceNo > maxInvoiceNoCompanyRegister)
            //{
            //    return "0000000";
            //}
            decimal? nextInvoiceNo = this.invoiceBO.GetNextInvoiceNo2(companyId, templateId, symbol);
            //if (status == (int)InvoiceStatus.Released)
            //{
                
            //}
            UserSessionCache.Instance.SaveMaxInvoiceNo(companyId, templateId, symbol, double.Parse(nextInvoiceNo.ToString()));
            return nextInvoiceNo.ToString();
        }
        public RELEASEINVOICE InsertReleaseInvoice(ReleaseInvoiceMaster info, ReleaseInvoiceStatus status)
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

            RELEASEINVOICE releaseInvoice = new RELEASEINVOICE();
            releaseInvoice.CopyData(info);
            releaseInvoice.STATUS = (int)status;
            releaseInvoice.RELEASEDDATE = info.ReleasedDate;
            releaseInvoice.RELEASEBY = info.UserActionId;
            this.releaseInvoiceRepository.Insert(releaseInvoice);
            return releaseInvoice;
        }
        #endregion

        #region Private Methods

        private List<string> GetSymbolsInReportCancellingInvoice(long companyId, long invoiceTemplateId)
        {
            IEnumerable<REPORTCANCELLINGDETAIL> reportCancelling = this.reportCancellingDetailRepository.Filter(companyId, invoiceTemplateId);
            return reportCancelling.Select(p => p.SYMBOL).ToList();

        }

        private void JobInsertReleaseInvoiceDetail(long releaseInvoiceId, long companyId, List<ReleaseInvoiceInfo> releaseInvoiceDetails)
        {
            releaseInvoiceDetails.ForEach(p =>
            {
                bool isValidate = true;
                INVOICE currentInvoice = GetInvoice(p.InvoiceId);
                if (currentInvoice.PARENTID != null && currentInvoice.INVOICESTATUS != (int)InvoiceStatus.Cancel)
                {
                    var invoiceParent = GetInvoice((long)currentInvoice.PARENTID);
                    if (currentInvoice.INVOICETYPE == (int)InvoiceType.Substitute)
                    {
                        if (invoiceParent.INVOICESTATUS != (int)InvoiceStatus.Cancel)
                        {
                            isValidate = false;//throw new BusinessLogicException(ResultCode.InvoiceNotInCanceledStatus, "Hóa đơn đang xử lý chưa được hủy. Bạn không thể phát hành hóa đơn này.");
                        }
                    }
                    else
                    {
                        if (invoiceParent.INVOICESTATUS == (int)InvoiceStatus.Cancel)
                        {
                            isValidate = false; //throw new BusinessLogicException(ResultCode.InvoiceTypeChanged, "Hóa đơn đang xử lý đã bị thay đổi trạng thái điều chỉnh. Bạn không thể phát hành hóa đơn này.");
                        }
                    }
                }
                //if (currentInvoice.PARENTID != null && currentInvoice.INVOICESTATUS != (int)InvoiceStatus.Cancel)
                //{
                //    var invoiceParent = GetInvoice((long)currentInvoice.PARENTID);
                //    if (invoiceParent.INVOICESTATUS == (int)InvoiceStatus.Cancel)
                //    {
                //        isValidate = false;
                //    }
                //}
                List<string> symbols = GetSymbolsInReportCancellingInvoice(companyId, (long)currentInvoice.REGISTERTEMPLATEID);

                isValidate = GetIsValidate(symbols, currentInvoice.SYMBOL, isValidate);
                if (isValidate)
                {
                    RELEASEINVOICEDETAIL releaseInvoiceDetail = new RELEASEINVOICEDETAIL();
                    releaseInvoiceDetail.INVOICEID = currentInvoice.ID;
                    releaseInvoiceDetail.RELEASEINVOICEID = releaseInvoiceId;
                    releaseInvoiceDetail.MESSAGES = "Hệ thống đang phát hành hóa đơn";
                    this.releseInvoiceDetailRepository.Insert(releaseInvoiceDetail);
                    UpdateInvoice(currentInvoice);
                }
            });
        }

        private bool GetIsValidate(List<string> symbols, string Symbol, bool isValidate)
        {
            if (symbols.Count > 0)
            {
                foreach (var item in symbols)
                {
                    if (Symbol == item)
                    {
                        isValidate = false;
                    }
                }
            }
            return isValidate;
        }

        private RELEASEINVOICE InsertReleaseInvoice(ReleaseGroupInvoice info)
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            RELEASEINVOICE releaseInvoice = new RELEASEINVOICE();
            releaseInvoice.CopyData(info);
            releaseInvoice.RELEASEDDATE = info.ReleasedDate;
            releaseInvoice.RELEASEBY = info.UserActionId;
            this.releaseInvoiceRepository.Insert(releaseInvoice);
            return releaseInvoice;
        }

        private void InsertReleaseInvoiceDetail(long releaseInvoiceId, IEnumerable<INVOICE> invoices)
        {
            invoices.ForEach(p =>
            {
                RELEASEINVOICEDETAIL releaseInvoiceDetail = new RELEASEINVOICEDETAIL();
                releaseInvoiceDetail.RELEASEINVOICEID = releaseInvoiceId;
                releaseInvoiceDetail.INVOICEID = p.ID;
                releaseInvoiceDetail.MESSAGES = "Đang phát hành hóa đơn";
                this.releseInvoiceDetailRepository.Insert(releaseInvoiceDetail);
                UpdateInvoice(p);
            });
        }

        private void InsertReleaseInvoiceDetail(long releaseInvoiceId, long companyId, List<ReleaseInvoiceInfo> releaseInvoiceDetails)
        {
            foreach (var p in releaseInvoiceDetails)
            {
                INVOICE currentInvoice = GetInvoice(p.InvoiceId);
                if (currentInvoice.INVOICESTATUS == (int)InvoiceStatus.Released || currentInvoice.INVOICESTATUS == (int)InvoiceStatus.Delete || currentInvoice.INVOICESTATUS == (int)InvoiceStatus.Cancel)
                {
                    continue;
                }
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
                RELEASEINVOICEDETAIL releaseInvoiceDetail = new RELEASEINVOICEDETAIL();
                releaseInvoiceDetail.INVOICEID = currentInvoice.ID;
                releaseInvoiceDetail.RELEASEINVOICEID = releaseInvoiceId;
                releaseInvoiceDetail.MESSAGES = "Hệ thống đang phát hành hóa đơn";
                this.releseInvoiceDetailRepository.Insert(releaseInvoiceDetail);
                UpdateInvoice(currentInvoice);
            }
        }

        private INVOICE GetInvoice(long id)
        {
            INVOICE invoice = this.invoiceRepository.GetById(id);
            if (invoice == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This invoice id [{0}] not found in data of client", id));
            }
            return invoice;
        }

        private void UpdateInvoice(INVOICE currentInvoice)
        {
            currentInvoice.RELEASED = true;
            if (currentInvoice.INVOICESTATUS != (int)InvoiceStatus.Cancel)
            {
                currentInvoice.INVOICESTATUS = (int)InvoiceStatus.Releaseding;
            }

            this.invoiceRepository.Update(currentInvoice);
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
        private string GetNumberForm(long companyId, long templateId, string symbol)
        {
            string numberFrom = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
            NOTIFICATIONUSEINVOICEDETAIL noticeInvoice = GetNotificationUseInvoiceDetail(companyId, templateId, symbol);
            if (noticeInvoice != null && noticeInvoice.NUMBERFROM.IsNullOrEmpty())
            {
                numberFrom = noticeInvoice.NUMBERFROM;
            }

            return numberFrom;
        }
        private NOTIFICATIONUSEINVOICEDETAIL GetNotificationUseInvoiceDetail(long companyId, long templateId, string symbol)
        {
            return this.noticeInvoiceDetailRepository.GetNotificationUseInvoiceDetails(companyId, templateId, symbol).OrderBy(p => p.NUMBERFROM).FirstOrDefault();
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
        private IEnumerable<INVOICE> FillterInvoice(ReleaseGroupInvoice info)
        {
            return this.invoiceRepository.FilterInvoice(info);
        }

        private List<SendEmailRelease> GetEmailRelease(IEnumerable<RELEASEINVOICE> releaseInvoices)
        {
            List<SendEmailRelease> emailReleases = new List<SendEmailRelease>();
            releaseInvoices.ForEach(p =>
            {
                p.RELEASEINVOICEDETAILs.Where(d => d.SENDEMAILSTATUS != (int)ReleaseInvoiceDetailStatus.SendEmailSuccess).ForEach(i =>
                {
                    SendEmailRelease sendEmailInfo = this.invoiceRepository.GetSendInvoiceInfo((i.INVOICEID ?? 0));
                    sendEmailInfo.ReleaseDetailId = i.ID;
                    sendEmailInfo.ReleaseId = i.RELEASEINVOICEID;
                    sendEmailInfo.CompanyId = p.COMPANYID;
                    emailReleases.Add(sendEmailInfo);
                });
            });

            return emailReleases;
        }

        private void UpdateProcessSendEmail(IEnumerable<RELEASEINVOICE> releaseInvoices)
        {
            releaseInvoices.ForEach(p =>
            {
                p.STATUS = (int)ReleaseInvoiceStatus.SenddingEmail;
                this.releaseInvoiceRepository.Update(p);
            });
        }

        public void UpdateProcessSendEmail(IList<ResultRelease> results)
        {
            if (results.Count == 0)
            {
                return;
            }

            UpdataStatusReleaseDetail(results);
            UpdataStatusRelease(results);
        }

        private void UpdataStatusRelease(IList<ResultRelease> results)
        {
            var releseInvoices = results.Select(p => p.ReleaseId).AsEnumerable().Distinct();
            releseInvoices.ForEach(p =>
            {
                var currentRelease = GetReleaseInvoiceById(p);
                int numerRecordError = currentRelease.RELEASEINVOICEDETAILs.Count(i => i.SENDEMAILSTATUS != (int)ReleaseInvoiceDetailStatus.SendEmailSuccess);
                currentRelease.STATUS = numerRecordError == 0 ? (int)ReleaseInvoiceStatus.SendEmailSuccess : (int)ReleaseInvoiceStatus.SendEmailError;
                this.releaseInvoiceRepository.Update(currentRelease);
            });
        }

        private void UpdataStatusReleaseDetail(IList<ResultRelease> results)
        {
            results.ForEach(p =>
            {
                RELEASEINVOICEDETAIL current = GetById(p.ReleaseDeatailId);
                current.SENDEMAILSTATUS = p.IsSuccessful ? (int)ReleaseInvoiceDetailStatus.SendEmailSuccess : (int)ReleaseInvoiceDetailStatus.SendEmailError;
                current.MESSAGES = p.ErrorMessage;
                this.releseInvoiceDetailRepository.Update(current);
            });
        }

        private RELEASEINVOICEDETAIL GetById(long id)
        {
            return this.releseInvoiceDetailRepository.GetById(id);
        }

        private RELEASEINVOICE GetReleaseInvoiceById(long id, long companyId)
        {
            return this.releaseInvoiceRepository.GetById(id, companyId);
        }

        public RELEASEINVOICE GetReleaseInvoiceById(long id)
        {
            return this.releaseInvoiceRepository.GetById(id);
        }

        public RELEASEINVOICE GetReleaseInvoiceByInvoiceId(long id)
        {
            return this.releaseInvoiceRepository.GetByInvoiceId(id);
        }

        public long? GetCompanyIdByVerifiCode(string VerifiCod)
        {
            return this.releaseInvoiceRepository.GetCompanyIdByVerifiCode(VerifiCod);
        }
        public IEnumerable<InvoicesReleaseDetail> GetInvoiceByReleaseId(long id)
        {
            return this.releaseInvoiceRepository.GetInvoiceByReleasedId(id);
        }
        #endregion

        #region Update status Sign invoice
        private RELEASEINVOICEDETAIL GetReleaseInvoiceDetail(long releaseId, long invoiceId)
        {
            RELEASEINVOICEDETAIL releaseInvoiceDetail = this.releseInvoiceDetailRepository.FilteReleaseInvoiceDetail(releaseId, invoiceId);
            if (releaseInvoiceDetail == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return releaseInvoiceDetail;
        }

        private ResultCode UpdateStatusReleaseInvoiceDetail(long releaseId, List<InvoicesRelease> invoicesRelease, UserSessionInfo currentUser)
        {
            invoicesRelease.ForEach(p =>
            {
                RELEASEINVOICEDETAIL currentReleaseInvoiceDetail = GetReleaseInvoiceDetail(releaseId, p.InvoiceId);
                currentReleaseInvoiceDetail.SIGNED = p.Signed;
                currentReleaseInvoiceDetail.PRINTED = p.Printed;
                currentReleaseInvoiceDetail.VERIFICATIONCODE = p.VerificationCode;
                this.releseInvoiceDetailRepository.Update(currentReleaseInvoiceDetail);
                if (p.Signed)
                {
                    UpdateStatusInvoice(p.InvoiceId, p.InvoiceNo, (int)InvoiceStatus.Released, currentUser);
                }
            });

            return ResultCode.NoError;
        }

        private void UpdateStatusInvoice(long invoiceId, string invoiceNo, int invoiceStatus, UserSessionInfo currentUser)
        {
            var currentInvoice = GetInvoice(invoiceId);
            currentInvoice.UPDATEDDATE = DateTime.Now;
            if (currentInvoice.INVOICESTATUS.HasValue && currentInvoice.INVOICESTATUS.Value != (int)InvoiceStatus.Cancel)
            {
                currentInvoice.INVOICESTATUS = invoiceStatus;
                this.CreateInvoiceNoWhenSign(currentInvoice, invoiceId);
                //if (currentUser.SystemSetting.StepToCreateInvoiceNo != StepToCreateInvoiceNo.WhenApprove)
                //{
                //    //sinh số hóa đơn khi đã ký duyệt
                    
                //}
                //else
                //{
                //    this.CreateInvoiceNoWhenApprove(currentInvoice, invoiceNo);
                //}
            }
            //hóa đơn thay thế đã ký duyệt thì update status cho hóa đơn bị thay thế
            if (currentInvoice.INVOICETYPE == (int)InvoiceType.Substitute)
            {
                var invoiceSubstitute = GetInvoice((int)currentInvoice.PARENTID);
                invoiceSubstitute.INVOICESTATUS = (int)InvoiceStatus.Cancel;
                invoiceSubstitute.UPDATEDDATE = DateTime.Now;
                this.invoiceRepository.Update(invoiceSubstitute);
            }

            //update status cho biên bản đã sử dụng của hóa đơn gốc
            int announType = 0;
            if (currentInvoice.INVOICETYPE == (int)InvoiceType.Substitute)
            {
                announType = 2;
            }
            else if (currentInvoice.INVOICETYPE == (int)InvoiceType.AdjustmentUpDown || currentInvoice.INVOICETYPE == (int)InvoiceType.AdjustmentTax || currentInvoice.INVOICETYPE == (int)InvoiceType.AdjustmentTaxCode)
            {
                announType = 1;
            }
            UpdateAnnouncementStatus(currentInvoice.PARENTID ?? 0, announType, currentInvoice.COMPANYID, currentUser.Id);
        }

        private void CreateInvoiceNoWhenSign(INVOICE currentInvoice, long invoiceId)
        {
            //sinh số hóa đơn khi đã ký duyệt
            // BIDC thì đã có ngày hóa đơn sẵn
            if (!currentInvoice.RELEASEDDATE.HasValue)
            {
                currentInvoice.RELEASEDDATE = DateTime.Now;
            }
            if (!(currentInvoice.INVOICENO > 0))
            {
                currentInvoice.NO = GeneratorInvoiceNoFromBD(currentInvoice.COMPANYID, currentInvoice.REGISTERTEMPLATEID.Value, currentInvoice.SYMBOL.Replace("/", ""), currentInvoice.INVOICESTATUS);
            }
            currentInvoice.INVOICENO = currentInvoice.NO.ToDecimal(0);
            if (currentInvoice.NO == DefaultFields.INVOICE_NO_DEFAULT_VALUE)
            {
                this.listInvoiceExceed.Add(invoiceId);
                currentInvoice.INVOICESTATUS = (int)InvoiceStatus.Releaseding;
            }
            else
            {
                var inv = this.invoiceRepository.GetByIdOfSign(invoiceId);
                CLIENT client = this.clientRepository.GetById(inv.CLIENTID ?? 0);
                if (client != null && currentInvoice.INVOICETYPE.IsNullOrEmpty())
                {
                    foreach (var item in currentInvoice.INVOICEDETAILs)
                    {
                        INVOICEDETAIL invoiceDetail = this.invoiceDetailRepository.GetById(item.ID);
                        this.invoiceDetailRepository.Update(invoiceDetail);
                    }
                }
                if (inv.NO != currentInvoice.NO)
                {
                    this.invoiceRepository.Update(currentInvoice);
                }
                else if (!(currentInvoice.INVOICENO > 0))
                {
                    currentInvoice.NO = GeneratorInvoiceNoFromBD(currentInvoice.COMPANYID, currentInvoice.REGISTERTEMPLATEID.Value, currentInvoice.SYMBOL.Replace("/", ""), currentInvoice.INVOICESTATUS);
                    currentInvoice.INVOICENO = currentInvoice.NO.ToDecimal(0);
                    this.invoiceRepository.Update(currentInvoice);
                }

            }
        }
        private void CreateInvoiceNoWhenApprove(INVOICE currentInvoice, string invoiceNo)
        {
            if (invoiceNo != DefaultFields.INVOICE_NO_DEFAULT_VALUE)
            {
                this.invoiceRepository.Update(currentInvoice);
            }
            else
            {
                currentInvoice.NO = GeneratorInvoiceNoFromBD(currentInvoice.COMPANYID, currentInvoice.REGISTERTEMPLATEID.Value, currentInvoice.SYMBOL.Replace("/", ""), currentInvoice.INVOICESTATUS);
                currentInvoice.INVOICENO = currentInvoice.NO.ToDecimal(0);
                this.invoiceRepository.Update(currentInvoice);
            }
        }

        public INVOICE GetInvoiceCheckSign(long invoiceId)
        {
            return this.invoiceRepository.GetById(invoiceId);
        }
        public ANNOUNCEMENT UpdateAnnouncementStatus(long id, int announType, long? companyId, long processBy)
        {
            ANNOUNCEMENT currentAnnouncement = GetAnnouncementByInvoiceId(id, announType, companyId);

            if (announType != 3)
            {
                currentAnnouncement = GetAnnouncementByInvoiceId(id, announType, companyId);
            }

            if (currentAnnouncement == null)
            {
                return null;
            }
            currentAnnouncement.ANNOUNCEMENTSTATUS = (int)AnnouncementStatus.Successfull;
            currentAnnouncement.PROCESSDATE = DateTime.Now;
            currentAnnouncement.PROCESSBY = processBy;
            this.announcementRepository.Update(currentAnnouncement);
            return currentAnnouncement;
        }

        // get Announcement cho hóa đơn bị thay thế hoặc hủy
        private ANNOUNCEMENT GetAnnouncementByInvoiceId(long id, int announType, long? companyId)
        {
            ANNOUNCEMENT announcements = this.announcementRepository.GetByInvoiceId(id, announType, companyId);
            return announcements;
        }
        public bool CheckSignedInvoice(INVOICE invoice, List<long> invoiceIDs)
        {
            var listNotSignYday = this.invoiceRepository.GetInvoiceSignYDay(invoice.COMPANYID, (long)invoice.REGISTERTEMPLATEID, invoice.SYMBOL, invoice.ID, invoiceIDs).ToList();
            if (listNotSignYday.Count > 0)
                return false;
            return true;
        }

        public void JobSignFile(ReleaseInvoiceMaster releaseInvoice, UserSessionInfo currentUser)
        {
            if (releaseInvoice.ReleaseInvoiceInfos.Count <= 0)
            {
                return;
            }
            // check signature info
            SIGNATURE signature = this.signatureRepository.GetByCompany((long)releaseInvoice.CompanyId);
            if (signature == null)
            {
                return;
            }
            JobLog.TotalInvoice += releaseInvoice.ReleaseInvoiceInfos.Count;
            //logger.Trace($"JobSignFile() -- start JobCreate(), releaseInvoice.ReleaseInvoiceInfos.Count{releaseInvoice.ReleaseInvoiceInfos.Count}");
            Result release = JobCreate(releaseInvoice);
            //logger.Trace($"JobSignFile() -- JobCreate() done, release.Count,{release.Count}");
            //get invoice realease
            List<int> releaseStatus = new List<int>() { (int)ReleaseInvoiceStatus.New, (int)ReleaseInvoiceStatus.SignError };
            IEnumerable<InvoicesRelease> invoicesRelease = this.releseInvoiceDetailRepository.FilteInvoiceRelease(release.ReleaseId, releaseInvoice.CompanyId ?? 0, releaseStatus).ToList();
            if (ValidDateSign(invoicesRelease))
            {
                this.systemSettingRepository.GetByCompany((long)releaseInvoice.CompanyId);
                foreach (var item in invoicesRelease)
                {
                    try
                    {
                        logger.Trace($"JobSignFileItem() InvoiceId:{item.InvoiceId}");
                        this.JobSignFileItem(item, release, signature);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        logger.Trace($"Class:ReleaseInvoiceBO Method:JobSignFile InvoiceId:{item.InvoiceId}", item);
                        logger.Error(currentUser.UserId, ex);
                        JobLog.AddFail(item.InvoiceId, item.InvoiceNo, string.Empty, ex.Message);
                    }
                }
            }
            else
            {
                logger.Error("SignInvoice", new Exception("Please release the invoice for the previous day"));
            }
        }
        private void JobSignFileItem(InvoicesRelease item, Result release, SIGNATURE signature)
        {
            transaction.BeginTransaction();
            string result = "";
            string companyName = "";
            List<InvoicesRelease> lst = new List<InvoicesRelease>();
            lst.Add(item);
            UpdateStatustAfterSign(lst, release, currentUser);
            InvoicePrintInfo invoicePrint = this.invoiceRepository.GetInvoicePrintInfo(item.InvoiceId);
            if (invoicePrint.Invoice_Status == (int)InvoiceStatus.Released)
            {
                this.invoiceBO.SetinvoiceInfo(invoicePrint);
                //get filepath pdf 
                string pathFilePdf = Path.Combine(this.config.FullPathInvoiceFileOfCompany, "Invoice", item.InvoiceId.ToString() + ".pdf");
                if (!File.Exists(pathFilePdf))//check file đã được tạo hay chưa
                {
                    pathFilePdf = getPathFilePdfSign(item.InvoiceId);
                }
                else if (!CheckInvoiceNoInPdf(pathFilePdf, item.InvoiceNo))// check file đã tạo có InvoiceNo hay chưa
                {
                    pathFilePdf = getPathFilePdfSign(item.InvoiceId);
                }
                //get filepath xml
                string pathFileXml = Path.Combine(this.config.FullPathInvoiceFileOfCompany, "Invoice", item.InvoiceId.ToString() + ".xml");
                if (!File.Exists(pathFileXml))
                {
                    pathFileXml = getPathFileXmlSign(item.InvoiceId);
                }
                string folderTree = PathUtil.FolderTree(invoicePrint.DateRelease);
                string signedPdfPath = pathFilePdf.Replace("\\Invoice\\", "\\Releases\\Sign\\" + folderTree + "\\");
                string onlyPath = Directory.GetParent(signedPdfPath).FullName;
                if (!Directory.Exists(onlyPath))
                {
                    Directory.CreateDirectory(onlyPath);
                }
                //call api sign
                result = callApiServerSign(pathFileXml, false, invoicePrint.CompanyName, item.InvoiceId, invoicePrint.DateRelease ?? DateTime.Now, signature, folderTree);
                //Xóa file hóa đơn chưa ký trước đó
                DeleteFileAfterSign(pathFilePdf);
                DeleteFileAfterSign(pathFileXml);
                companyName = invoicePrint.CompanyName;
            }
            if (result == "True")
            {
                transaction.Commit();
                // Add SignDetail
                this.AddSignDetail(item.InvoiceId, signature.SERIALNUMBER, companyName);
                JobSendMailAfterSign(lst, this.currentUser.EmailServer);
            }
            else
            {
                transaction.Rollback();
            }
        }

        /// <summary>
        /// Tạo chữ ký lên các hóa đơn
        /// </summary>
        /// <param name="invoices"></param>
        /// <param name="signature"></param>
        /// <param name="companyName"></param>
        public void JobSignShinhan(List<INVOICE> invoices, SIGNATURE signature, string companyName)
        {
            try
            {
                if (invoices.Count <= 0) return; 

                // Tạo format chữ ký
                SignListModel signListModel = JobSignFileItemShinhanMultipe(invoices, signature, companyName);

                // tạo chữ ký lên files
                string result = callApiServerSignShinhan(signListModel);
                if (result == "True")
                {
                    // Lấy danh sách clients
                    var objClients = this.clientRepository.GetByIds(invoices.Select(f=>f.CLIENTID).Distinct().ToList());

                    // Zip files ivnocies
                    ZipFileAfterSign(invoices, objClients);

                    // cập nhật trạng thái realease cho danh sách invoice
                    UpdateStatus(invoices);

                    // copys files vào danh sách gửi mail
                    if (CopyFileToSFTP(invoices, objClients))
                    {
                        SendMailAfterSign(invoices, objClients);
                    }
                    AddSignDetail(invoices, signListModel.SerialNumber, signListModel.CompanyName);
                    //DeleteFileOldAfterSign(invoices);
                }
                

            }
            catch (Exception ex)
            {
                logger.Error("SignJob", ex);
            }

        }
        private void UpdateStatus(List<INVOICE> invoices)
        {
            foreach (var item in invoices)
            {
                item.INVOICESTATUS = (int)InvoiceStatus.Released;
                
            }
            lock (ReleaseInvoiceLock.LockObject)
            {
                
                this.invoiceRepository.Update(invoices[0]);
            }
        }

        private void DeleteFileOldAfterSign(List<INVOICE> invoices)
        {
            Parallel.ForEach(invoices, item =>
            {
                string filePathPdf = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Invoice", item.ID.ToString() + ".pdf");
                DeleteFileAfterSign(filePathPdf);
                DeleteFileAfterSign(filePathPdf.Replace(".pdf", ".xml"));
            });
        }
        private bool CopyFileToSFTP(List<INVOICE> invoices, List<CLIENT> objClients)
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
                        var currentClient = objClients.FirstOrDefault(f => f.ID == item.CLIENTID);
                        //this.clientRepository.GetById((long)item.CLIENTID);
                        if (currentClient.ISORG == true)
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
            }
            catch (Exception ex)
            {
                logger.Error("CopyFileToSFTP", ex);
            }
            return true;
        }
        /// <summary>
        /// Zip các file invoice
        /// </summary>
        /// <param name="invoices"></param>
        /// <param name="objClients">thông tin của clients</param>
        private void ZipFileAfterSign(List<INVOICE> invoices,List<CLIENT> objClients)
        {         
            
            foreach (var item in invoices)
            {
                var client = objClients.FirstOrDefault(f => f.ID == item.CLIENTID);
                CreateZipFile(item, client);
            }
        }
        /// <summary>
        /// Tạo zip files sau
        /// </summary>
        /// <param name="invoice">Thông tin hóa đơn</param>
        /// <param name="client">Thông tin client</param>
        private void CreateZipFile(INVOICE invoice, CLIENT client)
        {
            try
            {
              
                if (client.ISORG == true)
                {
                    string folderName = "ShinhanBank_Inv_" + invoice.NO + "_" + invoice.COMPANYID + "_" + invoice.RELEASEDDATE.Value.ToString("ddMMyyy");
                    string folderPath = Path.Combine(this.config.PathInvoiceFile, invoice.COMPANYID.ToString(), "Releases", "Sign", invoice.RELEASEDDATE.Value.Year.ToString(), invoice.RELEASEDDATE.Value.ToString("MM"), invoice.RELEASEDDATE.Value.ToString("dd"), folderName);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    List<string> listFiles = new List<string>();
                    string pathFileInvoice = Path.Combine(this.config.PathInvoiceFile, invoice.COMPANYID.ToString(), "Releases", "Sign", invoice.RELEASEDDATE.Value.Year.ToString(), invoice.RELEASEDDATE.Value.ToString("MM"), invoice.RELEASEDDATE.Value.ToString("dd"));
                    string signedPdfPath = Path.Combine(pathFileInvoice, invoice.ID + "_sign.pdf");
                    listFiles.Add(signedPdfPath);
                    string signedXmlPath = Path.Combine(pathFileInvoice, invoice.ID + "_sign.xml");
                    listFiles.Add(signedXmlPath);
                    string pathFileTo = String.Empty;
                    foreach (var file in listFiles)
                    {
                        pathFileTo = Path.Combine(folderPath, Path.GetFileName(file));
                        File.Copy(file, pathFileTo);
                    }
                    zipFile(invoice.CUSTOMERTAXCODE, folderPath, folderPath + ".zip");
                    if (Directory.Exists(folderPath))
                    {
                        Directory.Delete(folderPath, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("zipFile", ex);
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
        private void SendMailAfterSignShinhan(long invoiceId, CLIENT client)
        {
            InvoicePrintInfo invoicePrint;
            InvoiceVerificationCode invoiceVerificationCode = new InvoiceVerificationCode();
            RELEASEINVOICEDETAIL releaseInvoiceDetail;
            invoicePrint = this.invoiceRepository.GetInvoicePrintInfo(invoiceId);
           //CLIENT client = this.clientRepository.GetById(invoicePrint.ClientId);
            invoiceVerificationCode.Mobile = invoicePrint.MobileClient;
            invoiceVerificationCode.Email = client.USEREGISTEREMAIL != true ? client.RECEIVEDINVOICEEMAIL : invoicePrint.EmailClient;
            invoiceVerificationCode.CompanyId = invoicePrint.CompanyId;
            if (client.ISORG == true)
            {
                releaseInvoiceDetail = this.releseInvoiceDetailRepository.FilteReleaseInvoiceDetail(invoiceId);
                invoiceVerificationCode.VerificationCode = releaseInvoiceDetail.VERIFICATIONCODE;
                if (invoicePrint.ClientSendByMonth != true && invoiceVerificationCode.Email != null)
                {
                    SendVerificationCodeJobShinhan(invoiceVerificationCode);
                }
            }
        }
        private SignListModel JobSignFileItemShinhanMultipe(List<INVOICE> invoices, SIGNATURE signature, string companyName)
        {
            SignListModel invoiceSign = new SignListModel();
            invoiceSign.Password = signature.PASSWORD;
            invoiceSign.SerialNumber = signature.SERIALNUMBER;
            invoiceSign.Slot = signature.SLOTS;
            invoiceSign.SignHSM = bool.Parse(WebConfigurationManager.AppSettings["SignHSM"] ?? "false");
            invoiceSign.CompanyName = companyName;
            List<InvoiceFor> invoiceFors = new List<InvoiceFor>();
            foreach (var item in invoices)
            {
                try
                {
                    InvoiceFor invoiceFor = new InvoiceFor();
                    invoiceFor.InvoiceId = item.ID;
                    //get filepath pdf 
                    //string filePathPdf = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Invoice", item.ID.ToString() + ".pdf");
                    string filePathPdf = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Invoice", item.ID.ToString() + ".pdf");
                    //logger.Trace("filePathPdf: " + filePathPdf);
                    try
                    {
                        FileStream aa = File.OpenRead(filePathPdf);

                    } catch(Exception ee)
                    {
                        //logger.Trace("filePathPdf: check file đã được tạo hay chưa");
                        filePathPdf = getPathFilePdfSign(item.ID);
                    }
                    
          
                    /*if (!File.Exists(filePathPdf))//check file đã được tạo hay chưa
                    {
                        logger.Trace("filePathPdf: check file đã được tạo hay chưa");
                        filePathPdf = getPathFilePdfSign(item.ID);
                    }*/
                    if (!CheckInvoiceNoInPdf(filePathPdf, item.NO))// check file đã tạo có InvoiceNo hay chưa
                    {
                        //logger.Trace("filePathPdf: check file đã tạo có InvoiceNo hay chưa");
                        filePathPdf = getPathFilePdfSign(item.ID);
                    }
                    invoiceFor.FilePathPdf = filePathPdf;
                    //get filepath xml
                    string pathFileXml = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Invoice", item.ID.ToString() + ".xml");
                    try
                    {
                        FileStream readXML = File.OpenRead(pathFileXml);

                    }catch(Exception ex)
                    {
                        pathFileXml = getPathFileXmlSign(item.ID);
                    }

                    //if (!File.Exists(pathFileXml))//check file đã được tạo hay chưa
                    //{
                        
                    //}
                    invoiceFor.PathFileXml = pathFileXml;
                    invoiceFor.DateFolder = PathUtil.FolderTree(item.RELEASEDDATE);
                    invoiceFor.DateSign = item.RELEASEDDATE.Value.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
                    string signedPdfPath = filePathPdf.Replace("\\Invoice\\", "\\Releases\\Sign\\" + invoiceFor.DateFolder + "\\");
                    string onlyPath = Directory.GetParent(signedPdfPath).FullName;
                    if (!Directory.Exists(onlyPath))
                    {
                        Directory.CreateDirectory(onlyPath);
                    }
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
        private string callApiServerSignShinhan(SignListModel invoiceSigns)
        {
            string response = String.Empty;
            var json = JsonConvert.SerializeObject(invoiceSigns);
            string DATA = json.ToString();
            string UrlServerSign = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSign"], ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionSignInvoiceMultipe);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlServerSign);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(DATA);
            request.ContentLength = byteArray.Length;
            request.Timeout = 3600000;
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
        private void DeleteInvoiceSino(long invoiceId)
        {
            var invoiceDt = this.invoiceDetailRepository.FilterInvoiceDetail(invoiceId).FirstOrDefault(p => p.ADJUSTMENTTYPE == -99);
            if (invoiceDt != null)
            {
                invoiceDt.ADJUSTMENTTYPE = null;
                this.invoiceDetailRepository.Update(invoiceDt);

                CancellingInvoiceMaster deleteInfo = new CancellingInvoiceMaster();
                deleteInfo.InvoiceId = invoiceId;
                deleteInfo.CancellingDate = DateTime.Now;
                this.invoiceDeleteBO.InvoiceDelete(deleteInfo);
            }
        }
        private bool CheckInvoiceNoInPdf(string pathFile, string invoiceNo)
        {
            int index = 0;
            using (PdfDocument pdf = new PdfDocument(pathFile))
            {
                for (int i = 0; i < pdf.Pages.Count; i++)
                {
                    string pageText = pdf.Pages[i].GetText();
                    index = pageText.IndexOf(invoiceNo, 0, StringComparison.CurrentCultureIgnoreCase);
                    if (index != -1) return true;
                }
            }
            return false;
            //if (index != -1)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }
        private bool ValidDateSign(IEnumerable<InvoicesRelease> invoicesReleases)
        {
            InvoicesRelease invoicesReleaseMinDate = invoicesReleases.OrderBy(p => p.ReleasedDate).First();
            logger.Error("invoicesReleaseDate", new Exception(invoicesReleaseMinDate.ReleasedDate.ToString("yyyy/MM/dd")));
            var InvoiceNotRelease = this.invoiceRepository.GetInvoiceNotRelease(invoicesReleaseMinDate.ReleasedDate);
            if (InvoiceNotRelease.Count() > 0)
            {
                foreach (var inv in InvoiceNotRelease)
                {
                    JobLog.AddFail(inv.ID, inv.NO, inv.RELEASEDDATE.Value.ToString("yyyy/MM/dd"), "Hóa đơn của ngày trước đó chưa được phát hành");
                }
                return false;
            }
            return true;
        }
        public ServerSignResult SignFile(ReleaseInvoiceMaster releaseInvoice, UserSessionInfo currentUser)
        {
            string result = "Can't connect to server";
            ServerSignResult serverSignResult = new ServerSignResult();
            logger.Error("releaseInvoice", new Exception(releaseInvoice.ReleasedDate.ToString("yyyy/MM/dd")));
            //create invoice realease
            Result release = Create(releaseInvoice);
            //get invoice realease
            List<int> releaseStatus = new List<int>() { (int)ReleaseInvoiceStatus.New, (int)ReleaseInvoiceStatus.SignError };
            IEnumerable<InvoicesRelease> invoicesRelease = this.releseInvoiceDetailRepository.FilteInvoiceRelease(release.ReleaseId, releaseInvoice.CompanyId ?? 0, releaseStatus);
            if(invoicesRelease.Count() > 0)
            {

                if (!ValidDateSign(invoicesRelease))
                {
                    serverSignResult.Message = "ReleaseDateInvalid";
                    return serverSignResult;
                }
                try
                {
                    transaction.BeginTransaction();
                    if (releaseInvoice.ReleaseInvoiceInfos.Count > 0)
                    {
                        if (releaseInvoice.ReleaseInvoiceInfos.Count == 1)
                        {
                            this.ValidateSignFileInvoiceRelease(releaseInvoice, serverSignResult);
                        }
                        //update status invoice
                        UpdateStatustAfterSign(invoicesRelease, release, currentUser);

                        result = this.SignFileCallApiServerSign(releaseInvoice, out string companyNameSign);

                        if (result == "True")
                        {
                            transaction.Commit();
                            foreach (var item in releaseInvoice.ReleaseInvoiceInfos)
                            {
                                // Add SignDetail
                                this.AddSignDetail(item.InvoiceId, item.SerialNumber, companyNameSign);
                            }
                            //comment tam thoi de Nam co the chay test cho shinhan - 20190723
                            //SendMailAfterSign(releaseInvoice, this.currentUser.EmailServer);
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            serverSignResult.InvoiceSuccess = invoicesRelease.Count() - this.listInvoiceExceed.Count;
            serverSignResult.InvoiceFail = this.listInvoiceExceed.Count;
            serverSignResult.Message = result;
            return serverSignResult;
        }

        public bool SignFileItem(List<ReleaseInvoiceInfo> invoiceInfos)
        {
            foreach(var item in invoiceInfos)
            {
                try
                {
                    //get invoice invoice infor
                    var invoiceInfo = this.invoiceRepository.GetInvoicePrint(item.InvoiceId);
                    var invoicePrint = this.invoiceBO.ProcessInvoicePrint(invoiceInfo);
                    string fullPathFile = Path.Combine(this.config.PathInvoiceFile, invoiceInfo.COMPANYID.ToString(), "Releases", "Sign", invoiceInfo.RELEASEDDATE.Value.ToString("yyyy"), invoiceInfo.RELEASEDDATE.Value.ToString("MM"), invoiceInfo.RELEASEDDATE.Value.ToString("dd"));
                    string fileName = invoiceInfo.ID.ToString() + "_sign.pdf";

                    var date = this.declarationRepository.ExpireTokenDate((long?)invoiceInfo.COMPANYID ?? 0);
                    var countDate = (date - DateTime.Now).TotalDays;
                    if (countDate < 0)
                    {
                        throw new BusinessLogicException(ResultCode.ExpireDateToken, "Token đã hết hạn");
                    }

                    //ký pdf
                    this.invoiceBO.ExportInvoiceToPdfCreateFile(invoicePrint.invoiceExportPdfModels.FirstOrDefault(), fullPathFile, fileName);
                    //ký xml
                    this.invoiceBO.ProcessXmls(invoicePrint.invoiceExportXmlModels);
                    var xmlModel = ProcessSignXml(invoiceInfo);
                    string result = callApiServerSign(xmlModel);
                    //update status
                    if (result == "True")
                    {
                        UpdateStatusInvoice((long)invoiceInfo.ID, invoiceInfo.INVOICENO.ToString(), (int)InvoiceStatus.Released, currentUser);
                    }
                    
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                
            }
            return true;
        }
        private string callApiServerSign(SignListModel invoiceSigns)
        {
            string response = String.Empty;
            var json = JsonConvert.SerializeObject(invoiceSigns);
            string DATA = json.ToString();
            string UrlServerSign = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSign"], ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionSignInvoiceMultipe);
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
        private SignListModel ProcessSignXml(InvoicePrintModel invoicePrintModel)
        {
            SignListModel invoiceSign = new SignListModel();
            //var invoiceModel = invoicePrintModel.FirstOrDefault();
            invoiceSign.Password = invoicePrintModel.PASSWORD;
            invoiceSign.SerialNumber = invoicePrintModel.SERIALNUMBER;
            invoiceSign.Slot = invoicePrintModel.SLOTS.ToInt();
            invoiceSign.SignHSM = true;
            invoiceSign.DateSignXml = invoicePrintModel.RELEASEDDATE.Value.ToString("dd/MM/yyyy");
            string pathFileXmlSign = string.Empty;
            List<InvoiceFor> invoiceFors = new List<InvoiceFor>();
            try
            {
                InvoiceFor invoiceFor = new InvoiceFor();
                invoiceFor.InvoiceId = Convert.ToInt64(invoicePrintModel.ID);
                //get filepath xml
                string pathFileXml = Path.Combine(this.config.PathInvoiceFile, invoicePrintModel.COMPANYID.ToString(), "Invoice", invoicePrintModel.ID.ToString() + ".xml");
                invoiceFor.PathFileXml = pathFileXml;
                invoiceFor.DateFolder = PathUtil.FolderTree(invoicePrintModel.RELEASEDDATE);
                invoiceFors.Add(invoiceFor);
            }
            catch (Exception ex)
            {
                logger.Error("SignJobInvoice", ex);
            }
            invoiceSign.invoiceFors = invoiceFors;

            return invoiceSign;
        }
        public ServerSignResult SignFileManual(ReleaseInvoiceMaster releaseInvoice, UserSessionInfo currentUser)
        {
            bool result = false;
            ServerSignResult serverSignResult = new ServerSignResult();
            logger.Error("releaseInvoice", new Exception(releaseInvoice.ReleasedDate.ToString("yyyy/MM/dd")));
            try
            {
                transaction.BeginTransaction();
                if (releaseInvoice.ReleaseInvoiceInfos.Count > 0)
                {
                    //if (releaseInvoice.ReleaseInvoiceInfos.Count == 1)
                    //{
                    //    this.ValidateSignFileInvoiceRelease(releaseInvoice, serverSignResult);
                    //}

                    result = SignFileItem(releaseInvoice.ReleaseInvoiceInfos);

                    if (result)
                    {
                        transaction.Commit();

                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                else
                {
                    transaction.Rollback();
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            serverSignResult.InvoiceSuccess = releaseInvoice.ReleaseInvoiceInfos.Count() - this.listInvoiceExceed.Count;
            serverSignResult.InvoiceFail = this.listInvoiceExceed.Count;
            if (result == true)
            {
                serverSignResult.Message = "True";
            }
            else
            {
                serverSignResult.Message = "False";
            }
            
            return serverSignResult;
        }
        public InvoiceMaster IscheckResponeCQT(long Id)
        {

            var checkStatus = this.releaseInvoiceRepository.getResponeCQT(Id).FirstOrDefault();

            return checkStatus;
        }

        public ServerSignResult IscheckCodeCQT(long Id)
        {
            var result = new ServerSignResult();

            var checkStatus = this.releaseInvoiceRepository.getResponeCQT(Id).FirstOrDefault();
            if (checkStatus != null)
            {
                if (checkStatus.codeCQT == null)
                {
                    result.Message = NotCodeCQT.ErrorrTwan;
                }

            }
            else
            {
                result.Message = NotCodeCQT.ErrorrTwan;
            }

            return result;

        }

        public ServerSignResult IscheckRepsoneTwanWithNotCode(long id)
        {
            var result = new ServerSignResult();

            var checkStatus = this.releaseInvoiceRepository.getResponeCQT(id).FirstOrDefault();
            if (checkStatus != null)
            {
                var message = checkStatus.MessageError;
                if (checkStatus.MessageError != null)
                {
                    message = checkStatus.MessageError.Replace('/', ',');
                }
                result.MessageError = message;

            }
            return result;

        }
        private void ValidateSignFileInvoiceRelease(ReleaseInvoiceMaster releaseInvoice, ServerSignResult serverSignResult)
        {
            INVOICE invoices = this.invoiceRepository.GetById(releaseInvoice.ReleaseInvoiceInfos[0].InvoiceId);
            if (invoices == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This InvoiceId [{0}] not found in data of client", releaseInvoice.ReleaseInvoiceInfos[0].InvoiceId));
            }

            //check err total number invoice notice with number invoice of contract
            //if (currentUser.SystemSetting.StepToCreateInvoiceNo != StepToCreateInvoiceNo.WhenApprove)
            //{
            //    //var symbol = invoices.SYMBOL.Replace("/", "");
            //    //double nextInvoiceNo = GetNextInvoiceNo(invoices.COMPANYID, invoices.REGISTERTEMPLATEID ?? 0, symbol);
            //    //double maxInvoiceNoCompanyRegister = GetMaxNumberInvoiceCompanyRegister(invoices.COMPANYID, invoices.REGISTERTEMPLATEID ?? 0, symbol);
            //    //if (nextInvoiceNo > maxInvoiceNoCompanyRegister)
            //    //{
            //    //    serverSignResult.Message = "The invoice number has exceeded the number of registrations";
            //    //    throw new BusinessLogicException(ResultCode.NumberInvoiceOverloadRegister, string.Format("The number invoice no [{0}] is exceed number invoice register ", nextInvoiceNo));
            //    //}
            //}
        }
        private string SignFileCallApiServerSign(ReleaseInvoiceMaster releaseInvoice, out string companyNameSign)
        {
            string result = "";
            var Company = myCompanyRepository.GetById(currentUser.Company.Id.Value);
            // truong hop la PGD thi get lai companyName hien thi tai phan chu ky
            companyNameSign = Company.COMPANYNAME;
            this.systemSettingRepository.GetByCompany((long)this.currentUser.Company.Id);
            foreach (var item in releaseInvoice.ReleaseInvoiceInfos)
            {
                var currentInvoice = invoiceRepository.GetById(item.InvoiceId);

                

                //check approve permission
                if (Company.LEVELCUSTOMER != "HO" && currentInvoice.COMPANYID != currentUser.Company.Id.Value)
                {
                    throw new BusinessLogicException(ResultCode.InvoiceHaveNoPermissionDataSign, "Không có quyền ký hóa đơn");
                }
                InvoicePrintInfo invoicePrint = this.invoiceRepository.GetInvoicePrintInfo(item.InvoiceId);
                if (invoicePrint.Invoice_Status == (int)InvoiceStatus.Released)
                {
                    SIGNATURE signature = this.signatureRepository.GetByCompany(currentInvoice.COMPANYID);
                    if (signature == null)
                    {
                        throw new BusinessLogicException(ResultCode.CaNotExisted, "Vui lòng upload file CA cho chi nhánh đang sử dụng");
                    }
                    this.invoiceBO.SetinvoiceInfo(invoicePrint);
                    //get filepath pdf
                    string pathFilePdf = getPathFilePdfSign(item.InvoiceId);
                    logger.Error("pathFilePdf", new Exception(pathFilePdf));
                    //get filepath xml
                    string pathFileXml = getPathFileXmlSign(item.InvoiceId);
                    string folderTree = PathUtil.FolderTree(invoicePrint.DateRelease);
                    string signedPdfPath = pathFilePdf.Replace("\\Invoice\\", "\\Releases\\Sign\\" + folderTree + "\\");
                    if (File.Exists(signedPdfPath))
                    {
                        result = "True";
                        continue;
                    }
                    string onlyPath = Directory.GetParent(signedPdfPath).FullName;
                    if (!Directory.Exists(onlyPath))
                    {
                        Directory.CreateDirectory(onlyPath);
                    }
                    //call api sign
                    result = callApiServerSign(pathFileXml, false, companyNameSign, item.InvoiceId, invoicePrint.DateRelease ?? DateTime.Now, signature, folderTree);
                    //Xóa file hóa đơn chưa ký trước đó
                    DeleteFileAfterSign(pathFilePdf);
                    DeleteFileAfterSign(pathFileXml);
                    // Add SignDetail
                    item.CompanyName = invoicePrint.CompanyName;
                    item.SerialNumber = signature.SERIALNUMBER;
                }
            }
            return result;
        }

        private void SendMailAfterSign(ReleaseInvoiceMaster releaseInvoice, EmailServerInfo emailServerInfo)
        {
            InvoicePrintInfo invoicePrint;
            InvoiceVerificationCode invoiceVerificationCode = new InvoiceVerificationCode();
            RELEASEINVOICEDETAIL releaseInvoiceDetail;
            foreach (var item in releaseInvoice.ReleaseInvoiceInfos)
            {
                invoicePrint = this.invoiceRepository.GetInvoicePrintInfo(item.InvoiceId);
                CLIENT client = this.clientRepository.GetById(invoicePrint.ClientId);
                string configCustomerCode = GetConfig("khachle");
                invoiceVerificationCode.Mobile = invoicePrint.MobileClient;
                invoiceVerificationCode.Email = client.USEREGISTEREMAIL != true ? client.RECEIVEDINVOICEEMAIL : invoicePrint.EmailClient;
                invoiceVerificationCode.CompanyId = invoicePrint.CompanyId;
                if (client.ISORG != true)
                {
                    continue;
                }

                releaseInvoiceDetail = this.releseInvoiceDetailRepository.FilteReleaseInvoiceDetail(item.InvoiceId);
                invoiceVerificationCode.VerificationCode = invoicePrint.VerificationCode;
                if (invoicePrint.ClientSendByMonth != true && invoiceVerificationCode.Email != null)
                {
                    SendVerificationCode(invoiceVerificationCode, emailServerInfo);
                }
            }
        }

        private void SendMailAfterSign(List<INVOICE> invoices,  List<CLIENT> objClients)
        {
            foreach (var item in invoices)
            {
                try
                {
                    string filePathPdf = Path.Combine(this.config.PathInvoiceFile, item.COMPANYID.ToString(), "Invoice", item.ID.ToString() + ".pdf");
                    string signedPdfPath = filePathPdf.Replace("\\Invoice\\", "\\Releases\\Sign\\" + PathUtil.FolderTree(item.RELEASEDDATE) + "\\").Replace(".pdf", "_sign.pdf");
                    if (File.Exists(signedPdfPath))
                    {
                        var objClient = objClients.FirstOrDefault(f => f.ID == item.CLIENTID);
                        SendMailAfterSignShinhan(item.ID, objClient);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("ProcessAfterSign", ex);
                }
            }
        }

        private void JobSendMailAfterSign(IEnumerable<InvoicesRelease> releaseInvoice, EmailServerInfo emailServerInfo)
        {
            InvoicePrintInfo invoicePrint;
            InvoiceVerificationCode invoiceVerificationCode = new InvoiceVerificationCode();
            RELEASEINVOICEDETAIL releaseInvoiceDetail;
            foreach (var item in releaseInvoice)
            {
                invoicePrint = this.invoiceRepository.GetInvoicePrintInfo(item.InvoiceId);
                CLIENT client = this.clientRepository.GetById(invoicePrint.ClientId);
                string configCustomerCode = GetConfig("khachle");
                invoiceVerificationCode.Mobile = invoicePrint.MobileClient;
                invoiceVerificationCode.Email = client.USEREGISTEREMAIL != true ? client.RECEIVEDINVOICEEMAIL : invoicePrint.EmailClient;
                invoiceVerificationCode.CompanyId = invoicePrint.CompanyId;
                if (client.ISORG != true)
                {
                    continue;
                }

                releaseInvoiceDetail = this.releseInvoiceDetailRepository.FilteReleaseInvoiceDetail(item.InvoiceId);
                invoiceVerificationCode.VerificationCode = releaseInvoiceDetail.VERIFICATIONCODE;
                if (invoicePrint.ClientSendByMonth != true && invoiceVerificationCode.Email != null)
                {
                    SendVerificationCode(invoiceVerificationCode, emailServerInfo);
                }
            }
        }
        private void SendVerificationCodeJobShinhan(InvoiceVerificationCode invoiceVerificationCode)
        {
            if (invoiceVerificationCode == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            invoiceVerificationCode.NumberFormat = this.currentUser.Company.NumberFormat.IsNullOrEmpty() ? "{0:#,##0.###}" : this.currentUser.Company.NumberFormat;
            SendEmailVerificationCode emailVerificationCodeInfo = this.invoiceBO.GetEmailVerificationCodeInfo(invoiceVerificationCode);
            this.emailActiveBO.SendEmailForJobShinhan(emailVerificationCodeInfo);
        }
        private void SendVerificationCode(InvoiceVerificationCode invoiceVerificationCode, EmailServerInfo emailServerInfo, bool isJob = false)
        {
            if (invoiceVerificationCode == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            invoiceVerificationCode.NumberFormat = this.currentUser.Company.NumberFormat.IsNullOrEmpty() ? "{0:#,##0.###}" : this.currentUser.Company.NumberFormat;
            long emailNoticeId = 0;
            SendEmailVerificationCode emailVerificationCodeInfo = this.invoiceBO.GetEmailVerificationCodeInfo(invoiceVerificationCode);
            emailNoticeId = this.invoiceBO.CreateEmailActive(invoiceVerificationCode.CompanyId, emailVerificationCodeInfo);
            this.emailActiveBO.SendEmailDaily(emailNoticeId, emailVerificationCodeInfo, isJob);
        }

        private string callApiServerSign(string pathFileXml, bool clientSign, string companyName, long invoiceId, DateTime dateSign, SIGNATURE signature, string dateFolder)
        {
            bool SignHsm = bool.Parse(WebConfigurationManager.AppSettings["SignHSM"] ?? "false");
            string json = new JavaScriptSerializer().Serialize(new
            {
                Password = signature.PASSWORD,
                PathFileXml = pathFileXml,
                ClientSign = clientSign,
                CompanyName = companyName,
                InvoiceId = invoiceId,
                DateSign = dateSign.ToString("dd/MM/yyyy"),
                SerialNumber = signature.SERIALNUMBER,
                SignHSM = SignHsm,
                Slot = signature.SLOTS,
                DateFolder = dateFolder
            });
            string response = String.Empty;
            string DATA = json.ToString();
            string UrlServerSign = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSign"], ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionSignInvoice);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlServerSign);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(DATA);
            request.ContentLength = byteArray.Length;
            using (Stream webStream = request.GetRequestStream())
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

        private void UpdateStatustAfterSign(IEnumerable<InvoicesRelease> invoicesRelease, Result release, UserSessionInfo currentUser)
        {
            //update status
            StatusReleaseInvoice statusReleaseInvoice = new StatusReleaseInvoice();
            statusReleaseInvoice.InvoicesRelease = invoicesRelease.ToList();
            statusReleaseInvoice.InvoicesRelease.ForEach(p =>
            {
                p.Signed = true;
                p.VerificationCode = GetVerificationCode(p.InvoiceId);
            });
            UpdateStatusReleaseInvoiceDetail(release.ReleaseId, statusReleaseInvoice, currentUser);
        }
        //
        public void SendMailSino()
        {
            var invoices = this.invoiceRepository.GetListInvoiceSendMail().ToList();
            List<SendEmailVerificationCode> emailVerificationCodes = new List<SendEmailVerificationCode>();
            foreach (var item in invoices)
            {
                RELEASEINVOICEDETAIL releaseInvoiceDetail = this.releseInvoiceDetailRepository.FilteReleaseInvoiceDetail(item.ID);
                var client = this.clientRepository.GetById((long)item.CLIENTID);
                InvoiceVerificationCode invoiceVerificationCode = new InvoiceVerificationCode();
                invoiceVerificationCode.Mobile = client.MOBILE;
                invoiceVerificationCode.Email = client.USEREGISTEREMAIL != true ? client.RECEIVEDINVOICEEMAIL : client.EMAIL;
                invoiceVerificationCode.VerificationCode = releaseInvoiceDetail.VERIFICATIONCODE;
                invoiceVerificationCode.CompanyId = item.COMPANYID;

                SendEmailVerificationCode emailVerificationCodeInfo = this.invoiceBO.GetEmailVerificationCodeInfo(invoiceVerificationCode);
                emailVerificationCodeInfo.EmailActiveId = this.invoiceBO.CreateEmailActive(invoiceVerificationCode.CompanyId, emailVerificationCodeInfo);
                emailVerificationCodes.Add(emailVerificationCodeInfo);
            }
            Task t = Task.Factory.StartNew(() => this.emailActiveBO.SendEmailSino(emailVerificationCodes));
            t.Wait();
            this.invoiceBO.UpdateStatusEmailActive(emailVerificationCodes);
            UpdateStatusSendMail(invoices);
        }
        public void SendMailByMonth(EmailServerInfo emailServerInfo)
        {
            var Day = DateTime.Today.Day;
            //get list client có ngày send mail là ngày hiện tại
            var listClient = this.clientRepository.GetListSendByMonth().Where(p => p.DATESENDINVOICE == Day).ToList();
            if (listClient.Count > 0)
            {
                foreach (var item in listClient)
                {
                    if (OtherExtensions.IsCurrentClient(item.CUSTOMERCODE))
                        continue;
                    item.EMAIL = item.USEREGISTEREMAIL != true ? item.RECEIVEDINVOICEEMAIL : item.EMAIL;

                    var listInvoice = this.invoiceRepository.GetListInvoiceByClient(item.ID).ToList();
                    if (item.EMAIL != null && listInvoice.Count > 0)
                    {
                        //gọi hàm gom và nén tất cả file hóa đơn đã ký trong tháng của this client
                        ZipfileSend(listInvoice, item);
                        //tiến hành gọi hàm send mail theo tháng
                        SendMailByMonthForClient(listInvoice, item, emailServerInfo);
                    }
                }
            }
        }
        private void SendMailByMonthForClient(IEnumerable<INVOICE> invoices, CLIENT client, EmailServerInfo emailServerInfo)
        {
            List<string> verificationCode = new List<string>();
            foreach (var item in invoices)
            {
                RELEASEINVOICEDETAIL releaseInvoiceDetail = this.releseInvoiceDetailRepository.FilteReleaseInvoiceDetail(item.ID);
                verificationCode.Add(releaseInvoiceDetail.VERIFICATIONCODE);
            }
            if ((client.USEREGISTEREMAIL == true ? client.EMAIL : client.RECEIVEDINVOICEEMAIL) != null)
            {
                SendVerificationCodeSendMonth(emailServerInfo, client, invoices, verificationCode);
            }
        }
        private void SendVerificationCodeSendMonth(EmailServerInfo emailServerInfo, CLIENT client, IEnumerable<INVOICE> invoices, List<string> listVericationCode)
        {
            if (OtherExtensions.IsCurrentClient(client.CUSTOMERCODE))
                return;
            InvoiceVerificationCode invoiceVerificationCode = new InvoiceVerificationCode();
            invoiceVerificationCode.Mobile = client.MOBILE;
            invoiceVerificationCode.Email = client.USEREGISTEREMAIL == true ? client.EMAIL : client.RECEIVEDINVOICEEMAIL;
            List<SendEmailVerificationCode> emailVerificationCodes = new List<SendEmailVerificationCode>();
            emailVerificationCodes = this.invoiceBO.GetVerificationCodeForSendMonth(invoiceVerificationCode, client, listVericationCode);
            long emailNoticeId = this.invoiceBO.CreateInvoiceEmailActiveSendByMonth(invoiceVerificationCode, client, listVericationCode);
            Task t = Task.Factory.StartNew(() => this.emailActiveBO.SendEmailByMonth(emailNoticeId, emailVerificationCodes));
            t.Wait();
            UpdateStatusSendMail(invoices);
        }

        private void UpdateStatusSendMail(IEnumerable<INVOICE> invoices)
        {
            foreach (var item in invoices)
            {
                INVOICE currenInvoice = GetInvoice(item.ID);
                currenInvoice.SENDMAILSTATUS = true;
                this.invoiceRepository.Update(currenInvoice);
            }
        }
        private void ZipfileSend(IEnumerable<INVOICE> listInvoice, CLIENT client)
        {
            //Tạo folder chứa tất cả file invoice đã ký của this client, tên folder là ID của this client
            string folderFileOfThisClient = Path.Combine(WebConfigurationManager.AppSettings["FolderInvoiceFile"], "FileSendByMonth", client.ID.ToString());
            if (!Directory.Exists(folderFileOfThisClient))
            {
                Directory.CreateDirectory(folderFileOfThisClient);
            }

            foreach (var item in listInvoice)
            {
                this.ZipfileSendItem(item, folderFileOfThisClient);
            }

            string fileNamZip = "ShinhanBank_Inv_" + client.CUSTOMERCODE + "_" + DateTime.Today.Month.ToString() + DateTime.Today.Year.ToString() + ".zip";
            string pathFileZip = Path.Combine(WebConfigurationManager.AppSettings["FolderInvoiceFile"], "FileSendByMonth", fileNamZip);

            if (File.Exists(pathFileZip))
            {
                File.Delete(pathFileZip);
            }

            using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
            {
                zip.Password = client.ISORG == true ? client.TAXCODE : client.IDENTITY;
                zip.AddDirectory(folderFileOfThisClient);
                zip.Save(pathFileZip);
            }

            if (Directory.Exists(folderFileOfThisClient))
            {
                Directory.Delete(folderFileOfThisClient, true);
            }
        }

        private void ZipfileSendItem(INVOICE item, string folderFileOfThisClient)
        {
            var tuple = this.SubZipfileSendItem(item, folderFileOfThisClient);
            string folderInvoiceOfCompany = tuple.Item1;
            string folderName = tuple.Item2;
            string fileName = tuple.Item3;
            string fileNameTo = tuple.Item4;

            fileName = fileName.Replace("_signCancel", "_sign.");
            string folderInvoiceXmlFrom = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, folderName, fileName + "xml");
            if (File.Exists(folderInvoiceXmlFrom))
            {
                string folderInvoiceXmlTo = folderFileOfThisClient + "\\" + fileNameTo + ".xml";
                if (!File.Exists(folderInvoiceXmlTo))
                {
                    File.Copy(folderInvoiceXmlFrom, folderInvoiceXmlTo);
                }
            }
            //Do file bangke k có file ký client -> chỉ có file 1 chữ sign
            //copy file bangr kee đã ký từ folder sign to folder this client để gửi mail
            string folderStatisticalFrom = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, folderName, string.Format("{0}_statistical_sign.pdf", item.ID));
            if (File.Exists(folderStatisticalFrom))
            {
                string folderStatisticalTo = folderFileOfThisClient + "\\" + string.Format("{0}_{1}_statistical_Signed.pdf", item.ID, item.NO);
                if (!File.Exists(folderStatisticalTo))
                {
                    File.Copy(folderStatisticalFrom, folderStatisticalTo);
                }
            }
            //Do file quà tặng k có file ký client -> chỉ có file 1 chữ sign
            //copy file quà tặng đã ký từ folder sign to folder this client để gửi mail
            string folderInvoiceGiftFrom = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, folderName, string.Format("{0}_invoiceGift_sign.pdf", item.ID));
            if (File.Exists(folderInvoiceGiftFrom))
            {
                string folderInvoiceGiftTo = folderFileOfThisClient + "\\" + string.Format("{0}_{1}_invoiceGift_Signed.pdf", item.ID, item.NO);
                if (!File.Exists(folderInvoiceGiftTo))
                {
                    File.Copy(folderInvoiceGiftFrom, folderInvoiceGiftTo);
                }
            }
        }

        private Tuple<string, string, string, string> SubZipfileSendItem(INVOICE item, string folderFileOfThisClient)
        {
            string folderInvoiceOfCompany = Path.Combine(config.PathInvoiceFile, item.COMPANYID.ToString());
            string fileName;
            string fileNameTo = item.ID + "_" + item.SYMBOL.Replace("/", "") + "_" + item.NO + "_" + item.RELEASEDDATE.Value.ToString("ddMMyyyy") + "_Signed";
            //copy file pdf đã ký từ folder sign to folder this client để gửi mail
            if (item.CLIENTSIGN == true)
            {
                fileName = string.Format("{0}_sign_sign.", item.ID);
            }
            else
            {
                fileName = string.Format("{0}_sign.", item.ID);
            }
            if (item.INVOICESTATUS == (int)InvoiceStatus.Cancel)
            {
                fileName = fileName.Replace("_sign.", "_signCancel.");
            }
            string folderName = PathUtil.FolderTree(item.RELEASEDDATE);
            string folderInvoicePdfFrom = Path.Combine(folderInvoiceOfCompany, AssetSignInvoice.Release, AssetSignInvoice.SignFile, folderName, fileName + "pdf");
            if (File.Exists(folderInvoicePdfFrom))
            {
                string folderInvoicePdfTo = folderFileOfThisClient + "\\" + fileNameTo + ".pdf";
                if (!File.Exists(folderInvoicePdfTo))
                {
                    File.Copy(folderInvoicePdfFrom, folderInvoicePdfTo);
                }
            }
            //copy file xml đã ký từ folder sign to folder this client để gửi mail

            return new Tuple<string, string, string, string>(folderInvoiceOfCompany, folderName, fileName, fileNameTo);
        }

        private string getPathFilePdfSign(long invoiceId)
        {
            //get path file
            ExportFileInfo file = this.invoiceBO.PrintView(invoiceId);
            return Path.Combine(file.FullPathFileName);
        }
        //
        private string getPathFileXmlSign(long invoiceId)
        {
            //get path file
            ExportFileInfo exportFile = this.invoiceBO.GetXmlFile(invoiceId);
            return Path.Combine(exportFile.FullPathFileName);
        }
        private string GetVerificationCode(long invoiceId)
        {
            string verificationCode = string.Format("{0}{1}", DateTime.Now.ToString(), invoiceId).GetHashCode().ToString("x");
            return verificationCode.ToUpper();
        }
        public EmailServerInfo GetEmailServerInfo()
        {
            var emailServer = this.configEmailServerRepository.GetByCompany();
            if (emailServer == null)
            {
                return null;
            }

            return new EmailServerInfo(emailServer);
        }

        private void AddSignDetail(long invoiceId, string serialNumber, string companyName)
        {
            var signDetail = new SIGNDETAIL()
            {
                INVOICEID = invoiceId,
                SERIALNUMBER = serialNumber,
                NAME = companyName,
                ISCLIENTSIGN = false,
                CREATEDDATE = DateTime.Now,
                TYPESIGN = (int)SignDetailTypeSign.Invoice,
            };
            this.signDetailRepository.Insert(signDetail);
        }

        private void AddSignDetail(List<INVOICE> invoices, string serialNumber, string companyName)
        {
            List<SIGNDETAIL> signDetails = new List<SIGNDETAIL>();
            foreach (var item in invoices)
            {
                SIGNDETAIL signDetail = new SIGNDETAIL();
                signDetail.INVOICEID = item.ID;
                signDetail.SERIALNUMBER = serialNumber;
                signDetail.NAME = companyName;
                signDetail.ISCLIENTSIGN = false;
                signDetail.CREATEDDATE = DateTime.Now;
                signDetail.TYPESIGN = (int)SignDetailTypeSign.Invoice;
                signDetails.Add(signDetail);
            }
            this.signDetailRepository.InsertMultipe(signDetails);
        }

        private void DeleteFileAfterSign(string pathFile)
        {
            try
            {
                if (File.Exists(pathFile))
                {
                    File.Delete(pathFile);
                }
                string fileStatical = pathFile.Replace(".pdf", "_statistical.pdf");
                if (File.Exists(fileStatical))
                {
                    File.Delete(fileStatical);
                }
                string fileGift = pathFile.Replace(".pdf", "_invoiceGift.pdf");
                if (File.Exists(fileGift))
                {
                    File.Delete(fileGift);
                }
            }
            catch (Exception ex)
            {
                logger.Error(this.currentUser.UserId, ex);
            }
        }
        public void SignShinhan(List<int> lstInvoice, string companyName, SIGNATURE signature)
        {
            //call api sign
            string resultSign = ApiSignShinhan(companyName, DateTime.Now, signature, lstInvoice);
            //update status
            //sendmail
        }
        private string ApiSignShinhan(string companyName, DateTime dateSign, SIGNATURE signature, List<int> lstInvoice)
        {
            bool SignHsm = bool.Parse(WebConfigurationManager.AppSettings["SignHSM"] ?? "false");
            List<int> invoiceIds = lstInvoice;
            string json = new JavaScriptSerializer().Serialize(new
            {
                Password = signature.PASSWORD,
                ClientSign = false,
                CompanyName = companyName,
                DateSign = dateSign.ToString("dd/MM/yyyy"),
                SerialNumber = signature.SERIALNUMBER,
                SignHSM = SignHsm,
                Slot = signature.SLOTS,
                DateFolder = dateSign.ToString("yyyyMMdd"),
                InvoiceIds = invoiceIds
            });
            string response = String.Empty;
            string DATA = json.ToString();
            string UrlServerSign = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSign"], ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionSignInvoice);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlServerSign);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(DATA);
            request.ContentLength = byteArray.Length;
            //request.Timeout = 
            using (Stream webStream = request.GetRequestStream())
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
        #endregion
    }
}