using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness'
    public class HoldInvoiceBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness'
    {
        private readonly IHoldInvoiceBO holdInvoiceBO;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.HoldInvoiceBusiness(IBOFactory)'
        public HoldInvoiceBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.HoldInvoiceBusiness(IBOFactory)'
        {
            this.holdInvoiceBO = boFactory.GetBO<IHoldInvoiceBO>(this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.Fillter(out long, ConditionSearchHoldInvoice, int, int)'
        public IEnumerable<HoldInvoiceViewModel> Fillter(out long totalRecords, ConditionSearchHoldInvoice condition, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.Fillter(out long, ConditionSearchHoldInvoice, int, int)'
        {
            totalRecords = this.holdInvoiceBO.Count(condition);
            return this.holdInvoiceBO.Filter(condition, skip, take);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.Create(HoldInvoiceViewModel)'
        public ResultCode Create(HoldInvoiceViewModel holdInvoiceViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.Create(HoldInvoiceViewModel)'
        {
            if (holdInvoiceViewModel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            holdInvoiceViewModel.HoldInvoiceDetails = new List<HoldInvoiceDetailViewModel>();
            int numberFrom = 0;
            int numberTo = 0;
            if (!int.TryParse(holdInvoiceViewModel.NumberFrom, out numberFrom) || !int.TryParse(holdInvoiceViewModel.NumberTo, out numberTo))
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            for (int i = numberFrom; i <= numberTo; i++)
            {
                holdInvoiceViewModel.HoldInvoiceDetails.Add(new HoldInvoiceDetailViewModel()
                {
                    Id = 0,
                    InvoiceNo = (i.ToString()).PadLeft(7, '0'),
                    InvoiceStatus = 0,
                });
            }

            holdInvoiceViewModel.CompanyId = this.CurrentUser.Company.Id ?? 0;
            return ResultCode.NoError;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.GetById(long)'
        public HoldInvoiceViewModel GetById(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.GetById(long)'
        {
            var holdInvoicevVm = this.holdInvoiceBO.GetById(id);
            if (holdInvoicevVm != null)
            {
                return holdInvoicevVm;
            }
            return new HoldInvoiceViewModel();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.Update(HoldInvoiceViewModel)'
        public ResultCode Update(HoldInvoiceViewModel holdInvoiceViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.Update(HoldInvoiceViewModel)'
        {
            if (holdInvoiceViewModel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            long idCompany = GetCompanyIdOfUser();
            holdInvoiceViewModel.CompanyId = idCompany;
            return this.holdInvoiceBO.Update(holdInvoiceViewModel);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceBusiness.Delete(long)'
        {
            return this.holdInvoiceBO.Delete(id);
        }
    }
}