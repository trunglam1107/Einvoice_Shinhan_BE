using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public interface IDeclarationReleaseRepository : IRepository<INVOICEDECLARATIONRELEASE>
    {
        IEnumerable<INVOICEDECLARATIONRELEASE> GetList(long id);
        INVOICEDECLARATIONRELEASE GetById(long id, long? detailId);
        IEnumerable<DeclarationReleaseInfo> FilterDeclarationRelease(long id);
        int CountDeclarationRelease(long id);
    }
}
