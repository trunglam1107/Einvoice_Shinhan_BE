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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness'
    public class InvoiceBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness'
    {
        #region Fields, Properties

        private readonly IInvoiceBO invoiceBO;
        private readonly IReleaseInvoiceBO releaseInvoiceBO;
        private readonly IRegisterTemplatesBO registerTemplateBO;
        private readonly PrintConfig printConfig;
        private readonly IEmailActiveBO emailActiveBO;
        private readonly INoticeUseInvoiceBO noticeUseInvoiceBO;
        private readonly IInvoiceSampleBO invoiceSampleBo;
        private readonly INoticeUseInvoiceDetailBO noticeUseInvoiceDetailBO;
        private readonly IInvoiceTemplateBO invoiceTemplateBO;
        private readonly ISessionManagerBO sessionManagerBO;
        private readonly IMyCompanyBO myCompanyBO;
        private readonly ISignatureBO signatureBO;
        private static readonly Logger logger = new Logger();
        private readonly SystemLogBusiness systemLogBusiness;
        private readonly IQuarztJobBO quarztJobBO;
        //private readonly IGenerateFile generateFile;
        #endregion Fields, Properties
        private readonly IBOFactory _bOFactory;

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.InvoiceBusiness(IBOFactory)'
        public InvoiceBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.InvoiceBusiness(IBOFactory)'
        {
            _bOFactory = boFactory;
            var config = new SessionConfig()
            {
                LoginSecretKey = Config.ApplicationSetting.Instance.LoginSecretKey,
                SessionTimeout = TimeSpan.FromMinutes(Config.ApplicationSetting.Instance.SessionTimeout),
                CreateTokenRandom = Config.ApplicationSetting.Instance.EnableCreateTokenRandom,
                SessionResetPasswordExpire = TimeSpan.FromMinutes(Config.ApplicationSetting.Instance.ResetPasswordTimeOut),
            };
            var uploadImageConfig = new UpdateloadImageConfig
            {
                MaxSizeImage = Config.ApplicationSetting.Instance.MaxSizeImage,
                RootFolderUpload = DefaultFields.ASSET_FOLDER
            };
            if (HttpContext.Current != null)
            {
                printConfig = GetPrintConfig();
            }
            else
            {
                printConfig = GetPrintConfigbyJob();
            }
            string location = HttpContext.Current == null ? null : HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath);
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = location,
            };
            var locationAsset = DefaultFields.ASSET_FOLDER;
            if (HttpContext.Current == null)
            {
                this.invoiceBO = boFactory.GetBO<IInvoiceBO>(printConfig, emailConfig);
            }
            else
            {
                this.invoiceBO = boFactory.GetBO<IInvoiceBO>(printConfig, emailConfig, this.CurrentUser);
            }

            this.releaseInvoiceBO = boFactory.GetBO<IReleaseInvoiceBO>(printConfig, this.CurrentUser, emailConfig);

            this.registerTemplateBO = boFactory.GetBO<IRegisterTemplatesBO>(printConfig);
            this.emailActiveBO = boFactory.GetBO<IEmailActiveBO>(printConfig);
            this.noticeUseInvoiceBO = boFactory.GetBO<INoticeUseInvoiceBO>(printConfig, this.CurrentUser);
            this.noticeUseInvoiceDetailBO = boFactory.GetBO<INoticeUseInvoiceDetailBO>();
            this.invoiceTemplateBO = boFactory.GetBO<IInvoiceTemplateBO>(new PrintConfig(locationAsset));
            this.sessionManagerBO = boFactory.GetBO<ISessionManagerBO>(config);
            this.myCompanyBO = boFactory.GetBO<IMyCompanyBO>(uploadImageConfig);
            this.signatureBO = boFactory.GetBO<ISignatureBO>();
            this.systemLogBusiness = new SystemLogBusiness(boFactory);
            //this.invoiceTemplateRepository = boFactory.GetBO<IInvoiceTemplateRepository>();
            this.invoiceSampleBo = boFactory.GetBO<IInvoiceSampleBO>(this.CurrentUser);
            this.quarztJobBO = boFactory.GetBO<IQuarztJobBO>();
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.InvoiceBusiness(IBOFactory, bool)'
        public InvoiceBusiness(IBOFactory boFactory, bool byPass)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.InvoiceBusiness(IBOFactory, bool)'
        {
            if (byPass)
                printConfig = GetPrintConfigbyPass();
            else
                printConfig = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };
            this.invoiceBO = boFactory.GetBO<IInvoiceBO>(printConfig, emailConfig);
            this.registerTemplateBO = boFactory.GetBO<IRegisterTemplatesBO>(printConfig);
            this.emailActiveBO = boFactory.GetBO<IEmailActiveBO>(printConfig);
            this.noticeUseInvoiceDetailBO = boFactory.GetBO<INoticeUseInvoiceDetailBO>();

        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.Fillter(ConditionSearchInvoice)'
        public IEnumerable<InvoiceMaster> Fillter(ConditionSearchInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.Fillter(ConditionSearchInvoice)'
        {
            condition.TotalRecords = this.invoiceBO.Count(condition);
            return this.invoiceBO.Filter(condition);
        }

        public IEnumerable<InvoiceCheckDaily> FilterInvoiceCheckDaily(ConditionSearchCheckNumberInvoice condition)
        {
            //condition.TotalRecords = this.invoiceBO.CountInvoiceCheckDaily(condition);
            return this.invoiceBO.FilterInvoiceCheckDaily(condition);
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.FillterSP(ConditionSearchInvoice)'
        public IEnumerable<InvoiceMaster> FillterSP(ConditionSearchInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.FillterSP(ConditionSearchInvoice)'
        {
            //condition.TotalRecords = this.invoiceBO.Count(condition);
            //return this.invoiceBO.Filter(condition);
            var result = this.invoiceBO.FilterSP(condition);
            condition.TotalRecords = result.Count() > 0 ? result.FirstOrDefault().CountRecord : 0;
            return result;
        }

        public IEnumerable<InvoiceMaster> FilterSP_Replace(ConditionSearchInvoice condition)
        {
            var result = this.invoiceBO.FilterSP_Replace(condition);
            condition.TotalRecords = result.Count();
            return result;
        }
        public IEnumerable<InvoiceMaster> FillterV2(ConditionSearchInvoice condition)
        {
            condition.TotalRecords = this.invoiceBO.CountV2(condition);
            return this.invoiceBO.FilterV2(condition);
        }

        private IEnumerable<ReleaseInvoiceInfo> JobApprove(long? branch)
        {
            var keyOfPackage = Config.ApplicationSetting.Instance.NumOfPackage;
            UserSessionInfo user = this.sessionManagerBO.JobLogin(keyOfPackage);
            List<int> invoiceStatus = new List<int>() { (int)InvoiceStatus.New };
            var condition = new ConditionSearchInvoice("CREATEDDATE", "ASC", invoiceStatus)
            {
                CurrentUser = user,
                Skip = 0,
                Branch = branch,
                ColumnOrder = "InvoiceId"
            };
            var total = this.invoiceBO.CountInvoiceCreated(condition, 0);
            var take = total / 1000;
            List<ReleaseInvoiceInfo> listInvoiceMaster = new List<ReleaseInvoiceInfo>();
            for (int i = 0; i <= take; i++)
            {
                condition.Skip = i * 1000;
                condition.Take = 1000;
                var invoiceMasters = this.invoiceBO.FilterListCreated(condition, 1);
                listInvoiceMaster.AddRange(invoiceMasters);
            }
            return listInvoiceMaster;
        }

        private List<ApprovedInvoice> listApproveInfor()
        {
            var keyOfPackage = Config.ApplicationSetting.Instance.NumOfPackage;
            UserSessionInfo user = this.sessionManagerBO.JobLogin(keyOfPackage);
            var listBranch = myCompanyBO.GetList();
            List<ApprovedInvoice> result = new List<ApprovedInvoice>();
            foreach (var branch in listBranch)
            {
                ApprovedInvoice appInvoice = new ApprovedInvoice();
                List<ReleaseInvoiceInfo> releases = JobApprove(branch.Id).ToList();
                appInvoice.CompanyId = branch.Id;
                appInvoice.UserAction = user.Id;
                appInvoice.UserActionId = user.Id;
                appInvoice.ReleaseInvoiceInfos = releases;
                result.Add(appInvoice);
            }
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ApproveNow()'
        public void ApproveNow()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ApproveNow()'
        {
            logger.Trace($"ApproveNow() -- Start listApproveInfor()");
            var approve = listApproveInfor();
            logger.Trace($"ApproveNow() -- listApproveInfor() done");
            foreach (var app in approve)
            {
                if (app.ReleaseInvoiceInfos.Count > 0)
                {
                    ApproveInvoice(app);
                }
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.JobServerSign(long)'
        public void JobServerSign(long Branch)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.JobServerSign(long)'
        {
            if (Branch == 0)
            {
                // run tat ca cac chi nhanh
                var listBranch = this.noticeUseInvoiceBO.GetListCompany().ToList();
                foreach (var branchCurrent in listBranch)
                {
                    ReleaseInvoiceMaster releaseInvoice = new ReleaseInvoiceMaster();
                    releaseInvoice.CompanyId = branchCurrent;
                    releaseInvoice.UserAction = this.CurrentUser.Id;
                    releaseInvoice.UserActionId = this.CurrentUser.Id;
                    releaseInvoice.ReleasedDate = DateTime.Now;
                    logger.Trace($"Start getListInvoiceApproved({branchCurrent})");
                    releaseInvoice.ReleaseInvoiceInfos = getListInvoiceApproved(branchCurrent).ToList();
                    logger.Trace($"Start JobSignFile()");
                    this.releaseInvoiceBO.JobSignFile(releaseInvoice, this.CurrentUser);
                }
            }
            else
            {
                // run tren duy nhat 1 chi nhanh -> lay thong tin chi nhanh dang nhap
                ReleaseInvoiceMaster releaseInvoice = new ReleaseInvoiceMaster();
                releaseInvoice.CompanyId = this.CurrentUser.Company.Id;
                releaseInvoice.UserAction = this.CurrentUser.Id;
                releaseInvoice.UserActionId = this.CurrentUser.Id;
                releaseInvoice.ReleasedDate = DateTime.Now;
                logger.Trace($"Start getListInvoiceApproved({(long)releaseInvoice.CompanyId})");
                releaseInvoice.ReleaseInvoiceInfos = getListInvoiceApproved((long)releaseInvoice.CompanyId).ToList();
                logger.Trace($"Start JobSignFile()");
                this.releaseInvoiceBO.JobSignFile(releaseInvoice, this.CurrentUser);
            }
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.UpdateProcessJob(string, bool)'
        public void UpdateProcessJob(string name, bool processing)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.UpdateProcessJob(string, bool)'
        {
            this.quarztJobBO.UpdateProcessByName(name, processing);
        }

        public QUARZTJOB findByName(string name)
        {
            return this.quarztJobBO.findByName(name);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.JobApproveShinhan()'
        public void JobApproveShinhan()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.JobApproveShinhan()'
        {
            #region Code cũ
            //var listBranch = this.noticeUseInvoiceBO.GetListCompanyShinhan().ToList();
            //Approve_Old(listBranch);
            //logger.QuarztJob(false, $"Start approve invoice");
            var listBranch = this.invoiceBO.listSymbol().OrderBy(x => x.CompanyId);
            logger.QuarztJob(false, $"listBranch count : {listBranch.Count()}");
            foreach (var item in listBranch)
            {
                logger.QuarztJob(false, $"Companyid {item.CompanyId} + Symbol {item.Symbol}");
                this.invoiceBO.JobApproveNew(DateTime.Now.ToString("yyyyMMdd"), item.CompanyId, item.Symbol,null);
                //Thread.Sleep(2000);
            }

            #endregion
            //ApprovePersonal_old(listBranch);
        }


        #region Hàm duyệt cũ trước khi chỉnh 
        private void Approve_Old(List<MYCOMPANY> companys)
        {
            foreach (var item in companys)
            {
                //get invoice need approve by company
                //logger.QuarztJob(false, $"Get invoice need approve by company start {DateTime.Now}");
                var invoices = this.invoiceBO.GetListInvoiceNewShinhan(item.COMPANYSID, true).OrderBy(x => x.RELEASEDDATE).ToList();
                //logger.Error("GetInvoiceApproveByCompany | invoices + COMPANYSID : " + item.COMPANYSID.ToString(), new Exception(invoices.Count().ToString()));
                //logger.QuarztJob(false, $"Get invoice need approve by company done {DateTime.Now}");
                if (invoices.Count() > 0)
                {
                    var take = 500;
                    int squen = (invoices.Count() / take) + 1;
                    for (int i = 0; i < squen; i++)
                    {
                        var listInvoice = new List<INVOICE>(invoices.Skip(i * take).Take(take).ToList());

                        //logger.Error("ApproveOrg | for : ", new Exception(listInvoice.Count().ToString()));

                        if (listInvoice.Count() > 0)
                        {
                            this.invoiceBO.JobApproveItemShinhan_Old(listInvoice, item);
                        }

                        //invoices = this.invoiceBO.GetListInvoiceNewShinhan(item.COMPANYSID);
                    }
                }

                //this.invoiceBO.UpdateNoForCompany(item.COMPANYSID);
            }

            //logger.QuarztJob(false, $"ApproveAll done {DateTime.Now}");
        }
        //private void ApprovePersonal_old(List<MYCOMPANY> companys)
        //{

        //    foreach (var item in companys)
        //    {
        //        //get invoice need approve by company
        //        var invoices = this.invoiceBO.GetListInvoiceNewShinhan(item.COMPANYSID, false).OrderBy(p => p.RELEASEDDATE).ToList();
        //        //logger.Error("ApprovePersonal | invoices + COMPANYSID : " + item.COMPANYSID.ToString(), new Exception(invoices.Count().ToString()));
        //        if (invoices.Count() > 0)
        //        {
        //            var take = 3000;
        //            var count = (invoices.Count() / take) + 1;
        //            for (int i = 0; i < count; i++)
        //            {
        //                var listInvoice = new List<INVOICE>(invoices.OrderBy(xx => 1 == 1).Skip(i * take).Take(take).ToList());

        //                //logger.Error("ApprovePersonal | invoices count: ", new Exception(invoices.Count().ToString()));

        //                //logger.Error("ApprovePersonal | listInvoice count: " + i.ToString(), new Exception(listInvoice.Count().ToString()));

        //                if (listInvoice.Count() > 0)
        //                {
        //                    this.invoiceBO.JobApproveItemShinhan_Old(listInvoice, item);
        //                }
        //            }
        //        }
        //    }
        //}
        #endregion
        //private void ApproveOrg(List<MYCOMPANY> companys)
        //{
        //    foreach (var item in companys)
        //    {
        //        //get invoice need approve by company
        //        var invoices = this.invoiceBO.GetListInvoiceNewShinhan(item.COMPANYSID, true);//.OrderBy(p => p.ID).ToList();
        //        //logger.Error("ApproveOrg | invoices + COMPANYSID : " + item.COMPANYSID.ToString(), new Exception(invoices.Count().ToString()));
        //        if (invoices.Any())
        //        {
        //            var take = 1000;

        //            // duyệt cho tới khi hết
        //            while (true == true)
        //            {
        //                // Lấy danh sách hóa đơn để duyệt                        
        //                var listInvoice = invoices.OrderBy(f => f.RELEASEDDATE).Take(take).ToList();

        //                if (listInvoice.Any())
        //                {                           
        //                    this.invoiceBO.JobApproveItemShinhan(listInvoice);                           
        //                }
        //                else
        //                {
        //                    break;
        //                }
        //            }

        //        }
        //    }
        //}
        ////Approve performance
        //private void ApproveOrg_New(List<MYCOMPANY> companys)
        //{
        //    foreach (var item in companys)
        //    {
        //        //get invoice need approve by company
        //        var invoices = this.invoiceBO.GetListInvoiceNewShinhan(item.COMPANYSID, true).OrderBy(f => f.RELEASEDDATE).Take(5000).ToList(); ;//.OrderBy(p => p.ID).ToList();
        //        //logger.Error("ApproveOrg | invoices + COMPANYSID : " + item.COMPANYSID.ToString(), new Exception(invoices.Count().ToString()));
        //        if (invoices.Any())
        //        {
        //            var take = 100;

        //            // duyệt cho tới khi hết
        //            //while (true == true)
        //            //{
        //            //    // Lấy danh sách hóa đơn để duyệt                        
        //            //    var listInvoice = invoices.OrderBy(f => f.RELEASEDDATE).Take(take).ToList();

        //            //    if (listInvoice.Any())
        //            //    {                           
        //            //        this.invoiceBO.JobApproveItemShinhan(listInvoice, item);                           
        //            //    }
        //            //    else
        //            //    {
        //            //        break;
        //            //    }
        //            //}
        //            int total = invoices.Count;
        //            var count = (total / take) + 1;
        //            Dictionary<int, List<INVOICE>> keyValuePairs = new Dictionary<int, List<INVOICE>>();
        //            int index = 0;
        //            for (int i = 0; i < count; i++)
        //            {
        //                var listInvoice = invoices.Skip(i * take).Take(take).ToList();
        //                if (listInvoice.Count > 0)
        //                {

        //                    keyValuePairs.Add(index, listInvoice);
        //                    index += 1;
        //                }
        //            }
        //            var currentUser = this.CurrentUser;
        //            Parallel.For(0, index, async (i) =>
        //            {
        //                var bo = new BOFactory(DbContextManager.GetContext());
        //                string location = HttpContext.Current == null ? null : HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath);
        //                var emailConfig = new EmailConfig()
        //                {
        //                    FolderEmailTemplate = location,
        //                };
        //                var invoiceBoRe = bo.GetBO<IInvoiceBO>(printConfig, emailConfig, currentUser);
        //                invoiceBoRe.JobApproveItemShinhan(keyValuePairs[i]);
        //            });

        //        }
        //    }
        //}

        //private void ApprovePersonal(List<MYCOMPANY> companys)
        //{
        //    var take = 1000;
        //    foreach (var item in companys)
        //    {
        //        //get invoice need approve by company
        //        var invoices = this.invoiceBO.GetListInvoiceNewShinhan(item.COMPANYSID, false);//.OrderBy(p => p.ID).ToList();
        //        //logger.Error("ApprovePersonal | invoices + COMPANYSID : " + item.COMPANYSID.ToString(), new Exception(invoices.Count().ToString()));
        //        if (invoices.Any())
        //        {

        //            // duyệt cho tới khi hết
        //            while (true == true)
        //            {
        //                // Lấy danh sách hóa đơn để duyệt                        
        //                var listInvoice = invoices.OrderBy(f => f.RELEASEDDATE).Take(take).ToList();

        //                if (listInvoice.Any())
        //                {
        //                    this.invoiceBO.JobApproveItemShinhan(listInvoice);
        //                }
        //                else
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}

        //private void ApprovePersonal_New(List<MYCOMPANY> companys)
        //{
        //    var take = 1000;
        //    foreach (var item in companys)
        //    {
        //        //get invoice need approve by company
        //        var invoices = this.invoiceBO.GetListInvoiceNewShinhan(item.COMPANYSID, false).OrderBy(f => f.RELEASEDDATE).Take(5000).ToList();//.OrderBy(p => p.ID).ToList();
        //                                                                                                                                        //logger.Error("ApprovePersonal | invoices + COMPANYSID : " + item.COMPANYSID.ToString(), new Exception(invoices.Count().ToString()));
        //                                                                                                                                        //if (invoices.Any())
        //                                                                                                                                        //{

        //        //    // duyệt cho tới khi hết
        //        //    while (true == true)
        //        //    {
        //        //        // Lấy danh sách hóa đơn để duyệt                        
        //        //        var listInvoice = invoices.OrderBy(f => f.RELEASEDDATE).Take(take).ToList();

        //        //        if (listInvoice.Any())
        //        //        {
        //        //            this.invoiceBO.JobApproveItemShinhan(listInvoice, item);
        //        //        }
        //        //        else
        //        //        {
        //        //            break;
        //        //        }
        //        //    }

        //        //}
        //        int total = invoices.Count;
        //        var count = (total / take) + 1;
        //        Dictionary<int, List<INVOICE>> keyValuePairs = new Dictionary<int, List<INVOICE>>();
        //        int index = 0;
        //        for (int i = 0; i < count; i++)
        //        {
        //            var listInvoice = invoices.Skip(i * take).Take(take).ToList();
        //            if (listInvoice.Count > 0)
        //            {

        //                keyValuePairs.Add(index, listInvoice);
        //                index += 1;
        //            }
        //        }
        //        var currentUser = this.CurrentUser;
        //        Parallel.For(0, index, async (i) =>
        //        {
        //            var bo = new BOFactory(DbContextManager.GetContext());
        //            string location = HttpContext.Current == null ? null : HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath);
        //            var emailConfig = new EmailConfig()
        //            {
        //                FolderEmailTemplate = location,
        //            };
        //            var invoiceBoRe = bo.GetBO<IInvoiceBO>(printConfig, emailConfig, currentUser);
        //            invoiceBoRe.JobApproveItemShinhan(keyValuePairs[i]);
        //        });
        //    }
        //}
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.JobSendMail()'
        public void JobSendMail()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.JobSendMail()'
        {
            var lstBranch = this.noticeUseInvoiceBO.GetListCompanySendMails();
            foreach(var item in lstBranch)
            {
                //gửi mail cho từng chi nhánh
                this.invoiceBO.SendMailJob(item.COMPANYSID);
            }
            
        }
        public void JobCreateFile()
        {
            #region Code của anh Phuong
            //var listBranch = this.noticeUseInvoiceBO.GetListCompanyShinhan();
            var listBranch = this.noticeUseInvoiceBO.GetListCompanyNeedCreateFiles();
            //var listSymolBranch = this.noticeUseInvoiceBO.GetSymbolCompanyForSignJob();
            logger.Error("listBranch", new Exception(listBranch.Count().ToString()));
            SignFileJob(listBranch);
            #endregion

        }
        private void SignFileJob(List<MYCOMPANY> companys)
        {
            foreach (var branchCurrent in companys)
            {
                try
                {
                    //var checkInvoiceNo = this.invoiceBO.checkBeforeSign(branchCurrent.CompanyId ?? 0, branchCurrent.Symbol.Substring(branchCurrent.Symbol.Length - 2, 2));
                    //if (!checkInvoiceNo)
                    //{
                    //    logger.Error("InvoiceNo invalid: " + branchCurrent.CompanyId + ", " + branchCurrent.Symbol, new Exception("SignFileJob"));
                    //    continue;
                    //}
                    var mycompany = this.myCompanyBO.GetCompanyById(branchCurrent.COMPANYSID);
                    //var total = this.invoiceBO.GetAllInvoiceApprovedNotCreateFileShinhan(branchCurrent.COMPANYSID, true).Count();
                    logger.Error("start get all: " + branchCurrent.COMPANYSID, new Exception(branchCurrent.COMPANYSID.ToString()));
                    var listAll = this.invoiceBO.GetInvoicePrintCreateFile(branchCurrent.COMPANYSID);
                    logger.Error("done get all: " + branchCurrent.COMPANYSID, new Exception(branchCurrent.COMPANYSID.ToString()));
                    var template = this.invoiceBO.GetTemplateNoTracking(1);
                    var totalAll = listAll.Count();
                    if (totalAll == 0)
                    {
                        logger.Error("Invoice not found for sign:  " + branchCurrent.COMPANYSID, new Exception("job sign"));
                        continue;
                    }
                    int takeList = 5000;
                    int skipList = 0;
                    int loop = (totalAll / takeList) + 1;
                    logger.Error("total: " + totalAll + ", loop " + loop + " lan", new Exception("totalAll"));
                    //try
                    //{
                    //    List<InvoicePrintModel> invoiceModels = new List<InvoicePrintModel>();
                    //    List<InvoiceExportModel> invoiceExportPdfs = new List<InvoiceExportModel>();
                    //    var listProcess = new List<InvoicePrintModel>(listAll);
                    //    int total = listProcess.Count();
                    //    int thread = 30;
                    //    var skip = 0;
                    //    var take = (total / thread) + 1;
                    //    List<Task> TaskList = new List<Task>();
                    //    for (int i = 1; i <= thread; i++)
                    //    {
                    //        //task
                    //        var invoices = new List<InvoicePrintModel>(listProcess.Skip(skip).Take(take));
                    //        logger.Error("Start get data task " + i, new Exception("printInvoice"));
                    //        var printInvoice = this.invoiceBO.ProcessInvoicePrint(invoices, mycompany, template);
                    //        logger.Error("Start task xml " + i, new Exception(printInvoice.invoiceExportXmlModels.Count().ToString()));
                    //        var task = Task.Run(() => this.invoiceBO.ProcessXmls(printInvoice.invoiceExportXmlModels));
                    //        logger.Error("End task xml " + i, new Exception(printInvoice.invoiceExportXmlModels.Count().ToString()));
                    //        invoiceModels.AddRange(invoices);
                    //        invoiceExportPdfs.AddRange(printInvoice.invoiceExportPdfModels);
                    //        skip = take * i;
                    //        TaskList.Add(task);
                    //    }
                    //    Task.WaitAll(TaskList.ToArray());
                    //    logger.Error("Start Sign Xml", new Exception(invoiceModels.Count().ToString()));
                    //    var signXml = Task.Run(() => this.invoiceBO.SignXmls(invoiceModels));
                    //    var totalPdf = invoiceExportPdfs.Count();
                    //    logger.Error("start task pdf", new Exception(totalPdf.ToString()));
                    //    var skippdf = 0;
                    //    var takepdf = (totalAll / thread) + 1;
                    //    List<Task> TaskListPdf = new List<Task>();
                    //    for (int i = 1; i <= thread; i++)
                    //    {
                    //        //task
                    //        var invoices = new List<InvoiceExportModel>(invoiceExportPdfs.Skip(skippdf).Take(takepdf));
                    //        //logger.Error("Start task pdf " + i, new Exception(invoices.Count().ToString()));
                    //        var taskpdf = Task.Run(() => this.invoiceBO.ProcessPdfs(invoices));

                    //        skippdf = takepdf * i;
                    //        TaskListPdf.Add(taskpdf);
                    //    }

                    //    Task.WaitAll(TaskListPdf.ToArray());
                    //    Task.WaitAll(signXml);
                    //    //logger.Error("signXml and wait all pdf", new Exception("15 task"));
                    //    this.invoiceBO.UpdateStatusAfterSign(invoiceModels);
                    //    logger.Error("Sign succcess Company: " + branchCurrent.COMPANYSID, new Exception("Sign success"));
                    //    Thread.Sleep(5000);
                    //}
                    //catch (Exception ex)
                    //{
                    //    logger.Error("Sign error for company: " + branchCurrent.COMPANYSID, ex);
                    //}
                    for (int index = 1; index <= loop; index++)
                    {
                        try
                        {
                            List<InvoicePrintModel> invoiceModels = new List<InvoicePrintModel>();
                            List<InvoiceExportModel> invoiceExportPdfs = new List<InvoiceExportModel>();
                            var listProcess = new List<InvoicePrintModel>(listAll.Skip(skipList).Take(takeList));
                            skipList = takeList * index;
                            int total = listProcess.Count();
                            int thread = 30;
                            var skip = 0;
                            var take = (total / thread) + 1;
                            List<Task> TaskList = new List<Task>();
                            for (int i = 1; i <= thread; i++)
                            {
                                //task
                                var invoices = new List<InvoicePrintModel>(listProcess.Skip(skip).Take(take));
                                logger.Error("Start get data task " + i, new Exception("printInvoice"));
                                var printInvoice = this.invoiceBO.ProcessInvoicePrint(invoices, mycompany, template);
                                logger.Error("Start task xml " + i, new Exception(printInvoice.invoiceExportXmlModels.Count().ToString()));
                                var task = Task.Run(() => this.invoiceBO.ProcessXmls(printInvoice.invoiceExportXmlModels));
                                logger.Error("End task xml " + i, new Exception(printInvoice.invoiceExportXmlModels.Count().ToString()));
                                invoiceModels.AddRange(invoices);
                                invoiceExportPdfs.AddRange(printInvoice.invoiceExportPdfModels);
                                skip = take * i;
                                TaskList.Add(task);
                            }
                            Task.WaitAll(TaskList.ToArray());
                            logger.Error("Start Sign Xml", new Exception(invoiceModels.Count().ToString()));
                            var signXml = Task.Run(() => this.invoiceBO.SignXmls(invoiceModels));
                            var totalPdf = invoiceExportPdfs.Count();
                            logger.Error("start task pdf", new Exception(totalPdf.ToString()));
                            var skippdf = 0;
                            var takepdf = (totalAll / thread) + 1;
                            List<Task> TaskListPdf = new List<Task>();
                            for (int i = 1; i <= thread; i++)
                            {
                                //task
                                var invoices = new List<InvoiceExportModel>(invoiceExportPdfs.Skip(skippdf).Take(takepdf));
                                //logger.Error("Start task pdf " + i, new Exception(invoices.Count().ToString()));
                                var taskpdf = Task.Run(() => this.invoiceBO.ProcessPdfs(invoices));

                                skippdf = takepdf * i;
                                TaskListPdf.Add(taskpdf);
                            }

                            Task.WaitAll(TaskListPdf.ToArray());
                            Task.WaitAll(signXml);
                            //logger.Error("signXml and wait all pdf", new Exception("15 task"));
                            this.invoiceBO.UpdateStatusAfterSign(invoiceModels);
                            logger.Error("Sign succcess Company: " + branchCurrent.COMPANYSID, new Exception("Sign success"));
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Sign error for company: " + branchCurrent.COMPANYSID, ex);
                        }

                    }
                }
                catch (Exception ex)
                {
                    logger.Error("branchCurrent: " + branchCurrent.COMPANYSID, ex);
                }
                
            }
        }


        public void SignFilePersonal()
        {
            var companys = this.noticeUseInvoiceBO.GetListCompanyNeedCreateFiles();
            logger.Error("SignFilePersonal listBranch", new Exception(companys.Count().ToString()));
            foreach (var branchCurrent in companys)
            {
                try
                {
                    //var checkInvoiceNo = this.invoiceBO.checkBeforeSign(branchCurrent.CompanyId ?? 0, branchCurrent.Symbol.Substring(branchCurrent.Symbol.Length - 2, 2));
                    //if (!checkInvoiceNo)
                    //{
                    //    logger.Error("InvoiceNo invalid: " + branchCurrent.CompanyId + ", " + branchCurrent.Symbol, new Exception("SignFileJob"));
                    //    continue;
                    //}
                    var mycompany = this.myCompanyBO.GetCompanyById(branchCurrent.COMPANYSID);
                    //var total = this.invoiceBO.GetAllInvoiceApprovedNotCreateFileShinhan(branchCurrent.COMPANYSID, true).Count();
                    logger.Error("SignFilePersonal start get all: " + branchCurrent.COMPANYSID, new Exception(branchCurrent.COMPANYSID.ToString()));
                    var listAll = this.invoiceBO.GetInvoicePrintPersonal(branchCurrent.COMPANYSID);
                    logger.Error("SignFilePersonal done get all: " + branchCurrent.COMPANYSID, new Exception(branchCurrent.COMPANYSID.ToString()));
                    var template = this.invoiceBO.GetTemplateNoTracking(1);
                    var totalAll = listAll.Count();
                    if (totalAll == 0)
                    {
                        logger.Error("SignFilePersonal Invoice not found for sign:  " + branchCurrent.COMPANYSID, new Exception("job sign"));
                        continue;
                    }
                    int takeList = 5000;
                    int skipList = 0;
                    int loop = (totalAll / takeList) + 1;
                    logger.Error("SignFilePersonal total: " + totalAll + ", loop " + loop + " lan", new Exception("totalAll"));
                    for (int index = 1; index <= loop; index++)
                    {
                        try
                        {
                            List<InvoicePrintModel> invoiceModels = new List<InvoicePrintModel>();
                            List<InvoiceExportModel> invoiceExportPdfs = new List<InvoiceExportModel>();
                            var listProcess = new List<InvoicePrintModel>(listAll.Skip(skipList).Take(takeList));
                            skipList = takeList * index;
                            int total = listProcess.Count();
                            int thread = 15;
                            var skip = 0;
                            var take = (total / thread) + 1;
                            List<Task> TaskList = new List<Task>();
                            for (int i = 1; i <= thread; i++)
                            {
                                //task
                                var invoices = new List<InvoicePrintModel>(listProcess.Skip(skip).Take(take));
                                logger.Error("SignFilePersonal Start get data task " + i, new Exception("printInvoice"));
                                var printInvoice = this.invoiceBO.ProcessInvoicePrint(invoices, mycompany, template);
                                logger.Error("SignFilePersonal Start task xml " + i, new Exception(printInvoice.invoiceExportXmlModels.Count().ToString()));
                                var task = Task.Run(() => this.invoiceBO.ProcessXmls(printInvoice.invoiceExportXmlModels));
                                logger.Error("SignFilePersonal End task xml " + i, new Exception(printInvoice.invoiceExportXmlModels.Count().ToString()));
                                invoiceModels.AddRange(invoices);
                                invoiceExportPdfs.AddRange(printInvoice.invoiceExportPdfModels);
                                skip = take * i;
                                TaskList.Add(task);
                            }
                            Task.WaitAll(TaskList.ToArray());
                            logger.Error("SignFilePersonal Start Sign Xml", new Exception(invoiceModels.Count().ToString()));
                            var signXml = Task.Run(() => this.invoiceBO.SignXmls(invoiceModels, true));
                            var totalPdf = invoiceExportPdfs.Count();
                            logger.Error("SignFilePersonal start task pdf", new Exception(totalPdf.ToString()));
                            var skippdf = 0;
                            var takepdf = (totalAll / thread) + 1;
                            List<Task> TaskListPdf = new List<Task>();
                            for (int i = 1; i <= thread; i++)
                            {
                                //task
                                var invoices = new List<InvoiceExportModel>(invoiceExportPdfs.Skip(skippdf).Take(takepdf));
                                //logger.Error("Start task pdf " + i, new Exception(invoices.Count().ToString()));
                                var taskpdf = Task.Run(() => this.invoiceBO.ProcessPdfs(invoices));

                                skippdf = takepdf * i;
                                TaskListPdf.Add(taskpdf);
                            }

                            Task.WaitAll(TaskListPdf.ToArray());
                            Task.WaitAll(signXml);
                            //logger.Error("signXml and wait all pdf", new Exception("15 task"));
                            this.invoiceBO.UpdateStatusAfterSign(invoiceModels);
                            logger.Error("SignFilePersonal Sign succcess Company: " + branchCurrent.COMPANYSID, new Exception("Sign success"));
                        }
                        catch (Exception ex)
                        {
                            logger.Error("SignFilePersonal Sign error for company: " + branchCurrent.COMPANYSID, ex);
                        }

                    }
                }
                catch (Exception ex)
                {
                    logger.Error("SignFilePersonal branchCurrent: " + branchCurrent.COMPANYSID, ex);
                }

            }
        }

        #region Code cũ
        private void SignOrg(List<MYCOMPANY> companys)
        {
            foreach (var branchCurrent in companys)
            {
                logger.Error("companyId : ", new Exception(branchCurrent.COMPANYSID.ToString()));

                SIGNATURE signature = this.signatureBO.GetByCompany(branchCurrent.COMPANYSID);

                if (branchCurrent.LEVELCUSTOMER == "PGD")
                {
                    signature = this.signatureBO.GetByCompany((long)branchCurrent.COMPANYID);
                }

                if (signature == null)
                {
                    continue; // next branch
                }

                var take = 300;

                //var invoicesApprove = this.invoiceBO.GetAllInvoiceApprovedShinhan(branchCurrent.COMPANYSID, true);

                var InvoiceCreatedFile = this.invoiceBO.GetListInvoiceCreatedFileShinhan(branchCurrent.COMPANYSID, true).OrderBy(p => p.ID).ToList();

                logger.Error("SignOrg || InvoiceCreatedFile : ", new Exception(InvoiceCreatedFile.Count().ToString()));

                if (InvoiceCreatedFile.Count() == 0)
                {
                    continue; // next branch
                }

                var count = (InvoiceCreatedFile.Count() / take) + 1;

                for (int i = 0; i < count; i++)
                {
                    var listInvoice = new List<INVOICE>(InvoiceCreatedFile.Skip(i * take).Take(take).ToList());
                    logger.Error("listInvoiceStartSign", new Exception(listInvoice.Count().ToString()));
                    if (listInvoice.Count() > 0)
                    {
                        this.releaseInvoiceBO.JobSignShinhan(listInvoice, signature, branchCurrent.COMPANYNAME);
                    }

                }

            }
        }

        private void SignPersonal(List<MYCOMPANY> companys)
        {
            var take = 300;
            foreach (var branchCurrent in companys)
            {

                logger.Trace("Curent Company: " + branchCurrent.COMPANYSID.ToString());
                SIGNATURE signature = this.signatureBO.GetByCompany(branchCurrent.COMPANYSID);

                if (branchCurrent.LEVELCUSTOMER == "PGD")
                {
                    signature = this.signatureBO.GetByCompany((long)branchCurrent.COMPANYID);
                }

                if (signature == null)
                {
                    continue;
                }

                var invoiceCreatedFile = this.invoiceBO.GetListInvoiceCreatedFileShinhan(branchCurrent.COMPANYSID, false).OrderBy(p => p.ID).ToList();

                //logger.Trace("Count: " + invoiceCreatedFile.Count().ToString());

                if (invoiceCreatedFile.Count() == 0)
                {
                    continue; //next branch
                }
                var count = (invoiceCreatedFile.Count() / take) + 1;

                for (int i = 0; i < count; i++)
                {
                    //logger.Trace("For " + i.ToString() + " : skip: " + (i * take).ToString() + " Take: " +take);
                    var listInvoice = new List<INVOICE>(invoiceCreatedFile.Skip(i * take).Take(take).ToList());
                    //logger.Trace("For " + i.ToString()+ " Count: " + listInvoice.Count().ToString());
                    if (listInvoice.Count() > 0)
                    {
                        this.releaseInvoiceBO.JobSignShinhan(listInvoice, signature, branchCurrent.COMPANYNAME);
                    }
                }

            }
        }
        #endregion

        #region Code A Long
        private void SignOrg(List<MYCOMPANY> companys, List<SIGNATURE> objSignatures)
        {         
           
            foreach (var branchCurrent in companys)
            {
                logger.Error("companyId : ", new Exception(branchCurrent.COMPANYSID.ToString()));

                SIGNATURE signature = objSignatures.FirstOrDefault(f => f.COMPANYID == branchCurrent.COMPANYSID);
                if (signature == null)
                {
                    logger.Trace(string.Format("SignPersonal - không tồn tại chữ ký : COMPANYSID :{0} ", branchCurrent.COMPANYSID));
                    continue; // next branch
                }

                var take = 300;
                var objInvoicesCreatedFiles = this.invoiceBO.GetListInvoiceCreatedFileShinhan(branchCurrent.COMPANYSID, true);

                // Không tồn tại một hóa đơn đã tạo file thì qua branch khác
                if (!objInvoicesCreatedFiles.Any())
                {
                    logger.Trace(string.Format("SignPersonal - không tồn tại hóa đơn đã tạo file : COMPANYSID :{0} ", branchCurrent.COMPANYSID));
                    continue; // next branch
                }

                // duyệt cho tới khi hết
                while (true == true) {
                    var listInvoice =objInvoicesCreatedFiles.OrderBy(f=>f.ID).Take(take).ToList();                   
                    if (listInvoice.Any())
                    {
                        logger.Error("listInvoiceStartSign", new Exception(listInvoice.Count.ToString()));
                        this.releaseInvoiceBO.JobSignShinhan(listInvoice, signature, branchCurrent.COMPANYNAME);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        private void SignPersonal(List<MYCOMPANY> companys, List<SIGNATURE> objSignatures)
        {
            var take = 300;
            foreach (var branchCurrent in companys)
            {

                logger.Trace("Curent Company: " + branchCurrent.COMPANYSID.ToString());
                SIGNATURE signature = objSignatures.FirstOrDefault(f => f.ID == branchCurrent.COMPANYSID);               

                if (signature == null)
                {   
                    logger.Trace( string.Format("SignPersonal - không tồn tại chữ ký : COMPANYSID :{0} ", branchCurrent.COMPANYSID));
                    continue;
                }

                // Lấy danh sách hóa đơn đã tạo files 
                var objInvoicesCreatedFiles = this.invoiceBO.GetListInvoiceCreatedFileShinhan(branchCurrent.COMPANYSID, false);               

                // Không tồn tại hóa đơn đã tạo files thì duyệt qua branch khác
                if (!objInvoicesCreatedFiles.Any())
                {
                    logger.Trace(string.Format("SignPersonal - không tồn tại hóa đơn đã tạo file : COMPANYSID :{0} ", branchCurrent.COMPANYSID));
                    continue; //next branch
                }

                // duyệt cho tới khi hết
                while (true == true)
                {
                    var listInvoice = objInvoicesCreatedFiles.OrderBy(f => f.ID).Take(take).ToList();
                    logger.Error("listInvoiceStartSign", new Exception(listInvoice.Count().ToString()));
                    if (listInvoice.Any())
                    {
                        this.releaseInvoiceBO.JobSignShinhan(listInvoice, signature, branchCurrent.COMPANYNAME);
                    }
                    else
                    {
                        break;
                    }
                }               

            }
        }

        #endregion
        private IEnumerable<ReleaseInvoiceInfo> getListInvoiceApproved(long Branch)
        {
            return this.invoiceBO.GetListInvoiceApproved(Branch);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetListInvoiceNo(List<ReleaseInvoiceInfo>)'
        public IEnumerable<ReleaseInvoiceInfo> GetListInvoiceNo(List<ReleaseInvoiceInfo> releaseInvoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetListInvoiceNo(List<ReleaseInvoiceInfo>)'
        {
            return this.invoiceBO.GetListInvoiceNo(releaseInvoiceInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.FillterCreated(ConditionSearchInvoice, int?)'
        public IEnumerable<InvoiceMaster> FillterCreated(ConditionSearchInvoice condition, int? flgCreate = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.FillterCreated(ConditionSearchInvoice, int?)'
        {
            condition.TotalRecords = this.invoiceBO.CountCreated(condition, flgCreate);
            return this.invoiceBO.FilterCreated(condition, flgCreate);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.FillterInvoiceConverted(ConditionSearchInvoice)'
        public IEnumerable<InvoiceMaster> FillterInvoiceConverted(ConditionSearchInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.FillterInvoiceConverted(ConditionSearchInvoice)'
        {
            condition.TotalRecords = this.invoiceBO.CountInvoiceConverted(condition);
            return this.invoiceBO.FillterInvoiceConverted(condition);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.FilterInvoiceSymbol(long)'
        public IEnumerable<string> FilterInvoiceSymbol(long invoiceTemplateId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.FilterInvoiceSymbol(long)'
        {
            var companyId = GetCompanyIdOfUser();
            return this.noticeUseInvoiceDetailBO.FilterInvoiceSymbol(companyId, invoiceTemplateId, true);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetInvoiceSample()'
        public IEnumerable<RegisterTemplateMaster> GetInvoiceSample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetInvoiceSample()'
        {
            long companyId = GetCompanyIdOfUser();
            return registerTemplateBO.GetList(companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetInvoicenfo(long)'
        public InvoiceInfo GetInvoicenfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetInvoicenfo(long)'
        {
            long companyId = GetCompanyIdOfUser();
            return this.invoiceBO.GetInvoiceInfo(id, companyId);

        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetInvoiceData(long)'
        public InvoicePrintInfo GetInvoiceData(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetInvoiceData(long)'
        {
            return this.invoiceBO.GetInvoiceData(id);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetListStatisticals(long)'
        public List<InvoiceStatistical> GetListStatisticals(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetListStatisticals(long)'
        {
            return this.invoiceBO.GetListStatisticals(id);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetListGift(long)'
        public List<InvoiceGift> GetListGift(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetListGift(long)'
        {
            return this.invoiceBO.GetListGift(id);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.Verify(string)'
        public bool Verify(string file)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.Verify(string)'
        {
            return this.invoiceBO.Verify(file);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetFileRecipt(long)'
        public ExportFileInfo GetFileRecipt(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetFileRecipt(long)'
        {
            InvoiceInfo info = this.GetInvoicenfo(id);
            string fileName = info.FileReceipt;
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FullPathFileName = Path.Combine(printConfig.PathInvoiceFile + "\\FileRecipt") + "\\" + fileName;
            fileInfo.FileName = fileName;
            return fileInfo;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetInvoiceOfClient(long)'
        public InvoiceClientSign GetInvoiceOfClient(long invoiceId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetInvoiceOfClient(long)'
        {
            return this.invoiceBO.GetInvoiceOfClient(invoiceId);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.Create(InvoiceInfo)'
        public ResultCode Create(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.Create(InvoiceInfo)'
        {
            this.ValidateUpdateCreateUpdate(invoiceInfo);

            invoiceInfo.CompanyId = this.CurrentUser.Company.Id;
            invoiceInfo.CompanyNameInvoice = this.CurrentUser.Company.CompanyName;
            invoiceInfo.CompanyAddress = this.CurrentUser.Company.Address;
            invoiceInfo.CompanyTaxCode = this.CurrentUser.Company.TaxCode;
            invoiceInfo.UserAction = this.CurrentUser.Id;
            return this.invoiceBO.Create(invoiceInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.Update(long, InvoiceInfo)'
        public ResultCode Update(long id, InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.Update(long, InvoiceInfo)'
        {
            this.ValidateUpdateCreateUpdate(invoiceInfo);

            long idCompany = GetCompanyIdOfUser();
            if (!invoiceInfo.CompanyId.HasValue || invoiceInfo.CompanyId == 0)
            {
                invoiceInfo.CompanyId = idCompany;
            }
            InvoiceInfo invoice = GetInvoicenfo(id);
            if (invoiceInfo.FileReceipt != invoice.FileReceipt)
            {
                DeleteInvoiceFile(invoiceInfo.Id);
                invoiceInfo.FileReceipt = SaveFileImport(invoiceInfo);
            }

            return this.invoiceBO.Update(id, idCompany, invoiceInfo);
        }
        private string SaveFileImport(InvoiceInfo fileImport)
        {
            string FolderPath = Config.ApplicationSetting.Instance.FolderInvoiceFile + "/FileRecipt";
            string fileName = string.Empty;
            string filePathFileName = string.Empty;
            string fullPathFile = this.printConfig.FullPathFileAsset;
            if (fileImport.InvoiceType == (int)InvoiceType.Substitute)
            {
                ANNOUNCEMENT currentAnnouncement = invoiceBO.GetAnnouncement(Convert.ToInt32(fileImport.ParentId), 2, fileImport.CompanyId);
                filePathFileName = string.Format("{0}\\{1}\\Releases\\SignAnnouncement\\{2}_sign.pdf", fullPathFile, currentAnnouncement.COMPANYID, currentAnnouncement.ID);
                fileName = string.Format(@"{0}\BienBanThayTheHoaDon_{1}.pdf", FolderPath, fileImport.Id);
            }
            else
            {
                ANNOUNCEMENT currentAnnouncement = invoiceBO.GetAnnouncement(Convert.ToInt32(fileImport.ParentId), 1, fileImport.CompanyId);
                filePathFileName = string.Format("{0}\\{1}\\Releases\\SignAnnouncement\\{2}_sign.pdf", fullPathFile, currentAnnouncement.COMPANYID, currentAnnouncement.ID);
                fileName = string.Format(@"{0}\BienBanDieuChinhHoaDon_{1}.pdf", FolderPath, fileImport.Id);
            }
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetAnnouncement(long, int, long?)'
        public Data.DBAccessor.ANNOUNCEMENT GetAnnouncement(long id, int announcementType, long? companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetAnnouncement(long, int, long?)'
        {
            Data.DBAccessor.ANNOUNCEMENT announcements = this.invoiceBO.GetAnnouncement(id, announcementType, companyId);
            if (announcements == null)
            {
                throw new BusinessLogicException(ResultCode.AnnouncementNotExist, string.Format("The announcement does not exist for this invoice [{0}]", id));
            }

            return announcements;
        }

        private void DeleteInvoiceFile(long invoiceId)
        {
            ExportFileInfo fileInfo = GetFileInvoiceSign(invoiceId);
            if (File.Exists(fileInfo.FullPathFileName))
            {
                File.Delete(fileInfo.FullPathFileName);
            }
        }

        private ExportFileInfo GetFileInvoiceSign(long invoiceId)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            InvoiceInfo invoice = GetInvoicenfo(invoiceId);
            string folderInvoiceOfCompany = Path.Combine(printConfig.PathInvoiceFile + "\\FileRecipt");
            string fileNameInvoice = invoice.FileReceipt ?? string.Empty;
            fileInfo.FullPathFileName = Path.Combine(folderInvoiceOfCompany, fileNameInvoice);
            return fileInfo;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.UpdateStatus(long, InvoiceReleasesStatus)'
        public ResultCode UpdateStatus(long id, InvoiceReleasesStatus invoiceStatus)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.UpdateStatus(long, InvoiceReleasesStatus)'
        {
            if (invoiceStatus == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            if (!Enum.IsDefined(typeof(RecordStatus), invoiceStatus.Status))
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long idCompany = GetCompanyIdOfUser();
            return this.invoiceBO.UpdateStatus(id, idCompany, invoiceStatus.Status);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.UpdateNote(long, InvoiceNote)'
        public ResultCode UpdateNote(long id, InvoiceNote note)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.UpdateNote(long, InvoiceNote)'
        {
            note.CompanyId = GetCompanyIdOfUser();
            return this.invoiceBO.UpdateNote(id, note);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.PrintView(long)'
        public ExportFileInfo PrintView(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.PrintView(long)'
        {
            ExportFileInfo fileInfo = this.invoiceBO.PrintView(id);
            return fileInfo;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetXmlFile(long)'
        public ExportFileInfo GetXmlFile(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetXmlFile(long)'
        {
            ExportFileInfo fileInfo = this.invoiceBO.GetXmlFile(id);
            return fileInfo;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.DownloadStatistical(long)'
        public ExportFileInfo DownloadStatistical(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.DownloadStatistical(long)'
        {
            ExportFileInfo fileInfo = this.invoiceBO.DownloadStatistical(id);
            return fileInfo;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.DownloadGift(long)'
        public ExportFileInfo DownloadGift(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.DownloadGift(long)'
        {
            ExportFileInfo fileInfo = this.invoiceBO.DownloadGift(id);
            return fileInfo;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.SwitchInvoice(long)'
        public ExportFileInfo SwitchInvoice(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.SwitchInvoice(long)'
        {
            ExportFileInfo fileInfo = this.invoiceBO.SwitchInvoice(id);
            return fileInfo;
        }

        public ResultCode Delete(long id,long companyId)
        {
            return this.invoiceBO.Delete(id, companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.RegistInvoiceNo(long, string)'
        public InvoiceNo RegistInvoiceNo(long tempalateId, string symbol)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.RegistInvoiceNo(long, string)'
        {
            long companyId = GetCompanyIdOfUser();
            return this.invoiceBO.RegistInvoiceNo(companyId, tempalateId, symbol);
        }


        private PrintConfig GetPrintConfig()
        {
            string fullPathFileAsset = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            string fullPathFileExportData = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.FolderExportDataOfCompany);
            PrintConfig config = new PrintConfig(fullPathFileAsset, fullPathFileInvoice, fullPathFileExportData);

            try
            {
                if (this.CurrentUser.Company != null)
                {
                    config.BuildAssetByCompany(this.CurrentUser.Company);
                }
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
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
        private PrintConfig GetPrintConfigbyJob()
        {
            PrintConfig config = new PrintConfig(null, null);
            config.FullPathFolderTemplateInvoice = null;
            return config;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ImportData(UploadInvoice)'
        public ResultImportSheet ImportData(UploadInvoice fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ImportData(UploadInvoice)'
        {
            
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }
            string fullPathfile = SaveFileImport(fileImport);
            long companyId = GetCompanyIdOfUser();
            var task = Task.Run(() => this.invoiceBO.ImportData(fileImport, fullPathfile, companyId));
            task.Wait(5000);
            var listResultImport = new ResultImportSheet();
            if (task.Status == TaskStatus.Running)
            {
                listResultImport.ErrorCode = ResultCode.ImportInvoiceFileRunning;
                listResultImport.Message = "Import file running";
                return listResultImport;
            }
            return task.Result;
            //return this.invoiceBO.ImportData(fileImport, fullPathfile, companyId);
        }

        public ResultImportSheet ImportDataInvoiceReplace(UploadInvoice fileImport)
        {
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }
            string fullPathfile = SaveFileImport(fileImport);
            long companyId = GetCompanyIdOfUser();
            //var task = Task.Run(() => );
            //task.Wait(5000);
            var listResultImport = new ResultImportSheet();
            //if (task.Status == TaskStatus.Running)
            //{
            //    listResultImport.ErrorCode = ResultCode.ImportInvoiceFileRunning;
            //    listResultImport.Message = "Import file running";
            //    return listResultImport;
            //}
            //return task.Result;

            ResultImportSheet checkImport = this.invoiceBO.ImportDataInvoiceReplace(fileImport, fullPathfile, companyId);
            if(checkImport.RowError.Count() > 0)
            {
                listResultImport.ErrorCode = ResultCode.ImportInvoiceReplaceFileError;
                listResultImport.Message = "Create Invoice Replace Failed";
            }
            else
            {
                listResultImport.ErrorCode = ResultCode.ImportInvoiceReplaceFileSuccess;
                listResultImport.Message = "Create Invoice Replace Success";
            }

            return listResultImport;
            //return this.invoiceBO.ImportData(fileImport, fullPathfile, companyId);
        }

        public ResultImportSheet ImportDataInvoiceAdjustForInvAdjusted(UploadInvoice fileImport)
        {
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }
            string fullPathfile = SaveFileImport(fileImport);
            long companyId = GetCompanyIdOfUser();
            //var task = Task.Run(() => );
            //task.Wait(5000);
            var listResultImport = new ResultImportSheet();
            //if (task.Status == TaskStatus.Running)
            //{
            //    listResultImport.ErrorCode = ResultCode.ImportInvoiceFileRunning;
            //    listResultImport.Message = "Import file running";
            //    return listResultImport;
            //}
            //return task.Result;

            ResultImportSheet checkImport = this.invoiceBO.ImportDataInvoiceAdjusted(fileImport, fullPathfile, companyId);
            if (checkImport.RowError.Count() > 0)
            {
                listResultImport.ErrorCode = ResultCode.ImportInvoiceReplaceFileError;
                listResultImport.Message = "Create Invoice Replace Failed";
            }
            else
            {
                listResultImport.ErrorCode = ResultCode.ImportInvoiceReplaceFileSuccess;
                listResultImport.Message = "Create Invoice Replace Success";
            }

            return listResultImport;
            //return this.invoiceBO.ImportData(fileImport, fullPathfile, companyId);
        }

        private static string SaveFileImport(UploadInvoice fileImport)
        {
            string FolderPath = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.UploadFolderImport);
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string fileName = string.Format(@"{0}\Invoice{1}.xlsx", FolderPath, DateTime.Now.ToString("ddMMyyyy"));
            try
            {
                File.WriteAllBytes(fileName, Convert.FromBase64String(fileImport.Data));
            }
            catch (Exception ex)
            {

                throw new BusinessLogicException(ResultCode.FileNotFound, ex.Message);
            }

            return fileName;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ReadFileGift(UploadInvoice)'
        public List<InvoiceGift> ReadFileGift(UploadInvoice fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ReadFileGift(UploadInvoice)'
        {
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }
            string fullPathfile = SaveFileGift(fileImport);
            var result = this.invoiceBO.ReadGiftFile(fullPathfile);
            if (File.Exists(fullPathfile))
            {
                File.Delete(fullPathfile);
            }
            return result;
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ReadDetailFile(UploadInvoice)'
        public List<InvoiceDetailInfo> ReadDetailFile(UploadInvoice fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ReadDetailFile(UploadInvoice)'
        {
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }
            string fullPathfile = SaveFileGift(fileImport);
            var result = this.invoiceBO.ReadDetailFile(fullPathfile);
            if (File.Exists(fullPathfile))
            {
                File.Delete(fullPathfile);
            }
            return result;
        }

        private static string SaveFileGift(UploadInvoice fileImport)
        {
            string FolderPath = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.UploadFolderImport);
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string fileName = string.Format(@"{0}\Gift{1}.xlsx", FolderPath, DateTime.Now.ToString("ddMMyyyyHHmmSSffff"));
            try
            {
                File.WriteAllBytes(fileName, Convert.FromBase64String(fileImport.Data));
            }
            catch (Exception ex)
            {

                throw new BusinessLogicException(ResultCode.FileNotFound, ex.Message);
            }

            return fileName;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.DownloadInvoiceXml(long)'
        public ExportFileInfo DownloadInvoiceXml(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.DownloadInvoiceXml(long)'
        {
            return this.invoiceBO.DownloadInvoiceXML(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetInvoiceReleaseInfo(long)'
        public InvoiceReleaseInfo GetInvoiceReleaseInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetInvoiceReleaseInfo(long)'
        {
            return this.invoiceBO.GetInvoiceReleaseInfo(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.PTGetInvoiceReleaseInfo(long)'
        public InvoiceReleaseInfo PTGetInvoiceReleaseInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.PTGetInvoiceReleaseInfo(long)'
        {
            return this.invoiceBO.GetInvoiceReleaseInfo(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.CheckPermission(ApprovedInvoice)'
        public Boolean CheckPermission(ApprovedInvoice approveInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.CheckPermission(ApprovedInvoice)'
        {
            if (approveInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            approveInfo.CompanyId = GetCompanyIdOfUser();
            approveInfo.UserActionId = this.CurrentUser.Id;
            return this.invoiceBO.CheckPermission(approveInfo, this.CurrentUser);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ApproveInvoice(ApprovedInvoice)'
        public ResultApproved ApproveInvoice(ApprovedInvoice approveInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ApproveInvoice(ApprovedInvoice)'
        {
            if (approveInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            approveInfo.CompanyId = HttpContext.Current != null ? GetCompanyIdOfUser() : approveInfo.CompanyId;
            approveInfo.UserActionId = HttpContext.Current != null ? this.CurrentUser.Id : approveInfo.UserActionId;

            if (!this.CurrentUser.UserId.Equals("quarzt"))
            {
                return this.invoiceBO.ApproveInvoice(approveInfo, this.CurrentUser);
            }
            else
            {
                var keyOfPackage = Config.ApplicationSetting.Instance.NumOfPackage;
                UserSessionInfo user = this.sessionManagerBO.JobLogin(keyOfPackage);
                return this.invoiceBO.JobApproveInvoice(approveInfo, user);
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.SendVerificationCode(InvoiceVerificationCode)'
        public ResultCode SendVerificationCode(InvoiceVerificationCode invoiceVerificationCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.SendVerificationCode(InvoiceVerificationCode)'
        {
            if (invoiceVerificationCode == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            invoiceVerificationCode.NumberFormat = this.CurrentUser.Company.NumberFormat.IsNullOrEmpty() ? "{0:#,##0.###}" : this.CurrentUser.Company.NumberFormat;
            invoiceVerificationCode.CompanyId = GetCompanyIdOfUser();
            long emailNoticeId = 0;
            SendEmailVerificationCode emailVerificationCodeInfo = this.invoiceBO.GetEmailVerificationCodeInfo(invoiceVerificationCode);
            long companyId = GetCompanyIdOfUser();
            emailNoticeId = this.invoiceBO.CreateEmailActive(companyId, emailVerificationCodeInfo);
            return this.emailActiveBO.SendEmailDaily(emailNoticeId, emailVerificationCodeInfo);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.DataInvoiceCanUse(InvoiceDateInfo, bool)'
        public ResultValidDate DataInvoiceCanUse(InvoiceDateInfo invoiceDateInfo, bool isNew = true)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.DataInvoiceCanUse(InvoiceDateInfo, bool)'
        {
            if (invoiceDateInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.invoiceBO.DateInvoiceCanUse(invoiceDateInfo, isNew);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ResearchInvoice(string)'
        public ExportFileInfo ResearchInvoice(string verificationCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ResearchInvoice(string)'
        {
            ExportFileInfo fileInfo = this.invoiceBO.ResearchInvoice(verificationCode);
            return fileInfo;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.FillterInvoiceOfClient(out long, string, string, int, int)'
        public IEnumerable<InvoiceMaster> FillterInvoiceOfClient(out long totalRecords, string dateFrom = null, string dateTo = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.FillterInvoiceOfClient(out long, string, string, int, int)'
        {
            var condition = new ConditionSearchInvoiceOfClient(this.CurrentUser, dateFrom, dateTo);
            totalRecords = this.invoiceBO.CountInvoice(condition);
            return this.invoiceBO.FilterInvoice(condition, skip, take);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ExportInvoiceConverted(ConditionSearchInvoice)'
        public ExportFileInfo ExportInvoiceConverted(ConditionSearchInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ExportInvoiceConverted(ConditionSearchInvoice)'
        {
            ExportFileInfo fileInfo = this.invoiceBO.ExportInvoiceConverted(condition);
            return fileInfo;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.MaxReleaseDate(InvoiceInfo)'
        public InvoiceReleaseDate MaxReleaseDate(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.MaxReleaseDate(InvoiceInfo)'
        {
            if (invoiceInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            invoiceInfo.CompanyId = GetCompanyIdOfUser();
            return this.invoiceBO.MaxReleaseDate(invoiceInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.MinInvoiceNo(InvoiceInfo)'
        public InvoiceNo MinInvoiceNo(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.MinInvoiceNo(InvoiceInfo)'
        {
            if (invoiceInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            invoiceInfo.CompanyId = GetCompanyIdOfUser();
            return this.invoiceBO.MinInvoiceNo(invoiceInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.MaxInvoiceNo(InvoiceInfo)'
        public InvoiceNo MaxInvoiceNo(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.MaxInvoiceNo(InvoiceInfo)'
        {
            if (invoiceInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            invoiceInfo.CompanyId = GetCompanyIdOfUser();
            return this.invoiceBO.MaxInvoiceNo(invoiceInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.RevertApprovedInvoice(ApprovedInvoice)'
        public ResultApproved RevertApprovedInvoice(ApprovedInvoice approvedInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.RevertApprovedInvoice(ApprovedInvoice)'
        {
            if (approvedInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            approvedInfo.CompanyId = GetCompanyIdOfUser();
            approvedInfo.UserActionId = this.CurrentUser.Id;
            return this.invoiceBO.RevertApprovedInvoice(approvedInfo, this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.importInvoice(bool)'
        public ResultCode importInvoice(bool isJob)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.importInvoice(bool)'
        {
            this.invoiceBO.ImportInvoiceFile(isJob);
            return ResultCode.ImportInvoiceFileSuccess;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ValidateUpdateCreateUpdate(InvoiceInfo)'
        public void ValidateUpdateCreateUpdate(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ValidateUpdateCreateUpdate(InvoiceInfo)'
        {
            if (invoiceInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var invoiceTemplateInfo = this.invoiceTemplateBO.GetByRegisterTemplateId(invoiceInfo.RegisterTemplateId);
            if (invoiceTemplateInfo != null && ExportInvoiceFileName.IsTemplateShinhan(invoiceTemplateInfo.UrlFile))
            {
                //if (invoiceInfo.InvoiceGift.Count > 0)
                //{
                //    var totalAmountGift = invoiceInfo.GiftSumAmount + invoiceInfo.GiftSumTaxAmount;
                //    if (totalAmountGift != invoiceInfo.Sum)
                //    {
                //        throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
                //    }
                //}
                //if (invoiceInfo.InvoiceStatistical.Count > 0)
                //{
                //    var totalAmountStatistical = invoiceInfo.StatisticalSumAmount + invoiceInfo.StatisticalSumTaxAmount;
                //    if (totalAmountStatistical != invoiceInfo.Sum)
                //    {
                //        throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
                //    }
                //}
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.BIDCImportInvoiceExternalApi(BIDCImportFromAPI)'
        public BIDCImportFromAPIOutput BIDCImportInvoiceExternalApi(BIDCImportFromAPI bIDCImportFromAPI)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.BIDCImportInvoiceExternalApi(BIDCImportFromAPI)'
        {
            StringBuilder INSTR = new StringBuilder("<INSTR><TXNREFCODE>{TXNREFCODE}</TXNREFCODE><TYPE>{TYPE}</TYPE><DESCRIPTION>{DESCRIPTION}</DESCRIPTION><TRANSREF>{TRANSREF}</TRANSREF></INSTR>");
            INSTR.Replace("{TXNREFCODE}", $"EINVOICE{DateTime.Now.ToString("yyyyMMddHHmmssfff")}");
            INSTR.Replace("{TYPE}", "BIDC.GET.INVOICE");
            INSTR.Replace("{DESCRIPTION}", "Get Data Invoice");
            INSTR.Replace("{TRANSREF}", bIDCImportFromAPI.TransRef);

            string xmlReturn;
            string status = "";
            string statusCode = "";
            T24WebServices t24WebServices;
            try
            {
                t24WebServices = new T24WebServices();
                xmlReturn = t24WebServices.Get(INSTR.ToString());
            }
            catch (Exception ex)
            {
                BIDCImportInvoiceExternalApiSaveSystemLog(bIDCImportFromAPI.TransRef, SystemLog_LogType.Error, "Fail when init connection to service");
                throw new BusinessLogicException(ResultCode.ImportAPIInitConnectionError, "Fail when init connection to service", ex);
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlReturn);
            try
            {
                // Nếu get data từ service có lỗi thì quăng lỗi ra
                status = xmlDoc.GetElementsByTagName(BIDCApiTag.STATUS)[0].InnerText;
                var statuses = status.Split('#');
                statusCode = statuses[0].PadLeft(2, '0');
                if (BIDCImportInvoiceExternalApiStatus.ListErrorCode.ContainsKey(statusCode))
                {
                    var resError = new BIDCImportFromAPIOutput()
                    {
                        TXNREFCODE = xmlDoc.GetElementsByTagName(BIDCApiTag.TXNREFCODE)[0].InnerText,
                        STATUS = status,
                        StatusCode = statusCode,
                    };
                    BIDCImportInvoiceExternalApiSaveSystemLog(bIDCImportFromAPI.TransRef, SystemLog_LogType.Error, status);
                    return resError;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            var res = invoiceBO.BIDCImportInvoiceExternalApi(xmlDoc);
            res.TXNREFCODE = xmlDoc.GetElementsByTagName(BIDCApiTag.TXNREFCODE)[0].InnerText;
            res.STATUS = status;
            res.StatusCode = statusCode;
            BIDCImportInvoiceExternalApiSaveSystemLog(bIDCImportFromAPI.TransRef, SystemLog_LogType.Success);
            return res;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetByInvoiceNo(ConditionSearchInvoice)'
        public InvoiceInfo GetByInvoiceNo(ConditionSearchInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetByInvoiceNo(ConditionSearchInvoice)'
        {
            return new InvoiceInfo();
        }
        public ExportFileInfo ExportExcel(ConditionSearchInvoice condition)
        {
            ExportFileInfo fileInfo = this.invoiceBO.ExportExcel(condition);
            return fileInfo;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ExportExcelInvoices(ConditionSearchInvoice)'
        public ExportFileInfo ExportExcelInvoices(ConditionSearchInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.ExportExcelInvoices(ConditionSearchInvoice)'
        {
            ExportFileInfo fileInfo = this.invoiceBO.ExportExcelInvoices(condition);
            return fileInfo;
        }

        public ExportFileInfo ExportExcelCkInvoiceNumber(ConditionSearchCheckNumberInvoice conditionSrc)
        {
            ExportFileInfo fileInfo = this.invoiceBO.ExportExcelCkInvoiceNumber(conditionSrc);
            return fileInfo;
        }
        #endregion

        #region Private Methods
        private void BIDCImportInvoiceExternalApiSaveSystemLog(string TransRef, SystemLog_LogType logType, string StatusError = "")
        {
            SaveSystemLog saveSystemLog = new SaveSystemLog()
            {
                FunctionCode = "createInvoiceExternalAPI",
                LogSummary = "Menu [Danh sách hóa đơn]",
                LogType = (int)logType,
                LogDetail = $"Tạo hóa đơn từ API bên ngoài. TransRef: {TransRef}" + (!string.IsNullOrEmpty(StatusError) ? ". Status: " + StatusError : string.Empty),
            };
            this.systemLogBusiness.SaveAudit(saveSystemLog);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetConfigInfo()'
        public FooterConfig GetConfigInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceBusiness.GetConfigInfo()'
        {
            return this.invoiceBO.GetConfigInfo();
        }

        #endregion
    }
}