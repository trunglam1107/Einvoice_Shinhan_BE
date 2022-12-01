using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceGift
    {
        [JsonProperty("id")]
        [DataConvert("ID")]
        public long Id { get; set; }
        [JsonProperty("invoiceId")]
        [DataConvert("INVOICEID")]
        public long? InvoiceId { get; set; }

        [JsonProperty("branchId")]
        [DataConvert("COMPANYID")]
        public long? CompanyId { get; set; }

        [JsonProperty("amount")]
        //[DataConvert("AMOUNT")]
        public decimal? Amount { get; set; }

        [JsonProperty("tradDate")]
        [DataConvert("CREATEDATE")]
        public string TradDate { get; set; }

        [JsonProperty("currency")]
        [DataConvert("CURRENCY")]
        public string Currency { get; set; }

        [JsonProperty("refNo")]
        [DataConvert("REFNO")]
        public string RefNo { get; set; }

        [JsonProperty("Sequence")]
        [DataConvert("SEQUENCE")]
        public string Sequence { get; set; }

        [JsonProperty("PeriodTax")]
        [DataConvert("PERIODTAX")]
        public string PeriodTax { get; set; }

        [JsonProperty("giftName")]
        [DataConvert("GIFTNAME")]
        public string GiftName { get; set; }

        [JsonProperty("Remark")]
        [DataConvert("REMARK")]
        public string Remark { get; set; }

        [JsonProperty("PreTaxAmount")]
        [DataConvert("PRETAXAMOUNT")]
        public decimal? PreTaxAmount { get; set; }

        [JsonProperty("Tax")]
        [DataConvert("TAX")]
        public string Tax { get; set; }

        [JsonProperty("taxAmount")]
        [DataConvert("AMOUNTTAX")]
        public decimal? TaxAmount { get; set; }

        [JsonProperty("TypeGift")]
        [DataConvert("TYPEGIFT")]
        public string TypeGift { get; set; }

        public InvoiceGift()
        {

        }
        public InvoiceGift(object srcObject)
           : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceGift>(srcObject, this);
            }
        }
    }
}
