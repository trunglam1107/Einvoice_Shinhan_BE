using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using System;
using System.IO;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using InvoiceServer.Common.Constants;

namespace InvoiceServer.Business
{
    public class ReportAgenciesView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportAgenciesExport agenciesReport;
        private int rowEndFillContentData = 0;
        private const int rowStartFillData = 8;
        private readonly PrintConfig config;
        public ReportAgenciesView(ReportAgenciesExport dataReports, PrintConfig config)
        {
            this.agenciesReport = dataReports;
            this.config = config;
            string FullPathFileTemplate = Path.Combine(config.FullPathFileAsset, "report-Agencies.xlsx");
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }

        private void CreateRowByData()
        {
            var itemReport = this.agenciesReport.Items;
            if (itemReport.Count == 0)
            {
                return;
            }

            int rowNumber = itemReport.Count;
            if (rowNumber > 1)
            {
                rowEndFillContentData = (rowStartFillData + rowNumber);
                this.spireProcess.Worksheet.InsertRow(rowStartFillData, rowNumber - 1);
            }
            else
            {
                rowEndFillContentData = rowStartFillData;
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
            this.agenciesReport.Items.ForEach(p =>
            {
                SetDataToCells(p, order, index);
                index++;
                order++;

            });
        }

        private void SetDataToCells(ReportAgencies item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = (item.ContractDate ?? DateTime.Now).ToString("dd/MM/yyyy"); //Tổng số
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.ContractNo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.AgenciesName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.PersonContact.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.Mobile.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.Email.ConvertToString(); 
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "report-Agencies", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
            ReplaceData("#Tungay", (this.agenciesReport.DateFrom ?? DateTime.Now).ToString("dd/MM/yyyy"));
            ReplaceData("#Denngay", (this.agenciesReport.DateTo ?? DateTime.Now).ToString("dd/MM/yyyy"));
            ReplaceData("#Doituong", this.agenciesReport.CustomerName.ConvertToString());
        }
    }
}

