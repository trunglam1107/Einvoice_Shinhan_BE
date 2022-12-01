using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common.Models;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace InvoiceServer.Business.DAO
{
    partial class ImportFileDAO
    {
        protected readonly static object lockObjectImportBIDC = new object();
        readonly List<ClientAddInfo> listSendMailBIDC = new List<ClientAddInfo>();

        // Chuyển sang import từ store, không làm từ code như ImportFileAuto_BIDC nữa
        public void ImportFileAuto_BIDC(bool isJob)
        {
            string FolderPath = GetConfig("ImportFile");
            int MaxDayStoreImport = int.Parse(GetConfig("MaxDayStoreImport"));
            int MaxDayReadImport = int.Parse(GetConfig("MaxDayReadImport"));
            String ip = IP.GetIPAddress();

            if (isJob)
            {
                // Clear all file in working directory
                ClearAllFileInWorkingFolder(FolderPath);
                // Clear all store files out of date
                RemoveOldFilesStored(FolderPath, MaxDayStoreImport);
                // copy file from orther server to FolderImportFile
                CopyMultiFileFromSourceBIDC(FolderPath, MaxDayReadImport);
                // Thêm khách hàng vãng lai cho BIDC
                SeedClientBIDC();
            }

            // Get all files in folder FolderPath
            string[] fileArray = Directory.GetFiles(Path.Combine(FolderPath));
            if (fileArray.Length == 0)
            {
                this.InsertSystemLog(ip, "Not found file in " + FolderPath, 1);
                throw new BusinessLogicException(ResultCode.ImportInvoiceFileError, "No files in import working folder");
            }

            // Get list date invoice
            List<string> myCollection = GetCollectionBIDC(fileArray);
            foreach (var c in myCollection)
            {
                string[] fileArrayDate = Directory.GetFiles(Path.Combine(FolderPath), "*." + c);
                this.InitImportDBLog(fileArrayDate, FolderPath);
                try
                {
                    this.transaction.BeginTransaction();
                    // Import vào DB
                    var clients = ImportFileForDayBIDC(fileArrayDate, FolderPath);
                    this.InsertSystemLog(ip, "Import invoices success", 0);
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
                            FileName = e.FILENAME,
                        })
                        .ToList();

                    foreach (var p in result)
                    {
                        try
                        {
                            MoveFileToStore(p, FolderPath);
                            DeleteFileInputImportSuccessBIDC(p);
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"ImportInvoice: MoveFileToStore({p?.FilePath}, {FolderPath})", ex);
                        }
                    }
                    // End chuyển file sang thư mục store

                    // Tạo account và gửi mail cho các user vừa import
                    this.transaction.BeginTransaction();
                    CreateAccountingAccountBIDC(clients);
                    this.transaction.Commit();

                }
                catch (Exception ex)
                {
                    this.transaction.Rollback();
                    throw new BusinessLogicException(ResultCode.ImportInvoiceFileError, "Import file error", ex);
                }
            }

            try
            {
                // Send mail sau khi import thành công
                this.SendMailFromList(this.listSendMailBIDC);
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(ResultCode.SendMailError, "Send mail import file error", ex);
            }
        }

        private List<CLIENT> ImportFileForDayBIDC(string[] fileArrayDate, string FolderPath)
        {
            List<ImportIndexError> clientRawErrors = new List<ImportIndexError>();
            List<ImportIndexError> invoiceRawErrors = new List<ImportIndexError>();
            List<InvoiceImportBIDC> invoicesRaw = new List<InvoiceImportBIDC>();
            var clients = new List<CLIENT>();
            var invoices = new List<INVOICE>();

            // Convert file data to model
            int isParseError = 0;
            foreach (var p in fileArrayDate)
            {
                if (p.Substring(FolderPath.Length + 1).Contains("CLIENT"))
                {
                    clients = GetDataClientBIDC(p, clientRawErrors);
                    if (WriteErrorLog(clientRawErrors, p, FolderPath))
                    {
                        isParseError += 1;
                    }
                }
                else
                {
                    invoices = GetDataInvoiceBIDC(p, invoiceRawErrors, invoicesRaw);
                    if (WriteErrorLog(invoiceRawErrors, p, FolderPath))
                    {
                        isParseError += 2;
                    }
                }
            }

            if (isParseError > 0)
            {
                this.WriteLog(isParseError);
            }

            var importCurrentClient = clients.FirstOrDefault(e => e.CUSTOMERCODE == BIDCDefaultFields.CURRENT_CLIENT); // khách hàng vãng lai
            if (importCurrentClient == null)
            {
                var currentClient = this.dbContext.CLIENTs.FirstOrDefault(e => e.CUSTOMERCODE == BIDCDefaultFields.CURRENT_CLIENT); // khách hàng vãng lai
                if (currentClient != null)
                    clients.Add(currentClient);    // add khách hàng vãng lai
            }
            MergeClientBIDC(clients);
            InsertIvoiceBIDC(invoices, clients, invoicesRaw);

            UpdateSuccessImportDBLog(fileArrayDate, FolderPath);
            return clients;
        }

        private List<CLIENT> WriteLog(int isParseError)
        {
            if (isParseError == 1)
                logger.Error("", new BusinessLogicException(ResultCode.ImportParseDataClientError, "Parse data client error!"));
            if (isParseError == 2)
                logger.Error("", new BusinessLogicException(ResultCode.ImportParseDataInvoiceError, "Parse data invoice error!"));
            if (isParseError == 3)
                logger.Error("", new BusinessLogicException(ResultCode.ImportParseDataClientAndInvoiceError, "Parse data client and invoice error!"));
            return new List<CLIENT>();
        }

        private List<CLIENT> GetDataClientBIDC(string pathClient, List<ImportIndexError> errors)
        {
            List<CLIENT> res = new List<CLIENT>();
            var rawClients = File.ReadAllLines(pathClient);
            if (rawClients.Length <= 0)
            {
                return res;
            }

            List<dynamic> resTemp = new List<dynamic>();
            Parallel.For(0, rawClients.Length, index =>
            {
                string fieldError = "Split step";
                string valueFieldError = "Start convert";
                try
                {
                    var clientParam = rawClients[index].Split('#').Select(x => x.Trim()).ToArray();
                    if (clientParam == null || clientParam.Length == 0)
                    {
                        return;
                    }

                    CLIENT clientImportBIDC = new CLIENT();
                    fieldError = "IsOrg";
                    valueFieldError = clientParam[0];
                    clientImportBIDC.ISORG = clientParam[0] == "1";
                    fieldError = "TaxCode";
                    valueFieldError = clientParam[1];
                    clientImportBIDC.TAXCODE = clientParam[1];
                    fieldError = "CustomerCode";
                    valueFieldError = clientParam[2];
                    clientImportBIDC.CUSTOMERCODE = string.IsNullOrEmpty(clientParam[2]) ? BIDCDefaultFields.CURRENT_CLIENT : clientParam[2];
                    fieldError = "CustomerName";
                    valueFieldError = clientParam[3];
                    if (clientImportBIDC.ISORG == true)
                    {
                        clientImportBIDC.CUSTOMERNAME = clientParam[3];
                    }
                    fieldError = "Address";
                    valueFieldError = clientParam[4];
                    clientImportBIDC.ADDRESS = clientParam[4];

                    fieldError = "SendInvoiceByMonth";
                    valueFieldError = clientParam[5];
                    clientImportBIDC.SENDINVOICEBYMONTH = clientParam[5] == "1";
                    fieldError = "DateSendInvoice";
                    valueFieldError = clientParam[6];
                    var parse = int.TryParse(clientParam[6], out int dateSendInvoice);
                    if (parse == true)
                    {
                        clientImportBIDC.DATESENDINVOICE = dateSendInvoice;
                    }
                    fieldError = "Mobile";
                    valueFieldError = clientParam[7];
                    clientImportBIDC.MOBILE = clientParam[7];
                    fieldError = "Email";
                    valueFieldError = clientParam[8];
                    clientImportBIDC.EMAIL = clientParam[8];
                    clientImportBIDC.RECEIVEDINVOICEEMAIL = clientParam[8];
                    fieldError = "Delegate";
                    valueFieldError = clientParam[9];
                    clientImportBIDC.DELEGATE = clientParam[9];

                    fieldError = "PersonalContact";
                    valueFieldError = clientParam[10];
                    if (clientImportBIDC.ISORG == true)
                    {
                        clientImportBIDC.PERSONCONTACT = clientParam[10];
                    }
                    else
                    {
                        clientImportBIDC.PERSONCONTACT = clientParam[3];
                    }

                    fieldError = "BankAccount";
                    valueFieldError = clientParam[11];
                    clientImportBIDC.BANKACCOUNT = clientParam[11];
                    fieldError = "AccountHolder";
                    valueFieldError = clientParam[12];
                    clientImportBIDC.ACCOUNTHOLDER = clientParam[12];
                    fieldError = "BankName";
                    valueFieldError = clientParam[13];
                    clientImportBIDC.BANKNAME = clientParam[13];

                    fieldError = "IsCreateAccount";
                    valueFieldError = clientParam[14];
                    int isCreateAccount = int.Parse(clientParam[14]);
                    clientImportBIDC.CUSTOMERTYPE = isCreateAccount == 1 ? (int)CustomerType.IsAccounting : (int)CustomerType.NoneAccounting; // CUSTOMERTYPE: IsAccounting, NoneAccounting

                    lock (lockObjectImportBIDC)
                    {
                        resTemp.Add(new { index, clientImportBIDC });
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObjectImportBIDC)
                    {
                        errors.Add(new ImportIndexError(index, ex, fieldError, valueFieldError, rawClients[index]));
                    }
                }
            });
            res = resTemp.OrderBy(x => x.index).Select(x => x.clientImportBIDC as CLIENT).ToList();
            errors.Sort((x, y) => x.Index.CompareTo(y.Index));
            return res;
        }

        private List<INVOICE> GetDataInvoiceBIDC(string pathInvoice, List<ImportIndexError> errors, List<InvoiceImportBIDC> invoicesRaw)
        {
            var textRawInvoice = File.ReadAllLines(pathInvoice);
            if (textRawInvoice.Length <= 0)
            {
                return new List<INVOICE>();
            }

            Parallel.For(0, textRawInvoice.Length, index =>
            {
                string fieldError = "Split step";
                string valueFieldError = "Start convert";
                try
                {
                    var invoiceParam = textRawInvoice[index].Split('#').Select(x => x.Trim()).ToArray();
                    if (invoiceParam == null || invoiceParam.Length == 0)
                    {
                        return;
                    }

                    InvoiceImportBIDC invoiceImportBIDC = new InvoiceImportBIDC();
                    invoiceImportBIDC.IndexImport = index;
                    fieldError = "BranchId";
                    valueFieldError = invoiceParam[0];
                    invoiceImportBIDC.BranchId = invoiceParam[0];
                    fieldError = "InvoiceNo";
                    valueFieldError = invoiceParam[1];
                    invoiceImportBIDC.InvoiceNo = invoiceParam[1];
                    fieldError = "InvoiceDate";
                    valueFieldError = invoiceParam[2];
                    DateTime invoiceDate = DateTime.ParseExact(invoiceParam[2], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    invoiceImportBIDC.InvoiceDate = invoiceDate;
                    fieldError = "CustomerCode";
                    valueFieldError = invoiceParam[3];
                    invoiceImportBIDC.CustomerCode = string.IsNullOrEmpty(invoiceParam[3]) ? BIDCDefaultFields.CURRENT_CLIENT : invoiceParam[3];
                    fieldError = "PaymentMethod";
                    valueFieldError = invoiceParam[4];
                    invoiceImportBIDC.PaymentMethod = invoiceParam[4];

                    fieldError = "CurrencyCode";
                    valueFieldError = invoiceParam[5];
                    invoiceImportBIDC.CurrencyCode = invoiceParam[5];
                    fieldError = "ExchangeRate";
                    valueFieldError = invoiceParam[6];
                    decimal exchangeRate = Decimal.Parse(invoiceParam[6]);
                    invoiceImportBIDC.ExchangeRate = exchangeRate;
                    fieldError = "Description";
                    valueFieldError = invoiceParam[7];
                    invoiceImportBIDC.Description = invoiceParam[7];
                    fieldError = "Quantity";
                    valueFieldError = invoiceParam[8];
                    invoiceImportBIDC.Quantity = Decimal.Parse(invoiceParam[8]);
                    fieldError = "Amount";
                    valueFieldError = invoiceParam[9];
                    decimal amount = Decimal.Parse(invoiceParam[9]);
                    invoiceImportBIDC.Amount = amount;
                    fieldError = "VatRate";
                    valueFieldError = invoiceParam[10];
                    var vatRate = invoiceParam[10];
                    if (string.IsNullOrEmpty(vatRate))
                        invoiceImportBIDC.VatRate = "TS0";
                    else if (vatRate == "0")
                        invoiceImportBIDC.VatRate = "T00";
                    else if (vatRate == "5")
                        invoiceImportBIDC.VatRate = "T05";
                    else invoiceImportBIDC.VatRate = "T10";

                    fieldError = "TaxAmount";
                    valueFieldError = invoiceParam[11];
                    Decimal.TryParse(invoiceParam[11], out decimal taxAmount);
                    invoiceImportBIDC.TaxAmount = taxAmount;
                    fieldError = "TotalAmount";
                    valueFieldError = invoiceParam[12];
                    decimal totalAmount = Decimal.Parse(invoiceParam[12]);
                    invoiceImportBIDC.TotalAmount = totalAmount;
                    fieldError = "BankRef";
                    valueFieldError = invoiceParam[13];
                    invoiceImportBIDC.BankRef = invoiceParam[13];
                    fieldError = "BankAccount";
                    valueFieldError = invoiceParam[14];
                    invoiceImportBIDC.BankAccount = invoiceParam[14];

                    if (invoiceImportBIDC.CurrentTransactions)  // Giao dịch vãng lai
                    {
                        fieldError = "TaxCode";
                        valueFieldError = invoiceParam[15];
                        invoiceImportBIDC.TaxCode = invoiceParam[15];
                        fieldError = "CustomerName";
                        valueFieldError = invoiceParam[16];
                        invoiceImportBIDC.CustomerName = invoiceParam[16];
                        fieldError = "Address";
                        valueFieldError = invoiceParam[17];
                        invoiceImportBIDC.Address = invoiceParam[17];
                    }

                    lock (lockObjectImportBIDC)
                    {
                        invoicesRaw.Add(invoiceImportBIDC);
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObjectImportBIDC)
                    {
                        errors.Add(new ImportIndexError(index, ex, fieldError, valueFieldError, textRawInvoice[index]));
                    }
                }
            });

            if (errors.Count > 0)
            {
                errors.Sort((x, y) => x.Index.CompareTo(y.Index));
                return new List<INVOICE>();
            }

            var listCompany = GetBranchIdFromDBBIDC(invoicesRaw);
            var registerTemplateInfos = GetRegisterTemplateIdsBIDC().ToList();
            var listTypePayment = GetListTypePaymentBIDC();
            var listCurrency = GetListCurrencyBIDC(invoicesRaw);
            var listTax = GetListTaxBIDC(invoicesRaw);

            this.RemoveInvoiceEixtedBIDC(invoicesRaw, listCompany);

            Dictionary<string, INVOICE> dicInvoice = new Dictionary<string, INVOICE>();

            for (int index = 0; index < invoicesRaw.Count; index++)
            {
                var item = invoicesRaw[index];
                string fieldError = "Convert invoice";
                try
                {
                    fieldError = "INVOICEDETAIL.TAXID";
                    item.TaxId = listTax.FirstOrDefault(x => x.CODE == item.VatRate).ID;

                    INVOICEDETAIL invoiceDetail = new INVOICEDETAIL();
                    invoiceDetail.PRICE = item.Amount;
                    invoiceDetail.TAXID = item.TaxId;
                    invoiceDetail.QUANTITY = item.Quantity;
                    invoiceDetail.TOTAL = item.Amount;
                    invoiceDetail.AMOUNTTAX = item.TaxAmount;
                    invoiceDetail.PRODUCTNAME = item.Description;

                    INVOICE invoice = new INVOICE();
                    fieldError = "INVOICE.COMPANYID";
                    var companyInfo = listCompany.FirstOrDefault(com => com.BranchId == item.BranchId);
                    invoice.COMPANYID = companyInfo.CompanySID;
                    fieldError = "INVOICE.REGISTERTEMPLATEID";
                    var registerTemplateInfo = registerTemplateInfos.FirstOrDefault(reg => reg.CompanyId == invoice.COMPANYID && reg.UsedDate.Value <= item.InvoiceDate);
                    invoice.REGISTERTEMPLATEID = registerTemplateInfo.RegisterTemplateId;
                    fieldError = "INVOICE.SYMBOL";
                    invoice.SYMBOL = registerTemplateInfo.Symbol;
                    fieldError = "INVOICE.NUMBERACCOUT";
                    invoice.CUSTOMERBANKACC = item.BankAccount;
                    fieldError = "INVOICE.TYPEPAYMENT";
                    invoice.TYPEPAYMENT = listTypePayment.FirstOrDefault(t => t.CODE == item.PaymentMethod)?.ID;
                    fieldError = "INVOICE.INVOICESTATUS";
                    invoice.INVOICESTATUS = 1;
                    fieldError = "INVOICE.TOTALTAX";
                    invoice.TOTALTAX = invoicesRaw.Where(ir2 => ir2.BankRef == item.BankRef).Sum(ir2 => ir2.TaxAmount);
                    fieldError = "INVOICE.TOTAL";
                    invoice.TOTAL = invoicesRaw.Where(ir2 => ir2.BankRef == item.BankRef).Sum(ir2 => ir2.Amount);
                    fieldError = "INVOICE.SUM";
                    invoice.SUM = invoicesRaw.Where(ir2 => ir2.BankRef == item.BankRef).Sum(ir2 => ir2.TotalAmount);
                    fieldError = "INVOICE.RELEASEDDATE";
                    invoice.RELEASEDDATE = item.InvoiceDate;
                    invoice.CREATEDDATE = DateTime.Now;
                    fieldError = "INVOICE.CURRENCYID";
                    invoice.CURRENCYID = listCurrency.FirstOrDefault(currency => currency.CODE == item.CurrencyCode)?.ID;
                    fieldError = "INVOICE.CURRENCYEXCHANGERATE";
                    invoice.CURRENCYEXCHANGERATE = item.ExchangeRate;
                    fieldError = "INVOICE.COMPANYNAME";
                    invoice.COMPANYNAME = companyInfo.CompanyName;
                    fieldError = "INVOICE.COMPANYADDRESS";
                    invoice.COMPANYADDRESS = companyInfo.CompanyAddress;
                    fieldError = "INVOICE.COMPANYTAXCODE";
                    invoice.COMPANYTAXCODE = companyInfo.CompanyTaxCode;
                    fieldError = "INVOICE.REFNUMBER";
                    invoice.REFNUMBER = item.BankRef;
                    fieldError = "INVOICE.REFINVOICENO";
                    invoice.REFINVOICENO = item.InvoiceNo;
                    invoice.NO = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
                    invoice.INVOICENO = 0;

                    lock (lockObjectImportBIDC)
                    {
                        if (!dicInvoice.ContainsKey(invoice.REFNUMBER))
                        {
                            dicInvoice.Add(invoice.REFNUMBER, invoice);
                        }
                        dicInvoice[invoice.REFNUMBER].INVOICEDETAILs.Add(invoiceDetail);
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObjectImportBIDC)
                    {
                        errors.Add(new ImportIndexError(item.IndexImport, ex, fieldError, "", "Convert to INVOICE, INVOICEDETAIL object"));
                    }
                }
            }

            errors.Sort((x, y) => x.Index.CompareTo(y.Index));
            var listInvoice = dicInvoice.Values.ToList();
            return listInvoice;
        }

        private void MergeClientBIDC(List<CLIENT> clients)
        {
            // Dùng Z.EntityFramework.Extensions.dll trong thư mục DLL chứ không lấy từ Nuget
            dbContext.BulkMerge(clients, option =>
            {
                option.ColumnPrimaryKeyExpression = c => c.CUSTOMERCODE;
                option.IgnoreOnMergeUpdateExpression = c => new
                {
                    c.SENDINVOICEBYMONTH,
                    c.DATESENDINVOICE,
                };
                option.ColumnInputExpression = c => new
                {
                    c.ACCOUNTHOLDER,
                    c.ADDRESS,
                    c.BANKACCOUNT,
                    c.BANKNAME,
                    c.CUSTOMERCODE,
                    c.CUSTOMERNAME,
                    c.CUSTOMERTYPE,
                    c.DELEGATE,
                    c.DATESENDINVOICE,
                    c.EMAIL,
                    c.RECEIVEDINVOICEEMAIL,
                    c.ISORG,
                    c.MOBILE,
                    c.PERSONCONTACT,
                    c.SENDINVOICEBYMONTH,
                    c.TAXCODE,
                    c.DELETED,
                };
                option.ColumnOutputExpression = c => new
                {
                    c.ID
                };
            });
            dbContext.BulkSaveChanges();
        }

        private List<CLIENT> GetListClientExistsBIDC(List<ClientImportBIDC> clientsRaw)
        {
            var listClientCode = clientsRaw.Select(x => x.CustomerCode).Distinct();

            var listClient = dbContext.CLIENTs.Where(e => listClientCode.Contains(e.CUSTOMERCODE));
            var res = listClient.ToList();
            return res;
        }

        private void InsertIvoiceBIDC(List<INVOICE> invoices, List<CLIENT> clients, List<InvoiceImportBIDC> invoicesRaw)
        {
            foreach (var invoice in invoices)
            {
                var invoiceRaw = invoicesRaw.First(e => e.BankRef == invoice.REFNUMBER);
                var client = clients.FirstOrDefault(e => e.CUSTOMERCODE == invoiceRaw.CustomerCode);
                invoice.CLIENTID = client?.ID;
                if (invoiceRaw.CurrentTransactions) // Giao dịch vãng lai
                {
                    if (string.IsNullOrEmpty(invoiceRaw.TaxCode))
                        invoice.PERSONCONTACT = invoiceRaw.CustomerName;
                    else
                        invoice.CUSTOMERNAME = invoiceRaw.CustomerName;
                    invoice.CUSTOMERTAXCODE = invoiceRaw.TaxCode;
                    invoice.CUSTOMERADDRESS = invoiceRaw.Address;
                }
                else
                {
                    invoice.CUSTOMERNAME = client?.CUSTOMERNAME;
                    invoice.PERSONCONTACT = client?.PERSONCONTACT;
                    invoice.CUSTOMERTAXCODE = client?.TAXCODE;
                    invoice.CUSTOMERADDRESS = client?.ADDRESS;
                }
            }

            dbContext.INVOICEs.AddRange(invoices);
            dbContext.BulkSaveChanges();
        }

        private void CreateAccountingAccountBIDC(List<CLIENT> clients)
        {
            var clientsCreateAccountQuery = clients.Where(e =>
                                e.CUSTOMERTYPE == (int)CustomerType.IsAccounting
                                && e.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT);
            if (clientsCreateAccountQuery.Count() == 0)
                return;
            var clientsAccounting = clientsCreateAccountQuery.ToList();

            var usersExists =
                //#if DEBUG
                //                new List<LOGINUSER>();
                //#else
                GetListLoginUserBIDC(clientsAccounting);
            //#endif

            var listClientCreateAccountAndSendMail = new List<CLIENT>();
            foreach (var client in clientsAccounting)
            {
                var user = usersExists.FirstOrDefault(e => e.CLIENTID == client.ID);
                if (user == null)
                {
                    listClientCreateAccountAndSendMail.Add(client);
                }
                else
                {
                    user.DELETED = false;
                    user.ISACTIVE = true;
                }
            }

            Account account = new Account(this.repoFactory);
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(GetConfig("EmailTemplateFilePath")),
            };
            ClientBO clientBO = new ClientBO(this.repoFactory, emailConfig);
            account.CreateAccountListClient(listClientCreateAccountAndSendMail);
            AddUserSendMail(listClientCreateAccountAndSendMail, clientBO, this.listSendMailBIDC);
        }

        private List<LOGINUSER> GetListLoginUserBIDC(List<CLIENT> clients)
        {
            var listCLientId = clients.Select(e => e.ID).ToList();
            var res = this.dbContext.LOGINUSERs.Where(e => listCLientId.Contains(e.CLIENTID ?? 0));
            return res.ToList();
        }

        private List<ImportGetInfo> GetBranchIdFromDBBIDC(List<InvoiceImportBIDC> invoicesRaw)
        {
            var listBranchId = invoicesRaw.Select(x => x.BranchId).Distinct();

            var companies = dbContext.MYCOMPANies.Where(e => listBranchId.Contains(e.BRANCHID)).ToList();
            return companies.Select(e => new ImportGetInfo()
            {
                CompanySID = e.COMPANYSID,
                BranchId = e.BRANCHID,
                CompanyName = e.COMPANYNAME,
                CompanyAddress = e.ADDRESS,
                CompanyTaxCode = e.TAXCODE,
            }).ToList();
        }

        private IQueryable<TemplateInfo> GetRegisterTemplateIdsBIDC()
        {
            var reportCancellingDetailIds =
                from reportCancellingDetail in dbContext.REPORTCANCELLINGDETAILs
                select reportCancellingDetail.REGISTERTEMPLATESID;
            var reportCancellingDetailSymbols =
                from reportCancellingDetail in dbContext.REPORTCANCELLINGDETAILs
                select reportCancellingDetail.SYMBOL;

            var res = from registerTemplate in dbContext.REGISTERTEMPLATES
                      join notificationUserInvoiceDetail in dbContext.NOTIFICATIONUSEINVOICEDETAILs on registerTemplate.ID equals notificationUserInvoiceDetail.REGISTERTEMPLATESID
                      join notificationUseInvoice in dbContext.NOTIFICATIONUSEINVOICEs on notificationUserInvoiceDetail.NOTIFICATIONUSEINVOICEID equals notificationUseInvoice.ID
                      where !(reportCancellingDetailIds.Contains(registerTemplate.ID) && reportCancellingDetailSymbols.Contains(notificationUserInvoiceDetail.CODE))    // Không là ký hiệu bị hủy trong table REPORTCANCELLINGDETAIL
                        && !(registerTemplate.DELETED ?? false)
                        && !(notificationUseInvoice.DELETED ?? false)
                        && !(notificationUserInvoiceDetail.DELETED ?? false)
                        && notificationUseInvoice.STATUS == (int)RecordStatus.Approved
                      orderby notificationUserInvoiceDetail.USEDDATE descending
                      select new TemplateInfo()
                      {
                          CompanyId = registerTemplate.COMPANYID ?? 0,
                          RegisterTemplateId = registerTemplate.ID,
                          Code = registerTemplate.CODE,
                          Symbol = notificationUserInvoiceDetail.CODE,
                          UsedDate = notificationUserInvoiceDetail.USEDDATE,
                      };
            return res;
        }

        private List<TYPEPAYMENT> GetListTypePaymentBIDC()
        {
            return dbContext.TYPEPAYMENTs.AsNoTracking().ToList();
        }

        private List<CURRENCY> GetListCurrencyBIDC(List<InvoiceImportBIDC> invoicesRaw)
        {
            var listCurrencyCode = invoicesRaw.Select(x => x.CurrencyCode).Distinct().ToArray();
            return dbContext.CURRENCies.AsNoTracking().Where(e => listCurrencyCode.Contains(e.CODE)).ToList();
        }

        private List<TAX> GetListTaxBIDC(List<InvoiceImportBIDC> invoicesRaw)
        {
            var listTaxCode = invoicesRaw.Select(x => x.VatRate).Distinct().ToArray();
            return dbContext.TAXes.AsNoTracking().Where(e => listTaxCode.Contains(e.CODE)).ToList();
        }

        private bool WriteErrorLog(List<ImportIndexError> errors, string filePath, string FolderPath)
        {
            if (errors.Count == 0)
            {
                return false;
            }
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "import");
            string fileName = filePath.Substring(FolderPath.Length + 1);
            string logFile = Path.Combine(logDirectory, fileName + ".log");
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            var fileLog = File.Create(logFile);
            fileLog.Close();

            var listError = errors.Select(x => $"Row: {x.Index + 1} {Environment.NewLine}Field: {x.FieldError} {Environment.NewLine}" +
                $"Value field: {x.ValueFieldError} {Environment.NewLine}" +
                $"Value: {x.ValueError} {Environment.NewLine} {x.Exception.ToString()}").ToArray();
            listError[0] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + listError[0];

            File.WriteAllLines(logFile, listError);

            var updateImportLog = this.dbContext.IMPORT_LOG.OrderBy(e => e.ID).Where(e => e.FILENAME == fileName).FirstOrDefault();
            updateImportLog.DESCRIPTION = logFile;
            updateImportLog.DATE = DateTime.Now.ToString();
            this.Update(updateImportLog);

            return true;
        }

        private void CopyMultiFileFromSourceBIDC(string FolderPath, int MaxDayReadImport)
        {
            var toDate = DateTime.Now;
            var fromDate = toDate.AddDays(-MaxDayReadImport);
            var listDay = GetListDate(fromDate, toDate, "yyyyMMdd");
            int numberOfDay = listDay.Count;

            foreach (var midDayString in listDay)
            {
                var fileNameInvoice = Path.Combine("BIDC.INVOICE.DATA." + midDayString);
                var fileNameClient = Path.Combine("BIDC.CLIENT." + midDayString);

                var importLogCount = dbContext.IMPORT_LOG.Count(e => e.ISSUCCESS == true
                        && (e.FILENAME.ToUpper() == fileNameInvoice.ToUpper() || e.FILENAME.ToUpper() == fileNameClient.ToUpper()));

                if (importLogCount < 2)
                {
                    CopyFileFromSourceBIDC(FolderPath, midDayString);
                }
            }
        }

        private void CopyFileFromSourceBIDC(string FolderPath, string dayGetFile)
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            // start funtion: copy file from orther server to FolderImportFile
            var todate = DateTime.Now.ToString("yyyyMMdd"); // Get todate to 
                                                            // for client
            string SourceFileClient = GetConfig("SourceFileClient");
            string[] clientFileArray = new string[0];
            if (Directory.Exists(Path.Combine(SourceFileClient)))
            {
                clientFileArray = Directory.GetFiles(Path.Combine(SourceFileClient), "BIDC.CLIENT." + dayGetFile);
            }
            foreach (var p in clientFileArray)
            {
                // Remove path from the file name.
                string fNameClient = p.Substring(SourceFileClient.Length + 1);
                if (DeleteFileInputAfterImport)
                    File.Move(p, Path.Combine(FolderPath, fNameClient));
                else
                    File.Copy(p, Path.Combine(FolderPath, fNameClient), true);
            }
            // for invoice
            string SourceFileInvoice = GetConfig("SourceFileInvoice"); // example path: 
            string[] invoiceFileArray = new string[0];
            if (Directory.Exists(Path.Combine(SourceFileInvoice)))
            {
                invoiceFileArray = Directory.GetFiles(Path.Combine(SourceFileInvoice), "BIDC.INVOICE.DATA." + dayGetFile);
            }
            foreach (var p in invoiceFileArray)
            {
                // Remove path from the file name.
                string fNameInvoice = p.Substring(SourceFileInvoice.Length + 1);
                if (DeleteFileInputAfterImport)
                    File.Move(p, Path.Combine(FolderPath, fNameInvoice));
                else
                    File.Copy(p, Path.Combine(FolderPath, fNameInvoice), true);
            }
            //end funtion: copy file from orther server to FolderImportFile
        }

        private void InsertSystemLog(String ip, string detail, int status)
        {
            SYSTEMLOG systemLog = new SYSTEMLOG()
            {
                FUNCTIONCODE = "quartzJobManagement",
                LOGSUMMARY = "Import invoices",
                LOGTYPE = status,
                LOGDATE = DateTime.Now,
                LOGDETAIL = detail,
                IP = ip,
                USERNAME = "Quarzt",
            };
            this.Insert(dbContext.SYSTEMLOGS, systemLog);
        }

        private List<string> GetCollectionBIDC(string[] fileArray)
        {
            List<string> myCollection = new List<string>();
            foreach (var p in fileArray)
            {
                string stringDate = p.Substring(p.Length - 8);
                myCollection.Add(stringDate);
            }
            return myCollection.Distinct().ToList();
        }

        private void UpdateSuccessImportDBLog(string[] fileArrayDate, string FolderPath)
        {
            foreach (var filePath in fileArrayDate)
            {
                string fileName = filePath.Substring(FolderPath.Length + 1);
                IMPORT_LOG importLog = dbContext.IMPORT_LOG
                    .OrderByDescending(e => e.ID)
                    .Where(e => e.FILENAME == fileName)
                    .FirstOrDefault();
                importLog.DATE = DateTime.Now.ToString();
                importLog.ISSUCCESS = true;
                this.Update(importLog);
            }
        }
        private void MoveFileToStore(ImportLog p, string FolderPath)
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
                throw new BusinessLogicException(ResultCode.ImportClientFileError, "Import client file error");
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
                logger.Trace($"SourceFile: {sourceFile} {Environment.NewLine} DestinationFile: {destinationFile}");
                throw;
            }
        }

        public bool IsFileReady(string filename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void DeleteFileInputImportSuccessBIDC(ImportLog importLog)
        {
            if (!DeleteFileInputAfterImport)
                return;

            if (importLog.FileName.StartsWith("BIDC.CLIENT."))
            {
                string SourceFileClient = GetConfig("SourceFileClient");
                string ClientFullPath = Path.Combine(SourceFileClient, importLog.FileName);
                if (Directory.Exists(SourceFileClient) && File.Exists(ClientFullPath))
                {
                    File.Delete(ClientFullPath);
                }
            }

            if (importLog.FileName.StartsWith("BIDC.INVOICE.DATA."))
            {
                string SourceFileInvoice = GetConfig("SourceFileInvoice"); // example path: 
                string InvoiceFullPath = Path.Combine(SourceFileInvoice, importLog.FileName);
                if (Directory.Exists(SourceFileInvoice) && File.Exists(InvoiceFullPath))
                {
                    File.Delete(InvoiceFullPath);
                }
            }
        }

        public BIDCImportFromAPIOutput BIDCImportInvoiceExternalApi(XmlDocument xmlDocument)
        {
            // Thêm khách hàng vãng lai cho BIDC
            SeedClientBIDC();

            Func<string, string> textFromTag = (tagName) =>
                xmlDocument.GetElementsByTagName(tagName)[0].InnerText.Trim();

            INVOICE invoice = new INVOICE();
            BIDCAPIGetDataInvoice(textFromTag, invoice);
            invoice.CREATEDBY = CurrentUser.Id;

            if (IsInvoiceExistedBIDC(invoice))
            {
                throw new BusinessLogicException(ResultCode.ImportAPIBankRefExisted, "BankRef existed in e-invoice system");
            }

            var client = new CLIENT();
            BIDCAPIGetDataClient(textFromTag, client);
            bool clientIOrg = client.ISORG ?? false;
            if (OtherExtensions.IsCurrentClient(client.CUSTOMERCODE))
            {
                clientIOrg = !string.IsNullOrEmpty(client.TAXCODE);
            }
            if (OtherExtensions.IsCurrentClient(client.CUSTOMERCODE))
            {
                invoice.CUSTOMERBANKACC = client.BANKACCOUNT;
            }

            var clients = new List<CLIENT>() { client };
            var invoices = new List<INVOICE>() { invoice };
            var invoiceImportBIDCs = new List<InvoiceImportBIDC>()
            {
                new InvoiceImportBIDC()
                {
                    InvoiceNo = invoice.REFINVOICENO,
                    BankRef = invoice.REFNUMBER,
                    CustomerCode = client.CUSTOMERCODE,
                    TaxCode = client.TAXCODE,
                    CustomerName = !clientIOrg ? client.PERSONCONTACT : client.CUSTOMERNAME,
                    Address = client.ADDRESS,
                }
            };

            try
            {
                this.transaction.BeginTransaction();
                this.MergeClientBIDC(clients);
                this.InsertIvoiceBIDC(invoices, clients, invoiceImportBIDCs);
                CreateAccountingAccountBIDC(clients);
                this.transaction.Commit();
            }
            catch (Exception)
            {
                this.transaction.Rollback();
                throw;
            }

            try
            {
                // Send mail sau khi import thành công
                this.SendMailFromList(this.listSendMailBIDC);
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(ResultCode.ImportInvoiceFileSuccess, "Send mail import api error", ex);
            }

            var res = new BIDCImportFromAPIOutput(invoice)
            {
                CustomerCode = client.CUSTOMERCODE,
                CustomerName = clientIOrg == true ? client.CUSTOMERNAME : client.PERSONCONTACT,
                CustomerTaxCode = client.TAXCODE,
            };
            return res;
        }

        private void BIDCAPIGetDataInvoice(Func<string, string> textFromTag, INVOICE invoice)
        {
            var fieldError = "";
            try
            {
                fieldError = "INVOICE.COMPANYID";
                var branchId = textFromTag(BIDCApiTag.BRANCHID);
                var companyInfo = this.dbContext.MYCOMPANies.AsNoTracking().FirstOrDefault(e => e.BRANCHID == branchId);
                invoice.COMPANYID = companyInfo.COMPANYSID;
                fieldError = "INVOICE.RELEASEDDATE";
                invoice.RELEASEDDATE = DateTime.ParseExact(textFromTag(BIDCApiTag.INVOICEDATE), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                fieldError = "INVOICE.REGISTERTEMPLATEID";
                var registerTemplateInfo = this.GetRegisterTemplateIdsBIDC().FirstOrDefault(reg => reg.CompanyId == invoice.COMPANYID && reg.UsedDate.Value <= invoice.RELEASEDDATE);
                invoice.REGISTERTEMPLATEID = registerTemplateInfo.RegisterTemplateId;
                invoice.SYMBOL = registerTemplateInfo.Symbol;
                fieldError = "INVOICE.NUMBERACCOUT";
                invoice.CUSTOMERBANKACC = textFromTag(BIDCApiTag.CHARGEACCTNO);
                fieldError = "INVOICE.TYPEPAYMENT";
                var paymentMethod = textFromTag(BIDCApiTag.PAYMENTMETHOD);
                invoice.TYPEPAYMENT = this.dbContext.TYPEPAYMENTs.AsNoTracking().FirstOrDefault(t => t.CODE == paymentMethod)?.ID;
                invoice.INVOICESTATUS = 1;
                fieldError = "INVOICE.TOTALTAX";
                Decimal.TryParse(textFromTag(BIDCApiTag.TAXAMOUNT), out decimal taxAmount);
                invoice.TOTALTAX = taxAmount;
                fieldError = "INVOICE.TOTAL";
                Decimal.TryParse(textFromTag(BIDCApiTag.AMOUNT), out decimal amount);
                invoice.TOTAL = amount;
                fieldError = "INVOICE.SUM";
                Decimal.TryParse(textFromTag(BIDCApiTag.TOTALAMOUNT), out decimal totalAmount);
                invoice.SUM = totalAmount;
                invoice.CREATEDDATE = DateTime.Now;
                fieldError = "INVOICE.CURRENCYID";
                var currencyCode = textFromTag(BIDCApiTag.CURRENCYCODE);
                invoice.CURRENCYID = this.dbContext.CURRENCies.AsNoTracking().FirstOrDefault(currency => currency.CODE == currencyCode)?.ID;
                fieldError = "INVOICE.CURRENCYEXCHANGERATE";
                invoice.CURRENCYEXCHANGERATE = Decimal.Parse(textFromTag(BIDCApiTag.EXCHANGERATE));
                fieldError = "INVOICE.COMPANYNAME";
                invoice.COMPANYNAME = companyInfo.COMPANYNAME;
                fieldError = "INVOICE.COMPANYADDRESS";
                invoice.COMPANYADDRESS = companyInfo.ADDRESS;
                fieldError = "INVOICE.COMPANYTAXCODE";
                invoice.COMPANYTAXCODE = companyInfo.TAXCODE;
                fieldError = "INVOICE.REFNUMBER";
                invoice.REFNUMBER = textFromTag(BIDCApiTag.BANKREF);
                fieldError = "INVOICE.REFINVOICENO";
                invoice.REFINVOICENO = textFromTag(BIDCApiTag.INVOICENO);
                invoice.NO = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
                invoice.INVOICENO = 0;

                INVOICEDETAIL invoiceDetail = new INVOICEDETAIL();
                fieldError = "INVOICEDETAIL.PRICE";
                invoiceDetail.PRICE = amount;
                fieldError = "INVOICEDETAIL.TAXID";
                var taxRate = textFromTag(BIDCApiTag.VATRATE);
                var taxCode = "TS0";
                if (string.IsNullOrEmpty(taxRate))
                    taxCode = "TS0";
                else if (taxRate == "0")
                    taxCode = "T00";
                else if (taxRate == "5")
                    taxCode = "T05";
                else taxCode = "T10";
                invoiceDetail.TAXID = this.dbContext.TAXes.AsNoTracking().FirstOrDefault(e => e.CODE == taxCode).ID;
                fieldError = "INVOICEDETAIL.QUANTITY";
                invoiceDetail.QUANTITY = Decimal.Parse(textFromTag(BIDCApiTag.QUANTITY));
                invoiceDetail.TOTAL = amount;
                fieldError = "INVOICEDETAIL.AMOUNTTAX";
                invoiceDetail.AMOUNTTAX = taxAmount;
                fieldError = "INVOICEDETAIL.PRODUCTNAME";
                invoiceDetail.PRODUCTNAME = textFromTag(BIDCApiTag.DESCRIPTION);

                fieldError = "";
                invoice.INVOICEDETAILs.Add(invoiceDetail);
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(ResultCode.ImportParseDataInvoiceError, fieldError, ex);
            }
        }

        private void BIDCAPIGetDataClient(Func<string, string> textFromTag, CLIENT client)
        {
            string fieldError = "";
            try
            {
                fieldError = "IsOrg";
                client.ISORG = textFromTag(BIDCApiTag.ISORG) == "1";
                fieldError = "TaxCode";
                client.TAXCODE = textFromTag(BIDCApiTag.TAXCODE);
                var clientIOrg = client.ISORG;
                if (string.IsNullOrEmpty(textFromTag(BIDCApiTag.CUSTOMERCODE)))
                {
                    clientIOrg = !string.IsNullOrEmpty(textFromTag(BIDCApiTag.TAXCODE));
                }
                fieldError = "CustomerCode";
                client.CUSTOMERCODE = string.IsNullOrEmpty(textFromTag(BIDCApiTag.CUSTOMERCODE)) ? BIDCDefaultFields.CURRENT_CLIENT : textFromTag(BIDCApiTag.CUSTOMERCODE);
                fieldError = "CustomerName";
                if (clientIOrg == true)
                {
                    client.CUSTOMERNAME = textFromTag(BIDCApiTag.CUSTOMERNAME);
                }
                fieldError = "Address";
                client.ADDRESS = textFromTag(BIDCApiTag.ADDRESS);
                fieldError = "SendInvoiceByMonth";
                client.SENDINVOICEBYMONTH = textFromTag(BIDCApiTag.SENDINVOICEBYMONTH) == "1";
                fieldError = "DateSendInvoice";
                var parse = int.TryParse(textFromTag(BIDCApiTag.DATESENDINVOICE), out int dateSendInvoice);
                if (parse == true)
                {
                    client.DATESENDINVOICE = dateSendInvoice;
                }
                fieldError = "Mobile";
                client.MOBILE = textFromTag(BIDCApiTag.MOBILE);
                fieldError = "Email";
                client.EMAIL = textFromTag(BIDCApiTag.EMAIL);
                client.RECEIVEDINVOICEEMAIL = client.EMAIL;
                fieldError = "Delegate";
                client.DELEGATE = textFromTag(BIDCApiTag.DELEGATE);
                fieldError = "PersonalContact";
                if (clientIOrg == true)
                {
                    client.PERSONCONTACT = textFromTag(BIDCApiTag.PERSONALCONTACT);
                }
                else
                {
                    client.PERSONCONTACT = textFromTag(BIDCApiTag.CUSTOMERNAME);
                }
                fieldError = "BankAccount";
                client.BANKACCOUNT = textFromTag(BIDCApiTag.BANKACCOUNT);
                fieldError = "AccountHolder";
                client.ACCOUNTHOLDER = textFromTag(BIDCApiTag.ACCOUNTHOLDER);
                fieldError = "BankName";
                client.BANKNAME = textFromTag(BIDCApiTag.BANKNAME);
                fieldError = "IsCreateAccount";
                int isCreateAccount = int.Parse(textFromTag(BIDCApiTag.ISCREATEACCOUNT));
                client.CUSTOMERTYPE = isCreateAccount == 1 ? (int)CustomerType.IsAccounting : (int)CustomerType.NoneAccounting; // CUSTOMERTYPE: IsAccounting, NoneAccounting
                fieldError = "";
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(ResultCode.ImportParseDataClientError, fieldError, ex);
            }
        }

        private bool IsInvoiceExistedBIDC(INVOICE invoice)
        {
            var startDay = invoice.RELEASEDDATE.Value.Date;
            var endDay = invoice.RELEASEDDATE.Value.Date.AddDays(1).AddTicks(-1);
            var count = this.dbContext.INVOICEs.Count(e => !(e.DELETED ?? false)
                            && e.RELEASEDDATE >= startDay && e.RELEASEDDATE <= endDay
                            && e.COMPANYID == invoice.COMPANYID
                            && e.REFINVOICENO == invoice.REFINVOICENO);
            if (count > 0)
                return true;
            return false;
        }

        private void RemoveInvoiceEixtedBIDC(List<InvoiceImportBIDC> invoicesRaw, List<ImportGetInfo> listCompany)
        {
            // Xóa những hóa đơn đã import bằng API nếu có
            var listInvoiceSearchInvoiceDate = invoicesRaw.Select(x => new
            {
                StartDay = x.InvoiceDate.Date,
                EndDay = x.InvoiceDate.Date.AddDays(1).AddTicks(-1),
            }).Distinct().ToList();

            Expression<Func<INVOICE, bool>> expressionInvoiceDate = null;
            foreach (var invoiceSearchDate in listInvoiceSearchInvoiceDate)
            {
                Expression<Func<INVOICE, bool>> predicate = (e) => !(e.DELETED ?? false) && e.RELEASEDDATE >= invoiceSearchDate.StartDay && e.RELEASEDDATE <= invoiceSearchDate.EndDay;

                if (expressionInvoiceDate == null)
                    expressionInvoiceDate = predicate;
                else
                    expressionInvoiceDate = expressionInvoiceDate.Or(predicate);
            }

            var listInvoiceSearch = invoicesRaw.Select(x => new
            {
                CompanyId = listCompany.FirstOrDefault(com => com.BranchId == x.BranchId).CompanySID,
                InvoiceNo = x.InvoiceNo,
            }).Distinct().ToList();

            Expression<Func<INVOICE, bool>> expression = null;
            foreach (var invoiceSearch in listInvoiceSearch)
            {
                Expression<Func<INVOICE, bool>> predicate = (e) => e.COMPANYID == invoiceSearch.CompanyId
                               && e.REFINVOICENO.Trim() == invoiceSearch.InvoiceNo.Trim();

                if (expression == null)
                    expression = predicate;
                else
                    expression = expression.Or(predicate);
            }

            // Những hóa đơn trùng ngày ReleaseDate và trùng RefInvoiceNo, CompanyId
            var listInvoiceNoExistedQuery = this.dbContext.INVOICEs.Where(expressionInvoiceDate).Where(expression);
            var listInvoiceNoExisted = listInvoiceNoExistedQuery.Select(e => e.REFINVOICENO.Trim()).ToList();
            foreach (var invoiceNo in listInvoiceNoExisted)
            {
                invoicesRaw.RemoveAll(x => x.InvoiceNo.Trim() == invoiceNo);
            }
        }

        private void SeedClientBIDC()
        {
            // Thêm khách hàng vãng lai cho BIDC
            var isCurrentClientExists = this.dbContext.CLIENTs.Count(e => e.CUSTOMERCODE == BIDCDefaultFields.CURRENT_CLIENT);
            if (isCurrentClientExists == 0)
            {
                this.dbContext.CLIENTs.Add(new CLIENT() { CUSTOMERCODE = BIDCDefaultFields.CURRENT_CLIENT, CUSTOMERNAME = BIDCDefaultFields.CURRENT_CLIENT, ISORG = false, });
                this.dbContext.SaveChanges();
            }
        }
    }
}