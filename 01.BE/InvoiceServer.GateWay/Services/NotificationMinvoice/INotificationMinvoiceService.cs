
using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.GateWay.Services.NotificationMinvoice
{
    public interface INotificationMinvoiceService
    {
        bool Insert(NOTIFICATIONMINVOICE entity);
        bool Update(NOTIFICATIONMINVOICE entity);
    }
}
