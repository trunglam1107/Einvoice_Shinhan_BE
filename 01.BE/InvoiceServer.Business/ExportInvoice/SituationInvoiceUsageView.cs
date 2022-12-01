using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class SituationInvoiceUsageView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportInvoiceUsing situationInvoiceUsage;
        private const int rowStartFillData = 17;
        private readonly PrintConfig config;
        private readonly string language;
        public SituationInvoiceUsageView(ReportInvoiceUsing dataReports, PrintConfig config, string language = null)
        {
            this.situationInvoiceUsage = dataReports;
            this.config = config;
            this.language = language;
            string FullPathFileTemplate = string.Format("{0}\\{1}", config.FullPathFileAsset, "SituationInvoiceUsage-vn.xlsx");
            if (language == "en")
            {
                FullPathFileTemplate = string.Format("{0}\\{1}", config.FullPathFileAsset, "SituationInvoiceUsage-en.xlsx");
            }
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }

        private void CreateRowByData()
        {
            var invoiceItems = this.situationInvoiceUsage.items;
            if (invoiceItems.Count == 0)
            {
                return;
            }

            int rowNumber = invoiceItems.Count;
            if (rowNumber > 1)
            {
                this.spireProcess.Worksheet.InsertRow(rowStartFillData, rowNumber - 1);
            }

            int targetRow = 0;
            for (int i = 0; i < rowNumber - 1; i++)
            {
                targetRow = rowStartFillData + i;
                CopyRow(targetRow);
            }
        }

        public ExportFileInfo ExportFile()
        {
            ReplateAlInvoiceHeader();
            CreateRowByData();
            FillDataToDetailInvoice();
            return SaveFile(this.config.FullFolderAssetOfCompany);
        }
        private void CopyRow(int targetRow)
        {
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:Y{0}", rowStartFillData - 1)], this.spireProcess.Worksheet.Range[string.Format("A{0}:Y{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData - 1;
            long TongDauKy = 0;
            long TongDaSuDung = 0;
            long SoLuongDaSuDung = 0;
            long TongXoaBo = 0;
            long TongMat = 0;
            long TongHuy = 0;
            long TongCuoiKy = 0;
            this.situationInvoiceUsage.items.ForEach(p =>
            {
                SetDataToCells(p, order, index);
                index++;
                order++;
                TongDauKy += p.DauKy_Tong;
                TongDaSuDung += p.TongSuDung_Cong;
                SoLuongDaSuDung += p.SoLuong_DaSuDung;
                TongXoaBo += p.XoaBoSoLuong;
                TongMat += p.MatSoLuong;
                TongHuy += p.HuyBoSoLuong;
                TongCuoiKy += p.CuoiKy_Tong;

            });

            this.spireProcess.Worksheet.Range[string.Format("G{0}", index)].Value2 = TongDauKy; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("N{0}", index)].Value2 = TongDaSuDung; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("O{0}", index)].Value2 = SoLuongDaSuDung; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("P{0}", index)].Value2 = TongXoaBo; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("R{0}", index)].Value2 = TongMat; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("T{0}", index)].Value2 = TongHuy; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("X{0}", index)].Value2 = TongCuoiKy; //Tổng số
        }

        private void SetDataToCells(ReportMonth item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Text = order.ToString();
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.InvoiceSample.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.InvoiceSampleCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = "E";
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.InvoiceSymbol.ConvertToString();//Ký hiểu

            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.DauKy_Tong.ToString(); //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = item.DauKy_TuSo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Text = item.DauKy_DenSo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Text = item.SoMua_TuSo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].Text = item.SoMua_DenSo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].Text = item.TongSuDung_TuSo.ConvertToString();//Từ số
            this.spireProcess.Worksheet.Range[string.Format("M{0}", rowIndex)].Text = item.TongSuDung_DenSo.ConvertToString();//Đến số
            this.spireProcess.Worksheet.Range[string.Format("N{0}", rowIndex)].Text = item.TongSuDung_Cong.ToString();
            this.spireProcess.Worksheet.Range[string.Format("O{0}", rowIndex)].Text = item.SoLuong_DaSuDung.ToString();

            this.spireProcess.Worksheet.Range[string.Format("P{0}", rowIndex)].Text = item.XoaBoSoLuong.ToString();
            this.spireProcess.Worksheet.Range[string.Format("Q{0}", rowIndex)].Text = item.XoaBoSo.ConvertToString();

            this.spireProcess.Worksheet.Range[string.Format("R{0}", rowIndex)].Text = item.MatSoLuong.ToString();
            this.spireProcess.Worksheet.Range[string.Format("S{0}", rowIndex)].Text = item.MatSo.ConvertToString();

            this.spireProcess.Worksheet.Range[string.Format("T{0}", rowIndex)].Text = item.HuySoLuong.ToString();
            this.spireProcess.Worksheet.Range[string.Format("U{0}", rowIndex)].Text = item.HuySo.ConvertToString();

            this.spireProcess.Worksheet.Range[string.Format("V{0}", rowIndex)].Text = item.CuoiKy_TuSo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("W{0}", rowIndex)].Text = item.CuoiKy_DenSo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("X{0}", rowIndex)].Text = item.CuoiKy_Tong.ToString();
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "SituationInvoiceUsage", DateTime.Now.ToString(Formatter.DateTimeFormat));
            string filePathFileName = string.Format("{0}\\Reports\\{1}", fullPathFile, fileName);
            this.spireProcess.Workbook.SaveToFile(filePathFileName, Spire.Xls.ExcelVersion.Version2013);
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = fileName;
            fileInfo.FullPathFileName = filePathFileName;
            return fileInfo;
        }

        private void ReplaceData(string oldValue, string newValue)
        {
            CellRange[] ranges = this.spireProcess.Worksheet.FindAllString(oldValue, false, false);
            foreach (CellRange range in ranges)
            {
                range.Text = range.Value.Replace(oldValue, newValue);
            }
        }

        private void ReplateAlInvoiceHeader()
        {
            string typeViewReport = "Quý";
            if (this.language == "en")
            {
                typeViewReport = "Quarter";
            }
            if (this.situationInvoiceUsage.IsViewByMonth)
            {
                typeViewReport = "Tháng";
                if (this.language == "en")
                {
                    typeViewReport = "Month";
                }
                this.situationInvoiceUsage.DateFrom = new DateTime(situationInvoiceUsage.Year, situationInvoiceUsage.Precious, 1);
                this.situationInvoiceUsage.DateTo = this.situationInvoiceUsage.DateFrom.AddMonths(1).AddDays(-1);
            }
            else
            {
                int month = situationInvoiceUsage.Precious * 3 - 2;
                this.situationInvoiceUsage.DateFrom = new DateTime(situationInvoiceUsage.Year, month, 1);
                this.situationInvoiceUsage.DateTo = this.situationInvoiceUsage.DateFrom.AddMonths(3).AddDays(-1);
            }

            ReplaceData("#Quy", string.Format("{0} {1}", typeViewReport, this.situationInvoiceUsage.Precious));
            ReplaceData("#TieuChi", typeViewReport);
            ReplaceData("#Nam", this.situationInvoiceUsage.Year.ToString());
            ReplaceData("#TenCongTy", this.situationInvoiceUsage.Branch.CompanyName);
            ReplaceData("#MaSoThue", this.situationInvoiceUsage.Branch.TaxCode);
            ReplaceData("#DiaChi", this.situationInvoiceUsage.Branch.Address);
            ReplaceData("#NgayDauKy", this.situationInvoiceUsage.DateFrom.ToString("dd/MM/yyyy"));
            ReplaceData("#NgayCuoiKy", this.situationInvoiceUsage.DateTo.ToString("dd/MM/yyyy"));
        }
    }
}

