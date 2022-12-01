using InvoiceServer.Business.BL;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.DAO.Interface
{
    public interface IInvoiceReplaceRepository : IRepository<INVOICEREPLACE>
    {
        IQueryable<INVOICEREPLACE> GetInvoiceReplaceByCompany(long companyId);

        IQueryable<InvoiceReplace> GetAllInvoiceReplace();

        IQueryable<InvoiceReplace> GetAllInvoiceReplaceMonthly();
    }
}
