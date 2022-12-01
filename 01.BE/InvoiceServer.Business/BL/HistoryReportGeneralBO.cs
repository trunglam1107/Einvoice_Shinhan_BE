using InvoiceServer.Common.Constants;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using InvoiceServer.Business.Models.HistoryReportGeneral;
using System.Linq;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using Dapper;

namespace InvoiceServer.Business.BL
{
    public class HistoryReportGeneralBO : IHistoryReportGeneralBO
    {
        #region Fields, Properties

        private readonly IHistoryReportGeneralRepository historyReportGeneralRepository;
        private readonly IDbTransactionManager transaction;
        #endregion

        #region Contructor

        public HistoryReportGeneralBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.NotNullArg(repoFactory, "repoFactory");
            this.historyReportGeneralRepository = repoFactory.GetRepository<IHistoryReportGeneralRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
        }

        public HISTORYREPORTGENERAL CreateHistoryGeneral(ConditionReportDetailUse condition,string fileName)
        {

            HISTORYREPORTGENERAL history = new HISTORYREPORTGENERAL();
            history.COMPANYID = condition.Branch;
            history.PERIODSREPORT = condition.IsMonth;
            history.FILENAME = fileName;
            if (condition.IsMonth == 0)
            {
                history.QUARTER = condition.Month;
            }
            else
            {
                history.MONTH = condition.Month;
            }

            history.YEAR = condition.Year;
            if (condition.TimeNo == 0)
            {
                history.ADDITIONALTIMES = 0;
            }
            else
            {
                history.ADDITIONALTIMES = condition.TimeNo;
            }
            //var isExistFirstTimeSuccess = this.historyReportGeneralRepository.IsExistFirstTime_Success(history.PERIODSREPORT, history.YEAR, history.MONTH, history.QUARTER, condition.Branch);
            //var isFirstTime = history.ADDITIONALTIMES == 0;
            //if (!isFirstTime && !isExistFirstTimeSuccess)
            //{
            //    //throw new BusinessLogicException(ResultCode.NotExistFirstTimeSuccess, MsgApiResponse.DataInvalid);
            //    history.SBTHDLIEU = 1;
            //}
            //if (isFirstTime && isExistFirstTimeSuccess)
            //{
            //    history.SBTHDLIEU = 1;
            //    //throw new BusinessLogicException(ResultCode.CanNotInsertFirstTimeTwice, MsgApiResponse.DataInvalid);
            //}

            history.SBTHDLIEU = condition.SBTHDulieu;//this.historyReportGeneralRepository.GetSBTHDLIEU(history.PERIODSREPORT, history.YEAR, history.MONTH, history.QUARTER, condition.Branch);
            history.CREATEDDATE = DateTime.Now;
            this.historyReportGeneralRepository.Insert(history);
            return history;

        }

        public HISTORYREPORTGENERAL GetById(long id)
        {
            var historyReportGeneral = this.historyReportGeneralRepository.GetById(id);
            if (historyReportGeneral == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);

            }
            return historyReportGeneral;
        }

        public IEnumerable<Models.HistoryReportGeneralInfo> Filter(int monthOrQuarterNumber, bool isMonth, int year,
            int additionalTimes)
        {
            var condition = new FilterHistoryReportGeneralCondition(monthOrQuarterNumber, isMonth, year, additionalTimes);
            var historyReportGenerals = this.historyReportGeneralRepository.Filter(condition);
            return historyReportGenerals.Select(x => new Models.HistoryReportGeneralInfo(x));
        }

        public void updateFileName(ReportHistoryXml reportHistoryXml)
        {
            var historyReportGeneral = this.historyReportGeneralRepository.GetById(reportHistoryXml.Id);

            if (historyReportGeneral == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);

            }
            historyReportGeneral.FILENAME = reportHistoryXml.fileInfo.FileName;
            historyReportGeneral.COMPANYID = reportHistoryXml.CompanyId;
            this.historyReportGeneralRepository.Update(historyReportGeneral);


        }

        public ResultSignReportgeneral CheckResultSendTwan(long id)
        {
            return this.historyReportGeneralRepository.CheckResultSendTwan(id);
        }

        public IEnumerable<Models.HistoryReportGeneralInfo> GetAll(FilterHistoryReportGeneralCondition condition)
        {
            //var condition = new FilterHistoryReportGeneralCondition(monthOrQuarterNumber, isMonth, year, additionalTimes);
            var historyReportGenerals = this.historyReportGeneralRepository.GetAll(condition);
            return historyReportGenerals.Select(x => new Models.HistoryReportGeneralInfo(x));
        }

        public long Count(FilterHistoryReportGeneralCondition condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.historyReportGeneralRepository.Count(condition);
        }

        public ExportFileInfo DownloadXMLBCTH(long id, long companyId)
        {
            ExportFileInfo fileInfo;
            fileInfo = GetXMLBCTHFile(id, companyId);
            return fileInfo;
        }

        private ExportFileInfo GetXMLBCTHFile(long id, long companyId)
        {
            //UpdateForErrorBTH("V0106026495A7B3E2B4F51F4C5A8737FD6E154A9132", "<MLoi>40051</MLoi>", "<MTLoi>Test</MTLoi>");

            var data = this.historyReportGeneralRepository.GetById(id);

            ExportFileInfo fileInfo = new ExportFileInfo();
            string folderStore = System.Configuration.ConfigurationManager.AppSettings["FolderInvoiceFile"];
            string path = Path.Combine(folderStore, companyId.ToString(), "Report", "Sign");
            string path2 = Path.Combine(folderStore, companyId.ToString(), "Report");
            string[] filePaths = Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly);
            string[] filePaths2 = Directory.GetFiles(path2, "*.xml", SearchOption.TopDirectoryOnly);
            string fileName;

            if (filePaths.Count() > 0)
            {
                foreach (var file in filePaths)
                {
                    fileName = Path.GetFileName(file);
                    if (fileName.Contains(data.FILENAME))
                    {
                        fileInfo.FileName = Path.GetFileName(file);
                        fileInfo.FullPathFileName = Path.Combine(folderStore, companyId.ToString(), "Report", "Sign", fileInfo.FileName);
                        break;
                    }
                }
            }

            if (fileInfo.FileName == null || string.IsNullOrEmpty(fileInfo.FileName))
            {
                if (filePaths2.Count() > 0)
                {
                    foreach (var file2 in filePaths2)
                    {
                        fileName = Path.GetFileName(file2);
                        if (fileName.Contains(data.FILENAME))
                        {
                            fileInfo.FileName = Path.GetFileName(file2);
                            fileInfo.FullPathFileName = Path.Combine(folderStore, companyId.ToString(), "Report", fileInfo.FileName);
                            break;
                        }
                    }
                }
            }

           

            return fileInfo;
        }

        public void UpdateForErrorBTH(string messagecode, string errorcode, string error)
        {
            //logger.Error("messagecode" + messagecode, new Exception(""));
            //logger.Error("errorcode : " + errorcode, new Exception(""));
            //logger.Error("error" + error, new Exception(""));
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
            string sqlResult = @"UPDATE INVOICE SET BTHERROR = '{error}', BTHERRORSTATUS = {errorcode} WHERE MESSAGECODE = '{messagecode}'";
            sqlResult = sqlResult.Replace("{errorcode}", errorcode.Replace("<MLoi>","").Replace("</MLoi>","").Trim()).Replace("{error}", error.Replace("<MTLoi>","").Replace("</MTLoi>", "").Trim()).Replace("{messagecode}", messagecode);
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                var result = connection.Execute(sqlResult);
                connection.Close();
            }
        }

        public IEnumerable<Models.HistoryReportGeneralInfo> GetAllForExcel(FilterHistoryReportGeneralCondition condition)
        {
            //var condition = new FilterHistoryReportGeneralCondition(monthOrQuarterNumber, isMonth, year, additionalTimes);
            var historyReportGenerals = this.historyReportGeneralRepository.GetAllForExcel(condition);
            return historyReportGenerals.Select(x => new Models.HistoryReportGeneralInfo(x));
        }
        public ExportFileInfo DownloadExcelHRG(FilterHistoryReportGeneralCondition condition)
        {
            var dataRport = new ReportExcelHGR();
            dataRport.Items = this.GetAllForExcel(condition).ToList();
            ReportExportAllHGRView reportExportAllHGRView = new ReportExportAllHGRView(dataRport);
            return reportExportAllHGRView.ExportFile();

        }

        private bool isCheckFirst(ConditionReportDetailUse condition)
        {
            var historyList = this.historyReportGeneralRepository.GetHistoryReportsList(condition)
                .Where(x => (x.LTBAO == StatusGDT.Mltbao2 || x.LTBAO == StatusGDT.Mltbao4)
                    && (x.MLTDIEP == StatusGDT.MLTDIEP)
                    && (x.AdditionalTimes == 0)
                    && ((condition.IsMonth == 1)
                        ? x.Month == condition.Month
                        : x.Quarter == condition.Month));

            var result = historyList.Count() > 0;

            return result;
        }

        public long GetAdditionTimesNext(ConditionReportDetailUse condition)
        {
            if (!isCheckFirst(condition))
            {
                throw new BusinessLogicException(ResultCode.NotExistFirstTimeSuccess, MsgApiResponse.DataInvalid);
            }

            var historyList = this.historyReportGeneralRepository.GetHistoryReportsList(condition)
                .Where(x => (x.LTBAO == StatusGDT.Mltbao2 || x.LTBAO == StatusGDT.Mltbao4)
                        && (x.MLTDIEP == StatusGDT.MLTDIEP)
                        && (x.AdditionalTimes != 0)
                        && ((condition.IsMonth == 1)
                            ? x.Month == condition.Month
                            : x.Quarter == condition.Month))
                    .OrderByDescending(p => p.SBTHDulieu)
                    .OrderByDescending(p => p.AdditionalTimes)
                    .FirstOrDefault();
            var result = (historyList != null) ? (long)(historyList.AdditionalTimes + 1) : 1;

            return result;
        }

        #endregion

        #region Methods

        //public ResultCode Create(CancellingPaymentMaster cancellingInfo)
        //{
        //    try
        //    {
        //        transaction.BeginTransaction();
        //        CancellingPayment(cancellingInfo);
        //        transaction.Commit();
        //    }
        //    catch
        //    {
        //        transaction.Rollback();
        //        throw;
        //    }

        //    return ResultCode.NoError;
        //}

        //private void CancellingPayment(CancellingPaymentMaster cancellingInvoiceinfo)
        //{
        //    if (cancellingInvoiceinfo == null)
        //    {
        //        throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
        //    }

        //    cancellingInvoiceinfo.CancellingPayment.ForEach(p => {
        //        UpdateInvoicePaymentStatus(p.InvoiceId, cancellingInvoiceinfo.CompanyId);
        //    });
        //}


        //private void UpdateInvoicePaymentStatus(long id, long companyId)
        //{
        //    INVOICE currentInvoice = GetInvoice(id, companyId);
        //    currentInvoice.PAYMENTSTATUS = true;
        //    this.invoiceRepository.Update(currentInvoice);
        //}

        //private INVOICE GetInvoice(long id, long companyId)
        //{
        //    INVOICE invoices = this.invoiceRepository.GetById(id, companyId);
        //    if (invoices == null)
        //    {
        //        throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This InvoiceId [{0}] not found in data of client", id));
        //    }

        //    return invoices;
        //}

        #endregion
    }
}