using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace InvoiceServer.Business.DAO
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T SingleOrDefault(Expression<Func<T, bool>> filter,
            params Expression<Func<T, object>>[] includes);

        T FirstOrDefault(
           Expression<Func<T, bool>> filter = null,
           params Expression<Func<T, object>>[] includes);

        IEnumerable<T> Find(Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            params Expression<Func<T, object>>[] includes);
        bool Insert(T entity);
        bool Update(T entity);
        bool Delete(T entity);
        bool Contains(Expression<Func<T, bool>> predicate);
    }
}
