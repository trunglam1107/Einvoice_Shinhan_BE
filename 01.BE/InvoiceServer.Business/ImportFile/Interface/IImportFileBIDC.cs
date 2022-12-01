using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Xml;

namespace InvoiceServer.Business.DAO
{
    public interface IImportFileDAO : IRepository<INVOICE>
    {
        void ImportFileAuto_BIDC(bool isJob);
        BIDCImportFromAPIOutput BIDCImportInvoiceExternalApi(XmlDocument xmlDocument);
        void SendMailHuanan();
        void ImportFileAuto_Sino(bool isJob);
        void ImportFileAuto_Mega(bool isJob);
    }
}
