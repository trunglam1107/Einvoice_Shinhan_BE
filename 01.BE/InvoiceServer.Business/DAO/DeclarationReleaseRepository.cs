using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceServer.Data.DBAccessor;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common;

namespace InvoiceServer.Business.DAO
{
    public class DeclarationReleaseRepository : GenericRepository<INVOICEDECLARATIONRELEASE>, IDeclarationReleaseRepository
    {
        private readonly DataClassesDataContext _dbSet = new DataClassesDataContext();
        public DeclarationReleaseRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<INVOICEDECLARATIONRELEASE> GetList(long id)
        {
            return GetDeclarationReleaseList(id);
        }
        public INVOICEDECLARATIONRELEASE GetById(long id, long? detailId)
        {
            return GetDeclarationReleaseList(id).FirstOrDefault(p => p.ID == detailId);
        }

        public IEnumerable<DeclarationReleaseInfo> FilterDeclarationRelease(long id)
        {
            var query = this.GetDeclarationReleaseList(id);
            var Declarations = query.AsQueryable().ToList();

            var DeclarationsMaster =
                from Declaration in Declarations
                select new DeclarationReleaseInfo(Declaration);

            var res = DeclarationsMaster.ToList();
            return res;

        }

        public int CountDeclarationRelease(long id)
        {
            var query = this.GetDeclarationReleaseList(id);
            return query.Count();
        }

        private IEnumerable<INVOICEDECLARATIONRELEASE> GetDeclarationReleaseList(long id)
        {
            var listDecla = dbSet.Where(P => P.DECLARATIONID == id);

            return listDecla;
        }

    }
}
