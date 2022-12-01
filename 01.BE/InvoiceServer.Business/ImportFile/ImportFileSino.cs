using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Configuration;

namespace InvoiceServer.Business.DAO
{
    partial class ImportFileDAO
    {
        private readonly string SourceFileInvoiceSino = GetConfig("SourceFileInvoice");
        private void GetFileFromFtpServer(string nameDay)
        {
            try
            {
                string fileName = "VAT_E_INVOICE_" + nameDay + ".TXT";
                string ftpFile = GetConfig("HostFtpInv") + fileName;
                logger.Error("FileName_Import", new Exception(ftpFile));
                string userName = GetConfig("UserNameFtpInv");
                string pass = GetConfig("PasswordFtpInv");
                FtpWebRequest request =
                (FtpWebRequest)WebRequest.Create(ftpFile);
                request.Credentials = new NetworkCredential(userName, pass);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (Stream ftpStream = request.GetResponse().GetResponseStream())
                using (Stream fileStream = File.Create(Path.Combine(SourceFileInvoiceSino, fileName)))
                {
                    byte[] buffer = new byte[10240];
                    int read;
                    while ((read = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, read);
                    }
                }
            }
            catch (WebException e)
            {
                String status = ((FtpWebResponse)e.Response).StatusDescription;
                logger.Error("Import_Sino", new Exception(status));
            }
        }
        public void ImportFileAuto_Sino(bool isJob)
        {
            string FolderPath = GetConfig("ImportFile");
            String ip = IP.GetIPAddress();
            int MaxDayStoreImport = int.Parse(GetConfig("MaxDayStoreImport"));
            int MaxDayReadImport = int.Parse(GetConfig("MaxDayReadImport"));

            if (isJob)
            {
                // Clear all file in working directory
                ClearAllFileInWorkingFolder(FolderPath);
                // Clear all store files out of date
                RemoveOldFilesStored(FolderPath, MaxDayStoreImport);
                // copy file from orther server to FolderImportFile
                CopyMultiFileFromSourceSino(FolderPath, MaxDayReadImport);
            }

            // Get all files in folder FolderPath
            string[] fileArray = Directory.GetFiles(Path.Combine(FolderPath));
            if (fileArray.Length == 0)
            {
                this.InsertSystemLog(ip, FolderPath);
                throw new BusinessLogicException(ResultCode.ImportInvoiceFileError, "No files in import working folder");
            }

            // Get list date invoice
            List<string> myCollection = GetCollectionSino(fileArray);
            foreach (var c in myCollection)
            {
                string[] fileArrayDate = Directory.GetFiles(Path.Combine(FolderPath), "*" + c + ".TXT");
                try
                {
                    this.transaction.BeginTransaction();
                    // Import vào DB
                    ImportFileForDaySino(fileArrayDate, FolderPath);
                    this.transaction.Commit();
                    // Import xong

                    // Chuyển file sang thư mục store
                    var result = dbContext.IMPORT_LOG.AsNoTracking()
                        .OrderByDescending(e => e.ID)
                        .Take(fileArrayDate.Count())
                        .Select(e => new ImportLog()
                        {
                            IsSuccess = e.ISSUCCESS == true,
                            FilePath = e.FILEPATH,
                        })
                        .ToList();

                    foreach (var p in result)
                    {
                        try
                        {
                            MoveFileToStore(p, FolderPath);
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"ImportInvoice: MoveFileToStore({p?.FilePath}, {FolderPath}", ex);
                        }
                    }
                    // End chuyển file sang thư mục store

                }
                catch (Exception ex)
                {
                    this.transaction.Rollback();
                    throw new BusinessLogicException(ResultCode.ImportInvoiceFileSuccess, "Import file error", ex);
                }
            }
        }

        private void ImportFileForDaySino(string[] fileArrayDate, string FolderPath)
        {
            string FormatFile = GetConfig("FormatFile");
            string FormatFile_File = GetConfig("FormatFile_File");
            string FormatFile_Invoice = GetConfig("FormatFile_Invoice");
            string FormatFile_Count = GetConfig("FormatFile_Count");
            foreach (var p in fileArrayDate)
            {
                string fName = p.Substring(FolderPath.Length + 1);

                this.dbContext.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "EXEC SP_IMPORTFILE_SINO @DATAPATH, @DATANAME, @FORMATPATH, @FORMAT_INVOICE_PATH, @COUNTPATH",
                    new SqlParameter("DATAPATH", FolderPath),
                    new SqlParameter("DATANAME", fName),
                    new SqlParameter("FORMATPATH", FormatFile + @"\" + FormatFile_File),
                    new SqlParameter("FORMAT_INVOICE_PATH", FormatFile + @"\" + FormatFile_Invoice),
                    new SqlParameter("COUNTPATH", FormatFile + @"\" + FormatFile_Count)
                    );

            }

        }

        private void CopyMultiFileFromSourceSino(string FolderPath, int MaxDayReadImport)
        {
            var toDate = DateTime.Now;
            var fromDate = toDate.AddDays(-MaxDayReadImport);
            int numberOfDay = Convert.ToInt32((toDate - fromDate).TotalDays);

            for (int i = 0; i < numberOfDay + 1; i++)
            {
                DateTime midDay = fromDate.AddDays(i);
                var midDayString = midDay.ToString("yyyyMMdd");
                var fileNameInvoice = Path.Combine("VAT_E_INVOICE_" + midDayString + ".TXT");

                var importLogCount = this.dbContext.IMPORT_LOG.Count(e =>
                                e.ISSUCCESS == true
                            && e.FILENAME.ToUpper().Contains(fileNameInvoice.ToUpper()));

                if (importLogCount < 1)
                {
                    CopyFileFromSourceSino(FolderPath, midDayString);
                }
            }
        }

        private void CopyFileFromSourceSino(string FolderPath, string dayGetFile)
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            // start funtion: copy file from orther server to FolderImportFile
            var usFTP = bool.Parse(WebConfigurationManager.AppSettings["usFtp"] ?? "false");
            if (usFTP)
            {
                GetFileFromFtpServer(dayGetFile);
            }
            // for invoice
            //string SourceFileInvoice = GetConfig("SourceFileInvoice"); // example path: ...path/VAT_E_INVOICE_20200223.txt
            string[] invoiceFileArray = new string[0];
            if (Directory.Exists(Path.Combine(SourceFileInvoiceSino)))
            {
                invoiceFileArray = Directory.GetFiles(Path.Combine(SourceFileInvoiceSino), "VAT_E_INVOICE_" + dayGetFile + ".TXT");
            }
            foreach (var p in invoiceFileArray)
            {
                string fNameInvoice = p.Substring(SourceFileInvoiceSino.Length + 1);
                File.Copy(Path.Combine(SourceFileInvoiceSino, fNameInvoice), Path.Combine(FolderPath, fNameInvoice), true);
            }
            //end funtion: copy file from orther server to FolderImportFile
        }

        private static List<string> GetCollectionSino(string[] fileArray)
        {
            List<string> myCollection = new List<string>();
            foreach (var p in fileArray)
            {
                string stringDate = p.Substring(p.Length - 12, 12);
                stringDate = stringDate.Substring(0, 8);
                myCollection.Add(stringDate);
            }
            return myCollection.Distinct().ToList();
        }
    }
}