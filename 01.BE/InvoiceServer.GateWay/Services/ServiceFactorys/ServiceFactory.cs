using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.GateWay.Services.ServiceFactorys
{
    public class ServiceFactory : IServiceFactory
    {
        #region Fields, Properties
        private static Dictionary<Type, Type> _classMap = new Dictionary<Type, Type>();
        private readonly IDbContext context;

        #endregion

        #region Contructor

        public ServiceFactory(IDbContext context)
        {
            Ensure.Argument.NotNullArg(context, "context");
            this.context = context;
        }

        #endregion

        public T GetService<T>(params object[] args)
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
            return (T)Activator.CreateInstance(implType, parameters.ToArray());
        }
    }
}
