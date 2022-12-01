using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class SystemSettingInfo
    {
        [JsonProperty("quantity")]
        [DataConvert("Quantity")]
        public int Quantity { get; set; }


        [JsonProperty("price")]
        [DataConvert("Price")]
        public int Price { get; set; }


        [JsonProperty("ratio")]
        [DataConvert("Ratio")]
        public int Ratio { get; set; }


        [JsonProperty("exchangeRate")]
        [DataConvert("ExchangeRate")]
        public int ExchangeRate { get; set; }


        [JsonProperty("amount")]
        [DataConvert("Amount")]
        public int Amount { get; set; }


        [JsonProperty("convertedAmount")]
        [DataConvert("ConvertedAmount")]
        public int ConvertedAmount { get; set; }


        [JsonProperty("numbers")]
        [DataConvert("Numbers")]
        public int Numbers { get; set; }

        [JsonProperty("stepToCreateInvoiceNo")]
        [DataConvert("StepToCreateInvoiceNo")]
        public int StepToCreateInvoiceNo { get; set; }

        [JsonProperty("signAfterApprove")]
        [DataConvert("SignAfterApprove")]
        public bool? SignAfterApprove { get; set; }

        [JsonProperty("serverSign")]
        [DataConvert("ServerSign")]
        public bool? ServerSign { get; set; }

        [JsonProperty("typeSign")]
        public string TypeSignServer { get; set; }

        [JsonProperty("fileCA")]
        public string FileCA { get; set; }

        [JsonProperty("rowNumber")]
        [DataConvert("RowNumber")]
        public int RowNumber { get; set; }
        [JsonProperty("listRowNumber")]
        public List<int> ListRowNumber { get; set; }

        [JsonProperty("isWarningMinimum")]
        [DataConvert("IsWarningMinimum")]
        public bool IsWarningMinimum { get; set; }

        [JsonProperty("emailReceiveWarning")]
        [DataConvert("EmailReceiveWarning")]
        public string EmailReceiveWarning { get; set; }

        [JsonProperty("numberMinimum")]
        [DataConvert("NumberMinimum")]
        public int NumberMinimum { get; set; }

        [JsonProperty("useSystemSettingVND")]
        public bool UseSystemSettingVND
        {
            get
            {
                return CommonUtil.GetConfig("UseSystemSettingVND", false);
            }
        }

        public SystemSettingInfo(object srcObject)
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, SystemSettingInfo>(srcObject, this);
            }
        }

        public SystemSettingInfo()
        {
            Quantity = Price = Ratio = ExchangeRate = Amount = ConvertedAmount = Numbers = 0;
        }
    }

}
