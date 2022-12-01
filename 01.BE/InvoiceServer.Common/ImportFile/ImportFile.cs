using Dapper;
using InvoiceServer.Common.Constants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Configuration;

namespace InvoiceServer.Common
{
    public partial class ImportFile
    {
        private const string PathSuccess = "Success";
        private const string PathFail = "Fail";
        private const string PathImportDate = "ImportDate_";
        private const int ExecuteTimeOut = 600; // timeout by second
        private static readonly Func<string, string> GetConfig = key => WebConfigurationManager.AppSettings[key];

        public static ImportFile Instance { get; } = new ImportFile();

        public class ImportLog
        {
            public int Id { get; set; }
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public string Decription { get; set; }
            public string Date { get; set; }
            public bool IsSuccess { get; set; }
            public Int16 IsSuccessOracle { get; set; }
        }

        public static void ImportFileAuto_Huanan(bool isJob)
        {
            string FolderPath = GetConfig("ImportFile");
            string FormatFile = GetConfig("FormatFile");
            string FormatFile_File = GetConfig("FormatFile_File");
            string FormatFile_Client = GetConfig("FormatFile_Client");
            string FormatFile_Invoice = GetConfig("FormatFile_Invoice");
            string FormatFile_Count = GetConfig("FormatFile_Count");
            string sql = "SP_IMPORTFILE_HUANAN";
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
            string sqlResult = "SELECT TOP 2 ISSUCCESS, FILEPATH FROM IMPORT_LOG ORDER BY ID DESC";
            // get ip
            String ip = IP.GetIPAddress();
            // end get ip
            string sqlInsert = "INSERT INTO SYSTEMLOGS (FUNCTIONCODE, LOGSUMMARY, LOGTYPE, LOGDATE, LOGDETAIL, IP, USERNAME) " +
                    "Values (@FUNCTIONCODE, @LOGSUMMARY, @LOGTYPE, @LOGDATE, @LOGDETAIL, @IP, @USERNAME);";
            int MaxDayStoreImport = int.Parse(GetConfig("MaxDayStoreImport"));
            int MaxDayReadImport = int.Parse(GetConfig("MaxDayReadImport"));

            //
            if (isJob)
            {
                // Clear all file in working directory
                ClearAllFileInFolder(FolderPath);
                // Clear all store files out of date
                RemoveOldFilesStored(FolderPath, MaxDayStoreImport);
                // start funtion: copy file from orther server to FolderImportFile
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    CopyMultiFileFromSourceHuanan(FolderPath, MaxDayReadImport, connection);
                    connection.Close();
                }
                // end funtion: copy file from orther server to FolderImportFile
            }

            // Get all files in folder FolderPath
            string[] fileArray = Directory.GetFiles(Path.Combine(FolderPath), "*.txt");
            if (fileArray.Length == 0)
            {
                insertSystemlogSqlServer(ip, sqlInsert, FolderPath, connectionString);
                throw new BusinessLogicException(ResultCode.ImportInvoiceFileError, "No files in import working folder");
            }

            // Get list date invoice
            List<string> myCollection = GetCollection(fileArray);
            foreach (var c in myCollection)
            {
                string[] fileArrayDate = Directory.GetFiles(Path.Combine(FolderPath), "*" + c + ".txt");
                using (var connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        ClearOldRawDataHuanan(connection);
                        string invoiceName = "";
                        string clientName = "";
                        foreach (var p in fileArrayDate)
                        {
                            string fName = p.Substring(FolderPath.Length + 1);
                            if (fName.Contains("VNTEVATM_"))
                                invoiceName = fName;
                            else
                                clientName = fName;
                            connection.Open();
                            DynamicParameters parameter = new DynamicParameters();
                            parameter.Add("@DATAPATH", FolderPath, DbType.String, ParameterDirection.Input);
                            parameter.Add("@DATANAME", fName, DbType.String, ParameterDirection.Input);
                            parameter.Add("@FORMATPATH", FormatFile + @"\" + FormatFile_File, DbType.String, ParameterDirection.Input);
                            parameter.Add("@FORMAT_CLIENT_PATH", FormatFile + @"\" + FormatFile_Client, DbType.String, ParameterDirection.Input);
                            parameter.Add("@FORMAT_INVOICE_PATH", FormatFile + @"\" + FormatFile_Invoice, DbType.String, ParameterDirection.Input);
                            parameter.Add("@COUNTPATH", FormatFile + @"\" + FormatFile_Count, DbType.String, ParameterDirection.Input);
                            parameter.Add("@INVOICENAME", invoiceName, DbType.String, ParameterDirection.Input);
                            parameter.Add("@CLIENTNAME", clientName, DbType.String, ParameterDirection.Input);
                            connection.Execute(sql, parameter, commandType: CommandType.StoredProcedure, commandTimeout: ExecuteTimeOut);
                            connection.Close();
                        }

                        connection.Open();
                        string queryTop = sqlResult.Replace(" 2 ", $" {fileArrayDate.Count().ToString()} ");
                        var result = connection.Query<ImportLog>(queryTop).ToList();
                        connection.Close();
                        foreach (var p in result)
                        {
                            try
                            {
                                MoveFileToStore(p, FolderPath);
                            }
                            catch (Exception ex)
                            {
                                Logger logger = new Logger();
                                logger.Error($"ImportInvoice: MoveFileToStore({p?.FilePath}, {FolderPath}", ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger logger = new Logger();
                        logger.Error($"ImportInvoice: import day {c}", ex);
                        throw new BusinessLogicException(ResultCode.ImportInvoiceFileSuccess, "Import file error", ex);
                    }
                }
            }
            // import/update mail for client from path FileMailPath
            ImportMailAuto();
        }

        private static List<string> GetCollection(string[] fileArray)
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

        private static void insertSystemlogSqlServer(String ip, string sqlInsert, string FolderPath, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                insertSystemlog(ip, sqlInsert, FolderPath, connection);
            }
        }

        private static void insertSystemlog(String ip, string sqlInsert, string FolderPath, IDbConnection connection)
        {
            connection.Open();
            var affectedRows = connection.Execute(sqlInsert, new
            {
                FUNCTIONCODE = "quartzJobManagement",
                LOGSUMMARY = "Import invoice",
                LOGTYPE = 1,
                LOGDATE = DateTime.Now,
                LOGDETAIL = "Not found file in " + FolderPath,
                IP = ip,
                USERNAME = "Quarzt"
            });
            connection.Close();
        }

        private static void ClearAllFileInFolder(string FolderPath)
        {
            try
            {
                if (!Directory.Exists(FolderPath))
                {
                    return;
                }
                string[] fileArray = Directory.GetFiles(FolderPath);
                foreach (var file in fileArray)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Logger logger = new Logger();
                        logger.Trace("Class:ImportFile Method:ClearAllFileInFolder" + Environment.NewLine + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void RemoveOldFilesStored(string FolderPath, int MaxDayStoreImport)
        {
            if (!Directory.Exists(FolderPath))
            {
                return;
            }
            var toDate = DateTime.Now;
            var fromDate = toDate.AddDays(-MaxDayStoreImport);
            var listDayString = GetListDate(fromDate, toDate, "yyyyMMdd");

            var pathSuccess = Path.Combine(FolderPath, PathSuccess);
            if (Directory.Exists(pathSuccess))
            {
                var listSuccessFolderKeep = listDayString.Select(x => PathImportDate + x).ToList();
                var folderSuccessArray = Directory.GetDirectories(pathSuccess, $"{PathImportDate}????????");
                foreach (var folderSuccessFullPath in folderSuccessArray)
                {
                    var folderSuccess = new DirectoryInfo(folderSuccessFullPath).Name;
                    var isPathValid = true;
                    if (!folderSuccess.StartsWith(PathImportDate, StringComparison.InvariantCultureIgnoreCase))
                        isPathValid = false;
                    if (!DateTime.TryParseExact(folderSuccess.Substring(PathImportDate.Length), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime oldDateImportSuccess))
                        isPathValid = false;

                    if (isPathValid && !listSuccessFolderKeep.Contains(folderSuccess))
                    {
                        Directory.Delete(folderSuccessFullPath, true);
                    }
                }
            }

            var pathFaild = Path.Combine(FolderPath, PathFail);
            if (Directory.Exists(pathFaild))
            {
                var listFailFolderKeep = listDayString.Select(x => PathImportDate + x).ToList();
                var folderFailArray = Directory.GetDirectories(pathFaild, $"{PathImportDate}????????");
                foreach (var folderFailFullPath in folderFailArray)
                {
                    var folderFail = new DirectoryInfo(folderFailFullPath).Name;
                    var isPathValid = true;
                    if (!folderFail.StartsWith(PathImportDate, StringComparison.InvariantCultureIgnoreCase))
                        isPathValid = false;
                    if (!DateTime.TryParseExact(folderFail.Substring(PathImportDate.Length), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime oldDateImportFail))
                        isPathValid = false;

                    if (isPathValid && !listFailFolderKeep.Contains(folderFail))
                    {
                        Directory.Delete(folderFailFullPath, true);
                    }
                }
            }
        }

        private static void CopyMultiFileFromSourceHuanan(string FolderPath, int MaxDayReadImport, SqlConnection connection)
        {
            string sqlGetImportLog = "SELECT COUNT(*) FROM IMPORT_LOG WHERE ISSUCCESS = 1 AND " +
                " (FILENAME like @FILENAMEINVOICE OR FILENAME LIKE @FILENAMECLIENT) ";
            var toDate = DateTime.Now;
            var fromDate = toDate.AddDays(-MaxDayReadImport);
            int numberOfDay = Convert.ToInt32((toDate - fromDate).TotalDays);

            for (int i = 0; i < numberOfDay + 1; i++)
            {
                DateTime midDay = fromDate.AddDays(i);
                var midDayString = midDay.ToString("yyyyMMdd");
                var fileNameInvoice = Path.Combine(midDayString + "VNTEVATM_" + midDayString + ".txt");
                var fileNameClient = Path.Combine("khachhang_" + midDayString + ".txt");

                var importLogCount = connection.Query<int>(sqlGetImportLog, new
                {
                    FILENAMEINVOICE = fileNameInvoice,
                    FILENAMECLIENT = fileNameClient,
                }).FirstOrDefault();

                if (importLogCount < 2)
                {
                    CopyFileFromSourceHuanan(FolderPath, midDayString);
                }
            }
        }

        private static void CopyFileFromSourceHuanan(string FolderPath, string dayGetFile)
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            // start funtion: copy file from orther server to FolderImportFile
            string SourceFileClient = GetConfig("SourceFileClient");
            SourceFileClient = SourceFileClient + @"\" + dayGetFile; // example path: ...path/20191125/khachhang_20191125.txt
            string[] clientFileArray = new string[0];
            if (Directory.Exists(Path.Combine(SourceFileClient)))
            {
                clientFileArray = Directory.GetFiles(Path.Combine(SourceFileClient), "khachhang_" + dayGetFile + ".txt");
            }
            else
            {
                Logger logger = new Logger();
                BusinessLogicException exception = new BusinessLogicException(ResultCode.NotPermisionData, $"Path not have permision/not exists: {SourceFileClient}");
                logger.Error("quarzt", exception);
            }
            foreach (var p in clientFileArray)
            {
                // Remove path from the file name.
                string fNameClient = p.Substring(SourceFileClient.Length + 1);
                File.Copy(Path.Combine(SourceFileClient, fNameClient), Path.Combine(FolderPath, fNameClient), true);
            }
            // for invoice
            string SourceFileInvoice = GetConfig("SourceFileInvoice"); // example path: ...path/20191125VNTEVATM_20191125.TXT
            string[] invoiceFileArray = new string[0];
            if (Directory.Exists(Path.Combine(SourceFileInvoice)))
            {
                invoiceFileArray = Directory.GetFiles(Path.Combine(SourceFileInvoice), dayGetFile + "VNTEVATM_" + dayGetFile + ".txt");
            }
            else
            {
                Logger logger = new Logger();
                BusinessLogicException exception = new BusinessLogicException(ResultCode.NotPermisionData, $"Path not have permision/not exists: {SourceFileInvoice}");
                logger.Error("quarzt", exception);
            }
            foreach (var p in invoiceFileArray)
            {
                // Remove path from the file name.
                string fNameInvoice = p.Substring(SourceFileInvoice.Length + 1);
                File.Copy(Path.Combine(SourceFileInvoice, fNameInvoice), Path.Combine(FolderPath, fNameInvoice), true);
            }
            //end funtion: copy file from orther server to FolderImportFile
        }

        private static void MoveFileToStore(ImportLog p, string FolderPath)
        {
            string todate = DateTime.Now.ToString("yyyyMMdd");
            if (!p.IsSuccess)
            {
                if (!Directory.Exists(Path.Combine(FolderPath, PathFail)))
                {
                    Directory.CreateDirectory(Path.Combine(FolderPath, PathFail));
                }

                //Create folder Fail 
                if (!Directory.Exists(Path.Combine(FolderPath, PathFail, PathImportDate + todate)))
                {
                    Directory.CreateDirectory(Path.Combine(FolderPath, PathFail, PathImportDate + todate));
                }
                string source = p.FilePath;
                string destination = p.FilePath.Replace(FolderPath, Path.Combine(FolderPath, PathFail, PathImportDate + todate));
                var fileInfo = new FileInfo(destination);
                var fileName = fileInfo.Name;
                var fileNameNoExtension = Path.GetFileNameWithoutExtension(destination);
                int index = 2;
                while (File.Exists(Path.Combine(fileInfo.DirectoryName, fileName)))
                {
                    fileName = fileNameNoExtension + $"_{index}" + fileInfo.Extension;
                    index++;
                }
                destination = Path.Combine(fileInfo.DirectoryName, fileName);
                File.Move(source, destination);
                throw new BusinessLogicException(ResultCode.ImportClientFileError, "Import file client error");
            }

            //Create folder SUCCESS
            if (!Directory.Exists(Path.Combine(FolderPath, PathSuccess)))
            {
                Directory.CreateDirectory(Path.Combine(FolderPath, PathSuccess));
            }

            //Create folder SUCCESS 
            if (!Directory.Exists(Path.Combine(FolderPath, PathSuccess, PathImportDate + todate)))
            {
                Directory.CreateDirectory(Path.Combine(FolderPath, PathSuccess, PathImportDate + todate));
            }
            string sourceFile = p.FilePath;
            string destinationFile = p.FilePath.Replace(FolderPath, Path.Combine(FolderPath, PathSuccess, PathImportDate + todate));
            try
            {
                int numTry = 0;
                while (!IsFileReady(sourceFile))
                {
                    System.Threading.Thread.Sleep(1000);
                    numTry++;
                    if (numTry > 600)
                    {
                        break;
                    }
                }
                var fileInfo = new FileInfo(destinationFile);
                var fileName = fileInfo.Name;
                var fileNameNoExtension = Path.GetFileNameWithoutExtension(destinationFile);
                int index = 2;
                while (File.Exists(Path.Combine(fileInfo.DirectoryName, fileName)))
                {
                    fileName = fileNameNoExtension + $"_{index}" + fileInfo.Extension;
                    index++;
                }
                destinationFile = Path.Combine(fileInfo.DirectoryName, fileName);
                File.Move(sourceFile, destinationFile);
            }
            catch
            {
                Logger logger = new Logger();
                logger.Trace($"SourceFile: {sourceFile} {Environment.NewLine} DestinationFile: {destinationFile}");
                throw;
            }
        }

        public static bool IsFileReady(string filename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void ClearOldRawDataHuanan(SqlConnection connection)
        {
            string sql1 = "TRUNCATE TABLE CLIENT_RAWDATA";
            string sql2 = "TRUNCATE TABLE INVOICE_RAWDATA";
            connection.Open();
            connection.Execute(sql1, commandTimeout: ExecuteTimeOut);
            connection.Execute(sql2, commandTimeout: ExecuteTimeOut);
            connection.Close();
        }

        public static void ImportFileAuto_Mega()
        {
            string FolderPath = GetConfig("FolderImportFile");
            string sql = "SP_IMPORTFILE_MEGA";
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
            string sqlResult = "SELECT TOP 1 ISSUCCESS, FILEPATH FROM IMPORT_LOG ORDER BY ID DESC";

            string[] fileArray = Directory.GetFiles(Path.Combine(FolderPath), "*.csv");
            if (fileArray.Length == 0)
            {
                throw new BusinessLogicException(ResultCode.ImportClientFileError, "No files in import working folder");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    foreach (var p in fileArray)
                    {
                        var fileName = Path.GetFileName(p);
                        var fileNameNoExtension = Path.GetFileNameWithoutExtension(p);
                        connection.Open();
                        DynamicParameters parameter = new DynamicParameters();
                        parameter.Add("@DATAPATH", FolderPath, DbType.String, ParameterDirection.Input);
                        parameter.Add("@FILENAME", fileName, DbType.String, ParameterDirection.Input);
                        parameter.Add("@FILENAMENOTEXTENSION", fileNameNoExtension, DbType.String, ParameterDirection.Input);
                        connection.Execute(sql, parameter, commandType: CommandType.StoredProcedure, commandTimeout: ExecuteTimeOut);
                        connection.Close();
                    }
                    connection.Open();
                    var resault = connection.Query<ImportLog>(sqlResult).ToList();
                    connection.Close();
                    foreach (var p in resault)
                    {
                        ImportFileAuto_MegaItem(p, FolderPath);
                    }
                }
                catch (Exception ex)
                {
                    throw new BusinessLogicException(ResultCode.ImportClientFileError, "Import client file error", ex);
                }
            }
        }

        private static void ImportFileAuto_MegaItem(ImportLog p, string FolderPath)
        {
            string todate = DateTime.Now.ToString("yyyyMMdd");
            if (!p.IsSuccess)
            {
                if (!Directory.Exists(Path.Combine(FolderPath, PathFail)))
                {
                    Directory.CreateDirectory(Path.Combine(FolderPath, PathFail));
                }

                //Create folder Fail 
                if (!Directory.Exists(Path.Combine(FolderPath, PathFail, todate)))
                {
                    Directory.CreateDirectory(Path.Combine(FolderPath, PathFail, todate));
                }
                string source = p.FilePath;
                string destination = p.FilePath.Replace(FolderPath, Path.Combine(FolderPath, PathFail, PathImportDate + todate));
                File.Move(source, destination);
                throw new BusinessLogicException(ResultCode.ImportClientFileError, "Import client file error");
            }

            //Create folder SUCCESS
            if (!Directory.Exists(Path.Combine(FolderPath, PathSuccess)))
            {
                Directory.CreateDirectory(Path.Combine(FolderPath, PathSuccess));
            }

            //Create folder SUCCESS 
            if (!Directory.Exists(Path.Combine(FolderPath, PathSuccess, todate)))
            {
                Directory.CreateDirectory(Path.Combine(FolderPath, PathSuccess, todate));
            }
            string sourceFile = p.FilePath;
            string destinationFile = p.FilePath.Replace(FolderPath, Path.Combine(FolderPath, PathSuccess, PathImportDate + todate));
            File.Move(sourceFile, destinationFile);
        }

        public static void ImportMailAuto()
        {
            string sql = "SPX_IMPORTEMAILFROMEXCEL";
            string TableName = "CLIENT_MAILDATA";
            string SheetName = GetConfig("SheetName");
            string FileMailPath = GetConfig("FileMailPath");
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");

            string[] fileArray = Directory.GetFiles(Path.Combine(FileMailPath), "*.xlsx");
            if (fileArray.Length == 0)
            {
                throw new BusinessLogicException(ResultCode.ImportClientFileError, "No files mail in import working folder");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    foreach (var p in fileArray)
                    {
                        connection.Open();
                        DynamicParameters parameter = new DynamicParameters();
                        parameter.Add("@SheetName", SheetName, DbType.String, ParameterDirection.Input);
                        parameter.Add("@FilePath", p, DbType.String, ParameterDirection.Input);
                        parameter.Add("@TableName", TableName, DbType.String, ParameterDirection.Input);
                        connection.Execute(sql, parameter, commandType: CommandType.StoredProcedure, commandTimeout: ExecuteTimeOut);
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw new BusinessLogicException(ResultCode.ImportClientFileError, "Import client file error", ex);
                }
            }
        }

        public static void ImportFileAuto_Sino(bool isJob)
        {
            string FolderPath = GetConfig("ImportFile");
            string FormatFolder = GetConfig("FormatFile");
            string FormatFile_Invoice = GetConfig("FormatFile_Invoice");
            string FormatFile_Count = GetConfig("FormatFile_Count");
            string sql = "SP_IMPORTFILE";
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
            string sqlResult = "SELECT TOP 2 ISSUCCESS, FILEPATH FROM IMPORT_LOG ORDER BY ID DESC";
            // get ip
            String ip = IP.GetIPAddress();
            // end get ip
            string sqlInsert = "INSERT INTO SYSTEMLOGS (FUNCTIONCODE, LOGSUMMARY, LOGTYPE, LOGDATE, LOGDETAIL, IP, USERNAME) " +
                    "Values (@FUNCTIONCODE, @LOGSUMMARY, @LOGTYPE, @LOGDATE, @LOGDETAIL, @IP, @USERNAME);";

            //
            if (isJob)
            {
                // Clear all file in working directory
                ClearAllFileInFolder(FolderPath);
                // start funtion: copy file from orther server to FolderImportFile
                CopyFileFromSourceHuanan(FolderPath, DateTime.Now.ToString("yyyyMMdd"));
                // end funtion: copy file from orther server to FolderImportFile
            }

            // Get all files in folder FolderPath
            string[] fileArray = Directory.GetFiles(Path.Combine(FolderPath), "*.txt");
            if (fileArray.Length == 0)
            {
                insertSystemlogSqlServer(ip, sqlInsert, FolderPath, connectionString);
                throw new BusinessLogicException(ResultCode.ImportInvoiceFileError, "No files in import working folder");
            }

            // Get list date invoice
            List<string> myCollection = GetCollection(fileArray);
            foreach (var c in myCollection)
            {
                string[] fileArrayDate = Directory.GetFiles(Path.Combine(FolderPath), "*" + c + ".txt");
                using (var connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        foreach (var p in fileArrayDate)
                        {
                            string fName = p.Substring(FolderPath.Length + 1);
                            connection.Open();
                            DynamicParameters parameter = new DynamicParameters();
                            parameter.Add("@DATAPATH", FolderPath, DbType.String, ParameterDirection.Input);
                            parameter.Add("@DATANAME", fName, DbType.String, ParameterDirection.Input);
                            parameter.Add("@FORMAT_INVOICE_PATH", FormatFolder + @"\" + FormatFile_Invoice, DbType.String, ParameterDirection.Input);
                            parameter.Add("@COUNTPATH", FormatFolder + @"\" + FormatFile_Count, DbType.String, ParameterDirection.Input);
                            connection.Execute(sql, parameter, commandType: CommandType.StoredProcedure, commandTimeout: ExecuteTimeOut);
                            connection.Close();
                        }

                        connection.Open();
                        var result = connection.Query<ImportLog>(sqlResult).ToList();
                        connection.Close();
                        foreach (var p in result)
                        {
                            MoveFileToStore(p, FolderPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger logger = new Logger();
                        logger.Error("ImportInvoice", ex);
                        throw new BusinessLogicException(ResultCode.ImportInvoiceFileSuccess, "Import file error", ex);
                    }
                }
            }
        }

        private static List<string> GetListDate(DateTime fromDate, DateTime toDate, string format = "yyyyMMdd")
        {
            List<string> res = new List<string>();
            DateTime from = fromDate.Date;
            DateTime to = toDate.Date;
            int numberOfDay = Convert.ToInt32((to - from).TotalDays);

            for (int i = 0; i < numberOfDay + 1; i++)
            {
                DateTime midDay = from.AddDays(i);
                res.Add(midDay.ToString(format));
            }
            return res;
        }
    }
}
