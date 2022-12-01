using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness'
    public class UnitListBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness'
    {
        private readonly IUnitListBO unitListBO;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.UnitListBusiness(IBOFactory)'
        public UnitListBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.UnitListBusiness(IBOFactory)'
        {
            this.unitListBO = boFactory.GetBO<IUnitListBO>();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.Fillter(ConditionSearchUnitList)'
        public IEnumerable<UnitListViewModel> Fillter(ConditionSearchUnitList condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.Fillter(ConditionSearchUnitList)'
        {
            condition.TotalRecords = this.unitListBO.Count(condition);
            return this.unitListBO.Filter(condition);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.GetByCode(ConditionSearchUnitList)'
        public UnitListViewModel GetByCode(ConditionSearchUnitList condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.GetByCode(ConditionSearchUnitList)'
        {
            if (condition.Key == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var currnetTaxTypeVm = this.unitListBO.GetByCode(condition);
            if (currnetTaxTypeVm != null)
            {
                return currnetTaxTypeVm;
            }
            return new UnitListViewModel();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.Create(UnitListViewModel)'
        public ResultCode Create(UnitListViewModel unitListViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.Create(UnitListViewModel)'
        {
            if (unitListViewModel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            unitListViewModel.CompanyId = this.CurrentUser.Company.Id ?? 0;
            return ResultCode.NoError;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.Update(string, UnitListViewModel)'
        public ResultCode Update(string code, UnitListViewModel unitListViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.Update(string, UnitListViewModel)'
        {
            if (unitListViewModel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            long idCompany = GetCompanyIdOfUser();
            unitListViewModel.CompanyId = idCompany;
            return this.unitListBO.Update(code, unitListViewModel);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.Delete(string)'
        public ResultCode Delete(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.Delete(string)'
        {
            long companyId = GetCompanyIdOfUser();
            return this.unitListBO.Delete(code, companyId);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.MyUnitUsing(long)'
        public MyUsingModel MyUnitUsing(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListBusiness.MyUnitUsing(long)'
        {
            var myusing = new MyUsingModel();
            myusing.isUsing = unitListBO.MyUnitUsing(id);
            return myusing;
        }
    }
}