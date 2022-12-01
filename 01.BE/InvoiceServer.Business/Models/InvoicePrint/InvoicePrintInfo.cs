using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class InvoicePrintInfo
    {
        public long CompanyId { get; set; }
        public long Id { get; set; }

        public long? ParentId { get; set; }

        public string CompanyName { get; set; }

        public string TaxCode { get; set; }
        public string TaxCodeInvoice { get; set; }
        public string TaxCodeMyCompany { get; set; }

        public string Address { get; set; }

        public string Tel { get; set; }

        public string Mobile { get; set; }

        public string Fax { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        public string Email { get; set; }

        public string EmailContract { get; set; }

        public string BankAccount { get; set; }

        public string BankName { get; set; }

        public string Website { get; set; }

        [JsonProperty("code")]
        public string InvoiceCode { get; set; }

        public string TypePayment { get; set; }

        public string TypePaymentCode { get; set; }


        public string CustomerName { get; set; }
        public string CustomerNameInvoice { get; set; }
        public string CustomerNameClient { get; set; }
        public string CustomerCode { get; set; }

        public string CustomerCompanyName { get; set; }
        public string CustomerCompanyNameInvoice { get; set; }
        public string CustomerCompanyNameClient { get; set; }

        public string CustomerTaxCode { get; set; }
        public string CustomerTaxCodeInvoice { get; set; }
        public string CustomerTaxCodeClient { get; set; }

        public decimal? ExchangeRate { get; set; }

        public string CustomerAddress { get; set; }
        public string CustomerAddressInvoice { get; set; }
        public string CustomerAddressClient { get; set; }

        public string CustomerBankAccount { get; set; }
        public string CustomerBankAccountInvoice { get; set; }
        public string CustomerBankAccountClient { get; set; }

        public string CustomerBankName { get; set; }

        public string InvoiceNo { get; set; }
        public long? ReplacedInvoiceID { get; set; }
        public string ReplacedInvoiceCode { get; set; }
        public string ReplacedInvoiceNo { get; set; }
        public string ReplacedSymbol { get; set; }
        public DateTime ReplacedReleaseDate { get; set; }

        public decimal? Total { get; set; }
        public string Tax { get; set; }
        public string TaxDisplay { get; set; }
        public decimal? TaxAmout { get; set; }
        public decimal? TaxAmout5 { get; set; }
        public decimal? TaxAmout10 { get; set; }
        public decimal? Sum { get; set; }

        public int? IsDiscount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TotalDiscountTax { get; set; }

        public DateTime? DataInvoice { get; set; }
        public DateTime? DateRelease { get; set; }
        public string DateSign { get; set; }
        public string ClientSignDate { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public ExportOption Option { get; set; }

        public long? InvoiceSampleId { get; set; }
        public bool isMultiTax { get; set; }
        public string ReportWebsite { get; set; }
        public string ReportTel { get; set; }
        public string Signature { get; set; }
        public string ClientSignature { get; set; }
        public string Delegate { get; set; }
        public string PersonContact { get; set; }
        public long ClientId { get; set; }
        public string EmailClient { get; set; }
        public string MobileClient { get; set; }
        public bool? ClientSendByMonth { get; set; }
        public string CurrenCyName { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyDecimalUnit { get; set; }
        public string CurrencyDecimalSeparator { get; set; }
        [JsonProperty("isOrg")]
        public bool IsOrg { get; set; }
        [JsonProperty("statisticalSumAmount")]
        public decimal? StatisticalSumAmount { get; set; }

        [JsonProperty("statisticalSumTaxAmount")]
        public decimal? StatisticalSumTaxAmount { get; set; }

        [JsonProperty("giftSumAmount")]
        public decimal? GiftSumAmount { get; set; }

        [JsonProperty("giftSumTaxAmount")]
        public decimal? GiftSumTaxAmount { get; set; }
        public string InvoiceDate { get; set; }
        public string CompanyNameConfig { get; set; }
        public string TaxCodeConfig { get; set; }
        public string PhoneConfig { get; set; }
        public string CompanyLinkConfig { get; set; }
        public string CompanyLinkConfigShow { get; set; }
        public string PortalCompanyLinkConfig { get; set; }
        public string PortalCompanyLinkConfigShow { get; set; }


        [JsonProperty("pathImageLogo")]
        public string PathImageLogo { get; set; }

        [JsonProperty("pathImageBackground")]
        public string PathImageBackground { get; set; }

        [JsonProperty("templateName")]
        public string TemplateName { get; set; }

        [JsonProperty("canCreateAnnoun")]
        public bool CanCreateAnnoun { get; set; }
        public string RefNumber { get; set; }

        public string TaxCodeName { get; internal set; }

        [JsonProperty("importType")]
        public long? ImportType { get; set; }

        public string AmountInwords
        {
            get
            {
                if (this.Sum.HasValue)
                {
                    var amountInWords = "";
                    if (this.CurrenCyName != null)
                    {
                        amountInWords = ReadNumberToWord(this.Sum.Value.ToString(), CurrenCyName);
                    }
                    return amountInWords;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string MarkImage
        {
            get
            {
                string urlImageMark = GetImageMarkByStatus(this.Invoice_Status);
                return urlImageMark;
            }

        }

        public List<InvoiceItem> InvoiceItems { get; set; }

        public List<InvoiceStatistical> InvoiceStatisticals { get; set; }
        public List<InvoiceGift> InvoiceGifts { get; set; }
        public string NoticeChangeInvoice
        {
            get
            {

                string prefixInvoiceSample = GetInvoiceSampleByCode(this.InvoiceSample);
                if (prefixInvoiceSample.IsNotNullOrEmpty())
                {
                    return string.Format(InvoicePrintInfoConst.NoticeChangeInvoice, prefixInvoiceSample,
                                         this.ReplacedInvoiceNo, this.ReplacedSymbol,
                                         this.ReplacedReleaseDate.Day, this.ReplacedReleaseDate.Month, this.ReplacedReleaseDate.Year);
                }

                return string.Empty;
            }
        }
        public long RegisterTemplateId { get; set; }
        public long TemplateId { get; set; }

        [JsonProperty("status")]
        public int Invoice_Status { get; set; }


        public int InvoiceSample { get; set; }

        public bool IsInvoiceSwitch { get; set; }

        [JsonProperty("statusnotification")]
        public int? StatusNotification { get; set; }
        [JsonProperty("MessagesCode")]
        public string MessagesCode { get; set; }
        [JsonProperty("HTHDon")]
        public int? HTHDon { get; set; }

        [JsonProperty("MCQT")]
        public string MCQT { get; set; }

        public string MessageSwitchInvoice
        {
            get
            {
                if (!IsInvoiceSwitch)
                {
                    return string.Empty;
                }

                return Common.Constants.InvoicePrintInfoConst.MessageSwitchInvoice;
            }

        }

        public string NoticeSwitchInvoice
        {
            get
            {
                if (!IsInvoiceSwitch)
                {
                    return string.Empty;
                }

                return string.Format("<b>Ghi chú:</b> In chuyển đổi lúc <i> {0} Ngày {1}</i>;<b> Người chuyển đổi</b>  <i> (ký ghi rõ họ tên)</i> :", DateTime.Now.ToString("HH:mm"), DateTime.Now.ToString("dd/MM/yyyy"));
            }

        }

        public bool ClientSign { get; set; }
        public DateTime? DateClientSign { get; set; }

        public string VerificationCode { get; set; }

        public string LevelCustomer { get; set; }
        public string ParentCompanyName { get; set; }

        public string ParentCompanyAddress { get; set; }

        [JsonProperty("haveAdjustment")]
        public bool HaveAdjustment { get; set; }

        [JsonProperty("isChange")]
        public bool IsChange { get; set; }
        public int? InvoiceType { get; set; }
        public string InvoiceNote { get; set; }
        public INVOICE ParentInvoice { get; set; }

        public string Report_class { get; set; }

        [JsonProperty("Vat_invoice_type")]
        public string Vat_invoice_type { get; set; }

        public InvoicePrintInfo()
        {
            this.Option = new ExportOption();
        }
        public InvoicePrintInfo(INVOICE objInvoice, MYCOMPANY company, CLIENT client, CURRENCY currency, INVOICETEMPLATE template) 
        {
            this.Option = new ExportOption();
            this.CompanyId = objInvoice.COMPANYID;
            this.Id = objInvoice.ID;
            this.Total = objInvoice.TOTAL;
            this.TaxAmout = objInvoice.TOTALTAX;
            this.Sum = objInvoice.SUM;
            this.DataInvoice = objInvoice.CREATEDDATE.HasValue ? objInvoice.CREATEDDATE.Value : DateTime.Now;
            this.DateRelease = objInvoice.RELEASEDDATE;
            this.InvoiceNo = objInvoice.NO;
            bool invoiceStatus = objInvoice.INVOICESTATUS != (int)InvoiceStatus.New || (objInvoice.INVOICETYPE ?? 0) != 0 || (objInvoice.INVOICENO ?? 0) != 0;
            bool levelCustomer = company.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice;
            this.CompanyName = invoiceStatus ? objInvoice.COMPANYNAME : levelCustomer ? company.COMPANYNAME : company.COMPANYNAME;
            this.TaxCodeInvoice = objInvoice.COMPANYTAXCODE;
            this.TaxCodeMyCompany = company.TAXCODE;
            this.Address = invoiceStatus ? objInvoice.COMPANYADDRESS : levelCustomer ? company.ADDRESS : company.ADDRESS;
            this.Tel = company.TEL1;
            this.Fax = company.FAX;
            this.Symbol = objInvoice.SYMBOL;
            this.Mobile = company.MOBILE;
            EmailContract = company.EMAILOFCONTRACT;
            Email = company.EMAIL;
            BankAccount = company.BANKACCOUNT;
            BankName = company.BANKNAME;
            Website = company.WEBSITE;
            CustomerNameInvoice = objInvoice.PERSONCONTACT;
            bool isCurrentClient = OtherExtensions.IsCurrentClient();
            bool notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT; // không phải là khách hàng vãng lai
            bool isOrgCurrentClient = notCurrentClient==true ? client.ISORG.GetValueOrDefault(false) : objInvoice.CUSTOMERTAXCODE != null;
            CustomerNameClient = notCurrentClient ? client.PERSONCONTACT : objInvoice.PERSONCONTACT;
            CustomerCode = client.CUSTOMERCODE;
            CustomerCompanyNameInvoice = objInvoice.CUSTOMERNAME;
            CustomerCompanyNameClient = notCurrentClient ? client.CUSTOMERNAME : objInvoice.CUSTOMERNAME;
            CustomerTaxCodeInvoice = objInvoice.CUSTOMERTAXCODE;
            CustomerTaxCodeClient = notCurrentClient ? client.TAXCODE : objInvoice.CUSTOMERTAXCODE;
            CustomerAddressInvoice = objInvoice.CUSTOMERADDRESS;
            CustomerAddressClient = notCurrentClient ? client.ADDRESS : objInvoice.CUSTOMERADDRESS;
            CustomerBankAccountInvoice = objInvoice.CUSTOMERBANKACC;
            CustomerBankAccountClient = notCurrentClient ? client.BANKACCOUNT : objInvoice.CUSTOMERBANKACC;
            InvoiceSample = (objInvoice.INVOICETYPE ?? 0);          
            Invoice_Status = (objInvoice.INVOICESTATUS ?? 0);
            ReportWebsite = company.REPORTWEBSITE;
            ReportTel = company.REPORTTEL;
            DiscountAmount = objInvoice.TOTALDISCOUNT;
            TotalDiscountTax = objInvoice.TOTALDISCOUNTTAX;
            ClientSign = (objInvoice.CLIENTSIGN ?? false);
            DateClientSign = objInvoice.CLIENTSIGNDATE;
            
            IsDiscount = objInvoice.ISDISCOUNT;
            ParentId = (objInvoice.PARENTID ?? 0);
            EmailClient = client.EMAIL;
            MobileClient = client.MOBILE;
            ClientSendByMonth = client.SENDINVOICEBYMONTH;
            CurrenCyName = currency.NAME;
            CurrencyCode = currency.CODE;
            CurrencyDecimalUnit = currency.DECIMALUNIT;
            CurrencyDecimalSeparator = currency.DECIMALSEPARATOR;
            ExchangeRate = objInvoice.CURRENCYEXCHANGERATE;
            
            IsOrg = isOrgCurrentClient;
            ClientId = client.ID;

            TemplateName = template.URLFILE;
            TemplateId = template.ID;


            RefNumber = objInvoice.REFNUMBER ?? "";

            LevelCustomer = company.LEVELCUSTOMER;

            //VerificationCode = releaseDetail.VERIFICATIONCODE,

            HTHDon = objInvoice.HTHDON;
            //  MCQT = minvoice.MCQT,
            Report_class = objInvoice.REPORT_CLASS;

            Func<InvoicePrintInfo, string, string, string> getParams = (invPrint, value1, value2) =>
            {
                if ((invPrint.Invoice_Status != (int)InvoiceStatus.New && invPrint.Invoice_Status != 0 || invPrint.InvoiceNo != DefaultFields.INVOICE_NO_DEFAULT_VALUE)
                    || invPrint.InvoiceSample != 0)
                    return value1;
                return value2;
            };
            CustomerName = this.CustomerNameInvoice;
            TaxCode = getParams(this, this.TaxCodeInvoice, this.TaxCodeMyCompany);
            CustomerName = this.CustomerNameInvoice;//getParams(invoicePrintInfo, invoicePrintInfo.CustomerNameInvoice, invoicePrintInfo.CustomerNameClient);
            CustomerCompanyName = getParams(this, this.CustomerCompanyNameInvoice, this.CustomerCompanyNameClient);
            CustomerTaxCode = getParams(this, this.CustomerTaxCodeInvoice, this.CustomerTaxCodeClient);
            CustomerAddress = getParams(this, this.CustomerAddressInvoice, this.CustomerAddressClient);
            CustomerBankAccount = getParams(this, this.CustomerBankAccountInvoice, this.CustomerBankAccountClient);

           

        }


        public InvoicePrintInfo(MYCOMPANY companyInfo)
            : this()
        {
            this.InvoiceNo = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
            this.CompanyId = companyInfo.COMPANYSID;
            this.CompanyName = companyInfo.COMPANYNAME;
            this.TaxCode = companyInfo.TAXCODE;
            this.Address = companyInfo.ADDRESS;
            this.Tel = companyInfo.TEL1;
            this.Mobile = companyInfo.MOBILE;
            this.Fax = companyInfo.FAX;
            this.Email = companyInfo.EMAIL;
            this.EmailContract = companyInfo.EMAILOFCONTRACT;
            this.BankAccount = companyInfo.BANKACCOUNT;
            this.BankName = companyInfo.BANKNAME;
            this.Website = companyInfo.WEBSITE;
            this.DataInvoice = DateTime.Now;
            this.InvoiceItems = new List<InvoiceItem>();
            this.InvoiceStatisticals = new List<InvoiceStatistical>();
            this.InvoiceGifts = new List<InvoiceGift>();
            if (companyInfo.MYCOMPANY2 != null)
            {
                this.ReportTel = companyInfo.MYCOMPANY2.REPORTTEL;
                this.ReportWebsite = companyInfo.MYCOMPANY2.REPORTWEBSITE;

                if (companyInfo.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
                {
                    this.CompanyName = companyInfo.MYCOMPANY2.COMPANYNAME;
                    this.Address = companyInfo.MYCOMPANY2.ADDRESS;
                }
            }

        }

        private string ReadNumberToWord(string number, string currencyName)
        {
            if (string.IsNullOrEmpty(CurrencyDecimalSeparator))
                CurrencyDecimalSeparator = string.Empty;
            var split = number.Split('.');
            var numberToString = ReadNuberToText.ConvertNumberToStringInvoice(split[0]);
            var decimalFractionToString = "";
            bool hadDecimalPath = false;
            if (split.Length >= 2)
            {
                var split1 = split[1];
                if (split1.Length == 1)
                    split1 += "0";
                split1 = split1.TrimStart('0');
                if (split1.Length > 0)
                {
                    hadDecimalPath = true;
                    decimalFractionToString = ReadNuberToText.ConvertNumberToStringInvoice(split1).ToLower();
                }
            }

            var res = "";
            if (hadDecimalPath)
            {
                if (!string.IsNullOrEmpty(CurrencyDecimalUnit))
                    res = numberToString + " " + currencyName + " " + CurrencyDecimalSeparator + " " + decimalFractionToString + " " + CurrencyDecimalUnit;
                else
                    res = numberToString + " " + CurrencyDecimalSeparator + " " + decimalFractionToString + " " + currencyName;
            }
            else
            {
                res = numberToString + " " + currencyName;
            }

            return res;
        }
        private string GetInvoiceSampleByCode(int type)
        {
            string invoiceSample = string.Empty;
            switch (type)
            {
                //case (int)InvoiceType.AdjustmentInfomation:
                case (int)Common.Enums.InvoiceType.AdjustmentTaxCode:
                case (int)Common.Enums.InvoiceType.AdjustmentElse:
                    invoiceSample = "Hóa đơn điều chỉnh thông tin cho hóa đơn điện tử số";
                    break;
                case (int)Common.Enums.InvoiceType.AdjustmentUpDown:
                case (int)Common.Enums.InvoiceType.AdjustmentTax:
                    invoiceSample = "Hóa đơn điều chỉnh tăng/giảm cho hóa đơn điện tử số";
                    break;
                case (int)Common.Enums.InvoiceType.Substitute:
                    invoiceSample = "Hóa đơn thay thế cho hóa đơn điện tử số";
                    break;
                default:
                    break;
            }

            return invoiceSample;
        }

        private string GetImageMarkByStatus(int status)
        {
            string urlImageMark = string.Empty;
            switch (status)
            {
                case (int)InvoiceStatus.Draft:
                    urlImageMark = "hoadon-mau.png";
                    break;
                case (int)InvoiceStatus.Cancel:
                    urlImageMark = "hoadon-huy.png";
                    break;
                default:
                    break;
            }

            return urlImageMark;
        }
    }
}
