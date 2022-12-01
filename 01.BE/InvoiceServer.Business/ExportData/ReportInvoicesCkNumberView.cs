using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.ExportData
{
    public class ReportInvoicesCkNumberView
    {
        private readonly SpireProcess spireProcess;
        private readonly InvoiceCkNumberExport reportInvoice;
        private const int rowStartFillData = 2;
        private readonly PrintConfig config;
        private readonly int rowTempalteItem = 10;

        public ReportInvoicesCkNumberView(InvoiceCkNumberExport dataReports, PrintConfig config)
        {
            this.reportInvoice = dataReports;
            this.config = config;
            var choooseFile = "Template_ExcelCkNumber.xlsx";
            string FullPathFileTemplate = string.Format("{0}\\{1}", config.FullPathFileExportData, choooseFile);
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
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
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:J{0}", rowTemplate)], this.spireProcess.Worksheet.Range[string.Format("A{0}:J{0}", targetRow)], true);
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

        private void SetDataItemInvoiceToCells(InvoiceCheckDaily item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.Symbol.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.BranchId.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.CompanyName.ConvertToString();
            if (item.ReleasedDate.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].DateTimeValue = item.ReleasedDate.Value;
                this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
                this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].VerticalAlignment = VerticalAlignType.Center;
            }
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.MinInvoiceNo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.MaxInvoiceNo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = item.Quantity.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Text = item.InvNoResult.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Text = item.InvInfoResult.ConvertToString();
            SetBorder(rowIndex);

        }
        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "report-invoice-ck-number", DateTime.Now.ToString(Formatter.DateTimeFormat));
            string filePathFileName = string.Format("{0}\\Reports\\{1}", fullPathFile, fileName);
            this.spireProcess.Workbook.SaveToFile(filePathFileName, Spire.Xls.ExcelVersion.Version2013);
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = fileName;
            fileInfo.FullPathFileName = filePathFileName;
            return fileInfo;
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
        }
    }
}
