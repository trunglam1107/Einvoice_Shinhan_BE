using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class AutoNumberViewModel
    {
        [JsonProperty("TYPEID")]
        [DataConvert("TYPEID")]
        public string TYPEID { get; set; }

        [JsonProperty("TYPENAME")]
        [DataConvert("TYPENAME")]
        public string TYPENAME { get; set; }

        [JsonProperty("ENABLED1")]
        [DataConvert("ENABLED1")]
        public bool ENABLED1 { get; set; }

        [JsonProperty("ENABLED2")]
        [DataConvert("ENABLED2")]
        public bool ENABLED2 { get; set; }

        [JsonProperty("ENABLED3")]
        [DataConvert("ENABLED3")]
        public bool ENABLED3 { get; set; }
        [JsonProperty("ENABLED4")]
        [DataConvert("ENABLED4")]
        public bool ENABLED4 { get; set; }

        [JsonProperty("S1TYPE")]
        [DataConvert("S1TYPE")]
        public byte? S1TYPE { get; set; }

        [JsonProperty("S2TYPE")]
        [DataConvert("S2TYPE")]
        public byte? S2TYPE { get; set; }

        [JsonProperty("S3TYPE")]
        [DataConvert("S3TYPE")]
        public byte? S3TYPE { get; set; }
        [JsonProperty("S4TYPE")]
        [DataConvert("S4TYPE")]
        public byte? S4TYPE { get; set; }

        [JsonProperty("S1")]
        [DataConvert("S1")]
        public string S1 { get; set; }

        [JsonProperty("S2")]
        [DataConvert("S2")]
        public string S2 { get; set; }

        [JsonProperty("S3")]
        [DataConvert("S3")]
        public string S3 { get; set; }
        [JsonProperty("S4")]
        [DataConvert("S4")]
        public string S4 { get; set; }
        // ---
        [JsonProperty("SEPARATED")]
        [DataConvert("SEPARATED")]
        public bool SEPARATED { get; set; }


        [JsonProperty("SEPARATORS")]
        [DataConvert("SEPARATORS")]
        public string SEPARATORS { get; set; }

        [JsonProperty("OUTPUTLENGTH")]
        [DataConvert("OUTPUTLENGTH")]
        public byte? OUTPUTLENGTH { get; set; }

        [JsonProperty("OUTPUTORDERS")]
        [DataConvert("OUTPUTORDERS")]
        public byte? OUTPUTORDERS { get; set; }

        [JsonProperty("ORDERBY")]
        [DataConvert("ORDERBY")]
        public int ORDERBY { get; set; }

        [JsonProperty("ACTIVED")]
        [DataConvert("ACTIVED")]
        public bool ACTIVED { get; set; }

        [JsonProperty("CREATEUSERSID")]
        [DataConvert("CREATEUSERSID")]
        public long CREATEUSERSID { get; set; }

        [JsonProperty("CREATEDATE")]
        [DataConvert("CREATEDATE")]
        public DateTime CREATEDATE { get; set; }

        [JsonProperty("UPDATEUSERSID")]
        [DataConvert("UPDATEUSERSID")]
        public long UPDATEUSERSID { get; set; }

        [JsonProperty("UPDATEDATE")]
        [DataConvert("UPDATEDATE")]
        public DateTime UPDATEDATE { get; set; }
    }
}
