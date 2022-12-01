using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.IO;
namespace InvoiceServer.Business.ExportData
{
    public class ReportInvoicesView
    {
        private readonly SpireProcess spireProcess;
        private readonly InvoiceExport reportInvoice;
        private const int rowStartFillData = 4;
        private readonly PrintConfig config;
        private readonly int rowTempalteItem = 15;
        private readonly SystemSettingInfo systemSettingInfo;
        public ReportInvoicesView(InvoiceExport dataReports, PrintConfig config, SystemSettingInfo _systemSettingInfo, string language)
        {
            this.reportInvoice = dataReports;
            this.config = config;
            var choooseFile = "";
            if (language == "en")
            {
                choooseFile = "report-invoices-en.xlsx";
            }
            else
            {
                choooseFile = "report-invoices-vn.xlsx";
            }
            string FullPathFileTemplate = string.Format("{0}\\{1}", config.FullPathFileExportData, choooseFile);
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
            this.systemSettingInfo = _systemSettingInfo;
        }

        private void CreateRowByData()
        {
            var invoiceItems = this.reportInvoice.Items;
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
            CreateRowByData();
            FillDataToCells();
            return SaveFile(this.config.FullPathFileExportData);
        }

        private void CopyRow(int targetRow, int rowTemplate)
        {
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:N{0}", rowTemplate)], this.spireProcess.Worksheet.Range[string.Format("A{0}:N{0}", targetRow)], true);
        }

        private void FillDataToCells()
        {
            int order = 1;
            int index = rowStartFillData;
            this.reportInvoice.Items.ForEach(p =>
            {
                CopyRow(index, rowTempalteItem);
                SetDataItemInvoiceToCells(p, order, index);

                index++;
                order++;
            });

        }

        private void SetDataItemInvoiceToCells(InvoiceMaster item, int order, int rowIndex)
        {
            var formatAmount = "#,##0";
            if (this.systemSettingInfo.Amount != 0)
            {
                formatAmount = "#,##0.".PadRight(6 + this.systemSettingInfo.Amount, '0');
            }

            if (item.IsOrg == false || (item.IsOrg == null && item.CustomerName == null))
            {
                item.CustomerName = item.PersonContact;
            }

            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.BranchId.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.InvoiceSymbol.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.InvoiceNo.ConvertToString();
            if (item.ReleasedDate.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].DateTimeValue = item.InvoiceDate.Value;
                this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
                this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].VerticalAlignment = VerticalAlignType.Center;
            }
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.CustomerCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.CustomerName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = item.CurrencyCode.ConvertToString();//Loại tiền
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Value2 = item.Total;
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Text = item.TaxName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].Value2 = item.TotalTax;
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].Value2 = item.Sum;
            this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("M{0}", rowIndex)].Text = item.Vat_invoice_type.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("N{0}", rowIndex)].Text = InvoiceProperties.getInvoiceStatus((InvoiceStatus)item.InvoiceStatus);

            SetBorder(rowIndex);
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "report-invoice-mintues-vn", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
            range = this.spireProcess.Worksheet.Range[rowIndex, 8];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 9];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 10];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 11];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 12];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 13];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 14];
            range.BorderAround();
        }
    }
}
