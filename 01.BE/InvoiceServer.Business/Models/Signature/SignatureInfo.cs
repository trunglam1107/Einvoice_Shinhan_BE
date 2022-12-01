using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class SignatureInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }
        [StringTrimAttribute]
        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("serialNumber")]
        [DataConvert("SerialNumber")]
        public string SerialNumber { get; set; }

        [StringTrimAttribute]
        [JsonProperty("certSerialNumber")]
        [DataConvert("CertSerialNumber")]
        public string CertSerialNumber { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyName")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }

        [JsonProperty("fromDate")]
        [DataConvert("FROMDATE")]
        public DateTime? FromDate { get; set; }

        [JsonProperty("toDate")]
        [DataConvert("TODATE")]
        public DateTime? ToDate { get; set; }

        [JsonProperty("password")]
        [DataConvert("Password")]
        public string Password { get; set; }

        [JsonProperty("typeSign")]
        [DataConvert("TypeSign")]
        public int? TypeSign { get; set; }

        [JsonProperty("slot")]
        [DataConvert("Slots")]
        public int? Slot { get; set; }

        [StringTrimAttribute]
        [JsonProperty("releaseName")]
        [DataConvert("ReleaseName")]
        public string ReleaseName { get; set; }
        public SignatureInfo(object srcObject)
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, SignatureInfo>(srcObject, this);
            }
        }

        public SignatureInfo()
        {
        }
    }

    public class SmallSignatureInfor
    {
        public string SerialNumber { get; set; }
        public string CompanyName { get; set; }
        public long CompanyId { get; set; }

        public SmallSignatureInfor() { }
    }

    public class SignCert
    {
        public string issuer { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string SerialNumber { get; set; }

    }
    public class CertInfo
    {
        public string EffectiveDate { get; set; }
        public string ExpirationDate { get; set; }
        public string ReleaseName { get; set; }
        public string SerialNumber { get; set; }
    }
}
