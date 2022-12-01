using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ExternalCustomer2
    {
        [JsonProperty("d")]
        public List<ExternalCustomer> d { get; set; }
    }
    public class ExternalCustomer
    {
        [JsonProperty("Enterprise_Gdt_Code")]
        public string Enterprise_Gdt_Code { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Name_F")]
        public string Name_F { get; set; }

        [JsonProperty("Ho_Address")]
        public string Ho_Address { get; set; }

        [JsonProperty("Legal_First_Name")]
        public string Legal_First_Name { get; set; }
        //[JsonProperty("MaSoThue")]
        public string MaSoThue { get; set; }

        //[JsonProperty("Title")]
        [DataConvert("Title")]
        public string Title { get; set; }

        //[JsonProperty("TitleEn")]
        [DataConvert("TitleEn")]
        public string TitleEn { get; set; }

        //[JsonProperty("DiaChiCongTy")]
        [DataConvert("DiaChiCongTy")]
        public string DiaChiCongTy { get; set; }

        //[JsonProperty("GiamDoc")]
        [DataConvert("GiamDoc")]
        public string GiamDoc { get; set; }
        //[JsonProperty("NoiNopThue_DienThoai")]
        public string NoiNopThue_DienThoai { get; set; }

        //[JsonProperty("NoiNopThue_Fax")]
        public string NoiNopThue_Fax { get; set; }

        public ExternalCustomer()
        {

        }

    }
}
