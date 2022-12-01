using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public partial class ImportFileDAO : GenericRepository<INVOICE>, IImportFileDAO
    {
        private readonly IDbTransactionManager transaction;
        private readonly IRepositoryFactory repoFactory;
        private readonly UserSessionInfo CurrentUser;
        private readonly Logger logger = new Logger();
        private readonly DataClassesDataContext dbContext;
        readonly string PathSuccess = "Success";
        readonly string PathFail = "Fail";
        readonly string PathImportDate = "ImportDate_";
        private readonly bool DeleteFileInputAfterImport = false;

        private static readonly Func<string, string> GetConfig = key => CommonUtil.GetConfig(key);

        public ImportFileDAO(IDbContext context, IRepositoryFactory repositoryFactory, UserSessionInfo userSessionInfo)
            : base(context)
        {
            this.dbContext = context as DataClassesDataContext;
            this.repoFactory = repositoryFactory;
            this.CurrentUser = userSessionInfo;
            this.transaction = repositoryFactory.GetRepository<IDbTransactionManager>();

            try
            {
                this.DeleteFileInputAfterImport = CommonUtil.GetConfig("DeleteFileInputAfterImport", false);
            }
            catch (Exception)
            {
                logger.Trace("config key DeleteFileInputAfterImport is false");
            }
        }

        private void SendMailFromList(List<ClientAddInfo> clientAddInfos)
        {
            // Send mail sau khi import thành công
            ClientBO clientBO = new ClientBO(this.repoFactory);

            for (int index = 0; index < clientAddInfos.Count; index++)
            {
                try
                {
                    var clientInfo = clientAddInfos[index];
                    var resultSendMail = ResultCode.NoError;
                    clientBO.CreateResult(clientInfo, resultSendMail, this.CurrentUser);
                    if (resultSendMail != ResultCode.NoError)
                    {
                        logger.Trace($"ImportInvoice: import done, send mail detail error. ClientID: {clientInfo.Id}, Email: {clientInfo.Email}");
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        private void AddUserSendMail(List<CLIENT> clients, ClientBO clientBO, List<ClientAddInfo> listSendMail)
        {
            var clientIdEmailAtives = clientBO.CreateEmailListAccountInfo(clients);

            foreach (var client in clients)
            {
                var clientAddInfo = new ClientAddInfo();
                clientAddInfo.UseRegisterEmail = false;
                clientAddInfo.Email = client.EMAIL;
                clientAddInfo.ReceivedInvoiceEmail = client.RECEIVEDINVOICEEMAIL;
                clientAddInfo.CustomerType = (int)CustomerType.IsAccounting;
                clientAddInfo.Id = client.ID;
                clientAddInfo.EmailActiveId = clientIdEmailAtives[client.ID].ID;
                listSendMail.Add(clientAddInfo);
            }
        }

        private void ClearAllFileInWorkingFolder(string FolderPath)
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
                        logger.Trace("Class:ImportFileDAO Method:ClearAllFileInFolder" + Environment.NewLine + ex.Message);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InsertSystemLog(String ip, string FolderPath)
        {
            SYSTEMLOG systemLog = new SYSTEMLOG()
            {
                FUNCTIONCODE = "quartzJobManagement",
                LOGSUMMARY = "Import invoice",
                LOGTYPE = 1,
                LOGDATE = DateTime.Now,
                LOGDETAIL = "Not found file in " + FolderPath,
                IP = ip,
                USERNAME = "Quarzt",
            };
            this.Insert(dbContext.SYSTEMLOGS, systemLog);
        }

        private void InitImportDBLog(string[] fileArrayDate, string FolderPath)
        {
            foreach (var filePath in fileArrayDate)
            {
                IMPORT_LOG importLog = new IMPORT_LOG()
                {
                    FILEPATH = filePath,
                    FILENAME = filePath.Substring(FolderPath.Length + 1),
                    DATE = DateTime.Now.ToString(),
                    ISSUCCESS = false,
                };
                this.Insert(dbContext.IMPORT_LOG, importLog);
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

        private void RemoveOldFilesStored(string destinationFolderPath, int MaxDayStoreImport)
        {
            if (!Directory.Exists(destinationFolderPath))
            {
                return;
            }
            DateTime toDate = DateTime.Now;
            DateTime fromDate = DateTime.MinValue;
            try
            {
                fromDate = toDate.AddDays(-MaxDayStoreImport);
            }
            catch (Exception ex)
            {
                logger.Trace("RemoveOldFilesStored, get fromDate error", ex);
            }
            var listDayString = GetListDate(fromDate, toDate, "yyyyMMdd");

            var pathSuccess = Path.Combine(destinationFolderPath, PathSuccess);
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

            var pathFaild = Path.Combine(destinationFolderPath, PathFail);
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

        public bool Insert<T>(IDbSet<T> dbSet, T entity)
            where T : class
        {
            try
            {
                dbSet.Add(entity);
                dbContext.BulkSaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
            }

            return true;
        }

        public bool Update<T>(T entity)
            where T : class
        {
            try
            {
                dbContext.BulkSaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
            }

            return true;
        }
    }
}