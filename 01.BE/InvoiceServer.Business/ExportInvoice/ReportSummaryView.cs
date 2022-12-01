using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ReportSummaryView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportSummaryExport reportSummary;
        private const int rowStartFillData = 9;
        private readonly PrintConfig config;
        private readonly SystemSettingInfo systemSettingInfo;
        public ReportSummaryView(ReportSummaryExport dataReports, PrintConfig config, SystemSettingInfo _systemSettingInfo)
        {
            this.reportSummary = dataReports;
            this.config = config;
            string FullPathFileTemplate = Path.Combine(config.FullPathFileAsset, "report-summary.xlsx");
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
            this.systemSettingInfo = _systemSettingInfo;
        }

        private void CreateRowByData()
        {
            var itemReport = this.reportSummary.Items;
            if (itemReport.Count == 0)
            {
                return;
            }

            int rowNumber = itemReport.Count;
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
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:G{0}", rowStartFillData - 1)], this.spireProcess.Worksheet.Range[string.Format("A{0}:G{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData - 1;
            this.reportSummary.Items.ForEach(p =>
            {
                SetDataToCells(p, order, index);
                index++;
                order++;

            });
        }

        private void SetDataToCells(ReportSummary item, int order, int rowIndex)
        {
            var formatAmount = "#,##0";
            if (this.systemSettingInfo.Amount > 0)
            {
                formatAmount += ".";
                formatAmount = formatAmount.PadRight(6 + this.systemSettingInfo.Amount, '0');
            }
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = (item.ContractDate ?? DateTime.Now).ToString("dd/MM/yyyy"); //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.CompanyName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.CustomerName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.TaxCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.ContracNo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Value2 = item.NumberInvoiceUse;
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].NumberFormat = formatAmount;
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "report_summary", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
            ReplaceData("#Tungay", (this.reportSummary.DateFrom ?? DateTime.Now).ToString("dd/MM/yyyy"));
            ReplaceData("#Denngay", (this.reportSummary.DateTo ?? DateTime.Now).ToString("dd/MM/yyyy"));
            ReplaceData("#Loaihopdong", this.reportSummary.ContractType.ConvertToString());
            ReplaceData("#Doituong", this.reportSummary.CustomerName.ConvertToString());
        }
    }
}

