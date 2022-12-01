using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace InvoiceServer.Business.Models
{

    [XmlRoot("TTChung")]
    public class DeclarationInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long ID { get; set; }

        [JsonProperty("branchID")]
        [XmlElement("PBan")]
        public const string BranchID = "2.0.0";

        [JsonProperty("declarationNo")]
        [XmlElement("MSo")]
        public const string DeclarationNo = "01/ĐKTĐ-HĐĐT";

        [JsonProperty("declarationName")]
        [XmlElement("Ten")]
        public const string DeclarationName = "Tờ khai đăng ký/thay đổi thông tin sử dụng hóa đơn điện tử";

        [JsonProperty("declarationType")]
        [XmlElement("HThuc")]
        [DataConvert("DeclarationType")]
        public int? DeclarationType { get; set; }


        [JsonProperty("companyID")]
        [XmlIgnore]
        [DataConvert("CompanyID")]
        public long? CompanyID { get; set; }

        [JsonProperty("registerTemplateID")]
        [DataConvert("RegisterTemplateID")]
        public long? RegisterTemplateID { get; set; }

        // 1. Đăng Ký , 2. Thay Đổi

        [StringTrimAttribute]
        [JsonProperty("companyName")]
        [XmlElement("TNNT")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyTaxCode")]
        [DataConvert("CompanyTaxCode")]
        [XmlElement("MST")]
        public string CompanyTaxCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("taxCompanyName")]
        [XmlElement("CQTQLy")]
        public string TaxCompanyName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("taxCompanyCode")]
        [XmlElement("MCQTQLy")]
        public string TaxCompanyCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("custName")]
        [DataConvert("CustName")]
        [XmlElement("NLHe")]
        public string CustName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("custAddress")]
        [DataConvert("CustAddress")]
        [XmlElement("DCLHe")]
        public string CustAddress { get; set; }

        [StringTrimAttribute]
        [JsonProperty("custEmail")]
        [DataConvert("CustEmail")]
        [XmlElement("DCTDTu")]
        public string CustEmail { get; set; }

        [StringTrimAttribute]
        [JsonProperty("custPhone")]
        [DataConvert("CustPhone")]
        [XmlElement("DTLHe")]
        public string CustPhone { get; set; }

        [StringTrimAttribute]
        [JsonProperty("provinceCity")]
        [XmlElement("DDanh")]
        public string ProvinceCity { get; set; }

        [JsonProperty("declarationDate")]
        [DataConvert("declarationDate")]
        [XmlElement("NLap")]
        public DateTime DeclarationDate { get; set; }

        [JsonProperty("approvedDate")]
        public DateTime? ApprovedDate { get; set; }

        [JsonProperty("hTHDon")]
        [DataConvert("HTHDon")]
        [XmlElement("HTHDon")]
        public int? HTHDon { get; set; }
        // 1. Có mã , 2. Không có mã

        [JsonProperty("hTGDLHDDT")]
        [DataConvert("HTGDLHDDT")]
        [XmlElement("HTGDLHDDT")] // Hình thức gửi dữ liệu
        public int? HTGDLHDDT { get; set; }
        // 1. NNTDBKKhan  - Doanh nghiệp khó khăn.
        // 2. NNTKTDNUBND  - Doanh nghiệp theo đề nghị của Ủy ban nhân dân tỉnh, thành phố.
        // 3. CDLTTDCQT - Chuyển dữ liệu trực tiếp đến cơ quan thuế
        // 4. CDLQTCTN - Thông qua tổ chức cung cấp hóa đơn dịch vụ

        [JsonProperty("pThuc")]
        [DataConvert("PThuc")]
        [XmlElement("PThuc")]
        public int? PThuc { get; set; }
        // 1.CDDu - Chuyển đầy đủ
        // 2.CBTHop - Chuyển tổng hợp

        [JsonProperty("lHDSDung")]
        [DataConvert("LHDSDung")]
        [XmlElement("LHDSDung")]
        public int? LHDSDung { get; set; }
        // 1. HDGTGT  - Hóa đơn GTGT
        // 2. HDBHang  - Hóa đơn bán hàng
        // 3. HDBTSCong - Hóa đơn bán tài sản công
        // 4. HDBHDTQGia - Hóa đơn bán hàng dự trữ quốc gia
        // 5. HDKhac - Các loại hóa đơn khác
        // 6. CTu - Các chứng từ được in, phát hành, sử dụng và quản lý như hóa đơn.

        [JsonProperty("items")]
        [XmlElement("DSCTSSDung")]
        public List<DeclarationReleaseInfo> releaseInfoes { get; set; }
        [JsonProperty("listCompanySymbols")]
        public List<CompanySymbolInfo> companySymbolInfos { get; set; }

        // Thông tin ký hiệu không có trong XML
        // Phân Loại hóa đơn điện tử
        [JsonProperty("type")]
        [DataConvert("Type")]
        public int? Type { get; set; }

        // Free text (2 ký tự) cuối Ký hiệu
        [StringTrimAttribute]
        [JsonProperty("symbol")]
        [DataConvert("Symbol")]
        public string Symbol { get; set; }

        //Thông tin khác
        [JsonProperty("cityId")]
        [DataConvert("CityId")]
        public long CityId { get; set; }

        [JsonProperty("taxDepartmentId")]
        [DataConvert("TaxDepartmentId")]
        public long TaxDepartmentId { get; set; }

        [JsonProperty("updateDate")]
        [DataConvert("UpdateDate")]
        public DateTime? UpdateDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("updateBy")]
        [DataConvert("UpdateBy")]
        public string UpdateBy { get; set; }

        [JsonProperty("status")]
        [DataConvert("Status")]
        public int? Status { get; set; }

        [JsonProperty("sendStatus")]
        public int? SendStatus { get; set; }

        [JsonProperty("sendStatusMessage")]        
        public string SendStatusMessage { get; set; }

        [JsonProperty("sendStatusCode")]
        public int SendStatusCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("messageCode")]
        [DataConvert("MessageCode")]
        public string MessageCode { get; set; }
        // 1. Khởi tạo.
        // 2. Đợi Duyệt.
        // 3. Duyệt.
        // 4. Đang gửi cơ quan thuế.
        // 5. Gửi thất bại.
        // 6. Cơ quan thuế không chấp nhận.
        // 7. Cơ quan thuế chấp nhận.
        public string XmlString { get; set; }
        [JsonIgnore]
        [XmlIgnore]
        public MINVOICE_DATA mINVOICE_DATA { get; set; }

        [JsonProperty("password")]
        public string PassWord { get; set; }

        [JsonProperty("seri")]
        public string Serinumber { get; set; }

        [JsonProperty("slot")]
        public string Slots { get; set; }

        public DeclarationInfo()
        {
        }
        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public DeclarationInfo(object srcObject)
           : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, DeclarationInfo>(srcObject, this);
            }
        }

        public DeclarationInfo(object srcObject, List<DeclarationReleaseInfo> concludeDetailInfos, List<CompanySymbolInfo> concludeCompanySymbols)
            : this(srcObject)
        {
            this.releaseInfoes = concludeDetailInfos;
            this.companySymbolInfos = concludeCompanySymbols;
        }

    }
}
