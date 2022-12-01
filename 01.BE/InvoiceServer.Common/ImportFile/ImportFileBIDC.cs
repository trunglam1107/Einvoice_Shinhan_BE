using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using System.IO;
using System.Web.Configuration;
using InvoiceServer.Common.Constants;
using Oracle.ManagedDataAccess.Client;
using InvoiceServer.Common.Models;
using System.Globalization;
using System.Runtime.InteropServices;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System.Reflection;
using NLog;
using System.Dynamic;

namespace InvoiceServer.Common
{
    public partial class ImportFile
    {
        private static object lockAddClient = new object();
        private static object lockAddException = new object();
        private static object lockAddInvoice = new object();
        // đây là file cũ
        // Chuyển sang import trong project Bussiness để dùng chung model, transaction, entity framework, gửi mail, ...
        public static void ImportFileAuto_BIDC(bool isJob)
        {
            string FolderPath = GetConfig("ImportFile");
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
            string sqlResult = "SELECT ISSUCCESS as IsSuccessOracle, FILEPATH FROM IMPORT_LOG where ROWNUM <=2 ORDER BY ID DESC";
            // get ip
            String strHostName = System.Net.Dns.GetHostName();
            System.Net.IPAddress[] ipaddress = System.Net.Dns.GetHostAddresses(strHostName);
            String ip = ipaddress.FirstOrDefault(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
            // end get ip
            string sqlInsert = "INSERT INTO SYSTEMLOGS (FUNCTIONCODE, LOGSUMMARY, LOGTYPE, LOGDATE, LOGDETAIL, IP, USERNAME) " +
                    "Values (:FUNCTIONCODE, :LOGSUMMARY, :LOGTYPE, :LOGDATE, :LOGDETAIL, :IP, :USERNAME)";

            //
            if (isJob)
            {
                // Clear all file in working directory
                ClearAllFileInFolder(FolderPath);
                // copy file from orther server to FolderImportFile
                Instance.CopyFileFromSourceBIDC(FolderPath);
            }

            // Get all files in folder FolderPath
            string[] fileArray = Directory.GetFiles(Path.Combine(FolderPath));
            if (fileArray.Length == 0)
            {
                Instance.insertSystemlogOracle(ip, sqlInsert, FolderPath, connectionString);
                throw new BusinessLogicException(ResultCode.ImportInvoiceFileError, "No files in import working folder");
            }

            // Get list date invoice
            List<string> myCollection = Instance.GetCollectionBIDC(fileArray);
            foreach (var c in myCollection)
            {
                string[] fileArrayDate = Directory.GetFiles(Path.Combine(FolderPath), "*." + c);
                using (var connection = new OracleConnection(connectionString))
                {
                    Instance.InitImportDBLog(fileArrayDate, FolderPath, connection);
                    try
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                Instance.ImportFileForDay(fileArrayDate, FolderPath, connection);
                                transaction.Commit();
                            }
                            catch (Exception)
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                        connection.Close();

                        connection.Open();
                        var result = connection.Query<ImportLog>(sqlResult).ToList();
                        foreach (var item in result)
                        {
                            item.IsSuccess = item.IsSuccessOracle == 1 ? true : false;
                        }
                        connection.Close();
                        foreach (var p in result)
                        {
                            BusinessLogicException businessLogicException = null;
                            try
                            {
                                MoveFileToStore(p, FolderPath);
                            }
                            catch (BusinessLogicException ex)
                            {
                                businessLogicException = ex;
                            }
                            if (businessLogicException != null && result.IndexOf(p) == result.Count - 1)
                            {
                                throw businessLogicException;
                            }
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

        private void ImportFileForDay(string[] fileArrayDate, string FolderPath, OracleConnection connection)
        {
            // connection opened
            List<ImportIndexError> clientRawErrors = new List<ImportIndexError>();
            List<ImportIndexError> invoiceRawErrors = new List<ImportIndexError>();
            var clientsRaw = new List<ClientImportBIDC>();
            var invoicesRaw = new List<InvoiceImportBIDC>();
            string clientPath = "", invoicePath = "";

            bool isParseError = false;
            foreach (var p in fileArrayDate)
            {
                if (p.Substring(FolderPath.Length + 1).Contains("CLIENT"))
                {
                    clientsRaw = Instance.GetDataClient(p, clientRawErrors);
                    clientPath = p;
                    if (WriteErrorLog(clientRawErrors, p, FolderPath, connection))
                    {
                        isParseError = true;
                    }
                }
                else
                {
                    invoicesRaw = Instance.GetDataInvoice(p, invoiceRawErrors);
                    invoicePath = p;
                    if (WriteErrorLog(invoiceRawErrors, p, FolderPath, connection))
                    {
                        isParseError = true;
                    }
                }
            }
            if (isParseError)
            {
                return;
            }

            var clients = MergeClient(clientsRaw, connection);
            InsertIvoice(invoicesRaw, clients, connection);
            
            UpdateSuccessImportDBLog(fileArrayDate, FolderPath, connection);
        }

        private void InsertIvoice(List<InvoiceImportBIDC> invoicesRaw, List<CLIENT> clients, OracleConnection connection)
        {
            var listCompany = GetBranchIdFromDB(invoicesRaw, connection);
            var registerTemplateInfo = GetRegisterTemplateId(connection);
            var listTypePayment = GetListTypePayment(connection);
            var listCurrencty = GetListCurrency(invoicesRaw, connection);

            var invoices = invoicesRaw.Select(ir => new INVOICE
            {
                CLIENTID = clients.FirstOrDefault(c => c.CUSTOMERCODE == ir.CustomerCode)?.ID,
                COMPANYID = listCompany.FirstOrDefault(com => com.BranchId == ir.BranchId).CompanySID,
                REGISTERTEMPLATEID = registerTemplateInfo.RegisterTemplateId,
                SYMBOL = registerTemplateInfo.Symbol,
                NUMBERACCOUT = ir.BankAccount,
                TYPEPAYMENT = listTypePayment.FirstOrDefault(t => t.CODE == ir.PaymentMethod)?.ID,
                INVOICESTATUS = 1,
                TOTALTAX = invoicesRaw.Where(ir2 => ir2.InvoiceNo == ir.InvoiceNo).Sum(ir2 => ir2.TaxAmount),
                TOTAL = invoicesRaw.Where(ir2 => ir2.InvoiceNo == ir.InvoiceNo).Sum(ir2 => ir2.Amount),
                SUM = invoicesRaw.Where(ir2 => ir2.InvoiceNo == ir.InvoiceNo).Sum(ir2 => ir2.TotalAmount),
                RELEASEDDATE = ir.InvoiceDate,
                CUSTOMERNAME = clients.FirstOrDefault(c => c.CUSTOMERCODE == ir.CustomerCode)?.CUSTOMERNAME,
                CURRENCYID = listCurrencty.FirstOrDefault(currency => currency.CODE == ir.CurrencyCode)?.ID,
                CURRENCYEXCHANGERATE = listCurrencty.FirstOrDefault(currency => currency.CODE == ir.CurrencyCode)?.EXCHANGERATE,
                PERSONCONTACT = clients.FirstOrDefault(c => c.CUSTOMERCODE == ir.CustomerCode)?.PERSONCONTACT,
                CUSTOMERTAXCODE = clients.FirstOrDefault(c => c.CUSTOMERCODE == ir.CustomerCode)?.TAXCODE,
                CUSTOMERADDRESS = clients.FirstOrDefault(c => c.CUSTOMERCODE == ir.CustomerCode)?.ADDRESS,
                CUSTOMERBANKACC = clients.FirstOrDefault(c => c.CUSTOMERCODE == ir.CustomerCode)?.BANKACCOUNT,
                COMPANYNAME = listCompany.FirstOrDefault(com => com.BranchId == ir.BranchId).CompanyName,
                COMPANYADDRESS = listCompany.FirstOrDefault(com => com.BranchId == ir.BranchId).CompanyAddress,
                COMPANYTAXCODE = listCompany.FirstOrDefault(com => com.BranchId == ir.BranchId).CompanyTaxCode,
                REFNUMBER = ir.BankRef,
                REFINVOICENO = ir.InvoiceNo,
            }).Distinct().ToList();

            string queryInsertInvoice = $"INSERT INTO INVOICE(COMPANYID,CLIENTID,REGISTERTEMPLATEID,SYMBOL,NUMBERACCOUT," +
                $"TYPEPAYMENT,TOTALTAX,TOTAL,SUM," +
                $"CUSTOMERNAME,CURRENCYID,CURRENCYEXCHANGERATE,PERSONCONTACT,CUSTOMERTAXCODE," +
                $"CUSTOMERADDRESS,CUSTOMERBANKACC,COMPANYNAME,COMPANYADDRESS,COMPANYTAXCODE," +
                $"REFNUMBER,REFINVOICENO) " +
                $" VALUES(:COMPANYID,:CLIENTID,:REGISTERTEMPLATEID,:SYMBOL,:NUMBERACCOUT," +
                $":TYPEPAYMENT,:TOTALTAX,:TOTAL,:SUM," +
                $":CUSTOMERNAME,:CURRENCYID,:CURRENCYEXCHANGERATE,:PERSONCONTACT,:CUSTOMERTAXCODE," +
                $":CUSTOMERADDRESS,:CUSTOMERBANKACC,:COMPANYNAME,:COMPANYADDRESS,:COMPANYTAXCODE," +
                $":REFNUMBER,:REFINVOICENO)";

            var rowsInsertInvoice = connection.Execute(queryInsertInvoice, invoices);
            var listInvoiceExistsInsertUpdate = GetListInvoiceExists(invoicesRaw, connection);

            var listTax = GetListTax(invoicesRaw, connection);
            foreach (var item in invoicesRaw)
            {
                item.Id = listInvoiceExistsInsertUpdate.FirstOrDefault(x => x.REFINVOICENO == item.InvoiceNo).ID;
                item.TaxId = listTax.FirstOrDefault(x => x.CODE == item.VatRate).ID;
            }

            var invoicedetails = invoicesRaw.Select(x => new { x.Id, x.Amount, x.TaxId, x.TaxAmount, x.Description }).ToList();
            string queryInsertInvoiceDetail = $"INSERT INTO INVOICEDETAIL(INVOICEID,PRICE,TAXID,TOTAL,AMOUNTTAX,PRODUCTNAME) " +
                $" VALUES(:Id,:Amount,:TaxId,:Amount,:TaxAmount,:Description)";
            var rowsInsertInvoiceDetail = connection.Execute(queryInsertInvoiceDetail, invoicesRaw);
        }

        private List<CLIENT> MergeClient(List<ClientImportBIDC> clientsRaw, OracleConnection connection)
        {
            // connection opened
            var listClientCodeExists = GetListClientCodeExists(clientsRaw, connection);
            var listInsert = new List<ClientImportBIDC>();
            var listUpdate = new List<ClientImportBIDC>();
            foreach (var item in clientsRaw)
            {
                if (listClientCodeExists.Contains(item.CustomerCode))
                {
                    listUpdate.Add(item);
                }
                else
                {
                    listInsert.Add(item);
                }
            }

            string queryUpdate = $"UPDATE CLIENT SET " +
                $"ADDRESS=:Address, " +
                $"CUSTOMERNAME=:CustomerName, " +
                $"CUSTOMERTYPE=:IsCreateAccount, " +
                $"DELEGATE=:Delegate, " +
                $"DATESENDINVOICE=1, " +
                $"EMAIL=:Email, " +
                $"ISORG=:IsOrg, " +
                $"MOBILE=:Mobile, " +
                $"PERSONCONTACT=:PersonalContact, " +
                $"SENDINVOICEBYMONTH=:SendInvoiceByMonth, " +
                $"TAXCODE=:TaxCode " +
                $" WHERE CUSTOMERCODE = :CustomerCode";
            string queryInsert = $"INSERT INTO CLIENT(ADDRESS,CUSTOMERCODE,CUSTOMERNAME,CUSTOMERTYPE,DELEGATE,DATESENDINVOICE,EMAIL,ISORG,MOBILE,PERSONCONTACT,SENDINVOICEBYMONTH,TAXCODE) " +
                $" VALUES(:Address,:CustomerCode,:CustomerName,:IsCreateAccount,:Delegate,1,:Email,:IsOrg,:Mobile,:PersonalContact,:SendInvoiceByMonth,:TaxCode)";

            var rowsUpdate = connection.Execute(queryUpdate, listUpdate);
            var rowsInsert = connection.Execute(queryInsert, listInsert);

            var listClientExistsInsertUpdate = GetListClientExists(clientsRaw, connection);
            return listClientExistsInsertUpdate;
        }

        private List<string> GetListClientCodeExists(List<ClientImportBIDC> clientsRaw, OracleConnection connection)
        {
            // connection opened
            var listClientCode = clientsRaw.Select(x => x.CustomerCode).Distinct();
            List<dynamic> results = new List<dynamic>();

            string sqlGetBranchs = "SELECT CUSTOMERCODE " +
                " FROM CLIENT WHERE CUSTOMERCODE IN :CODES";
            int step = 999;
            for (int i = 0; i < listClientCode.Count(); i += step)
            {
                int end = i + step < listClientCode.Count() ? i + step : listClientCode.Count();
                var subList = listClientCode.Skip(i).Take(end - i);
                results.AddRange(connection.Query(sqlGetBranchs, new { CODES = subList }));
            }

            return results.Select(x => x.CUSTOMERCODE as string).ToList();
        }

        private List<CLIENT> GetListClientExists(List<ClientImportBIDC> clientsRaw, OracleConnection connection)
        {
            // connection opened
            var listClientCode = clientsRaw.Select(x => x.CustomerCode).Distinct();
            List<dynamic> results = new List<dynamic>();

            string sqlGetBranchs = "SELECT * " +
                " FROM CLIENT WHERE CUSTOMERCODE IN :CODES";
            int step = 999;
            for (int i = 0; i < listClientCode.Count(); i += step)
            {
                int end = i + step < listClientCode.Count() ? i + step : listClientCode.Count();
                var subList = listClientCode.Skip(i).Take(end - i);
                results.AddRange(connection.Query(sqlGetBranchs, new { CODES = subList }));
            }

            return results.Select(x =>
            {
                ConvertObjectToPrimaryType(x, out CLIENT result);
                return result;
            }).ToList();
        }

        private List<INVOICE> GetListInvoiceExists(List<InvoiceImportBIDC> invoicesRaw, OracleConnection connection)
        {
            // connection opened
            var listInvoiceNo = invoicesRaw.Select(x => x.InvoiceNo).Distinct();
            List<dynamic> results = new List<dynamic>();

            string sqlGetBranchs = "SELECT * " +
                " FROM INVOICE WHERE REFINVOICENO IN :REFINVOICENOS";
            int step = 999;
            for (int i = 0; i < listInvoiceNo.Count(); i += step)
            {
                int end = i + step < listInvoiceNo.Count() ? i + step : listInvoiceNo.Count();
                var subList = listInvoiceNo.Skip(i).Take(end - i);
                results.AddRange(connection.Query(sqlGetBranchs, new { REFINVOICENOS = subList }));
            }

            return results.Select(x =>
            {
                ConvertObjectToPrimaryType(x, out INVOICE result);
                return result;
            }).ToList();
        }

        private T ConvertObjectToPrimaryType<T>(dynamic obj, out T result)
            where T: new()
        {
            result = new T();
            var objValues = ((IDictionary<String, Object>)obj);
            var type = result.GetType();
            var properties = type.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.PropertyType.IsGenericType &&
                        propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var propertyType = propertyInfo.PropertyType.GetGenericArguments()[0];
                    if (!propertyType.IsPrimitive && propertyType != typeof(string))
                        continue;
                    string propName = propertyInfo.Name.ToUpper();
                    string typeName = type.Name;
                    if (typeName + "1" == propName)
                        propName = typeName;
                    try
                    {
                        if (objValues[propName] == null)
                            continue;
                        var value = objValues[propName];
                        var convertedValue2 = Convert.ChangeType(value, propertyType);
                        propertyInfo.SetValue(result, convertedValue2);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                else
                {
                    if (!propertyInfo.PropertyType.IsPrimitive && propertyInfo.PropertyType != typeof(string))
                        continue;
                    string propName = propertyInfo.Name.ToUpper();
                    string typeName = type.Name;
                    if (typeName + "1" == propName)
                        propName = typeName;
                    try
                    {
                        var value = objValues[propName];
                        var convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
                        propertyInfo.SetValue(result, convertedValue);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            return result;
        }

        private List<ImportGetInfo> GetBranchIdFromDB(List<InvoiceImportBIDC> invoicesRaw, OracleConnection connection)
        {
            // connection opened
            var listBranchId = invoicesRaw.Select(x => x.BranchId).Distinct();

            string sqlGetBranchs = "SELECT COMPANYSID AS COMPANYSID, BRANCHID AS BRANCHID, COMPANYNAME AS COMPANYNAME, ADDRESS AS COMPANYADDRESS, TAXCODE AS COMPANYTAXCODE " +
                " FROM MYCOMPANY WHERE BRANCHID IN :IDS";
            List<dynamic> results = new List<dynamic>();
            int step = 999;
            for (int i = 0; i < listBranchId.Count(); i += step)
            {
                int end = i + step < listBranchId.Count() ? i + step : listBranchId.Count();
                var subList = listBranchId.Skip(i).Take(end - i);
                results.AddRange(connection.Query(sqlGetBranchs, new { IDS = subList }));
            }
            return results.Select(x =>
            {
                ConvertObjectToPrimaryType(x, out ImportGetInfo result);
                return result;
            }).ToList();
        }

        private TemplateInfo GetRegisterTemplateId(OracleConnection connection)
        {
            // connection opened
            string sqlGetRegisterTemplate = "SELECT R.ID AS REGISTERTEMPLATEID, R.CODE AS CODE, ND.CODE AS SYMBOL FROM REGISTERTEMPLATES R " +
                " JOIN NOTIFICATIONUSEINVOICEDETAIL ND ON R.ID = ND.REGISTERTEMPLATESID " +
                " WHERE ROWNUM = 1 ORDER BY R.ID DESC";
            var results = connection.Query(sqlGetRegisterTemplate);
            return results.Select(x =>
            {
                ConvertObjectToPrimaryType(x, out TemplateInfo result);
                return result;
            }).FirstOrDefault();
        }

        private List<TYPEPAYMENT> GetListTypePayment(OracleConnection connection)
        {
            // connection opened
            string sqlTypePayment = "SELECT * FROM TYPEPAYMENT";
            var results = connection.Query(sqlTypePayment);
            return results.Select(x =>
            {
                ConvertObjectToPrimaryType(x, out TYPEPAYMENT result);
                return result;
            }).ToList();
        }

        private List<CURRENCY> GetListCurrency(List<InvoiceImportBIDC> invoicesRaw, OracleConnection connection)
        {
            // connection opened
            var listCurrencyCode = invoicesRaw.Select(x => x.CurrencyCode).Distinct().ToArray();
            string sqlCurrency = "SELECT * FROM CURRENCY WHERE CODE IN :CODES";
            List<dynamic> results = new List<dynamic>();
            int step = 999;
            for (int i = 0; i < listCurrencyCode.Count(); i += step)
            {
                int end = i + step < listCurrencyCode.Count() ? i + step : listCurrencyCode.Count();
                var subList = listCurrencyCode.Skip(i).Take(end - i);
                results.AddRange(connection.Query(sqlCurrency, new { CODES = subList }));
            }
            return results.Select(x =>
            {
                ConvertObjectToPrimaryType(x, out CURRENCY result);
                return result;
            }).ToList();
        }

        private List<TAX> GetListTax(List<InvoiceImportBIDC> invoicesRaw, OracleConnection connection)
        {
            // connection opened
            var listTaxCode = invoicesRaw.Select(x => x.VatRate).Distinct().ToArray();
            string sqlTax = "SELECT * FROM TAX WHERE CODE IN :CODES";
            List<dynamic> results = new List<dynamic>();
            int step = 999;
            for (int i = 0; i < listTaxCode.Count(); i += step)
            {
                int end = i + step < listTaxCode.Count() ? i + step : listTaxCode.Count();
                var subList = listTaxCode.Skip(i).Take(end - i);
                results.AddRange(connection.Query(sqlTax, new { CODES = subList }));
            }
            return results.Select(x =>
            {
                ConvertObjectToPrimaryType(x, out TAX result);
                return result;
            }).ToList();
        }

        private bool WriteErrorLog(List<ImportIndexError> errors, string filePath, string FolderPath, OracleConnection connection)
        {
            // connection opened
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
                $"Value: {x.ValueError} {Environment.NewLine} {x.Exception.ToString()}").ToArray();
            listError[0] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + listError[0];

            File.WriteAllLines(logFile, listError);

            string queryUpdate = "UPDATE IMPORT_LOG SET DESCRIPTION = :DESCRIPTION, \"DATE\"=:LOGDATE" +
                " WHERE FILENAME=:FILENAME AND ROWID IN(" +
                " SELECT RID FROM(SELECT ROWID as RID FROM IMPORT_LOG ORDER BY ID DESC) WHERE ROWNUM <= 2 )";
            var affectedRows = connection.Execute(queryUpdate, new
            {
                FILENAME = filePath.Substring(FolderPath.Length + 1),
                DESCRIPTION = logFile,
                LOGDATE = DateTime.Now.ToString(),
            });

            return true;
        }

        private void InitImportDBLog(string[] fileArrayDate, string FolderPath, OracleConnection connection)
        {
            foreach (var filePath in fileArrayDate)
            {
                string queryInsert = "INSERT INTO IMPORT_LOG(FILEPATH, FILENAME, \"DATE\", ISSUCCESS) VALUES (:FILEPATH, :FILENAME, :LOGDATE, :ISSUCCESS)";
                connection.Open();
                var affectedRows = connection.Execute(queryInsert, new
                {
                    FILEPATH = filePath,
                    FILENAME = filePath.Substring(FolderPath.Length + 1),
                    LOGDATE = DateTime.Now.ToString(),
                    ISSUCCESS = 0,
                });
                connection.Close();
            }
        }

        private void UpdateSuccessImportDBLog(string[] fileArrayDate, string FolderPath, OracleConnection connection)
        {
            // connection opened
            foreach (var filePath in fileArrayDate)
            {
                string queryInsert = "UPDATE IMPORT_LOG SET \"DATE\"=:LOGDATE, ISSUCCESS=:ISSUCCESS " +
                    " WHERE FILENAME=:FILENAME AND ROWID IN(" +
                    " SELECT RID FROM(SELECT ROWID as RID FROM IMPORT_LOG ORDER BY ID DESC) WHERE ROWNUM <= 2 )";
                var affectedRows = connection.Execute(queryInsert, new
                {
                    FILENAME = filePath.Substring(FolderPath.Length + 1),
                    LOGDATE = DateTime.Now.ToString(),
                    ISSUCCESS = 1,
                });
            }
        }

        private List<ClientImportBIDC> GetDataClient(string pathClient, List<ImportIndexError> errors)
        {
            List<ClientImportBIDC> res = new List<ClientImportBIDC>();
            var rawClients = File.ReadAllLines(pathClient);
            if (rawClients.Length <= 0)
            {
                return res;
            }

            List<dynamic> resTemp = new List<dynamic>();
            Parallel.For(0, rawClients.Length, index =>
            {
                string fieldError = "Split step";
                try
                {
                    var clientParam = rawClients[index].Split('#').Select(x => x.Trim()).ToArray();
                    if (clientParam == null || clientParam.Length == 0)
                    {
                        return;
                    }

                    ClientImportBIDC clientImportBIDC = new ClientImportBIDC();
                    fieldError = "IsOrg";
                    clientImportBIDC.IsOrg = clientParam[0] == "1" ? 1 : 0;
                    fieldError = "TaxCode";
                    clientImportBIDC.TaxCode = clientParam[1];
                    fieldError = "CustomerCode";
                    clientImportBIDC.CustomerCode = clientParam[2];
                    fieldError = "CustomerName";
                    clientImportBIDC.CustomerName = clientParam[3];
                    fieldError = "Address";
                    clientImportBIDC.Address = clientParam[4];

                    fieldError = "SendInvoiceByMonth";
                    clientImportBIDC.SendInvoiceByMonth = clientParam[5] == "1" ? 1 : 0;
                    fieldError = "DateSendInvoice";
                    DateTime.TryParseExact(clientParam[6], "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dateSendInvoice);
                    if (dateSendInvoice != DateTime.MinValue)
                    {
                        clientImportBIDC.DateSendInvoice = dateSendInvoice;
                    }
                    fieldError = "Mobile";
                    clientImportBIDC.Mobile = clientParam[7];
                    fieldError = "Email";
                    clientImportBIDC.Email = clientParam[8];
                    fieldError = "Delegate";
                    clientImportBIDC.Delegate = clientParam[9];

                    fieldError = "PersonalContact";
                    clientImportBIDC.PersonalContact = clientParam[10];
                    fieldError = "IsCreateAccount";
                    int isCreateAccount = int.Parse(clientParam[11]);
                    clientImportBIDC.IsCreateAccount = isCreateAccount == 1 ? (int)CustomerType.IsAccounting : (int)CustomerType.NoneAccounting; // CUSTOMERTYPE: IsAccounting, NoneAccounting

                    lock (lockAddClient)
                    {
                        resTemp.Add(new { index, clientImportBIDC });
                    }
                }
                catch (Exception ex)
                {
                    lock (lockAddException)
                    {
                        errors.Add(new ImportIndexError(index, ex, fieldError, "", rawClients[index]));
                    }
                }
            });
            res = resTemp.OrderBy(x => x.index).Select(x => x.clientImportBIDC as ClientImportBIDC).ToList();
            errors.Sort((x, y) => x.Index.CompareTo(y.Index));
            return res;
        }

        private List<InvoiceImportBIDC> GetDataInvoice(string pathInvoice, List<ImportIndexError> errors)
        {
            List<InvoiceImportBIDC> res = new List<InvoiceImportBIDC>();
            var rawInvoices = File.ReadAllLines(pathInvoice);
            if (rawInvoices.Length <= 0)
            {
                return res;
            }

            List<dynamic> resTemp = new List<dynamic>();
            Parallel.For(0, rawInvoices.Length, index =>
            {
                string fieldError = "Split step";
                try
                {
                    var invoiceParam = rawInvoices[index].Split('#').Select(x => x.Trim()).ToArray();
                    if (invoiceParam == null || invoiceParam.Length == 0)
                    {
                        return;
                    }

                    InvoiceImportBIDC invoiceImportBIDC = new InvoiceImportBIDC();
                    fieldError = "BranchId";
                    invoiceImportBIDC.BranchId = invoiceParam[0];
                    fieldError = "InvoiceNo";
                    invoiceImportBIDC.InvoiceNo = invoiceParam[1];
                    fieldError = "InvoiceDate";
                    DateTime invoiceDate = DateTime.ParseExact(invoiceParam[2], "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                    invoiceImportBIDC.InvoiceDate = invoiceDate;
                    fieldError = "CustomerCode";
                    invoiceImportBIDC.CustomerCode = invoiceParam[3];
                    fieldError = "PaymentMethod";
                    invoiceImportBIDC.PaymentMethod = invoiceParam[4];

                    fieldError = "CurrencyCode";
                    invoiceImportBIDC.CurrencyCode = invoiceParam[5];
                    fieldError = "ExchangeRate";
                    decimal exchangeRate = Decimal.Parse(invoiceParam[6]);
                    invoiceImportBIDC.ExchangeRate = exchangeRate;
                    fieldError = "Description";
                    invoiceImportBIDC.Description = invoiceParam[7];
                    fieldError = "Amount";
                    decimal amount = Decimal.Parse(invoiceParam[8]);
                    invoiceImportBIDC.Amount = amount;
                    fieldError = "VatRate";
                    var vatRate = invoiceParam[9];
                    if (string.IsNullOrEmpty(vatRate))
                        invoiceImportBIDC.VatRate = "TS0";
                    else if (vatRate == "0")
                        invoiceImportBIDC.VatRate = "T00";
                    else if (vatRate == "5")
                        invoiceImportBIDC.VatRate = "T05";
                    else invoiceImportBIDC.VatRate = "T10";

                    fieldError = "TaxAmount";
                    Decimal.TryParse(invoiceParam[10], out decimal taxAmount);
                    invoiceImportBIDC.TaxAmount = taxAmount;
                    fieldError = "TotalAmount";
                    decimal totalAmount = Decimal.Parse(invoiceParam[11]);
                    invoiceImportBIDC.TotalAmount = totalAmount;
                    fieldError = "BankRef";
                    invoiceImportBIDC.BankRef = invoiceParam[12];
                    fieldError = "BankAccount";
                    invoiceImportBIDC.BankAccount = invoiceParam[13];

                    lock (lockAddInvoice)
                    {
                        resTemp.Add(new { index, invoiceImportBIDC });
                    }
                }
                catch (Exception ex)
                {
                    lock (lockAddException)
                    {
                        errors.Add(new ImportIndexError(index, ex, fieldError, "", rawInvoices[index]));
                    }
                }
            });
            res = resTemp.OrderBy(x => x.index).Select(x => x.invoiceImportBIDC as InvoiceImportBIDC).ToList();
            errors.Sort((x, y) => x.Index.CompareTo(y.Index));
            return res;
        }

        private void CopyFileFromSourceBIDC(string FolderPath)
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            // start funtion: copy file from orther server to FolderImportFile
            var yesterdate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd"); // Get yesterdate to 
            var todate = DateTime.Now.ToString("yyyyMMdd"); // Get todate to 
                                                            // for client
            string SourceFileClient = GetConfig("SourceFileClient");
            string[] clientFileArray = new string[0];
            if (Directory.Exists(Path.Combine(SourceFileClient)))
            {
                clientFileArray = Directory.GetFiles(Path.Combine(SourceFileClient), "BIDC.CLIENT." + yesterdate);
            }
            foreach (var p in clientFileArray)
            {
                // Remove path from the file name.
                string fNameClient = p.Substring(SourceFileClient.Length + 1);
                string pathFileSuccess = Path.Combine(FolderPath, PathSuccess, PathImportDate + todate, fNameClient);
                if (!File.Exists(pathFileSuccess))
                {
                    File.Copy(Path.Combine(SourceFileClient, fNameClient), Path.Combine(FolderPath, fNameClient), true);
                }
            }
            // for invoice
            string SourceFileInvoice = GetConfig("SourceFileInvoice"); // example path: 
            string[] invoiceFileArray = new string[0];
            if (Directory.Exists(Path.Combine(SourceFileInvoice)))
            {
                invoiceFileArray = Directory.GetFiles(Path.Combine(SourceFileInvoice), "BIDC.INVOICE.DATA." + yesterdate);
            }
            foreach (var p in invoiceFileArray)
            {
                // Remove path from the file name.
                string fNameInvoice = p.Substring(SourceFileInvoice.Length + 1);
                string pathFileSuccess = Path.Combine(FolderPath, PathSuccess, PathImportDate + todate, fNameInvoice);
                if (!File.Exists(pathFileSuccess))
                {
                    File.Copy(Path.Combine(SourceFileInvoice, fNameInvoice), Path.Combine(FolderPath, fNameInvoice), true);
                }
            }
            //end funtion: copy file from orther server to FolderImportFile
        }

        private void insertSystemlogOracle(String ip, string sqlInsert, string FolderPath, string connectionString)
        {
            using (var connection = new OracleConnection(connectionString))
            {
                insertSystemlog(ip, sqlInsert, FolderPath, connection);
            }
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
    }
}
