namespace InvoiceServer.Business.Models.Account
{
    public class ExportAccount
    {
        public int? STT { get; set; }
        public string ChiNhanh { get; set; }
        public string TenNhanVien { get; set; }
        public string TenTaiKhoan { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string TenQuyen { get; set; }
        public string NgayKichHoat { get; set; }
        public string NgayHetHieuLuc { get; set; }
        public string TrangThai { get; set; }

        public ExportAccount() { }
    }
}
