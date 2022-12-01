using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.ReportCancelling;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness'
    public class ReportCancellingBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness'
    {
        #region Fields, Properties

        private readonly IReportCancellingBO reportCancellingBO;
        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.ReportCancellingBusiness(IBOFactory)'
        public ReportCancellingBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.ReportCancellingBusiness(IBOFactory)'
        {
            this.reportCancellingBO = boFactory.GetBO<IReportCancellingBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.FilterReportCancelling()'
        public IEnumerable<ReportCancellingMaster> FilterReportCancelling()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.FilterReportCancelling()'
        {
            long companyId = GetCompanyIdOfUser();
            return this.reportCancellingBO.FilterReportCancelling(companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.GetReportCancelling(long)'
        public ReportCancellingInfo GetReportCancelling(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.GetReportCancelling(long)'
        {
            long companyId = GetCompanyIdOfUser();
            return this.reportCancellingBO.GetById(id, companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.Create(ReportCancellingInfo)'
        public ResultCode Create(ReportCancellingInfo cancellingInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.Create(ReportCancellingInfo)'
        {
            long companyId = GetCompanyIdOfUser();
            cancellingInfo.CompanyId = companyId;
            return this.reportCancellingBO.Create(cancellingInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.ApproveCancelling(ApprovedReportCancelling)'
        public ResultCode ApproveCancelling(ApprovedReportCancelling cancellingInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.ApproveCancelling(ApprovedReportCancelling)'
        {
            if (cancellingInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            long companyId = GetCompanyIdOfUser();
            cancellingInfo.CompanyId = companyId;
            cancellingInfo.UserActionId = this.CurrentUser.Id;

            return this.reportCancellingBO.ApproveCancelling(cancellingInfo, this.CurrentUser);

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.revertApproveCancelling(ApprovedReportCancelling)'
        public ResultCode revertApproveCancelling(ApprovedReportCancelling cancellingInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.revertApproveCancelling(ApprovedReportCancelling)'
        {
            if (cancellingInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            long companyId = GetCompanyIdOfUser();
            cancellingInfo.CompanyId = companyId;
            cancellingInfo.UserActionId = this.CurrentUser.Id;
            return this.reportCancellingBO.revertApproveCancelling(cancellingInfo, this.CurrentUser);

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.Delete(long)'
        {
            long companyId = GetCompanyIdOfUser();

            return this.reportCancellingBO.Delete(id, companyId);

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.Update(long, ReportCancellingInfo)'
        public ResultCode Update(long id, ReportCancellingInfo cancellingInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingBusiness.Update(long, ReportCancellingInfo)'
        {
            if (cancellingInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            long idCompany = GetCompanyIdOfUser();
            cancellingInfo.CompanyId = idCompany;
            cancellingInfo.UpdateBy = this.CurrentUser.Id;


            return this.reportCancellingBO.Update(id, idCompany, cancellingInfo);
        }



        #endregion
    }
}