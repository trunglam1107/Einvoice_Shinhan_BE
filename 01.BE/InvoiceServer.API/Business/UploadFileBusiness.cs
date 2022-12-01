using InvoiceServer.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness'
    public class UploadFileBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.SaveFile(FileUploadInfo, string)'
        public ResultCode SaveFile(FileUploadInfo fileUploadInfo, string filePath)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.SaveFile(FileUploadInfo, string)'
        {
            var file = fileUploadInfo.File;
            if (file == null || file.InputStream == null || string.IsNullOrWhiteSpace(filePath))
            {
                return ResultCode.RequestDataInvalid;
            }

            ResultCode resultCode = ResultCode.WaitNextRequest;
            using (var fs = new FileStream(filePath, fileUploadInfo.Chunk == 0 ? FileMode.Create : FileMode.Append))
            {
                var buffer = new byte[Config.ApplicationSetting.Instance.MaxLengthBuffer];
                int bytesRead;
                while ((bytesRead = file.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, bytesRead);
                }
            }

            if (fileUploadInfo.Chunk == (fileUploadInfo.TotalChunk - 1))
            {
                resultCode = ResultCode.NoError;
            }

            return resultCode;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.IsValidFile(FileUploadInfo)'
        public bool IsValidFile(FileUploadInfo fileUploadInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.IsValidFile(FileUploadInfo)'
        {
            if (fileUploadInfo == null)
            {
                return false;
            }

            bool fileSizeValid = fileUploadInfo.File.ContentLength > 0 && fileUploadInfo.File.ContentLength <= Config.ApplicationSetting.Instance.MaxSizeFileUpload;
            bool extensionValid = Utility.Equals(Path.GetExtension(fileUploadInfo.FileName), FileUpload.FileExtension);
            return fileSizeValid && extensionValid;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.IsUploadedFile(FileUploadInfo)'
        public bool IsUploadedFile(FileUploadInfo fileUploadInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.IsUploadedFile(FileUploadInfo)'
        {
            return fileUploadInfo.Chunk == (fileUploadInfo.TotalChunk - 1);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.CreateFilePathByDocId(long, FileUploadInfo)'
        public string CreateFilePathByDocId(long releaseId, FileUploadInfo fileUploadInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.CreateFilePathByDocId(long, FileUploadInfo)'
        {
            long companyId = GetCompanyIdOfUser();
            string path = Path.Combine(Config.ApplicationSetting.Instance.FolderInvoiceFile, companyId.ToString(), AssetSignInvoice.Release, AssetSignInvoice.TempSignFile, releaseId.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Path.Combine(path, fileUploadInfo.FileName);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.CreateFilePathByDocId(long, long, FileUploadInfo)'
        public string CreateFilePathByDocId(long releaseId, long companyId, FileUploadInfo fileUploadInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.CreateFilePathByDocId(long, long, FileUploadInfo)'
        {
            string path = Path.Combine(Config.ApplicationSetting.Instance.FolderInvoiceFile, companyId.ToString(), AssetSignInvoice.Release, AssetSignInvoice.TempSignFile, releaseId.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Path.Combine(path, fileUploadInfo.FileName);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.StandardAnnounFileUpload(string)'
        public string StandardAnnounFileUpload(string sourceFile)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.StandardAnnounFileUpload(string)'
        {
            string newFilePath = string.Empty;
            try
            {
                FileInfo fileInfo = new FileInfo(sourceFile);

                var extension = fileInfo.Extension;
                var nameWithoutExtension = fileInfo.Name.Remove(fileInfo.Name.LastIndexOf(extension));

                string suffix = DateTime.Now.ToString(Formatter.DateTimeFormat);
                string newName = nameWithoutExtension + Characters.Underscore + suffix + extension;

                string folderPath = fileInfo.DirectoryName;
                string destFile = Path.Combine(folderPath, newName);

                fileInfo.MoveTo(destFile);
                long companyId = GetCompanyIdOfUser();
                string path = Path.Combine(Config.ApplicationSetting.Instance.FolderInvoiceFile, companyId.ToString(), AssetSignAnnoucement.Release, AssetSignAnnoucement.SignFile);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                FileProcess.ExtractFile(destFile, path);
                newFilePath = destFile;
                DeleteFolderTempl(folderPath);
            }
            catch
            {
                // Don't need handle exception. 
                // When exception, will set file path is source file
                // TODO: Maybe, write log to trace exception.
                newFilePath = string.Empty;
            }

            return newFilePath;

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.StandardFileUpload(string, long, InvoiceInfo, int)'
        public string StandardFileUpload(string sourceFile, long companyId, InvoiceInfo invoiceInfo, int type)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.StandardFileUpload(string, long, InvoiceInfo, int)'
        {
            string newFilePath = string.Empty;
            try
            {
                FileInfo fileInfo = new FileInfo(sourceFile);
                var releaseDateFolder = PathUtil.FolderTree(invoiceInfo.ReleasedDate);

                var extension = fileInfo.Extension;
                var nameWithoutExtension = fileInfo.Name.Remove(fileInfo.Name.LastIndexOf(extension));

                string suffix = DateTime.Now.ToString(Formatter.DateTimeFormat);
                string newName = nameWithoutExtension + Characters.Underscore + suffix + extension;

                string folderPath = fileInfo.DirectoryName;
                string destFile = Path.Combine(folderPath, newName);

                fileInfo.MoveTo(destFile);
                //truong hop Invoice
                string path = Path.Combine(Config.ApplicationSetting.Instance.FolderInvoiceFile, companyId.ToString(), AssetSignInvoice.Release, AssetSignInvoice.SignFile, releaseDateFolder);
                //truong hop Announcement
                if (type != 1)
                {
                    path = Path.Combine(Config.ApplicationSetting.Instance.FolderInvoiceFile, companyId.ToString(), AssetSignAnnoucement.Release, AssetSignAnnoucement.SignFile, releaseDateFolder);
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                FileProcess.ExtractFile(destFile, path);
                newFilePath = destFile;
                DeleteFolderTempl(folderPath);
            }
            catch
            {
                // Don't need handle exception. 
                // When exception, will set file path is source file
                // TODO: Maybe, write log to trace exception.
                newFilePath = string.Empty;
            }

            return newFilePath;

        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.StandardFileUpload(string, IEnumerable<InvoicesReleaseDetail>)'
        public string StandardFileUpload(string sourceFile, IEnumerable<InvoicesReleaseDetail> listInvoices)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadFileBusiness.StandardFileUpload(string, IEnumerable<InvoicesReleaseDetail>)'
        {
            string newFilePath = string.Empty;
            try
            {
                FileInfo fileInfo = new FileInfo(sourceFile);

                var extension = fileInfo.Extension;
                var nameWithoutExtension = fileInfo.Name.Remove(fileInfo.Name.LastIndexOf(extension));

                string suffix = DateTime.Now.ToString(Formatter.DateTimeFormat);
                string newName = nameWithoutExtension + Characters.Underscore + suffix + extension;

                string folderPath = fileInfo.DirectoryName;
                string destFile = Path.Combine(folderPath, newName);

                fileInfo.MoveTo(destFile);
                long companyId = GetCompanyIdOfUser();

                string path = Path.Combine(Config.ApplicationSetting.Instance.FolderInvoiceFile, companyId.ToString(), AssetSignInvoice.Release, AssetSignInvoice.SignFile, "SignTemp");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                FileProcess.ExtractFile(destFile, path);
                ProcessFolderFile(path, listInvoices);
                Directory.Delete(path, true);



                newFilePath = destFile;
                DeleteFolderTempl(folderPath);
            }
            catch
            {
                // Don't need handle exception. 
                // When exception, will set file path is source file
                // TODO: Maybe, write log to trace exception.
                newFilePath = string.Empty;
            }
            return newFilePath;
        }
        private void ProcessFolderFile(string folderTemp, IEnumerable<InvoicesReleaseDetail> listInvoices)
        {
            foreach (var invoice in listInvoices)
            {
                string pathSign = Path.Combine(Config.ApplicationSetting.Instance.FolderInvoiceFile, invoice.CompanyId.ToString(), AssetSignInvoice.Release, AssetSignInvoice.SignFile);
                string folderFileDaily = Path.Combine(pathSign, PathUtil.FolderTree(invoice.InvoiceDate));
                if (!Directory.Exists(folderFileDaily))
                {
                    Directory.CreateDirectory(folderFileDaily);
                }
                string FileNamePdf = invoice.Id.ToString() + "_sign.pdf";
                string FileNameXml = invoice.Id.ToString() + "_sign.xml";
                string currentFilePdf = Path.Combine(folderTemp, FileNamePdf);
                string currentFileXml = Path.Combine(folderTemp, FileNameXml);
                File.Move(currentFilePdf, Path.Combine(folderFileDaily, FileNamePdf));
                File.Move(currentFileXml, Path.Combine(folderFileDaily, FileNameXml));
            }
        }

        private void DeleteFolderTempl(string destFile)
        {

            if (destFile.IsNotNullOrEmpty() && Directory.Exists(destFile))
            {
                Directory.Delete(destFile, true);
            }
        }
    }
}