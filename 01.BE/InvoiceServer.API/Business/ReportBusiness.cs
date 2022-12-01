using InvoiceServer.Business.BL;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.ExportXml;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using InvoiceServer.GateWay.Models.MVan;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness'
    public class ReportBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness'
    {
        #region Fields, Properties
        string folderStore = System.Configuration.ConfigurationManager.AppSettings["FolderInvoiceFile"];

        private readonly IInvoiceBO invoiceBO;
        private readonly IReportSummaryBO reportSummaryBO;
        private readonly ITaxBO taxBO;
        private readonly IHistoryReportGeneralBO historyReportGeneralBO;
        private readonly ISignatureBO signatureBO;
        private readonly IQuarztJobBO quarztJobBO;
        private readonly INoticeUseInvoiceBO noticeUseInvoiceBO;
        private readonly Logger logger = new Logger();

        #endregion Fields, Properties

        #region Contructor

        public ReportBusiness(IBOFactory boFactory)
        {
            PrintConfig printConfig = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };

            this.reportSummaryBO = boFactory.GetBO<IReportSummaryBO>(printConfig);
            this.invoiceBO = boFactory.GetBO<IInvoiceBO>(printConfig, emailConfig);
            this.taxBO = boFactory.GetBO<ITaxBO>();
            this.historyReportGeneralBO = boFactory.GetBO<IHistoryReportGeneralBO>();
            this.quarztJobBO = boFactory.GetBO<IQuarztJobBO>();
            this.signatureBO = boFactory.GetBO<ISignatureBO>();
            this.noticeUseInvoiceBO = boFactory.GetBO<INoticeUseInvoiceBO>(printConfig, this.CurrentUser);

        }


        #endregion Contructor

        #region Methods

        public ExportFileInfo ReportInvoiceUse(int precious = 0, int year = 2017, int isMonth = 0, string branch = null, string language = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ReportInvoiceUse(int, int, int, string, string)'
        {
            var condition = new ConditionReportUse(this.CurrentUser, precious, year, isMonth, branch, language);
            ExportFileInfo fileInfo = this.invoiceBO.PrintViewSituationInvoiceUsage(condition, this.CurrentUser.Company);
            return fileInfo;
        }

        public ReportInvoiceUsing ReportInvoiceUseView(int precious = 0, int year = 2017, int isMonth = 0, string branch = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ReportInvoiceUseView(int, int, int, string)'
        {
            var condition = new ConditionReportUse(this.CurrentUser, precious, year, isMonth, branch);
            ReportInvoiceUsing situationInvoiceUsage = this.invoiceBO.ViewSituationInvoiceUsage(condition, this.CurrentUser.Company);
            return situationInvoiceUsage;
        }
        public ReportSituationInvoiceUsage ReportInvoiceUseViewSummary(int precious = 0, int year = 2017, int isMonth = 0, string branch = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ReportInvoiceUseViewSummary(int, int, int, string)'
        {
            var condition = new ConditionReportUse(this.CurrentUser, precious, year, isMonth, branch);
            ReportSituationInvoiceUsage situationInvoiceUsage = this.invoiceBO.ViewSituationInvoiceUsageSummary(condition, this.CurrentUser.Company);
            return situationInvoiceUsage;
        }
        public ReportSituationInvoiceUsage ReportInvoiceUseViewSixMonth(int precious = 0, int year = 2017, int isMonth = 0, string branch = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ReportInvoiceUseViewSixMonth(int, int, int, string)'
        {
            var condition = new ConditionReportUse(this.CurrentUser, precious, year, isMonth, branch);
            ReportSituationInvoiceUsage situationInvoiceUsage = this.invoiceBO.ViewSituationInvoiceUsageSixMonth(condition, this.CurrentUser.Company);
            return situationInvoiceUsage;
        }

        public int ReportInvoiceUsed(int precious = 0, int year = 2017, int isMonth = 0,long companyId = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ReportInvoiceUsed(int, int, int)'
        {
            var condition = new ConditionReportUse(precious, year, isMonth,null,companyId);
            int countInvoiceUsage = this.invoiceBO.GetInvoiceUsed(condition);
            return countInvoiceUsage;
        }

        public ReportInvoiceUsing ViewReportInvoiceUse(int precious = 0, int year = 2017)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ViewReportInvoiceUse(int, int)'
        {
            var condition = new ConditionReportUse(this.CurrentUser, precious, year);
            ReportInvoiceUsing reportSituationInvoice = this.invoiceBO.ViewSituationInvoiceUsage(condition, this.CurrentUser.Company);
            return reportSituationInvoice;
        }


        public ExportFileInfo ReportInvoiceUseXML(int precious = 0, int year = 2017, int isMonth = 0, string branch = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ReportInvoiceUseXML(int, int, int, string)'
        {
            var condition = new ConditionReportUse(this.CurrentUser, precious, year, isMonth, branch);
            ExportFileInfo fileInfo = this.invoiceBO.PrintViewSituationInvoiceUsageXML(condition, this.CurrentUser.Company);
            return fileInfo;
        }

        public ExportFileInfo ReportMonthlyBoardInvoice(int month = 1, int year = 12, int isMonth = 0, long? branch = null, string language = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ReportMonthlyBoardInvoice(int, int, int, long?, string)'
        {
            var condition = new ConditionInvoiceBoard(this.CurrentUser, null, month, year, isMonth, branch, language);
            ExportFileInfo fileInfo = this.reportSummaryBO.PrintViewMonthlyInvoiceBoard(condition, this.CurrentUser.Company);
            return fileInfo;
        }

        public ReporMonthlyBoard ReportMonthlyBoardInvoiceView(int month = 1, int year = 12, int isMonth = 0, long? branch = null, int skip = 0, int take = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ReportMonthlyBoardInvoiceView(int, int, int, long?, int, int)'
        {
            var condition = new ConditionInvoiceBoard(this.CurrentUser, null, month, year, isMonth, branch);
            ReporMonthlyBoard reporMonthlyBoard = this.reportSummaryBO.PrintViewMonthlyInvoiceBoardView(condition, this.CurrentUser.Company, skip, take);
            return reporMonthlyBoard;
        }

        public ExportFileInfo ReportMonthlyBoardInvoiceXml(int month = 1, int year = 12, int isMonth = 0, long? branch = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ReportMonthlyBoardInvoiceXml(int, int, int, long?)'
        {
            var condition = new ConditionInvoiceBoard(this.CurrentUser, null, month, year, isMonth, branch);
            ExportFileInfo fileInfo = this.reportSummaryBO.PrintViewMonthlyInvoiceBoardXML(condition, this.CurrentUser.Company);
            return fileInfo;
        }

        public IEnumerable<ReportInvoiceDetail> FillterListInvoice(ConditionReportDetailUse condition, bool isGroupBy = true)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.FillterListInvoice(ConditionReportDetailUse, bool)'
        {
            IEnumerable<ReportInvoiceDetail> listInvoice;
            if (isGroupBy)
            {
                condition.TotalRecords = this.invoiceBO.CountFillterListInvoiceGroupBy(condition);
                listInvoice = this.invoiceBO.FillterListInvoiceGroupBy(condition);
            }
            else
            {
                condition.TotalRecords = this.invoiceBO.CountFillterListInvoice(condition);
                listInvoice = this.invoiceBO.FillterListInvoice(condition);
                foreach (var item in listInvoice)
                {
                    //if (item.IsMultiTax != true)
                    //{
                    //    var taxInvoice = this.taxBO.GetById(item.TaxId);
                    //    item.Tax = (decimal)taxInvoice.Code;
                    //    item.AmountTax = (item.Price * (decimal)taxInvoice.Code) / 100;
                    //}
                    if (item.CustomerName == "")
                    {
                        item.CustomerName = item.PersonContact;
                    }
                }
            }
            return listInvoice;
        }

        public ExportFileInfo DownloadReportCombine(ConditionReportDetailUse condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.DownloadReportCombine(ConditionReportDetailUse)'
        {
            ExportFileInfo fileInfo = this.invoiceBO.DownloadReportCombine(condition);
            return fileInfo;
        }

        public ExportFileInfo DownloadReportInvoiceDetail(ConditionReportDetailUse condition, bool type = true)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.DownloadReportInvoiceDetail(ConditionReportDetailUse, bool)'
        {
            ExportFileInfo fileInfo;
            if (type)
            {
                fileInfo = this.invoiceBO.DownloadReportInvoiceDetailGroupBy(condition);
            }
            else
            {
                fileInfo = this.invoiceBO.DownloadReportInvoiceDetail(condition);
            }

            return fileInfo;
        }
        public ExportFileInfo DownloadReportInvoiceDataSummary(ConditionReportDetailUse condition, bool type = true)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.DownloadReportInvoiceDataSummary(ConditionReportDetailUse, bool)'
        {
            ExportFileInfo fileInfo;
            fileInfo = this.invoiceBO.DownloadReportInvoiceDataSummary(condition);
            return fileInfo;
        }

        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

        public ReportGeneralInvoice ReportGeneralInvoice(ConditionReportDetailUse condition)
        {
            ReportGeneralInvoice reportGeneralInvoice = this.invoiceBO.ViewGeneralInvoiceUsage(condition);
            return reportGeneralInvoice;
        }
        public ExportFileInfo ReportGeneralInvoiceXml(ConditionReportDetailUse condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ReportGeneralInvoiceXml(ConditionReportDetailUse)'
        {
            //SIGNATURE signature = this.signatureBO.GetByCompany((long)condition.Branch);
            ExportFileInfo reportGeneralInvoice = this.invoiceBO.ReportInvoiceDataSummaryXML(condition);
            return reportGeneralInvoice;
        }
        public ReportHistoryXml SendMQTXml(ConditionReportDetailUse condition)
        {
            var history = this.historyReportGeneralBO.CreateHistoryGeneral(condition,null);

            SIGNATURE signature = this.signatureBO.GetByCompany((long)condition.CompanyId);

            if (signature == null)
            {
                throw new BusinessLogicException(ResultCode.CaNotExisted, "Vui lòng upload file CA cho chi nhánh đang sử dụng");
            }

            ReportHistoryXml reportGeneralInvoice = this.invoiceBO.SendMQTXml(condition, history, signature);


            reportGeneralInvoice.CompanyId = condition.CurrentUser.Company.Id;
            this.historyReportGeneralBO.updateFileName(reportGeneralInvoice);
            return reportGeneralInvoice;
        }

        public List<ReportHistoryXml> SendMQTXml_1(ConditionReportDetailUse condition)
        {
            //var history = this.historyReportGeneralBO.CreateHistoryGeneral(condition);
            List<ReportHistoryXml> lstreportGeneralInvoice = new List<ReportHistoryXml>();
            //try
            //{

            //}
            //catch (Exception ex)
            //{
            //    logger.Error("Exception Branch : " + condition.Branch.ToString(), ex);
            //    throw ex;
            //    return lstreportGeneralInvoice;
            //}

            logger.Error("Branch : " + condition.Branch.ToString(), new Exception("Branch current"));

            SIGNATURE signature = this.signatureBO.GetByCompany((long)condition.Branch);

            if (signature == null)
            {
                //throw new BusinessLogicException(ResultCode.CaNotExisted, "Vui lòng upload file CA cho chi nhánh đang sử dụng");
                logger.Error("Không tìm thấy thông tin chữ ký : " + condition.Branch.ToString(), new Exception("Không tìm thấy thông tin chữ ký"));
            }

            lstreportGeneralInvoice = this.invoiceBO.ListReportHistoryXml(condition, signature);

            //lstreportGeneralInvoice = this.ListReportHistoryXml(condition, signature);

            //foreach (var item in lstreportGeneralInvoice)
            //{
            //    item.CompanyId = condition.Branch;
            //    this.historyReportGeneralBO.updateFileName(item);
            //}

            return lstreportGeneralInvoice;
        }
        public List<MYCOMPANY> GetListCompanyNeedCreateFiles()
        {
            return this.noticeUseInvoiceBO.GetListCompanyShinhan();
        }
        public List<ReportHistoryXml> SendMQTXml_Auto(ConditionReportDetailUse condition)
        {
            List<ReportHistoryXml> lstreportGeneralInvoice = new List<ReportHistoryXml>();

            logger.Error("Branch current : " + condition.Branch.ToString(), new Exception("Branch current"));
            logger.Error("Current datefrom : " + condition.DateFrom.ToString(), new Exception("Current datefrom"));
            logger.Error("Current dateto : " + condition.DateTo.ToString(), new Exception("Current dateto"));

            SIGNATURE signature = this.signatureBO.GetByCompany((long)condition.Branch);

            if (signature == null)
            {
                logger.Error("Không tìm thấy thông tin chữ ký : " + condition.Branch.ToString(), new Exception("Không tìm thấy thông tin chữ ký"));
            }

            lstreportGeneralInvoice = this.invoiceBO.ListReportHistoryXml_Auto(condition, signature);

            //foreach(var item in lstreportGeneralInvoice)
            //{
            //    string textXml = File.ReadAllText(item.fileInfo.FullPathFileName);
            //    var base64File = Convert.ToBase64String(File.ReadAllBytes(item.fileInfo.FullPathFileName));

            //    logger.Error("Start call API tvan : " + DateTime.Now, new Exception("Start call API tvan"));

            //    VanDefaulRequest vanDefaulRequest = new VanDefaulRequest()
            //    {
            //        HistoryReportId = item.Id,
            //        XmlData = base64File,
            //        InvoiceIds = item.listInvoiceId,
            //        MstNnt = item.CompanyTaxCode
            //    };
            //    VanDefaulResult vanDefaulResult = this.businessTWan.VanSynthesis(vanDefaulRequest);

            //    logger.Error("End call API tvan : " + DateTime.Now, new Exception("End call API tvan"));
            //}

            //foreach (var item in lstreportGeneralInvoice)
            //{
            //    item.CompanyId = condition.Branch;
            //    this.historyReportGeneralBO.updateFileName(item);
            //}

            return lstreportGeneralInvoice;
        }

        public ReportHistoryXml SendMQTXml_Auto_SingleFile(ConditionReportDetailUse condition)
        {
            ReportHistoryXml reportHistoryXml = new ReportHistoryXml();
            logger.Error("Branch current : " + condition.Branch.ToString(), new Exception("Branch current"));
            logger.Error("Current datefrom : " + condition.DateFrom.ToString(), new Exception("Current datefrom"));
            logger.Error("Current dateto : " + condition.DateTo.ToString(), new Exception("Current dateto"));
            SIGNATURE signature = this.signatureBO.GetByCompany((long)condition.Branch);

            return reportHistoryXml;
        }


        public ResultSignReportgeneral CheckResultSendTwan(long id)
        {
            return this.historyReportGeneralBO.CheckResultSendTwan(id);
        }

        public ExportFileInfo DownloadXMLBCTH(long id,long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.DownloadXMLBCTH(long, long)'
        {
            return this.historyReportGeneralBO.DownloadXMLBCTH(id,companyId);
        }

        public ReportGeneralInvoice GetBSLThu(ConditionReportDetailUse condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.GetBSLThu(ConditionReportDetailUse)'
        {
            ReportGeneralInvoice reportGeneralInvoice = this.invoiceBO.GetBSLThu(condition);
            return reportGeneralInvoice;
        }
        public ExportFileInfo DownloadHisItem(long hisId)
        {
            return this.invoiceBO.DownloadHisItem(hisId);
        }
        public ResultCode ProcessXmlError(string mesCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportBusiness.ProcessXmlError(string)'
        {
            return this.invoiceBO.processXmlError(mesCode);
        }

        public void UpdateProcessJob(string name, bool processing)
        {
            this.quarztJobBO.UpdateProcessByName(name, processing);
        }

        public long GetlatestSHDTH(ConditionReportDetailUse condition)
        {
            var result = this.historyReportGeneralBO.GetAdditionTimesNext(condition);
            return result;
        }
        #endregion
    }
}