using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace InvoiceServer.Business.DAO
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// Last SID in the table
        /// </summary>
        protected readonly static object lockObject = new object();
        protected readonly IDbContext context;
        protected readonly IDbSet<T> dbSet;

        public GenericRepository(IDbContext context)
        {
            Ensure.Argument.ArgumentNotNull(context, "context");

            this.context = context;
            this.dbSet = context.Set<T>();
        }

        public GenericRepository(IDbContext context, IDbSet<T> dbSet)
        {
            Ensure.Argument.ArgumentNotNull(context, "context");

            this.context = context;
            this.dbSet = dbSet;
        }

        public virtual IEnumerable<T> GetAll()
        {
            return this.dbSet;
        }

        public virtual IEnumerable<T> Find(
            Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query;
        }

        public virtual IEnumerable<T> Filter(
            out int total,
            Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            int index = 0,
            int size = 50)
        {

            int skipCount = index * size;
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            query = skipCount == 0 ? query.Take(size) : query.Skip(skipCount).Take(size);
            total = query.Count();
            return query;
        }

        public virtual T SingleOrDefault(
            Expression<Func<T, bool>> filter,
            params Expression<Func<T, object>>[] includes
            )
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query.SingleOrDefault();
        }

        public virtual T FirstOrDefault(
           Expression<Func<T, bool>> filter = null,
           params Expression<Func<T, object>>[] includes
           )
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query.FirstOrDefault();
        }

        public bool Contains(Expression<Func<T, bool>> predicate)
        {
            return dbSet.Count(predicate) > 0;
        }

        public virtual int Count(Expression<Func<T, bool>> predicate)
        {
            return dbSet.Count(predicate);
        }

        public virtual bool Insert(T entity)
        {
            int result = 0;
            try
            {
                lock (lockObject)
                {
                    dbSet.Add(entity);
                    result = context.SaveChanges();
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
            }

            return result > 0;
        }

        public virtual bool Delete(T entity)
        {
            lock (lockObject)
            {
                dbSet.Remove(entity);
                return context.SaveChanges() > 0;
            }
        }

        public virtual bool Update(T entity)
        {
            int result = 0;
            try
            {
                lock (lockObject)
                {
                    result = context.SaveChanges();
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
            }

            return result > 0;
        }
    }
}