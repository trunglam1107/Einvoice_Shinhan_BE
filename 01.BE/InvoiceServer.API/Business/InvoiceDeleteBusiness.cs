using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteBusiness'
    public class InvoiceDeleteBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteBusiness'
    {
        #region Fields, Properties

        private readonly IInvoiceDeleteBO invoiceDeleteBO;

        #endregion Fields, Properties

        #region Contructor
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteBusiness.InvoiceDeleteBusiness(IBOFactory)'
        public InvoiceDeleteBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteBusiness.InvoiceDeleteBusiness(IBOFactory)'
        {
            this.invoiceDeleteBO = boFactory.GetBO<IInvoiceDeleteBO>(this.CurrentUser);
        }

        #endregion Contructor
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteBusiness.InvoiceDelete(CancellingInvoiceMaster)'
        public ResultCode InvoiceDelete(CancellingInvoiceMaster deleteInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteBusiness.InvoiceDelete(CancellingInvoiceMaster)'
        {
            this.invoiceDeleteBO.InvoiceDelete(deleteInfo);
            return ResultCode.NoError;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteBusiness.MultipleInvoiceDelete(CancellingInvoiceMaster)'
        public ResultCode MultipleInvoiceDelete(CancellingInvoiceMaster deleteInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteBusiness.MultipleInvoiceDelete(CancellingInvoiceMaster)'
        {
            this.invoiceDeleteBO.MultipleInvoiceDelete(deleteInfo);
            return ResultCode.NoError;
        }

        public ResultCode MultipleInvoiceCancel(CancellingInvoiceMaster deleteInfo)
        {
            //this.invoiceDeleteBO.MultipleInvoiceCancel(deleteInfo);
            //this.jobReplacedInvoiceBO.MultipleInvoiceCancel(deleteInfo);
            return ResultCode.NoError;
        }
        #region Methods
        #endregion
    }
}