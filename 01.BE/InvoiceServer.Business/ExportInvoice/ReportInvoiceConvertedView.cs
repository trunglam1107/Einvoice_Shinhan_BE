using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ReportInvoiceConvertedView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportInvoiceConvertedBoard reportInvoiceConvertedBoard;
        private const int rowStartFillData = 6;
        private readonly PrintConfig config;
        private readonly int rowTempalteItem = 15;
        private readonly string lang;
        public ReportInvoiceConvertedView(ReportInvoiceConvertedBoard dataReports, PrintConfig config, string language)
        {
            this.reportInvoiceConvertedBoard = dataReports;
            this.config = config;
            var choooseFile = "";
            if (language == "en")
            {
                choooseFile = "report-invoice-converted-en.xlsx";
                this.lang = "en";
            }
            else
            {
                choooseFile = "report-invoice-converted-vn.xlsx";
                this.lang = "vi";
            }
            string FullPathFileTemplate = string.Format("{0}\\{1}", config.FullPathFileAsset, choooseFile);
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }

        private void CreateRowByData()
        {
            var invoiceItems = this.reportInvoiceConvertedBoard.items;
            if (invoiceItems.Count == 0)
            {
                return;
            }

            int rowNumber = invoiceItems.Count;
            if (rowNumber > 1)
            {
                this.spireProcess.Worksheet.InsertRow(rowStartFillData, rowNumber - 1);
            }
        }

        public ExportFileInfo ExportFile()
        {
            ReplateAlInvoiceHeader();
            CreateRowByData();
            FillDataToCells();
            return SaveFile(this.config.FullFolderAssetOfCompany);
        }

        private void CopyRow(int targetRow, int rowTemplate)
        {
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:G{0}", rowTemplate)], this.spireProcess.Worksheet.Range[string.Format("A{0}:G{0}", targetRow)], true);
        }

        private void FillDataToCells()
        {
            int order = 1;
            int index = rowStartFillData;
            this.reportInvoiceConvertedBoard.items.ForEach(p =>
            {
                CopyRow(index, rowTempalteItem);
                SetDataItemInvoiceToCells(p, order, index);

                index++;
                order++;
            });

        }

        private void SetDataItemInvoiceToCells(ReportInvoiceConverted item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            //this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceCode.ConvertToString();   // Mẫu số
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceSymbol.ConvertToString(); // Ký hiệu
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.InvoiceNo.ConvertToString();     // Số
            if (item.InvoiceDate.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].DateTimeValue = item.InvoiceDate.Value;
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].HorizontalAlignment = HorizontalAlignType.Center;

            }
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.CustomerCode.ConvertToString();  // Số CIF
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.CustomerName.ConvertToString();  // Tên khách hàng
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = this.lang == "en" ? "Converted" : "Đã chuyển đổi";
            SetBorder(rowIndex);
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "report-monthly-board-invoice", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
            ReplaceData("#TenCongTy", this.reportInvoiceConvertedBoard.Company.CompanyName);
        }

        private void SetBorder(int rowIndex)
        {
            var range = this.spireProcess.Worksheet.Range[rowIndex, 1];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 2];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 3];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 4];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 5];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 6];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 7];
            range.BorderAround();
        }
    }
}

