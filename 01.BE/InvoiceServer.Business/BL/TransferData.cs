using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Data;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public static class TransferData
    {
        public static void CopyData(this LOGINUSER toObject, AccountInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            if (toObject.USERSID == 0)
            {
                toObject.USERID = fromObject.UserID;
            }

            if (fromObject.Password.IsNotNullOrEmpty())
            {
                toObject.PASSWORD = fromObject.Password;
            }

            toObject.USERNAME = fromObject.UserName;
            toObject.EMAIL = fromObject.Email;
            toObject.MOBILE = fromObject.Mobile;
            toObject.ISACTIVE = true;
        }

        public static void CopyData(this LOGINUSER toObject, AccountRoleInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            if (toObject.USERSID == 0)
            {
                toObject.USERID = fromObject.UserID;
            }

            if (fromObject.Password.IsNotNullOrEmpty())
            {
                toObject.PASSWORD = fromObject.Password;
            }

            toObject.USERNAME = fromObject.UserName;
            toObject.EMAIL = fromObject.Email;
            toObject.MOBILE = fromObject.Mobile;
            toObject.ISACTIVE = fromObject.IsActive;
        }

        public static void CopyData(this CLIENT toObject, ClientAddInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.CUSTOMERID = fromObject.CustomerId;
            toObject.CUSTOMERCODE = fromObject.CustomerCode;
            toObject.TAXCODE = fromObject.TaxCode;
            toObject.CUSTOMERNAME = fromObject.CustomerName;
            toObject.ADDRESS = fromObject.Address;
            toObject.FAX = fromObject.Fax;
            toObject.TYPESENDINVOICE = fromObject.TypeSendInvoice;
            toObject.MOBILE = fromObject.Mobile;
            toObject.EMAIL = fromObject.Email;
            toObject.DELEGATE = fromObject.Delegate;
            toObject.BANKACCOUNT = fromObject.BankAccount;
            toObject.ACCOUNTHOLDER = fromObject.AccountHolder;
            toObject.BANKNAME = fromObject.BankName;
            toObject.DESCRIPTION = fromObject.Description;
            toObject.CUSTOMERTYPE = fromObject.CustomerType;
            toObject.PERSONCONTACT = fromObject.PersonContact;
            toObject.ISORG = fromObject.IsOrg;
            toObject.SENDINVOICEBYMONTH = fromObject.SendInvoiceByMonth;
            toObject.DATESENDINVOICE = fromObject.DateSendInvoice;
            toObject.TAXINCENTIVES = fromObject.TaxIncentives;
            toObject.USEREGISTEREMAIL = fromObject.UseRegisterEmail;
            toObject.USETOTALINVOICE = fromObject.UseTotalInvoice;
            toObject.RECEIVEDINVOICEEMAIL = fromObject.ReceivedInvoiceEmail;
            if (toObject.ISORG == false)
            {
                toObject.PERSONCONTACT = fromObject.CustomerName;
                toObject.CUSTOMERNAME = null;
            }
        }

        public static void CopyData(this CLIENT toObject, DataRow fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.TAXCODE = fromObject[ImportClient.TaxCode].ToString();
            toObject.CUSTOMERNAME = fromObject[ImportClient.CustomerName].ToString();
            toObject.CUSTOMERCODE = fromObject[ImportClient.CustomerCode].ToString();
            toObject.ADDRESS = fromObject[ImportClient.Address].ToString();
            toObject.FAX = fromObject[ImportClient.Fax].ToString();
            toObject.TYPESENDINVOICE = fromObject[ImportClient.TypeReceptionInvoice].ToInt(0) > 2 ? 1 : fromObject[ImportClient.TypeReceptionInvoice].ToInt(0);
            toObject.MOBILE = fromObject[ImportClient.Mobile].ToString();
            toObject.EMAIL = fromObject[ImportClient.Email].ToString().ToLower();
            toObject.RECEIVEDINVOICEEMAIL = fromObject[ImportClient.Email].ToString().ToLower();
            toObject.BANKACCOUNT = fromObject[ImportClient.BankAccount].ToString();
            toObject.ACCOUNTHOLDER = fromObject[ImportClient.AccountHolder].ToString();
            toObject.BANKNAME = fromObject[ImportClient.BankName].ToString();
            toObject.DESCRIPTION = fromObject[ImportClient.Description].ToString();
            toObject.CUSTOMERTYPE = fromObject[ImportClient.CustomerType].ToInt(0) > 1 ? 0 : fromObject[ImportClient.CustomerType].ToInt(0);
            toObject.ISORG = fromObject[ImportClient.Organization].ToBoolean();

            if (toObject.ISORG == false)
            {
                toObject.PERSONCONTACT = toObject.CUSTOMERNAME;
            }
            else
            {
                toObject.PERSONCONTACT = fromObject[ImportClient.PersonContact].ToString();
            }
        }

        public static void CopyDataOfDataRow(this CLIENT toObject, DataRow fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.TAXCODE = fromObject[ImportInvoice.ClientTaxCode].ToString();
            toObject.CUSTOMERNAME = fromObject[ImportInvoice.CompanyName].ToString();
            toObject.PERSONCONTACT = fromObject[ImportInvoice.Client].ToString();
            toObject.ADDRESS = fromObject[ImportInvoice.ClientAddress].ToString();
            toObject.BANKACCOUNT = fromObject[ImportInvoice.ClientAccount].ToString();
            toObject.BANKNAME = fromObject[ImportInvoice.ClientBankName].ToString();
            toObject.EMAIL = fromObject[ImportInvoice.ClientEmail].ToString();
            toObject.RECEIVEDINVOICEEMAIL = fromObject[ImportInvoice.ClientEmail].ToString();
            toObject.MOBILE = fromObject[ImportInvoice.Phone].ToString();
            if (fromObject[ImportInvoice.InvoiceisOrg].Trim().ToString() == "x" || fromObject[ImportInvoice.InvoiceisOrg].Trim().ToString() == "X")
            {
                toObject.ISORG = true;
            }
            else
            {
                toObject.ISORG = false;
            }
        }

        public static void CopyData(this MYCOMPANY toObject, CompanyInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYNAME = fromObject.CompanyName;
            toObject.TAXCODE = fromObject.TaxCode;
            toObject.ADDRESS = fromObject.Address;
            toObject.TEL1 = fromObject.Tel;
            toObject.FAX = fromObject.Fax;
            toObject.EMAIL = fromObject.Email;
            toObject.PERSONCONTACT = fromObject.PersonContact;
            toObject.DELEGATE = fromObject.Delegate;
            toObject.BANKACCOUNT = fromObject.BankAccount;
            toObject.BANKNAME = fromObject.BankName;
            toObject.TAXDEPARTMENTID = fromObject.TaxDepartmentId;
            toObject.DESCRIPTION = fromObject.Description;
            toObject.ACCOUNTHOLDER = fromObject.AccountHolder;
            toObject.WEBSITE = fromObject.WebSite;
            toObject.OPENFILEWHENSIGN = fromObject.OpenFileWhenSign;
            toObject.REPORTWEBSITE = fromObject.ReportWebsite;
            toObject.REPORTTEL = fromObject.ReportTel;
            toObject.NUMBERFORMAT = "{0:#,##0.###}";
            toObject.CLIENTNUMBERFORMAT = "0,0";
            toObject.SIGNATUREFILENAME = fromObject.SignaturePicture;
        }

        public static void CopyDataOfDataRow(this MYCOMPANY toObject, DataRow fromObject)
        {
            if (fromObject == null)
            {
                return;
            }
            toObject.BRANCHID = fromObject[ImportCustomer.BrandId].ToString();
            toObject.COREBRID = long.Parse(fromObject[ImportCustomer.BrandId].ToString());
            toObject.COMPANYNAME = fromObject[ImportCustomer.CustomerName].ToString();
            toObject.TAXCODE = fromObject[ImportCustomer.TaxCode].ToString();
            toObject.LEVELCUSTOMER = fromObject[ImportCustomer.LevelCustomer].ToString();
            toObject.COMPANYID = fromObject[ImportCustomer.Company2].IsNullOrEmpty() ? null : fromObject[ImportCustomer.Company2].ToInt();
            toObject.ADDRESS = fromObject[ImportCustomer.Address].ToString();
            toObject.DELEGATE = fromObject[ImportCustomer.Delegate].ToString();
            toObject.POSITION = fromObject[ImportCustomer.Position].ToString();
            toObject.TEL1 = fromObject[ImportCustomer.Tel].ToString();
            toObject.FAX = fromObject[ImportCustomer.Fax].ToString();
            toObject.EMAIL = fromObject[ImportCustomer.Email].ToString();
            toObject.WEBSITE = fromObject[ImportCustomer.Website].ToString();
            toObject.BANKACCOUNT = fromObject[ImportCustomer.BankAccount].ToString();
            toObject.ACCOUNTHOLDER = fromObject[ImportCustomer.AccountHolder].ToString();
            toObject.BANKNAME = fromObject[ImportCustomer.BankName].ToString();
            toObject.CITYID = fromObject[ImportCustomer.City].IsNullOrEmpty() ? null : fromObject[ImportCustomer.City].ToInt();
            toObject.TAXDEPARTMENTID = fromObject[ImportCustomer.TaxDepartment].IsNullOrEmpty() ? null : fromObject[ImportCustomer.TaxDepartment].ToInt();
            toObject.ACTIVE = fromObject[ImportCustomer.Active].ToBoolean();
            toObject.PERSONCONTACT = fromObject[ImportCustomer.PersonContact].ToString();
            toObject.MOBILE = fromObject[ImportCustomer.Mobile].ToString();
            toObject.EMAILOFCONTRACT = fromObject[ImportCustomer.EmailOfContact].ToString();

        }

        public static void CopyData(this MYCOMPANY toObject, MyCompanyInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYNAME = fromObject.CompanyName;
            toObject.TAXCODE = fromObject.TaxCode;
            toObject.ADDRESS = fromObject.Address;
            toObject.TEL1 = fromObject.Tel;
            toObject.FAX = fromObject.Fax;
            toObject.EMAIL = fromObject.Email;
            toObject.PERSONCONTACT = fromObject.PersonContact;
            toObject.DELEGATE = fromObject.Delegate;
            toObject.BANKACCOUNT = fromObject.BankAccount;
            toObject.BANKNAME = fromObject.BankName;
            toObject.TAXDEPARTMENTID = fromObject.TaxDepartmentId;
            toObject.DESCRIPTION = fromObject.Description;
            toObject.ACCOUNTHOLDER = fromObject.AccountHolder;
            toObject.WEBSITE = fromObject.WebSite;
            toObject.OPENFILEWHENSIGN = fromObject.OpenFileWhenSign;
            toObject.REPORTTEL = fromObject.ReportTel;
            toObject.REPORTWEBSITE = fromObject.ReportWebsite;
            toObject.NUMBERFORMAT = "{0:#,##0.###}";
            toObject.CLIENTNUMBERFORMAT = "0,0";
        }

        public static void CopyData(this PRODUCT toObject, ProductInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.PRODUCTCODE = fromObject.ProductCode;
            toObject.PRODUCTNAME = fromObject.ProductName;
            toObject.PRICE = fromObject.Price;
            toObject.UNIT = fromObject.UnitName;
            toObject.DESCRIPTION = fromObject.Description;
            toObject.TAXID = fromObject.TaxId;
            toObject.UNITID = fromObject.UnitId;
        }

        public static void CopyData(this PRODUCT toObject, PRODUCT fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.PRODUCTNAME = fromObject.PRODUCTNAME;
            toObject.PRICE = fromObject.PRICE;
            toObject.UNIT = fromObject.UNIT;
            toObject.DESCRIPTION = fromObject.DESCRIPTION;
            toObject.TAXID = fromObject.TAXID;
            toObject.UNITID = fromObject.UNITID;
        }

        public static void CopyData(this PRODUCT toObject, DataRow fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.PRODUCTCODE = fromObject[ImportProduct.ProductCode].ToString();
            toObject.PRODUCTNAME = fromObject[ImportProduct.ProductName].ToString();
            toObject.PRICE = fromObject[ImportProduct.Price].ToDecimal();
            toObject.UNIT = fromObject[ImportProduct.Unit].ToString();
            toObject.TAXID = fromObject[ImportProduct.TaxId].ToInt();
            toObject.DESCRIPTION = fromObject[ImportProduct.Description].ToString();
        }

        public static void CopyData(this INVOICEDETAIL toObject, DataRow fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.QUANTITY = fromObject[ImportInvoice.InvoiceDetailQuantity].ToInt();
            toObject.PRICE = fromObject[ImportInvoice.InvoiceDetailPrice].ToDecimal();
            toObject.TOTAL = fromObject[ImportInvoice.InvoiceDetailTotal].ToDecimal();
        }

        public static void CopyData(this PRODUCT toObject, InvoiceDetailInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.PRODUCTNAME = fromObject.ProductName;
            toObject.PRICE = fromObject.Price;
        }


        public static void CopyData(this INVOICERELEAS toObject, InvoiceReleasesInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYID = fromObject.CompanyId;
            toObject.CODE = fromObject.Code;
            toObject.MANAGER = fromObject.Manager;
            toObject.PERSIONSUGGEST = fromObject.PersonSuggest;
            toObject.NOTE = fromObject.Note;
            toObject.NOTE1 = fromObject.Note1;
            toObject.NOTE2 = fromObject.Note2;
            toObject.NOTE3 = fromObject.Note3;
            toObject.NOTE4 = fromObject.Note4;
            toObject.RECIPIENTS = fromObject.Recipients;
            toObject.RELEASEDDATE = fromObject.ReleasedDate;
            toObject.CITYID = fromObject.CityId;
        }

        public static void CopyData(this INVOICERELEASESDETAIL toObject, InvoiceReleasesDetailInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }
            toObject.DESCRIPTION = fromObject.Description;
            toObject.SYMBOL = fromObject.Symbol;
        }

        public static void CopyData(this NOTIFICATIONUSEINVOICE toObject, UseInvoiceInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYID = fromObject.CompanyId;
            toObject.TAXDEPARTMENTID = fromObject.TaxDepartmentId;
            toObject.CITYID = fromObject.CityId;
            toObject.DESCRIPTION = fromObject.Description;
        }

        public static void CopyData(this NOTIFICATIONUSEINVOICEDETAIL toObject, UseInvoiceDetailInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }
            toObject.REGISTERTEMPLATESID = fromObject.RegisterTemplateId;
            toObject.CODE = fromObject.Code;
            toObject.NUMBERUSE = fromObject.NumberUse;
            toObject.NUMBERFROM = fromObject.NumberFrom;
            toObject.NUMBERTO = fromObject.NumberTo;
            toObject.PREFIX = fromObject.Prefix;
            toObject.SUFFIX = fromObject.Suffix;
            toObject.USEDDATE = fromObject.UsedDate;
            toObject.COMPANYID = fromObject.CompanyId;
        }

        public static void CopyData(this INVOICE toObject, InvoiceInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYID = fromObject.CompanyId.Value;
            toObject.COMPANYNAME = fromObject.CompanyNameInvoice;
            toObject.COMPANYTAXCODE = fromObject.CompanyTaxCode;
            toObject.COMPANYADDRESS = fromObject.CompanyAddress;
            toObject.CLIENTID = fromObject.ClientId;
            toObject.SYMBOL = fromObject.Symbol;
            toObject.REGISTERTEMPLATEID = fromObject.RegisterTemplateId;
            toObject.CODE = fromObject.InvoiceCode;
            toObject.TYPEPAYMENT = fromObject.TypePayment;
            toObject.TOTAL = fromObject.Total;
            toObject.TOTALTAX = fromObject.TotalTax;
            toObject.TOTALDISCOUNTTAX = fromObject.TotalDiscountTax;
            toObject.TOTALDISCOUNT = fromObject.TotalDiscount;
            toObject.ISDISCOUNT = fromObject.IsDiscount;
            toObject.SUM = fromObject.Sum;
            toObject.NOTE = fromObject.Note;
            toObject.CUSTOMERNAME = fromObject.CompanyName;
            toObject.PERSONCONTACT = fromObject.PersonContact;
            toObject.CUSTOMERADDRESS = fromObject.Address;
            toObject.CUSTOMERTAXCODE = fromObject.TaxCode;
            toObject.CUSTOMERBANKACC = fromObject.BankAccount;
            toObject.CURRENCYID = fromObject.CurrencyId;
            toObject.CURRENCYEXCHANGERATE = fromObject.ExchangeRate;
            toObject.HTHDON = fromObject.HTHDon;
            toObject.DECLAREID = fromObject.DeclareId;
            toObject.VAT_INVOICE_TYPE = fromObject.Vat_invoice_type;
            toObject.REPORT_CLASS = fromObject.Report_Class;
            toObject.FEE_BEN_OUR = fromObject.Fee_ben_our;
            if (fromObject.InvoiceType == 3)
            {
                toObject.INVOICETYPE = fromObject.InvoiceType;
                toObject.PARENTID = fromObject.Id;
            }

            toObject.REFNUMBER = fromObject.InvoiceDetail[0].RefNumber;
        }

        public static void CopyData(this INVOICEDETAIL toObject, InvoiceDetailInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.PRODUCTID = fromObject.ProductId;
            toObject.PRICE = fromObject.Price;
            toObject.TAXID = fromObject.TaxId;
            toObject.TOTAL = fromObject.Total;
            toObject.SUM = fromObject.Sum;
            toObject.AMOUNTTAX = fromObject.AmountTax;
            toObject.ADJUSTMENTTYPE = fromObject.AdjustmentType;
            toObject.PRODUCTNAME = fromObject.ProductName;
            // đợi confirm BIDC
            toObject.QUANTITY = fromObject.Quantity;
            toObject.UNITNAME = fromObject.UnitName;
        }

        public static void CopyData(this CLIENT toObject, InvoiceInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.TAXCODE = fromObject.TaxCode;
            toObject.CUSTOMERNAME = fromObject.CompanyName;
            toObject.ADDRESS = fromObject.Address;
            toObject.MOBILE = fromObject.Mobile;
            toObject.PERSONCONTACT = fromObject.PersonContact;
            // truong hop tao moi hoa don va chua ton tai client trong DB
            toObject.RECEIVEDINVOICEEMAIL = fromObject.Email;
            toObject.ISORG = fromObject.IsOrg;
        }

        public static void CopyData(this REGISTERTEMPLATE toObject, RegisterTemplateInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYID = fromObject.CompanyId;
            toObject.INVOICESAMPLEID = fromObject.InvoiceSampleId;
            toObject.DESCRIPTION = fromObject.Description;
            toObject.INVOICETEMPLATEID = fromObject.InvoiceTemplateId;
            toObject.PREFIX = fromObject.Prefix;
            toObject.SUFFIX = fromObject.Suffix;
            toObject.CODE = fromObject.Name;
        }

        public static void CopyData(this RELEASEINVOICE toObject, ReleaseGroupInvoice fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYID = fromObject.CompanyId;
            toObject.DESCRIPTION = fromObject.Description;
        }

        public static void CopyData(this RELEASEINVOICE toObject, ReleaseInvoiceMaster fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYID = fromObject.CompanyId;
            toObject.RELEASEDDATE = fromObject.ReleasedDate;
            toObject.DESCRIPTION = fromObject.Description;
        }

        public static void CopyData(this RELEASEANNOUNCEMENT toObject, ReleaseAnnouncementMaster fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYID = fromObject.CompanyId;
            toObject.RELEASEDDATE = fromObject.ReleasedDate;
            toObject.DESCRIPTION = fromObject.Description;
        }

        public static void CopyData(this CANCELLINGINVOICE toObject, CancellingInvoiceMaster fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.CANCELLEDDATE = fromObject.CancellingDate;
            toObject.DESCRIPTION = fromObject.Description;
        }

        public static void CopyData(this KEYSTORE toObject, KeyStoreOfCompany fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYID = fromObject.CompanyId;
            toObject.SERIALNUMBER = fromObject.SerialNumber;
            toObject.TYPE = fromObject.Type;
        }

        public static void CopyData(this MYCOMPANY toObject, CustomerInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYNAME = fromObject.CompanyName;
            toObject.TAXCODE = fromObject.TaxCode;
            toObject.ADDRESS = fromObject.Address;
            toObject.TEL1 = fromObject.Tel;
            toObject.FAX = fromObject.Fax;
            toObject.EMAIL = fromObject.Email;
            toObject.PERSONCONTACT = fromObject.PersonContact;
            toObject.DELEGATE = fromObject.Delegate;
            toObject.BANKACCOUNT = fromObject.BankAccount;
            toObject.BANKNAME = fromObject.BankName;
            toObject.ACCOUNTHOLDER = fromObject.AccountHolder;
            toObject.MOBILE = fromObject.Mobile;
            toObject.POSITION = fromObject.Position;
            toObject.CITYID = fromObject.CityId;
            toObject.TAXDEPARTMENTID = fromObject.TaxDepartmentId;
            toObject.WEBSITE = fromObject.WebSite;
            toObject.OPENFILEWHENSIGN = fromObject.OpenFileWhenSign;
            toObject.EMAILOFCONTRACT = fromObject.EmailContract;
            toObject.ACTIVE = fromObject.Active;
            toObject.LEVELCUSTOMER = fromObject.LevelCustomer;
            toObject.BRANCHID = fromObject.BranchId;
            if (fromObject.CompanyId == 0)
            {
                toObject.COMPANYID = null;
            }
            else
            {
                toObject.COMPANYID = fromObject.CompanyId;
            }
        }
        public static void CopyData(this CONFIGEMAILSERVER toObject, EmailServerInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.AUTOSENDEMAIL = fromObject.AutoSendEmail;
            toObject.METHODSENDSSL = fromObject.MethodSendSSL;
            toObject.SMTPSERVER = fromObject.SMTPServer;
            toObject.PORT = fromObject.Port;
            toObject.EMAILSERVER = fromObject.EmailServer;
            toObject.SECURESOCKETOPTIONS = fromObject.SecureSocketOptions;
            toObject.USERNAME = fromObject.UserName;
            toObject.PASSWORD = fromObject.Password;
        }

        public static void CopyData(this LOGINUSER toObject, ContractAccount fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.USERID = fromObject.UserId;
            toObject.USERNAME = fromObject.UserName;
            toObject.COMPANYID = fromObject.CompanyId;
            toObject.PASSWORD = fromObject.Password;
            toObject.EMAIL = fromObject.Email;
            toObject.PASSWORDSTATUS = fromObject.PassWordStatus;
        }

        public static void CopyData(this EMAILACTIVE toObject, EmailInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }
            if (fromObject.EmailTo.IsNotNullOrEmpty())
            {
                var listEmail = fromObject.EmailTo.Split(',').Select(x => x.Trim()).Distinct().ToList();
                toObject.EMAILTO = string.Join(",", listEmail);
            }
            toObject.TITLE = fromObject.Subject;
            toObject.CONTENTEMAIL = fromObject.Content;
            toObject.RefIds = fromObject.RefIds; // Task #30077 // toObject.RefIds: thuộc tính của partial class, không phải entity trong DB
            toObject.COMPANYID = fromObject.CompanyId;
        }

        public static void CopyDataOfDataRow(this INVOICEDETAIL toObject, DataRow fromObject, bool isDiscountTemplate)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.QUANTITY = fromObject[ImportInvoice.InvoiceDetailQuantity].ToInt();
            toObject.PRICE = fromObject[ImportInvoice.InvoiceDetailPrice].ToDecimal();
            if (isDiscountTemplate)
            {
                var discount = fromObject[ImportInvoice.DiscountPercent].Trim().ToLower();
                if (discount != "")
                {
                    var discountPercentText = discount.TrimEnd('%');
                    int discountPercent = 0;
                    if (int.TryParse(discountPercentText, out discountPercent))
                    {
                        toObject.DISCOUNTRATIO = discountPercent;
                    }
                }
            }
            else
            {
                if (fromObject[ImportInvoice.InvoiceIsDiscount].Trim() == "x" || fromObject[ImportInvoice.InvoiceIsDiscount].Trim() == "X")
                {
                    toObject.DISCOUNT = true;
                }
                else
                {
                    toObject.DISCOUNT = false;
                }
            }
        }

        public static void CopyDataOfDataRow(this PRODUCT toObject, DataRow fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.PRODUCTNAME = fromObject[ImportInvoice.InvoiceDetailProductName].ToString();
            toObject.PRICE = fromObject[ImportInvoice.InvoiceDetailPrice].ToDecimal();
            toObject.UNIT = fromObject[ImportInvoice.InvoiceDetailUnit].ToString();
        }

        public static void CopyData(this REPORTCANCELLING toObject, ReportCancellingInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYID = fromObject.CompanyId;
            toObject.TYPECANCELLING = fromObject.TypeCancelling;
            toObject.HOURCANCELLING = fromObject.HourCancelling;
            toObject.MINUTECANCELLING = fromObject.MinuteCancelling;
            toObject.CANCELLINGDATE = fromObject.CancellingDate;
            toObject.CREATEDDATE = fromObject.CreatedDate;
            toObject.CREATEDBY = fromObject.CreatedBy;
            toObject.DELEGATE = fromObject.Delegate;
        }

        public static void CopyData(this REPORTCANCELLINGDETAIL toObject, RegisterTemplateCancelling fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.REGISTERTEMPLATESID = fromObject.RegisterTemplatesId;
            toObject.CODE = fromObject.Code;
            toObject.SYMBOL = fromObject.Symbol;
            toObject.NUMBERFROM = fromObject.NumberFrom;
            toObject.NUMBERTO = fromObject.NumberTo;
            toObject.NUMBER = fromObject.Number;
        }

        public static void CopyData(this EMAILACTIVEFILEATTACH toObject, ExportFileInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.FILENAME = fromObject.FileName;
            toObject.FULLPATHFILEATTACH = fromObject.FullPathFileName;
        }

        public static void CopyData(this SYSTEMSETTING toObject, SystemSettingInfo fromObject)
        {
            toObject.AMOUNT = fromObject.Amount;
            toObject.CONVERTEDAMOUNT = fromObject.ConvertedAmount;
            toObject.EXCHANGERATE = fromObject.ExchangeRate;
            toObject.NUMBERS = fromObject.Numbers;
            toObject.PRICE = fromObject.Price;
            toObject.QUANTITY = fromObject.Quantity;
            toObject.RATIO = fromObject.Ratio;
            toObject.STEPTOCREATEINVOICENO = fromObject.StepToCreateInvoiceNo;
            toObject.SIGNAFTERAPPROVE = fromObject.SignAfterApprove;
            toObject.SERVERSIGN = fromObject.ServerSign;
            toObject.ROWNUMBER = fromObject.RowNumber;
            toObject.ISWARNINGMINIMUM = fromObject.IsWarningMinimum;
            toObject.EMAILRECEIVEWARNING = fromObject.EmailReceiveWarning;
            toObject.NUMBERMINIMUM = fromObject.NumberMinimum;
        }

        public static void CopyData(this SIGNDETAIL toObject, SignDetailInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.INVOICEID = fromObject.InvoiceId;
            toObject.SERIALNUMBER = fromObject.SerialNumber;
            toObject.NAME = fromObject.Name;
            toObject.ISCLIENTSIGN = fromObject.IsClientSign;
            toObject.CREATEDDATE = fromObject.CreatedDate;
            toObject.ANNOUNCEMENTID = fromObject.AnnouncementId;
            toObject.TYPESIGN = fromObject.TypeSign;
        }

        public static void CopyData(this MYCOMPANYUPGRADE toObject, CompanyUpgradeInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.COMPANYSID = fromObject.CompanySid;
            toObject.TYPE = fromObject.Type;
            toObject.COMPANYID = fromObject.CompanyId;
            toObject.APPLYDATE = fromObject.ApplyDate;
            toObject.TAXCODE = fromObject.TaxCode;
            toObject.COMPANYNAME = fromObject.CompanyName;
            toObject.ADDRESS = fromObject.Address;
            toObject.DELEGATE = fromObject.Delegate;
            toObject.TEL = fromObject.Tel;
        }
        public static void CopyData(this INVOICEDECLARATION toObject, DeclarationInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }

            toObject.ID = fromObject.ID;
            toObject.DECLARATIONTYPE = fromObject.DeclarationType;
            toObject.COMPANYNAME = fromObject.CompanyName;
            toObject.COMPANYTAXCODE = fromObject.CompanyTaxCode;
            toObject.TAXDEPARTMENTID = fromObject.TaxDepartmentId;
            toObject.CUSTNAME = fromObject.CustName;
            toObject.CUSTADDRESS = fromObject.CustAddress;
            toObject.CUSTEMAIL = fromObject.CustEmail;
            toObject.CUSTPHONE = fromObject.CustPhone;
            toObject.CITYID = fromObject.CityId;
            toObject.DECLARATIONDATE = fromObject.DeclarationDate;
            toObject.HTHDON = fromObject.HTHDon;
            toObject.HTGDLHDDT = fromObject.HTGDLHDDT;
            toObject.PTHUC = fromObject.PThuc;
            toObject.LHDSDUNG = fromObject.LHDSDung;
            toObject.TYPE = fromObject.Type;
            toObject.SYMBOL = fromObject.Symbol;
            toObject.UPDATEDATE = fromObject.UpdateDate;
            toObject.STATUS = fromObject.Status;
            toObject.UPDATEBY = fromObject.UpdateBy;
            toObject.COMPANYID = fromObject.CompanyID;
            toObject.REGISTERTEMPLATEID = fromObject.RegisterTemplateID;
        }
        public static void CopyData(this INVOICEDECLARATIONRELEASE toObject, DeclarationReleaseInfo fromObject)
        {
            if (fromObject == null)
            {
                return;
            }
            toObject.ID = fromObject.Id;
            toObject.RELEASECOMPANYNAME = fromObject.ReleaseCompanyName;
            toObject.SERI = fromObject.Seri;
            toObject.FROMDATE = fromObject.fromDate;
            toObject.TODATE = fromObject.toDate;
            toObject.REGISTERTYPEID = fromObject.RegisterTypeID;
        }

        public static void CopyData(this SYMBOL toObject, CompanySymbolInfo fromObject,long declarationId)
        {
            if (fromObject == null)
            {
                return;
            }
            toObject.REFID = declarationId;
            toObject.COMPANYID = fromObject.CompanyId;
            toObject.SYMBOL1 = fromObject.Symbol;
        }
    }
}
