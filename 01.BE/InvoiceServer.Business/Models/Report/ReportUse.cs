using InvoiceServer.Common.Extensions;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ReportUse
    {
        [JsonIgnore]
        public long NotificationUseInvoiceDetailId { get; set; }

        [JsonProperty("invoiceSampleCode")] //2
        public string InvoiceSampleCode { get; set; }

        [JsonProperty("invoiceSample")] //2
        public string InvoiceSample { get; set; }

        [JsonProperty("code")] //3
        public string InvoiceCode { get; set; }

        [JsonProperty("symbol")] //4
        public string InvoiceSymbol { get; set; }

        [JsonProperty("dauKy_TongSo")] //5
        public long DauKy_Tong
        {
            get
            {
                if (this.SoDuHoaDonDauKy.IsNullOrEmpty())
                {
                    var soTonDauKy = this.RegistNumberTo.ToInt(0);
                    var soMua = (this.SoMua_DenSo.ToInt(0) - this.SoMua_TuSo.ToInt(0) + 1);
                    return this.SoMua_TuSo == null ? soTonDauKy : soTonDauKy + soMua;
                }
                else if (this.RegistNumberTo.IsNotNullOrEmpty())
                {
                    var soTonDauKy = this.RegistNumberTo.ToInt(0);
                    var soMua = (this.SoMua_DenSo.ToInt(0) - this.SoMua_TuSo.ToInt(0) + 1);
                    var result = this.SoMua_TuSo == null ? soTonDauKy : soTonDauKy + soMua;
                    return this.isReport ? (result - SoDuHoaDonDauKy.ToInt(0)) : result;
                }
                else
                {
                    return 0;
                }
            }
        }
        [JsonProperty("dauKy_TuSo")] //6
        public string DauKy_TuSo
        {
            get
            {
                if (this.DauKy_Tong == 0)
                {
                    return string.Empty;
                }
                if (SoDuHoaDonDauKy.IsNotNullOrEmpty())
                {
                    return (SoDuHoaDonDauKy.ToInt(0) + 1).ToString().PadLeft(SoDuHoaDonDauKy.Length, '0');
                }
                else if (this.RegistNumberFrom.IsNotNullOrEmpty())
                {
                    return this.RegistNumberFrom;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [JsonProperty("dauKy_DenSo")] //7
        public string DauKy_DenSo
        {
            get
            {
                if (this.DauKy_Tong == 0)
                {
                    return string.Empty;
                }

                if (this.RegistNumberTo.IsNotNullOrEmpty())
                {
                    return RegistNumberTo;
                }
                else
                {
                    return string.Empty;
                }
            }
        }


        [JsonProperty("soMua_TuSo")] //8
        public string SoMua_TuSo { get; set; }

        [JsonProperty("soMua_DenSo")] //9
        public string SoMua_DenSo { get; set; }

        [JsonProperty("TongSuDung_TuSo")] //10
        public string TongSuDung_TuSo { get; set; }

        [JsonProperty("TongSuDung_DenSo")] //11
        public string TongSuDung_DenSo { get; set; }

        [JsonProperty("TongSuDung_Cong")] //12
        public long TongSuDung_Cong
        {
            get
            {
                if (this.TongSuDung_TuSo.IsNotNullOrEmpty() && this.TongSuDung_TuSo != "0" && this.TongSuDung_DenSo.IsNotNullOrEmpty())
                {
                    return ((this.TongSuDung_DenSo.ToInt(0) - this.TongSuDung_TuSo.ToInt(0)) + 1);
                }
                else
                {
                    return 0;
                }
            }
        }
        [JsonProperty("SoLuong_DaSuDung")] //13
        public long SoLuong_DaSuDung { get; set; }

        [JsonProperty("huyBo_Soluog")] //14
        public long HuyBoSoLuong { get; set; }

        [JsonProperty("xoaBo_So")] //15
        public string XoaBoSo { get; set; }
        [JsonProperty("xoaBo_Soluog")] //14
        public long XoaBoSoLuong { get; set; }

        [JsonProperty("huyBo_So")] //15
        public string HuyBoSo { get; set; }

        [JsonProperty("mat_Soluog")] //16
        public long MatSoLuong { get; set; }

        [JsonProperty("mat_So")] //17
        public string MatSo { get; set; }

        [JsonProperty("huy_Soluog")] //18
        public long HuySoLuong { get; set; }

        [JsonProperty("huy_So")] //19
        public string HuySo { get; set; }

        [JsonProperty("chua_Phat_Hanh")]
        public long ChuaPhatHanh { get; set; }

        [JsonProperty("dieu_Chinh")]
        public long DieuChinh { get; set; }

        [JsonProperty("thay_The")]
        public long ThayThe { get; set; }

        [JsonProperty("cuoiKy_TongSo")] //22
        public long CuoiKy_Tong
        {
            get
            {
                if (this.CuoiKy_TuSo.IsNotNullOrEmpty() && this.CuoiKy_DenSo.IsNotNullOrEmpty())
                {
                    return ((this.CuoiKy_DenSo.ToInt(0) - this.CuoiKy_TuSo.ToInt(0)) + 1);
                }
                else
                {
                    return 0;
                }
            }
        }

        [JsonProperty("cuoiKy_TuSo")] //20
        public string CuoiKy_TuSo
        {
            get
            {
                if (this.TongSuDung_DenSo.IsEquals(this.RegistNumberTo) || this.TongSuDung_DenSo.IsEquals(this.SoMua_DenSo))
                {
                    return string.Empty;
                }

                if (this.DauKy_Tong == this.TongSuDung_Cong + this.HuySoLuong)
                {
                    return string.Empty;
                }

                if (this.TongSuDung_DenSo.IsNotNullOrEmpty() && this.TongSuDung_DenSo != "0")
                {
                    return (this.TongSuDung_DenSo.ToInt(0) + 1).ToString().PadLeft(TongSuDung_DenSo.Length, '0');
                }
                //truong hop chua phat hanh hoa don nao trong kỳ
                else if (this.DauKy_TuSo.IsNotNullOrEmpty())
                {
                    return this.DauKy_TuSo;//003
                }
                else if (this.SoMua_TuSo.IsNotNullOrEmpty())
                {
                    return this.SoMua_TuSo;
                }
                else
                {
                    return this.DauKy_TuSo;
                }
            }
        }

        [JsonProperty("cuoiKy_DenSo")] //21
        public string CuoiKy_DenSo
        {
            get
            {

                if (this.DauKy_Tong == this.TongSuDung_Cong + this.HuySoLuong)
                {
                    return string.Empty;
                }

                if (this.TongSuDung_DenSo.IsEquals(this.RegistNumberTo) || this.TongSuDung_DenSo.IsEquals(this.SoMua_DenSo)
                    || this.SoDuHoaDonDauKy.IsEquals(this.RegistNumberTo))
                {
                    return string.Empty;
                }
                //truong hop co phat hanh them trong kỳ
                else if (this.SoMua_DenSo.IsNotNullOrEmpty() && this.SoMua_DenSo != "0")
                {
                    return (this.SoMua_DenSo.ToInt(0) - this.HuySoLuong).ToString().PadLeft(this.SoMua_DenSo.Length, '0');
                }
                else if (RegistNumberTo.IsNotNullOrEmpty())
                {
                    return (this.RegistNumberTo.ToInt(0) - this.HuySoLuong).ToString().PadLeft(this.RegistNumberTo.Length, '0');
                }
                else
                {
                    return this.SoMua_DenSo;
                }
            }
        }
        [JsonIgnore] //22
        public string NumberFromUse { get; set; }

        [JsonIgnore] //22
        public string RegistNumberFrom { get; set; }

        [JsonIgnore] //22
        public string RegistNumberTo { get; set; }

        [JsonIgnore] //22
        public string SoDuHoaDonDauKy { get; set; }

        [JsonProperty("soLuong_DaDuyet")] //21 //26
        public int SoLuong_DaDuyet { get; set; }

        [JsonIgnore] //27
        public int KyHieu_DaHuy { get; set; }

        [JsonIgnore] //28
        public long CheckHuy { get; set; }

        public bool isReport { get; set; }

    }
}
