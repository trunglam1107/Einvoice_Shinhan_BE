using InvoiceServer.Business.BL;
using InvoiceServer.Business.BL.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
using InvoiceServer.Business.Models.InvoiceSample_Type;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListBusiness'
    public class CurrencyListBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListBusiness'
    {
        private readonly ICurrencyListBO currencyListBO;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListBusiness.CurrencyListBusiness(IBOFactory)'
        public CurrencyListBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListBusiness.CurrencyListBusiness(IBOFactory)'
        {
            this.currencyListBO = boFactory.GetBO<ICurrencyListBO>();
        }

        
#pragma warning disable CS1570 // XML comment has badly formed XML -- 'End tag 'returns' does not match the start tag 'CurrencyListViewModel'.'
/// <summary>
        /// GetAll
        /// </summary>
        /// <returns>IEnumerable<CurrencyListViewModel></returns>
#pragma warning disable CS1570 // XML comment has badly formed XML -- 'Expected an end tag for element 'returns'.'
        public IEnumerable<CurrencyListViewModel> GetAll()
#pragma warning restore CS1570 // XML comment has badly formed XML -- 'Expected an end tag for element 'returns'.'
#pragma warning restore CS1570 // XML comment has badly formed XML -- 'End tag 'returns' does not match the start tag 'CurrencyListViewModel'.'
        {
            return this.currencyListBO.GetList();
        }

        
#pragma warning disable CS1570 // XML comment has badly formed XML -- 'End tag 'returns' does not match the start tag 'CurrencyListViewModel'.'
/// <summary>
        /// Fillter
        /// </summary>
        /// <param name="totalRecords"></param>
        /// <param name="key"></param>
        /// <param name="orderType"></param>
        /// <param name="orderby"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns>IEnumerable<CurrencyListViewModel></returns>
#pragma warning disable CS1570 // XML comment has badly formed XML -- 'Expected an end tag for element 'returns'.'
        public IEnumerable<CurrencyListViewModel> Fillter(ConditionSearchInvoiceType condition)
#pragma warning restore CS1570 // XML comment has badly formed XML -- 'End tag 'returns' does not match the start tag 'CurrencyListViewModel'.'
#pragma warning restore CS1570 // XML comment has badly formed XML -- 'Expected an end tag for element 'returns'.'
        {
            condition.TotalRecords = this.currencyListBO.Count(condition);
            return this.currencyListBO.Filter(condition);
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="currencyListViewModel"></param>
        /// <returns>ResultCode</returns>
        public ResultCode Create(CurrencyListViewModel currencyListViewModel)
        {
            if (currencyListViewModel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            currencyListViewModel.CompanyId = this.CurrentUser.Company.Id ?? 0;
            this.currencyListBO.Create(currencyListViewModel);
            return ResultCode.NoError;
        }

        
#pragma warning disable CS1572 // XML comment has a param tag for 'unitListViewModel', but there is no parameter by that name
/// <summary>
        /// Update
        /// </summary>
        /// <param name="code"></param>
        /// <param name="unitListViewModel"></param>
        /// <returns>ResultCode</returns>
#pragma warning disable CS1573 // Parameter 'currencyListViewModel' has no matching param tag in the XML comment for 'CurrencyListBusiness.Update(string, CurrencyListViewModel)' (but other parameters do)
        public ResultCode Update(string code, CurrencyListViewModel currencyListViewModel)
#pragma warning restore CS1572 // XML comment has a param tag for 'unitListViewModel', but there is no parameter by that name
#pragma warning restore CS1573 // Parameter 'currencyListViewModel' has no matching param tag in the XML comment for 'CurrencyListBusiness.Update(string, CurrencyListViewModel)' (but other parameters do)
        {
            if (currencyListViewModel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.currencyListBO.Update(code, currencyListViewModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ResultCode Delete(string code)
        {
            return this.currencyListBO.Delete(code);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListBusiness.GetByCode(string)'
        public CurrencyListViewModel GetByCode(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListBusiness.GetByCode(string)'
        {
            if (code == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var condition = new ConditionSearchInvoiceType(null, null)
            {
                CurrentUser = this.CurrentUser,
                Key = code.DecodeUrl(),
            };

            var currnetTaxTypeVm = this.currencyListBO.GetByCode(condition);
            if (currnetTaxTypeVm != null)
            {
                return currnetTaxTypeVm;
            }
            return new CurrencyListViewModel();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListBusiness.MyCurrencyUsing(long)'
        public MyUsingModel MyCurrencyUsing(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListBusiness.MyCurrencyUsing(long)'
        {
            var myusing = new MyUsingModel();
            myusing.isUsing = currencyListBO.MyCurrencyUsing(id);
            return myusing;
        }
    }
}