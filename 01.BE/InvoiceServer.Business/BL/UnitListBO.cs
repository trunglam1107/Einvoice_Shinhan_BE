using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class UnitListBO : IUnitListBO
    {
        private readonly IUnitListRepository unitListRepository;

        public UnitListBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.unitListRepository = repoFactory.GetRepository<IUnitListRepository>();
        }
        public IEnumerable<UnitListViewModel> GetList(long CompanyId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UnitListViewModel> Filter(ConditionSearchUnitList condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var listUnit = this.unitListRepository.Filter(condition).AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
            var listUnitListViewModel = new List<UnitListViewModel>();
            if (listUnit.Any())
            {
                listUnit.ForEach(x => listUnitListViewModel.Add(
                   new UnitListViewModel()
                   {
                       Id = x.ID,
                       Code = x.CODE,
                       Name = x.NAME
                   }
                   ));
            }
            return listUnitListViewModel;
        }

        public UnitListViewModel GetByCode(ConditionSearchUnitList condition)
        {
            var currentUnitList = unitListRepository.GetByCode(condition.Key, condition.CompanyId ?? 0);
            if (currentUnitList != null)
            {
                return new UnitListViewModel()
                {
                    Id = currentUnitList.ID,
                    Code = currentUnitList.CODE,
                    Name = currentUnitList.NAME
                };
            }
            return new UnitListViewModel();
        }

        public bool MyUnitUsing(long unitId)
        {
            return unitListRepository.MyUnitUsing(unitId);
        }

        public long Count(ConditionSearchUnitList condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.unitListRepository.Filter(condition).Count();
        }

        public ResultCode Create(UnitListViewModel unitListViewModel)
        {
            if (IsExistedCode(unitListViewModel.Code, unitListViewModel.Name, true, unitListViewModel.CompanyId))
            {
                throw new BusinessLogicException(ResultCode.ExistedUnit, string.Format("Template [{0}/{1}] is existed", unitListViewModel.Code, unitListViewModel.Name));
            }
            var unitList = new UNITLIST()
            {
                CODE = unitListViewModel.Code,
                NAME = unitListViewModel.Name
            };
            var isUpdateSuccess = unitListRepository.Insert(unitList);
            if (isUpdateSuccess)
            {
                return ResultCode.NoError;
            }
            return ResultCode.UnitIssuedNotUpdate;
        }

        public ResultCode Update(string code, UnitListViewModel info)
        {
            var unitList = this.unitListRepository.GetByCode(info.Code, info.CompanyId);
            unitList.NAME = info.Name;

            var isUpdateSuccess = unitListRepository.Update(code, unitList);
            if (isUpdateSuccess)
            {
                return ResultCode.NoError;
            }
            return ResultCode.UnitIssuedNotUpdate;
        }
        private bool IsExistedCode(string code, string name, bool create, long companySID)
        {
            return this.unitListRepository.ContainCode(code, name, create, companySID);
        }
        public ResultCode Delete(string code, long companyId)
        {
            var result = unitListRepository.DeleteTypeTax(code, companyId);
            if (result)
            {
                return ResultCode.NoError;
            }

            return ResultCode.UnitIssuedNotUpdate;
        }
    }
}