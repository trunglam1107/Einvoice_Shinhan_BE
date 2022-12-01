using InvoiceServer.Business.DAO;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InvoiceServer.Business.BL
{
    public class BOFactory : IBOFactory
    {
        #region Fields, Properties
        private static readonly Dictionary<Type, Type> _classMap = new Dictionary<Type, Type>();
        private readonly IRepositoryFactory repoFactory;

        #endregion

        #region Contructor

        public BOFactory(IDbContext context)
        {
            Ensure.Argument.ArgumentNotNull(context, "context");

            this.repoFactory = new RepositoryFactory(context);
        }

        #endregion

        #region Methods

        public T GetBO<T>(params object[] args)
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
            parameters.Add(this.repoFactory);
            parameters.AddRange(args);

            return (T)Activator.CreateInstance(implType, parameters.ToArray());
        }

        #endregion
    }
}
