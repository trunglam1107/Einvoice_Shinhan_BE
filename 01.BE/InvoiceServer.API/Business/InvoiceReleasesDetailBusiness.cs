using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesDetailBusiness'
    public class InvoiceReleasesDetailBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesDetailBusiness'
    {
        #region Fields, Properties

        private readonly IInvoiceReleasesDetailBO invoiceReleaseDetailBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesDetailBusiness.InvoiceReleasesDetailBusiness(IBOFactory)'
        public InvoiceReleasesDetailBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesDetailBusiness.InvoiceReleasesDetailBusiness(IBOFactory)'
        {
            this.invoiceReleaseDetailBO = boFactory.GetBO<IInvoiceReleasesDetailBO>();
        }

        #endregion Contructor

        #region Methods


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesDetailBusiness.GetInvoiceReleasesDetailInfo(long)'
        public InvoiceReleasesDetailInfo GetInvoiceReleasesDetailInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesDetailBusiness.GetInvoiceReleasesDetailInfo(long)'
        {
            return this.invoiceReleaseDetailBO.GetInvoiceReleasesDetailInfo(id);
        }
        #endregion
    }
}