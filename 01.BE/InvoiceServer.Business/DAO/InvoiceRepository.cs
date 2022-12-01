using Dapper;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Portal;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceRepository : GenericRepository<INVOICE>, IInvoiceRepository
    {
        private readonly DataClassesDataContext _dbSet = new DataClassesDataContext();
        private readonly Logger logger = new Logger();
        private static readonly Func<string, string> GetConfig = key => WebConfigurationManager.AppSettings[key];
        public InvoiceRepository(IDbContext context)
            : base(context)
        {
        }

        public IQueryable<InvoiceMaster> OrderByDefault(IQueryable<InvoiceMaster> invoiceInfos, bool desc = true)
        {
            return desc
                    ? invoiceInfos.OrderByDescending(i => i.InvoiceNo == 0)
                      .ThenByDescending(i => i.InvoiceCode)
                      .ThenByDescending(x => x.InvoiceSymbol)
                      .ThenByDescending(x => x.InvoiceNo)
                      .ThenByDescending(x => x.ReleasedDate)
                      .ThenByDescending(x => x.Id)
                    : invoiceInfos.OrderBy(i => i.InvoiceNo == 0)
                      .ThenBy(x => x.InvoiceCode)
                      .ThenBy(x => x.InvoiceSymbol)
                      .ThenBy(x => x.InvoiceNo)
                      .ThenBy(x => x.ReleasedDate)
                      .ThenBy(x => x.Id);
        }

        public long CountV2(ConditionSearchInvoice condition)
        {
            return FilterInvoiceListV2(condition).Count();
        }

        public IEnumerable<InvoiceMaster> FilterInvoice(ConditionSearchInvoice condition)
        {
            IQueryable<InvoiceMaster> invoiceInfos = FilterInvoiceList(condition);
            var listInvoiceInfos = invoiceInfos.GroupBy(x => x.Id).Select(g => g.FirstOrDefault()).AsNoTracking();
            var orderByDefault = false;

            //add sort theo dk
            listInvoiceInfos = listInvoiceInfos.OrderByDescending(p => p.Updated);

            if (condition.ColumnOrder.ToUpper() == "ID")
            {
                listInvoiceInfos = OrderByDefault(listInvoiceInfos, condition.OrderType.Equals(OrderTypeConst.Desc));
                orderByDefault = true;
            }

            if (!orderByDefault && (condition.InvoiceStatus.Count == 0 || condition.ColumnOrder.ToUpper() != "ID"))
            {
                listInvoiceInfos = listInvoiceInfos.OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc));
            }

            //var test = invoiceInfos.Skip(condition.Skip).Take(condition.Take);
            //var tt = test.ToList();

            var res = listInvoiceInfos.Skip(condition.Skip).Take(condition.Take).ToList();
            return res;
        }

        public IEnumerable<InvoiceMaster> FilterInvoiceSP_Replace(ConditionSearchInvoice condition)
        {
            var res = SearchInvoiceReplace(condition);
            return res;
        }

        public List<InvoicePrintInfo> SearchInvoiceReplace_Info(long id)
        {
            var listResult = new List<InvoicePrintInfo>();
            var result = new DataTable();
            DataSet dataset = new DataSet();
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
            using (OracleConnection objConn = new OracleConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = objConn;
                cmd.CommandText = "INVOICE_REPLACE_SEARCH_ANNOUNCE";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("P_INVOICEID", OracleDbType.Int64).Value = id; // Companyid
                cmd.Parameters.Add("P_OUTSTR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                try
                {
                    objConn.Open();
                    cmd.ExecuteNonQuery();
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    da.Fill(dataset);
                    result = dataset.Tables[0];
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Exception: {0}", ex.ToString());
                }
                objConn.Close();
            }

            foreach (DataRow row in result.Rows)
            {
                var invoice = new InvoicePrintInfo();
                invoice.CustomerName = row["CUSTOMERNAME"].ToString(); 
                invoice.CompanyName = row["COMPANYNAME"].ToString();
                invoice.CustomerCompanyName = row["CUSTOMERNAME"].ToString();
                invoice.CustomerAddress = row["CMPADDRESS"].ToString();
                invoice.Tel = row["CUSMOBILE"].ToString();
                invoice.CustomerTaxCode = row["CUSTOMERTAXCODE"].ToString();
                invoice.InvoiceCode = row["INVOICECODE"].ToString();
                invoice.Id = Int64.Parse(row["ID"].ToString());
                invoice.InvoiceNo = row["INVOICENO"].ToString();
                invoice.Total = decimal.Parse(row["TOTAL"].ToString());
                invoice.Sum = decimal.Parse(row["SUM"].ToString());
                invoice.Delegate = row["CUSDELEGATE"].ToString();
                invoice.DateRelease = Convert.ToDateTime(row["RELEASEDDATE"].ToString());
                invoice.DateClientSign = Convert.ToDateTime(row["CREATEDDATE"].ToString());
                invoice.PersonContact = row["PERSONCONTACT"].ToString();
                invoice.CurrencyCode = row["CURRENCYCODE"].ToString();

                //CustomerName = isOrgCurrentClient == false ? (notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT) : (notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME),
                //         CompanyName = company.COMPANYNAME,
                //         CustomerCompanyName = notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                //         CustomerAddress = notCurrentClient ? client.ADDRESS : invoice.CUSTOMERADDRESS,
                //         Tel = client.MOBILE,
                //         CustomerTaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                //         InvoiceCode = template.CODE,
                //         Id = invoice.ID,
                //         InvoiceNo = invoice.NO,
                //         Total = invoice.TOTAL,
                //         Sum = invoice.SUM,
                //         Delegate = client.DELEGATE,
                //         DateRelease = invoice.RELEASEDDATE,
                //         DateClientSign = invoice.CREATEDDATE,
                //         PersonContact = isOrgCurrentClient == false ? null : (notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT),
                //         CurrencyCode = currency.CODE,

                listResult.Add(invoice);
            }
            return listResult;
        }

        public List<InvoiceMaster> SearchInvoiceReplace(ConditionSearchInvoice condition)
        {
            var listResult = new List<InvoiceMaster>();
            var result = new DataTable();
            DataSet dataset = new DataSet();
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
            using (OracleConnection objConn = new OracleConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = objConn;
                cmd.CommandText = "INVOICE_REPLACE_SEARCH";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("P_COMPANYID", OracleDbType.Int64).Value = condition.CompanyId; // Companyid
                cmd.Parameters.Add("P_SYMBOL", OracleDbType.Char).Value = condition.Symbol; // Symbol
                cmd.Parameters.Add("P_INVOICENO", OracleDbType.Int64).Value = condition.InvoiceNo; // InvoiceNo
                cmd.Parameters.Add("P_OUTSTR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                try
                {
                    objConn.Open();
                    cmd.ExecuteNonQuery();
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    da.Fill(dataset);
                    result = dataset.Tables[0];
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Exception: {0}", ex.ToString());
                }
                objConn.Close();
            }

            foreach (DataRow row in result.Rows)
            {
                var invoice = new InvoiceMaster();

                invoice.Id = Int64.Parse(row["ID"].ToString());
                invoice.CompanyId = Int64.Parse(row["COMPANYID"].ToString());
                invoice.InvoiceSymbol = row["SYMBOL"].ToString();
                invoice.InvoiceCode = row["INVOICECODE"].ToString();
                invoice.ReleasedDate = Convert.ToDateTime(row["RELEASEDDATE"].ToString());
                invoice.InvoiceNo = decimal.Parse(row["INVOICENO"].ToString());
                invoice.No = row["INVOICENO"].ToString();
                invoice.Total = decimal.Parse(row["SUM"].ToString());
                invoice.Sum = decimal.Parse(row["SUM"].ToString());
                invoice.CurrencyCode = row["CURRENCYCODE"].ToString();
                invoice.ProductName = row["PRODUCTNAME"].ToString();
                invoice.CompanyName = row["COMPANYNAME"].ToString();
                invoice.CompanyTaxCode = row["CMPTAXCODE"].ToString();
                invoice.CompanyAddress = row["CMPADDRESS"].ToString();
                invoice.CompanyDelegate = row["CMPDELEGATE"].ToString();
                invoice.CompanyTel =  row["CMPTEL"].ToString();
                invoice.CompanyPosition = row["CMPPOS"].ToString();
                invoice.CustomerName = row["CUSTOMERNAME"].ToString();
                invoice.CustomerAddress = row["CUSTOMERADDRESS"].ToString();
                invoice.CustomerTaxCode = row["CUSTOMERTAXCODE"].ToString();
                invoice.CustomerDelegate = row["CUSDELEGATE"].ToString();
                invoice.CustomerMobile = row["CUSMOBILE"].ToString();
                invoice.CanCreateAnnoun = row["CANCREATEANOUMT"].ToString() == "1";
                invoice.TypeAnnoun = int.Parse(row["ANNOUNCEMENTTYPE"].ToString());
                invoice.InvoiceStatus = int.Parse(row["INVOICESTATUS"].ToString());
                listResult.Add(invoice);
            }
            return listResult;
        }

        public IEnumerable<InvoiceMaster> FilterInvoiceSP(ConditionSearchInvoice condition)
        {
            var res = SearchInvoice(condition);
            var orderByDefault = false;

            IQueryable<InvoiceMaster> test = res.AsQueryable();

            if (condition.ColumnOrder.ToUpper() == "ID")
            {
                test = OrderByDefault(test, condition.OrderType.Equals(OrderTypeConst.Desc));
                orderByDefault = true;
            }
            

            if (!orderByDefault && (condition.InvoiceStatus.Count == 0 || condition.ColumnOrder.ToUpper() != "ID"))
            {
                test = test.OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc));
            }
            return test.ToList();
        }
        public List<InvoiceMaster> SearchInvoice(ConditionSearchInvoice condition)
        {
            var listResult = new List<InvoiceMaster>();
            var result = new DataTable();
            DataSet dataset = new DataSet();
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
            using (OracleConnection objConn = new OracleConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = objConn;
                cmd.CommandText = "INVOICE_SEARCH";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("P_INVOICESAMPLE", OracleDbType.Int64).Value = condition.InvoiceSample; // Input id
                cmd.Parameters.Add("P_INVOICENO", OracleDbType.Int64).Value = condition.InvoiceNo; // Input id
                if (condition.DateFrom == null)
                {
                    cmd.Parameters.Add("P_DATEFROM", OracleDbType.Char).Value = null;
                }
                else
                {
                    cmd.Parameters.Add("P_DATEFROM", OracleDbType.Char).Value = condition.DateFrom.Value.ToString("yyyyMMdd");
                }
                if (condition.DateTo == null)
                {
                    cmd.Parameters.Add("P_DATETO", OracleDbType.Char).Value = null;
                }
                else
                {
                    cmd.Parameters.Add("P_DATETO", OracleDbType.Char).Value = condition.DateTo.Value.ToString("yyyyMMdd");
                }
                // Input id

                if (condition.InvoiceStatus.Count() > 0)
                {
                    if (condition.InvoiceStatus.FirstOrDefault() == 8)
                    {
                        cmd.Parameters.Add("P_INVOICESTATUS", OracleDbType.Int64).Value = null;
                    }
                    else
                    {
                        cmd.Parameters.Add("P_INVOICESTATUS", OracleDbType.Int64).Value = condition.InvoiceStatus.FirstOrDefault();
                    }
                    
                }
                else
                {
                    cmd.Parameters.Add("P_INVOICESTATUS", OracleDbType.Int64).Value = null;
                }
                //cmd.Parameters.Add("P_INVOICESTATUS", OracleDbType.Int64).Value = condition.InvoiceStatus.Count() > 0 ? condition.InvoiceStatus.FirstOrDefault() : 0;
                cmd.Parameters.Add("P_CIF", OracleDbType.Char).Value = condition.CustomerCode;
                cmd.Parameters.Add("P_TAXCODE", OracleDbType.Char).Value = condition.TaxCode;
                cmd.Parameters.Add("P_CUSTOMERNAME", OracleDbType.Char).Value = condition.CustomerName;
                cmd.Parameters.Add("P_COMPANYID", OracleDbType.Int64).Value = condition.Branch;
                cmd.Parameters.Add("P_SYMBOL", OracleDbType.Char).Value = condition.Symbol;
                cmd.Parameters.Add("P_BRANCHCODE", OracleDbType.Char).Value = condition.BranchID;
                cmd.Parameters.Add("P_IMPORTTYPE", OracleDbType.Int64).Value = condition.ImportType == 0 ? null : condition.ImportType;
                if (condition.TaxVAT != null)
                {
                    if (!condition.TaxVAT.Equals("null"))
                    {
                        cmd.Parameters.Add("P_VATTYPE", OracleDbType.Char).Value = condition.TaxVAT;
                    }
                    else
                    {
                        cmd.Parameters.Add("P_VATTYPE", OracleDbType.Char).Value = null;
                    }
                }
                else
                {
                    cmd.Parameters.Add("P_VATTYPE", OracleDbType.Char).Value = null;
                }


                cmd.Parameters.Add("P_SKIP", OracleDbType.Int64).Value = condition.Skip;
                cmd.Parameters.Add("P_TAKE", OracleDbType.Int64).Value = condition.Take;
                cmd.Parameters.Add("P_COUNT", OracleDbType.Int64).Value = condition.TotalRecords;
                cmd.Parameters.Add("P_OUTSTR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                try
                {
                    objConn.Open();
                    cmd.ExecuteNonQuery();
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    da.Fill(dataset);
                    result = dataset.Tables[0];
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Exception: {0}", ex.ToString());
                }
                objConn.Close();
            }

            foreach (DataRow row in result.Rows)
            {
                var invoice = new InvoiceMaster();
                invoice.Id = Int64.Parse(row["ID"].ToString());
                invoice.BranchId = row["TRX_BRANCH_NO"].ToString();
                invoice.InvoiceSymbol = row["SYMBOL"].ToString();
                var date = row["RELEASEDDATE"].ToString();
                invoice.ReleasedDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                invoice.InvoiceDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                invoice.InvoiceNo = decimal.Parse(row["INVOICENO"].ToString());
                invoice.No = row["INVOICENO"].ToString();
                invoice.CustomerCode = row["CIF_NO"].ToString();
                invoice.CustomerName = row["CUSTOMERNAME"].ToString();
                invoice.CurrencyCode = row["CURRENCYCODE"].ToString();
                invoice.Total = decimal.Parse(row["TOTAL"].ToString());
                invoice.TaxName = row["REPORT_CLASS"].ToString();
                invoice.TotalTax = decimal.Parse(row["TOTALTAX"].ToString());
                invoice.Sum = decimal.Parse(row["SUM"].ToString());
                invoice.Vat_invoice_type = row["VAT_INVOICE_TYPE"].ToString();
                invoice.InvoiceStatus = int.Parse(row["INVOICESTATUS"].ToString());
                invoice.HTHDon = int.Parse(row["HTHDon"].ToString());
                var statusCQT = row["StatusCQT"].ToString() == "" ? "0" : row["StatusCQT"].ToString();
                //invoice.statusCQT = int.Parse(statusCQT);
                
                invoice.codeCQT = row["codeCQT"].ToString();
                invoice.IsSentMail = int.Parse(row["sentMailStatus"].ToString() == "" ? "0" : row["sentMailStatus"].ToString());
                invoice.MessageCode = row["MESSAGECODE"].ToString();
                invoice.pThuc = int.Parse(row["PThuc"].ToString() == "" ? "0" : row["PThuc"].ToString());
                if (row["CONVERTED"].ToString() != "")
                {
                    if (row["CONVERTED"].ToString() == "1")
                    {
                        invoice.Converted = true;
                    }
                }
                else
                {
                    invoice.Converted = null;
                }

                invoice.CountRecord = int.Parse(row["COUNT_INVOICE"].ToString());

                if (row["INVOICETYPE"].ToString() != "")
                {
                    invoice.InvoiceType = int.Parse(row["INVOICETYPE"].ToString());
                }
                else
                {
                    invoice.InvoiceType = null;
                }

                if (row["BTHERRORSTATUS"].ToString() != "")
                {
                    invoice.Btherrorstatus = int.Parse(row["BTHERRORSTATUS"].ToString());
                }
                else
                {
                    invoice.Btherrorstatus = null;
                }

                invoice.Btherror = row["BTHERROR"].ToString();

                if (row["StatusCQT"].ToString() != "")
                {
                    if (invoice.Btherrorstatus != null)
                    {
                        invoice.statusCQT = 0;
                    }
                    else
                    {
                        invoice.statusCQT = int.Parse(statusCQT);
                    }
                }
                else
                {
                    invoice.statusCQT = null;
                }

                listResult.Add(invoice);
            }

            if (condition.InvoiceStatus.Count() > 0)
            {
                if (condition.InvoiceStatus.FirstOrDefault() == 8)
                {
                    listResult = listResult.Where(x => x.InvoiceStatus == (int)InvoiceStatus.Released || x.InvoiceStatus == (int)InvoiceStatus.Cancel || x.InvoiceStatus == (int)InvoiceStatus.Delete).ToList();
                }
                else
                {
                    listResult = listResult;
                }
            }
            else
            {
                listResult = listResult;
            }

            return listResult;
        }
        private IQueryable<InvoiceMaster> FilterInvoiceList(ConditionSearchInvoice condition)
        {
            var invoices = Filter(condition);

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));

            string configCustomerCode = GetConfig("khachle");
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var invoiceInfos = (from invoice in invoices
                                join templateCompanyuse in this.context.Set<REGISTERTEMPLATE>() on invoice.REGISTERTEMPLATEID equals templateCompanyuse.ID
                                join client in clients on invoice.CLIENTID equals client.ID
                                join announ in this.context.Set<ANNOUNCEMENT>().Where(p => p.ANNOUNCEMENTSTATUS >= 3 && (p.ANNOUNCEMENTTYPE == 2 || p.ANNOUNCEMENTTYPE == 3 || p.ANNOUNCEMENTTYPE == 1))
                                     on invoice.ID equals announ.INVOICEID into announs
                                from announ in announs.DefaultIfEmpty()
                                join invoice2 in this.dbSet.Where(p => !(p.DELETED ?? false))
                                     on invoice.ID equals invoice2.PARENTID into invoice2Temp
                                from invoice2 in invoice2Temp.DefaultIfEmpty()
                                join currency in this.context.Set<CURRENCY>() on invoice.CURRENCYID equals currency.ID into currencyTemp
                                from currency in currencyTemp.DefaultIfEmpty()
                                join minvoice in this.context.Set<MINVOICE_DATA>().Where(x => x.MESSAGECODE != null) on invoice.MESSAGECODE equals minvoice.MESSAGECODE into minvoices
                                from minvoice in minvoices.DefaultIfEmpty()
                                join declaration in this.context.Set<INVOICEDECLARATION>() on invoice.DECLAREID equals declaration.ID into declarations
                                from declaration in declarations.DefaultIfEmpty()
                                join mycompany in this.context.Set<MYCOMPANY>()
                                on invoice.COMPANYID equals mycompany.COMPANYSID
                                let invoiceStatus_Type_No = invoice.INVOICESTATUS != (int)InvoiceStatus.New || (invoice.INVOICETYPE ?? 0) != 0 || (invoice.INVOICENO ?? 0) != 0 //true: Hóa đơn điều chỉnh/thay thế, hoặc hóa đơn import đã có số sẵn
                                let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
                                select new InvoiceMaster
                                {
                                    Id = invoice.ID,
                                    InvoiceCode = templateCompanyuse.CODE,
                                    InvoiceSymbol = invoice.SYMBOL,
                                    InvoiceNo = invoice.INVOICENO ?? 0,
                                    No = invoice.NO,
                                    CustomerName = !invoiceStatus_Type_No && notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                    InvoiceDate = invoice.RELEASEDDATE ?? invoice.CREATEDDATE,
                                    ReleasedDate = invoice.RELEASEDDATE,
                                    Updated = invoice.UPDATEDDATE,
                                    InvoiceStatus = invoice.INVOICESTATUS,
                                    NumberAccount = invoice.NUMBERACCOUT,
                                    InvoiceNote = invoice.NOTE,
                                    TaxCode = !invoiceStatus_Type_No && notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                    Released = invoice.RELEASED,
                                    PersonContact = !invoiceStatus_Type_No && notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT,
                                    IsOrg = notCurrentClient ? isOrgCurrentClient : invoice.CUSTOMERTAXCODE != null,
                                    Sum = invoice.SUM,
                                    Converted = (invoice.CONVERTED ?? false),
                                    CompanyId = invoice.COMPANYID,
                                    CustomerCode = client.CUSTOMERCODE,
                                    Total = invoice.TOTAL,
                                    TotalTax = invoice.TOTALTAX,
                                    CanCreateAnnoun = announ == null,
                                    TypeAnnoun = announ == null ? 0 : announ.ANNOUNCEMENTTYPE,
                                    //TypeAnnoun = 0,
                                    RefNumber = invoice.REFNUMBER,
                                    ParentId = invoice.PARENTID,
                                    ChildId = invoice2 == null ? 0 : invoice2.ID,
                                    Teller = invoice.TELLER,
                                    HaveAdjustment = invoice2.PARENTID != null && invoice2.INVOICETYPE != (int)InvoiceType.Substitute,
                                    IsChange = invoice2.INVOICETYPE != null,
                                    CurrencyCode = currency.CODE,//Cột loại tiền
                                    fileReceipt = invoice.FILERECEIPT,
                                    codeCQT = minvoice.MCQT,
                                    MessageCode = invoice.MESSAGECODE,
                                    //statusCQT = minvoice.STATUS,
                                    statusCQT = (minvoice.STATUS != null)
                                        ? (minvoice.STATUS == 1) || (minvoice.STATUS == 0 && invoice.BTHERROR == null && invoice.BTHERRORSTATUS == null)
                                            ? 1
                                            : 0
                                        : minvoice.STATUS,
                                    HTHDon = invoice.HTHDON,
                                    pThuc = declaration.PTHUC,
                                    BranchId = mycompany.BRANCHID,
                                    TaxName = invoice.REPORT_CLASS,
                                    Vat_invoice_type = invoice.VAT_INVOICE_TYPE,
                                });


            invoiceInfos = this.SubFilterInvoiceList(invoiceInfos, condition);
            return invoiceInfos;
        }
        //private IQueryable<InvoiceMaster> FilterInvoiceListV2(ConditionSearchInvoice condition)
        //{
        //    var invoices = Filter(condition);

        //    var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
        //    var notification = from noti in this.context.Set<NOTIFICATIONMINVOICE>() select noti;
        //    var email = from e in this.context.Set<EMAILACTIVE>() select e;
        //    var emailRef = from er in this.context.Set<EMAILACTIVEREFERENCE>() select er;

        //    string configCustomerCode = GetConfig("khachle");
        //    bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
        //    var invoiceInfos = (from invoice in invoices
        //                        join templateCompanyuse in this.context.Set<REGISTERTEMPLATE>() on invoice.REGISTERTEMPLATEID equals templateCompanyuse.ID
        //                        join client in clients on invoice.CLIENTID equals client.ID
        //                        //join releaseInvoiceDetail in this.context.Set<RELEASEINVOICEDETAIL>().Where(p => p.CONVERTED != null && p.CONVERTED == true)
        //                        //     on invoice.ID equals releaseInvoiceDetail.INVOICEID into rid
        //                        //from releaseInvoiceDetail in rid.DefaultIfEmpty()
        //                        join announ in this.context.Set<ANNOUNCEMENT>().Where(p => p.ANNOUNCEMENTSTATUS >= 3 && (p.ANNOUNCEMENTTYPE == 2 || p.ANNOUNCEMENTTYPE == 3))
        //                             on invoice.ID equals announ.INVOICEID into announs
        //                        from announ in announs.DefaultIfEmpty()
        //                        join invoice2 in this.dbSet.Where(p => !(p.DELETED ?? false))
        //                             on invoice.ID equals invoice2.PARENTID into invoice2Temp
        //                        from invoice2 in invoice2Temp.DefaultIfEmpty()
        //                        join currency in this.context.Set<CURRENCY>() on invoice.CURRENCYID equals currency.ID into currencyTemp
        //                        from currency in currencyTemp.DefaultIfEmpty()
        //                        join minvoice in this.context.Set<MINVOICE_DATA>() on invoice.MESSAGECODE equals minvoice.MESSAGECODE into minvoices
        //                        from minvoice in minvoices.DefaultIfEmpty()
        //                        join minvoiceData in this.context.Set<MINVOICE_DATA>() on invoice.MESSAGECODE equals minvoiceData.MESSAGECODE into minvoiceDatas
        //                        from minvoiceData in minvoiceDatas.DefaultIfEmpty()
        //                        join declaration in this.context.Set<INVOICEDECLARATION>() on invoice.DECLAREID equals declaration.ID into declarations
        //                        from declaration in declarations.DefaultIfEmpty()
        //                        let invoiceStatus_Type_No = invoice.INVOICESTATUS != (int)InvoiceStatus.New || (invoice.INVOICETYPE ?? 0) != 0 || (invoice.INVOICENO ?? 0) != 0 //true: Hóa đơn điều chỉnh/thay thế, hoặc hóa đơn import đã có số sẵn
        //                        let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
        //                        let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
        //                        where (declaration.PTHUC != 1 || (declaration.PTHUC == 1 && minvoice.STATUS == StatusCQT.success))
        //                        select new InvoiceMaster
        //                        {
        //                            Id = invoice.ID,
        //                            InvoiceCode = templateCompanyuse.CODE,
        //                            InvoiceSymbol = invoice.SYMBOL,
        //                            InvoiceNo = invoice.INVOICENO ?? 0,
        //                            No = invoice.NO,
        //                            CustomerName = !invoiceStatus_Type_No && notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
        //                            InvoiceDate = invoice.RELEASEDDATE ?? invoice.CREATEDDATE,
        //                            ReleasedDate = invoice.RELEASEDDATE,
        //                            Updated = invoice.UPDATEDDATE,
        //                            InvoiceStatus = invoice.INVOICESTATUS,
        //                            NumberAccount = invoice.NUMBERACCOUT,
        //                            InvoiceNote = invoice.NOTE,
        //                            TaxCode = !invoiceStatus_Type_No && notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
        //                            Released = invoice.RELEASED,
        //                            PersonContact = !invoiceStatus_Type_No && notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT,
        //                            IsOrg = notCurrentClient ? isOrgCurrentClient : invoice.CUSTOMERTAXCODE != null,
        //                            Sum = invoice.SUM,
        //                            Converted = (invoice.CONVERTED ?? false),
        //                            CompanyId = invoice.COMPANYID,
        //                            CustomerCode = client.CUSTOMERCODE,
        //                            Total = invoice.TOTAL,
        //                            TotalTax = invoice.TOTALTAX,
        //                            CanCreateAnnoun = announ == null,
        //                            TypeAnnoun = announ == null ? 0 : announ.ANNOUNCEMENTTYPE,
        //                            RefNumber = invoice.REFNUMBER,
        //                            ParentId = invoice.PARENTID,
        //                            ChildId = invoice2 == null ? 0 : invoice2.ID,
        //                            Teller = invoice.TELLER,
        //                            HaveAdjustment = invoice2.PARENTID != null && invoice2.INVOICETYPE != (int)InvoiceType.Substitute,
        //                            IsChange = invoice2.INVOICETYPE != null,
        //                            CurrencyCode = currency.CODE,
        //                            fileReceipt = invoice.FILERECEIPT,
        //                            codeCQT = minvoice.MCQT,
        //                            MessageCode = invoice.MESSAGECODE,
        //                            statusCQT = minvoice.STATUS,
        //                            HTHDon = invoice.HTHDON,
        //                            statusNotificationCancel = (notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanCancel).STATUS == 1 && minvoiceData.MLTDIEP == MinvoiceStatus.NotificationApproved) ? 2
        //                                                        : notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanCancel).STATUS,
        //                            statusNotificationDelete = (notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanDelete).STATUS == 1 && minvoiceData.MLTDIEP == MinvoiceStatus.NotificationApproved) ? 2
        //                                                        : notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanDelete).STATUS,
        //                            statusNotificationAdjustment = (notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanAdjustment).STATUS == 1 && minvoiceData.MLTDIEP == MinvoiceStatus.NotificationApproved) ? 2
        //                                                        : notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanAdjustment).STATUS,
        //                            pThuc = declaration.PTHUC,
        //                            IsSentMail = email.FirstOrDefault(x => x.ID == emailRef.Where(i => i.REFID == invoice.ID).OrderByDescending(y => y.EMAILACTIVEID).FirstOrDefault().EMAILACTIVEID).SENDSTATUS,
        //                        });

        //    invoiceInfos = this.SubFilterInvoiceListV2(invoiceInfos, condition);
        //    return invoiceInfos;
        //}

        private IQueryable<InvoiceMaster> FilterInvoiceListV2(ConditionSearchInvoice condition)
        {
            var invoices = Filter(condition);
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            var emailRef = from er in this.context.Set<EMAILACTIVEREFERENCE>() select er;
            var email = from e in this.context.Set<EMAILACTIVE>() select e;
            var notification = from noti in this.context.Set<NOTIFICATIONMINVOICE>() select noti;
            var invoiceInfos = (from invoice in invoices
                                join templateCompanyuse in this.context.Set<REGISTERTEMPLATE>() on invoice.REGISTERTEMPLATEID equals templateCompanyuse.ID
                                join client in clients on invoice.CLIENTID equals client.ID
                                //join releaseInvoiceDetail in this.context.Set<RELEASEINVOICEDETAIL>().Where(p => p.CONVERTED != null && p.CONVERTED == true)
                                //     on invoice.ID equals releaseInvoiceDetail.INVOICEID into rid
                                //from releaseInvoiceDetail in rid.DefaultIfEmpty()
                                join announ in this.context.Set<ANNOUNCEMENT>().Where(p => p.ANNOUNCEMENTSTATUS >= 3 && (p.ANNOUNCEMENTTYPE == 2 || p.ANNOUNCEMENTTYPE == 3))
                                     on invoice.ID equals announ.INVOICEID into announs
                                from announ in announs.DefaultIfEmpty()
                                join invoice2 in this.dbSet.Where(p => !(p.DELETED ?? false))
                                        on invoice.ID equals invoice2.PARENTID into invoice2Temp
                                from invoice2 in invoice2Temp.DefaultIfEmpty()
                                join minvoiceData in this.context.Set<MINVOICE_DATA>() on invoice.MESSAGECODE equals minvoiceData.MESSAGECODE into minvoiceDatas
                                from minvoiceData in minvoiceDatas.DefaultIfEmpty()
                                join declar in this.context.Set<INVOICEDECLARATION>() on invoice.DECLAREID equals declar.ID into declarations
                                from declar in declarations.DefaultIfEmpty()
                                    //join emailRef in this.context.Set<EMAILACTIVEREFERENCE>() on invoice.ID equals emailRef.REFID into emailRefs
                                    //from emailRef in emailRefs.DefaultIfEmpty()
                                    //join email in this.context.Set<EMAILACTIVE>() on emailRef.EMAILACTIVEID equals email.ID into emails
                                    //from email in emails.DefaultIfEmpty()
                                join currency in this.context.Set<CURRENCY>() on invoice.CURRENCYID equals currency.ID into currencyTemp
                                from currency in currencyTemp.DefaultIfEmpty()
                                let invoiceStatus_Type_No = invoice.INVOICESTATUS != (int)InvoiceStatus.New || (invoice.INVOICETYPE ?? 0) != 0 || (invoice.INVOICENO ?? 0) != 0 //true: Hóa đơn điều chỉnh/thay thế, hoặc hóa đơn import đã có số sẵn
                                let notCurrentClient = client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                where declar.PTHUC != 1 || (declar.PTHUC == 1 && minvoiceData.STATUS == StatusCQT.success)
                                select new InvoiceMaster
                                {
                                    Id = invoice.ID,
                                    InvoiceCode = templateCompanyuse.CODE,
                                    InvoiceSymbol = invoice.SYMBOL,
                                    InvoiceNo = invoice.INVOICENO ?? 0,
                                    No = invoice.NO,
                                    CustomerName = !invoiceStatus_Type_No && notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                    InvoiceDate = invoice.RELEASEDDATE ?? invoice.CREATEDDATE,
                                    ReleasedDate = invoice.RELEASEDDATE,
                                    Updated = invoice.UPDATEDDATE,
                                    InvoiceStatus = invoice.INVOICESTATUS,
                                    NumberAccount = invoice.NUMBERACCOUT,
                                    InvoiceNote = invoice.NOTE,
                                    TaxCode = !invoiceStatus_Type_No && notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                    Released = invoice.RELEASED,
                                    PersonContact = !invoiceStatus_Type_No && notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT,
                                    IsOrg = client.ISORG ?? false,
                                    Sum = invoice.SUM,
                                    Converted = (invoice.CONVERTED ?? false),
                                    CompanyId = invoice.COMPANYID,
                                    CustomerCode = client.CUSTOMERCODE,
                                    Total = invoice.TOTAL,
                                    TotalTax = invoice.TOTALTAX,
                                    CanCreateAnnoun = announ == null,
                                    TypeAnnoun = announ == null ? 0 : announ.ANNOUNCEMENTTYPE,
                                    RefNumber = invoice.REFNUMBER,
                                    ParentId = invoice.PARENTID,
                                    ChildId = invoice2 == null ? 0 : invoice2.ID,
                                    Teller = invoice.TELLER,
                                    HaveAdjustment = invoice2.PARENTID != null && invoice2.INVOICETYPE != (int)InvoiceType.Substitute,
                                    IsChange = invoice2.INVOICETYPE != null,
                                    CurrencyCode = currency.CODE,
                                    fileReceipt = invoice.FILERECEIPT,
                                    statusNotificationCancel = (notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanCancel).STATUS == 1 && minvoiceData.MLTDIEP == MinvoiceStatus.NotificationApproved) ? 2
                                                                : notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanCancel).STATUS,
                                    statusNotificationDelete = (notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanDelete).STATUS == 1 && minvoiceData.MLTDIEP == MinvoiceStatus.NotificationApproved) ? 2
                                                                : notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanDelete).STATUS,
                                    statusNotificationAdjustment = (notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanAdjustment).STATUS == 1 && minvoiceData.MLTDIEP == MinvoiceStatus.NotificationApproved) ? 2
                                                                : notification.FirstOrDefault(x => x.INVOICEID == invoice.ID && x.NAME == GateWayLogName.VanAdjustment).STATUS,
                                    statusCQT = minvoiceData.STATUS,
                                    HTHDon = invoice.HTHDON,
                                    pThuc = declar.PTHUC,
                                    codeCQT = minvoiceData == null ? null : minvoiceData.MCQT,
                                    MessageCode = minvoiceData == null ? null : minvoiceData.MESSAGECODE,
                                    IsSentMail = email.FirstOrDefault(x => x.ID == emailRef.Where(i => i.REFID == invoice.ID).OrderByDescending(y => y.EMAILACTIVEID).FirstOrDefault().EMAILACTIVEID).SENDSTATUS,
                                });
            invoiceInfos = this.SubFilterInvoiceListV2(invoiceInfos, condition);
            return invoiceInfos;
        }

        private IQueryable<InvoiceMaster> SubFilterInvoiceList(IQueryable<InvoiceMaster> invoiceInfos, ConditionSearchInvoice condition)
        {
            //Add condition TH invoice.NO != DefaultFields.INVOICE_NO_DEFAULT_VALUE
            if (condition.CustomerName.IsNotNullOrEmpty())
            {
                bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
                if (isCurrentClient)
                {
                    invoiceInfos = from invoiceInfo in invoiceInfos
                                   let notCurrentClient = !isCurrentClient || invoiceInfo.CustomerCode != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                   where notCurrentClient ? (invoiceInfo.IsOrg == true ? invoiceInfo.CustomerName : invoiceInfo.PersonContact).ToUpper().Contains(condition.CustomerName.ToUpper())
                                        : invoiceInfo.CustomerName.ToUpper().Contains(condition.CustomerName.ToUpper()) || invoiceInfo.PersonContact.ToUpper().Contains(condition.CustomerName.ToUpper())
                                   select invoiceInfo;
                }
                else
                {
                    invoiceInfos = invoiceInfos.Where(p => (p.IsOrg == true ? p.CustomerName : p.PersonContact).ToUpper().Contains(condition.CustomerName.ToUpper()));
                }
            }
            //search CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                invoiceInfos = invoiceInfos.Where(p => p.CustomerCode.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                invoiceInfos = invoiceInfos.Where(p => p.TaxCode.Contains(condition.TaxCode));
            }

            if (condition.TaxVAT.IsNotNullOrEmpty())
            {
                if (!condition.TaxVAT.Equals("null"))
                {
                    invoiceInfos = invoiceInfos.Where(p => p.TaxName.Contains(condition.TaxVAT));
                }
            }

            if (condition.BranchID.IsNotNullOrEmpty())
            {
                invoiceInfos = invoiceInfos.Where(p => p.BranchId == condition.BranchID);
            }

            return invoiceInfos;
        }
        private IQueryable<InvoiceMaster> SubFilterInvoiceListV2(IQueryable<InvoiceMaster> invoiceInfos, ConditionSearchInvoice condition)
        {
            //Add condition TH invoice.NO != DefaultFields.INVOICE_NO_DEFAULT_VALUE
            if (condition.InvoiceNotiType == InvoiceNotiType.VanCancel)
            {
                invoiceInfos = invoiceInfos.Where(x => x.statusNotificationAdjustment != 1 && x.statusNotificationDelete != 1 &&
                                                       x.statusNotificationAdjustment != 2 && x.statusNotificationDelete != 2 && !x.HaveAdjustment);
            }
            else if (condition.InvoiceNotiType == InvoiceNotiType.VanAdjustment)
            {
                invoiceInfos = invoiceInfos.Where(x => x.statusNotificationCancel != 1 && x.statusNotificationDelete != 1 &&
                                                       x.statusNotificationCancel != 2 && x.statusNotificationDelete != 2 && !x.HaveAdjustment);
            }
            else if (condition.InvoiceNotiType == InvoiceNotiType.VanDelete)
            {
                invoiceInfos = invoiceInfos.Where(x => x.statusNotificationCancel != 1 && x.statusNotificationAdjustment != 1 &&
                                                       x.statusNotificationCancel != 2 && x.statusNotificationAdjustment != 2 && !x.HaveAdjustment && x.IsSentMail != 1);
            }
            if (condition.CustomerName.IsNotNullOrEmpty())
            {
                bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
                if (isCurrentClient)
                {
                    invoiceInfos = from invoiceInfo in invoiceInfos
                                   let notCurrentClient = !isCurrentClient || invoiceInfo.CustomerCode != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                   where notCurrentClient ? (invoiceInfo.IsOrg == true ? invoiceInfo.CustomerName : invoiceInfo.PersonContact).ToUpper().Contains(condition.CustomerName.ToUpper())
                                        : invoiceInfo.CustomerName.ToUpper().Contains(condition.CustomerName.ToUpper()) || invoiceInfo.PersonContact.ToUpper().Contains(condition.CustomerName.ToUpper())
                                   select invoiceInfo;
                }
                else
                {
                    invoiceInfos = invoiceInfos.Where(p => (p.IsOrg == true ? p.CustomerName : p.PersonContact).ToUpper().Contains(condition.CustomerName.ToUpper()));
                }
            }
            //search CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                invoiceInfos = invoiceInfos.Where(p => p.CustomerCode.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                invoiceInfos = invoiceInfos.Where(p => p.TaxCode.Contains(condition.TaxCode));
            }

            if (condition.FromInvoiceNo > 0)
            {
                invoiceInfos = invoiceInfos.Where(p => p.InvoiceNo >= condition.FromInvoiceNo);
            }

            if (condition.ToInvoiceNo > 0)
            {
                invoiceInfos = invoiceInfos.Where(p => p.InvoiceNo <= condition.ToInvoiceNo);
            }
            return invoiceInfos;
        }

        public long Count(ConditionSearchInvoice condition)
        {
            var listInvoiceInfos = FilterInvoiceList(condition).GroupBy(x => x.Id).Select(g => g.FirstOrDefault()).AsNoTracking();
            return listInvoiceInfos.Count();
        }

        public long CountCreated(ConditionSearchInvoice condition, int? flgCreate)
        {
            //FillterInvoiceCreated
            var invoices = FilterCreated(condition, flgCreate);
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));

            if (condition.CustomerName.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.ToUpper().Contains(condition.CustomerName.ToUpper()) || p.PERSONCONTACT.ToUpper().Contains(condition.CustomerName.ToUpper()));
            }

            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.Contains(condition.TaxCode));
            }
            //search CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            var invoiceInfos = (from invoice in invoices
                                join templateCompanyuse in this.context.Set<REGISTERTEMPLATE>()
                                    on invoice.REGISTERTEMPLATEID equals templateCompanyuse.ID
                                join client in clients
                                on invoice.CLIENTID equals client.ID
                                select new InvoiceMaster
                                {
                                    Id = invoice.ID,
                                });
            return invoiceInfos.Count();
        }

        public long CountConverted(ConditionSearchInvoice condition)
        {
            //FillterInvoiceConverted
            var invoices = Filter(condition);
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            var releaseInvoiceDetails = this.context.Set<RELEASEINVOICEDETAIL>().Select(d => d);

            if (condition.CustomerName.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.ToUpper().Contains(condition.CustomerName.ToUpper()) || p.PERSONCONTACT.ToUpper().Contains(condition.CustomerName.ToUpper()));
            }

            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.Contains(condition.TaxCode));
            }

            //search CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (condition.Converted != null)
            {
                if (condition.Converted == true)
                {
                    releaseInvoiceDetails = releaseInvoiceDetails.Where(d => d.CONVERTED == condition.Converted);
                }
                else
                {
                    releaseInvoiceDetails = releaseInvoiceDetails.Where(d => d.CONVERTED == null || d.CONVERTED == false);
                }
            }

            var invoiceInfos = (from invoice in invoices
                                join templateCompanyuse in this.context.Set<REGISTERTEMPLATE>()
                                    on invoice.REGISTERTEMPLATEID equals templateCompanyuse.ID
                                join client in clients
                                     on invoice.CLIENTID equals client.ID
                                join releaseInvoiceDetail in releaseInvoiceDetails
                                     on invoice.ID equals releaseInvoiceDetail.INVOICEID
                                where (releaseInvoiceDetail.VERIFICATIONCODE != null
                                    && releaseInvoiceDetail.VERIFICATIONCODE.Trim() != string.Empty)

                                select new InvoiceMaster
                                {
                                    Id = invoice.ID,
                                });
            return invoiceInfos.Count();
        }

        public IEnumerable<InvoiceMaster> FilterInvoiceAnnoun(ConditionSearchInvoice condition)
        {
            var invoices = FilterAnnoun(condition);

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));

            if (condition.CustomerName.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.Contains(condition.CustomerName) || p.PERSONCONTACT.Contains(condition.CustomerName));
            }

            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.Contains(condition.TaxCode));
            }

            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var invoiceInfos = from invoice in invoices
                               join templateCompanyuse in this.context.Set<REGISTERTEMPLATE>() on invoice.REGISTERTEMPLATEID equals templateCompanyuse.ID
                               join client in clients on invoice.CLIENTID equals client.ID
                               let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                               let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
                               select new InvoiceMaster
                               {
                                   Id = invoice.ID,
                                   InvoiceCode = templateCompanyuse.CODE,
                                   InvoiceSymbol = invoice.SYMBOL,
                                   InvoiceNo = invoice.INVOICENO ?? 0,
                                   No = invoice.NO,
                                   CustomerName = notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                   InvoiceDate = invoice.CREATEDDATE,
                                   ReleasedDate = invoice.RELEASEDDATE,
                                   Updated = invoice.UPDATEDDATE,
                                   InvoiceStatus = invoice.INVOICESTATUS,
                                   NumberAccount = invoice.NUMBERACCOUT,
                                   InvoiceNote = invoice.NOTE,
                                   TaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                   Released = invoice.RELEASED,
                                   PersonContact = notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT,
                                   IsOrg = isOrgCurrentClient,
                                   Sum = invoice.SUM,
                               };
            return invoiceInfos;
        }

        public IEnumerable<ReleaseInvoiceInfo> GetListInvoiceNo(List<ReleaseInvoiceInfo> releaseInvoiceInfo)
        {
            var invoiceInfos = this.dbSet.Select(i => new ReleaseInvoiceInfo()
            {
                InvoiceId = i.ID,
                InvoiceNo = i.NO
            });

            //var listreleaseInvoiceId = releaseInvoiceInfo.Select(i => i.InvoiceId).ToList();
            var listreleaseInvoiceId = releaseInvoiceInfo.Select(i => i.InvoiceId);
            if (listreleaseInvoiceId.Count() > 0)
            {
                invoiceInfos = invoiceInfos.Where(i => listreleaseInvoiceId.Contains(i.InvoiceId));
            }
            else
            {
                invoiceInfos = null;
            }

            return invoiceInfos;
        }

        public ReleaseInvoiceInfo GetInvoiceReleaseInfoById(long invoiceId)
        {
            var invoiceInfo = this.dbSet
                .Where(p => p.ID == invoiceId)
                .Select(i => new ReleaseInvoiceInfo()
                {
                    InvoiceId = i.ID,
                    InvoiceNo = i.NO,
                    IsImportedByJob = i.CREATEDBY == null,
                })
                .FirstOrDefault();

            return invoiceInfo;
        }

        public IEnumerable<ReleaseInvoiceInfo> GetAllInvoiceApproved()
        {
            var invoiceInfos = this.dbSet
                .Where(p => !(p.DELETED ?? false)
                    && (p.INVOICESTATUS == (int)InvoiceStatus.Approved || p.INVOICESTATUS == (int)InvoiceStatus.Releaseding))
                .Select(i => new ReleaseInvoiceInfo()
                {
                    InvoiceId = i.ID,
                    InvoiceNo = i.NO,
                    IsImportedByJob = i.CREATEDBY == null,
                });

            return invoiceInfos;
        }
        public IEnumerable<InvoiceMaster> FilterInvoiceV2(ConditionSearchInvoice condition)
        {
            IQueryable<InvoiceMaster> invoiceInfos = FilterInvoiceListV2(condition);

            // Add and sort follow condition
            invoiceInfos = OrderByDefault(invoiceInfos, condition.OrderType.Equals(OrderTypeConst.Desc));

            invoiceInfos = invoiceInfos.Skip(condition.Skip).Take(condition.Take);

            return invoiceInfos.ToList();
        }
        public List<InvoicePrintModel> GetInvoiceSendMails(long companyid)
        {
            var result = new List<InvoicePrintModel>();
            try
            {

                string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
                string sqlResult = @"SELECT IV.*, CL.CUSTOMERCODE, CL.MOBILE, CL.FAX, CL.EMAIL, CL.DELEGATE, CL.ACCOUNTHOLDER, 
                                    to_char(CL.ISORG) ISORG, to_char(CL.USEREGISTEREMAIL) USEREGISTEREMAIL, CL.RECEIVEDINVOICEEMAIL
                                    FROM INVOICE IV
                                        INNER JOIN CLIENT CL ON CL.ID = IV.CLIENTID
                                    WHERE IV.COMPANYID = @companyid 
                                        AND (IV.DELETED IS NULL OR IV.DELETED = 0)
                                        AND IV.INVOICESTATUS = 4
                                        AND IV.INVOICENO > 0
                                        AND (IV.SENDMAILSTATUS IS NULL OR IV.SENDMAILSTATUS = 0)
                                        AND CL.ISORG = 1
                                    ORDER BY IV.ID";
                sqlResult = sqlResult.Replace("@companyid", companyid.ToString());
                using (var connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    result = connection.Query<InvoicePrintModel>(sqlResult).ToList();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }

        public void UpdateNoForCompany(long? companyId)
        {
            //this._dbSet.TEST_SETINVOICENO(companyId);
            var db = new DataClassesDataContext();
            //var outputparam = new ObjectParameter("SP_RESULTOUTPUT", typeof(decimal));
            db.TEST_SETINVOICENO(companyId);
        }

        public bool Approve_Invoice(string day, long? companyId, string symbol, long? invoiceid)
        {
            //var db = new DataClassesDataContext();

            var outputparam = new ObjectParameter("p_SUCCESS", typeof(decimal));
            this._dbSet.APPROVE_INVOICE(day, companyId, symbol, invoiceid, outputparam);
            var result = outputparam.Value.ToDecimal();
            if (result == 1)
            {
                return true;
            }
            return false;
        }
        public bool checkBeforeSign(long companyId, string symbol)
        {
            var outputparam = new ObjectParameter("p_SUCCESS", typeof(decimal));
            this._dbSet.INVOICENO_CHECK(DateTime.Now.ToString("yyyyMMdd"), companyId, symbol, outputparam);
            var result = outputparam.Value.ToDecimal();
            return result == 1 ? true : false;
        }
        public List<CompanySymbolInfo> GetAllSymbol()
        {
            List<CompanySymbolInfo> sYMBOLs = (from symbol in this.context.Set<SYMBOL>()
                                               join invoiceDeclara in this.context.Set<INVOICEDECLARATION>()
                                               on symbol.REFID equals invoiceDeclara.ID
                                               join mycompany in this.context.Set<MYCOMPANY>()
                                               on symbol.COMPANYID equals mycompany.COMPANYSID
                                               where invoiceDeclara.STATUS == (int)DeclarationStatus.Completed
                                               select new CompanySymbolInfo
                                               {
                                                   CompanyId = mycompany.COMPANYID,
                                                   Symbol = symbol.SYMBOL1
                                               }
                                    ).ToList();

            sYMBOLs = sYMBOLs.GroupBy(x => new { x.CompanyId, x.Symbol }).Select(x => new CompanySymbolInfo
            {
                CompanyId = x.FirstOrDefault().CompanyId,
                Symbol = x.FirstOrDefault().Symbol
            }).ToList();

            return sYMBOLs;
        }
        public List<InvoicePrintModel> GetInvoicePrintCreateFile(long companyid)
        {
            var result = new List<InvoicePrintModel>();
            try
            {
                bool signOrg = bool.Parse(GetConfig("signOrg"));
                string whereIsorg = string.Empty;
                if (signOrg)
                {
                    whereIsorg = "AND CL.ISORG = 1";
                }
                string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
                string sqlResult = @"SELECT IV.*, CL.CUSTOMERCODE, CL.MOBILE, CL.FAX, CL.EMAIL, CL.DELEGATE, CL.ACCOUNTHOLDER, to_char(CL.ISORG) ISORG,
                                MY.TEL1, MY.MOBILE AS COMMOBILE, RG.INVOICETEMPLATEID, RG.CODE AS INVOICECODE, TY.NAME AS TYPEPAYMENTNAME, 
                                CR.CODE AS CURRENCYCODE, CR.NAME AS CURRENCYNAME,CR.DECIMALSEPARATOR, CR.DECIMALUNIT,
                                IVREP.NO AS REPLACENO, IVREP.RELEASEDDATE AS REPLACEDATE, IVREP.SYMBOL AS REPLACESYMBOL,
                                AC.REPORTTEL, AC.REPORTWEBSITE, 
                                IV.VERIFICATIONCODE
                                , DE.HTHDON, MI.MCQT, MI.STATUS AS STATUSNOTIFICATION, 
                                IVR.ID AS REPLACEDINVOICEID, IVR.SYMBOL AS REPLACEDINVOICESYMBOL, IVR.NO AS REPLACEDINVOICENO, IVR.RELEASEDDATE AS REPLACEDINVOICERELEASEDDATE
                                , SG.SLOTS, SG.SERIALNUMBER, SG.PASSWORD,IV.IMPORTTYPE
                            FROM INVOICE IV
                                INNER JOIN CLIENT CL ON CL.ID = IV.CLIENTID
                                INNER JOIN MYCOMPANY MY ON MY.COMPANYSID = IV.COMPANYID
                                INNER JOIN CURRENCY CR ON CR.ID = IV.CURRENCYID
                                INNER JOIN InvoiceDeclaration DE ON DE.COMPANYTAXCODE = MY.TAXCODE
                                INNER JOIN MINVOICE_DATA MI ON mi.messagecode = de.messagecode
                                INNER JOIN SIGNATURE SG ON SG.COMPANYID = MY.COMPANYID
                                LEFT JOIN INVOICE IVREP ON IVREP.ID = IV.PARENTID
                                LEFT JOIN REGISTERTEMPLATES RG ON RG.ID = IV.REGISTERTEMPLATEID
                                LEFT JOIN TYPEPAYMENT TY ON TY.ID = IV.TYPEPAYMENT
                                LEFT JOIN MYCOMPANY AC ON AC.COMPANYSID = MY.COMPANYID
                                LEFT JOIN INVOICE IVR ON IVR.ID = IV.PARENTID
                            WHERE IV.COMPANYID = @companyid
                                AND IV.DELETED IS NULL
                                AND (IV.INVOICESTATUS = 2 OR IV.INVOICESTATUS = 3)
                                AND IV.INVOICENO > 0
                                AND DE.STATUS = 5
                                {isorg}
                            ORDER BY IV.RELEASEDDATE ASC";
                sqlResult = sqlResult.Replace("@companyid", companyid.ToString()).Replace("{isorg}", whereIsorg);
                using (var connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    result = connection.Query<InvoicePrintModel>(sqlResult).ToList();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Logger logger = new Logger();
                logger.Error("GetInvoicePrintCreateFile", ex);
            }

            return result;
        }

        public List<InvoicePrintModel> GetInvoicePrintPersonal(long companyid)
        {
            var result = new List<InvoicePrintModel>();
            try
            {
                string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
                string sqlResult = @"SELECT IV.*, CL.CUSTOMERCODE, CL.MOBILE, CL.FAX, CL.EMAIL, CL.DELEGATE, CL.ACCOUNTHOLDER, to_char(CL.ISORG) ISORG,
                                MY.TEL1, MY.MOBILE AS COMMOBILE,MY.BRANCHID, RG.INVOICETEMPLATEID, RG.CODE AS INVOICECODE, TY.NAME AS TYPEPAYMENTNAME, 
                                CR.CODE AS CURRENCYCODE, CR.NAME AS CURRENCYNAME,CR.DECIMALSEPARATOR, CR.DECIMALUNIT,
                                IVREP.NO AS REPLACENO, IVREP.RELEASEDDATE AS REPLACEDATE, IVREP.SYMBOL AS REPLACESYMBOL,
                                AC.REPORTTEL, AC.REPORTWEBSITE, 
                                IV.VERIFICATIONCODE
                                , DE.HTHDON, MI.MCQT, MI.STATUS AS STATUSNOTIFICATION, 
                                IVR.ID AS REPLACEDINVOICEID, IVR.SYMBOL AS REPLACEDINVOICESYMBOL, IVR.NO AS REPLACEDINVOICENO, IVR.RELEASEDDATE AS REPLACEDINVOICERELEASEDDATE
                                , SG.SLOTS, SG.SERIALNUMBER, SG.PASSWORD, SG.PASSWORDCA
                            FROM INVOICE IV
                                INNER JOIN CLIENT CL ON CL.ID = IV.CLIENTID
                                INNER JOIN MYCOMPANY MY ON MY.COMPANYSID = IV.COMPANYID
                                INNER JOIN CURRENCY CR ON CR.ID = IV.CURRENCYID
                                INNER JOIN InvoiceDeclaration DE ON DE.COMPANYTAXCODE = MY.TAXCODE
                                INNER JOIN MINVOICE_DATA MI ON mi.messagecode = de.messagecode
                                INNER JOIN SIGNATURE SG ON SG.COMPANYID = MY.COMPANYID
                                LEFT JOIN INVOICE IVREP ON IVREP.ID = IV.PARENTID
                                LEFT JOIN REGISTERTEMPLATES RG ON RG.ID = IV.REGISTERTEMPLATEID
                                LEFT JOIN TYPEPAYMENT TY ON TY.ID = IV.TYPEPAYMENT
                                LEFT JOIN MYCOMPANY AC ON AC.COMPANYSID = MY.COMPANYID
                                LEFT JOIN INVOICE IVR ON IVR.ID = IV.PARENTID
                            WHERE IV.COMPANYID = @companyid
                                AND IV.DELETED IS NULL
                                AND (IV.INVOICESTATUS = 2 OR IV.INVOICESTATUS = 3)
                                AND IV.INVOICENO > 0
                                AND DE.STATUS = 5
                                AND (CL.ISORG = 0 OR CL.ISORG = NULL)
                            ORDER BY IV.RELEASEDDATE ASC";
                sqlResult = sqlResult.Replace("@companyid", companyid.ToString());
                using (var connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    result = connection.Query<InvoicePrintModel>(sqlResult).ToList();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Logger logger = new Logger();
                logger.Error("GetInvoicePrintCreateFile", ex);
            }

            return result;
        }
        public InvoicePrintModel GetInvoicePrint(long invoiceid)
        {
            var result = new InvoicePrintModel();
            try
            {
                string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
                string sqlResult = @"SELECT IV.*, CL.CUSTOMERCODE, CL.MOBILE, CL.FAX, CL.EMAIL, CL.DELEGATE, CL.ACCOUNTHOLDER, to_char(CL.ISORG) ISORG,
                                MY.TEL1, MY.MOBILE AS COMMOBILE, RG.INVOICETEMPLATEID, RG.CODE AS INVOICECODE, TY.NAME AS TYPEPAYMENTNAME, 
                                CR.CODE AS CURRENCYCODE, CR.NAME AS CURRENCYNAME,CR.DECIMALSEPARATOR, CR.DECIMALUNIT,
                                IVREP.NO AS REPLACENO, IVREP.RELEASEDDATE AS REPLACEDATE, IVREP.SYMBOL AS REPLACESYMBOL,
                                AC.REPORTTEL, AC.REPORTWEBSITE, 
                                IV.VERIFICATIONCODE
                                , DE.HTHDON, MI.MCQT, MI.STATUS AS STATUSNOTIFICATION, 
                                IVR.ID AS REPLACEDINVOICEID, IVR.SYMBOL AS REPLACEDINVOICESYMBOL, IVR.NO AS REPLACEDINVOICENO, IVR.RELEASEDDATE AS REPLACEDINVOICERELEASEDDATE
                                , SG.SLOTS, SG.SERIALNUMBER, SG.PASSWORD,IV.IMPORTTYPE
                            FROM INVOICE IV
                                INNER JOIN CLIENT CL ON CL.ID = IV.CLIENTID
                                INNER JOIN MYCOMPANY MY ON MY.COMPANYSID = IV.COMPANYID
                                INNER JOIN CURRENCY CR ON CR.ID = IV.CURRENCYID
                                INNER JOIN InvoiceDeclaration DE ON DE.COMPANYTAXCODE = MY.TAXCODE
                                LEFT JOIN MINVOICE_DATA MI ON MI.MESSAGECODE = DE.MESSAGECODE
                                INNER JOIN SIGNATURE SG ON SG.COMPANYID = MY.COMPANYID
                                LEFT JOIN INVOICE IVREP ON IVREP.ID = IV.PARENTID
                                LEFT JOIN REGISTERTEMPLATES RG ON RG.ID = IV.REGISTERTEMPLATEID
                                LEFT JOIN TYPEPAYMENT TY ON TY.ID = IV.TYPEPAYMENT
                                LEFT JOIN MYCOMPANY AC ON AC.COMPANYSID = MY.COMPANYID
                                LEFT JOIN INVOICE IVR ON IVR.ID = IV.PARENTID
                            WHERE IV.ID = @invoiceid";
                sqlResult = sqlResult.Replace("@invoiceid", invoiceid.ToString());
                using (var connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    result = connection.Query<InvoicePrintModel>(sqlResult).FirstOrDefault();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Logger logger = new Logger();
                logger.Error("GetInvoicePrint", ex);
            }

            return result;
        }
        public bool UpdateStatusAfterSign(List<InvoicePrintModel> invoiceModels, string pathSign)
        {
            List<INVOICE> lstInv = new List<INVOICE>();
            string pathFilePdf = String.Empty;
            string pathXml = String.Empty;
            foreach (var item in invoiceModels)
            {
                pathFilePdf = Path.Combine(pathSign, item.COMPANYID.ToString(), "Releases", "Sign", item.RELEASEDDATE.Value.ToString("yyyy"), item.RELEASEDDATE.Value.ToString("MM"), item.RELEASEDDATE.Value.ToString("dd"), item.ID.ToString() + "_sign.pdf");
                pathXml = pathFilePdf.Replace(".pdf", ".xml");
                if (File.Exists(pathFilePdf) && File.Exists(pathXml))
                {
                    var inv = GetById(Convert.ToInt64(item.ID));
                    inv.INVOICESTATUS = (int)InvoiceStatus.Released;
                    inv.UPDATEDDATE = DateTime.Now;
                    lstInv.Add(inv);
                }

            }
            return this.Update(lstInv[0]);
        }

        public IQueryable<INVOICE> GetAllInvoiceCreatedFileShinhan(long companyId, bool isOrg)
        {
            var invoiceInfos = this.dbSet
                .Where(p => !(p.DELETED ?? false)
                    && (p.INVOICESTATUS == (int)InvoiceStatus.Approved || p.INVOICESTATUS == (int)InvoiceStatus.Releaseding)
                    && p.COMPANYID == companyId
                    && p.CREATEDFILE == true);

            var clients = from cl in this.context.Set<CLIENT>()
                          where cl.ISORG == isOrg
                          select cl.ID;

            invoiceInfos = invoiceInfos
                .Where(p => (clients.Contains((long)p.CLIENTID)));

            return invoiceInfos;
        }
        public IQueryable<INVOICE> GetAllInvoiceNewShinhan(long companyId, bool isOrg)
        {
            var invoiceInfos = this.dbSet
                .Where(p => !(p.DELETED ?? false)
                    && (p.INVOICESTATUS == (int)InvoiceStatus.New)
                    && p.COMPANYID == companyId);

            //var clients = from cl in this.context.Set<CLIENT>()
            //              where cl.ISORG == isOrg
            //              select cl.ID;

            //invoiceInfos = invoiceInfos.Where(p => (clients.Contains((long)p.CLIENTID))).OrderBy(x=> x.RELEASEDDATE);

            return invoiceInfos;
        }
        public IEnumerable<ReleaseInvoiceInfo> GetInvoiceApprovedByBranch(long branch)
        {
            var invoiceInfos = this.dbSet
                .Where(p => !(p.DELETED ?? false)
                    && (p.INVOICESTATUS == (int)InvoiceStatus.Approved || p.INVOICESTATUS == (int)InvoiceStatus.Releaseding)
                    && p.COMPANYID == branch)
                .Select(i => new ReleaseInvoiceInfo()
                {
                    InvoiceId = i.ID,
                    InvoiceNo = i.NO,
                    IsImportedByJob = i.CREATEDBY == null,
                });

            return invoiceInfos;
        }

        public IEnumerable<InvoiceMaster> FilterInvoiceCreated(ConditionSearchInvoice condition, int? flgCreate = 0)
        {
            var invoices = FilterCreated(condition, flgCreate);
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false) && p.CUSTOMERNAME != "DRAFT");

            if (condition.CustomerName.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.ToUpper().Contains(condition.CustomerName.ToUpper()) || p.PERSONCONTACT.ToUpper().Contains(condition.CustomerName.ToUpper()));
            }

            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.Contains(condition.TaxCode));
            }
            //search CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            IQueryable<InvoiceMaster> invoiceInfos = (from invoice in invoices
                                                      join templateCompanyuse in this.context.Set<REGISTERTEMPLATE>()
                                                          on invoice.REGISTERTEMPLATEID equals templateCompanyuse.ID
                                                      join client in clients
                                                      on invoice.CLIENTID equals client.ID
                                                      let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                                      let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
                                                      select new InvoiceMaster
                                                      {
                                                          Id = invoice.ID,
                                                          InvoiceCode = templateCompanyuse.CODE,
                                                          InvoiceSymbol = invoice.SYMBOL,
                                                          InvoiceNo = invoice.INVOICENO ?? 0,
                                                          No = invoice.NO,
                                                          CustomerName = notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                                          InvoiceDate = invoice.CREATEDDATE,
                                                          ReleasedDate = invoice.RELEASEDDATE,
                                                          InvoiceStatus = invoice.INVOICESTATUS,
                                                          NumberAccount = invoice.NUMBERACCOUT,
                                                          InvoiceNote = invoice.NOTE,
                                                          TaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                                          Released = invoice.RELEASED,
                                                          PersonContact = notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT,
                                                          IsOrg = isOrgCurrentClient,
                                                          CustomerCode = notCurrentClient ? client.CUSTOMERCODE : invoice.CUSTOMERTAXCODE,
                                                          Teller = invoice.TELLER
                                                      });
            invoiceInfos = OrderByDefault(invoiceInfos, condition.OrderType.Equals(OrderTypeConst.Desc));
            var res = invoiceInfos.Skip(condition.Skip).Take(condition.Take).ToList();
            return res;
        }
        public IEnumerable<ReleaseInvoiceInfo> ListInvoiceCreated(ConditionSearchInvoice condition, int? flgCreate = 0)
        {
            var invoices = FilterCreated(condition, flgCreate);
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));

            if (condition.CustomerName.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.ToUpper().Contains(condition.CustomerName.ToUpper()) || p.PERSONCONTACT.ToUpper().Contains(condition.CustomerName.ToUpper()));
            }

            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.Contains(condition.TaxCode));
            }
            //search CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            var invoiceInfosShinhan = (from invoice in invoices
                                       join templateCompanyuse in this.context.Set<REGISTERTEMPLATE>()
                                           on invoice.REGISTERTEMPLATEID equals templateCompanyuse.ID
                                       join client in clients
                                       on invoice.CLIENTID equals client.ID
                                       select new ReleaseInvoiceInfo
                                       {
                                           InvoiceId = invoice.ID,
                                           InvoiceNo = invoice.NO,
                                           IsImportedByJob = invoice.CREATEDBY == null,
                                           DateRelease = invoice.RELEASEDDATE
                                       });
            var resShinhan = invoiceInfosShinhan.OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
            return resShinhan;
        }
        public long CountInvoiceCreated(ConditionSearchInvoice condition, int? flgCreate = 0)
        {
            var invoices = FilterCreated(condition, flgCreate);
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));

            if (condition.CustomerName.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.ToUpper().Contains(condition.CustomerName.ToUpper()) || p.PERSONCONTACT.ToUpper().Contains(condition.CustomerName.ToUpper()));
            }

            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.Contains(condition.TaxCode));
            }
            //search CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            var invoiceInfosShinhan = (from invoice in invoices
                                       join templateCompanyuse in this.context.Set<REGISTERTEMPLATE>()
                                           on invoice.REGISTERTEMPLATEID equals templateCompanyuse.ID
                                       join client in clients
                                       on invoice.CLIENTID equals client.ID
                                       select new ReleaseInvoiceInfo
                                       {
                                           InvoiceId = invoice.ID,
                                           InvoiceNo = invoice.NO
                                       });
            return invoiceInfosShinhan.Count();
        }

        public IEnumerable<InvoiceMaster> OrderByDefault(IEnumerable<InvoiceMaster> invoiceInfos, bool desc = true)
        {
            return desc
                   ? invoiceInfos.OrderByDescending(i => i.InvoiceNo == 0)
                     .ThenByDescending(i => i.InvoiceNo)
                     .ThenByDescending(x => x.InvoiceSymbol)
                     .ThenByDescending(x => x.InvoiceCode)
                     .ThenByDescending(x => x.ReleasedDate)
                     .ThenByDescending(x => x.Id)
                   : invoiceInfos.OrderBy(i => i.InvoiceNo == 0)
                     .ThenBy(x => x.InvoiceNo)
                     .ThenBy(x => x.InvoiceSymbol)
                     .ThenBy(x => x.InvoiceCode)
                     .ThenBy(x => x.ReleasedDate)
                     .ThenBy(x => x.Id);
        }

        public IEnumerable<InvoiceMaster> FillterInvoiceConverted(ConditionSearchInvoice condition)
        {
            var invoices = Filter(condition);
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            //var releaseInvoiceDetails = this.context.Set<RELEASEINVOICEDETAIL>().Select(d => d);

            if (condition.Converted != null)
            {
                if (condition.Converted == true)
                {
                    invoices = invoices.Where(d => d.CONVERTED == condition.Converted);
                }
                else
                {
                    invoices = invoices.Where(d => d.CONVERTED == null || d.CONVERTED == false);
                }
            }

            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var invoiceInfos = (from invoice in invoices
                                join templateCompanyuse in this.context.Set<REGISTERTEMPLATE>()
                                    on invoice.REGISTERTEMPLATEID equals templateCompanyuse.ID
                                join client in clients
                                on invoice.CLIENTID equals client.ID into clientTemp
                                from client in clientTemp.DefaultIfEmpty()
                                    //join releaseInvoiceDetail in releaseInvoiceDetails
                                    //     on invoice.ID equals releaseInvoiceDetail.INVOICEID
                                where (invoice.VERIFICATIONCODE != null
                                    && invoice.VERIFICATIONCODE.Trim() != string.Empty)
                                let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
                                select new InvoiceMaster
                                {
                                    Id = invoice.ID,
                                    InvoiceCode = templateCompanyuse.CODE,
                                    InvoiceSymbol = invoice.SYMBOL,
                                    InvoiceNo = invoice.INVOICENO ?? 0,
                                    No = invoice.NO,
                                    CustomerName = invoice.CUSTOMERNAME,
                                    InvoiceDate = invoice.RELEASEDDATE ?? invoice.CREATEDDATE,
                                    ReleasedDate = invoice.RELEASEDDATE,
                                    Updated = invoice.UPDATEDDATE,
                                    InvoiceStatus = invoice.INVOICESTATUS,
                                    NumberAccount = invoice.NUMBERACCOUT,
                                    InvoiceNote = invoice.NOTE,
                                    TaxCode = invoice.CUSTOMERTAXCODE,
                                    Released = invoice.RELEASED,
                                    Converted = invoice.CONVERTED,
                                    PersonContact = invoice.PERSONCONTACT,
                                    IsOrg = isOrgCurrentClient,
                                    CustomerCode = client.CUSTOMERCODE,
                                });

            // search for condition
            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                invoiceInfos = invoiceInfos.Where(p => p.TaxCode.Contains(condition.TaxCode));
            }

            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                invoiceInfos = invoiceInfos.Where(p => p.CustomerCode.Contains(condition.CustomerCode));
            }

            invoiceInfos = OrderByDefault(invoiceInfos, condition.OrderType.Equals(OrderTypeConst.Desc));

            return invoiceInfos;
        }

        private IQueryable<INVOICE> Filter(ConditionSearchInvoice condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.COMPANYID > 0);

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            }
            //else if (condition.Branch.HasValue && condition.Branch == 0)
            //{
            //    invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            //}

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.InvoiceSample == 1)
            {
                invoices = invoices.Where(p => p.INVOICETYPE == condition.InvoiceSample);
            }
            else if (condition.InvoiceSample == 2)
            {
                invoices = invoices.Where(p => 
                p.INVOICETYPE == (int)InvoiceType.AdjustmentUpDown || 
                p.INVOICETYPE == (int)InvoiceType.AdjustmentInfomation || 
                p.INVOICETYPE == (int)InvoiceType.AdjustmentTax || 
                p.INVOICETYPE == (int)InvoiceType.AdjustmentTaxCode || 
                p.INVOICETYPE == (int)InvoiceType.AdjustmentElse);
            }

            invoices = SubFilter(invoices, condition);

            return invoices;
        }

        private IQueryable<INVOICE> SubFilter(IQueryable<INVOICE> invoices, ConditionSearchInvoice condition)
        {
            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Contains(condition.Symbol));
            }

            if (condition.InvoiceStatus.Count > 0)
            {
                if (condition.InvoiceStatus[0] == 999)
                {
                    invoices = invoices.Where(p => p.INVOICESTATUS != (int)InvoiceStatus.Cancel && p.INVOICESTATUS != (int)InvoiceStatus.Released && p.INVOICESTATUS != (int)InvoiceStatus.Delete);
                }
                else
                {
                    invoices = invoices.Where(p => condition.InvoiceStatus.Contains(p.INVOICESTATUS ?? 0));
                }

            }

            if (condition.InvoiceNo.IsNotNullOrEmpty())
            {
                if (condition.AnnouncementSearch)
                {
                    invoices = invoices.Where(p => p.NO == condition.InvoiceNo);
                }
                else
                {
                    //invoices = invoices.Where(p => p.NO.Contains(condition.InvoiceNo));
                    invoices = invoices.Where(p => p.NO == condition.InvoiceNo);
                }
            }

            if (condition.NumberAccout.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.NUMBERACCOUT.Contains(condition.NumberAccout));
            }

            //them dieu kien p.RELEASEDDATE == null -> hien thi tat ca cac hoa don duoc tao bang tay
            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE == null || p.RELEASEDDATE >= condition.DateFrom.Value);
            }
            if (condition.DateTo.HasValue)
            {
                var dateTo = ((DateTime)condition.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                invoices = invoices.Where(p => p.RELEASEDDATE == null || p.RELEASEDDATE <= condition.DateTo.Value);
            }
            if (condition.IsInvoiceChange)
            {
                invoices = invoices.Where(p => (p.INVOICETYPE ?? 0) == 0);
            }

            if (condition.ImportType > 0)
            {
                invoices = invoices.Where(p => p.IMPORTTYPE == condition.ImportType);
            }

     
            return invoices;
        }

        private IQueryable<INVOICE> FilterAnnoun(ConditionSearchInvoice condition)
        {
            var invoices = GetInvoiceActive();

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL == condition.Symbol);
            }

            if (condition.InvoiceStatus.Count > 0)
            {
                invoices = invoices.Where(p => condition.InvoiceStatus.Contains(p.INVOICESTATUS ?? 0));
            }

            if (condition.InvoiceNo.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.NO.Contains(condition.InvoiceNo));
            }

            if (condition.NumberAccout.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.NUMBERACCOUT.Contains(condition.NumberAccout));
            }

            if (condition.InvoiceSample.HasValue)
            {
                invoices = invoices.Where(p => p.INVOICETYPE == condition.InvoiceSample);
            }

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                var dateTo = ((DateTime)condition.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                invoices = invoices.Where(p => p.RELEASEDDATE <= dateTo);
            }

            return invoices;
        }

        private IQueryable<INVOICE> FilterCreated(ConditionSearchInvoice condition, int? flagCreated = 0)
        {
            var invoices = GetInvoiceActive();
            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            }
            else if (condition.Branch.HasValue && condition.Branch == 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            }

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL == condition.Symbol);
            }

            if (condition.InvoiceStatus.Count > 0)
            {
                invoices = invoices.Where(p => condition.InvoiceStatus.Contains(p.INVOICESTATUS ?? 0));
            }

            if (condition.InvoiceNo.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.NO.Contains(condition.InvoiceNo));
            }

            if (condition.NumberAccout.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.NUMBERACCOUT.Contains(condition.NumberAccout));
            }

            if (condition.InvoiceSample == 1)
            {
                invoices = invoices.Where(p => p.INVOICETYPE == condition.InvoiceSample);
            }
            else if (condition.InvoiceSample == 2)
            {
                invoices = invoices.Where(p => p.INVOICETYPE == 2 || p.INVOICETYPE == 3);
            }

            invoices = this.SubFilterCreated(invoices, condition, flagCreated);

            return invoices;
        }

        public int GetNumberInvoiceUsed(ConditionReportUse condition)
        {
            var invoices = GetInvoiceActive()
                .Where(p => p.INVOICESTATUS >= ((int)InvoiceStatus.Released) &&
                    p.RELEASEDDATE >= condition.DateFrom &&
                    p.RELEASEDDATE < condition.DateTo && 
                    p.COMPANYID == condition.CompanyId);

            return invoices.Count();
        }

        public decimal CountInvoiceNotRelease(long companyId)
        {
            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);
            var lastDay = new DateTime(DateTime.Now.Year, 12, 31);
            var invoices = GetInvoiceActive()
                .Where(p => p.COMPANYID == companyId && p.INVOICESTATUS == ((int)InvoiceStatus.New) &&
                    p.CREATEDDATE > firstDay &&
                    p.CREATEDDATE < lastDay);

            return invoices.Count();
        }

        public decimal CountInvoiceAdjustment(long companyId)
        {
            List<int?> lstStatus = new List<int?>() {
                (int)InvoiceType.AdjustmentUpDown,
                (int)InvoiceType.AdjustmentInfomation,
                (int)InvoiceType.AdjustmentTax,
                (int)InvoiceType.AdjustmentTaxCode,
                (int)InvoiceType.AdjustmentElse
            };
            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);
            var lastDay = new DateTime(DateTime.Now.Year, 12, 31);
            var invoices = GetInvoiceActive()
                .Where(p => p.COMPANYID == companyId &&
                   lstStatus.Contains(p.INVOICETYPE)
                    && p.CREATEDDATE > firstDay && p.CREATEDDATE < lastDay);

            return invoices.Count();
        }

        public decimal CountInvoiceCancel(long companyId)
        {
            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);
            var lastDay = new DateTime(DateTime.Now.Year, 12, 31);
            var invoices = GetInvoiceActive()
                .Where(p => p.COMPANYID == companyId &&
                    p.INVOICESTATUS == ((int)InvoiceStatus.Cancel) &&
                    p.CREATEDDATE > firstDay &&
                    p.CREATEDDATE < lastDay);

            return invoices.Count();
        }

        public decimal CountInvoiceSubstitute(long companyId)
        {
            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);
            var lastDay = new DateTime(DateTime.Now.Year, 12, 31);
            var invoices = GetInvoiceActive()
                .Where(p => p.COMPANYID == companyId &&
                    p.INVOICETYPE == ((int)InvoiceType.Substitute) &&
                    p.CREATEDDATE > firstDay &&
                    p.CREATEDDATE < lastDay);

            return invoices.Count();
        }

        public decimal CountInvoiceDeleted(long companyId)
        {
            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);
            var lastDay = new DateTime(DateTime.Now.Year, 12, 31);
            var invoices = GetInvoiceActive()
                .Where(p => p.COMPANYID == companyId &&
                    p.INVOICESTATUS == ((int)InvoiceStatus.Delete) &&
                    p.CREATEDDATE > firstDay &&
                    p.CREATEDDATE < lastDay);

            return invoices.Count();
        }

        private IQueryable<INVOICE> SubFilterCreated(IQueryable<INVOICE> invoices, ConditionSearchInvoice condition, int? flagCreated = 0)
        {
            var systemSetting = this.context.Set<SYSTEMSETTING>().FirstOrDefault();

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                var dateTo = ((DateTime)condition.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                invoices = invoices.Where(p => p.RELEASEDDATE <= dateTo);
            }

            if (condition.IsInvoiceChange)
            {
                invoices = invoices.Where(p => (p.INVOICETYPE ?? 0) == 0);
            }

            return invoices;
        }

        public IEnumerable<INVOICE> GetInvoiceSignYDay(long CompanyId, long RegisterTemplateId, string Symbol, long InvoiceId, List<long> invoiceIDs)
        {

            DateTime date = DateTime.Today;

            DateTime? MaxDate = this.dbSet.Where(p => p.CREATEDDATE < date).Max(p => p.CREATEDDATE);
            return this.dbSet.Where(p =>
                p.CREATEDDATE == MaxDate &&
                p.COMPANYID == CompanyId &&
                p.REGISTERTEMPLATEID == RegisterTemplateId &&
                p.SYMBOL == Symbol &&
                p.INVOICESTATUS != (int)InvoiceStatus.Released &&
                p.INVOICESTATUS != (int)InvoiceStatus.Cancel &&
                !invoiceIDs.Contains(p.ID)
            );
        }
        public INVOICE GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }

        public INVOICE GetInvoiceBySymbolReleaseddateInvoiceno(DateTime releaseddate, decimal invoiceno, string symbol, long? clientId)
        {
            return this.dbSet.FirstOrDefault(p => p.RELEASEDDATE == releaseddate && p.INVOICENO == invoiceno && p.SYMBOL.Substring(p.SYMBOL.Length - 2, 2) == symbol && p.CLIENTID == clientId);
        }
        public INVOICE GetByIdOfSign(long Id)
        {
            var invoice = this.dbSet.Where(p => p.ID == Id).AsNoTracking();
            return invoice.FirstOrDefault();
        }
        private IQueryable<INVOICE> GetInvoiceActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false));
        }

        public INVOICE GetById(long id, long companyId)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id && p.COMPANYID == companyId);
        }

        public IEnumerable<INVOICE> FilterInvoice(long companyId, long templateId, string symbol)
        {
            return GetInvoiceActive().Where(p => p.COMPANYID == companyId && p.REGISTERTEMPLATEID == templateId && p.SYMBOL.Replace("/", "").Equals(symbol));
        }

        public string GetMaxInvoiceNo(long companyId, long templateId, string symbol)
        {
            var kq = GetInvoiceActive()
                .Where(p => p.COMPANYID == companyId && p.REGISTERTEMPLATEID == templateId && p.SYMBOL.Equals(symbol))
                .OrderByDescending(p => p.INVOICENO).Select(p => p.INVOICENO).FirstOrDefault().Value.ToString();
            return kq;
        }
        public decimal? GetMaxInvoiceNo2(long companyId, long templateId, string symbol)
        {
            //&& p.REGISTERTEMPLATEID == templateId
            decimal? kq = GetInvoiceActive().Where(p => p.COMPANYID == companyId && p.SYMBOL.Equals(symbol))
                .OrderByDescending(p => p.INVOICENO)
                .Select(p => p.INVOICENO).FirstOrDefault();

            return kq;
        }
        public IEnumerable<INVOICE> GetListInvoiceChild(long id)
        {
            var invoices = GetInvoiceActive().Where(p => p.PARENTID == id);
            return invoices;
        }

        public IEnumerable<INVOICE> FilterInvoice(ReleaseGroupInvoice condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.COMPANYID == condition.CompanyId && p.INVOICESTATUS == (int)InvoiceStatus.Approved);

            if (condition.invoiceTemplateId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.invoiceTemplateId.Value);
            }
            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.CREATEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.CREATEDDATE <= condition.DateTo.Value);
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Equals(condition.Symbol));
            }

            return invoices;
        }
        public long CountReplace(SearchReplacedInvoice condition)
        {
            var invoices = condition.InvoiceSamples.IsNotNullOrEmpty() ? GetInvoiceActive().Where(p => condition.InvoiceSamples.Contains((p.INVOICETYPE ?? 0)))
            : GetInvoiceActive();


            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            }
            else if (condition.Branch.HasValue && condition.Branch == 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            }

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.InvoiceSymbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Equals(condition.InvoiceSymbol));
            }

            if (condition.InvoiceNo.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.NO.Contains(condition.InvoiceNo));
            }

            if (condition.InvoiceNoReplaced.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.INVOICE2.NO.Contains(condition.InvoiceNoReplaced));
            }

            if (condition.NumberAccout.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.NUMBERACCOUT.Equals(condition.NumberAccout));
            }

            if (condition.InvoiceType.HasValue)
            {
                invoices = invoices.Where(p => p.INVOICETYPE == condition.InvoiceType);
            }

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                var dateTo = ((DateTime)condition.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                invoices = invoices.Where(p => p.RELEASEDDATE <= dateTo);
                var abc = invoices.ToList();
            }

            if (condition.CustomerName.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.Equals(condition.CustomerName));
            }

            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.Contains(condition.TaxCode));
            }

            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.Contains(condition.CustomerCode));
            }

            var replaceInvoices = from invoice in invoices
                                  join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                                      on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                                  join invoicetemp in this.context.Set<INVOICESAMPLE>()
                                      on registerTemplate.INVOICESAMPLEID equals invoicetemp.ID
                                  join client in clients
                                  on invoice.CLIENTID equals client.ID
                                  select new ReplacedInvoice
                                  {
                                      Id = invoice != null ? invoice.ID : 0,
                                  };
            return replaceInvoices.Count();
        }

        public IEnumerable<ReplacedInvoice> FilterReplacedInvoice(SearchReplacedInvoice condition)
        {
            var invoices = GetInvoiceActive().Where(p => condition.InvoiceSamples.Contains((p.INVOICETYPE ?? 0)));
            var aaaa = invoices.ToList();
            invoices = this.SubFilterReplacedInvoice(invoices, condition);

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));

            if (condition.CustomerName.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.Equals(condition.CustomerName));
            }

            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.Contains(condition.TaxCode));
            }

            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.Contains(condition.CustomerCode));
            }

            var replaceInvoicesQuery = (from invoice in invoices
                                        join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                                            on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                                        join invoicetemp in this.context.Set<INVOICESAMPLE>()
                                            on registerTemplate.INVOICESAMPLEID equals invoicetemp.ID
                                        join client in clients
                                        on invoice.CLIENTID equals client.ID
                                        select new ReplacedInvoice
                                        {
                                            Id = invoice.ID,
                                            Code = registerTemplate.CODE,
                                            Symbol = invoice.SYMBOL,
                                            No = invoice.NO,
                                            Note = invoice.NOTE,
                                            FileReceipt = invoice.FILERECEIPT,
                                            Created = invoice.CREATEDDATE,
                                            ParentId = invoice.PARENTID ?? -1,
                                            InvoiceStatus = (int)invoice.INVOICESTATUS
                                        });
            //var replaceInvoices = replaceInvoicesQuery.ToList();
            var replaceInvoices = replaceInvoicesQuery.OrderByDescending(p => p.Created).Skip(condition.Skip).Take(condition.Take).ToList();
            foreach (var replaceInvoice in replaceInvoices)
            {
                var parentInvoice = this.dbSet.Where(i => i.ID == replaceInvoice.ParentId).FirstOrDefault();
                if (parentInvoice != null)
                {
                    var beReplacedInvoices = new List<BeReplacedInvoice>()
                    {
                        new BeReplacedInvoice()
                        {
                            IdSubstitute = parentInvoice.ID,
                            CodeSubstitute = parentInvoice.REGISTERTEMPLATE.CODE,
                            SymbolSubstitute = parentInvoice.SYMBOL,
                            NoSubstitute = parentInvoice.NO,
                            NoteSubstitute = parentInvoice.NOTE,
                            FileReceiptSubstitute = parentInvoice.FILERECEIPT
                        }
                    };
                    replaceInvoice.BeReplacedInvoices = beReplacedInvoices;
                }
            }
            //var res = replaceInvoices.Skip(condition.Skip).Take(condition.Take).ToList();
            return replaceInvoices;
        }

        private IQueryable<INVOICE> SubFilterReplacedInvoice(IQueryable<INVOICE> invoices, SearchReplacedInvoice condition)
        {
            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            }
            else if (condition.Branch.HasValue && condition.Branch == 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            }

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.InvoiceSymbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Equals(condition.InvoiceSymbol));
            }

            if (condition.InvoiceNo.IsNotNullOrEmpty())
            {
                var No = Decimal.Parse(condition.InvoiceNo);
                invoices = invoices.Where(p => p.INVOICENO == No);
            }

            if (condition.InvoiceNoReplaced.IsNotNullOrEmpty())
            {
                var NoReplace = Decimal.Parse(condition.InvoiceNoReplaced);
                invoices = invoices.Where(p => p.INVOICE2.INVOICENO == NoReplace);
            }

            if (condition.NumberAccout.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.NUMBERACCOUT.Equals(condition.NumberAccout));
            }

            if (condition.InvoiceType.HasValue)
            {
                invoices = invoices.Where(p => p.INVOICETYPE == condition.InvoiceType);
            }

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                var dateTo = ((DateTime)condition.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                invoices = invoices.Where(p => p.RELEASEDDATE <= dateTo);
            }
            if (condition.InvoiceNoReplaced.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.INVOICE2.NO.Contains(condition.InvoiceNoReplaced));
            }

            return invoices;
        }

        public long CountList(ConditionReportDetailUse condition, int flgFill = 0)
        {
            if (flgFill == 0)
            {
                return this.CountListDetail(condition);
            }
            else if (flgFill == 1)
            {
                return this.CountListGroupBy(condition);
            }

            return 0;
        }

        private long CountListDetail(ConditionReportDetailUse condition)
        {
            //FillterListInvoice
            var invoices = this.dbSet.Where(p => p.COMPANYID > 0);

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            }
            else if (!condition.Branch.HasValue)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            }

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            //add condition CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Replace("/", "").Equals(condition.Symbol));
            }

            if (condition.InvoiceStatus.Count > 0)
            {
                invoices = invoices.Where(p => condition.InvoiceStatus.Contains(p.INVOICESTATUS ?? 0));
            }

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
            }
            //

            if (condition.CurrencyId.HasValue)
            {
                invoices = invoices.Where(p => p.CURRENCYID == condition.CurrencyId.Value);
            }

            var invoiceDetails = this.context.Set<INVOICEDETAIL>().Where(p => !(p.DELETED ?? false));

            if (condition.TaxId.HasValue)
            {
                invoiceDetails = invoiceDetails.Where(p => p.TAXID == condition.TaxId.Value);
            }

            var invoiceInfos = from invoice in invoices
                                   //join invoiceDetail in this.context.Set<INVOICEDETAIL>()
                               join invoiceDetail in invoiceDetails
                               on invoice.ID equals invoiceDetail.INVOICEID
                               //join client in this.context.Set<CLIENT>()
                               join client in clients
                               on invoice.CLIENTID equals client.ID
                               join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                               on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                               join invoiceSample in this.context.Set<INVOICESAMPLE>()
                               on registerTemplate.INVOICESAMPLEID equals invoiceSample.ID
                               join tax in this.context.Set<TAX>()
                                on invoiceDetail.TAXID equals tax.ID
                               join product in this.context.Set<PRODUCT>()
                                on invoiceDetail.PRODUCTID equals product.ID into gj
                               from product in gj.DefaultIfEmpty()
                               orderby registerTemplate.CODE, invoice.SYMBOL, invoice.NO
                               select new ReportInvoiceDetail
                               {
                                   Id = invoice.ID,
                               };

            var listInvoiceInfos = invoiceInfos.Count();
            return listInvoiceInfos;
        }

        private long CountListGroupBy(ConditionReportDetailUse condition)
        {
            //FillterListInvoiceGroupBy
            var invoices = this.dbSet.Where(p => p.COMPANYID > 0);

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            }
            else if (!condition.Branch.HasValue)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            }

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            //add condition CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Replace("/", "").Equals(condition.Symbol));
            }

            if (condition.InvoiceStatus.Count > 0)
            {
                invoices = invoices.Where(p => condition.InvoiceStatus.Contains(p.INVOICESTATUS ?? 0));
            }

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
            }
            //add condition
            if (condition.CurrencyId.HasValue)
            {
                invoices = invoices.Where(p => p.CURRENCYID == condition.CurrencyId.Value);
            }

            var invoiceInfos = from invoice in invoices
                                   //join client in this.context.Set<CLIENT>()
                               join client in clients
                               on invoice.CLIENTID equals client.ID
                               join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                               on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                               join invoiceSample in this.context.Set<INVOICESAMPLE>()
                               on registerTemplate.INVOICESAMPLEID equals invoiceSample.ID
                               select new ReportInvoiceDetail
                               {
                                   Id = invoice.ID,
                               };

            var listInvoiceInfos = invoiceInfos.Count();
            return listInvoiceInfos;
        }

        public IEnumerable<ReportInvoiceDetail> FillterListInvoice(ConditionReportDetailUse condition)
        {
            var invoices = this.dbSet.Where(p => p.COMPANYID > 0);

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            }
            else if (!condition.Branch.HasValue)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            }

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            //add condition CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Replace("/", "").Equals(condition.Symbol));
            }

            if (condition.InvoiceStatus.Count > 0)
            {
                invoices = invoices.Where(p => condition.InvoiceStatus.Contains(p.INVOICESTATUS ?? 0));
            }

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
            }

            //
            if (condition.CurrencyId.HasValue)
            {
                invoices = invoices.Where(p => p.CURRENCYID == condition.CurrencyId.Value);
            }

            var invoiceDetails = this.context.Set<INVOICEDETAIL>().Where(p => !(p.DELETED ?? false));

            if (condition.TaxId.HasValue)
            {
                invoiceDetails = invoiceDetails.Where(p => p.TAXID == condition.TaxId.Value);
            }

            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var invoiceInfos = from invoice in invoices
                                   //join invoiceDetail in this.context.Set<INVOICEDETAIL>()
                               join invoiceDetail in invoiceDetails
                               on invoice.ID equals invoiceDetail.INVOICEID
                               //join client in this.context.Set<CLIENT>()
                               join client in clients
                               on invoice.CLIENTID equals client.ID
                               join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                               on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                               join invoiceSample in this.context.Set<INVOICESAMPLE>()
                               on registerTemplate.INVOICESAMPLEID equals invoiceSample.ID
                               join tax in this.context.Set<TAX>()
                                on invoiceDetail.TAXID equals tax.ID
                               join currency in this.context.Set<CURRENCY>()
                                on invoice.CURRENCYID equals currency.ID
                               join product in this.context.Set<PRODUCT>()
                                on invoiceDetail.PRODUCTID equals product.ID into gj
                               from product in gj.DefaultIfEmpty()
                               orderby registerTemplate.CODE, invoice.SYMBOL, invoice.NO
                               let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                               select new ReportInvoiceDetail
                               {
                                   Id = invoice.ID,
                                   CustomerCode = client.CUSTOMERCODE,
                                   CustomerName = notCurrentClient ? (client.CUSTOMERNAME ?? client.PERSONCONTACT) : (invoice.CUSTOMERNAME ?? invoice.PERSONCONTACT),
                                   PersonContact = client.PERSONCONTACT,
                                   InvoiceStatus = (int)invoice.INVOICESTATUS,
                                   Address = notCurrentClient ? client.ADDRESS : invoice.CUSTOMERADDRESS,
                                   TaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                   Created = invoice.CREATEDDATE,
                                   InvoiceCode = registerTemplate.CODE,
                                   InvoiceSymbol = invoice.SYMBOL,
                                   InvoiceNo = invoice.NO,
                                   ProductCode = product.PRODUCTCODE,
                                   ProductName = invoiceDetail.PRODUCTNAME,
                                   Unit = product.UNIT,
                                   Quantity = (invoiceDetail.QUANTITY ?? 0),
                                   Price = (invoiceDetail.PRICE ?? 0),
                                   TotalAmount = (invoiceDetail.TOTAL ?? 0),
                                   TaxName = tax.NAME,
                                   Tax = (tax.TAX1 ?? 0),
                                   TaxId = (long)invoiceDetail.TAXID,
                                   TypePayment = (invoice.TYPEPAYMENT ?? 1),
                                   ReleasedDate = invoice.RELEASEDDATE,
                                   Note = invoice.NOTE,
                                   ProductId = invoiceDetail.PRODUCTID,
                                   Discount = (invoiceDetail.DISCOUNT ?? false),
                                   DiscountDescription = invoiceDetail.DISCOUNTDESCRIPTION,
                                   AmountTax = (invoiceDetail.AMOUNTTAX ?? 0),
                                   IsMultiTax = invoiceSample.ISMULTITAX,
                                   Currency = currency.CODE,
                                   CurrencyExchangeRate = (decimal)invoice.CURRENCYEXCHANGERATE, // add currency exchange rate
                                   IsOrg = client.ISORG ?? false,
                                   Total = invoice.TOTAL,
                                   InvoiceNo2 = invoice.INVOICENO
                               };
            List<ReportInvoiceDetail> listInvoiceInfos = new List<ReportInvoiceDetail>();
            if (condition.ColumnOrder == FirstLoad.ID)
            {
                //listInvoiceInfos = invoiceInfos.OrderByDescending(x => x.InvoiceCode).ThenByDescending(x => x.InvoiceSymbol).ThenByDescending(x => x.InvoiceNo).ThenByDescending(x => x.ReleasedDate).Skip(condition.Skip).Take(condition.Take).ToList();
                listInvoiceInfos = invoiceInfos.OrderByDescending(x => x.InvoiceNo2).ThenByDescending(x => x.ReleasedDate).Skip(condition.Skip).Take(condition.Take).ToList();
            }
            else
            {
                //listInvoiceInfos = invoiceInfos.OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
                listInvoiceInfos = invoiceInfos.OrderByDescending(x => x.InvoiceNo2).ThenByDescending(x => x.ReleasedDate).Skip(condition.Skip).Take(condition.Take).ToList();


            }
            //foreach (var item in listInvoiceInfos)
            //{
            //    if (item.CustomerName == "")
            //    {
            //        item.CustomerName = item.PersonContact;
            //    }
            //}

            return listInvoiceInfos;
        }

        public IEnumerable<ReportInvoiceDetail> FillterListInvoiceGroupBy(ConditionReportDetailUse condition)
        {
            var invoices = this.dbSet.Where(p => p.COMPANYID > 0);

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            }
            else if (!condition.Branch.HasValue)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            }

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            //add condition CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Replace("/", "").Equals(condition.Symbol));
            }

            if (condition.InvoiceStatus.Count > 0)
            {
                invoices = invoices.Where(p => condition.InvoiceStatus.Contains(p.INVOICESTATUS ?? 0));
            }

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
            }
            //add condition
            if (condition.CurrencyId.HasValue)
            {
                invoices = invoices.Where(p => p.CURRENCYID == condition.CurrencyId.Value);
            }

            var invoiceInfos = from invoice in invoices
                               join invoiceDetail in this.context.Set<INVOICEDETAIL>()
                               on invoice.ID equals invoiceDetail.INVOICEID
                               //join client in this.context.Set<CLIENT>()
                               join client in clients
                               on invoice.CLIENTID equals client.ID
                               join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                               on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                               join invoiceSample in this.context.Set<INVOICESAMPLE>()
                               on registerTemplate.INVOICESAMPLEID equals invoiceSample.ID
                               join currency in this.context.Set<CURRENCY>()
                               on invoice.CURRENCYID equals currency.ID
                               join tax in this.context.Set<TAX>()
                                on invoiceDetail.TAXID equals tax.ID
                               select new ReportInvoiceDetail
                               {
                                   Id = invoice.ID,
                                   CustomerCode = client.CUSTOMERCODE,
                                   CustomerName = invoice.CUSTOMERNAME,
                                   InvoiceStatus = (int)invoice.INVOICESTATUS,
                                   PersonContact = invoice.PERSONCONTACT,
                                   Address = invoice.CUSTOMERADDRESS,
                                   TaxCode = invoice.CUSTOMERTAXCODE,
                                   Created = invoice.CREATEDDATE,
                                   InvoiceCode = registerTemplate.CODE,
                                   InvoiceSymbol = invoice.SYMBOL,
                                   InvoiceNo = invoice.NO,
                                   TotalAmount = (invoice.TOTAL ?? 0),
                                   TaxName = tax.NAME,
                                   ReleasedDate = invoice.RELEASEDDATE,
                                   AmountTax = (invoice.TOTALTAX ?? 0),
                                   TotalDiscount = (invoice.TOTALDISCOUNT ?? 0),
                                   TotalDiscountTax = (invoice.TOTALDISCOUNTTAX ?? 0),
                                   Sum = invoice.SUM,
                                   IsMultiTax = invoiceSample.ISMULTITAX,
                                   Currency = currency.CODE, //add currency
                                   CurrencyExchangeRate = (decimal)invoice.CURRENCYEXCHANGERATE, // add currency exchange rate
                                   IsOrg = client.ISORG ?? false,
                                   Tax = (tax.TAX1 ?? 0),
                                   InvoiceNo2 = invoice.INVOICENO
                               };
            List<ReportInvoiceDetail> listInvoiceInfos = new List<ReportInvoiceDetail>();
            if (condition.ColumnOrder == FirstLoad.ID)
            {
                //listInvoiceInfos = invoiceInfos.Distinct().OrderByDescending(x => x.InvoiceCode).ThenByDescending(x => x.InvoiceSymbol).ThenByDescending(x => x.InvoiceNo).ThenByDescending(x => x.ReleasedDate).Skip(condition.Skip).Take(condition.Take).ToList();
                listInvoiceInfos = invoiceInfos.Distinct().OrderByDescending(x => x.InvoiceNo2).ThenByDescending(x => x.ReleasedDate).Skip(condition.Skip).Take(condition.Take).ToList();
            }
            else
            {
                //listInvoiceInfos = invoiceInfos.Distinct().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
                listInvoiceInfos = invoiceInfos.Distinct().OrderByDescending(x => x.InvoiceNo2).ThenByDescending(x => x.ReleasedDate).Skip(condition.Skip).Take(condition.Take).ToList();
            }
            //foreach (var item in listInvoiceInfos)
            //{
            //    if (item.CustomerName == "")
            //    {
            //        item.CustomerName = item.PersonContact;
            //    }
            //}

            return listInvoiceInfos;
        }
        public IEnumerable<ReportInvoiceDetail> FillterListInvoiceSummary(ConditionReportDetailUse condition)
        {

            var invoices = this.dbSet.Where(p => p.COMPANYID > 0 && p.INVOICESTATUS >= 4 && p.DECLAREID != null);

            //if (condition.Branch.HasValue && condition.Branch > 0)
            //{
            //    invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            //}
            //else if (!condition.Branch.HasValue)
            //{
            //    invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            //}
            //var getlist = invoices.ToList();
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            //add condition CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Replace("/", "").Equals(condition.Symbol));
            }

            //if (condition.InvoiceStatus.Count > 0)
            //{
            //    invoices = invoices.Where(p => condition.InvoiceStatus.Contains(p.INVOICESTATUS ?? 0));
            //}

            //getlist = invoices.ToList();

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE < condition.DateTo.Value);
            }
            //add condition
            if (condition.CurrencyId.HasValue)
            {
                invoices = invoices.Where(p => p.CURRENCYID == condition.CurrencyId.Value);
            }
            //getlist = invoices.ToList();
            var invoiceInfos = (from invoice in invoices
                                join invoiceDetail in this.context.Set<INVOICEDETAIL>()
                                on invoice.ID equals invoiceDetail.INVOICEID
                                join minvoicedata in this.context.Set<MINVOICE_DATA>()
                                on invoice.MESSAGECODE equals minvoicedata.MESSAGECODE into minvoicedatas
                                from minvoicedata in minvoicedatas.DefaultIfEmpty()
                                join client in clients
                                on invoice.CLIENTID equals client.ID
                                join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                                on invoice.REGISTERTEMPLATEID equals registerTemplate.ID into registerTemplates
                                from registerTemplate in registerTemplates.DefaultIfEmpty()
                                join invoiceSample in this.context.Set<INVOICESAMPLE>()
                                on registerTemplate.INVOICESAMPLEID equals invoiceSample.ID into invoiceSamples
                                from invoiceSample in invoiceSamples.DefaultIfEmpty()
                                join currency in this.context.Set<CURRENCY>()
                                on invoice.CURRENCYID equals currency.ID
                                join tax in this.context.Set<TAX>()
                                 on invoiceDetail.TAXID equals tax.ID
                                join invoiceParent in this.context.Set<INVOICE>()
                                on invoice.PARENTID equals invoiceParent.ID into invoiceParents
                                from invoiceParent in invoiceParents.DefaultIfEmpty()
                                join announcement in this.context.Set<ANNOUNCEMENT>()
                                 on invoiceParent.ID equals announcement.INVOICEID into announcements
                                from announcement in announcements.DefaultIfEmpty()
                                join announcement2 in this.context.Set<ANNOUNCEMENT>()
                                on invoice.ID equals announcement2.INVOICEID into announcement2s
                                from announcement2 in announcement2s.DefaultIfEmpty()
                                join mycompany in this.context.Set<MYCOMPANY>()
                                on invoice.COMPANYID equals mycompany.COMPANYSID
                                where mycompany.COMPANYID == condition.Branch
                                select new ReportInvoiceDetail
                                {
                                    Id = invoice.ID,
                                    CustomerCode = client.CUSTOMERCODE,
                                    CustomerName = invoice.CUSTOMERNAME,
                                    PersonContact = invoice.PERSONCONTACT,
                                    Address = invoice.CUSTOMERADDRESS,
                                    TaxCode = invoice.CUSTOMERTAXCODE,
                                    //                TaxCode = client.ISORG == true ?
                                    //invoice.CUSTOMERTAXCODE != null ?
                                    //(invoice.CUSTOMERTAXCODE.Length == 10 || invoice.CUSTOMERTAXCODE.Length == 14) ?
                                    //invoice.CUSTOMERTAXCODE.Contains("-000") ?
                                    //invoice.CUSTOMERTAXCODE.Replace("-000", "") :
                                    //!(invoice.CUSTOMERTAXCODE.Equals("88888888") || invoice.CUSTOMERTAXCODE.Equals("9999999999")) ?
                                    //invoice.CUSTOMERTAXCODE.All(c => ((c >= 48 && c <= 57 || (c == 45)))) ?
                                    //invoice.CUSTOMERTAXCODE : null : null : null : null : null,
                                    Created = invoice.CREATEDDATE,
                                    InvoiceCode = registerTemplate.CODE,
                                    InvoiceSymbol = invoice.SYMBOL,
                                    InvoiceNo = invoice.NO,
                                    InvoiceNo2 = invoice.INVOICENO,
                                    TotalAmount = (invoiceDetail.TOTAL ?? 0),
                                    TaxName = tax.NAME,
                                    ReleasedDate = invoice.RELEASEDDATE,
                                    AmountTax = (invoiceDetail.AMOUNTTAX ?? 0),
                                    TotalDiscount = (invoiceDetail.AMOUNTDISCOUNT ?? 0),
                                    TotalDiscountTax = (invoice.TOTALDISCOUNTTAX ?? 0),
                                    Sum = (invoiceDetail.TOTAL ?? 0) + (invoiceDetail.AMOUNTTAX ?? 0),
                                    IsMultiTax = invoiceSample.ISMULTITAX,
                                    Currency = currency.CODE, //add currency
                                    ProductName = invoiceDetail.PRODUCTNAME,
                                    Unit = invoiceDetail.UNITNAME,
                                    Quantity = invoiceDetail.QUANTITY,
                                    IsOrg = client.ISORG ?? false,
                                    InvoiceType = (invoice.INVOICESTATUS == 7) ? 1 : (invoice.INVOICESTATUS == 5 && invoiceParent.NO != null) ? 0 : (invoice.INVOICESTATUS == 5 && invoiceParent.NO == null) ? 1 : invoice.INVOICETYPE == null ? 0 : (invoice.INVOICETYPE == 2 || invoice.INVOICETYPE == 6) ? 2 : invoice.INVOICETYPE == 1 ? 3 : invoice.INVOICETYPE == 3 ? 4 : invoice.INVOICETYPE,
                                    ParentSymbol = invoiceParent.NO != null ? invoiceParent.SYMBOL : null,
                                    ParentNo = invoiceParent.NO != null ? invoiceParent.NO : null,
                                    ParentCode = invoiceParent.NO != null ? invoiceParent.SYMBOL.Substring(0, 1) : null,
                                    Note = invoice.INVOICETYPE != 1 ? (invoice.INVOICESTATUS == 5 || invoice.INVOICESTATUS == 7) ? announcement2.REASION : invoice.INVOICETYPE == 3 ? announcement2.REASION : announcement.REASION : null,
                                    SendCQT = (minvoicedata.STATUS == 1 && minvoicedata.MLTDIEP == "204" && minvoicedata.LTBAO == "2") ? 1 : 0,
                                    MessageCode = minvoicedata.MESSAGECODE,
                                    InvoiceDetailId = invoiceDetail.ID,
                                    CurrencyExchangeRate = (invoice.CURRENCYEXCHANGERATE ?? 1),
                                    Total = (invoice.TOTAL ?? 0),
                                    TotalTax = (invoice.TOTALTAX ?? 0),
                                    SumAmountInvoice = (invoice.SUM ?? 0),
                                    Report_class = invoice.REPORT_CLASS,
                                    CompanyId = mycompany.COMPANYID,
                                    Ltbao = minvoicedata.LTBAO,
                                    BTHERRORSTATUS = invoice.BTHERRORSTATUS,
                                    BTHERROR = invoice.BTHERROR,
                                }).ToList();

            //if (condition.Branch.HasValue && condition.Branch > 0)
            //{
            //    invoiceInfos = invoiceInfos.Where(x => x.CompanyId == condition.Branch).ToList();
            //}

            logger.Error("Invoiceinfo count : " + invoiceInfos.Count(), new Exception("Invoiceinfo count"));

            invoiceInfos = invoiceInfos.GroupBy(x => new { x.Id }).Select(group => new ReportInvoiceDetail
            {
                Id = group.FirstOrDefault().Id,
                CustomerCode = group.FirstOrDefault().CustomerCode,
                CustomerName = group.FirstOrDefault().CustomerName,
                PersonContact = group.FirstOrDefault().PersonContact,
                Address = group.FirstOrDefault().Address,
                TaxCode = group.FirstOrDefault().IsOrg == true ?
                    group.FirstOrDefault().TaxCode != null ?
                    (group.FirstOrDefault().TaxCode.Length == 10 || group.FirstOrDefault().TaxCode.Length == 14) ?
                    group.FirstOrDefault().TaxCode.Contains("-000") ?
                    group.FirstOrDefault().TaxCode.Replace("-000", "") :
                    !(group.FirstOrDefault().TaxCode.Equals("88888888") || group.FirstOrDefault().TaxCode.Equals("9999999999")) ?
                    group.FirstOrDefault().TaxCode.All(c => ((c >= 48 && c <= 57 || (c == 45)))) ?
                    group.FirstOrDefault().TaxCode : null : null : null : null : null,
                Created = group.FirstOrDefault().Created,
                InvoiceCode = group.FirstOrDefault().InvoiceCode,
                InvoiceSymbol = group.FirstOrDefault().InvoiceSymbol,
                InvoiceNo = group.FirstOrDefault().InvoiceNo,
                TotalAmount = group.Sum(x => x.TotalAmount),
                TaxName = group.FirstOrDefault().TaxName,
                ReleasedDate = group.FirstOrDefault().ReleasedDate,
                AmountTax = group.Sum(x => x.AmountTax),
                TotalDiscount = group.Sum(x => x.TotalDiscount),
                TotalDiscountTax = group.Sum(x => x.TotalDiscountTax),
                Sum = group.Sum(x => x.Sum),
                IsMultiTax = group.FirstOrDefault().IsMultiTax,
                Currency = group.FirstOrDefault().Currency, //add currency
                ProductName = group.Count() == 1 ? group.FirstOrDefault().ProductName :
                    group.FirstOrDefault().Report_class != null ? group.FirstOrDefault().Report_class.Contains("6") ? "Tiền lãi ngân hàng(interest collection)" : "Phí dịch vụ ngân hàng ( Fee commission)" : null,
                Unit = group.Count() == 1 ? group.FirstOrDefault().Unit : null,
                Quantity = group.Count() == 1 ? group.FirstOrDefault().Quantity : null,
                IsOrg = group.FirstOrDefault().IsOrg,
                InvoiceType = group.FirstOrDefault().InvoiceType,
                ParentSymbol = group.FirstOrDefault().ParentSymbol,
                ParentNo = group.FirstOrDefault().ParentNo,
                ParentCode = group.FirstOrDefault().ParentCode,
                Note = group.FirstOrDefault().Note,
                SendCQT = group.FirstOrDefault().SendCQT,
                Ltbao = group.FirstOrDefault().Ltbao,
                MessageCode = group.FirstOrDefault().MessageCode,
                InvoiceNo2 = group.FirstOrDefault().InvoiceNo2,
                InvoiceDetailId = group.FirstOrDefault().InvoiceDetailId,
                CurrencyExchangeRate = group.FirstOrDefault().CurrencyExchangeRate,
                Total = group.FirstOrDefault().Total,
                TotalTax = group.FirstOrDefault().TotalTax,
                SumAmountInvoice = group.FirstOrDefault().SumAmountInvoice,
                BTHERRORSTATUS = group.FirstOrDefault().BTHERRORSTATUS,
                BTHERROR = group.FirstOrDefault().BTHERROR
            }).ToList();

            //if (!String.IsNullOrEmpty(condition.StatusSendTVan))
            //{
            //    if (condition.StatusSendTVan.Equals("all"))
            //    {
            //        invoiceInfos = invoiceInfos.ToList();
            //    }
            //    else if (condition.StatusSendTVan.Equals("1"))
            //    {
            //        invoiceInfos = invoiceInfos.Where(x => x.SendCQT == StatusCQT.success || x.Ltbao == "4").ToList();
            //    }
            //    else
            //    {
            //        invoiceInfos = invoiceInfos.Where(x => x.SendCQT == StatusCQT.error || x.MessageCode == null || x.MessageCode == String.Empty || x.BTHERRORSTATUS == null || x.BTHERRORSTATUS != 40050).ToList();
            //    }
            //}

            if (!String.IsNullOrEmpty(condition.StatusSendTVan))
            {
                if (condition.StatusSendTVan.Equals("all"))
                {
                    invoiceInfos = invoiceInfos.ToList();
                }
                else if (condition.StatusSendTVan.Equals("1"))
                {
                    invoiceInfos = invoiceInfos.Where(x => (x.SendCQT == StatusCQT.success && x.MessageCode != null) || (x.MessageCode != null && x.Ltbao == "4" && x.BTHERROR == null && x.BTHERRORSTATUS == null)).ToList();
                }
                else
                {
                    invoiceInfos = invoiceInfos.Where(x =>
                    //(x.SendCQT != StatusCQT.success && x.Ltbao == "4" && x.BTHERROR != null && x.BTHERRORSTATUS != null) ||
                    //(x.SendCQT != StatusCQT.success && x.Ltbao != "4") ||
                    //(x.MessageCode == null) ||
                    //(String.IsNullOrEmpty(x.MessageCode)) ||
                    ////(x.MessageCode != null && x.BTHERROR == null) ||
                    //(x.SendCQT == StatusCQT.success && x.MessageCode == null)
                    (x.MessageCode == null) ||
                    (String.IsNullOrEmpty(x.MessageCode)) ||
                    ((x.MessageCode != null || !String.IsNullOrEmpty(x.MessageCode)) && 
                    (x.BTHERROR != null && !String.IsNullOrEmpty(x.BTHERROR)) && x.BTHERRORSTATUS != 40050) ||
                    ((x.MessageCode != null || !String.IsNullOrEmpty(x.MessageCode)) && x.SendCQT == 0)
                    ).ToList();

                }
            }

            var listInvoiceInfos = invoiceInfos.OrderBy(x => x.InvoiceNo2).ThenBy(x => x.ReleasedDate).ToList();


            if (condition.IsSkipTake == true)
            {
                listInvoiceInfos = listInvoiceInfos.Skip(condition.Skip).Take(condition.Take).ToList();
            }
            else
            {
                listInvoiceInfos = listInvoiceInfos.ToList();
            }

            logger.Error("Invoiceinfo count last : " + invoiceInfos.Count(), new Exception("Invoiceinfo count"));

            return listInvoiceInfos;
        }
        public IEnumerable<INVOICE> FilterInvoice(ConditionReportUse condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol));

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
            }

            return invoices;
        }

        public IEnumerable<INVOICE> FilterInvoiceReleased(ConditionReportUse condition)
        {
            IEnumerable<INVOICE> invoices;
            invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol));

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
            }

            return invoices;
        }

        public IEnumerable<INVOICE> FilterInvoiceCanceled(ConditionReportUse condition)
        {
            IEnumerable<INVOICE> invoices;

            invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol));

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
            }
            invoices = invoices.Where(p => p.RELEASEDDATE != null && p.INVOICESTATUS == (int)InvoiceStatus.Cancel);
            return invoices;
        }
        public IEnumerable<INVOICE> FilterInvoiceDeleted(ConditionReportUse condition)
        {
            IEnumerable<INVOICE> invoices;

            invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol));

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE < condition.DateTo.Value);
            }
            invoices = invoices.Where(p => p.RELEASEDDATE != null && (p.INVOICESTATUS == (int)InvoiceStatus.Delete || p.INVOICESTATUS == (int)InvoiceStatus.Cancel));
            return invoices;
        }
        public long FilterInvoiceNotRelease(ConditionReportUse condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol));
            invoices = invoices.Where(p => p.INVOICESTATUS != (int)InvoiceStatus.Cancel && p.INVOICESTATUS != (int)InvoiceStatus.Released && p.INVOICESTATUS != (int)InvoiceStatus.Delete);
            var rpCancel = from rp in this.context.Set<REPORTCANCELLINGDETAIL>()
                           where rp.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved
                           select rp.SYMBOL;
            var invoicesNotRelease = from invoice in invoices
                                     where !(rpCancel.Contains(invoice.SYMBOL))
                                     select invoice;
            return invoicesNotRelease.Count();
        }
        public long FilterInvoiceAdjustment(ConditionReportUse condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol));
            invoices = invoices.Where(p => p.INVOICETYPE == 2 || p.INVOICETYPE == 3);
            var rpCancel = from rp in this.context.Set<REPORTCANCELLINGDETAIL>()
                           where rp.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved
                           select rp.SYMBOL;
            var invoicesAdjustments = from invoice in invoices
                                      where !(rpCancel.Contains(invoice.SYMBOL))
                                      select invoice;
            return invoicesAdjustments.Count();
        }
        public long FilterInvoiceSubstitute(ConditionReportUse condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol));
            invoices = invoices.Where(p => p.INVOICETYPE == 1);
            var rpCancel = from rp in this.context.Set<REPORTCANCELLINGDETAIL>()
                           where rp.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved
                           select rp.SYMBOL;
            var invoicesSubstitutes = from invoice in invoices
                                      where !(rpCancel.Contains(invoice.SYMBOL))
                                      select invoice;
            return invoicesSubstitutes.Count();
        }
        public long GetNumberInvoiceReleased(ConditionReportUse condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol) && p.INVOICENO > 0);

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE < condition.DateTo.Value);
            }

            return invoices.Count();
        }
        public long GetNumberInvoiceReleasedSummary(ConditionReportUse condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol) && p.INVOICENO > 0);
            var rpCancel = from rp in this.context.Set<REPORTCANCELLINGDETAIL>()
                           where rp.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved
                           select rp.SYMBOL;
            var invoicesReleased = from invoice in invoices
                                   where !(rpCancel.Contains(invoice.SYMBOL))
                                   select invoice;

            return invoicesReleased.Count();
        }
        public long GetNumberInvoiceReleased(ConditionInvoiceUse condition)
        {
            //var invoices = GetInvoiceActive();
            //invoices = invoices.Where(p => p.RELEASEDDATE != null);

            //if (condition.CompanyId > 0)
            //{
            //    invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            //}

            //invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId
            //                            && p.SYMBOL.Equals(condition.Symbol));
            var invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol) && p.INVOICENO > 0);
            var rpCancel = from rp in this.context.Set<REPORTCANCELLINGDETAIL>()
                           where rp.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved
                           select rp.SYMBOL;

            var invoicesReleased = from invoice in invoices
                                   where !(rpCancel.Contains(invoice.SYMBOL))
                                   select invoice;
            return invoices.Count();
        }

        public long GetNumberInvoiceCanceled(ConditionReportUse condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol));
            if (condition.isReport)
            {
                if (condition.DateFrom.HasValue)
                {
                    invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
                }

                if (condition.DateTo.HasValue)
                {
                    invoices = invoices.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
                }
            }
            invoices = invoices.Where(p => p.RELEASEDDATE != null && p.INVOICESTATUS == (int)InvoiceStatus.Cancel);
            var rpCancel = from rp in this.context.Set<REPORTCANCELLINGDETAIL>()
                           where rp.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved
                           select rp.SYMBOL;
            var invoicesCancel = from invoice in invoices
                                 where !(rpCancel.Contains(invoice.SYMBOL))
                                 select invoice;
            return invoicesCancel.Count();
        }
        public long GetNumberInvoiceDeleted(ConditionReportUse condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol));

            invoices = invoices.Where(p => p.RELEASEDDATE != null && p.INVOICESTATUS == (int)InvoiceStatus.Cancel);
            var rpCancel = from rp in this.context.Set<REPORTCANCELLINGDETAIL>()
                           where rp.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved
                           select rp.SYMBOL;
            var invoicesCancel = from invoice in invoices
                                 where !(rpCancel.Contains(invoice.SYMBOL))
                                 select invoice;
            return invoicesCancel.Count();
        }

        public decimal CountInvoiceRelease(long companyId)
        {
            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);
            var lastDay = new DateTime(DateTime.Now.Year, 12, 31);
            var invoices = GetInvoiceActive()
                .Where(p => p.COMPANYID == companyId && p.INVOICESTATUS >= ((int)InvoiceStatus.Released) &&
                    p.CREATEDDATE > firstDay &&
                    p.CREATEDDATE < lastDay);

            return invoices.Count();
        }
        public string GetMinNo(ConditionReportUse condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol) && p.INVOICENO > 0);

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE < condition.DateTo.Value);
            }

            invoices = invoices.Where(p => p.RELEASEDDATE != null).OrderBy(p => p.NO);
            if (invoices.FirstOrDefault() != null)
            {
                return invoices.FirstOrDefault().NO;
            }
            else
            {
                return "";
            }
        }

        public string GetMaxNo(ConditionReportUse condition)
        {
            var invoices = GetInvoiceActive();

            invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.RegisterTemplateId && p.SYMBOL.Equals(condition.Symbol));

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            }

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE < condition.DateTo.Value);
            }

            invoices = invoices.Where(p => p.RELEASEDDATE != null && p.INVOICENO > 0).OrderByDescending(p => p.NO);
            if (invoices.FirstOrDefault() != null)
            {
                return invoices.FirstOrDefault().NO;
            }
            else
            {
                return "";
            }
        }

        public SendEmailRelease GetSendInvoiceInfo(long id)
        {
            var invoices = GetInvoiceActive().Where(p => p.ID == id);
            var invoicesOld = GetInvoiceActive().Where(p => (p.PARENTID ?? 0) == 0);
            var invoiceDelete = from invoice in invoicesOld
                                join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                                    on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                                join invoicetemp in this.context.Set<INVOICESAMPLE>()
                                    on registerTemplate.INVOICESAMPLEID equals invoicetemp.ID
                                select new SendEmailRelease
                                {
                                    InvoiceId = invoice.ID,
                                    ParentId = 0,
                                    InvoiceNo = string.Empty,
                                    InvoicePattern = string.Empty,
                                    InvoiceSeria = string.Empty,
                                    CustomerName = string.Empty,
                                    Email = string.Empty,
                                    Address = string.Empty,
                                    InvoiceSample = (invoice.INVOICETYPE ?? 0),
                                    InvoiceNoOld = invoice.NO,
                                    InvoicePatternOld = invoicetemp.CODE,
                                    InvoiceSeriaOld = invoice.SYMBOL
                                };

            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            var invoiceInfos = from invoice in invoices
                               join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                                   on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                               join invoicetemp in this.context.Set<INVOICESAMPLE>()
                                   on registerTemplate.INVOICESAMPLEID equals invoicetemp.ID
                               join client in clients
                               on invoice.CLIENTID equals client.ID
                               join ivOld in invoiceDelete
                               on invoice.PARENTID equals ivOld.InvoiceId into temp
                               from old in temp.DefaultIfEmpty()
                               let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                               select new SendEmailRelease
                               {
                                   InvoiceId = invoice.ID,
                                   ParentId = (invoice.PARENTID ?? 0),
                                   InvoiceNo = invoice.NO,
                                   InvoicePattern = invoicetemp.CODE,
                                   InvoiceSeria = invoice.SYMBOL,
                                   CustomerName = notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                   Email = client.USEREGISTEREMAIL == true ? client.EMAIL : client.RECEIVEDINVOICEEMAIL,
                                   Address = notCurrentClient ? client.ADDRESS : invoice.CUSTOMERADDRESS,
                                   InvoiceSample = (invoice.INVOICETYPE ?? 0),
                                   InvoiceNoOld = (old.InvoiceNo ?? string.Empty),
                                   InvoicePatternOld = (old.InvoicePatternOld ?? string.Empty),
                                   InvoiceSeriaOld = (old.InvoiceSeriaOld ?? string.Empty)
                               };


            return invoiceInfos.FirstOrDefault();
        }

        public string GetMaxInvoiceNo(long companyId, long notificationUseInvoiceDetailId, DateTime? dateTo)
        {
            var invoice = GetInvoiceActive().OrderByDescending(p => p.NO).FirstOrDefault(p => p.COMPANYID == companyId && p.CREATEDDATE <= dateTo.Value);
            if (invoice == null)
            {
                return string.Empty;
            }

            return invoice.NO;
        }

        public IQueryable<ReportListInvoices> FilterInvoice(ConditionInvoiceBoard condition, IList<int> invoiceStatus)
        {
            var invoices = GetInvoiceActive().Where(p => p.COMPANYID == condition.CompanyId && p.CREATEDDATE >= condition.DateFrom &&
                p.CREATEDDATE < condition.DateTo && invoiceStatus.Contains((p.INVOICESTATUS ?? 0)));

            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            var invoiceInfos = from invoice in invoices
                               join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>()
                                  on invoice.ID equals releaseDetail.INVOICEID
                               join release in this.context.Set<RELEASEINVOICE>()
                                   on releaseDetail.RELEASEINVOICEID equals release.ID
                               join templateCompanyUse in this.context.Set<REGISTERTEMPLATE>()
                                   on invoice.REGISTERTEMPLATEID equals templateCompanyUse.ID
                               join invoicetemp in this.context.Set<INVOICESAMPLE>()
                                   on templateCompanyUse.INVOICESAMPLEID equals invoicetemp.ID
                               join client in clients
                                    on invoice.CLIENTID equals client.ID
                               join invoiceDetail in this.context.Set<INVOICEDETAIL>()
                                   on invoice.ID equals invoiceDetail.INVOICEID
                               join tax in this.context.Set<TAX>()
                                 on invoiceDetail.TAXID equals tax.ID
                               let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                               select new ReportListInvoices
                               {
                                   Id = invoice.ID,
                                   InvoiceCode = invoicetemp.CODE,
                                   InvoiceSymbol = invoice.SYMBOL,
                                   InvoiceNo = invoice.NO,
                                   CustomerName = notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                   ReleasedDate = invoice.CREATEDDATE,
                                   TaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                   Code = tax.CODE,
                                   TaxId = invoiceDetail.TAXID,
                                   Total = invoice.TOTAL,
                                   TotalTax = invoice.TOTALTAX,
                                   InvoiceNote = invoice.NOTE,
                               };


            return invoiceInfos.OrderBy(p => (p.TaxId ?? 0)).ThenByDescending(p => p.ReleasedDate);
        }

        public IEnumerable<ReportListInvoices> FilterInvoiceGroupByTaxId(ConditionInvoiceBoard condition, IList<int> invoiceStatus, int skip = 0, int take = 0)
        {
            long? branchId = 0;

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                branchId = condition.Branch;
            }
            else if (!condition.Branch.HasValue)
            {
                branchId = condition.CompanyId;
            }

            var invoices = GetInvoiceActive().Where(p => p.COMPANYID == branchId && p.RELEASEDDATE >= condition.DateFrom &&
                p.RELEASEDDATE < condition.DateTo && p.INVOICENO != 0 && invoiceStatus.Contains((p.INVOICESTATUS ?? 0)));

            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            var invoiceDetailInfos = from invoice in invoices
                                     join templateCompanyUse in this.context.Set<REGISTERTEMPLATE>()
                                         on invoice.REGISTERTEMPLATEID equals templateCompanyUse.ID
                                     join invoicetemp in this.context.Set<INVOICESAMPLE>()
                                         on templateCompanyUse.INVOICESAMPLEID equals invoicetemp.ID
                                     join client in clients
                                         on invoice.CLIENTID equals client.ID
                                     join invoiceDetail in this.context.Set<INVOICEDETAIL>()
                                         on invoice.ID equals invoiceDetail.INVOICEID
                                     join currency in this.context.Set<CURRENCY>()
                                         on invoice.CURRENCYID equals currency.ID into currencyTemp
                                     from currency in currencyTemp.DefaultIfEmpty()
                                         //join tax in this.context.Set<TAX>()
                                         //    on invoiceDetail.TAXID equals tax.ID
                                     where (invoiceDetail.DISCOUNT != true)
                                     let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                     let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
                                     select new
                                     {
                                         InvoiceId = invoice.ID,
                                         InvoiceSymbol = invoice.SYMBOL,
                                         InvoiceNo = invoice.NO,
                                         DateRelease = invoice.RELEASEDDATE,
                                         CustomerName = isOrgCurrentClient == true ? invoice.CUSTOMERNAME : invoice.PERSONCONTACT,
                                         ClientTaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                         TaxId = invoiceDetail.TAXID,
                                         //TaxCode = tax.CODE,
                                         Price = invoiceDetail.PRICE,
                                         Total = invoiceDetail.TOTAL,
                                         TotalTax = invoiceDetail.AMOUNTTAX,
                                         Note = invoice.NOTE,
                                         RefNumber = invoice.REFNUMBER,
                                         AdjustmentType = invoiceDetail.ADJUSTMENTTYPE,
                                         CurrencyExchangeRate = (invoice.CURRENCYEXCHANGERATE ?? 1),
                                         CurrencyCode = currency.CODE,
                                         InvoiceTotalTax = invoice.TOTALTAX,
                                         InvoiceType = invoice.INVOICETYPE,
                                         ParentId = invoice.PARENTID,
                                         InvoiceNo2 = invoice.INVOICENO
                                     };

            //var detailInfos = invoiceDetailInfos.GroupBy(x => new { x.InvoiceId, x.TaxId })
            var detailInfos = invoiceDetailInfos.GroupBy(x => new { x.InvoiceId, x.TaxId })
                                .Select(g => new ReportListInvoices()
                                {
                                    Id = g.Key.InvoiceId,
                                    InvoiceSymbol = g.FirstOrDefault().InvoiceSymbol,
                                    InvoiceNo = g.FirstOrDefault().InvoiceNo,
                                    ReleasedDate = g.FirstOrDefault().DateRelease,
                                    CustomerName = g.FirstOrDefault().CustomerName,
                                    //Code = g.FirstOrDefault().TaxCode,
                                    TaxId = g.Key.TaxId,
                                    TaxCode = g.FirstOrDefault().ClientTaxCode,
                                    Total = g.Sum(x => x.Total),
                                    Price = g.Sum(x => x.Price),
                                    TotalTax = g.Sum(x => x.TotalTax),
                                    InvoiceNote = g.FirstOrDefault().Note,
                                    RefNumber = g.FirstOrDefault().RefNumber,
                                    AdjustmentType = g.FirstOrDefault().AdjustmentType,
                                    CurrencyExchangeRate = g.FirstOrDefault().CurrencyExchangeRate,
                                    CurrencyCode = g.FirstOrDefault().CurrencyCode,
                                    InvoiceTotalTax = g.FirstOrDefault().InvoiceTotalTax,
                                    InvoiceType = g.FirstOrDefault().InvoiceType,
                                    ParentId = g.FirstOrDefault().ParentId,
                                    InvoiceNo2 = g.FirstOrDefault().InvoiceNo2
                                });

            return detailInfos.OrderBy(p => (p.TaxId ?? 0)).OrderBy(x => x.InvoiceNo2).ThenByDescending(p => p.ReleasedDate);
        }

        public IEnumerable<INVOICE> GetInvoiceUsed()
        {
            var invoices = GetInvoiceActive().Where(p => p.INVOICENO > 0);
            return invoices;
        }
        public long CountInvoice(int flgInvoice = 0, long? Id = 0)
        {
            var invoices = GetInvoiceActive().Where(p => p.INVOICENO > 0);

            if (flgInvoice == 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == Id);
            }
            return invoices.Count();
        }
        public IEnumerable<INVOICE> GetInvoiceOfCompany(long companyId)
        {
            var invoices = GetInvoiceActive().Where(p => p.COMPANYID == companyId && p.INVOICENO > 0);
            return invoices;
        }

        public decimal GetNumInvoiceOfCompany(long companyId)
        {
            var invoices = GetInvoiceActive().Where(p => p.COMPANYID == companyId && p.INVOICENO > 0).Count();
            return invoices;
        }

        public bool ContainClient(long clientId)
        {
            return Contains(p => p.CLIENTID == clientId);
        }

        public bool GetInvoiceByClient(long clientId)
        {
            return GetInvoiceActive().Where(p => p.CLIENTID == clientId).Count() > 0;
        }

        public InvoiceReleaseInfo GetInvoiceReleaseInfo(long id)
        {
            INVOICE invoices = GetById(id);

            var invoiceReleaseInfo = from invoice in this.dbSet
                                     join client in this.context.Set<CLIENT>() on invoice.CLIENTID equals client.ID
                                     join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>() on invoice.ID equals releaseDetail.INVOICEID into releaseDetailTemp
                                     from releaseDetail in releaseDetailTemp.DefaultIfEmpty()
                                     join invoice2 in
                                        (from invoice3 in this.dbSet where (invoice3.DELETED ?? false) == false select invoice3)
                                        on invoice.ID equals invoice2.PARENTID into invoice2Temp
                                     from invoice2 in invoice2Temp.DefaultIfEmpty()
                                     where invoice.ID == id  //&& releaseDetail.VerificationCode != null
                                     select new InvoiceReleaseInfo()
                                     {
                                         Id = invoice.ID,
                                         VerificationCode = invoice.VERIFICATIONCODE,
                                         Converted = (invoice.CONVERTED ?? false),
                                         Email = client.USEREGISTEREMAIL != true ? client.RECEIVEDINVOICEEMAIL : client.EMAIL,
                                         Status = (invoice.INVOICESTATUS ?? 0),
                                         ChildId = invoice2 == null ? 0 : invoice2.ID,
                                     };
            if (invoices.INVOICESTATUS == (int)InvoiceStatus.Released)
            {
                if (invoiceReleaseInfo.FirstOrDefault().VerificationCode != null)
                {
                    return invoiceReleaseInfo.Where(p => p.VerificationCode != null).FirstOrDefault();
                }
                else
                {
                    return invoiceReleaseInfo.FirstOrDefault();
                }
            }
            else
            {
                return invoiceReleaseInfo.FirstOrDefault();
            }
        }

        public INVOICE ResearchInvoice(string verificationCode)
        {
            var researchInvoice = from invoice in this.dbSet
                                  join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>() on invoice.ID equals releaseDetail.INVOICEID
                                  where releaseDetail.VERIFICATIONCODE.Equals(verificationCode)
                                  select invoice;
            return researchInvoice.FirstOrDefault();
        }

        public PTInvoice PTSearchInvoice(string verificationCode)
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var researchInvoice = from invoice in this.dbSet
                                      //join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>() on invoice.ID equals releaseDetail.INVOICEID
                                  join client in this.context.Set<CLIENT>() on invoice.CLIENTID equals client.ID
                                  join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                                    on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                                  join invoiceType in this.context.Set<INVOICETYPE>() on registerTemplate.CODE.Substring(0, 6) equals invoiceType.DENOMINATOR
                                  where invoice.VERIFICATIONCODE.Equals(verificationCode)
                                  let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                  select new PTInvoice()
                                  {
                                      ID = invoice.ID,
                                      CompanyID = invoice.COMPANYID,
                                      CustomerID = null,
                                      CustomerCode = null,
                                      TaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                      CustomerName = invoice.CUSTOMERNAME,
                                      Address = null,
                                      TypeSendInvoice = null,
                                      Mobile = null,
                                      Fax = null,
                                      Email = null,
                                      Delegate = null,
                                      PersonContact = invoice.CUSTOMERNAME,
                                      BankAccount = null,
                                      AccountHolder = null,
                                      BankName = null,
                                      Description = null,
                                      CustomerType = null,
                                      Password = null,
                                      Deleted = invoice.DELETED,
                                      Created = invoice.CREATEDDATE,
                                      CreatedBy = invoice.CREATEDBY,
                                      Updated = invoice.UPDATEDDATE,
                                      UpdateBy = null,
                                      InvoiceCode = registerTemplate.CODE,
                                      Symbol = invoice.SYMBOL,
                                      InvoiceNo = invoice.NO,
                                      InvoiceSample_Name = invoiceType.NAME,
                                      ReleasedDate = invoice.RELEASEDDATE,
                                      Sum = invoice.SUM, //Cột số tiền hóa đơn ( thể hiện số tiền sau thuế ),
                                      CurrencyExchangeRate = invoice.CURRENCYEXCHANGERATE,//Currency,
                                      TaxCodeBuyer = invoice.COMPANYTAXCODE //Cột MST người bán
                                  };

            return researchInvoice.FirstOrDefault();
        }

        public SendEmailVerificationCode GetVerificationCodeInfomation(InvoiceVerificationCode invoiceVerificationCode)
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var informationEmailVerificationCode = from invoice in this.dbSet
                                                   join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>() on invoice.ID equals releaseDetail.INVOICEID into releaseDetailTemp
                                                   join client in this.context.Set<CLIENT>() on invoice.CLIENTID equals client.ID
                                                   join company in this.context.Set<MYCOMPANY>() on invoice.COMPANYID equals company.COMPANYSID
                                                   join template in this.context.Set<REGISTERTEMPLATE>() on invoice.REGISTERTEMPLATEID equals template.ID into templateTemp
                                                   join login in this.context.Set<LOGINUSER>() on client.ID equals login.CLIENTID into loginTemp
                                                   from releaseDetail in releaseDetailTemp.DefaultIfEmpty()
                                                   from template in templateTemp.DefaultIfEmpty()
                                                   from login in loginTemp.DefaultIfEmpty()
                                                   where invoice.VERIFICATIONCODE.Equals(invoiceVerificationCode.VerificationCode)// && invoice.COMPANYID == invoiceVerificationCode.CompanyId
                                                   let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                                   let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
                                                   select new SendEmailVerificationCode()
                                                   {
                                                       InvoiceId = (releaseDetail.INVOICEID ?? 0),
                                                       VerificationCode = invoice.VERIFICATIONCODE,
                                                       CompanyId = invoice.COMPANYID,
                                                       CompanyName = company.COMPANYNAME,
                                                       CompanyEmail = company.EMAIL,
                                                       CustomerName = notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                                       CustomerCode = client.CUSTOMERCODE,
                                                       PersonContact = notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT,
                                                       InvoiceCode = template.CODE,
                                                       InvoiceNo = invoice.NO,
                                                       Symbol = invoice.SYMBOL,
                                                       InvoiceDate = (invoice.RELEASEDDATE ?? invoice.CREATEDDATE ?? DateTime.Now),
                                                       Total = (invoice.TOTAL ?? 0),
                                                       TotalTax = (invoice.TOTALTAX ?? 0),
                                                       Sum = (invoice.SUM ?? 0),
                                                       ClientUser = login.USERID,
                                                       ClientPassword = login.PASSWORD,
                                                       IsOrg = isOrgCurrentClient ?? false,
                                                       TaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                                       Identity = client.IDENTITY
                                                   };
            return informationEmailVerificationCode.FirstOrDefault();
        }
        public SendEmailVerificationCode GetVerificationCodeInfomation(string VerificationCode, long companyId)
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var informationEmailVerificationCode = from invoice in this.dbSet
                                                   join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>() on invoice.ID equals releaseDetail.INVOICEID into releaseDetailTemp
                                                   join client in this.context.Set<CLIENT>() on invoice.CLIENTID equals client.ID
                                                   join company in this.context.Set<MYCOMPANY>() on invoice.COMPANYID equals company.COMPANYSID
                                                   join template in this.context.Set<REGISTERTEMPLATE>() on invoice.REGISTERTEMPLATEID equals template.ID into templateTemp
                                                   join login in this.context.Set<LOGINUSER>() on client.ID equals login.CLIENTID into loginTemp
                                                   from releaseDetail in releaseDetailTemp.DefaultIfEmpty()
                                                   from template in templateTemp.DefaultIfEmpty()
                                                   from login in loginTemp.DefaultIfEmpty()
                                                   where releaseDetail.VERIFICATIONCODE.Equals(VerificationCode)
                                                   let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                                   let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
                                                   select new SendEmailVerificationCode()
                                                   {
                                                       InvoiceId = (releaseDetail.INVOICEID ?? 0),
                                                       VerificationCode = invoice.VERIFICATIONCODE,
                                                       CompanyName = company.COMPANYNAME,
                                                       CompanyEmail = company.EMAIL,
                                                       CustomerName = notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                                       CustomerCode = client.CUSTOMERCODE,
                                                       PersonContact = notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT,
                                                       InvoiceCode = template.CODE,
                                                       InvoiceNo = invoice.NO,
                                                       Symbol = invoice.SYMBOL,
                                                       InvoiceDate = (invoice.RELEASEDDATE ?? DateTime.Now),
                                                       Total = (invoice.TOTAL ?? 0),
                                                       TotalTax = (invoice.TOTALTAX ?? 0),
                                                       Sum = (invoice.SUM ?? 0),
                                                       ClientUser = login.USERID,
                                                       ClientPassword = login.PASSWORD,
                                                       IsOrg = isOrgCurrentClient ?? false,
                                                   };
            return informationEmailVerificationCode.FirstOrDefault();
        }

        /// <summary>
        /// Láy danh sách mininvociedata
        /// </summary>
        /// <returns></returns>
        public List<MINVOICE_DATA> GetAllMINVOICE_DATA()
        {
            var obj = this.context.Set<MINVOICE_DATA>().AsNoTracking().ToList();
            return obj;
        }

        public InvoicePrintInfo GetInvoicePrintInfo(long invoiceId)
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            string configCustomerCode = GetConfig("khachle");

            var invoicePrintInfos = from invoice in this.dbSet
                                    join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>() on invoice.ID equals releaseDetail.INVOICEID into releaseDetailTemp
                                    join client in this.context.Set<CLIENT>() on invoice.CLIENTID equals client.ID into clientTemp
                                    join company in this.context.Set<MYCOMPANY>() on invoice.COMPANYID equals company.COMPANYSID into companyTemp
                                    join invoiceDetail in this.context.Set<INVOICEDETAIL>() on invoice.ID equals invoiceDetail.INVOICEID
                                    join tax in this.context.Set<TAX>() on invoiceDetail.TAXID equals tax.ID into taxTemp
                                    join registerTemplate in this.context.Set<REGISTERTEMPLATE>() on invoice.REGISTERTEMPLATEID equals registerTemplate.ID into registerTemplateTemp
                                    join typePayment in this.context.Set<TYPEPAYMENT>() on invoice.TYPEPAYMENT equals typePayment.ID into typePaymentTemp
                                    join invoiceReplace in this.context.Set<INVOICE>() on invoice.PARENTID equals invoiceReplace.ID into invoiceReplaceTemp
                                    join currency in this.context.Set<CURRENCY>() on invoice.CURRENCYID equals currency.ID into currencyTemp
                                    from invoiceReplace in invoiceReplaceTemp.DefaultIfEmpty()
                                    join registerTemplateReplace in this.context.Set<REGISTERTEMPLATE>() on invoiceReplace.REGISTERTEMPLATEID equals registerTemplateReplace.ID into registerTemplateReplaceTemp
                                    from company in companyTemp.DefaultIfEmpty()
                                    join companyAgencies in this.context.Set<MYCOMPANY>() on company.COMPANYID equals companyAgencies.COMPANYSID into companyAgenciesTemp
                                    from registerTemplateReplace in registerTemplateReplaceTemp.DefaultIfEmpty()
                                    from tax in taxTemp.DefaultIfEmpty()
                                    from registerTemplate in registerTemplateTemp.DefaultIfEmpty()
                                    join invoiceTemplate in this.context.Set<INVOICETEMPLATE>() on registerTemplate.INVOICETEMPLATEID equals invoiceTemplate.ID into invoiceTemplateTemp
                                    from invoiceTemplate in invoiceTemplateTemp.DefaultIfEmpty()
                                    from client in clientTemp.DefaultIfEmpty()
                                    from typePayment in typePaymentTemp.DefaultIfEmpty()
                                    from releaseDetail in releaseDetailTemp.DefaultIfEmpty()
                                    from companyAgencies in companyAgenciesTemp.DefaultIfEmpty()
                                    from currency in currencyTemp.DefaultIfEmpty()
                                    join announ in this.context.Set<ANNOUNCEMENT>().Where(p => p.ANNOUNCEMENTSTATUS >= 3 && (p.ANNOUNCEMENTTYPE == 2 || p.ANNOUNCEMENTTYPE == 3))
                                         on invoice.ID equals announ.INVOICEID into announs
                                    from announ in announs.DefaultIfEmpty()
                                    join invoice2 in this.dbSet.Where(p => !(p.DELETED ?? false))
                                     on invoice.ID equals invoice2.PARENTID into invoice2Temp
                                    from invoice2 in invoice2Temp.DefaultIfEmpty()
                                    join minvoice in this.context.Set<MINVOICE_DATA>() on invoice.MESSAGECODE equals minvoice.MESSAGECODE into minvoices
                                    from minvoice in minvoices.DefaultIfEmpty()
                                    join declaration in this.context.Set<INVOICEDECLARATION>() on invoice.DECLAREID equals declaration.ID into declarations
                                    from declaration in declarations.DefaultIfEmpty()
                                    join mycompanyupgrade in this.context.Set<MYCOMPANYUPGRADE>() on company.COMPANYSID equals mycompanyupgrade.COMPANYSID into mycompanyupgradetemp
                                    from mycompanyupgrade in mycompanyupgradetemp.DefaultIfEmpty()
                                    join mycompanyinfoupgrade in this.context.Set<MYCOMPANY>() on mycompanyupgrade.COMPANYID equals mycompanyinfoupgrade.COMPANYSID into mycompanyinfoupgrades
                                    from mycompanyinfoupgrade in mycompanyinfoupgrades.DefaultIfEmpty()
                                    where invoice.ID == invoiceId
                                    let invoiceStatus = invoice.INVOICESTATUS != (int)InvoiceStatus.New || (invoice.INVOICETYPE ?? 0) != 0 || (invoice.INVOICENO ?? 0) != 0
                                    let levelCustomer = company.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice
                                    select new InvoicePrintInfo()
                                    {
                                        CompanyId = invoice.COMPANYID,
                                        Id = invoice.ID,
                                        Total = invoice.TOTAL,
                                        Tax = tax.NAME,
                                        TaxDisplay = tax.DISPLAYINVOICE,
                                        TaxAmout = invoice.TOTALTAX,
                                        Sum = invoice.SUM,
                                        DataInvoice = invoice.RELEASEDDATE,
                                        DateRelease = invoice.RELEASEDDATE,
                                        InvoiceNo = invoice.NO,
                                        CompanyName = levelCustomer ? mycompanyinfoupgrade.COMPANYNAME != null ? mycompanyinfoupgrade.COMPANYNAME : companyAgencies.COMPANYNAME : company.COMPANYNAME,
                                        TaxCodeInvoice = invoice.COMPANYTAXCODE,
                                        TaxCodeMyCompany = levelCustomer ? companyAgencies.COMPANYNAME : company.TAXCODE,
                                        Address = levelCustomer ? mycompanyinfoupgrade.ADDRESS != null ? mycompanyinfoupgrade.ADDRESS : companyAgencies.ADDRESS : company.ADDRESS,
                                        Tel = company.TEL1,
                                        Fax = company.FAX,
                                        Symbol = invoice.SYMBOL,
                                        Mobile = company.MOBILE,
                                        EmailContract = company.EMAILOFCONTRACT,
                                        Email = company.EMAIL,
                                        BankAccount = company.BANKACCOUNT,
                                        BankName = company.BANKNAME,
                                        Website = company.WEBSITE,
                                        InvoiceCode = registerTemplate.CODE,
                                        TypePayment = typePayment.NAME,
                                        TypePaymentCode = typePayment.CODE,
                                        CustomerNameInvoice = invoice.PERSONCONTACT,
                                        CustomerNameClient = invoice.PERSONCONTACT != null ? client.PERSONCONTACT : invoice.PERSONCONTACT,
                                        CustomerCode = client.CUSTOMERCODE,
                                        CustomerCompanyNameInvoice = invoice.CUSTOMERNAME,
                                        CustomerCompanyNameClient = invoice.CUSTOMERNAME != null ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                        CustomerTaxCodeInvoice = invoice.CUSTOMERTAXCODE,
                                        CustomerTaxCodeClient = invoice.CUSTOMERTAXCODE != null ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                        CustomerAddressInvoice = invoice.CUSTOMERADDRESS,
                                        CustomerAddressClient = invoice.CUSTOMERADDRESS != null ? client.ADDRESS : invoice.CUSTOMERADDRESS,
                                        CustomerBankAccountInvoice = invoice.CUSTOMERBANKACC,
                                        CustomerBankAccountClient = invoice.CUSTOMERBANKACC != null ? client.BANKACCOUNT : invoice.CUSTOMERBANKACC,
                                        TemplateId = (registerTemplate.INVOICETEMPLATEID ?? 0),// lấy ID của table invoiceTemplate
                                        RegisterTemplateId = registerTemplate.ID,// lấy ID của table registerTemplate
                                        InvoiceSample = (invoice.INVOICETYPE ?? 0),
                                        ReplacedInvoiceCode = registerTemplateReplace.CODE,
                                        ReplacedInvoiceNo = invoiceReplace.NO,
                                        ReplacedSymbol = invoiceReplace.SYMBOL,
                                        ReplacedReleaseDate = invoiceReplace.RELEASEDDATE ?? DateTime.Now,
                                        Invoice_Status = (invoice.INVOICESTATUS ?? 0),
                                        ReportWebsite = companyAgencies.REPORTWEBSITE,
                                        ReportTel = companyAgencies.REPORTTEL,
                                        DiscountAmount = invoice.TOTALDISCOUNT,
                                        TotalDiscountTax = invoice.TOTALDISCOUNTTAX,
                                        ClientSign = (invoice.CLIENTSIGN ?? false),
                                        DateClientSign = invoice.CLIENTSIGNDATE,
                                        InvoiceSampleId = registerTemplate.INVOICESAMPLEID,
                                        IsDiscount = invoice.ISDISCOUNT,
                                        ParentId = (invoice.PARENTID ?? 0),
                                        EmailClient = client.EMAIL,
                                        MobileClient = client.MOBILE,
                                        ClientSendByMonth = client.SENDINVOICEBYMONTH,
                                        CurrenCyName = currency.NAME,
                                        CurrencyCode = currency.CODE,
                                        CurrencyDecimalUnit = currency.DECIMALUNIT,
                                        CurrencyDecimalSeparator = currency.DECIMALSEPARATOR,
                                        ExchangeRate = invoice.CURRENCYEXCHANGERATE,
                                        IsOrg = client.ISORG ?? false,
                                        ClientId = client.ID,
                                        TemplateName = invoiceTemplate.URLFILE,
                                        CanCreateAnnoun = announ == null,
                                        RefNumber = invoice.REFNUMBER ?? "",

                                        LevelCustomer = company.LEVELCUSTOMER,
                                        ParentCompanyName = companyAgencies.COMPANYNAME,
                                        ParentCompanyAddress = companyAgencies.ADDRESS,
                                        VerificationCode = invoice.VERIFICATIONCODE,
                                        HaveAdjustment = invoice2.PARENTID != null && invoice2.INVOICETYPE != (int)InvoiceType.Substitute,
                                        IsChange = invoice2.INVOICETYPE != null,
                                        HTHDon = invoice.HTHDON,
                                        MCQT = minvoice.MCQT,
                                        StatusNotification = minvoice.STATUS,
                                        ReplacedInvoiceID = invoiceReplace.ID,
                                        Report_class = invoice.REPORT_CLASS,
                                        Vat_invoice_type = invoice.VAT_INVOICE_TYPE,
                                        InvoiceType = invoice.INVOICETYPE,
                                        Signature = levelCustomer ? companyAgencies.COMPANYNAME : company.COMPANYNAME,
                                        TaxCodeName = tax.CODE,
                                        ImportType = invoice.IMPORTTYPE
                                    };

            Func<InvoicePrintInfo, string, string, string> getParams = (invPrint, value1, value2) =>
            {
                if ((invPrint.Invoice_Status != (int)InvoiceStatus.New && invPrint.Invoice_Status != 0 || invPrint.InvoiceNo != DefaultFields.INVOICE_NO_DEFAULT_VALUE)
                    || invPrint.InvoiceSample != 0)
                    return value1;
                return value2;
            };
            var invoicePrintInfo = invoicePrintInfos.FirstOrDefault();
            invoicePrintInfo.TaxCode = getParams(invoicePrintInfo, invoicePrintInfo.TaxCodeInvoice, invoicePrintInfo.TaxCodeMyCompany);
            invoicePrintInfo.CustomerName = invoicePrintInfo.CustomerNameInvoice;//getParams(invoicePrintInfo, invoicePrintInfo.CustomerNameInvoice, invoicePrintInfo.CustomerNameClient);
            invoicePrintInfo.CustomerCompanyName = getParams(invoicePrintInfo, invoicePrintInfo.CustomerCompanyNameInvoice, invoicePrintInfo.CustomerCompanyNameClient);
            invoicePrintInfo.CustomerTaxCode = getParams(invoicePrintInfo, invoicePrintInfo.CustomerTaxCodeInvoice, invoicePrintInfo.CustomerTaxCodeClient);
            invoicePrintInfo.CustomerAddress = getParams(invoicePrintInfo, invoicePrintInfo.CustomerAddressInvoice, invoicePrintInfo.CustomerAddressClient);
            invoicePrintInfo.CustomerBankAccount = getParams(invoicePrintInfo, invoicePrintInfo.CustomerBankAccountInvoice, invoicePrintInfo.CustomerBankAccountClient);

            //this.SubGetInvoicePrintInfo(invoicePrintInfo);

            return invoicePrintInfo;
        }
        public void SubGetInvoicePrintInfo(InvoicePrintInfo invoicePrintInfo)
        {
            var signDetailClient = this._dbSet.SIGNDETAILs.Where(x => invoicePrintInfo.ClientSign && x.INVOICEID == invoicePrintInfo.Id && x.ISCLIENTSIGN == true && x.TYPESIGN == (int)SignDetailTypeSign.Invoice)
                       .OrderByDescending(x => x.CREATEDDATE)
                       .FirstOrDefault();
            invoicePrintInfo.ClientSignDate = invoicePrintInfo.DateClientSign.HasValue ? invoicePrintInfo.DateClientSign.Value.ToString("dd/MM/yyy") : String.Empty;
            invoicePrintInfo.ClientSignature = signDetailClient?.NAME;

            var signDetail = this._dbSet.SIGNDETAILs.Where(x => invoicePrintInfo.DateRelease.HasValue && x.INVOICEID == invoicePrintInfo.Id && !(x.ISCLIENTSIGN ?? false) && x.TYPESIGN == (int)SignDetailTypeSign.Invoice)
                    .OrderByDescending(x => x.CREATEDDATE)
                    .FirstOrDefault();
            invoicePrintInfo.DateSign = invoicePrintInfo.DateRelease.HasValue ? invoicePrintInfo.DateRelease.Value.ToString("dd/MM/yyy") : String.Empty;
            invoicePrintInfo.Signature = signDetail?.NAME;
        }

        public IEnumerable<DateTime> DateOfInvoiceCanUse(InvoiceDateInfo invoiceDateInfo)
        {
            List<DateTime> dateOfInvoiceCanUse = new List<DateTime>();
            decimal invoiceNo = decimal.Parse(invoiceDateInfo.InvoiceNo);
            DateTime? maxDate = this.dbSet.Where(p => p.COMPANYID == invoiceDateInfo.CompanyId && p.INVOICENO < invoiceNo &&
                  p.REGISTERTEMPLATEID == invoiceDateInfo.RegisterTemplateId
                  && p.SYMBOL.Replace("/", "").Equals(invoiceDateInfo.Symbol.Replace("/", ""))).Max(i => i.CREATEDDATE);
            dateOfInvoiceCanUse.Add(maxDate ?? DateTime.MinValue);

            DateTime? minDate = this.dbSet.Where(p => p.COMPANYID == invoiceDateInfo.CompanyId && p.INVOICENO > invoiceNo &&
                p.REGISTERTEMPLATEID == invoiceDateInfo.RegisterTemplateId
                && p.SYMBOL.Replace("/", "").Equals(invoiceDateInfo.Symbol.Replace("/", ""))).Min(i => i.CREATEDDATE);
            dateOfInvoiceCanUse.Add(minDate ?? DateTime.MaxValue);
            return dateOfInvoiceCanUse;
        }

        public INVOICE GetMaxInvoice(long companyId, long registerId, string symbol, DateTime? dateFrom)
        {
            var maxInvoice = this.dbSet.Where(p => (p.COMPANYID == companyId)
                    && p.REGISTERTEMPLATEID == registerId
                    && p.SYMBOL.Equals(symbol)
                    && p.RELEASEDDATE < dateFrom)
                .OrderByDescending(p => p.NO)
                .FirstOrDefault();

            return maxInvoice;
        }

        public IEnumerable<InvoiceDebts> GetOpenningDebts(ConditionSearchInvoiceDebts condition)
        {
            var invoices = this.dbSet.Where(p => p.COMPANYID == condition.CompanyId && p.INVOICESTATUS == (int)InvoiceStatus.Released);
            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.CREATEDDATE <= condition.DateFrom);
            }

            var invoiceBalance = from invoice in invoices
                                 select new InvoiceDebts()
                                 {
                                     ClientId = invoice.CLIENTID,
                                     Summary = (int)InvoiceType.AdjustmentInfomation == (invoice.INVOICETYPE ?? 0) ? (invoice.SUM ?? 0) * -1 : (invoice.SUM ?? 0),
                                 };

            var invoiceOpenningDebts = from invoice in invoiceBalance
                                       group invoice by new { invoice.ClientId } into g
                                       select new InvoiceDebts()
                                       {
                                           ClientId = g.Key.ClientId,
                                           Summary = g.Sum(i => i.Summary),
                                       };

            return invoiceOpenningDebts;

        }

        public IEnumerable<InvoiceDebts> GetHappeningDebts(ConditionSearchInvoiceDebts condition)
        {
            var invoices = this.dbSet.Where(p => p.COMPANYID == condition.CompanyId && p.INVOICESTATUS == (int)InvoiceStatus.Released);
            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.CREATEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.CREATEDDATE <= condition.DateTo.Value);
            }

            var invoiceBalance = from invoice in invoices
                                 select new InvoiceDebts()
                                 {
                                     ClientId = invoice.CLIENTID,
                                     Summary = (int)InvoiceType.AdjustmentInfomation == (invoice.INVOICETYPE ?? 0) ? (invoice.SUM ?? 0) * -1 : (invoice.SUM ?? 0),
                                 };

            var invoiceOpenningDebts = from invoice in invoiceBalance
                                       group invoice by new { invoice.ClientId } into g
                                       select new InvoiceDebts()
                                       {
                                           ClientId = g.Key.ClientId,
                                           Summary = g.Sum(i => i.Summary),
                                       };

            return invoiceOpenningDebts;
        }

        public IEnumerable<InvoiceMaster> FilterInvoiceOfClient(ConditionSearchInvoiceOfClient condition)
        {
            var invoices = GetInvoiceActive().Where(p => p.CLIENTID == condition.ClientId && condition.Status.Contains(p.INVOICESTATUS ?? 0));
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false) && p.ID == condition.ClientId);
            if (condition.Status.Count > 0)
            {
                invoices = invoices.Where(p => condition.Status.Contains(p.INVOICESTATUS ?? 0));
            }

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.CREATEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.CREATEDDATE <= condition.DateTo.Value);
            }

            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var invoiceInfos = from invoice in invoices
                               join templateCompanyuse in this.context.Set<REGISTERTEMPLATE>()
                                   on invoice.REGISTERTEMPLATEID equals templateCompanyuse.ID
                               join invoiceSample in this.context.Set<INVOICESAMPLE>()
                                    on templateCompanyuse.INVOICESAMPLEID equals invoiceSample.ID into invoiceSampleTemp
                               from invoiceSample in invoiceSampleTemp.DefaultIfEmpty()
                               join client in clients
                               on invoice.CLIENTID equals client.ID
                               let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                               select new InvoiceMaster
                               {
                                   Id = invoice.ID,
                                   InvoiceCode = templateCompanyuse.CODE,
                                   InvoiceName = invoiceSample.NAMEREPORT,
                                   InvoiceSymbol = invoice.SYMBOL,
                                   InvoiceNo = invoice.INVOICENO ?? 0,
                                   No = invoice.NO,
                                   CustomerName = notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                   InvoiceDate = invoice.CREATEDDATE,
                                   InvoiceStatus = invoice.INVOICESTATUS,
                                   NumberAccount = invoice.NUMBERACCOUT,
                                   InvoiceNote = invoice.NOTE,
                                   TaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                   Released = invoice.RELEASED,
                                   TotalAmount = (invoice.SUM ?? 0),
                                   IsClientSign = (invoice.CLIENTSIGN ?? false),
                               };

            return invoiceInfos;
        }

        public InvoiceClientSign GetInvoiceOfClient(long invoiceId)
        {
            var invoiceOfClient = from invoice in this.dbSet
                                  join registerTempalate in this.context.Set<REGISTERTEMPLATE>()
                                  on invoice.REGISTERTEMPLATEID equals registerTempalate.ID
                                  join tempalate in this.context.Set<INVOICETEMPLATE>()
                                  on registerTempalate.INVOICETEMPLATEID equals tempalate.ID
                                  where invoice.ID == invoiceId
                                  select new InvoiceClientSign()
                                  {
                                      Id = invoice.ID,
                                      Symbol = invoice.SYMBOL,
                                      No = invoice.NO,
                                      Created = (invoice.CREATEDDATE ?? DateTime.Now),
                                      DateReleaseClient = DateTime.Now,
                                      LocationSignLeft = (double)(tempalate.LOCATIONSIGNRIGHT ?? 0),
                                      LocationSignButton = (double)(tempalate.LOCATIONSIGNBUTTON),
                                      ClientSign = (invoice.CLIENTSIGN ?? false),

                                  };
            return invoiceOfClient.FirstOrDefault();
        }

        public bool ContainProduct(long productId)
        {
            return context.Set<INVOICEDETAIL>().Where(p => p.PRODUCTID == productId && dbSet.Where(c => !(c.DELETED ?? false)).Select(c => c.ID).Contains(p.INVOICEID ?? 0)).Count() > 0;
        }

        public DateTime? MaxReleaseDate(InvoiceInfo invoiceInfo)
        {
            var query = this.dbSet.Where(i => i.COMPANYID == invoiceInfo.CompanyId);
            if (!string.IsNullOrEmpty(invoiceInfo.InvoiceCode))
            {
                query = query.Where(i => i.REGISTERTEMPLATE.CODE == invoiceInfo.InvoiceCode);
            }

            if (!string.IsNullOrEmpty(invoiceInfo.Symbol))
            {
                query = query.Where(i => i.SYMBOL == invoiceInfo.Symbol);
            }

            var res = query.Max(i => i.RELEASEDDATE);
            return res;
        }

        public string MinInvoiceNo(InvoiceInfo invoiceInfo)
        {
            var query = this.dbSet.Where(i => i.COMPANYID == invoiceInfo.CompanyId);
            if (!string.IsNullOrEmpty(invoiceInfo.InvoiceCode))
            {
                query = query.Where(i => i.REGISTERTEMPLATE.CODE == invoiceInfo.InvoiceCode);
            }

            if (!string.IsNullOrEmpty(invoiceInfo.Symbol))
            {
                query = query.Where(i => i.SYMBOL == invoiceInfo.Symbol);
            }

            var res = query.Max(i => i.NO);
            return res;
        }
        public IEnumerable<INVOICE> GetListInvoiceByClient(long ClientId)
        {
            var DateSend = DateTime.Today.AddMonths(-1).AddDays(1);
            var today = DateTime.Today.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);// lay toi 12:PM
            var result = this.dbSet.Where(p => p.CLIENTID == ClientId &&
                        p.INVOICESTATUS == (int)InvoiceStatus.Released &&
                        p.RELEASEDDATE >= DateSend && p.RELEASEDDATE <= today &&
                        p.SENDMAILSTATUS != true)
                    .OrderBy(p => p.RELEASEDDATE);
            return result;
        }
        public INVOICE GetInvoiceByRefNumber(string RefNumber)
        {
            return dbSet.FirstOrDefault(p => p.REFNUMBER == RefNumber && (p.INVOICESTATUS == (int)InvoiceStatus.Approved || p.INVOICESTATUS == (int)InvoiceStatus.Released) && !(p.DELETED ?? false));
        }

        public INVOICE GetInvoiceForAdjustmentMonthly(string CIF_NO, string yearmonth, long? CURRENCYID, long? COMPANYID, string VAT_RATE, string REPORT_CLASS)
        {
            return dbSet.FirstOrDefault(p => p.REPORT_CLASS == REPORT_CLASS && (p.INVOICESTATUS == (int)InvoiceStatus.Approved || p.INVOICESTATUS == (int)InvoiceStatus.Released) && !(p.DELETED ?? false));
        }

        public IQueryable<INVOICE> GetListInvoiceApproveNo()
        {
            return GetInvoiceActive().Where(p => p.INVOICESTATUS == (int)InvoiceStatus.Approved && !(p.DELETED ?? false));
        }
        public IEnumerable<INVOICE> GetListInvoiceSendMail()
        {

            var invoices = from invoice in this.dbSet
                           join client in this.context.Set<CLIENT>() on invoice.CLIENTID equals client.ID
                           where (client.USEREGISTEREMAIL == true ? client.EMAIL != null : client.RECEIVEDINVOICEEMAIL != null)
                           && invoice.INVOICESTATUS == (int)InvoiceStatus.Released && invoice.SENDMAILSTATUS != true
                           select invoice;
            return invoices;
        }
        public IEnumerable<ReleaseInvoiceInfo> GetListInvoiceNeedDelete()
        {
            var invoices = from invoice in this.dbSet
                           join ivDetail in this.context.Set<INVOICEDETAIL>() on invoice.ID equals ivDetail.INVOICEID
                           where ivDetail.ADJUSTMENTTYPE == -99
                           select (new ReleaseInvoiceInfo()
                           {
                               InvoiceId = invoice.ID,
                               InvoiceNo = invoice.NO,
                               InvoiceDetailId = ivDetail.ID
                           });
            return invoices;
        }
        public IQueryable<INVOICE> GetAllQueryable()
        {
            return this.dbSet;
        }

        public IEnumerable<InvoiceMaster> FilterSubstitute(ConditionSearchInvoice condition)
        {
            IQueryable<InvoiceMaster> invoiceInfos = FilterSubstituteQueryable(condition);

            //add sort theo dk
            invoiceInfos = invoiceInfos.OrderByDescending(p => p.Updated);
            if (condition.InvoiceStatus.Count == 0 || condition.ColumnOrder != "Id")
            {
                invoiceInfos = invoiceInfos.OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc));
            }

            var res = invoiceInfos.Skip(condition.Skip).Take(condition.Take);
            return res.ToList();
        }

        public long CountSubstitute(ConditionSearchInvoice condition)
        {
            IQueryable<InvoiceMaster> invoiceInfos = FilterSubstituteQueryable(condition);

            var res = invoiceInfos.Count();
            return res;
        }

        private IQueryable<InvoiceMaster> FilterSubstituteQueryable(ConditionSearchInvoice condition)
        {
            IQueryable<InvoiceMaster> invoiceInfos = FilterInvoiceList(condition).Where(x => x.ChildId == 0 && x.statusCQT != 1);
            return invoiceInfos;
        }

        public InvoiceInfo GetByInvoiceNo(ConditionSearchInvoice condition)
        {
            var invoiceNo = int.Parse(condition.InvoiceNo);
            var invoices = GetInvoiceActive();
            var clientDraft = this.context.Set<CLIENT>().Where(p => p.CUSTOMERCODE == CustomerDraft.CustomerDraftString).Select(p => p.ID);
            var invoiceInfoQuery = from invoice in invoices
                                   where invoice.COMPANYID == condition.CurrentUser.Company.Id
                                         && invoice.REGISTERTEMPLATEID == condition.InvoiceSampleId
                                         && invoice.SYMBOL.Trim() == condition.Symbol.Trim()
                                         && invoice.INVOICENO != 0
                                         && invoice.INVOICENO == invoiceNo
                                   select new
                                   {
                                       invoice.ID,
                                       invoice.INVOICESTATUS,
                                       invoice.COMPANYID,
                                       invoice.RELEASEDDATE,
                                       invoice.CLIENTID,
                                       invoice.NO
                                   };

            var invoiceInfo = invoiceInfoQuery.FirstOrDefault();
            if (invoiceInfo != null && invoiceInfo.INVOICESTATUS != (int)InvoiceStatus.New)
            {
                throw new BusinessLogicException(ResultCode.InvoiceNotInNewStatus, "Hóa đơn này không còn ở trạng thái tạo mới, không thể dùng để thay thế.");
            }
            return invoiceInfo != null
                ? new InvoiceInfo() { Id = invoiceInfo.ID, CompanyId = invoiceInfo.COMPANYID, ReleasedDate = invoiceInfo.RELEASEDDATE, No = invoiceInfo.NO }
                : new InvoiceInfo() { Id = 0, CompanyId = 0 };
        }

        public bool CheckInvoiceDeleted(List<long> ids)
        {
            var invoiceDeleted = from invoice in this.dbSet
                                 where invoice.INVOICESTATUS == (int)InvoiceStatus.Delete && ids.Contains(invoice.ID)
                                 select invoice;

            return invoiceDeleted.Count() > 0;
        }

        public bool CheckSendMailStatus(long id)
        {
            var invoiceActive = this.GetInvoiceActive();
            var emailActiveQuery = from emailActive in this.context.Set<EMAILACTIVE>()
                                   join emailReference in this.context.Set<EMAILACTIVEREFERENCE>() on emailActive.ID equals emailReference.EMAILACTIVEID
                                   join invoice in invoiceActive on emailReference.REFID equals invoice.ID
                                   where invoice.ID == id
                                       && invoice.SENDMAILSTATUS == true
                                       && emailActive.SENDSTATUS == (int)StatusSendEmail.Successfull
                                   select emailActive;
            if (emailActiveQuery.Any())
            {
                return false;
            }
            return true;
        }

        public IQueryable<INVOICE> GetInvoiceNotRelease(DateTime invoiceRelease)
        {
            List<int?> listinvoiceStatus = new List<int?>()
            {
                        (int?)InvoiceStatus.Released,
                        (int?)InvoiceStatus.Cancel,
                        (int?)InvoiceStatus.Delete
            };
            var result = this.dbSet.Where(p => p.RELEASEDDATE.HasValue && p.RELEASEDDATE < invoiceRelease && !listinvoiceStatus.Contains(p.INVOICESTATUS) && p.DELETED == null);
            if (result.Count() > 0)
            {
                return result;
            }
            return this.dbSet.Where(p => p.RELEASEDDATE > invoiceRelease && p.INVOICESTATUS == 4 && p.DELETED == null);
        }
        public IQueryable<INVOICE> GetInvoiceNotReleaseForApprove(DateTime invoiceRelease)
        {
            var result = this.dbSet.Where(p => p.RELEASEDDATE < invoiceRelease && p.INVOICENO == 0 && p.DELETED == null);
            if (result.Count() > 0)
            {
                return result;
            }
            return this.dbSet.Where(p => p.RELEASEDDATE > invoiceRelease && p.INVOICENO > 0 && p.DELETED == null);
        }

        public IQueryable<ReportInvoiceDetail> FillterListInvoiceSummaryViewReport(ConditionReportDetailUse condition)
        {
            var invoices = this.dbSet.Where(p => p.COMPANYID > 0 && p.INVOICESTATUS >= 4 && p.DECLAREID != null);

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            //add condition CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Replace("/", "").Equals(condition.Symbol));
            }

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE < condition.DateTo.Value);
            }
            //add condition
            if (condition.CurrencyId.HasValue)
            {
                invoices = invoices.Where(p => p.CURRENCYID == condition.CurrencyId.Value);
            }
            //getlist = invoices.ToList();
            var invoiceInfos = (from invoice in invoices
                                join invoiceDetail in this.context.Set<INVOICEDETAIL>()
                                on invoice.ID equals invoiceDetail.INVOICEID
                                join minvoicedata in this.context.Set<MINVOICE_DATA>()
                                on invoice.MESSAGECODE equals minvoicedata.MESSAGECODE into minvoicedatas
                                from minvoicedata in minvoicedatas.DefaultIfEmpty()
                                join client in clients
                                on invoice.CLIENTID equals client.ID
                                join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                                on invoice.REGISTERTEMPLATEID equals registerTemplate.ID into registerTemplates
                                from registerTemplate in registerTemplates.DefaultIfEmpty()
                                join invoiceSample in this.context.Set<INVOICESAMPLE>()
                                on registerTemplate.INVOICESAMPLEID equals invoiceSample.ID into invoiceSamples
                                from invoiceSample in invoiceSamples.DefaultIfEmpty()
                                join currency in this.context.Set<CURRENCY>()
                                on invoice.CURRENCYID equals currency.ID
                                join tax in this.context.Set<TAX>()
                                 on invoiceDetail.TAXID equals tax.ID
                                join invoiceParent in this.context.Set<INVOICE>()
                                on invoice.PARENTID equals invoiceParent.ID into invoiceParents
                                from invoiceParent in invoiceParents.DefaultIfEmpty()
                                join announcement in this.context.Set<ANNOUNCEMENT>()
                                 on invoiceParent.ID equals announcement.INVOICEID into announcements
                                from announcement in announcements.DefaultIfEmpty()
                                join announcement2 in this.context.Set<ANNOUNCEMENT>()
                                on invoice.ID equals announcement2.INVOICEID into announcement2s
                                from announcement2 in announcement2s.DefaultIfEmpty()
                                join mycompany in this.context.Set<MYCOMPANY>()
                                on invoice.COMPANYID equals mycompany.COMPANYSID
                                where mycompany.COMPANYID == condition.Branch
                                select new ReportInvoiceDetail
                                {
                                    Id = invoice.ID,
                                    CustomerCode = client.CUSTOMERCODE,
                                    CustomerName = invoice.CUSTOMERNAME,
                                    PersonContact = invoice.PERSONCONTACT,
                                    Address = invoice.CUSTOMERADDRESS,
                                    TaxCode = invoice.CUSTOMERTAXCODE,
                                    Created = invoice.CREATEDDATE,
                                    InvoiceCode = registerTemplate.CODE,
                                    InvoiceSymbol = invoice.SYMBOL,
                                    InvoiceNo = invoice.NO,
                                    InvoiceNo2 = invoice.INVOICENO,
                                    TotalAmount = (invoiceDetail.TOTAL ?? 0),
                                    TaxName = tax.NAME,
                                    ReleasedDate = invoice.RELEASEDDATE,
                                    AmountTax = (invoiceDetail.AMOUNTTAX ?? 0),
                                    TotalDiscount = (invoiceDetail.AMOUNTDISCOUNT ?? 0),
                                    TotalDiscountTax = (invoice.TOTALDISCOUNTTAX ?? 0),
                                    Sum = (invoiceDetail.TOTAL ?? 0) + (invoiceDetail.AMOUNTTAX ?? 0),
                                    IsMultiTax = invoiceSample.ISMULTITAX,
                                    Currency = currency.CODE, //add currency
                                    ProductName = invoiceDetail.PRODUCTNAME,
                                    Unit = invoiceDetail.UNITNAME,
                                    Quantity = invoiceDetail.QUANTITY,
                                    IsOrg = client.ISORG ?? false,
                                    InvoiceType = (invoice.INVOICESTATUS == 7) ? 1 : (invoice.INVOICESTATUS == 5 && invoiceParent.NO != null) ? 0 : (invoice.INVOICESTATUS == 5 && invoiceParent.NO == null) ? 1 : invoice.INVOICETYPE == null ? 0 : (invoice.INVOICETYPE == 2 || invoice.INVOICETYPE == 6) ? 2 : invoice.INVOICETYPE == 1 ? 3 : invoice.INVOICETYPE == 3 ? 4 : invoice.INVOICETYPE,
                                    ParentSymbol = invoiceParent.NO != null ? invoiceParent.SYMBOL : null,
                                    ParentNo = invoiceParent.NO != null ? invoiceParent.NO : null,
                                    ParentCode = invoiceParent.NO != null ? invoiceParent.SYMBOL.Substring(0, 1) : null,
                                    Note = invoice.INVOICETYPE != 1 ? (invoice.INVOICESTATUS == 5 || invoice.INVOICESTATUS == 7) ? announcement2.REASION : invoice.INVOICETYPE == 3 ? announcement2.REASION : announcement.REASION : null,
                                    //SendCQT = (minvoicedata.STATUS == 1 && minvoicedata.MLTDIEP == "204" && minvoicedata.LTBAO == "2") ? 1 : 0,
                                    SendCQT = minvoicedata.STATUS,
                                    MessageCode = minvoicedata.MESSAGECODE,
                                    InvoiceDetailId = invoiceDetail.ID,
                                    CurrencyExchangeRate = (invoice.CURRENCYEXCHANGERATE ?? 1),
                                    Total = (invoice.TOTAL ?? 0),
                                    TotalTax = (invoice.TOTALTAX ?? 0),
                                    SumAmountInvoice = (invoice.SUM ?? 0),
                                    Report_class = invoice.REPORT_CLASS,
                                    CompanyId = mycompany.COMPANYID,
                                    Ltbao = minvoicedata.LTBAO,
                                    BTHERRORSTATUS = invoice.BTHERRORSTATUS,
                                    BTHERROR = invoice.BTHERROR,
                                }).ToList();


            invoiceInfos = invoiceInfos.GroupBy(x => new { x.Id }).Select(group => new ReportInvoiceDetail
            {
                Id = group.FirstOrDefault().Id,
                CustomerCode = group.FirstOrDefault().CustomerCode,
                CustomerName = group.FirstOrDefault().CustomerName,
                PersonContact = group.FirstOrDefault().PersonContact,
                Address = group.FirstOrDefault().Address,
                TaxCode = group.FirstOrDefault().IsOrg == true ?
                    group.FirstOrDefault().TaxCode != null ?
                    (group.FirstOrDefault().TaxCode.Length == 10 || group.FirstOrDefault().TaxCode.Length == 14) ?
                    group.FirstOrDefault().TaxCode.Contains("-000") ?
                    group.FirstOrDefault().TaxCode.Replace("-000", "") :
                    !(group.FirstOrDefault().TaxCode.Equals("88888888") || group.FirstOrDefault().TaxCode.Equals("9999999999")) ?
                    group.FirstOrDefault().TaxCode.All(c => ((c >= 48 && c <= 57 || (c == 45)))) ?
                    group.FirstOrDefault().TaxCode : null : null : null : null : null,
                Created = group.FirstOrDefault().Created,
                InvoiceCode = group.FirstOrDefault().InvoiceCode,
                InvoiceSymbol = group.FirstOrDefault().InvoiceSymbol,
                InvoiceNo = group.FirstOrDefault().InvoiceNo,
                TotalAmount = group.Sum(x => x.TotalAmount),
                TaxName = group.FirstOrDefault().TaxName,
                ReleasedDate = group.FirstOrDefault().ReleasedDate,
                AmountTax = group.Sum(x => x.AmountTax),
                TotalDiscount = group.Sum(x => x.TotalDiscount),
                TotalDiscountTax = group.Sum(x => x.TotalDiscountTax),
                Sum = group.Sum(x => x.Sum),
                IsMultiTax = group.FirstOrDefault().IsMultiTax,
                Currency = group.FirstOrDefault().Currency, //add currency
                ProductName = group.Count() == 1 ? group.FirstOrDefault().ProductName :
                    group.FirstOrDefault().Report_class != null ? group.FirstOrDefault().Report_class.Contains("6") ? "Tiền lãi ngân hàng(interest collection)" : "Phí dịch vụ ngân hàng ( Fee commission)" : null,
                Unit = group.Count() == 1 ? group.FirstOrDefault().Unit : null,
                Quantity = group.Count() == 1 ? group.FirstOrDefault().Quantity : null,
                IsOrg = group.FirstOrDefault().IsOrg,
                InvoiceType = group.FirstOrDefault().InvoiceType,
                ParentSymbol = group.FirstOrDefault().ParentSymbol,
                ParentNo = group.FirstOrDefault().ParentNo,
                ParentCode = group.FirstOrDefault().ParentCode,
                Note = group.FirstOrDefault().Note,
                SendCQT = group.FirstOrDefault().SendCQT,
                Ltbao = group.FirstOrDefault().Ltbao,
                MessageCode = group.FirstOrDefault().MessageCode,
                InvoiceNo2 = group.FirstOrDefault().InvoiceNo2,
                InvoiceDetailId = group.FirstOrDefault().InvoiceDetailId,
                CurrencyExchangeRate = group.FirstOrDefault().CurrencyExchangeRate,
                Total = group.FirstOrDefault().Total,
                TotalTax = group.FirstOrDefault().TotalTax,
                SumAmountInvoice = group.FirstOrDefault().SumAmountInvoice,
                BTHERRORSTATUS = group.FirstOrDefault().BTHERRORSTATUS,
                BTHERROR = group.FirstOrDefault().BTHERROR
            }).ToList();

            if (!String.IsNullOrEmpty(condition.StatusSendTVan))
            {
                if (condition.StatusSendTVan.Equals("all"))
                {
                    invoiceInfos = invoiceInfos.ToList();
                }
                else if (condition.StatusSendTVan.Equals("1"))
                {
                    invoiceInfos = invoiceInfos.Where(x =>
                    (x.SendCQT == StatusCQT.success && x.MessageCode != null) ||
                    (x.MessageCode != null && x.Ltbao == "4" && x.BTHERROR == null && x.BTHERRORSTATUS == null)).ToList();
                }
                else
                {
                    //Note : 20/08/2022 --Comment tạm mở ra

                    invoiceInfos = invoiceInfos.Where(x =>
                    //(x.MessageCode == null) ||
                    //(String.IsNullOrEmpty(x.MessageCode)) ||
                    //(x.SendCQT != StatusCQT.success && x.Ltbao == "4" && x.BTHERROR != null && x.BTHERRORSTATUS != null) ||
                    //(x.SendCQT != StatusCQT.success && x.Ltbao != "4") ||
                    //(x.MessageCode != null && x.BTHERROR != null) ||
                    //(x.SendCQT == StatusCQT.success && x.MessageCode == null)
                    (x.MessageCode == null) ||
                    (String.IsNullOrEmpty(x.MessageCode)) ||
                    ((x.MessageCode != null || !String.IsNullOrEmpty(x.MessageCode)) && (x.BTHERROR != null && !String.IsNullOrEmpty(x.BTHERROR)) && x.BTHERRORSTATUS != 40050)||
                    ((x.MessageCode != null || !String.IsNullOrEmpty(x.MessageCode)) && x.SendCQT == 0 )
                    ).ToList();

                    //invoiceInfos = invoiceInfos.Where(x => (x.MessageCode == null) || (String.IsNullOrEmpty(x.MessageCode))).ToList();
                }
            }

            var listInvoiceInfos = invoiceInfos.OrderBy(x => x.InvoiceNo2).ThenBy(x => x.ReleasedDate).ToList();

            logger.Error("Invoiceinfo count last : " + invoiceInfos.Count(), new Exception("Invoiceinfo count"));

            return listInvoiceInfos.AsQueryable();
        }

        public IQueryable<ReportInvoiceDetail> FillterListInvoiceSummaryViewReport_Auto(ConditionReportDetailUse condition)
        {
            var invoices = this.dbSet.Where(p => p.COMPANYID > 0 && p.INVOICESTATUS >= 4 && p.DECLAREID != null);

            //logger.Error("Invoiceinfo count first : " + invoices.Count() + " Current branch : " + condition.Branch.ToString(), new Exception("ListInfoSendGDT count first"));

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            //add condition CIF
            //if (condition.CustomerCode.IsNotNullOrEmpty())
            //{
            //    clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            //}

            //if (condition.InvoiceSampleId.HasValue)
            //{
            //    invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            //}

            //if (condition.Symbol.IsNotNullOrEmpty())
            //{
            //    invoices = invoices.Where(p => p.SYMBOL.Replace("/", "").Equals(condition.Symbol));
            //}

            //if (condition.DateFromJob.HasValue)
            //{
            //    invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFromJob.Value);
            //}

            //if (condition.DateToJob.HasValue)
            //{
            //    invoices = invoices.Where(p => p.RELEASEDDATE < condition.DateFromJob.Value);
            //}

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE < condition.DateTo.Value);
            }

            //logger.Error("Invoiceinfo count second : " + invoices.Count() + " Current branch : " + condition.Branch.ToString(), new Exception("ListInfoSendGDT count second"));


            //add condition
            //if (condition.CurrencyId.HasValue)
            //{
            //    invoices = invoices.Where(p => p.CURRENCYID == condition.CurrencyId.Value);
            //}

            //getlist = invoices.ToList();
            var invoiceInfos = (from invoice in invoices
                                join invoiceDetail in this.context.Set<INVOICEDETAIL>()
                                on invoice.ID equals invoiceDetail.INVOICEID into invoiceDetails
                                from invoiceDetail in invoiceDetails.DefaultIfEmpty()
                                join minvoicedata in this.context.Set<MINVOICE_DATA>()
                                on invoice.MESSAGECODE equals minvoicedata.MESSAGECODE into minvoicedatas
                                from minvoicedata in minvoicedatas.DefaultIfEmpty()
                                join client in clients
                                on invoice.CLIENTID equals client.ID
                                join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                                on invoice.REGISTERTEMPLATEID equals registerTemplate.ID into registerTemplates
                                from registerTemplate in registerTemplates.DefaultIfEmpty()
                                join invoiceSample in this.context.Set<INVOICESAMPLE>()
                                on registerTemplate.INVOICESAMPLEID equals invoiceSample.ID into invoiceSamples
                                from invoiceSample in invoiceSamples.DefaultIfEmpty()
                                join currency in this.context.Set<CURRENCY>()
                                on invoice.CURRENCYID equals currency.ID
                                join tax in this.context.Set<TAX>()
                                 on invoiceDetail.TAXID equals tax.ID into taxes
                                 from tax in taxes.DefaultIfEmpty()
                                join invoiceParent in this.context.Set<INVOICE>()
                                on invoice.PARENTID equals invoiceParent.ID into invoiceParents
                                from invoiceParent in invoiceParents.DefaultIfEmpty()
                                join announcement in this.context.Set<ANNOUNCEMENT>()
                                 on invoiceParent.ID equals announcement.INVOICEID into announcements
                                from announcement in announcements.DefaultIfEmpty()
                                join announcement2 in this.context.Set<ANNOUNCEMENT>()
                                on invoice.ID equals announcement2.INVOICEID into announcement2s
                                from announcement2 in announcement2s.DefaultIfEmpty()
                                join mycompany in this.context.Set<MYCOMPANY>()
                                on invoice.COMPANYID equals mycompany.COMPANYSID
                                where mycompany.COMPANYID == condition.Branch
                                select new ReportInvoiceDetail
                                {
                                    Id = invoice.ID,
                                    CustomerCode = client.CUSTOMERCODE,
                                    CustomerName = invoice.CUSTOMERNAME,
                                    PersonContact = invoice.PERSONCONTACT,
                                    Address = invoice.CUSTOMERADDRESS,
                                    TaxCode = invoice.CUSTOMERTAXCODE,
                                    Created = invoice.CREATEDDATE,
                                    InvoiceCode = registerTemplate.CODE,
                                    InvoiceSymbol = invoice.SYMBOL,
                                    InvoiceNo = invoice.NO,
                                    InvoiceNo2 = invoice.INVOICENO,
                                    TotalAmount = (invoice.TOTAL ?? 0),
                                    TaxName = tax.NAME,
                                    ReleasedDate = invoice.RELEASEDDATE,
                                    AmountTax = (invoice.TOTALTAX ?? 0),
                                    TotalDiscount = (invoice.TOTALDISCOUNT ?? 0),
                                    TotalDiscountTax = (invoice.TOTALDISCOUNTTAX ?? 0),
                                    //Sum = (invoice.SUM ?? 0) + (invoiceDetail.AMOUNTTAX ?? 0),
                                    Sum = (invoice.SUM ?? 0),
                                    IsMultiTax = invoiceSample.ISMULTITAX,
                                    Currency = currency.CODE, //add currency
                                    ProductName = invoiceDetail.PRODUCTNAME,
                                    Unit = invoiceDetail.UNITNAME,
                                    Quantity = invoiceDetail.QUANTITY ?? 1,
                                    IsOrg = client.ISORG ?? false,
                                    InvoiceType = (invoice.INVOICESTATUS == 7) ? 1 : (invoice.INVOICESTATUS == 5 && invoiceParent.NO != null) ? 0 : (invoice.INVOICESTATUS == 5 && invoiceParent.NO == null) ? 1 : invoice.INVOICETYPE == null ? 0 : (invoice.INVOICETYPE == 2 || invoice.INVOICETYPE == 6) ? 2 : invoice.INVOICETYPE == 1 ? 3 : invoice.INVOICETYPE == 3 ? 4 : invoice.INVOICETYPE,
                                    ParentSymbol = invoiceParent.NO != null ? invoiceParent.SYMBOL : null,
                                    ParentNo = invoiceParent.NO != null ? invoiceParent.NO : null,
                                    ParentCode = invoiceParent.NO != null ? invoiceParent.SYMBOL.Substring(0, 1) : null,
                                    Note = invoice.INVOICETYPE != 1 ? (invoice.INVOICESTATUS == 5 || invoice.INVOICESTATUS == 7) ? announcement2.REASION : invoice.INVOICETYPE == 3 ? announcement2.REASION : announcement.REASION : null,
                                    SendCQT = (minvoicedata.STATUS == 1 && minvoicedata.MLTDIEP == "204" && minvoicedata.LTBAO == "2") ? 1 : 0,
                                    MessageCode = invoice.MESSAGECODE,
                                    //InvoiceDetailId = invoiceDetail.ID,
                                    CurrencyExchangeRate = (invoice.CURRENCYEXCHANGERATE ?? 1),
                                    Total = (invoice.TOTAL ?? 0),
                                    TotalTax = (invoice.TOTALTAX ?? 0),
                                    SumAmountInvoice = (invoice.SUM ?? 0),
                                    Report_class = invoice.REPORT_CLASS,
                                    CompanyId = mycompany.COMPANYID,
                                    Ltbao = minvoicedata.LTBAO,
                                    BTHERRORSTATUS = invoice.BTHERRORSTATUS,
                                    BTHERROR = invoice.BTHERROR,
                                }).ToList();


            invoiceInfos = invoiceInfos.GroupBy(x => new { x.Id }).Select(group => new ReportInvoiceDetail
            {
                Id = group.FirstOrDefault().Id,
                CustomerCode = group.FirstOrDefault().CustomerCode,
                CustomerName = group.FirstOrDefault().CustomerName,
                PersonContact = group.FirstOrDefault().PersonContact,
                Address = group.FirstOrDefault().Address,
                TaxCode = group.FirstOrDefault().IsOrg == true ?
                    group.FirstOrDefault().TaxCode != null ?
                    (group.FirstOrDefault().TaxCode.Length == 10 || group.FirstOrDefault().TaxCode.Length == 14) ?
                    group.FirstOrDefault().TaxCode.Contains("-000") ?
                    group.FirstOrDefault().TaxCode.Replace("-000", "") :
                    !(group.FirstOrDefault().TaxCode.Equals("88888888") || group.FirstOrDefault().TaxCode.Equals("9999999999")) ?
                    group.FirstOrDefault().TaxCode.All(c => ((c >= 48 && c <= 57 || (c == 45)))) ?
                    group.FirstOrDefault().TaxCode : null : null : null : null : null,
                Created = group.FirstOrDefault().Created,
                InvoiceCode = group.FirstOrDefault().InvoiceCode,
                InvoiceSymbol = group.FirstOrDefault().InvoiceSymbol,
                InvoiceNo = group.FirstOrDefault().InvoiceNo,
                TotalAmount = group.Sum(x => x.TotalAmount),
                TaxName = group.FirstOrDefault().TaxName,
                ReleasedDate = group.FirstOrDefault().ReleasedDate,
                AmountTax = group.Sum(x => x.AmountTax),
                TotalDiscount = group.Sum(x => x.TotalDiscount),
                TotalDiscountTax = group.Sum(x => x.TotalDiscountTax),
                Sum = group.Sum(x => x.Sum),
                IsMultiTax = group.FirstOrDefault().IsMultiTax,
                Currency = group.FirstOrDefault().Currency, //add currency
                ProductName = group.Count() == 1 ? group.FirstOrDefault().ProductName :
                    group.FirstOrDefault().Report_class != null ? group.FirstOrDefault().Report_class.Contains("6") ? "Tiền lãi ngân hàng(interest collection)" : "Phí dịch vụ ngân hàng ( Fee commission)" : null,
                Unit = group.Count() == 1 ? group.FirstOrDefault().Unit : null,
                Quantity = group.Count() == 1 ? group.FirstOrDefault().Quantity : null,
                IsOrg = group.FirstOrDefault().IsOrg,
                InvoiceType = group.FirstOrDefault().InvoiceType,
                ParentSymbol = group.FirstOrDefault().ParentSymbol,
                ParentNo = group.FirstOrDefault().ParentNo,
                ParentCode = group.FirstOrDefault().ParentCode,
                Note = group.FirstOrDefault().Note,
                SendCQT = group.FirstOrDefault().SendCQT,
                Ltbao = group.FirstOrDefault().Ltbao,
                MessageCode = group.FirstOrDefault().MessageCode,
                InvoiceNo2 = group.FirstOrDefault().InvoiceNo2,
                //InvoiceDetailId = group.FirstOrDefault().InvoiceDetailId,
                CurrencyExchangeRate = group.FirstOrDefault().CurrencyExchangeRate,
                Total = group.FirstOrDefault().Total,
                TotalTax = group.FirstOrDefault().TotalTax,
                SumAmountInvoice = group.FirstOrDefault().SumAmountInvoice,
                BTHERRORSTATUS = group.FirstOrDefault().BTHERRORSTATUS,
                BTHERROR = group.FirstOrDefault().BTHERROR
            }).ToList();

            if (!String.IsNullOrEmpty(condition.StatusSendTVan))
            {
                if (condition.StatusSendTVan.Equals("all"))
                {
                    invoiceInfos = invoiceInfos.ToList();
                }
                else if (condition.StatusSendTVan.Equals("1"))
                {
                    invoiceInfos = invoiceInfos.Where(x => (x.SendCQT == StatusCQT.success && x.MessageCode != null) || (x.MessageCode != null && x.Ltbao == "4" && x.BTHERROR == null && x.BTHERRORSTATUS == null)).ToList();
                }
                else
                {
                    //Note : 20/08/2022 --Comment tạm mở ra

                    invoiceInfos = invoiceInfos.Where(x =>
                    (x.MessageCode == null) ||
                    (String.IsNullOrEmpty(x.MessageCode))
                    ).ToList();
                    //||
                    //(x.SendCQT != StatusCQT.success && x.Ltbao == "4" && x.BTHERROR != null && x.BTHERRORSTATUS != null) ||
                    //(x.SendCQT != StatusCQT.success && x.Ltbao != "4") ||
                    //(x.MessageCode != null && x.BTHERROR != null) ||
                    //(x.SendCQT == StatusCQT.success && x.MessageCode == null)
                    //invoiceInfos = invoiceInfos.Where(x => (x.MessageCode == null) || (String.IsNullOrEmpty(x.MessageCode))).ToList();
                    //|| ((x.MessageCode != null || !String.IsNullOrEmpty(x.MessageCode)) && (x.BTHERROR != null && !String.IsNullOrEmpty(x.BTHERROR)))
                }
            }

            var listInvoiceInfos = invoiceInfos.OrderBy(x => x.InvoiceNo2).ThenBy(x => x.ReleasedDate).ToList();

            logger.Error("Invoiceinfo count last : " + invoiceInfos.Count() + " Current branch : " + condition.Branch.ToString(), new Exception("ListInfoSendGDT count"));

            return listInvoiceInfos.AsQueryable();
        }
        public long CountFillterListInvoiceSummaryViewReport(ConditionReportDetailUse condition)
        {
            var invoices = this.dbSet.Where(p => p.COMPANYID > 0 && p.INVOICESTATUS >= (int)InvoiceStatus.Released && p.DECLAREID != null);

            //if (condition.Branch.HasValue && condition.Branch > 0)
            //{
            //    invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            //}
            //else if (!condition.Branch.HasValue)
            //{
            //    invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            //}

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            //add condition CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Replace("/", "").Equals(condition.Symbol));
            }

            //if (condition.InvoiceStatus.Count > 0)
            //{
            //    invoices = invoices.Where(p => condition.InvoiceStatus.Contains(p.INVOICESTATUS ?? 0));
            //}

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
            }
            //add condition
            if (condition.CurrencyId.HasValue)
            {
                invoices = invoices.Where(p => p.CURRENCYID == condition.CurrencyId.Value);
            }

            var invoiceInfos = (from invoice in invoices
                                join invoiceDetail in this.context.Set<INVOICEDETAIL>()
                                on invoice.ID equals invoiceDetail.INVOICEID
                                join minvoicedata in this.context.Set<MINVOICE_DATA>()
                                on invoice.MESSAGECODE equals minvoicedata.MESSAGECODE into minvoicedatas
                                from minvoicedata in minvoicedatas.DefaultIfEmpty()
                                join client in clients
                                on invoice.CLIENTID equals client.ID
                                join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                                on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                                join invoiceSample in this.context.Set<INVOICESAMPLE>()
                                on registerTemplate.INVOICESAMPLEID equals invoiceSample.ID
                                join currency in this.context.Set<CURRENCY>()
                                on invoice.CURRENCYID equals currency.ID
                                join tax in this.context.Set<TAX>()
                                 on invoiceDetail.TAXID equals tax.ID
                                join invoiceParent in this.context.Set<INVOICE>()
                                on invoice.PARENTID equals invoiceParent.ID into invoiceParents
                                from invoiceParent in invoiceParents.DefaultIfEmpty()
                                join announcement in this.context.Set<ANNOUNCEMENT>()
                                 on invoiceParent.ID equals announcement.INVOICEID into announcements
                                from announcement in announcements.DefaultIfEmpty()
                                join announcement2 in this.context.Set<ANNOUNCEMENT>()
                                on invoice.ID equals announcement2.INVOICEID into announcement2s
                                from announcement2 in announcement2s.DefaultIfEmpty()
                                join mycompany in this.context.Set<MYCOMPANY>()
                                on invoice.COMPANYID equals mycompany.COMPANYSID
                                where mycompany.COMPANYID == condition.Branch
                                select new ReportInvoiceDetail
                                {
                                    Id = invoice.ID,
                                    CustomerCode = client.CUSTOMERCODE,
                                    CustomerName = invoice.CUSTOMERNAME,
                                    PersonContact = invoice.PERSONCONTACT,
                                    Address = invoice.CUSTOMERADDRESS,
                                    TaxCode = invoice.CUSTOMERTAXCODE,
                                    Created = invoice.CREATEDDATE,
                                    InvoiceCode = registerTemplate.CODE,
                                    InvoiceSymbol = invoice.SYMBOL,
                                    InvoiceNo = invoice.NO,
                                    TotalAmount = (invoiceDetail.TOTAL ?? 0),
                                    TaxName = tax.NAME,
                                    ReleasedDate = invoice.RELEASEDDATE,
                                    AmountTax = (invoiceDetail.AMOUNTTAX ?? 0),
                                    TotalDiscount = (invoiceDetail.AMOUNTDISCOUNT ?? 0),
                                    TotalDiscountTax = (invoice.TOTALDISCOUNTTAX ?? 0),
                                    Sum = (invoiceDetail.TOTAL ?? 0) + (invoiceDetail.AMOUNTTAX ?? 0),
                                    IsMultiTax = invoiceSample.ISMULTITAX,
                                    Currency = currency.CODE, //add currency
                                    ProductName = invoiceDetail.PRODUCTNAME,
                                    Unit = invoiceDetail.UNITNAME,
                                    Quantity = invoiceDetail.QUANTITY,
                                    IsOrg = client.ISORG ?? false,
                                    InvoiceType = (invoice.INVOICESTATUS == 5 || invoice.INVOICESTATUS == 7) ? 1 : invoice.INVOICETYPE == null ? 0 : invoice.INVOICETYPE == 2 ? 2 : invoice.INVOICETYPE == 1 ? 3 : invoice.INVOICETYPE == 3 ? 4 : invoice.INVOICETYPE,
                                    ParentSymbol = invoiceParent.NO != null ? invoiceParent.SYMBOL + ", " : null,
                                    ParentNo = invoiceParent.NO != null ? invoiceParent.NO : null,
                                    ParentCode = invoiceParent.NO != null ? invoiceParent.SYMBOL.Substring(0, 1) + ", " : null,
                                    Note = invoice.INVOICETYPE != 1 ? (invoice.INVOICESTATUS == 5 || invoice.INVOICESTATUS == 7) ? announcement2.REASION : invoice.INVOICETYPE == 3 ? announcement2.REASION : announcement.REASION : null,
                                    SendCQT = minvoicedata.STATUS,
                                    MessageCode = minvoicedata.MESSAGECODE,
                                    InvoiceNo2 = invoice.INVOICENO,
                                    InvoiceDetailId = invoiceDetail.ID,
                                    CurrencyExchangeRate = (invoice.CURRENCYEXCHANGERATE ?? 1),
                                    Total = (invoice.TOTAL ?? 0),
                                    TotalTax = (invoice.TOTALTAX ?? 0),
                                    SumAmountInvoice = (invoice.SUM ?? 0),
                                    CompanyId = mycompany.COMPANYID,
                                    LtBao = minvoicedata.LTBAO,
                                    MltDiep = minvoicedata.MLTDIEP,
                                    BTHERRORSTATUS = invoice.BTHERRORSTATUS,
                                    BTHERROR = invoice.BTHERROR,
                                }).ToList();

            //if (condition.Branch.HasValue && condition.Branch > 0)
            //{
            //    invoiceInfos = invoiceInfos.Where(x => x.CompanyId == condition.Branch).ToList();
            //}

            invoiceInfos = invoiceInfos.GroupBy(x => new { x.Id }).Select(group => new ReportInvoiceDetail
            {
                Id = group.FirstOrDefault().Id,
                CustomerCode = group.FirstOrDefault().CustomerCode,
                CustomerName = group.FirstOrDefault().CustomerName,
                PersonContact = group.FirstOrDefault().PersonContact,
                Address = group.FirstOrDefault().Address,
                TaxCode = group.FirstOrDefault().TaxCode,
                Created = group.FirstOrDefault().Created,
                InvoiceCode = group.FirstOrDefault().InvoiceCode,
                InvoiceSymbol = group.FirstOrDefault().InvoiceSymbol,
                InvoiceNo = group.FirstOrDefault().InvoiceNo,
                TotalAmount = group.Sum(x => x.TotalAmount),
                TaxName = group.FirstOrDefault().TaxName,
                ReleasedDate = group.FirstOrDefault().ReleasedDate,
                AmountTax = group.Sum(x => x.AmountTax),
                TotalDiscount = group.Sum(x => x.TotalDiscount),
                TotalDiscountTax = group.Sum(x => x.TotalDiscountTax),
                Sum = group.Sum(x => x.Sum),
                IsMultiTax = group.FirstOrDefault().IsMultiTax,
                Currency = group.FirstOrDefault().Currency, //add currency
                ProductName = group.Count() == 1 ? group.FirstOrDefault().ProductName : null,
                Unit = group.Count() == 1 ? group.FirstOrDefault().Unit : null,
                Quantity = group.Count() == 1 ? group.FirstOrDefault().Quantity : null,
                IsOrg = group.FirstOrDefault().IsOrg,
                InvoiceType = group.FirstOrDefault().InvoiceType,
                ParentSymbol = group.FirstOrDefault().ParentSymbol,
                ParentNo = group.FirstOrDefault().ParentNo,
                ParentCode = group.FirstOrDefault().ParentCode,
                Note = group.FirstOrDefault().Note,
                SendCQT = group.FirstOrDefault().SendCQT,
                MessageCode = group.FirstOrDefault().MessageCode,
                InvoiceNo2 = group.FirstOrDefault().InvoiceNo2,
                InvoiceDetailId = group.FirstOrDefault().InvoiceDetailId,
                CurrencyExchangeRate = group.FirstOrDefault().CurrencyExchangeRate,
                Total = group.FirstOrDefault().Total,
                TotalTax = group.FirstOrDefault().TotalTax,
                MltDiep = group.FirstOrDefault().MltDiep,
                LtBao = group.FirstOrDefault().LtBao,
                BTHERROR = group.FirstOrDefault().BTHERROR,
                BTHERRORSTATUS = group.FirstOrDefault().BTHERRORSTATUS
            }).ToList();

            if (!String.IsNullOrEmpty(condition.StatusSendTVan))
            {
                if (condition.StatusSendTVan.Equals("all"))
                {
                    invoiceInfos = invoiceInfos.ToList();
                }
                else if (condition.StatusSendTVan.Equals("1"))
                {
                    invoiceInfos = invoiceInfos.Where(x => (x.SendCQT == StatusCQT.success) || (x.MltDiep == "204" && x.LtBao == "4" && x.BTHERROR == null && x.BTHERRORSTATUS == null)).ToList();
                }
                else
                {
                    invoiceInfos = invoiceInfos.Where(x =>
                    (x.MessageCode == null) ||
                    (String.IsNullOrEmpty(x.MessageCode)) ||
                    (x.SendCQT != StatusCQT.success && x.Ltbao == "4" && x.BTHERROR != null && x.BTHERRORSTATUS != null) ||
                    (x.SendCQT != StatusCQT.success && x.Ltbao != "4") ||
                    (x.MessageCode != null && x.BTHERROR != null) ||
                    (x.SendCQT == StatusCQT.success && x.MessageCode == null)).ToList();
                }
            }

            var listInvoiceInfos = invoiceInfos.OrderBy(x => x.InvoiceSymbol).ThenBy(x => x.InvoiceNo2).ThenBy(x => x.ReleasedDate).ToList();

            return listInvoiceInfos.Count();
        }

        public IEnumerable<ReportInvoiceDetail> FillterListInvoiceSummaryDataXML(ConditionReportDetailUse condition)
        {
            var invoices = this.dbSet.Where(p => p.COMPANYID > 0 && p.INVOICESTATUS >= 4 && p.DECLAREID != null);

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.Branch);
            }
            else if (!condition.Branch.HasValue)
            {
                invoices = invoices.Where(p => p.COMPANYID == condition.CompanyId);
            }

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            //add condition CIF
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.SYMBOL.Replace("/", "").Equals(condition.Symbol));
            }

            //if (condition.InvoiceStatus.Count > 0)
            //{
            //    invoices = invoices.Where(p => condition.InvoiceStatus.Contains(p.INVOICESTATUS ?? 0));
            //}

            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                invoices = invoices.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
            }
            //add condition
            if (condition.CurrencyId.HasValue)
            {
                invoices = invoices.Where(p => p.CURRENCYID == condition.CurrencyId.Value);
            }

            var invoiceInfos = from invoice in invoices
                               join invoiceDetail in this.context.Set<INVOICEDETAIL>()
                               on invoice.ID equals invoiceDetail.INVOICEID
                               join minvoicedata in this.context.Set<MINVOICE_DATA>()
                               on invoice.MESSAGECODE equals minvoicedata.MESSAGECODE into minvoicedatas
                               from minvoicedata in minvoicedatas.DefaultIfEmpty()
                               join client in clients
                               on invoice.CLIENTID equals client.ID
                               join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                               on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                               join invoiceSample in this.context.Set<INVOICESAMPLE>()
                               on registerTemplate.INVOICESAMPLEID equals invoiceSample.ID
                               join currency in this.context.Set<CURRENCY>()
                               on invoice.CURRENCYID equals currency.ID
                               join tax in this.context.Set<TAX>()
                                on invoiceDetail.TAXID equals tax.ID
                               join invoiceParent in this.context.Set<INVOICE>()
                               on invoice.PARENTID equals invoiceParent.ID into invoiceParents
                               from invoiceParent in invoiceParents.DefaultIfEmpty()
                               join announcement in this.context.Set<ANNOUNCEMENT>()
                               on invoiceParent.ID equals announcement.INVOICEID into announcements
                               from announcement in announcements.DefaultIfEmpty()
                               join announcement2 in this.context.Set<ANNOUNCEMENT>()
                               on invoice.ID equals announcement2.INVOICEID into announcement2s
                               from announcement2 in announcement2s.DefaultIfEmpty()
                               select new ReportInvoiceDetail
                               {
                                   Id = invoice.ID,
                                   CustomerCode = client.CUSTOMERCODE,
                                   CustomerName = invoice.CUSTOMERNAME,
                                   PersonContact = invoice.PERSONCONTACT,
                                   Address = invoice.CUSTOMERADDRESS,
                                   TaxCode = invoice.CUSTOMERTAXCODE,
                                   Created = invoice.CREATEDDATE,
                                   InvoiceCode = registerTemplate.CODE,
                                   InvoiceSymbol = invoice.SYMBOL,
                                   InvoiceNo = invoice.NO,
                                   TotalAmount = (invoiceDetail.TOTAL ?? 0),
                                   //TaxName = tax.NAME.Equals("Không chịu thuế") ? "KCT" : tax.NAME,
                                   TaxName = tax.NAME,
                                   ReleasedDate = invoice.RELEASEDDATE,
                                   AmountTax = (invoiceDetail.AMOUNTTAX ?? 0),
                                   TotalDiscount = (invoiceDetail.AMOUNTDISCOUNT ?? 0),
                                   TotalDiscountTax = (invoice.TOTALDISCOUNTTAX ?? 0),
                                   Sum = (invoiceDetail.TOTAL ?? 0) + (invoiceDetail.AMOUNTTAX ?? 0),
                                   IsMultiTax = invoiceSample.ISMULTITAX,
                                   Currency = currency.CODE, //add currency
                                   ProductName = invoiceDetail.PRODUCTNAME,
                                   Unit = invoiceDetail.UNITNAME,
                                   Quantity = invoiceDetail.QUANTITY ?? 0,
                                   IsOrg = client.ISORG ?? false,
                                   InvoiceType = (invoice.INVOICESTATUS == 5 || invoice.INVOICESTATUS == 7) ? 1 : invoice.INVOICETYPE == null ? 0 : invoice.INVOICETYPE == 2 ? 2 : invoice.INVOICETYPE == 1 ? 3 : invoice.INVOICETYPE == 3 ? 4 : invoice.INVOICETYPE,
                                   ParentSymbol = invoiceParent.SYMBOL,
                                   ParentNo = invoiceParent.NO,
                                   ParentCode = invoiceParent.SYMBOL.Substring(0, 1),
                                   Note = invoice.INVOICETYPE != 1 ? (invoice.INVOICESTATUS == 5 || invoice.INVOICESTATUS == 7) ? announcement2.REASION : invoice.INVOICETYPE == 3 ? announcement2.REASION : announcement.REASION : null,
                                   SendCQT = minvoicedata.STATUS,
                                   MessageCode = minvoicedata.MESSAGECODE,
                                   InvoiceNo2 = invoice.INVOICENO,
                                   InvoiceDetailId = invoiceDetail.ID,
                                   CurrencyExchangeRate = (invoice.CURRENCYEXCHANGERATE ?? 1)
                               };

            //var listInvoiceInfos = invoiceInfos.Distinct()
            //                    .OrderBy("InvoiceSymbol", condition.OrderType.Equals(OrderTypeConst.Asc))
            //                    .ThenBy("InvoiceNo2", condition.OrderType.Equals(OrderTypeConst.Asc))
            //                    .ThenBy("ReleasedDate", condition.OrderType.Equals(OrderTypeConst.Asc))
            //                    .Skip(condition.Skip).Take(condition.Take)
            //                    .ToList();


            var listInvoiceInfos = invoiceInfos.Distinct()
                                .OrderBy("InvoiceSymbol", condition.OrderType.Equals(OrderTypeConst.Asc))
                                .ThenBy("InvoiceNo2", condition.OrderType.Equals(OrderTypeConst.Asc))
                                .ThenBy("InvoiceDetailId", condition.OrderType.Equals(OrderTypeConst.Asc))
                                .ThenBy("ReleasedDate", condition.OrderType.Equals(OrderTypeConst.Asc))
                                .ToList();

            //foreach (var item in listInvoiceInfos)
            //{
            //    if (item.CustomerName == "")
            //    {
            //        item.CustomerName = item.PersonContact;
            //    }
            //}.OrderBy("InvoiceCode", condition.OrderType.Equals(OrderTypeConst.Asc))

            if (!String.IsNullOrEmpty(condition.StatusSendTVan))
            {
                if (condition.StatusSendTVan.Equals("all"))
                {
                    listInvoiceInfos = listInvoiceInfos.ToList();
                }
                else if (condition.StatusSendTVan.Equals("1"))
                {
                    listInvoiceInfos = listInvoiceInfos.Where(x => x.SendCQT == StatusCQT.success).ToList();
                }
                else
                {
                    listInvoiceInfos = listInvoiceInfos.Where(x => x.SendCQT == StatusCQT.error || x.MessageCode == null || String.IsNullOrEmpty(x.MessageCode)).ToList();
                }
            }


            return listInvoiceInfos;
        }

        /// <summary>
        /// Lấy danh sách hóa đơn
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<INVOICE> GetByIds(List<long> ids)
        {
            return this.dbSet.Where(f => ids.Any(t => t == f.ID)).ToList();
        }

        public decimal? GetBSLThu(ConditionReportDetailUse condition)
        {
            var historys = this.context.Set<HISTORYREPORTGENERAL>().Where(p => p.MESSAGECODE != null); ;

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                historys = historys.Where(p => p.COMPANYID == condition.Branch);
            }
            else if (!condition.Branch.HasValue)
            {
                historys = historys.Where(p => p.COMPANYID == condition.CompanyId);
            }

            if (condition.IsMonth == 0)
            {
                historys = historys.Where(p => p.QUARTER == condition.Month && p.YEAR == condition.Year);
            }
            else
            {
                historys = historys.Where(p => p.MONTH == condition.Month && p.YEAR == condition.Year);
            }

            var minvoices = this.context.Set<MINVOICE_DATA>().Where(p => (p.STATUS == 1 && p.MLTDIEP == "204" && p.LTBAO == "2") || 
                                                                         (p.STATUS == 0 && p.MLTDIEP == "204" && p.LTBAO == "4")
                                                                         );

            var historyInfos = (from history in historys
                                join minvoice in minvoices
                                on history.MESSAGECODE equals minvoice.MESSAGECODE
                                select new ReportInvoiceDetail
                                {
                                    BSLThu = history.ADDITIONALTIMES == null ? 1 : history.ADDITIONALTIMES,
                                }).ToList();

            decimal? BSLThu = 1;
            if (historyInfos.Count() != 0)
            {
                BSLThu = historyInfos.Max(p => p.BSLThu) + 1;
            }

            return BSLThu;
        }
        public List<HistoryReportItem> GetInvoiceInfoBTH(long hisId) 
        {
            var result = (from his in this.context.Set<HISTORYREPORTGENERAL>()
                          join inv in this.context.Set<INVOICE>() on his.MESSAGECODE equals inv.MESSAGECODE
                          join company in this.context.Set<MYCOMPANY>() on inv.COMPANYID equals company.COMPANYSID
                          join dataMinvoice in this.context.Set<MINVOICE_DATA>() on his.MESSAGECODE equals dataMinvoice.MESSAGECODE
                           into dataMinvoices
                          from dataMinvoice in dataMinvoices.DefaultIfEmpty()
                          where his.ID == hisId
                          select new HistoryReportItem
                          {
                              CompanyId = company.COMPANYSID,
                              CompanyName = company.COMPANYNAME,
                              Symbol = inv.SYMBOL,
                              InvoiceNo = inv.INVOICENO ?? 0,
                              InvoiceDate = inv.RELEASEDDATE ?? DateTime.Now,
                              SBTHDLIEU = his.SBTHDLIEU ?? 0,
                              CQTStatus = dataMinvoice.STATUS != null ? dataMinvoice.STATUS == 0 ? 1 : 2 : 3,
                              MessageCode = his.MESSAGECODE
                          }).ToList();

            return result;
        }
        public List<MINVOICE_DATA> ListErrorMinvoice(string messgaeCode)
        {
            return this.context.Set<MINVOICE_DATA>().Where(p => p.MESSAGECODE == messgaeCode).ToList();
        }
        public INVOICE GetByNoSymbol(string symbol, decimal invoiceNo)
        {
            return this.dbSet.FirstOrDefault(p => p.SYMBOL == symbol && p.INVOICENO == invoiceNo);
        }

        public IQueryable<InvoiceCheckDaily> FilterInvoiceCheckDaily(ConditionSearchCheckNumberInvoice condition)
        {
           const int TYPEFILTER_INVOICENO = 1;
           const int TYPEFILTER_INVOICEINFO = 2;
           const string VALUEFILTER_YES = "Y";
           const string VALUEFILTER_NO = "N";


            var invoicesM = this.context.Set<INVOICE>().Where(x => x.DELETED != true && x.INVOICESTATUS >= 2);
            var invoices = invoicesM;
            var companies = this.context.Set<MYCOMPANY>().AsQueryable();
            var clients = this.context.Set<CLIENT>();
            var symbols = this.context.Set<SYMBOL>();
            //if (condition.Branch.HasValue)
            //{
            //    companies = companies.Where(x => x.COMPANYSID == condition.Branch);
            //}
            
            if (condition.DateFrom.HasValue)
            {
                invoices = invoices.Where(x => x.RELEASEDDATE >= condition.DateFrom);
            }
            if (condition.DateTo.HasValue)
            {
                var dateTo = condition.DateTo.Value.AddDays(1);
                invoices = invoices.Where(x => x.RELEASEDDATE < dateTo);
            }
            if (!string.IsNullOrWhiteSpace(condition.Symbols))
            {
                invoices = invoices.Where(x => x.SYMBOL.Substring(5, 2) == condition.Symbols);
            }

            var joins = from invoice in invoices
                        join company in companies on invoice.COMPANYID equals company.COMPANYSID
                        join companyParent in companies on company.COMPANYID equals companyParent.COMPANYSID
                        join client in clients on invoice.CLIENTID equals client.ID
                        let date = DbFunctions.TruncateTime(invoice.RELEASEDDATE)
                        select new { invoice, company, client, date, companyParent };        

            var group = from a in (from j in joins select new { j.companyParent.COMPANYSID, j.companyParent.COMPANYNAME, j.companyParent.BRANCHID, j.invoice.SYMBOL, j.date }).Distinct()
                        from b in (from j in joins.Where(x => x.companyParent.COMPANYSID == a.COMPANYSID && x.companyParent.BRANCHID == a.BRANCHID && x.invoice.SYMBOL == a.SYMBOL && x.date == a.date)
                                   group j by new { j.companyParent.COMPANYSID, j.companyParent.COMPANYNAME, j.companyParent.BRANCHID, j.invoice.SYMBOL, j.date } into g
                                   select new
                                   {
                                       min = g.Min(x => x.invoice.INVOICENO),
                                       max = g.Max(x => x.invoice.INVOICENO),
                                       count = g.Count(),
                                       yesno_missingInfo = g.Any(x =>
                                            string.IsNullOrEmpty(x.invoice.CUSTOMERTAXCODE)
                                            || string.IsNullOrEmpty(x.invoice.CUSTOMERADDRESS)
                                            || (x.client.ISORG == true && string.IsNullOrEmpty(x.invoice.CUSTOMERNAME))
                                            || (x.client.ISORG != true && string.IsNullOrEmpty(x.invoice.PERSONCONTACT))
                                        ) ? VALUEFILTER_NO : VALUEFILTER_YES
                                   }).Take(1)
                        let prevDateInvoice = invoicesM.Where(x => x.COMPANYID == a.COMPANYSID && x.SYMBOL == a.SYMBOL && x.RELEASEDDATE < a.date).OrderByDescending(x => x.RELEASEDDATE).ThenByDescending(x => x.INVOICENO).FirstOrDefault()
                        let nextDateInvoice = invoicesM.Where(x => x.COMPANYID == a.COMPANYSID && x.SYMBOL == a.SYMBOL && x.RELEASEDDATE >= DbFunctions.AddDays(a.date, 1)).OrderBy(x => x.RELEASEDDATE).OrderBy(x => x.INVOICENO).FirstOrDefault()
                        let quantity = b.max - b.min + 1
                        let countInvoice = b.count
                        let nextno = b.max + 1
                        let soHDLung = quantity - countInvoice
                        let yesno_missingNum = ((quantity != b.count)
                                                || (prevDateInvoice != null && b.min - prevDateInvoice.INVOICENO > 1)
                                                || (nextDateInvoice != null && nextDateInvoice.INVOICENO - b.max > 1) 
                                                || (nextDateInvoice != null && nextDateInvoice.INVOICENO != nextno))
                                                ? VALUEFILTER_NO : VALUEFILTER_YES
                        orderby a.date descending, a.COMPANYSID, a.BRANCHID
                        select new InvoiceCheckDaily
                        {
                            CompanyId = a.COMPANYSID,
                            BranchId = a.BRANCHID,
                            CompanyName = a.COMPANYNAME,
                            Symbol = a.SYMBOL,
                            ReleasedDate = a.date,
                            MinInvoiceNo = b.min,
                            MaxInvoiceNo = b.max,
                            Quantity = quantity,
                            InvNoResult = yesno_missingNum,
                            InvInfoResult = b.yesno_missingInfo
                        };

            if (condition.Branch.HasValue)
            {
                group = group.Where(x => x.CompanyId == condition.Branch);
            }

            if (!string.IsNullOrWhiteSpace(condition.BranchID))
            {
                group = group.Where(x => x.BranchId == condition.BranchID);
            }

            if (!string.IsNullOrEmpty(condition.ResultType))
            {
                group = group.Where(x => x.InvNoResult == condition.ResultType);
            }


            if (!string.IsNullOrEmpty(condition.ResultValue))
            {
                group = group.Where(x => x.InvInfoResult == condition.ResultValue);
            }


            return group;
        }
    }
}