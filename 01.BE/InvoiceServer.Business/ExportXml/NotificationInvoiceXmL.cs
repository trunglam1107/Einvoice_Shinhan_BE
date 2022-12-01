using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace InvoiceServer.Business.ExportXml
{
    public class NotificationInvoiceXmL
    {
        public readonly ReporNotificationInvoice DataReport;
        public readonly PrintConfig Config;
        public NotificationInvoiceXmL(ReporNotificationInvoice dataReports, PrintConfig config)
        {
            this.Config = config;
            this.DataReport = dataReports;
        }

        public ExportFileInfo ExportReport()
        {
            CreateFolderContainReport();
            string fileName = string.Format("{0}_{1}.xlsx", "invoice-notification", DateTime.Now.ToString(Formatter.DateTimeFormat));
            string filePathFileName = string.Format("{0}\\Reports\\{1}", this.Config.FullFolderAssetOfCompany, fileName);
            string reportStringXml = WriteXMLReport();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(reportStringXml);
            xmlDoc.Save(filePathFileName);
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = fileName;
            fileInfo.FullPathFileName = filePathFileName;
            return fileInfo;
        }

        private string WriteXMLReport()
        {
            StringBuilder xmlReport = new StringBuilder();
            xmlReport.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            xmlReport.Append("<HSoThueDTu xmlns=\"http://kekhaithue.gdt.gov.vn/TKhaiThue\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            xmlReport.Append("    <HSoKhaiThue>");
            xmlReport.Append("        <TTinChung>");
            xmlReport.Append("            <TTinDVu>");
            xmlReport.Append("                <maDVu>ETAX</maDVu>");
            xmlReport.Append("                <tenDVu>ETAX 1.0</tenDVu>");
            xmlReport.Append("                <pbanDVu>1.0</pbanDVu>");
            xmlReport.Append("                <ttinNhaCCapDVu>ETAX_TCT</ttinNhaCCapDVu>");
            xmlReport.Append("            </TTinDVu>");
            xmlReport.Append("            <TTinTKhaiThue>");
            xmlReport.Append("                <TKhaiThue>");
            xmlReport.Append("                    <maTKhai>371</maTKhai>");
            xmlReport.Append("                    <tenTKhai>Thông báo phát hành hóa đơn điện tử</tenTKhai>");
            xmlReport.Append("                    <moTaBMau/>");
            xmlReport.Append("                    <pbanTKhaiXML>2.1.3</pbanTKhaiXML>");
            xmlReport.Append("                    <loaiTKhai>C</loaiTKhai>");
            xmlReport.Append("                    <soLan>0</soLan>");
            xmlReport.Append("                    <KyKKhaiThue>");
            xmlReport.Append("                        <kieuKy>D</kieuKy>");
            xmlReport.AppendFormat("                        <kyKKhai>{0}</kyKKhai>", (this.DataReport.NoticeInvoice.CreatedDate ?? DateTime.Now).ToString("dd/MM/yyyy"));
            xmlReport.AppendFormat("                        <kyKKhaiTuNgay>{0}</kyKKhaiTuNgay>", (this.DataReport.NoticeInvoice.CreatedDate ?? DateTime.Now).ToString("dd/MM/yyyy"));
            xmlReport.AppendFormat("                        <kyKKhaiDenNgay>{0}</kyKKhaiDenNgay>", (this.DataReport.NoticeInvoice.CreatedDate ?? DateTime.Now).ToString("dd/MM/yyyy"));
            xmlReport.Append("                        <kyKKhaiTuThang></kyKKhaiTuThang>");
            xmlReport.Append("                        <kyKKhaiDenThang></kyKKhaiDenThang>");
            xmlReport.Append("                    </KyKKhaiThue>");
            xmlReport.Append("                    <maCQTNoiNop>21701</maCQTNoiNop>");
            xmlReport.AppendFormat("                    <tenCQTNoiNop>{0}</tenCQTNoiNop>", this.DataReport.NoticeInvoice.TaxDepartmentName.EscapeXMLValue());
            xmlReport.AppendFormat("                    <ngayLapTKhai>{0}</ngayLapTKhai>", (this.DataReport.NoticeInvoice.CreatedDate ?? DateTime.Now).ToString("yyyy-MM-dd"));
            xmlReport.AppendFormat("                    <nguoiKy>{0}</nguoiKy>", this.DataReport.Company.Delegate);
            xmlReport.AppendFormat("                    <ngayKy>{0}</ngayKy>", (this.DataReport.NoticeInvoice.CreatedDate ?? DateTime.Now).ToString("yyyy-MM-dd"));
            xmlReport.Append("                    <nganhNgheKD></nganhNgheKD>");
            xmlReport.Append("                </TKhaiThue>");
            xmlReport.Append("                <NNT>");
            xmlReport.AppendFormat("                    <mst>{0}</mst>", this.DataReport.NoticeInvoice.Status == 2 ? this.DataReport.NoticeInvoice.TaxCode : this.DataReport.Company.TaxCode.EscapeXMLValue());
            xmlReport.AppendFormat("                    <tenNNT>{0}</tenNNT>", this.DataReport.NoticeInvoice.Status == 2 ? this.DataReport.NoticeInvoice.CompanyName : this.DataReport.Company.CompanyName.EscapeXMLValue());
            xmlReport.AppendFormat("                    <dchiNNT>{0}</dchiNNT>", this.DataReport.NoticeInvoice.Status == 2 ? this.DataReport.NoticeInvoice.Address : this.DataReport.Company.Address.EscapeXMLValue());
            xmlReport.Append("                    <phuongXa></phuongXa>");
            xmlReport.Append("                    <maHuyenNNT></maHuyenNNT>");
            xmlReport.AppendFormat("                    <tenHuyenNNT>Thành phố {0}</tenHuyenNNT>", this.DataReport.NoticeInvoice.CityName.EscapeXMLValue());
            xmlReport.Append("                    <maTinhNNT></maTinhNNT>");
            xmlReport.Append("                    <tenTinhNNT>Việt Nam</tenTinhNNT>");
            xmlReport.AppendFormat("                    <dthoaiNNT>{0}</dthoaiNNT>", this.DataReport.NoticeInvoice.Status == 2 ? this.DataReport.NoticeInvoice.Tel : this.DataReport.Company.Tel.EscapeXMLValue());
            xmlReport.AppendFormat("                    <faxNNT>{0}</faxNNT>", this.DataReport.Company.Fax.EscapeXMLValue());
            xmlReport.AppendFormat("                    <emailNNT>{0}</emailNNT>", this.DataReport.Company.Email.EscapeXMLValue());
            xmlReport.Append("                </NNT>");
            xmlReport.Append("            </TTinTKhaiThue>");
            xmlReport.Append("        </TTinChung>");
            xmlReport.Append("        <CTieuTKhaiChinh>");
            xmlReport.Append("            <HoaDon>");
            xmlReport.Append(CreateItemReport());
            xmlReport.Append("            </HoaDon>");
            xmlReport.AppendFormat("            <tenCQTTiepNhan>{0}</tenCQTTiepNhan>", this.DataReport.NoticeInvoice.TaxDepartmentName.EscapeXMLValue());
            xmlReport.AppendFormat("            <nguoiDaiDien>{0}</nguoiDaiDien>", this.DataReport.Company.Delegate.EscapeXMLValue());
            xmlReport.AppendFormat("            <ngayBCao>{0}</ngayBCao>", (this.DataReport.NoticeInvoice.CreatedDate ?? DateTime.Now).ToString("yyyy-MM-dd"));
            xmlReport.Append("        </CTieuTKhaiChinh>");
            xmlReport.Append("    </HSoKhaiThue>");
            xmlReport.Append("    <CKyDTu/>");
            xmlReport.Append("</HSoThueDTu>");
            return xmlReport.ToString();
        }

        private string CreateItemReport()
        {
            StringBuilder itenReport = new StringBuilder();
            foreach (var item in this.DataReport.NoticeInvoice.UseInvoiceDetails)
            {
                itenReport.Append("<ChiTiet ID=\"ID_1\"> ");
                itenReport.AppendFormat("   <tenLoaiHDon>{0}</tenLoaiHDon> ", item.InvoiceSampleName.EscapeXMLValue());
                itenReport.AppendFormat("   <mauSo>{0}</mauSo> ", item.RegisterTemplateCode.EscapeXMLValue());
                itenReport.AppendFormat("   <kyHieu>{0}</kyHieu> ", item.Code.EscapeXMLValue());
                itenReport.AppendFormat("   <soLuong>{0}</soLuong> ", item.NumberUse);
                itenReport.AppendFormat("   <tuSo>{0}</tuSo> ", item.NumberFrom);
                itenReport.AppendFormat("   <denSo>{0}</denSo> ", item.NumberTo);
                itenReport.AppendFormat("   <ngayBDauSDung>{0} 12:00AM</ngayBDauSDung> ", item.UsedDate.ToString("yyyy-MM-dd"));
                itenReport.Append("</ChiTiet> ");
            }

            return itenReport.ToString();
        }

        private void CreateFolderContainReport()
        {
            string fullPathForlder = Path.Combine(this.Config.FullFolderAssetOfCompany, "Reports");
            if (!Directory.Exists(fullPathForlder))
            {
                Directory.CreateDirectory(fullPathForlder);
            }
        }
    }
}
