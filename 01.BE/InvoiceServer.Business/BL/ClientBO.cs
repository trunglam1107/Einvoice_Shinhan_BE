using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Email;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Portal;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common.Utils;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class ClientBO : IClientBO
    {
        #region Fields, Properties
        private readonly DataClassesDataContext db = new DataClassesDataContext();
        private readonly IClientRepository clientRepository;
        private readonly ILoginUserRepository loginUserRepository;
        private readonly IDbTransactionManager transaction;
        private readonly IInvoiceRepository invoiceRepository;
        private readonly Account account;
        private readonly PrintConfig config;
        private readonly EmailConfig emailConfig;
        private readonly Email email;
        private readonly EmailActiveBO emailActiveBO;
        protected readonly static object lockObjectMail = new object();
        #endregion

        #region Contructor

        public ClientBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.clientRepository = repoFactory.GetRepository<IClientRepository>();
            this.loginUserRepository = repoFactory.GetRepository<ILoginUserRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.account = new Account(repoFactory);
            this.emailActiveBO = new EmailActiveBO(repoFactory);
        }

        public ClientBO(IRepositoryFactory repoFactory, PrintConfig config) : this(repoFactory)
        {
            this.config = config;
        }

        public ClientBO(IRepositoryFactory repoFactory, PrintConfig config, EmailConfig emailConfig) : this(repoFactory, config)
        {
            this.email = new Email(repoFactory);
            this.emailConfig = emailConfig;
        }

        // Constructor for import
        public ClientBO(IRepositoryFactory repoFactory, EmailConfig emailConfig) : this(repoFactory)
        {
            this.emailConfig = emailConfig;
            this.email = new Email(repoFactory);
        }
        #endregion

        #region Methods

        public IEnumerable<ClientDetail> Filter(ConditionSearchClient condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            IQueryable<ClientDetail> clientDetails = this.clientRepository.Filter(condition);
            var projects = clientDetails.OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
            return projects;
        }

        public List<ClientDetail> FilterByCIF(ConditionSearchClient condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var clients = this.clientRepository.FilterByCIF(condition);
            return clients;
        }

        public long Count(ConditionSearchClient condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.clientRepository.Count(condition);
        }

        public long CountByCIF(ConditionSearchClient condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.clientRepository.CountByCIF(condition);
        }

        public ResultCode Create(ClientAddInfo client,long? companyId)
        {
            try
            {
                transaction.BeginTransaction();
                CLIENT clientInfo = CreateClient(client);
                this.account.CreateAccountClient(clientInfo);
                client.Id = clientInfo.ID;
                client.EmailActiveId = CreateEmailAccountInfo(clientInfo, companyId);
                transaction.Commit();
            }
            catch (BusinessLogicException ex)
            {
                transaction.Rollback();
                throw new BusinessLogicException(ex.ErrorCode, ex.Message);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        public long CreateEmailAccountInfo(CLIENT client,long? companyId)
        {
            if (client.CUSTOMERTYPE != (int)CustomerType.IsAccounting)
            {
                return 0;
            }
            long emailNoticeId = 0;
            emailNoticeId = CreateEmailActive(client.ID,companyId);
            return emailNoticeId;
        }
        public Dictionary<long, EMAILACTIVE> CreateEmailListAccountInfo(List<CLIENT> clients)
        {
            var res = CreateEmailActive(clients);
            return res;
        }

        private long CreateEmailActive(long clientId,long? companyId)
        {
            ReceiverAccountActiveInfo NoticeAccountInfo = this.clientRepository.GetClientAccountInfo(clientId);
            EmailInfo emailInfo = EmailTemplate.GetEmail(this.emailConfig, (int)SendEmailType.NoticeAccountCustomer, NoticeAccountInfo);
            emailInfo.RefIds.Add(clientId);  // Task #30077
            long emailActiveId = this.email.CreateEmailActive(emailInfo, (int)TypeEmailActive.ProvideAccountEmail,companyId);
            return emailActiveId;
        }

        public bool UpdateEmailActive(long clientId, long emailActiveId)
        {
            if (clientId == 0 || emailActiveId == 0)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ReceiverAccountActiveInfo NoticeAccountInfo = this.clientRepository.GetClientAccountInfo(clientId);
            EmailInfo emailInfo = EmailTemplate.GetEmail(this.emailConfig, (int)SendEmailType.NoticeAccountCustomer, NoticeAccountInfo);
            return this.email.UpdateEmailActive(emailInfo, emailActiveId);
        }

        private Dictionary<long, EMAILACTIVE> CreateEmailActive(List<CLIENT> clients)
        {
            Dictionary<long, string> clientEmailInfos = new Dictionary<long, string>();
            List<EmailInfo> emailInfos = new List<EmailInfo>();
            foreach (var client in clients)
            {
                ReceiverAccountActiveInfo NoticeAccountInfo = this.clientRepository.GetClientAccountInfo(client.ID);
                EmailInfo emailInfo = EmailTemplate.GetEmail(this.emailConfig, (int)SendEmailType.NoticeAccountCustomer, NoticeAccountInfo);
                emailInfo.RefIds.Add(client.ID); // Task #30077
                emailInfos.Add(emailInfo);
                clientEmailInfos.Add(client.ID, emailInfo.Content);
            }
            var emailInfoActives = this.email.CreateEmailActive(emailInfos, (int)TypeEmailActive.ProvideAccountEmail);

            var res = new Dictionary<long, EMAILACTIVE>();
            foreach (var clientEmailInfo in clientEmailInfos)
            {
                res.Add(clientEmailInfo.Key, emailInfoActives[clientEmailInfo.Value]);
            }
            return res;
        }

        public long CreateEmailActiveByClientId(long clientId, long? companyId)
        {
            long emailNoticeId = 0;
            if (clientId == 0)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            PTClientAccountInfo getClient = this.clientRepository.GetClient(clientId);
            emailNoticeId = CreateEmailActive(clientId, getClient.CompanyId);
            return emailNoticeId;
        }

        private CLIENT CreateClient(ClientAddInfo client)
        {
            if (client == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!client.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            bool isExitTaxCode = this.clientRepository.ContainTax(client.TaxCode);
            if (isExitTaxCode)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceTaxCode,
                                  string.Format("Create client failed because user with tax code = [{0}] is exist.", client.TaxCode));
            }

            bool isExitCustomerCode = this.clientRepository.ContainCustomerCode(client.CustomerCode);
            if (isExitCustomerCode)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceCustomerCode,
                                  string.Format("Create client failed because user with customer code = [{0}] is exist.", client.CustomerCode));
            }

            var clientNew = new CLIENT();
            clientNew.CopyData(client);
            clientNew.CREATEDDATE = DateTime.Now;
            clientNew.CREATEDBY = client.UserAction;
            this.clientRepository.Insert(clientNew);
            return clientNew;
        }

        public ResultCode Update(long id, ClientAddInfo client,long? companyId)
        {
            if (client == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!client.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            CLIENT currentClient = GetClient(id);
            if (!currentClient.TAXCODE.IsEquals(client.TaxCode)        // TaxCode is changed
              && this.clientRepository.ContainTax(client.TaxCode)) // New TaxCode is existed
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceTaxCode,
                                  string.Format("Create client failed because user with tax code = [{0}] is exist.", client.TaxCode));
            }

            currentClient.CopyData(client);
            currentClient.UPDATEDDATE = DateTime.Now;
            currentClient.UPDATEDBY = client.UserAction;

            ResultCode res;
            try
            {
                this.transaction.BeginTransaction();
                res = this.clientRepository.Update(currentClient) ? ResultCode.NoError : ResultCode.UnknownError;
                // get LoginUser truoc khi thuc hien update LoginUser
                var loginUser = this.loginUserRepository.GetByClientId(id);
                this.account.UpdateAccountClient(currentClient);
                // truong hop client la don vi ke toan va chua ton tai LoginUser -> tao mail de gui thong bao account info
                if (loginUser == null && currentClient.CUSTOMERTYPE == (int)CustomerType.IsAccounting)
                {
                    client.EmailActiveId = CreateEmailAccountInfo(currentClient,companyId);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return res;
        }

        public ResultCode Delete(long id, long? companyId)
        {
            CLIENT currentClient = GetClient(id);
            bool clientIsUsed = ClientIsUsed(currentClient.ID);
            if (clientIsUsed)
            {
                throw new BusinessLogicException(ResultCode.DataIsUsed, "Data is used on the invoice can not deleted");
            }
            currentClient.DELETED = true;
            return this.clientRepository.Update(currentClient) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public ResultCode EncryptUserPassword(long clientId)
        {
            ResultCode resultCode;
            try
            {
                this.transaction.BeginTransaction();
                resultCode = this.account.UpdateClientPassword(clientId) ? ResultCode.NoError : ResultCode.UnknownError;
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return resultCode;
        }

        public bool CheckExistLoginUser(long clientId)
        {
            bool result = false;
            LOGINUSER userLogin = this.loginUserRepository.GetByClientId(clientId);
            if (userLogin != null)
            {
                result = true;
            }
            return result;
        }

        public ClientAddInfo GetClientInfo(long id)
        {
            CLIENT client = GetClient(id);
            return new ClientAddInfo(client);
        }

        public ResultImportSheet ImportData(string fullPathFile, long companyId)
        {

            var rowError = new ListError<ImportRowError>();
            var listResultImport = new ResultImportSheet();
            ImportExcel importExcel = new ImportExcel(fullPathFile, ImportClient.SheetNames);
            DataTable dtClients = importExcel.GetBySheetName(ImportClient.SheetName, ImportClient.ColumnImport, ref rowError);
            if (rowError.Count > 0)
            {
                listResultImport.RowError.AddRange(rowError);
                listResultImport.ErrorCode = ResultCode.ImportFileFormatInvalid;
                listResultImport.Message = MsgApiResponse.DataInvalid;
                return listResultImport;
            }

            if (dtClients == null || dtClients.Rows.Count == 0)
            {
                throw new BusinessLogicException(ResultCode.FileUploadImportEmpty, "File upload Import Empty");
            }
            try
            {
                transaction.BeginTransaction();
                listResultImport = ProcessData(companyId, dtClients);
                if (listResultImport.ErrorCode == ResultCode.NoError)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new BusinessLogicException(ResultCode.UnknownError, ex.Message);
            }

            return listResultImport;
        }

        private ResultImportSheet ProcessData(long companyId, DataTable dtDistintc)
        {
            ResultImportSheet listResultImport = new ResultImportSheet();
            int rowIndex = 1;
            foreach (DataRow row in dtDistintc.Rows)
            {
                if (CheckRowIsEmpty(row))
                {
                    rowIndex++;
                    continue;
                }
                listResultImport.RowError.AddRange(Validate(row, rowIndex));
                listResultImport.RowError.AddRange(ExcecuteData(row, rowIndex, companyId));
                rowIndex++;
            }

            if (listResultImport.RowError.Count == 0)
            {
                listResultImport.RowSuccess = (rowIndex - 1);
                listResultImport.ErrorCode = ResultCode.NoError;
                listResultImport.Message = MsgApiResponse.ExecuteSeccessful;
            }
            else
            {
                listResultImport.ErrorCode = ResultCode.ImportDataNotSuccess;
                listResultImport.Message = MsgApiResponse.DataInvalid;
            }

            return listResultImport;
        }

        private bool CheckRowIsEmpty(DataRow row)
        {
            foreach (var column in ImportClient.ColumnImport)
            {
                if (!row[column].IsNullOrWhitespace())
                {
                    return false;
                }
            }

            return true;
        }

        public ExportFileInfo DownloadDataClients(ConditionSearchClient condition)
        {
            var dataRport = new ClientExport(condition.CurrentUser?.Company);
            var clients = Filter(condition);
            dataRport.Items = clients.Select(p => new ClientDetail(p)).ToList();
            ExportClientView export = new ExportClientView(dataRport, config);
            return export.ExportFile();
        }
        public ExportFileInfo ExportExcel(CompanyInfo company, string branch, string userid, string branchId)
        {
            long _branch = long.Parse(branch ?? company.Id.ToString());
            var dataRport = new LoginUserExport(company);
            var clients = this.db.Set<LOGINUSER>().Where(p => p.COMPANYID == (_branch == 0 ? p.COMPANYID : _branch) && p.COMPANYID != null && p.DELETED != true && p.USERID == (userid ?? p.USERID)).ToList();
            var companies = this.db.Set<MYCOMPANY>().ToList();
            var listUser = (from client in clients
                            join comp in companies on client.COMPANYID equals comp.COMPANYSID
                            where client.USERID == (userid ?? client.USERID)
                            select new LoginUserExportDetail()
                            {
                                ID = client.USERSID,
                                BranchId = comp.BRANCHID ?? "",
                                ChiNhanh =  comp.COMPANYNAME ?? "",
                                TenNhanVien = client.USERNAME ?? "",
                                Email = client.EMAIL ?? "",
                                SoDienThoai = client.MOBILE ?? "",
                                TrangThai = client.ISACTIVE.ToString() ?? "",
                                TenTaiKhoan = client.USERID ?? "",
                                TenQuyen = client.ROLEs?.Select(p => p.ID).FirstOrDefault() + " - " + client.ROLEs?.Select(p => p.NAME).FirstOrDefault() ?? "",
                                NgayKichHoat = client.CREATEDDATE.ConvertToString() ?? "",
                                NgayHetHieuLuc = client.EFFECTIVEDATE.ConvertToString() ?? ""
                            }).OrderByDescending(x => x.TenTaiKhoan).ToList();
            if (!string.IsNullOrEmpty(branchId))
            {
                listUser = listUser.Where(p => p.BranchId == branchId).ToList();
            }
            dataRport.Items = listUser;



            ExportLoginUserView export = new ExportLoginUserView(dataRport, config);
            return export.ExportFile();
        }

        public bool MyClientUsing(long id)
        {
            return clientRepository.MyClientUsing(id);
        }

        public ResultCode CreateResult(ClientAddInfo clientInfo, ResultCode result, UserSessionInfo CurrentUser)
        {
            var clientEmail = clientInfo.UseRegisterEmail != true ? clientInfo.ReceivedInvoiceEmail : clientInfo.Email;
            if (result == ResultCode.NoError && clientInfo.CustomerType == (int)CustomerType.IsAccounting && !string.IsNullOrEmpty(clientEmail))
            {
                result = this.emailActiveBO.SendEmailProvideAccount(clientInfo);
                if (result == ResultCode.NoError)
                {
                    //encrypt password after send email account infomation
                    result = EncryptUserPassword(clientInfo.Id);
                }
            }
            return result;

        }
        #endregion

        #region Private Methods

        private CLIENT GetClient(long id)
        {
            CLIENT client = this.clientRepository.GetById(id);
            if (client == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.ResouceIdNotFound);
            }

            return client;
        }


        #endregion

        private ListError<ImportRowError> Validate(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            listError.Add(ValidDataIsNull(row, ImportClient.CustomerName, rowIndex));
            if (row[ImportClient.Organization] != null && row[ImportClient.Organization].Trim() == "1")
            {
                listError.Add(ValidDataIsNull(row, ImportClient.TaxCode, rowIndex));
            }
            listError.Add(ValidDataIsNull(row, ImportClient.Mobile, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportClient.Address, rowIndex));

            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength12, ImportClient.Mobile, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength12, ImportClient.Fax, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength50, ImportClient.TaxCode, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength50, ImportClient.Email, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength50, ImportClient.PersonContact, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength50, ImportClient.BankName, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength25, ImportClient.BankAccount, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength50, ImportClient.AccountHolder, rowIndex));

            return listError;
        }

        private ImportRowError ValidDataIsNull(DataRow row, string colunName, int rowIndex)
        {
            ImportRowError errorColumn = null;
            try
            {
                if (!row[colunName].IsNullOrWhitespace())
                {
                    return errorColumn;
                }

                return new ImportRowError(ResultCode.ImportDataIsEmpty, colunName, rowIndex);
            }
            catch (Exception)
            {
                return errorColumn;
            }
        }

        private ImportRowError ValidDataMaxLength(DataRow row, int maxLength, string colunName, int rowIndex)
        {
            ImportRowError errorColumn = null;
            try
            {
                if (row[colunName].IsNullOrEmpty() || row[colunName].ToString().Length < maxLength)
                {
                    return errorColumn;
                }

                return new ImportRowError(ResultCode.ImportDataExceedMaxLength, colunName, rowIndex);
            }
            catch (Exception)
            {

                return errorColumn;
            }
        }

        private ListError<ImportRowError> ExcecuteData(DataRow row, int rowIndex, long companyId)
        {
            var listError = new ListError<ImportRowError>();
            var client = FilterClient(companyId, row[ImportClient.TaxCode].Trim(), row[ImportClient.CustomerName].Trim());
            if (client == null)
            {
                listError.AddRange(InsertClient(row, rowIndex));
            }
            else
            {
                UpdateClient(client, row);
            }

            return listError;

        }

        private CLIENT FilterClient(long companyId, string taxCode, string customerName)
        {
            return this.clientRepository.FilterClient(companyId, taxCode, customerName);
        }

        private ListError<ImportRowError> InsertClient(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            bool isExitedTax = this.clientRepository.ContainTax(row[ImportClient.TaxCode].ToString());
            if (isExitedTax)
            {
                listError.Add(new ImportRowError(ResultCode.ImportDataIsExisted, ImportClient.TaxCode, rowIndex, row[ImportClient.TaxCode].ToString()));
            }

            if (listError.Count > 0)
            {
                return listError;
            }

            CLIENT client = new CLIENT();
            client.CopyData(row);
            client.CREATEDDATE = DateTime.Now;
            this.clientRepository.Insert(client);
            this.account.CreateAccountClient(client);
            return listError;
        }

        private void UpdateClient(CLIENT currentClient, DataRow row)
        {
            currentClient.CopyData(row);
            this.clientRepository.Update(currentClient);
            this.account.UpdateAccountClient(currentClient);
        }

        private bool ClientIsUsed(long id)
        {
            if (this.invoiceRepository.ContainClient(id))
                return this.invoiceRepository.GetInvoiceByClient(id);
            return false;
        }
        public ResultCode ImportClientFile()
        {
            ImportFile.ImportMailAuto();
            return ResultCode.ImportClientFileSuccess;
        }

        public List<ClientDetail> ClientListSuggestion(ConditionSearchClient condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            List<ClientDetail> clientDetails = this.clientRepository.ClientListSuggestion(condition);
            return clientDetails;
        }
    }
}