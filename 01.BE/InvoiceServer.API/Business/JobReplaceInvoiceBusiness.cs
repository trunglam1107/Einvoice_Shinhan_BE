using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceBusiness'
    public class JobReplaceInvoiceBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceBusiness'
    {
        private static readonly Logger logger = new Logger();
        private readonly IJobReplacedInvoice jobReplacedInvoice;
        private readonly PrintConfig printConfig;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceBusiness.JobReplaceInvoiceBusiness(IBOFactory)'
        public JobReplaceInvoiceBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceBusiness.JobReplaceInvoiceBusiness(IBOFactory)'
        {
            if (HttpContext.Current != null)
            {
                printConfig = GetPrintConfig();
            }
            else
            {
                printConfig = GetPrintConfigbyJob();
            }
            this.jobReplacedInvoice = boFactory.GetBO<IJobReplacedInvoice>(printConfig, this.CurrentUser);
        }
        private PrintConfig GetPrintConfigbyJob()
        {
            PrintConfig config = new PrintConfig(null, null);
            config.FullPathFolderTemplateInvoice = null;
            return config;
        }
        private PrintConfig GetPrintConfig()
        {
            string fullPathFileAsset = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig config = new PrintConfig(fullPathFileAsset, fullPathFileInvoice);
            try
            {
                if (this.CurrentUser.Company != null)
                {
                    config.BuildAssetByCompany(this.CurrentUser.Company);
                }
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceBusiness.ReplaceInvoice()'
        public ResultCode ReplaceInvoice()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceBusiness.ReplaceInvoice()'
        {
            try
            {
                Httpcontext.Instance.getHttpcontext();
                this.jobReplacedInvoice.ReplaceInvoice();
            }
            catch (Exception ex)
            {
                logger.Error("Mr Job", ex);
            }
            return ResultCode.NoError;
        }

        public bool ReplaceInvoiceNew()
        {
            try
            {
                Httpcontext.Instance.getHttpcontext();
                var result = this.jobReplacedInvoice.ReplaceInvoice_new();
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Mr Job", ex);
                return false;
            }
        }

        public ResultCode MultipleInvoiceCancel(CancellingInvoiceMaster deleteInfo)
        {
            //this.invoiceDeleteBO.MultipleInvoiceCancel(deleteInfo);
            this.jobReplacedInvoice.MultipleInvoiceCancel(deleteInfo);
            return ResultCode.NoError;
        }

        public ResultCode MultipleInvoiceAdjusted(CancellingInvoiceMaster deleteInfo)
        {
            this.jobReplacedInvoice.MultipleInvoiceAdjusted(deleteInfo);
            return ResultCode.NoError;
        }

        public ResultImportSheet ImportData(UploadInvoice fileImport)
        {
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }
            string fullPathfile = SaveFileImport(fileImport);
            long companyId = GetCompanyIdOfUser();
            //var task = Task.Run(() => );
            ResultImportSheet checkImport = this.jobReplacedInvoice.ImportData(fileImport, fullPathfile, companyId);
            //task.Wait(5000);
            var listResultImport = new ResultImportSheet();

            if (checkImport.RowError.Count > 0)
            {
                listResultImport.ErrorCode = ResultCode.ImportInvoiceCancelFileError;
                listResultImport.Message = "Cancel Invoice Error";
            }
            else
            {
                listResultImport.ErrorCode = ResultCode.ImportInvoiceCancelFileSuccess;
                listResultImport.Message = "Cancel Invoice Success";
            }

            //if (task.Status == TaskStatus.Running)
            //{

            //}

            return listResultImport;
            //return task.Result;
            //return this.invoiceBO.ImportData(fileImport, fullPathfile, companyId);
        }

        public ResultImportSheet ImportDataInvoiceAdjust(UploadInvoice fileImport)
        {
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }
            string fullPathfile = SaveFileImport(fileImport);
            long companyId = GetCompanyIdOfUser();
            //var task = Task.Run(() => );
            ResultImportSheet checkImport = this.jobReplacedInvoice.ImportDataInvoiceAdjusted(fileImport, fullPathfile, companyId);
            //task.Wait(5000);
            var listResultImport = new ResultImportSheet();

            if (checkImport.RowError.Count > 0)
            {
                listResultImport.ErrorCode = ResultCode.ImportInvoiceAdjustedFileError;
                listResultImport.Message = "Import Adjusted Invoice Error";
            }
            else
            {
                listResultImport.ErrorCode = ResultCode.ImportInvoiceAdjustedFileSuccess;
                listResultImport.Message = "Import Adjusted Invoice Success";
            }

            //if (task.Status == TaskStatus.Running)
            //{

            //}

            return listResultImport;
            //return task.Result;
            //return this.invoiceBO.ImportData(fileImport, fullPathfile, companyId);
        }

        private static string SaveFileImport(UploadInvoice fileImport)
        {
            string FolderPath = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.UploadFolderImport);
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string fileName = string.Format(@"{0}\Invoice{1}.xlsx", FolderPath, DateTime.Now.ToString("ddMMyyyy"));
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
    }
}