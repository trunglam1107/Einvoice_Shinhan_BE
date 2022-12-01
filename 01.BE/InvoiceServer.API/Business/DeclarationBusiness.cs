using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;
using System;
using System.Web.Http;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using System.IO;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness'
    public class DeclarationBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness'
    {
        #region Fields, Properties

        private readonly IDeclarationBO DeclarationBO;
        private readonly IRegisterTemplatesBO templateCompanyUseReleaseBO;
        private readonly PrintConfig PrintConfig;
        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.DeclarationBusiness(IBOFactory)'
        public DeclarationBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.DeclarationBusiness(IBOFactory)'
        {
            var printConfig = GetPrintConfig();
            this.PrintConfig = printConfig;
            this.DeclarationBO = boFactory.GetBO<IDeclarationBO>(this.CurrentUser);
            this.templateCompanyUseReleaseBO = boFactory.GetBO<IRegisterTemplatesBO>(printConfig);
        }

        #endregion Contructor

        #region Methods
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.Fillter(out long, string, string, string, int, int, int)'
        public IEnumerable<DeclarationInfo> Fillter(out long totalRecords, string status = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue, int companyID = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.Fillter(out long, string, string, string, int, int, int)'
        {
            var condition = new ConditionSearchDeclaration(this.CurrentUser, status, orderby, orderType, companyID);
            totalRecords = this.DeclarationBO.Count(condition);
            return this.DeclarationBO.Filter(condition, skip, take);
        }
        public List<CompanySymbolInfo> ListSymbol(long companyId)
        {
            return this.DeclarationBO.ListSymbol(companyId);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetDeclarationInfo(long)'
        public DeclarationInfo GetDeclarationInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetDeclarationInfo(long)'
        {
            return this.DeclarationBO.GetDeclarationInfo(id,null);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetDeclarationLast(long?)'
        public DeclarationInfo GetDeclarationLast(long? companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetDeclarationLast(long?)'
        {
            return this.DeclarationBO.GetDeclarationLast(companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.Create(DeclarationInfo)'
        public ResultCode Create(DeclarationInfo DeclarationInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.Create(DeclarationInfo)'
        {
            if (DeclarationInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode result = this.DeclarationBO.Create(DeclarationInfo);
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.Update(long, DeclarationInfo)'
        public ResultCode Update(long id, DeclarationInfo DeclarationInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.Update(long, DeclarationInfo)'
        {
            if (DeclarationInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.DeclarationBO.Update(id, DeclarationInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.UpdateSymbol(long, DeclarationInfo)'
        public ResultCode UpdateSymbol(long id, DeclarationInfo DeclarationInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.UpdateSymbol(long, DeclarationInfo)'
        {
            if (DeclarationInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.DeclarationBO.UpdateSymbol(id, DeclarationInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.UpdateStatus(long, int?)'
        public ResultCode UpdateStatus(long id, int? status)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.UpdateStatus(long, int?)'
        {
            if (status == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            if (!Enum.IsDefined(typeof(DeclarationStatus), status))
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.DeclarationBO.UpdateStatus(id, status);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.Delete(long)'
        {
            return this.DeclarationBO.Delete(id,null);
        }


        private PrintConfig GetPrintConfig()
        {
            string fullPathFolderAsset = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.FolderAssetOfCompany);
            PrintConfig config = new PrintConfig(fullPathFolderAsset);
            config.BuildAssetByCompany(this.CurrentUser.Company);

            config.FullPathFolderTemplateDeclaration = 
                HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateDeClarationFolder);

            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetFileXML(long)'
        public ExportFileInfo GetFileXML(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetFileXML(long)'
        {
            return this.DeclarationBO.GetXmlFile(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetFileNotiXML()'
        public ExportFileInfo GetFileNotiXML()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetFileNotiXML()'
        {
            return this.DeclarationBO.GetXmlNotiFile();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetDeclarationSymbol(long?)'
        public DeclarationMaster GetDeclarationSymbol(long? companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetDeclarationSymbol(long?)'
        {
            
            return this.DeclarationBO.GetDeclarationSymbol(companyId);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.UpdateAfterSign(DeclarationInfo)'
        public ResultCode UpdateAfterSign(DeclarationInfo declarationInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.UpdateAfterSign(DeclarationInfo)'
        {
            return this.DeclarationBO.UpdateAfterSign(declarationInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.SignDeclaration(long)'
        public ResultCode SignDeclaration(long Id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.SignDeclaration(long)'
        {
            return this.DeclarationBO.SignDeclaration(Id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.CheckSendTwan(long)'
        public ResultCode CheckSendTwan(long Id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.CheckSendTwan(long)'
        {
             return this.DeclarationBO.CheckSendTwan(Id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.getFileSignXML(long)'
        public ExportFileInfo getFileSignXML(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.getFileSignXML(long)'
        {
           return this.DeclarationBO.getFileSignXML(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetRegisterTypes()'
        public IEnumerable<RegisterType> GetRegisterTypes()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.GetRegisterTypes()'
        {
            return this.DeclarationBO.GetRegisterTypes();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.Approve(long)'
        public string Approve(long invoiceDeclarationID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.Approve(long)'
        {
            return this.DeclarationBO.Approve(invoiceDeclarationID); 
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.RevertApprove(long)'
        public string RevertApprove(long invoiceDeclarationID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.RevertApprove(long)'
        {
            return this.DeclarationBO.RevertApprove(invoiceDeclarationID);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.ExportPdf(long)'
        public ExportFileInfo ExportPdf(long invoiceDeclarationID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.ExportPdf(long)'
        {
            return this.DeclarationBO.ExportPdf(invoiceDeclarationID, this.PrintConfig);          
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.MappDataSignatureInfo(HttpRequest)'
        public DeclarationReleaseInfo MappDataSignatureInfo(HttpRequest httpRequest)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.MappDataSignatureInfo(HttpRequest)'
        {
           

            var docfiles = new List<string>();
            var filenames = new List<string>();
            string Password = "";
          
            if (httpRequest.Files.Count > 0)
            {

                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    var filePath = CreateFullFileName(postedFile.FileName);
                    filenames.Add(postedFile.FileName);
                    postedFile.SaveAs(filePath);
                    docfiles.Add(filePath);
                }

            }

            if (httpRequest.Form.Count > 0)
            {
                Password = getFormDta("password", httpRequest);
             
            }

            var declarationReleaseInfo = new DeclarationReleaseInfo()
            {
                PassWord = Password,
                FileName = filenames.Count > 0 ? filenames[0] : null,
              
            };

            return declarationReleaseInfo;


        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.getFormDta(string, HttpRequest)'
        public string getFormDta(string name, HttpRequest httpRequest)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.getFormDta(string, HttpRequest)'
        {
            string listname = "";
            var namestring = httpRequest.Form[name];
            listname = namestring;

            return listname;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.CreateFullFileName(string)'
        public string CreateFullFileName(string fileName)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.CreateFullFileName(string)'
        {
            string pathFile = "";
            if (fileName != null)
            {
                string FolderPath = Path.Combine(CommonUtil.GetConfig("SourceFileInvoice"), "Certificate");
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                pathFile = string.Format(@"{0}\{1}", FolderPath, fileName);

            }
            return pathFile;


        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.UploadFile(long)'
        public DeclarationReleaseInfo UploadFile(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationBusiness.UploadFile(long)'
        {
            return this.DeclarationBO.UploadFile(companyId);
        }

        public ExportFileInfo DownloadExcelDeclare(ConditionSearchDeclaration condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness.DownloadExcelHRG(FilterHistoryReportGeneralCondition)'
        {
            ExportFileInfo fileInfo;
            fileInfo = this.DeclarationBO.DownloadExcelDeclare(condition);
            return fileInfo;
        }
        public ExportFileInfo DownloadExcelSymbol(long companyId)
        {
            ExportFileInfo fileInfo;
            fileInfo = this.DeclarationBO.DownloadExcelSymbol(companyId);
            return fileInfo;
        }

        #endregion
    }
}