using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models.GatewayLog
{
    public class GatewaylogDetail
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        [DataConvert("Name")]
        public string Name { get; set; }

        [JsonProperty("body")]
        [DataConvert("Body")]
        public string Body { get; set; }

        [JsonProperty("objectName")]
        [DataConvert("ObjectName")]
        public string ObjectName { get; set; }

        [JsonProperty("ip")]
        [DataConvert("IP")]
        public string IP { get; set; }

        [JsonProperty("createDate")]
        [DataConvert("CreatedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? CreatedDate { get; set; }

        [JsonProperty("createBy")]
        [DataConvert("CreatedBy")]
        public DateTime? CreatedBy { get; set; }

        public GatewaylogDetail()
        {

        }

        public GatewaylogDetail(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, GatewaylogDetail>(srcObject, this);
            }
        }

    }
}
