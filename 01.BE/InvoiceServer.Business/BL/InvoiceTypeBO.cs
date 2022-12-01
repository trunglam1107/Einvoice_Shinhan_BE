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
    public class InvoiceTypeBO : IInvoiceTypeBO
    {
        private readonly IInvoiceTypeRepository invoiceTypeRepository;

        public InvoiceTypeBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.invoiceTypeRepository = repoFactory.GetRepository<IInvoiceTypeRepository>();
        }
        public IEnumerable<InvoiceTypeViewModel> GetList(long CompanyId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<InvoiceTypeViewModel> Filter(ConditionSearchInvoiceType condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var listInvoiceSample = this.invoiceTypeRepository.Filter(condition).AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
            var listInvoiceTypeViewModel = new List<InvoiceTypeViewModel>();
            if (listInvoiceSample.Any())
            {
                listInvoiceSample.ForEach(x => listInvoiceTypeViewModel.Add(
                   new InvoiceTypeViewModel()
                   {
                       Id = x.ID,
                       Code = x.CODE,
                       Note = x.NOTE,
                       Name = x.NAME,
                       Denominator = x.DENOMINATOR,
                   }
                   ));
            }
            return listInvoiceTypeViewModel;
        }

        public InvoiceTypeViewModel GetByCode(string code)
        {
            var currentInvoiceType = invoiceTypeRepository.GetByCode(code);
            if (currentInvoiceType != null)
            {
                return new InvoiceTypeViewModel()
                {
                    Id = currentInvoiceType.ID,
                    Code = currentInvoiceType.CODE,
                    Name = currentInvoiceType.NAME,
                    Denominator = currentInvoiceType.DENOMINATOR,
                    Note = currentInvoiceType.NOTE
                };
            }
            return new InvoiceTypeViewModel();
        }

        public long Count(ConditionSearchInvoiceType condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.invoiceTypeRepository.Filter(condition).Count();
        }

        public ResultCode Create(InvoiceTypeViewModel invoiceTypeViewModel)
        {
            if (IsExisted(invoiceTypeViewModel.Code, invoiceTypeViewModel.Name, invoiceTypeViewModel.Denominator, true))
            {
                throw new BusinessLogicException(ResultCode.ExistedInvoiceType, string.Format("Template [{0}/{1}] is existed", invoiceTypeViewModel.Code, invoiceTypeViewModel.Name));
            }
            var invoiceType = new INVOICETYPE()
            {
                CODE = invoiceTypeViewModel.Code,
                NAME = invoiceTypeViewModel.Name,
                NOTE = invoiceTypeViewModel.Note,
                DENOMINATOR = invoiceTypeViewModel.Denominator,
            };
            var isUpdateSuccess = invoiceTypeRepository.Insert(invoiceType);
            if (isUpdateSuccess)
            {
                return ResultCode.NoError;
            }
            return ResultCode.InvoiceIssuedNotUpdate;
        }
        private bool IsExisted(string code, string name, string denominator, bool create)
        {
            return this.invoiceTypeRepository.ContainCode(code, name, denominator, create);
        }

        public ResultCode Update(string code, InvoiceTypeViewModel invoiceTypeViewModel)
        {
            var invoiceType = new INVOICETYPE()
            {
                ID = invoiceTypeViewModel.Id,
                CODE = invoiceTypeViewModel.Code,
                NAME = invoiceTypeViewModel.Name,
                NOTE = invoiceTypeViewModel.Note,
                DENOMINATOR = invoiceTypeViewModel.Denominator,
            };
            var isUpdateSuccess = invoiceTypeRepository.Update(code, invoiceType);
            if (isUpdateSuccess)
            {
                return ResultCode.NoError;
            }
            return ResultCode.InvoiceIssuedNotUpdate;
        }

        public ResultCode Delete(string code)
        {
            var result = invoiceTypeRepository.DeleteInvoiceType(code);
            if (result)
            {
                return ResultCode.NoError;
            }

            return ResultCode.InvoiceIssuedNotUpdate;
        }

        public bool MyInvoiceTypeUsing(long invoiceTypeId)
        {
            return invoiceTypeRepository.MyInvoiceTypeUsing(invoiceTypeId);
        }
    }
}