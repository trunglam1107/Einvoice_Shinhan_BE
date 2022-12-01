using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ReportInvoiceReplacedView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly InvoiceExport reportInvoice;
        private const int rowStartFillData = 5;
        private readonly PrintConfig config;
        private readonly int rowTempalteItem = 15;
        public ReportInvoiceReplacedView(InvoiceExport dataReports, PrintConfig config, string language)
        {
            this.reportInvoice = dataReports;
            this.config = config;
            var choooseFile = "";
            if (language == "en")
            {
                choooseFile = "report-invoice-replaced-en.xlsx";
            }
            else
            {
                choooseFile = "report-invoice-replaced-vn.xlsx";
            }
            string FullPathFileTemplate = string.Format("{0}\\{1}", config.FullPathFileExportData, choooseFile);
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }

        private void CreateRowByData()
        {
            var invoiceItems = this.reportInvoice.Replaced;
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
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:I{0}", rowTemplate)], this.spireProcess.Worksheet.Range[string.Format("A{0}:I{0}", targetRow)], true);
        }

        private void FillDataToCells()
        {
            int order = 1;
            int index = rowStartFillData;
            this.reportInvoice.Replaced.ForEach(p =>
            {
                CopyRow(index, rowTempalteItem);
                SetDataItemInvoiceToCells(p, order, index);

                index++;
                order++;
            });

        }

        private void SetDataItemInvoiceToCells(ReplacedInvoice item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            //this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.Code.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.Symbol.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.No.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.Note.ConvertToString();
            //this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.BeReplacedInvoices[0].CodeSubstitute.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.BeReplacedInvoices[0].SymbolSubstitute.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.BeReplacedInvoices[0].NoSubstitute.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.BeReplacedInvoices[0].NoteSubstitute.ConvertToString();

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
            //range = this.spireProcess.Worksheet.Range[rowIndex, 8];
            //range.BorderAround();
            //range = this.spireProcess.Worksheet.Range[rowIndex, 9];
            //range.BorderAround();
        }
    }
}

