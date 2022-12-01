
using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.GateWay.Services.GateWayLog
{
    public interface IGateWayLogService
    {
        bool Insert(GATEWAY_LOG entity);
    }
}
