using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ReportMothlyBoardView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReporMonthlyBoard mothlyBoardInvoice;
        private const int rowStartFillData = 17;
        private readonly PrintConfig config;
        private readonly int rowTempalteCategory = 14;
        private readonly int rowTempalteItem = 15;
        private readonly int rowTempalteSum = 16;
        private readonly string prefixView = "tháng";
        private readonly SystemSettingInfo systemSettingInfo;

        public ReportMothlyBoardView(ReporMonthlyBoard dataReports, SystemSettingInfo _systemSettingInfo, PrintConfig config, bool isMonth = false, string language = null)
        {
            this.mothlyBoardInvoice = dataReports;
            this.config = config;
            string FullPathFileTemplate = string.Format("{0}\\{1}", config.FullPathFileAsset, "report-monthly-board-invoice-vn.xlsx");
            if (language == "en")
            {
                FullPathFileTemplate = string.Format("{0}\\{1}", config.FullPathFileAsset, "report-monthly-board-invoice-en.xlsx");
                prefixView = "month";
            }
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
            if (!isMonth)
            {
                prefixView = "quý";
                if (language == "en")
                {
                    prefixView = "quarter";
                }
            }

            this.systemSettingInfo = _systemSettingInfo;
        }

        private void CreateRowByData()
        {
            var invoiceItems = this.mothlyBoardInvoice.items;
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
            FillDataToDetailInvoice();
            DeleteRowTemplate();
            return SaveFile(this.config.FullFolderAssetOfCompany);
        }

        private void CopyRow(int targetRow, int rowTemplate)
        {
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:H{0}", rowTemplate)], this.spireProcess.Worksheet.Range[string.Format("A{0}:I{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData;
            this.mothlyBoardInvoice.items.ForEach(p =>
            {
                if (p.IsCategoryTax)
                {
                    CopyRow(index, rowTempalteCategory);
                    SetDataCategoryTaxToCells(p, index);
                    order = 0;
                }
                else if (p.IsSummary)
                {
                    CopyRow(index, rowTempalteSum);
                    SetDataSumToCells(p, index);
                    order = 0;
                }
                else
                {
                    CopyRow(index, rowTempalteItem);
                    SetDataItemInvoiceToCells(p, order, index);
                }

                index++;
                order++;
            });

        }

        private void SetDataItemInvoiceToCells(ReportListInvoices item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            //this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceSymbol.ConvertToString(); //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceNo.ConvertToString();
            if (item.ReleasedDate.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].DateTimeValue = item.ReleasedDate.Value;
                this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
                this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].VerticalAlignment = VerticalAlignType.Center;
            }

            var formatAmount = "#,##0";
            if (this.systemSettingInfo.ConvertedAmount > 0)
            {
                formatAmount += ".";
                formatAmount = formatAmount.PadRight(6 + this.systemSettingInfo.ConvertedAmount, '0');
            }

            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.CustomerName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.TaxCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Value2 = item.Total;
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Value2 = item.TotalTax;
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = item.InvoiceNote.ConvertToString();
        }

        private void SetDataCategoryTaxToCells(ReportListInvoices item, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}:H{0}", rowIndex)].Merge();
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Text = item.InvoiceName+":";
        }

        private void SetDataSumToCells(ReportListInvoices item, int rowIndex)
        {
            var formatAmount = "#,##0";
            if (this.systemSettingInfo.ConvertedAmount > 0)
            {
                formatAmount += ".";
                formatAmount = formatAmount.PadRight(6 + this.systemSettingInfo.ConvertedAmount, '0');
            }

            this.spireProcess.Worksheet.Range[string.Format("A{0}:E{0}", rowIndex)].Merge();
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Value2 = item.TotalByTax;
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Value2 = item.TotalTaxByTax;
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].NumberFormat = formatAmount;
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
            var formatAmount = "#,##0";
            if (this.systemSettingInfo.ConvertedAmount > 0)
            {
                formatAmount += ".";
                formatAmount = formatAmount.PadRight(6 + this.systemSettingInfo.ConvertedAmount, '0');
            }

            ReplaceData("#TenCongTy", this.mothlyBoardInvoice.Company.CompanyName);
            ReplaceData("#MaSoThue", this.mothlyBoardInvoice.Company.TaxCode);
            ReplaceData("#ThangXen", string.Format("{0} {1}", prefixView, this.mothlyBoardInvoice.Month));
            ReplaceData("#NamXem", this.mothlyBoardInvoice.Year.ToString());
            ReplaceData("#NgayIn", DateTime.Now.Day.ToString());
            ReplaceData("#ThangIn", DateTime.Now.Month.ToString());
            ReplaceData("#NamIn", DateTime.Now.Year.ToString());
            //ReplaceData("#Tongsotienbanra", String.Format($"{{0:{formatAmount}}}", this.mothlyBoardInvoice.Total));
            ReplaceData("#Tongsobanra", String.Format($"{{0:{formatAmount}}}", this.mothlyBoardInvoice.TotalBeforeTax));
            ReplaceData("#TongsogiatriGTGT", String.Format($"{{0:{formatAmount}}}", this.mothlyBoardInvoice.TotalOfTax));
        }

        private void DeleteRowTemplate()
        {
            this.spireProcess.Worksheet.DeleteRow(rowTempalteSum);
            this.spireProcess.Worksheet.DeleteRow(rowTempalteItem);
            this.spireProcess.Worksheet.DeleteRow(rowTempalteCategory);
        }
    }
}

