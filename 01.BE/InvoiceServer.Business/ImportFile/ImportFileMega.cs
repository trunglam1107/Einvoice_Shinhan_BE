using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace InvoiceServer.Business.DAO
{
    partial class ImportFileDAO
    {
        public void ImportFileAuto_Mega(bool isJob)
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
                CopyMultiFileFromSourceMega(FolderPath, MaxDayReadImport);
            }

            // Get all files in folder FolderPath
            string[] fileArray = Directory.GetFiles(Path.Combine(FolderPath));
            if (fileArray.Length == 0)
            {
                this.InsertSystemLog(ip, FolderPath);
                throw new BusinessLogicException(ResultCode.ImportInvoiceFileError, "No files in import working folder");
            }

            // Get list date invoice
            List<string> myCollection = GetCollectionMega(fileArray);
            foreach (var c in myCollection)
            {
                string[] fileArrayDate = Directory.GetFiles(Path.Combine(FolderPath), "*" + c + ".csv");
                //this.InitImportDBLog(fileArrayDate, FolderPath);Đã code trong store
                try
                {
                    // Import vào DB
                    ImportFileForDayMega(fileArrayDate, FolderPath);
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
                            DeleteFileInputImportSuccessMega(p);
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

        private void ImportFileForDayMega(string[] fileArrayDate, string FolderPath)
        {
            foreach (var p in fileArrayDate)
            {
                var fileName = Path.GetFileName(p);
                var fileNameNoExtension = Path.GetFileNameWithoutExtension(p);
                var cSVFilePath = CreateFileSchemaCSV(p, FolderPath);

                this.dbContext.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "EXEC SP_IMPORTFILE_MEGA @DATAPATH, @FILENAME, @FILENAMENOTEXTENSION",
                    new SqlParameter("DATAPATH", FolderPath),
                    new SqlParameter("FILENAME", fileName),
                    new SqlParameter("FILENAMENOTEXTENSION", fileNameNoExtension)
                    );

                if (File.Exists(cSVFilePath))
                {
                    File.Delete(cSVFilePath);
                }
            }
            //UpdateSuccessImportDBLog(fileArrayDate, FolderPath);Đã code trong store
        }

        private string CreateFileSchemaCSV(string fullPathFileName, string workingFolderPath)
        {
            // File schema.ini để import data từ csv vô sql server không bị mất các số 0 ở đầu dữ liệu dạng số: 000012345
            string fullPathFileAsset = DefaultFields.ASSET_FOLDER;
            var pathFileSchemaIni = Path.Combine(fullPathFileAsset, "ImportSchemaCSV_3.ini");

            var fileName = Path.GetFileName(fullPathFileName);
            var workingSchemeFilePath = Path.Combine(workingFolderPath, $"Schema.ini");

            var schemaText = new StringBuilder(File.ReadAllText(pathFileSchemaIni));
            schemaText.Replace("@CSVFileName", fileName);

            File.WriteAllText(workingSchemeFilePath, schemaText.ToString());
            return workingSchemeFilePath;
        }

        private void CopyMultiFileFromSourceMega(string destinationFolderPath, int MaxDayReadImport)
        {
            var toDate = DateTime.Now;
            var fromDate = toDate.AddDays(-MaxDayReadImport);
            int numberOfDay = Convert.ToInt32((toDate - fromDate).TotalDays);

            for (int i = 0; i < numberOfDay + 1; i++)
            {
                DateTime midDay = fromDate.AddDays(i);
                var midMonthDayString = midDay.ToString("MMdd");
                var midYearMonthDayStringShort = midDay.ToString("yyMMdd");
                var midYearMonthDayStringLong = midDay.ToString("yyyyMMdd");
                var fileNameInvoiceDailyInLog = $"EI{midYearMonthDayStringShort}_{midYearMonthDayStringLong}.csv";

                var importLogCountDaily = this.dbContext.IMPORT_LOG.Count(e =>
                                e.ISSUCCESS == true
                            && e.FILENAME.ToUpper().Contains(fileNameInvoiceDailyInLog.ToUpper()));

                if (importLogCountDaily < 1)
                {
                    var fileName = $"EI{midYearMonthDayStringShort}.csv";
                    CopyFileFromSourceMega(destinationFolderPath, midDay, fileName);
                }

                string sourceFileInvoice = GetConfig("SourceFileInvoice");
                var fileArrayDay = Directory.GetFiles(Path.Combine(sourceFileInvoice), $"*{midMonthDayString}.csv");
                foreach (var p in fileArrayDay)
                {
                    var fileName = Path.GetFileName(p);
                    int.TryParse(fileName.Substring(0, 4), out int indexOfFileInDay);
                    if (indexOfFileInDay != 0)
                    {
                        var fileNameInvoiceManuallyInLog = $"{indexOfFileInDay.ToString().PadLeft(4, '0')}{midMonthDayString}_{midYearMonthDayStringLong}.csv";
                        var importLogCountManually = this.dbContext.IMPORT_LOG.Count(e =>
                                        e.ISSUCCESS == true
                                    && e.FILENAME.ToUpper().Contains(fileNameInvoiceManuallyInLog.ToUpper()));
                        if (importLogCountManually < 1)
                        {
                            CopyFileFromSourceMega(destinationFolderPath, midDay, fileName);
                        }
                    }
                }
            }
        }

        private void CopyFileFromSourceMega(string destinationFolderPath, DateTime dayGetFile, string fileName)
        {
            if (!Directory.Exists(destinationFolderPath))
            {
                Directory.CreateDirectory(destinationFolderPath);
            }
            string SourceFileInvoice = GetConfig("SourceFileInvoice"); // example path: ...path/EINV0331.csv
            string invoiceFullPathFile = Path.Combine(SourceFileInvoice, fileName);
            if (File.Exists(invoiceFullPathFile))
            {
                var midDayYearString = dayGetFile.ToString("yyyyMMdd");
                string fNameInvoiceNoExtension = Path.GetFileNameWithoutExtension(invoiceFullPathFile);
                string newFNameInvoice = $"{fNameInvoiceNoExtension}_{midDayYearString}.csv";
                if (DeleteFileInputAfterImport)
                    File.Move(invoiceFullPathFile, Path.Combine(destinationFolderPath, newFNameInvoice));
                else
                    File.Copy(invoiceFullPathFile, Path.Combine(destinationFolderPath, newFNameInvoice), true);
            }
        }

        private static List<string> GetCollectionMega(string[] fileArray)
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

        private void DeleteFileInputImportSuccessMega(ImportLog importLog)
        {
            if (!DeleteFileInputAfterImport)
                return;

            string SourceFileInvoice = GetConfig("SourceFileInvoice");
            string InvoiceFullPath = Path.Combine(SourceFileInvoice, importLog.FileName);
            if (Directory.Exists(SourceFileInvoice) && File.Exists(InvoiceFullPath))
            {
                File.Delete(InvoiceFullPath);
            }
        }
    }
}