using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace InvoiceServer.Business.Models
{
    [XmlRoot("CTS")]
    public class DeclarationReleaseInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonProperty("releaseCompanyName")]
        [DataConvert("ReleaseCompanyName")]
        [XmlElement("TTChuc")]
        public string ReleaseCompanyName { get; set; }

        [JsonProperty("seri")]
        [DataConvert("Seri")]
        [XmlElement("Seri")]
        public string Seri { get; set; }

        [JsonProperty("fromDate")]
        [DataConvert("fromDate")]
        [XmlElement("TNgay")]
        [JsonConverter(typeof(JsonDateTimeConverterString))]

        public DateTime fromDate { get; set; }

        [JsonProperty("toDate")]
        [DataConvert("toDate")]
        [XmlElement("DNgay")]
        [JsonConverter(typeof(JsonDateTimeConverterString))]
        public DateTime toDate { get; set; }

        //[JsonProperty("releaseType")]
        //[DataConvert("ReleaseType")]
        //[XmlElement("HThuc")]
        //public int ReleaseType { get; set; }


        [JsonProperty("registerTypeID")]
        [DataConvert("RegisterTypeID")]
        [XmlElement("HThuc")]
        public int RegisterTypeID { get; set; }
        // 1. Thêm mới
        // 2. Gia hạn
        // 3. Ngừng sử dụng
        [JsonIgnore]
        [DataConvert("INVOICEDECLARATIONRELEASEREGISTERTYPE.NAME")]
        public string RegisterType { get; set; }


        public string FileName { get; set; }

        public string PassWord { get; set; }
        public DeclarationReleaseInfo()
        {
        }
        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public DeclarationReleaseInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, DeclarationReleaseInfo>(srcObject, this);
            }
        }
    }
}
