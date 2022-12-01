using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.DAO.Interface
{
    public interface ICompanySymbolRepository : IRepository<SYMBOL>
    {
        IEnumerable<SYMBOL> GetList(long id);
        SYMBOL GetById(long id, long? detailId);
        IEnumerable<CompanySymbolInfo> FilterComSymbol(long id);
        int CountComSymbol(long id);

        bool DeleteCompanySymbol(long companyId);
    }
}
