using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public  interface IDeclarationRepository:IRepository<INVOICEDECLARATION>
    {
        IEnumerable<INVOICEDECLARATION> GetList();
        IQueryable<DeclarationMaster> GetListSymboy(long? companyId);
        INVOICEDECLARATION GetById(long id);
        IEnumerable<DeclarationInfo> FilterDeclaration(ConditionSearchDeclaration condition, int skip = 0, int take = int.MaxValue);
        int CountDeclaration(ConditionSearchDeclaration condition);
        ResultCode CheckSendTwan(long id);

        INVOICEDECLARATION GetLastInfo(long? CompanyID);

        int GetDeclarationUseRegisterTemplate(long id);

        IQueryable<SYMBOL> GetbyRefId(long id);

        DateTime? GetMaxDateInvoice(long? companyId, string symbol);

        IEnumerable<DeclarationInfo> FilterDeclarationForExcel(ConditionSearchDeclaration condition);
        List<CompanySymbolInfo> ListSymbol(long companyId);

        DateTime ExpireTokenDate(long? companyId);

        int GetExpireDateToken(long? companyId);

    }
}
