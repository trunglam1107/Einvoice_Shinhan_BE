using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InvoiceServer.Business.DAO
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private static readonly Dictionary<Type, Type> _classMap = new Dictionary<Type, Type>();
        private readonly IDbContext context;

        public RepositoryFactory(IDbContext context)
        {
            Ensure.Argument.ArgumentNotNull(context, "context");
            this.context = context;
        }

        public T GetRepository<T>(params object[] args)
            where T : class
        {
            Type returnType = typeof(T);
            Type implType = null;

            if (_classMap.ContainsKey(returnType) && _classMap[returnType] != null)
            {
                implType = _classMap[returnType];
            }
            else
            {
                var types = Assembly.GetExecutingAssembly().GetTypes();
                implType = types.FirstOrDefault(t => t.IsClass && returnType.IsAssignableFrom(t));

                if (implType != null)
                {
                    _classMap[returnType] = implType;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            List<object> parameters = new List<object>();
            parameters.Add(this.context);
            if (args != null && args.Length > 0)
            {
                parameters.AddRange(args);
            }

            return (T)Activator.CreateInstance(implType, parameters.ToArray());
        }

        public IDbTransactionManager GetTransactionManager()
        {
            return GetRepository<IDbTransactionManager>();
        }
    }
}
