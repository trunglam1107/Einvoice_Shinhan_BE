using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public class EmailActiveReferenceRepository : GenericRepository<EMAILACTIVEREFERENCE>, IEmailActiveReferenceRepository
    {
        public EmailActiveReferenceRepository(IDbContext context)
            : base(context)
        {
        }

        public void AddRange(List<EMAILACTIVEREFERENCE> emailActiveReferences)
        {
            var dbContext = this.context as DataClassesDataContext;
            dbContext.BulkInsert(emailActiveReferences, bulk =>
            {
                bulk.SkipPropagationAndAcceptChanges = false;
                bulk.BatchSize = 1000;
                bulk.BatchTimeout = 3600;
            });
        }
    }
}
