using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class RegisterTemplateCancelling
    {
        [StringTrimAttribute]
        [JsonIgnore()]
        public long? CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonIgnore()]
        public long ReportCancellingId { get; set; }


        [StringTrimAttribute]
        [JsonProperty("invoiceSample", NullValueHandling = NullValueHandling.Ignore)]
        [DataConvert("RegisterTemplate.InvoiceSample.Name", ThrowExceptionIfSourceNotExist = false)]
        public string InvoiceSample { get; set; }


        [StringTrimAttribute]
        [JsonProperty("invoiceSampleId", NullValueHandling = NullValueHandling.Ignore)]
        [DataConvert("RegisterTemplate.InvoiceSample.Id", ThrowExceptionIfSourceNotExist = false)]
        public string InvoiceSampleId { get; set; }


        [StringTrimAttribute]
        [JsonProperty("templateId")]
        [DataConvert("RegisterTemplatesId")]
        public long RegisterTemplatesId { get; set; }

        [JsonProperty("name")]
        public string NameDisplay
        {
            get
            {
                return string.Format("{0} - {1}({2} - {3})", this.Code, this.Symbol, this.NumberFrom, this.NumberTo);
            }
        }

        [StringTrimAttribute]
        [JsonProperty("symbol")]
        [DataConvert("Symbol")]
        public string Symbol { get; set; }

        [StringTrimAttribute]
        [JsonProperty("code")]
        [DataConvert("Code")]
        public string Code { get; set; }

        [JsonProperty("numberFrom")]
        [DataConvert("NumberFrom")]
        public string NumberFrom { get; set; }

        [JsonProperty("numberTo")]
        [DataConvert("NumberTo")]
        public string NumberTo { get; set; }

        [JsonProperty("number")]
        public decimal? Number
        {
            get
            {
                if (this.NumberFrom.IsNotNullOrEmpty() && this.NumberTo.IsNotNullOrEmpty())
                {
                    return (this.NumberTo.ToDecimal(0) - this.NumberFrom.ToDecimal(0) + 1);
                }
                else
                {
                    return 0;
                }
            }
        }

        public RegisterTemplateCancelling()
        {
        }

        public RegisterTemplateCancelling(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, RegisterTemplateCancelling>(srcObject, this);
            }

        }
    }
}
