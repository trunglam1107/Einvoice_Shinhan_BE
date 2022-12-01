using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface ISignatureBO
    {

        /// <summary>
        /// Search DataPermission by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>

        SIGNATURE GetSignature(SignatureInfo signatureInfo);
        string GetSerialnumber(long compnyID);
        SignatureInfo GetSignatureInfo(long id);
        ResultCode CreateOrUpdate(SignatureInfo signatureInfo);

        ResultCode Create(SignatureInfo signatureInfo);

        ResultCode Update(SignatureInfo signatureInfo);
        ResultCode Delete(long id);
        IEnumerable<SignatureInfo> GetList(long companyId);
        long CountFillterCA(ConditionSearchCA condition);
        IEnumerable<SignatureInfo> FilterCA(ConditionSearchCA condition, int skip = int.MinValue, int take = int.MaxValue);
        string GetSerialNumberByPassword(SignatureInfo fileInfo);
        bool GetTypeSign();
        string GetSerialBySlot(long slotId, string password);
        string GetSlots(string password);
        bool GetBySlot(int slot, string cert);
        bool GetByCompanyId(long companyId);
        SIGNATURE GetByCompany(long companyId);
        List<SIGNATURE> GetByCompanies(List<long> companyIds);
        ExportFileInfo DownloadDataSignature(ConditionSearchCA condition);
    }
}
