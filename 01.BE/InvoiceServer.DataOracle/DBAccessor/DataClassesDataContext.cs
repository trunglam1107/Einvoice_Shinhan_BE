using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace InvoiceServer.Data.DBAccessor
{
    public partial class DataClassesDataContext : DbContext, IDbContext
    {
        private DbContextTransaction transaction;

        IDbSet<T> IDbContext.Set<T>()
        {
            return base.Set<T>();
        }

        int IDbContext.SaveChanges()
        {
            return base.SaveChanges();
        }

        public DataClassesDataContext(string sqlConnection)
            : base(sqlConnection)
        {
        }

        DbEntityEntry<T> IDbContext.Entry<T>(T t)
        {
            return base.Entry<T>(t);
        }

        public void BeginTransaction()
        {
            this.transaction = this.Database.BeginTransaction();
        }

        public void Commit()
        {
            if (this.transaction != null)
            {
                this.transaction.Commit();
                this.transaction = null;
            }
        }

        public void Rollback()
        {
            if (this.transaction != null)
            {
                this.transaction.Rollback();
                this.transaction = null;
            }
        }
    }
}
