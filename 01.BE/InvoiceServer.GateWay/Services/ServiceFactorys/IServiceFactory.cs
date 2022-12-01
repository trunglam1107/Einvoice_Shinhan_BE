using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.GateWay.Services.ServiceFactorys
{
    public interface IServiceFactory
    {
        T GetService<T>(params object[] args) where T : class;
    }
}
