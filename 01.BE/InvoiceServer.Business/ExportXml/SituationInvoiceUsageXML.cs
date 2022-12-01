using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace DQSServer.Business.ExportXml
{
    public class SituationInvoiceUsageXml
    {

        private readonly PrintConfig config;
        private readonly ReportInvoiceUsing dataInvoiceUsage;
        public SituationInvoiceUsageXml(ReportInvoiceUsing dataReports, PrintConfig config)
        {
            this.dataInvoiceUsage = dataReports;
            this.config = config;
        }


        public ExportFileInfo WriteFileXML()
        {
            string fileName = string.Format("{0}_{1}.xml", "Notice-Not-Usel-Invoice", DateTime.Now.ToString(Formatter.DateTimeFormat));
            string path = System.IO.Path.Combine(this.config.FullFolderAssetOfCompany, "Reports");
            string filePathFileName = System.IO.Path.Combine(path, fileName);
            CheckFullPathFile(path);
            string kieuKy = "M";
            if (this.dataInvoiceUsage.IsViewByMonth)
            {
                kieuKy = "Q";
            }
            StringBuilder headerReport = new StringBuilder();
            headerReport.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            headerReport.Append("<HSoThueDTu xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://kekhaithue.gdt.gov.vn/TKhaiThue\">");
            headerReport.Append("<HSoKhaiThue id=\"_NODE_TO_SIGN\">");
            headerReport.Append("    <TTinChung>");
            headerReport.Append("      <TTinDVu>");
            headerReport.Append("        <maDVu>HTKK</maDVu>");
            headerReport.Append("        <tenDVu>HTKK</tenDVu>");
            headerReport.Append("        <pbanDVu>3.8.4</pbanDVu>");
            headerReport.Append("        <ttinNhaCCapDVu>");
            headerReport.Append("        </ttinNhaCCapDVu>");
            headerReport.Append("      </TTinDVu>");
            headerReport.Append("      <TTinTKhaiThue>");
            headerReport.Append("        <TKhaiThue>");
            headerReport.Append("          <maTKhai>102</maTKhai>");
            headerReport.Append("          <tenTKhai>Báo cáo tình hình sử dụng hóa đơn</tenTKhai>");
            headerReport.Append("          <moTaBMau>(Ban hành kèm theo Thông tư số 156/2013/TT-BTC ngày 06/11/2013 của Bộ Tài chính)</moTaBMau>");
            headerReport.Append("          <pbanTKhaiXML>2.0.8</pbanTKhaiXML>");
            headerReport.Append("          <loaiTKhai>C</loaiTKhai>");
            headerReport.Append("          <soLan>0</soLan>");
            headerReport.Append("          <KyKKhaiThue>");
            headerReport.AppendFormat("            <kieuKy>{0}</kieuKy>", kieuKy);
            headerReport.AppendFormat("            <kyKKhai>{0}/{1}</kyKKhai>", this.dataInvoiceUsage.Precious, this.dataInvoiceUsage.Year);
            headerReport.AppendFormat("            <kyKKhaiTuNgay>{0}</kyKKhaiTuNgay>", this.dataInvoiceUsage.DateFrom.ToString("dd/MM/yyyy"));
            headerReport.AppendFormat("            <kyKKhaiDenNgay>{0}</kyKKhaiDenNgay>", this.dataInvoiceUsage.DateTo.ToString("dd/MM/yyyy"));
            headerReport.Append("            <kyKKhaiTuThang></kyKKhaiTuThang>");
            headerReport.Append("            <kyKKhaiDenThang></kyKKhaiDenThang>");
            headerReport.Append("          </KyKKhaiThue>");
            headerReport.Append("          <maCQTNoiNop></maCQTNoiNop>");
            headerReport.Append("          <tenCQTNoiNop></tenCQTNoiNop>");
            headerReport.AppendFormat("          <ngayLapTKhai>{0}</ngayLapTKhai>", DateTime.Now.ToString("yyyy-MM-dd"));
            headerReport.Append("          <GiaHan>");
            headerReport.Append("            <maLyDoGiaHan></maLyDoGiaHan>");
            headerReport.Append("            <lyDoGiaHan></lyDoGiaHan>");
            headerReport.Append("          </GiaHan>");
            headerReport.AppendFormat("          <nguoiKy>{0}</nguoiKy>", this.dataInvoiceUsage.Company.Delegate.EscapeXMLValue());
            headerReport.AppendFormat("          <ngayKy>{0}</ngayKy>", DateTime.Now.ToString("yyyy-MM-dd"));
            headerReport.Append("          <nganhNgheKD></nganhNgheKD>");
            headerReport.Append("        </TKhaiThue>");
            headerReport.Append("        <NNT>");
            headerReport.AppendFormat("          <mst>{0}</mst>", this.dataInvoiceUsage.Branch.TaxCode.EscapeXMLValue());
            headerReport.AppendFormat("          <tenNNT>{0}</tenNNT>", this.dataInvoiceUsage.Branch.CompanyName.EscapeXMLValue());
            headerReport.AppendFormat("          <dchiNNT>{0}</dchiNNT>", this.dataInvoiceUsage.Branch.Address.EscapeXMLValue());
            headerReport.Append("          <phuongXa></phuongXa>");
            headerReport.Append("          <maHuyenNNT></maHuyenNNT>");
            headerReport.Append("          <tenHuyenNNT></tenHuyenNNT>");
            headerReport.Append("          <maTinhNNT></maTinhNNT>");
            headerReport.Append("          <tenTinhNNT></tenTinhNNT>");
            headerReport.AppendFormat("          <dthoaiNNT>{0}</dthoaiNNT>", this.dataInvoiceUsage.Company.Tel.EscapeXMLValue());
            headerReport.AppendFormat("          <faxNNT>{0}</faxNNT>", this.dataInvoiceUsage.Company.Fax.EscapeXMLValue());
            headerReport.AppendFormat("          <emailNNT>{0}</emailNNT>", this.dataInvoiceUsage.Company.Email.EscapeXMLValue());
            headerReport.Append("        </NNT>");
            headerReport.Append("      </TTinTKhaiThue>");
            headerReport.Append("    </TTinChung>");
            headerReport.Append(CreateDetailReport());
            headerReport.Append(CreateItemReport());
            headerReport.Append(CreateFooterReport());
            headerReport.Append("</HSoKhaiThue>");
            headerReport.Append("</HSoThueDTu>");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(headerReport.ToString());
            xmlDoc.Save(filePathFileName);
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = fileName;
            fileInfo.FullPathFileName = filePathFileName;
            return fileInfo;
        }

        private string CreateFooterReport()
        {
            StringBuilder footerlReport = new StringBuilder();
            footerlReport.AppendFormat("   <nguoiLapBieu>{0}</nguoiLapBieu>", this.dataInvoiceUsage.Company.PersonContact.EscapeXMLValue());
            footerlReport.AppendFormat("   <nguoiDaiDien>{0}</nguoiDaiDien>", this.dataInvoiceUsage.Company.Delegate.EscapeXMLValue());
            footerlReport.AppendFormat("   <ngayBCao>{0}</ngayBCao>", DateTime.Now.ToString("yyyy-MM-dd"));
            footerlReport.Append(" </CTieuTKhaiChinh>");
            return footerlReport.ToString();
        }

        private string CreateDetailReport()
        {
            StringBuilder detailReport = new StringBuilder();
            detailReport.Append(" <CTieuTKhaiChinh>");
            detailReport.Append("   <kyBCaoCuoi>0</kyBCaoCuoi>");
            detailReport.Append("   <chuyenDiaDiem>0</chuyenDiaDiem>");
            detailReport.AppendFormat("   <ngayDauKyBC>{0}</ngayDauKyBC>", this.dataInvoiceUsage.DateFrom.ToString("dd/MM/yyyy"));
            detailReport.AppendFormat("   <ngayCuoiKyBC>{0}</ngayCuoiKyBC>", this.dataInvoiceUsage.DateTo.ToString("dd/MM/yyyy"));
            return detailReport.ToString();
        }

        private string CreateItemReport()
        {
            StringBuilder itenInvoice = new StringBuilder();
            itenInvoice.Append("<HoaDon>");
            long TongCuoiKy = 0;
            long TongDauKy = 0;
            long TongDaSuDung = 0;
            int identity = 1;
            foreach (var item in dataInvoiceUsage.items)
            {
                itenInvoice.AppendFormat("<ChiTiet id=\"ID_{0}\">", identity);
                itenInvoice.AppendFormat("<maHoaDon>{0}</maHoaDon>", item.InvoiceSampleCode);
                itenInvoice.AppendFormat("<tenHDon>{0}</tenHDon>", item.InvoiceSample);
                itenInvoice.AppendFormat("<kHieuMauHDon>{0}</kHieuMauHDon>", item.InvoiceSampleCode);
                itenInvoice.AppendFormat("<kHieuHDon>{0}</kHieuHDon>", item.InvoiceSymbol);
                itenInvoice.AppendFormat("<soTonMuaTrKy_tongSo>{0}</soTonMuaTrKy_tongSo>", item.DauKy_Tong);
                itenInvoice.AppendFormat("<soTonDauKy_tuSo>{0}</soTonDauKy_tuSo>", item.DauKy_TuSo);
                itenInvoice.AppendFormat("<soTonDauKy_denSo>{0}</soTonDauKy_denSo>", item.DauKy_DenSo);
                itenInvoice.AppendFormat("<muaTrongKy_tuSo>{0}</muaTrongKy_tuSo>", item.SoMua_TuSo);
                itenInvoice.AppendFormat("<muaTrongKy_denSo>{0}</muaTrongKy_denSo>", item.SoMua_DenSo);
                itenInvoice.AppendFormat("<tongSoSuDung_tuSo>{0}</tongSoSuDung_tuSo>", item.TongSuDung_TuSo);
                itenInvoice.AppendFormat("<tongSoSuDung_denSo>{0}</tongSoSuDung_denSo>", item.TongSuDung_DenSo);
                itenInvoice.AppendFormat("<tongSoSuDung_cong>{0}</tongSoSuDung_cong>", item.TongSuDung_Cong);
                itenInvoice.AppendFormat("<soDaSDung>{0}</soDaSDung>", item.SoLuong_DaSuDung);
                itenInvoice.AppendFormat("<xoaBo_soLuong>{0}</xoaBo_soLuong>", item.XoaBoSoLuong);
                itenInvoice.AppendFormat("<xoaBo_so>{0}</xoaBo_so>", item.XoaBoSo);
                itenInvoice.AppendFormat("<mat_soLuong>{0}</mat_soLuong>", item.MatSoLuong);
                itenInvoice.AppendFormat("<mat_so>{0}</mat_so>", item.MatSoLuong);
                itenInvoice.AppendFormat("<huy_soLuong>{0}</huy_soLuong>", item.HuySoLuong);
                itenInvoice.AppendFormat("<huy_so>{0}</huy_so>", item.HuySo);
                itenInvoice.AppendFormat("<tonCuoiKy_tuSo>{0}</tonCuoiKy_tuSo>", item.CuoiKy_TuSo);
                itenInvoice.AppendFormat("<tonCuoiKy_denSo>{0}</tonCuoiKy_denSo>", item.CuoiKy_DenSo);
                itenInvoice.AppendFormat("<tonCuoiKy_soLuong>{0}</tonCuoiKy_soLuong>", item.CuoiKy_Tong);
                itenInvoice.Append("</ChiTiet>");
                TongCuoiKy += item.CuoiKy_Tong;
                TongDaSuDung += item.TongSuDung_Cong;
                TongDauKy += item.DauKy_Tong;
                identity++;
            }

            itenInvoice.Append("</HoaDon>");
            itenInvoice.AppendFormat("<tongCongSoTonDKy>{0}</tongCongSoTonDKy>", TongDauKy);
            itenInvoice.AppendFormat("<tongCongSDung>{0}</tongCongSDung>", TongDaSuDung);
            itenInvoice.AppendFormat("<tongCongSoTonCKy>{0}</tongCongSoTonCKy>", TongCuoiKy);
            return itenInvoice.ToString();
        }

        private void CheckFullPathFile(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
