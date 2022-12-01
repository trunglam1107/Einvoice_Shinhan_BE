using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.InvoiceSample_Type;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL.Interface
{
    public interface ICurrencyListBO
    {
        IEnumerable<CurrencyListViewModel> GetList();

        IEnumerable<CurrencyListViewModel> Filter(ConditionSearchInvoiceType condition);
        long Count(ConditionSearchInvoiceType condition);

        ResultCode Create(CurrencyListViewModel currencyListViewModel);

        ResultCode Update(string code, CurrencyListViewModel viewModel);

        ResultCode Delete(string code);
        CurrencyListViewModel GetByCode(ConditionSearchInvoiceType condition);

        bool MyCurrencyUsing(long currencyId);
    }
}
