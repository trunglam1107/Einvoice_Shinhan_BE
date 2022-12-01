using InvoiceServer.Business.Utils;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{ 
  
    public class DeclarationMaster
    {
        [DataConvert("Id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        public const string BranchID = "2.0.0";
         
        public const string DeclarationNo = "01/ĐKTĐ-HĐĐT";
          
        public const string DeclarationName = "Tờ khai đăng ký/thay đổi thông tin sử dụng hóa đơn điện tử";

         
        public int DeclarationType { get; set; }
        // 1. Đăng Ký , 2. Thay Đổi

           
        public string CompanyName { get; set; }
         
        public string CompanyTaxCode { get; set; }

           
        public string TaxCompanyName { get; set; }
         
        public string TaxCompanyCode { get; set; }

           
        public string CustName { get; set; }
           
        public string CustAddress { get; set; }
          
        public string CustEmail { get; set; }
           
        public string CustPhone { get; set; }
            
        public string ProvinceCity { get; set; }
        [DataConvert("DeclarationDate")]
        public DateTime DeclarationDate { get; set; }


        [DataConvert("HTHDon")]
        public int? HTHDon { get; set; }
        // 1. Có mã , 2. Không có mã

        // Hình thức gửi dữ liệu
        public int HTGDLHDDT { get; set; }
        // 1. NNTDBKKhan  - Doanh nghiệp khó khăn.
        // 2. NNTKTDNUBND  - Doanh nghiệp theo đề nghị của Ủy ban nhân dân tỉnh, thành phố.
        // 3. CDLTTDCQT - Chuyển dữ liệu trực tiếp đến cơ quan thuế
        // 4. CDLQTCTN - Thông qua tổ chức cung cấp hóa đơn dịch vụ

            
        public int PThuc { get; set; }
        // 1.CDDu - Chuyển đầy đủ
        // 2.CBTHop - Chuyển tổng hợp

        [DataConvert("LHDSDung")]

        public int? LHDSDung { get; set; }
            // 1. HDGTGT  - Hóa đơn GTGT
            // 2. HDBHang  - Hóa đơn bán hàng
            // 3. HDBTSCong - Hóa đơn bán tài sản công
            // 4. HDBHDTQGia - Hóa đơn bán hàng dự trữ quốc gia
            // 5. HDKhac - Các loại hóa đơn khác
            // 6. CTu - Các chứng từ được in, phát hành, sử dụng và quản lý như hóa đơn.

           
            public List<DeclarationReleaseInfo> releaseInfoes { get; set; }

        // Thông tin ký hiệu không có trong XML
        // Phân Loại hóa đơn điện tử

        [DataConvert("Type")]

        public int? Type { get; set; }

        // Free text (2 ký tự) cuối Ký hiệu
        [DataConvert("Symbol")]
        public string Symbol { get; set; }

        //Thông tin khác
        public DateTime? UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public int Status { get; set; }

        [DataConvert("RegisterTemplateId")]
        public long? RegisterTemplateId { get; set; }
        public string symboyFinal
        {
            get
            {
                if (this.HTHDon>0 && this.LHDSDung>0 && this.Symbol !=null && this.Type>0 && this.DeclarationDate!=null)
                {
                    var symboyMapping = "";
                    symboyMapping= StringExtensions.MappingSymbol(this.LHDSDung, this.HTHDon,
                        this.DeclarationDate.ToString(), this.Type, this.Symbol
                      );

                    return symboyMapping;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [DataConvert("Releaseddate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? Releaseddate { get; set; }
        // 1. Khởi tạo.
        // 2. Đợi Duyệt.
        // 3. Duyệt.
        // 4. Đang gửi cơ quan thuế.
        // 5. Gửi thất bại.
        // 6. Cơ quan thuế không chấp nhận.
        // 7. Cơ quan thuế chấp nhận.

        public DeclarationMaster()
            {
            }
            /// <summary>
            /// Constructor current Client object by copying data in the specified object
            /// </summary>
            /// <param name="srcUser">Source object</param>
            public DeclarationMaster(object srcObject)
               : this()
            {
                if (srcObject != null)
                {
                    DataObjectConverter.Convert<object, DeclarationMaster>(srcObject, this);
                }
            }

         

        }
    }


