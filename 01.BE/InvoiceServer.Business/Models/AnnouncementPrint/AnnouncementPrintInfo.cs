using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.Models
{
    public class AnnouncementPrintInfo
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string LegalGrounds { get; set; }
        public DateTime? AnnouncementDate { get; set; }
        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyTel { get; set; }
        public string CompanyTaxCode { get; set; }
        public string CompanyRepresentative { get; set; }
        public string CompanyPosition { get; set; }
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public string ClientTel { get; set; }
        public string ClientTaxCode { get; set; }
        public string ClientRepresentative { get; set; }
        public string ClientPosition { get; set; }
        public long InvoiceId { get; set; }
        public long RegisterTemplateId { get; set; }
        public string Denominator { get; set; }
        public string Symbol { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string ServiceName { get; set; }
        public string Reasion { get; set; }
        public string BeforeContain { get; set; }
        public string ChangeContain { get; set; }
        public int? AnnouncementType { get; set; }
        public int? AnnouncementStatus { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public long? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public long? ProcessBy { get; set; }
        public DateTime? ProcessDate { get; set; }
        public string TaxCode { get; set; }

        public string Address { get; set; }

        public string Tel { get; set; }

        public string Mobile { get; set; }

        public string Fax { get; set; }

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

        public string CustomerCompanyName { get; set; }

        public string CustomerTaxCode { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerBankAccount { get; set; }



        public string ReplacedInvoiceCode { get; set; }
        public string ReplacedInvoiceNo { get; set; }


        public decimal? Total { get; set; }
        public string Tax { get; set; }
        public decimal? TaxAmout { get; set; }
        public decimal? Sum { get; set; }

        public decimal? DiscountAmount { get; set; }
        public decimal? TotalDiscountTax { get; set; }


        public DateTime? DateRelease { get; set; }
        public string DateSign { get; set; }
        public string ClientSignDate { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public ExportOption Option { get; set; }

        public string ReportWebsite { get; set; }
        public string ReportTel { get; set; }
        public string AmountInwords
        {
            get
            {
                if (this.Sum.HasValue)
                {
                    return ReadNumberToWord(this.Sum.Value.ToString());
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

        public long TemplateId { get; set; }

        [JsonProperty("status")]
        public int Invoice_Status { get; set; }


        public int InvoiceSample { get; set; }

        public bool IsInvoiceSwitch { get; set; }

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
        public AnnouncementPrintInfo()
        {
            this.Option = new ExportOption();
        }


        public AnnouncementPrintInfo(MYCOMPANY companyInfo)
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
            this.InvoiceDate = DateTime.Now;
            this.InvoiceItems = new List<InvoiceItem>();
            if (companyInfo.MYCOMPANY2 != null)
            {
                this.ReportTel = companyInfo.MYCOMPANY2.REPORTTEL;
                this.ReportWebsite = companyInfo.MYCOMPANY2.REPORTWEBSITE;
            }
        }

        private string ReadNumberToWord(string number)
        {
            var split = number.Split('.');
            var numberToString = ReadNuberToText.ConvertNumberToString(split[0]);
            if (split.Length >= 2)
            {
                var split1 = split[1].Trim('0');
                if (split1.Length > 0)
                {
                    var decimalFractionToString = ReadNuberToText.ConvertNumberToString(split1);
                    numberToString = numberToString.Replace("đồng.", "");
                    numberToString += decimalFractionToString.ToLower();
                }
            }
            if (!string.IsNullOrEmpty(numberToString))
            {
                numberToString = numberToString.First().ToString().ToUpper() + numberToString.Substring(1);
            }

            return numberToString;
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
