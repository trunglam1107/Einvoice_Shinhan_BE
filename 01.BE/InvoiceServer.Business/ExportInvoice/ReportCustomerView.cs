using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ReportCustomerView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportCustomerExport customerReport;
        private const int rowStartFillData = 8;
        private readonly PrintConfig config;
        private readonly SystemSettingInfo systemSettingInfo;
        public ReportCustomerView(ReportCustomerExport dataReports, PrintConfig config, SystemSettingInfo _systemSettingInfo)
        {
            this.customerReport = dataReports;
            this.config = config;
            string FullPathFileTemplate = Path.Combine(config.FullPathFileAsset, "report-Customer.xlsx");
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
            this.systemSettingInfo = _systemSettingInfo;
        }

        private void CreateRowByData()
        {
            var itemReport = this.customerReport.Items;
            if (itemReport.Count == 0)
            {
                return;
            }

            int rowNumber = itemReport.Count;
            if (rowNumber > 1)
            {
                this.spireProcess.Worksheet.InsertRow(rowStartFillData, rowNumber - 1);
            }

            int targetRow;
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
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:I{0}", rowStartFillData - 1)], this.spireProcess.Worksheet.Range[string.Format("A{0}:I{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData - 1;
            this.customerReport.Items.ForEach(p =>
            {
                SetDataToCells(p, order, index);
                index++;
                order++;

            });
        }

        private void SetDataToCells(ReportCustomer item, int order, int rowIndex)
        {
            var formatAmount = "#,##0";
            if (this.systemSettingInfo.Amount > 0)
            {
                formatAmount += ".";
                formatAmount = formatAmount.PadRight(6 + this.systemSettingInfo.Amount, '0');
            }
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = (item.ContractDate ?? DateTime.Now).ToString("dd/MM/yyyy"); //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.ContractNo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.CustomerName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.TaxCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = (item.PersonContact + " " + item.Email + " " + item.Tel).ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Value2 = item.RumberRegister;
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Value2 = item.NumberUse;
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Value2 = item.Overbalance;
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].NumberFormat = formatAmount;
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "report-Customer", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
            ReplaceData("#Tungay", (this.customerReport.DateFrom ?? DateTime.Now).ToString("dd/MM/yyyy"));
            ReplaceData("#Denngay", (this.customerReport.DateTo ?? DateTime.Now).ToString("dd/MM/yyyy"));
            ReplaceData("#Doituong", this.customerReport.CustomerName.ConvertToString());
        }
    }
}

