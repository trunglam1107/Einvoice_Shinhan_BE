using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace InvoiceServer.Data.DBAccessor
{
    public interface IDbContext : IDisposable
    {
        DbChangeTracker ChangeTracker { get; }
        IDbSet<T> Set<T>() where T : class;
        DbEntityEntry<T> Entry<T>(T t) where T : class;
        int SaveChanges();

        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}
