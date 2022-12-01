using InvoiceServer.Business.BL.Interface;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.DAO.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.InvoiceSample_Type;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class CurrencyListBO : ICurrencyListBO
    {
        private readonly ICurrencyRepository currencyListRepository;

        public CurrencyListBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.currencyListRepository = repoFactory.GetRepository<ICurrencyRepository>();
        }

        /// <summary>
        /// GetList
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <returns>IEnumerable<CurrencyListViewModel></returns>
        public IEnumerable<CurrencyListViewModel> GetList()
        {
            var listCurrency = currencyListRepository.GetAll();
            var listCurrencyViewModel = new List<CurrencyListViewModel>();
            if (listCurrency.Any())
            {
                listCurrency.ForEach(x => listCurrencyViewModel.Add(
                   new CurrencyListViewModel()
                   {
                       Id = x.ID,
                       Code = x.CODE,
                       Name = x.NAME,
                       Exchangerate = x.EXCHANGERATE
                   }
                   ));
            }
            return listCurrencyViewModel;

        }

        /// <summary>
        /// Filter
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns>IEnumerable<CurrencyListViewModel></returns>
        public IEnumerable<CurrencyListViewModel> Filter(ConditionSearchInvoiceType condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var listCurrency = this.currencyListRepository.Filter(condition).AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
            var listCurrencyViewModel = new List<CurrencyListViewModel>();
            if (listCurrency.Any())
            {
                listCurrency.ForEach(x => listCurrencyViewModel.Add(
                   new CurrencyListViewModel()
                   {
                       Id = x.ID,
                       Code = x.CODE,
                       Name = x.NAME,
                       Exchangerate = x.EXCHANGERATE,
                       DecimalSeparator = x.DECIMALSEPARATOR,
                       DecimalUnit = x.DECIMALUNIT,
                   }
                   ));
            }
            return listCurrencyViewModel;
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="condition"></param>
        /// <returns>long</returns>
        public long Count(ConditionSearchInvoiceType condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.currencyListRepository.Filter(condition).Count();
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="currencyListViewModel"></param>
        /// <returns>ResultCode</returns>
        public ResultCode Create(CurrencyListViewModel currencyListViewModel)
        {
            if (IsExistedCode(currencyListViewModel.Code, true))
            {
                throw new BusinessLogicException(ResultCode.ExistedCurrencyCode, string.Format("Template [{0}] is existed", currencyListViewModel.Code));
            }
            var currencyList = new CURRENCY()
            {
                CODE = currencyListViewModel.Code,
                NAME = currencyListViewModel.Name,
                EXCHANGERATE = currencyListViewModel.Exchangerate,
                DECIMALSEPARATOR = currencyListViewModel.DecimalSeparator,
                DECIMALUNIT = currencyListViewModel.DecimalUnit,
            };
            var isCreateSuccess = currencyListRepository.Insert(currencyList);
            if (isCreateSuccess)
            {
                return ResultCode.NoError;
            }
            return ResultCode.InvoiceIssuedNotCreate;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="code"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public ResultCode Update(string code, CurrencyListViewModel viewModel)
        {

            var currencyList = this.currencyListRepository.GetByCode(viewModel.Code);
            currencyList.NAME = viewModel.Name;
            currencyList.EXCHANGERATE = viewModel.Exchangerate;
            currencyList.DECIMALSEPARATOR = viewModel.DecimalSeparator;
            currencyList.DECIMALUNIT = viewModel.DecimalUnit;

            var isUpdateSuccess = currencyListRepository.Update(code, currencyList);
            if (isUpdateSuccess)
            {
                return ResultCode.NoError;
            }
            return ResultCode.InvoiceIssuedNotUpdate;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="code"></param>
        /// <returns>ResultCode</returns>
        public ResultCode Delete(string code)
        {
            var result = currencyListRepository.Delete(code);
            if (result)
            {
                return ResultCode.NoError;
            }

            return ResultCode.InvoiceIssuedNotDelete;
        }

        /// <summary>
        /// GetByCode
        /// </summary>
        /// <param name="condition"></param>
        /// <returns>CurrencyListViewModel</returns>
        public CurrencyListViewModel GetByCode(ConditionSearchInvoiceType condition)
        {
            var currentCurrencyList = currencyListRepository.GetByCode(condition.Key);
            if (currentCurrencyList != null)
            {
                return new CurrencyListViewModel()
                {
                    Id = currentCurrencyList.ID,
                    Code = currentCurrencyList.CODE,
                    Name = currentCurrencyList.NAME,
                    Exchangerate = currentCurrencyList.EXCHANGERATE,
                    DecimalSeparator = currentCurrencyList.DECIMALSEPARATOR,
                    DecimalUnit = currentCurrencyList.DECIMALUNIT,
                };
            }
            return new CurrencyListViewModel();
        }

        public bool MyCurrencyUsing(long currencyId)
        {
            return currencyListRepository.MyCurrencyUsing(currencyId);
        }
        private bool IsExistedCode(string code, bool create)
        {
            return this.currencyListRepository.ContainCode(code, create);
        }
    }
}
