using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class EmailActiveFileAttachRepository : GenericRepository<EMAILACTIVEFILEATTACH>, IEmailActiveFileAttachRepository
    {
        public EmailActiveFileAttachRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<EMAILACTIVEFILEATTACH> Filter(int emailActiveId)
        {
            var emailActives = this.dbSet.Where(p => p.EMAILACTIVEID == emailActiveId);
            return emailActives;
        }
    }
}
