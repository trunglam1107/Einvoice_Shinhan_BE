using InvoiceServer.Business;
using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using System;
using System.IO;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ValidatorInvoiceBusiness'
    public class ValidatorInvoiceBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ValidatorInvoiceBusiness'
    {
        #region Fields, Properties


        #endregion Fields, Properties

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ValidatorInvoiceBusiness.ValidatorInvoiceBusiness(IBOFactory)'
        public ValidatorInvoiceBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ValidatorInvoiceBusiness.ValidatorInvoiceBusiness(IBOFactory)'
        {

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ValidatorInvoiceBusiness.ImportData(FileImport)'
        public ValidatorResult ImportData(FileImport fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ValidatorInvoiceBusiness.ImportData(FileImport)'
        {
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > 2048) //Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }

            string fullPathfile = SaveFileImport(fileImport);
            string resultVerify = FileProcess.VerifyPdfFile(fullPathfile);
            ValidatorResult validater = new ValidatorResult();
            validater.Result = resultVerify;
            return validater;
        }

        private static string SaveFileImport(FileImport fileImport)
        {
            string FolderPath = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.UploadFolderImport);
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string fileName = string.Format(@"{0}\{1}.pdf", FolderPath, DateTime.Now.ToString("ddMMyyyy"));
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


        #endregion
    }
}