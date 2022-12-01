using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace InvoiceServer.Business.ExportXml
{
    public class ReportMothlyBoardXml
    {
        public DateTime DateFrom { get; private set; }
        public DateTime DateTo { get; private set; }

        private readonly PrintConfig config;
        private readonly ReporMonthlyBoard dataInvoiceUsage;
        public ReportMothlyBoardXml(ReporMonthlyBoard dataReports, PrintConfig config)
        {
            this.dataInvoiceUsage = dataReports;
            this.config = config;
            BuildDateFormDateTo(dataReports.Month, dataReports.Year);
        }


        public ExportFileInfo WriteFileXML()
        {
            string fileName = string.Format("{0}_{1}.xml", "report-monthly-board-invoice", DateTime.Now.ToString(Formatter.DateTimeFormat));
            string path = System.IO.Path.Combine(this.config.FullFolderAssetOfCompany, "Reports");
            string filePathFileName = Path.Combine(path, fileName);
            CheckFullPathFile(path);
            string kieuKy = "E";
            string ReportStringXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
            + "<HSoThueDTu>"
            + "   <HSoKhaiThue>"
            + "      <TTinChung>"
            + "         <TTinDVu>"
            + "            <maDVu />"
            + "            <tenDVu />"
            + "            <pbanDVu />"
            + "            <ttinNhaCCapDVu />"
            + "         </TTinDVu>"
            + "         <TTinTKhaiThue>"
            + "            <TKhaiThue>"
            + "               <maTKhai />"
            + "               <tenTKhai />"
            + "               <moTaBMau>(Ban hành kèm theo Thông tư số 119/2014/TT-BTC ngày 25/8/2014 của Bộ Tài chính)</moTaBMau>"
            + "               <pbanTKhaiXML>2.0.7</pbanTKhaiXML>"
            + "               <loaiTKhai>C</loaiTKhai>"
            + "               <soLan>0</soLan>"
            + "               <KyKKhaiThue>"
            + "                  <kieuKy>" + kieuKy + "</kieuKy>"
            + "                  <kyKKhai>" + dataInvoiceUsage.Month + "/" + dataInvoiceUsage.Year + "</kyKKhai>"
            + "                  <kyKKhaiTuNgay>" + DateFrom.ToString("dd/MM/yyyy") + "</kyKKhaiTuNgay>"
            + "                  <kyKKhaiDenNgay>" + DateTo.ToString("dd/MM/yyyy") + "</kyKKhaiDenNgay>"
            + "                  <kyKKhaiTuThang>" + dataInvoiceUsage.Month + "</kyKKhaiTuThang>"
            + "                  <kyKKhaiDenThang>" + dataInvoiceUsage.Month + "</kyKKhaiDenThang>"
            + "               </KyKKhaiThue>"
            + "               <maCQTNoiNop />"
            + "               <tenCQTNoiNop />"
            + "               <ngayLapTKhai>" + DateTime.Now.ToString("yyyy-MM-dd") + "</ngayLapTKhai>"
            + "               <GiaHan>"
            + "                  <maLyDoGiaHan />"
            + "                  <lyDoGiaHan />"
            + "               </GiaHan>"
            + "               <nguoiKy />"
            + "               <ngayKy />"
            + "               <nganhNgheKD />"
            + "            </TKhaiThue>"
            + "            <NNT>"
            + "               <mst>" + dataInvoiceUsage.Company.TaxCode + "</mst>"
            + "               <tenNNT><![CDATA[" + dataInvoiceUsage.Company.CompanyName + "]]></tenNNT>"
            + "               <dchiNNT><![CDATA[" + dataInvoiceUsage.Company.Address + "]]></dchiNNT>"
            + "               <phuongXa />"
            + "               <maHuyenNNT />"
            + "               <tenHuyenNNT />"
            + "               <maTinhNNT />"
            + "               <tenTinhNNT />"
            + "               <dthoaiNNT>" + dataInvoiceUsage.Company.Tel + "</dthoaiNNT>"
            + "               <faxNNT />"
            + "               <emailNNT />"
            + "            </NNT>"
            + "         </TTinTKhaiThue>"
            + "      </TTinChung>"
            + "      <CTieuTKhaiChinh>"
            + "         <tieuMucHachToan />"
            + "         <ct21 />"
            + "         <ct22 />"
            + "         <GiaTriVaThueGTGTHHDVMuaVao>"
            + "            <ct23 />"
            + "            <ct24 />"
            + "         </GiaTriVaThueGTGTHHDVMuaVao>"
            + "         <ct25 />"
            + "         <ct26 />"
            + "         <HHDVBRaChiuThueGTGT>"
            + "            <ct27 />"
            + "            <ct28 />"
            + "         </HHDVBRaChiuThueGTGT>"
            + "         <ct29 />"
            + "         <HHDVBRaChiuTSuat5>"
            + "            <ct30 />"
            + "            <ct31 />"
            + "         </HHDVBRaChiuTSuat5>"
            + "         <HHDVBRaChiuTSuat10>"
            + "            <ct32 />"
            + "            <ct33 />"
            + "         </HHDVBRaChiuTSuat10>"
            + "         <TongDThuVaThueGTGTHHDVBRa>"
            + "            <ct34 />"
            + "            <ct35 />"
            + "         </TongDThuVaThueGTGTHHDVBRa>"
            + "         <ct36 />"
            + "         <ct37 />"
            + "         <ct38 />"
            + "         <ct39 />"
            + "         <ct40a />"
            + "         <ct40b />"
            + "         <ct40 />"
            + "         <ct41 />"
            + "         <ct42 />"
            + "         <ct43 />"
            + "      </CTieuTKhaiChinh>"
            + "      <PLuc>"
            + "        <PL01_1_GTGT>"
            + CreateInvoiceNotUseTax()
            + CreateInvoiceUseZeroPercent()
            + CreateInvoiceUseFivePercent()
            + CreateInvoiceUseTenPercent()
            + "         <tongDThuBRa>0</tongDThuBRa>"
            + "         <tongDThuBRaChiuThue>0</tongDThuBRaChiuThue>"
            + "         <tongThueBRa>0</tongThueBRa>"
            + "         </PL01_1_GTGT>"
            + "      </PLuc>"
            + "        </HSoKhaiThue>"
            + "     </HSoThueDTu>";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(ReportStringXml);
            xmlDoc.Save(filePathFileName);
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = fileName;
            fileInfo.FullPathFileName = filePathFileName;
            return fileInfo;
        }

        private string CreateInvoiceNotUseTax()
        {
            StringBuilder itemReport = new StringBuilder();
            var invoices = dataInvoiceUsage.items.Where(p => "HHDVKChiuThue".Equals(p.Code));
            itemReport.Append("<HHDVKChiuThue>");
            itemReport.Append("<ChiTietHHDVKChiuThue>");

            decimal total = 0;
            int index = 1;
            foreach (var item in invoices)
            {
                if (item.IsSummary)
                {
                    total = item.TotalByTax ?? 0;
                    continue;
                }

                itemReport.AppendFormat("<HDonBRa id=\"ID_{0}\">", index);
                itemReport.AppendFormat("	 <kyHieuMauHDon>{0}</kyHieuMauHDon>", item.InvoiceCode);
                itemReport.AppendFormat("	 <kyHieuHDon>{0}</kyHieuHDon>", item.InvoiceSymbol);
                itemReport.AppendFormat("	 <soHDon>{0}</soHDon>", item.InvoiceNo);
                if (item.ReleasedDate.HasValue)
                {
                    itemReport.AppendFormat("	 <ngayPHanh>{0}</ngayPHanh>", item.ReleasedDate.Value.ToString("dd/MM/yyyy"));
                }
                else
                {
                    itemReport.AppendLine("	 <ngayPHanh/>");
                }

                itemReport.AppendFormat("	 <tenNMUA><![CDATA[{0}]]></tenNMUA>", item.CustomerName);
                itemReport.AppendFormat("	 <mstNMUA>{0}</mstNMUA>", item.TaxCode);
                itemReport.AppendLine("	 <matHang/>");
                if (item.Total.HasValue)
                {
                    itemReport.AppendFormat("	 <dsoBanChuaThue>{0}</dsoBanChuaThue>", item.Total.Value);
                }
                else
                {
                    itemReport.AppendLine("	 <dsoBanChuaThue>00</dsoBanChuaThue>");
                }
                itemReport.Append("	 <thueGTGT>00</thueGTGT>");
                itemReport.AppendFormat("	 <ghiChu>{0}</ghiChu>", item.InvoiceNote);
                itemReport.Append("</HDonBRa>");
                index++;
            }

            itemReport.Append("</ChiTietHHDVKChiuThue>");
            itemReport.AppendFormat("<tongDThuBRaKChiuThue>{0}</tongDThuBRaKChiuThue>", total);
            itemReport.Append("</HHDVKChiuThue>");
            return itemReport.ToString();
        }

        private string CreateInvoiceUseZeroPercent()
        {
            StringBuilder itemReport = new StringBuilder();
            var invoices = dataInvoiceUsage.items.Where(p => "HHDVThue0".Equals(p.Code));
            itemReport.Append("<HHDVThue0>");
            itemReport.Append("<ChiTietHHDVThue0>");

            decimal total = 0;
            decimal totalTax = 0;
            int index = 1;
            foreach (var item in invoices)
            {
                if (item.IsSummary)
                {
                    total = item.TotalByTax ?? 0;
                    totalTax = item.TotalTaxByTax ?? 0;
                    continue;
                }

                itemReport.AppendFormat("<HDonBRa id=\"ID_{0}\">", index);
                itemReport.AppendFormat("	 <kyHieuMauHDon>{0}</kyHieuMauHDon>", item.InvoiceCode);
                itemReport.AppendFormat("	 <kyHieuHDon>{0}</kyHieuHDon>", item.InvoiceSymbol);
                itemReport.AppendFormat("	 <soHDon>{0}</soHDon>", item.InvoiceNo);
                if (item.ReleasedDate.HasValue)
                {
                    itemReport.AppendFormat("	 <ngayPHanh>{0}</ngayPHanh>", item.ReleasedDate.Value.ToString("dd/MM/yyyy"));
                }
                else
                {
                    itemReport.AppendLine("	 <ngayPHanh/>");
                }

                itemReport.AppendFormat("	 <tenNMUA><![CDATA[{0}]]></tenNMUA>", item.CustomerName);
                itemReport.AppendFormat("	 <mstNMUA>{0}</mstNMUA>", item.TaxCode);
                itemReport.AppendLine("	 <matHang/>");
                if (item.Total.HasValue)
                {
                    itemReport.AppendFormat("	 <dsoBanChuaThue>{0}</dsoBanChuaThue>", item.Total.Value);
                }
                else
                {
                    itemReport.AppendLine("	 <dsoBanChuaThue>00</dsoBanChuaThue>");
                }
                itemReport.Append("	 <thueGTGT>00</thueGTGT>");
                itemReport.AppendFormat("	 <ghiChu>{0}</ghiChu>", item.InvoiceNote);
                itemReport.Append("</HDonBRa>");
                index++;
            }

            itemReport.Append("</ChiTietHHDVThue0>");
            itemReport.AppendFormat("<tongDThuBRaThue0>{0}</tongDThuBRaThue0>", total);
            itemReport.AppendFormat("<tongThueBRaThue0>{0}</tongThueBRaThue0>", totalTax);
            itemReport.Append("</HHDVThue0>");
            return itemReport.ToString();
        }

        private string CreateInvoiceUseFivePercent()
        {
            StringBuilder itemReport = new StringBuilder();
            var invoices = dataInvoiceUsage.items.Where(p => "HHDVThue5".Equals(p.Code));
            itemReport.Append("<HHDVThue5>");
            itemReport.Append("<ChiTietHHDVThue5>");

            decimal total = 0;
            decimal totalTax = 0;
            int index = 1;
            foreach (var item in invoices)
            {
                if (item.IsSummary)
                {
                    total = item.TotalByTax ?? 0;
                    totalTax = item.TotalTaxByTax ?? 0;
                    continue;
                }

                itemReport.AppendFormat("<HDonBRa id=\"ID_{0}\">", index);
                itemReport.AppendFormat("	 <kyHieuMauHDon>{0}</kyHieuMauHDon>", item.InvoiceCode);
                itemReport.AppendFormat("	 <kyHieuHDon>{0}</kyHieuHDon>", item.InvoiceSymbol);
                itemReport.AppendFormat("	 <soHDon>{0}</soHDon>", item.InvoiceNo);
                if (item.ReleasedDate.HasValue)
                {
                    itemReport.AppendFormat("	 <ngayPHanh>{0}</ngayPHanh>", item.ReleasedDate.Value.ToString("dd/MM/yyyy"));
                }
                else
                {
                    itemReport.AppendLine("	 <ngayPHanh/>");
                }

                itemReport.AppendFormat("	 <tenNMUA><![CDATA[{0}]]></tenNMUA>", item.CustomerName);
                itemReport.AppendFormat("	 <mstNMUA>{0}</mstNMUA>", item.TaxCode);
                itemReport.AppendLine("	 <matHang/>");
                if (item.Total.HasValue)
                {
                    itemReport.AppendFormat("	 <dsoBanChuaThue>{0}</dsoBanChuaThue>", item.Total.Value);
                }
                else
                {
                    itemReport.AppendLine("	 <dsoBanChuaThue>00</dsoBanChuaThue>");
                }
                itemReport.Append("	 <thueGTGT>00</thueGTGT>");
                itemReport.AppendFormat("	 <ghiChu>{0}</ghiChu>", item.InvoiceNote);
                itemReport.Append("</HDonBRa>");
                index++;
            }

            itemReport.Append("</ChiTietHHDVThue5>");
            itemReport.AppendFormat("<tongDThuBRaThue5>{0}</tongDThuBRaThue5>", total);
            itemReport.AppendFormat("<tongThueBRaThue5>{0}</tongThueBRaThue5>", totalTax);
            itemReport.Append("</HHDVThue5>");
            return itemReport.ToString();
        }

        private string CreateInvoiceUseTenPercent()
        {
            StringBuilder itemReport = new StringBuilder();
            var invoices = dataInvoiceUsage.items.Where(p => "HHDVThue10".Equals(p.Code));
            itemReport.Append("<HHDVThue10>");
            itemReport.Append("<ChiTietHHDVThue10>");

            decimal total = 0;
            decimal totalTax = 0;
            int index = 1;
            foreach (var item in invoices)
            {
                if (item.IsSummary)
                {
                    total = item.TotalByTax ?? 0;
                    totalTax = item.TotalTaxByTax ?? 0;
                    continue;
                }

                itemReport.AppendFormat("<HDonBRa id=\"ID_{0}\">", index);
                itemReport.AppendFormat("	 <kyHieuMauHDon>{0}</kyHieuMauHDon>", item.InvoiceCode);
                itemReport.AppendFormat("	 <kyHieuHDon>{0}</kyHieuHDon>", item.InvoiceSymbol);
                itemReport.AppendFormat("	 <soHDon>{0}</soHDon>", item.InvoiceNo);
                if (item.ReleasedDate.HasValue)
                {
                    itemReport.AppendFormat("	 <ngayPHanh>{0}</ngayPHanh>", item.ReleasedDate.Value.ToString("dd/MM/yyyy"));
                }
                else
                {
                    itemReport.AppendLine("	 <ngayPHanh/>");
                }

                itemReport.AppendFormat("	 <tenNMUA><![CDATA[{0}]]></tenNMUA>", item.CustomerName);
                itemReport.AppendFormat("	 <mstNMUA>{0}</mstNMUA>", item.TaxCode);
                itemReport.AppendLine("	 <matHang/>");
                if (item.Total.HasValue)
                {
                    itemReport.AppendFormat("	 <dsoBanChuaThue>{0}</dsoBanChuaThue>", item.Total.Value);
                }
                else
                {
                    itemReport.AppendLine("	 <dsoBanChuaThue>00</dsoBanChuaThue>");
                }
                itemReport.Append("	 <thueGTGT>00</thueGTGT>");
                itemReport.AppendFormat("	 <ghiChu>{0}</ghiChu>", item.InvoiceNote);
                itemReport.Append("</HDonBRa>");
                index++;
            }

            itemReport.Append("</ChiTietHHDVThue10>");
            itemReport.AppendFormat("<tongDThuBRaThue10>{0}</tongDThuBRaThue10>", total);
            itemReport.AppendFormat("<tongThueBRaThue10>{0}</tongThueBRaThue10>", totalTax);
            itemReport.Append("</HHDVThue10>");
            return itemReport.ToString();
        }

        private void BuildDateFormDateTo(int month, int year)
        {
            this.DateFrom = new DateTime(year, month, 1);
            this.DateTo = this.DateFrom.AddMonths(1).AddDays(-1);
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
