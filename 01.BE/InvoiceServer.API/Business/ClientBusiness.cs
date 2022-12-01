using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness'
    public class ClientBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness'
    {
        #region Fields, Properties

        private readonly IClientBO clientUseBO;
        private readonly IEmailActiveBO emailActiveBO;
        #endregion Fields, Properties

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.ClientBusiness(IBOFactory)'
        public ClientBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.ClientBusiness(IBOFactory)'
        {
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };
            PrintConfig printConfig = GetPrintConfig();
            this.clientUseBO = boFactory.GetBO<IClientBO>(printConfig, emailConfig);
            this.emailActiveBO = boFactory.GetBO<IEmailActiveBO>(printConfig);

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.FillterClient(ConditionSearchClient)'
        public List<ClientDetail> FillterClient(ConditionSearchClient condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.FillterClient(ConditionSearchClient)'
        {
            condition.TotalRecords = this.clientUseBO.Count(condition);
            return this.clientUseBO.Filter(condition) as List<ClientDetail>;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.FillterClientByCIF(ConditionSearchClient)'
        public List<ClientDetail> FillterClientByCIF(ConditionSearchClient condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.FillterClientByCIF(ConditionSearchClient)'
        {
            condition.TotalRecords = this.clientUseBO.CountByCIF(condition);
            return this.clientUseBO.FilterByCIF(condition);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.GetClientDetail(long)'
        public ClientAddInfo GetClientDetail(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.GetClientDetail(long)'
        {
            var client = this.clientUseBO.GetClientInfo(id);
            return client;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.Create(ClientAddInfo)'
        public ResultCode Create(ClientAddInfo clientInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.Create(ClientAddInfo)'
        {
            ResultCode result;
            if (clientInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            clientInfo.UserAction = this.CurrentUser.Id;
            long? companyId = this.CurrentUser.Company.Id;
            result = this.clientUseBO.Create(clientInfo, companyId);
            result = this.clientUseBO.CreateResult(clientInfo, result, this.CurrentUser);
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.Update(long, ClientAddInfo)'
        public ResultCode Update(long id, ClientAddInfo clientInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.Update(long, ClientAddInfo)'
        {
            ResultCode result;
            if (clientInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var isExistLoginUser = this.clientUseBO.CheckExistLoginUser(id);
            clientInfo.UserAction = this.CurrentUser.Id;
            long? companyId = this.CurrentUser.Company.Id;
            result = this.clientUseBO.Update(id, clientInfo, companyId);

            if (!isExistLoginUser && result == ResultCode.NoError && clientInfo.CustomerType == 1)
            {
                var clientEmail = clientInfo.UseRegisterEmail != true ? clientInfo.ReceivedInvoiceEmail : clientInfo.Email;
                result = ReturnResult(result, clientEmail, clientInfo);
            }

            return result;
        }

        private ResultCode ReturnResult(ResultCode result, string clientEmail, ClientAddInfo clientInfo)
        {
            if (!string.IsNullOrEmpty(clientEmail))
            {
                result = this.emailActiveBO.SendEmailProvideAccount(clientInfo);
                if (result == ResultCode.NoError)
                {
                    //encrypt password after send email account infomation
                    result = this.clientUseBO.EncryptUserPassword(clientInfo.Id);
                }
            }
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.Delete(long)'
        {
            long? companyId = this.CurrentUser.Company.Id;
            return this.clientUseBO.Delete(id, companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.ImportData(FileImport)'
        public ResultImportSheet ImportData(FileImport fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.ImportData(FileImport)'
        {
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }

            string fullPathfile = SaveFileImport(fileImport);
            long companyId = GetCompanyIdOfUser();

            return this.clientUseBO.ImportData(fullPathfile, companyId);
        }

        private static string SaveFileImport(FileImport fileImport)
        {
            string FolderPath = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.UploadFolderImport);
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string fileName = string.Format(@"{0}\{1}.xlsx", FolderPath, DateTime.Now.ToString("ddMMyyyy"));
            try
            {
                File.WriteAllBytes(fileName, Convert.FromBase64String(fileImport.Data));
            }
            catch (Exception ex)
            {

                throw new BusinessLogicException(ResultCode.FileNotFound, ex.Message);
            }

            return fileName;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.DownloadDataClients(ConditionSearchClient)'
        public ExportFileInfo DownloadDataClients(ConditionSearchClient condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.DownloadDataClients(ConditionSearchClient)'
        {
            ExportFileInfo fileInfo = this.clientUseBO.DownloadDataClients(condition);

            return fileInfo;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.ExportExcel(string, string, string)'
        public ExportFileInfo ExportExcel(string branch, string userid,string branchId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.ExportExcel(string, string, string)'
        {

            return this.clientUseBO.ExportExcel(this.CurrentUser.Company, branch, userid, branchId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.MyClientUsing(long)'
        public MyUsingModel MyClientUsing(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.MyClientUsing(long)'
        {
            var myusing = new MyUsingModel();
            myusing.isUsing = clientUseBO.MyClientUsing(id);
            return myusing;
        }
        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.ImportClient()'
        public ResultCode ImportClient()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.ImportClient()'
        {
            this.clientUseBO.ImportClientFile();
            return ResultCode.ImportClientFileSuccess;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.ClientListSuggestion(ConditionSearchClient)'
        public List<ClientDetail> ClientListSuggestion(ConditionSearchClient condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientBusiness.ClientListSuggestion(ConditionSearchClient)'
        {
            condition.TotalRecords = this.clientUseBO.Count(condition);
            return this.clientUseBO.ClientListSuggestion(condition);
        }
        #endregion
    }
}