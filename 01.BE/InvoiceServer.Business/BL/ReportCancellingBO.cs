using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.ReportCancelling;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class ReportCancellingBO : IReportCancellingBO
    {

        private readonly IReportCancellingRepository reportCancellingRepository;
        private readonly IReportCancellingDetailRepository reportCancellingDetailRepository;
        private readonly IDbTransactionManager transaction;
        private readonly IInvoiceRepository invoiceRepository;

        #region Contructor

        public ReportCancellingBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.reportCancellingRepository = repoFactory.GetRepository<IReportCancellingRepository>();
            this.reportCancellingDetailRepository = repoFactory.GetRepository<IReportCancellingDetailRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
        }

        #endregion

        public IEnumerable<ReportCancellingMaster> FilterReportCancelling(long companyId)
        {
            var reportsCancelling = this.reportCancellingRepository.FilterReportCancelling(companyId);
            return reportsCancelling.Select(p => new ReportCancellingMaster(p)).ToList();
        }

        public ResultCode Create(ReportCancellingInfo cancellingInfo)
        {
            try
            {
                transaction.BeginTransaction();
                CreateReport(cancellingInfo);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        private void CreateReport(ReportCancellingInfo cancellingInfo)
        {
            if (cancellingInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            REPORTCANCELLING reportCancelling = CreateReportCancelling(cancellingInfo);
            foreach (var item in cancellingInfo.RegisterTemplatesCancelling)
            {
                bool isExisted = this.reportCancellingDetailRepository.ContainRegisterTemplate(item.RegisterTemplatesId, item.Code, 0);
                if (isExisted)
                {
                    continue;
                }

                CreateReportCancellingDetail(item, reportCancelling.ID);
            }
        }
        private REPORTCANCELLING CreateReportCancelling(ReportCancellingInfo cancellingInfo)
        {
            if (cancellingInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!cancellingInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            var reportCancellling = new REPORTCANCELLING();
            reportCancellling.CopyData(cancellingInfo);
            reportCancellling.STATUS = (int)ReportCancellingStatus.New;
            this.reportCancellingRepository.Insert(reportCancellling);
            return reportCancellling;
        }

        private void CreateReportCancellingDetail(RegisterTemplateCancelling cancellingDetailInfo, long reportCancellingId)
        {
            if (cancellingDetailInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!cancellingDetailInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            var reportCancellling = new REPORTCANCELLINGDETAIL();
            reportCancellling.CopyData(cancellingDetailInfo);
            reportCancellling.REPORTCANCELLINGID = reportCancellingId;
            this.reportCancellingDetailRepository.Insert(reportCancellling);
        }

        public ReportCancellingInfo GetById(long id, long companyId)
        {
            var reportCancelling = this.reportCancellingRepository.GetById(id, companyId);
            if (reportCancelling == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.DataNotFound);
            }

            ReportCancellingInfo reportcancellingInfo = new ReportCancellingInfo(reportCancelling);
            reportcancellingInfo.RegisterTemplatesCancelling = reportCancelling.REPORTCANCELLINGDETAILs.Select(p => new RegisterTemplateCancelling(p)).ToList();
            return reportcancellingInfo;
        }

        public long GetAllNumberInvoiceCancelling()
        {
            decimal sumNumberCancelling = 0;
            var reportsCancellings = this.reportCancellingRepository.FilterAllReportCancelling();

            foreach (var item in reportsCancellings)
            {
                var reportCancellingDetail = this.reportCancellingDetailRepository.GetById(item.ID);

                sumNumberCancelling = sumNumberCancelling + (reportCancellingDetail.NUMBER ?? 0);

            }

            return (long)sumNumberCancelling;
        }

        public long GetNumberInvoiceCancelling(long companyId)
        {
            decimal sumNumberCancelling = 0;
            var reportsCancellings = this.reportCancellingRepository.FilterReportCancelling(companyId);

            foreach (var item in reportsCancellings)
            {
                var reportCancellingDetail = this.reportCancellingDetailRepository.FilterBy(companyId, item.ID).FirstOrDefault();

                sumNumberCancelling = sumNumberCancelling + (reportCancellingDetail.NUMBER ?? 0);

            }

            return (long)sumNumberCancelling;
        }

        public long GetNumberInvoiceCancelling(ConditionInvoiceUse condition)
        {
            return this.reportCancellingDetailRepository.GetNumberInvoiceCancelling(condition);
        }

        public ResultCode ApproveCancelling(ApprovedReportCancelling cancellingInfo, UserSessionInfo currentUser)
        {
            try
            {
                transaction.BeginTransaction();
                AproveReport(cancellingInfo, currentUser);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }


        private ResultApproved AproveReport(ApprovedReportCancelling cancellingInfo, UserSessionInfo currentUser)
        {

            ResultApproved result = new ResultApproved();

            var reportCanceling = reportCancellingRepository.GetById(cancellingInfo.Id, cancellingInfo.CompanyId);

            foreach (var R in reportCanceling.REPORTCANCELLINGDETAILs)
            {
                var invoices = this.invoiceRepository.FilterInvoice(cancellingInfo.CompanyId, R.REGISTERTEMPLATESID ?? 0, R.SYMBOL.Replace("/", "")).OrderByDescending(p => p.NO).FirstOrDefault();
                string maxInvoice = "0";
                if (invoices != null)
                {
                    maxInvoice = invoices.NO;
                }
                var nextInvoiceNo = int.Parse(maxInvoice);
                if (nextInvoiceNo >= int.Parse(R.NUMBERFROM) || nextInvoiceNo >= int.Parse(R.NUMBERTO))
                {
                    throw new BusinessLogicException(ResultCode.ReportCancellingNotApprove, "Do not approve report cancelling, please check serial number need to cancel");
                }

                reportCanceling.STATUS = (int)ReportCancellingStatus.Approved;
                reportCanceling.APPROVEDBY = cancellingInfo.UserActionId;
                reportCanceling.APPROVEDDATE = DateTime.Now;
                this.reportCancellingRepository.Update(reportCanceling);
            }

            if (result.RowError.Any())
            {
                result.ErrorCode = ResultCode.ImportFileFormatInvalid;
                result.Message = MsgApiResponse.DataInvalid;
            }

            return result;

        }

        public ResultCode revertApproveCancelling(ApprovedReportCancelling cancellingInfo, UserSessionInfo currentUser)
        {
            try
            {
                transaction.BeginTransaction();
                revertApproveReport(cancellingInfo, currentUser);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        private ResultApproved revertApproveReport(ApprovedReportCancelling cancellingInfo, UserSessionInfo currentUser)
        {
            ResultApproved result = new ResultApproved();


            var reportCanceling = reportCancellingRepository.GetById(cancellingInfo.Id, cancellingInfo.CompanyId);


            foreach (var R in reportCanceling.REPORTCANCELLINGDETAILs)
            {

                reportCanceling.STATUS = (int)ReportCancellingStatus.New;
                reportCanceling.APPROVEDBY = cancellingInfo.UserActionId;
                reportCanceling.APPROVEDDATE = DateTime.Now;
                this.reportCancellingRepository.Update(reportCanceling);
            }

            if (result.RowError.Any())
            {
                result.ErrorCode = ResultCode.ImportFileFormatInvalid;
                result.Message = MsgApiResponse.DataInvalid;
            }

            return result;

        }

        public ResultCode Delete(long id, long? companyId)
        {
            try
            {
                transaction.BeginTransaction();
                DeleteReportCancelling(id, companyId);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        private void DeleteReportCancelling(long id, long? companyId)
        {
            var reportCancelling = this.reportCancellingRepository.GetById(id, companyId ?? 0);
            if (reportCancelling.STATUS != 1)
            {

                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var reportCancellingDetails = reportCancelling.REPORTCANCELLINGDETAILs.ToList();
            foreach (var R in reportCancellingDetails)
            {
                this.reportCancellingDetailRepository.Delete(R);
            }
            this.reportCancellingRepository.Delete(reportCancelling);

        }


        public ResultCode Update(long id, long? companyId, ReportCancellingInfo cancellingInfo)
        {
            try
            {
                transaction.BeginTransaction();
                this.UpdateReport(id, companyId, cancellingInfo);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        private void UpdateReport(long id, long? companyId, ReportCancellingInfo cancellingInfo)
        {
            if (cancellingInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            REPORTCANCELLING reportCancelling = UpdateReportCancelling(id, companyId, cancellingInfo);
            foreach (var reportId in reportCancelling.REPORTCANCELLINGDETAILs.ToList())
            {

                DeleteReportCancellingDetail(reportId.ID);


            }
            foreach (var item in cancellingInfo.RegisterTemplatesCancelling)
            {
                bool isExisted = this.reportCancellingDetailRepository.ContainRegisterTemplate(item.RegisterTemplatesId, item.Code, reportCancelling.ID);
                if (isExisted)
                {
                    continue;
                }


                CreateReportCancellingDetail(item, reportCancelling.ID);

            }
        }
        private REPORTCANCELLING UpdateReportCancelling(long id, long? companyId, ReportCancellingInfo cancellingInfo)
        {

            var reportCancelling = this.reportCancellingRepository.GetById(id, cancellingInfo.CompanyId);
            if (reportCancelling == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            if (reportCancelling.STATUS != 1)
            {

                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            reportCancelling.CopyData(cancellingInfo);
            reportCancelling.UPDATEDDATE = DateTime.Now;
            reportCancelling.UPDATEDBY = cancellingInfo.UpdateBy;
            this.reportCancellingRepository.Update(reportCancelling);
            return reportCancelling;
        }
        private void DeleteReportCancellingDetail(long reportCancellingDetailId)
        {

            var detailCancelling = reportCancellingDetailRepository.GetById(reportCancellingDetailId);

            reportCancellingDetailRepository.Delete(detailCancelling);

        }



    }

}
