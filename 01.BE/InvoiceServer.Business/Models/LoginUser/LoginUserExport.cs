using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class LoginUserExportDetail
    {
        public long ID { get; set; }
        public string ChiNhanh { get; set; }
        public string TenNhanVien { get; set; }
        public string TenTaiKhoan { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string TenQuyen { get; set; }
        public string NgayKichHoat { get; set; }
        public string NgayHetHieuLuc { get; set; }
        public string TrangThai { get; set; }
        public string BranchId { get; set; }
        public LoginUserExportDetail()
        {
        }
    }

    public class LoginUserExport
    {

        public CompanyInfo Company { get; set; }

        public List<LoginUserExportDetail> Items { get; set; }

        public LoginUserExport(CompanyInfo company)
        {
            this.Company = company;
        }
    }
}
