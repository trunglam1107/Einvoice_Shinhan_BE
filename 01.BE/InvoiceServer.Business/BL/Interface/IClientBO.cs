using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IClientBO
    {
        /// <summary>
        /// Add new client
        /// </summary>
        /// <param name="client">client info</param>
        /// <returns></returns>
        ResultCode Create(ClientAddInfo client, long? companyId);

        /// <summary>
        /// Update client info
        /// </summary>
        /// <param name="client">client info to update</param>
        /// <returns></returns>
        ResultCode Update(long id, ClientAddInfo client, long? companyId);

        /// <summary>
        /// Delete Client By Update Deleted Flag = true
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        ResultCode Delete(long id, long? companyId);

        /// <summary>
        /// Get the client info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        ClientAddInfo GetClientInfo(long id);

        /// <summary>
        /// Search Clients by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<ClientDetail> Filter(ConditionSearchClient condition);

        /// <summary>
        /// Search Clients by CIF
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        List<ClientDetail> FilterByCIF(ConditionSearchClient condition);

        /// <summary>
        /// Count number clients by condition
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        long Count(ConditionSearchClient condition);

        long CountByCIF(ConditionSearchClient condition);

        bool MyClientUsing(long id);
        ResultImportSheet ImportData(string fullPathFile, long companyId);
        ExportFileInfo DownloadDataClients(ConditionSearchClient condition);

        long CreateEmailAccountInfo(CLIENT client, long? companyId);

        long CreateEmailActiveByClientId(long clientId, long? companyId = 0);

        ResultCode EncryptUserPassword(long clientId);

        bool CheckExistLoginUser(long clientId);

        ExportFileInfo ExportExcel(CompanyInfo company, string branch, string userid, string branchId);

        ResultCode ImportClientFile();

        ResultCode CreateResult(ClientAddInfo clientInfo, ResultCode result, UserSessionInfo CurrentUser);

        List<ClientDetail> ClientListSuggestion(ConditionSearchClient condition);

        bool UpdateEmailActive(long clientId, long emailActiveId);
    }
}
