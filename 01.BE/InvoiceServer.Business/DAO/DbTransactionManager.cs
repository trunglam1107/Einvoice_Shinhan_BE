using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public class DbTransactionManager : IDbTransactionManager
    {
        private readonly IDbContext context;

        public DbTransactionManager(IDbContext context)
        {
            Ensure.Argument.ArgumentNotNull(context, "context");

            this.context = context;
        }

        public void BeginTransaction()
        {
            this.context.BeginTransaction();
        }

        public void Commit()
        {
            this.context.Commit();
        }

        public void Rollback()
        {
            this.context.Rollback();
        }
    }
}
