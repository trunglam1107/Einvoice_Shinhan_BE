using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class InvoicePrintModel
    {
        public decimal ID { get; set; }
        public Nullable<decimal> PARENTID { get; set; }
        public decimal CLIENTID { get; set; }
        public decimal COMPANYID { get; set; }
        public decimal REGISTERTEMPLATEID { get; set; }
        public string SYMBOL { get; set; }
        public string NO { get; set; }
        public string NUMBERACCOUT { get; set; }
        public string CODE { get; set; }
        public Nullable<decimal> TYPEPAYMENT { get; set; }
        public long INVOICESTATUS { get; set; }
        public long INVOICETYPE { get; set; }
        public string NOTE { get; set; }
        public Nullable<bool> PAYMENTSTATUS { get; set; }
        public Nullable<decimal> TOTALTAX { get; set; }
        public Nullable<decimal> TOTAL { get; set; }
        public Nullable<decimal> SUM { get; set; }
        public Nullable<bool> DELETED { get; set; }
        public Nullable<System.DateTime> CREATEDDATE { get; set; }
        public Nullable<decimal> CREATEDBY { get; set; }
        public Nullable<decimal> UPDATEDBY { get; set; }
        public Nullable<System.DateTime> UPDATEDDATE { get; set; }
        public Nullable<decimal> DELETEDBY { get; set; }
        public Nullable<System.DateTime> APPROVEDDATE { get; set; }
        public Nullable<decimal> APPROVEDBY { get; set; }
        public string DESCRIPTIONPAYMENT { get; set; }
        //public Nullable<bool> RELEASED { get; set; }
        public Nullable<System.DateTime> RELEASEDDATE { get; set; }
        public decimal INVOICENO { get; set; }
        public string CUSTOMERNAME { get; set; }
        public Nullable<decimal> TOTALDISCOUNT { get; set; }
        public Nullable<bool> CLIENTSIGN { get; set; }
        public Nullable<decimal> TOTALDISCOUNTTAX { get; set; }
        public Nullable<long> ISDISCOUNT { get; set; }
        public string ORIGINNO { get; set; }
        public string PARENTCODE { get; set; }
        public string PARENTSYMBOL { get; set; }
        public string FILERECEIPT { get; set; }
        public Nullable<System.DateTime> CLIENTSIGNDATE { get; set; }
        public string PERSONCONTACT { get; set; }
        public string CUSTOMERTAXCODE { get; set; }
        public string CUSTOMERADDRESS { get; set; }
        public string CUSTOMERBANKACC { get; set; }
        public Nullable<decimal> CURRENCYID { get; set; }
        public Nullable<decimal> CURRENCYEXCHANGERATE { get; set; }
        //public Nullable<bool> SENDMAILSTATUS { get; set; }
        public string COMPANYNAME { get; set; }
        public string COMPANYADDRESS { get; set; }
        public string COMPANYTAXCODE { get; set; }
        public string REFNUMBER { get; set; }
        public string REFINVOICENO { get; set; }
        public string TELLER { get; set; }
        //public Nullable<bool> CREATEDFILE { get; set; }
        public Nullable<decimal> SIGNBY { get; set; }
        public Nullable<System.DateTime> DELETEDATE { get; set; }
        public Nullable<decimal> DELETEBY { get; set; }
        public Nullable<System.DateTime> CANCELDATE { get; set; }
        public Nullable<decimal> CANCELBY { get; set; }
        public Nullable<long> TYPEINVOICE { get; set; }
        public string LOANACNO { get; set; }
        public string VAT_INVOICE_TYPE { get; set; }
        public string REPORT_CLASS { get; set; }
        public string CUSTOMERCODE { get; set; }
        public Nullable<long> TYPESENDINVOICE { get; set; }
        public string MOBILE { get; set; }
        public string FAX { get; set; }
        public string EMAIL { get; set; }
        public string DELEGATE { get; set; }
        public string ACCOUNTHOLDER { get; set; }
        public Nullable<long> CUSTOMERTYPE { get; set; }
        public string ISORG { get; set; }
        public string USEREGISTEREMAIL { get; set; }
        public string TEL1 { get; set; }
        public string COMMOBILE { get; set; }
        public string RECEIVEDINVOICEEMAIL { get; set; }


        public string TAXNAME { get; set; }
        public decimal INVOICETEMPLATEID { get; set; }
        public string INVOICECODE { get; set; }
        public string TYPEPAYMENTNAME { get; set; }
        public string CURRENCYCODE { get; set; }
        public string CURRENCYNAME { get; set; }
        public string REPORTTEL { get; set; }
        public string REPORTWEBSITE { get; set; }
        public string VERIFICATIONCODE { get; set; }
        public string SIGNBYNAME { get; set; }
        public string DECIMALUNIT { get; set; }
        public string DECIMALSEPARATOR { get; set; }

        public long? STATUSNOTIFICATION { get; set; }
        public string MESSAGESCODE { get; set; }
        public long? HTHDON { get; set; }
        public string MCQT { get; set; }
        public Nullable<decimal> REPLACEDINVOICEID { get; set; }
        public string REPLACEDINVOICENO { get; set; }
        public string REPLACEDINVOICESYMBOL { get; set; }
        public DateTime REPLACEDINVOICERELEASEDDATE { get; set; }

        public long? SLOTS { get; set; }
        public string SERIALNUMBER { get; set; }
        public string PASSWORD { get; set; }
        public Nullable<decimal> IMPORTTYPE { get; set; }
        public ExportOption Option { get; set; }
        public InvoicePrintModel()
        {
            this.Option = new ExportOption();
        }
        public List<InvoiceItem> InvoiceItems { get; set; }
        public string MessageSwitchInvoice
        {
            get
            {
                return string.Empty;
            }

        }

        public string NoticeSwitchInvoice
        {
            get
            {
                return string.Empty;
            }

        }
        public string AmountInwords
        {
            get
            {
                if (this.SUM.HasValue)
                {
                    var amountInWords = "";
                    if (this.CURRENCYNAME != null)
                    {
                        amountInWords = ReadNumberToWord(this.SUM.Value.ToString(), CURRENCYNAME);
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
                string urlImageMark = GetImageMarkByStatus(this.INVOICESTATUS);
                return urlImageMark;
            }

        }
        private string ReadNumberToWord(string number, string currencyName)
        {
            if (string.IsNullOrEmpty(DECIMALSEPARATOR))
                DECIMALSEPARATOR = string.Empty;
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
                if (!string.IsNullOrEmpty(DECIMALUNIT))
                    res = numberToString + " " + currencyName + " " + DECIMALSEPARATOR + " " + decimalFractionToString + " " + DECIMALUNIT;
                else
                    res = numberToString + " " + DECIMALSEPARATOR + " " + decimalFractionToString + " " + currencyName;
            }
            else
            {
                res = numberToString + " " + currencyName;
            }

            return res;
        }
        private string GetImageMarkByStatus(long status)
        {
            string urlImageMark = string.Empty;
            switch (status)
            {
                case 5:
                    urlImageMark = "hoadon-huy.png";
                    break;
                default:
                    break;
            }

            return urlImageMark;
        }
        public string NoticeChangeInvoice
        {
            get
            {

                string prefixInvoiceSample = GetInvoiceSampleByCode(this.INVOICETYPE);
                if (!string.IsNullOrEmpty(prefixInvoiceSample))
                {
                    return string.Format(InvoicePrintInfoConst.NoticeChangeInvoice, prefixInvoiceSample,
                                         this.REPLACEDINVOICENO, this.REPLACEDINVOICESYMBOL,
                                         this.REPLACEDINVOICERELEASEDDATE.Day, this.REPLACEDINVOICERELEASEDDATE.Month, this.REPLACEDINVOICERELEASEDDATE.Year);
                }

                return string.Empty;
            }
        }
        private string GetInvoiceSampleByCode(long type)
        {
            string invoiceSample = string.Empty;
            switch (type)
            {
                //case (long)InvoiceType.AdjustmentInfomation:
                case 5:
                    invoiceSample = "Hóa đơn điều chỉnh thông tin cho hóa đơn điện tử số";
                    break;
                case 2:
                case 4:
                    invoiceSample = "Hóa đơn điều chỉnh tăng/giảm cho hóa đơn điện tử số";
                    break;
                case 1:
                    invoiceSample = "Hóa đơn thay thế cho hóa đơn điện tử số";
                    break;
                default:
                    break;
            }

            return invoiceSample;
        }
    }
}
