using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ReportNotUseInvoiceView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportNoticeCancelInvoice situationInvoiceUsage;
        private const int rowStartFillData = 9;
        private readonly PrintConfig config;
        public ReportNotUseInvoiceView(ReportNoticeCancelInvoice dataReports, PrintConfig config)
        {
            this.situationInvoiceUsage = dataReports;
            this.config = config;
            string FullPathFileTemplate = string.Format("{0}\\{1}", config.FullFolderAssetOfCompany, "Notice-Not-Usel-Invoice.xlsx");
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
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:H{0}", rowStartFillData - 1)], this.spireProcess.Worksheet.Range[string.Format("A{0}:H{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData - 1;
            decimal sumNotUse = 0;
            this.situationInvoiceUsage.items.ForEach(p =>
            {
                SetDataToCells(p, order, index);
                index++;
                order++;
                sumNotUse += (p.NumberTotall ?? 0);
            });

            this.spireProcess.Worksheet.Range[string.Format("F{0}", index)].Value2 = sumNotUse;
        }

        private void SetDataToCells(NoticeCancelInvoice item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceCode.ConvertToString(); //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.InvoiceSample.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.InvoiceSymbol.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.InvoiceSampleCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Value2 = item.NumberTotall;
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.NumberFromUse.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = item.NumberToUse.ConvertToString();
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "Notice-Not-Usel-Invoice", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
            ReplaceData("#TenCongTy", this.situationInvoiceUsage.Company.CompanyName);
            ReplaceData("#MaSoThue", this.situationInvoiceUsage.Company.TaxCode);
            ReplaceData("#DaiDienPhapLuat", this.situationInvoiceUsage.Company.PersonContact.ConvertToString());
            ReplaceData("#NgayBaoCao", DateTime.Now.ToString("dd/MM/yyyy"));
        }
    }
}

