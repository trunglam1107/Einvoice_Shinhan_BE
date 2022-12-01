
using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.GateWay.Services.MinVoice
{
    public interface IMinvoiceService
    {
        bool Insert(MINVOICE_DATA entity);
        bool Update(MINVOICE_DATA entity);
    }
}
