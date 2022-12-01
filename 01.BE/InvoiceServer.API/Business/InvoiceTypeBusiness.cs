using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness'
    public class InvoiceTypeBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness'
    {
        private readonly IInvoiceTypeBO invoiceTypeBO;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.InvoiceTypeBusiness(IBOFactory)'
        public InvoiceTypeBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.InvoiceTypeBusiness(IBOFactory)'
        {
            this.invoiceTypeBO = boFactory.GetBO<IInvoiceTypeBO>();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.Fillter(ConditionSearchInvoiceType)'
        public IEnumerable<InvoiceTypeViewModel> Fillter(ConditionSearchInvoiceType condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.Fillter(ConditionSearchInvoiceType)'
        {
            condition.TotalRecords = this.invoiceTypeBO.Count(condition);
            return this.invoiceTypeBO.Filter(condition);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.GetByCode(string)'
        public InvoiceTypeViewModel GetByCode(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.GetByCode(string)'
        {
            if (code == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var currnetTaxTypeVm = this.invoiceTypeBO.GetByCode(code);
            if (currnetTaxTypeVm != null)
            {
                return currnetTaxTypeVm;
            }
            return new InvoiceTypeViewModel();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.Create(InvoiceTypeViewModel)'
        public ResultCode Create(InvoiceTypeViewModel invoiceTypeViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.Create(InvoiceTypeViewModel)'
        {
            if (invoiceTypeViewModel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return ResultCode.NoError;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.Update(string, InvoiceTypeViewModel)'
        public ResultCode Update(string code, InvoiceTypeViewModel invoiceTypeViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.Update(string, InvoiceTypeViewModel)'
        {
            if (invoiceTypeViewModel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.invoiceTypeBO.Update(code, invoiceTypeViewModel);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.Delete(string)'
        public ResultCode Delete(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.Delete(string)'
        {
            return this.invoiceTypeBO.Delete(code);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.MyInvoiceTypeUsing(long)'
        public MyUsingModel MyInvoiceTypeUsing(long invoiceTypeId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeBusiness.MyInvoiceTypeUsing(long)'
        {
            var myUsing = new MyUsingModel();
            myUsing.isUsing = invoiceTypeBO.MyInvoiceTypeUsing(invoiceTypeId);
            return myUsing;
        }
    }
}