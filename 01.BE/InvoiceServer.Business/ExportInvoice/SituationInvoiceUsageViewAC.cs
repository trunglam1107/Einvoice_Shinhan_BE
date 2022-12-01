using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class SituationInvoiceUsageViewAC : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportSituationInvoiceUsage situationInvoiceUsage;
        private const int rowStartFillData = 13;
        private readonly PrintConfig config;
        public SituationInvoiceUsageViewAC(ReportSituationInvoiceUsage dataReports, PrintConfig config)
        {
            this.situationInvoiceUsage = dataReports;
            this.config = config;
            string FullPathFileTemplate = string.Format("{0}\\{1}", config.FullFolderAssetOfCompany, "SituationInvoiceUsag01.xlsx");
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
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:N{0}", rowStartFillData - 1)], this.spireProcess.Worksheet.Range[string.Format("A{0}:N{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData - 1;
            long TongDauKy = 0;
            long TongDaSuDung = 0;
            long TongCongDaSuDung = 0;
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
                TongDaSuDung += p.SoLuong_DaSuDung;
                TongXoaBo += p.HuyBoSoLuong;
                TongMat += p.MatSoLuong;
                TongHuy += p.HuySoLuong;
                TongCuoiKy += p.CuoiKy_Tong;
                TongCongDaSuDung += p.TongSuDung_Cong;

            });

            this.spireProcess.Worksheet.Range[string.Format("F{0}", index)].Value2 = TongDauKy; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("G{0}", index)].Value2 = TongDaSuDung; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("H{0}", index)].Value2 = TongDaSuDung; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("I{0}", index)].Value2 = TongXoaBo; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("J{0}", index)].Value2 = TongMat; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("K{0}", index)].Value2 = TongHuy; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("L{0}", index)].Value2 = TongCongDaSuDung; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("M{0}", index)].Value2 = TongCuoiKy; //Tổng số
        }

        private void SetDataToCells(ReportUse item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Text = order.ToString();
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceSampleCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.InvoiceSample.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.InvoiceCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = "E";

            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Value2 = item.DauKy_Tong; //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Value2 = item.SoMua_TuSo;
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Value2 = item.SoLuong_DaSuDung;
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Value2 = item.HuyBoSoLuong;//Từ số
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Value2 = item.MatSoLuong;//Đến số
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].Value2 = item.HuySoLuong;
            this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].Value2 = item.TongSuDung_Cong;
            this.spireProcess.Worksheet.Range[string.Format("M{0}", rowIndex)].Value2 = item.CuoiKy_Tong.ToInt(0);

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
            ReplaceData("#Quy", this.situationInvoiceUsage.Precious.ToString());
            ReplaceData("#Nam", this.situationInvoiceUsage.Year.ToString());
            ReplaceData("#TenCongTy", this.situationInvoiceUsage.Company.CompanyName);
            ReplaceData("#MaSoThue", this.situationInvoiceUsage.Company.TaxCode);
            ReplaceData("#DiaChi", this.situationInvoiceUsage.Company.Address);
        }
    }
}

