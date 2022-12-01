using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceDetailBusiness'
    public class NoticeUseInvoiceDetailBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceDetailBusiness'
    {
        #region Fields, Properties

        private readonly INoticeUseInvoiceDetailBO noticeUseInvoiceDetailBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceDetailBusiness.NoticeUseInvoiceDetailBusiness(IBOFactory)'
        public NoticeUseInvoiceDetailBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceDetailBusiness.NoticeUseInvoiceDetailBusiness(IBOFactory)'
        {
            this.noticeUseInvoiceDetailBO = boFactory.GetBO<INoticeUseInvoiceDetailBO>();
        }

        #endregion Contructor

        #region Methods


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceDetailBusiness.GetInvoiceReleasesDetailInfo(long)'
        public UseInvoiceDetailInfo GetInvoiceReleasesDetailInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceDetailBusiness.GetInvoiceReleasesDetailInfo(long)'
        {
            return this.noticeUseInvoiceDetailBO.GetNoticeUseInvoiceDetail(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceDetailBusiness.FilterInvoiceSymbol(long)'
        public IEnumerable<string> FilterInvoiceSymbol(long invoiceTemplateId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceDetailBusiness.FilterInvoiceSymbol(long)'
        {
            var companyId = GetCompanyIdOfUser();
            return this.noticeUseInvoiceDetailBO.FilterInvoiceSymbol(companyId, invoiceTemplateId, false);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceDetailBusiness.FilterInvoiceSymbolUse(long, long)'
        public IEnumerable<RegisterTemplateCancelling> FilterInvoiceSymbolUse(long invoiceSampleId, long reportCancellingId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceDetailBusiness.FilterInvoiceSymbolUse(long, long)'
        {
            var companyId = GetCompanyIdOfUser();
            return this.noticeUseInvoiceDetailBO.FilterInvoiceSymbolUse(companyId, invoiceSampleId, reportCancellingId);
        }


        #endregion
    }
}