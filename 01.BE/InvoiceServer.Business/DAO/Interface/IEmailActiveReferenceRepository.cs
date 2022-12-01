using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IEmailActiveReferenceRepository : IRepository<EMAILACTIVEREFERENCE>
    {
        void AddRange(List<EMAILACTIVEREFERENCE> emailActiveReferences);
    }
}
