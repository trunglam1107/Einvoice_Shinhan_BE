using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceDeclarationReleaseRegisterTypeReposity : GenericRepository<INVOICEDECLARATIONRELEASEREGISTERTYPE>, IInvoiceDeclarationReleaseRegisterTypeReposity
    {
        public InvoiceDeclarationReleaseRegisterTypeReposity(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<INVOICEDECLARATIONRELEASEREGISTERTYPE> GetList()
        {
            return this.dbSet;
        }
    }
}
