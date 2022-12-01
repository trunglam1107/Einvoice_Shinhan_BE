using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.BL
{
    public interface IDeclarationBO
    {
        DeclarationInfo GetDeclarationInfo(long id, long? companyId);
        IEnumerable<DeclarationInfo> Filter(ConditionSearchDeclaration condition, int skip = 0, int take = int.MaxValue);
        long Count(ConditionSearchDeclaration condition);
        ResultCode Create(DeclarationInfo info);
        ResultCode Update(long id, DeclarationInfo info);

        ResultCode UpdateSymbol(long id, DeclarationInfo info);
        ResultCode UpdateStatus(long id, int? status);
        ResultCode Delete(long id, long? companyId);
        DeclarationMaster GetDeclarationSymbol(long? companyId);
        ExportFileInfo GetXmlFile(long id);
        string GetXmlNotifullPathFile();
        ExportFileInfo GetXmlNotiFile();
        ExportFileInfo GetXmlNotiFileById(long id, long companyId);
        ResultCode UpdateAfterSign(DeclarationInfo declarationInfo);
        ResultCode SignDeclaration(long id);
        string CallApiSignDecalaration(string pathXml, SIGNATURE signature, string tagKey = null);
        ResultCode CheckSendTwan(long id);
        SIGNATURE GetSignature(long companyId);
        ExportFileInfo getFileSignXML(long id);
        IEnumerable<RegisterType> GetRegisterTypes();
        string Approve(long INVOICEDECLARATIONID);
        string RevertApprove(long INVOICEDECLARATIONID);
        ExportFileInfo ExportPdf(long INVOICEDECLARATIONID, PrintConfig config);

        DeclarationInfo GetDeclarationLast(long? companyId);
        DeclarationReleaseInfo UploadFile(long companyId);

        ExportFileInfo DownloadExcelDeclare(ConditionSearchDeclaration condition);
        List<CompanySymbolInfo> ListSymbol(long companyId);
        ExportFileInfo DownloadExcelSymbol(long companyId);

        int GetExpireDateToken(long? companyId);

        DateTime ExpireDateToken(long? companyId);
    }
}
