using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ExportSystemLogView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly SystemLogExport systemLogExport;
        private const int rowStartFillData = 3;
        private readonly PrintConfig config;
        public ExportSystemLogView(SystemLogExport dataReports, PrintConfig config, string language)
        {
            this.systemLogExport = dataReports;
            this.config = config;
            var choooseFile = "TemplateExport_SystemLogs-vn.xlsx";
            if (language == "en")
            {
                choooseFile = "TemplateExport_SystemLogs-en.xlsx";
            }
            string FullPathFileTemplate = Path.Combine(config.FullPathFileAsset, choooseFile);
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }

        private void CreateRowByData()
        {
            var itemReport = this.systemLogExport.Items;
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
            this.systemLogExport.Items.ForEach(p =>
            {
                SetDataToCells(p, order, index);
                index++;
                order++;
            });
        }

        private void SetDataToCells(SystemLogInfo item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.FunctionName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.LogSummary.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.LogType == 0 ? "Success" : item.LogType == 1 ? "Error" : "Warning";
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.LogDate.Value.ToString("dd/MM/yyyy hh:mm:ss tt");
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.LogDetail.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.IP.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = item.UserName.ConvertToString();
            SetBorder(rowIndex);

        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "SystemLogs", DateTime.Now.ToString(Formatter.DateTimeFormat));
            string filePathFileName = string.Format("{0}\\Reports\\{1}", fullPathFile, fileName);
            this.spireProcess.Workbook.SaveToFile(filePathFileName, Spire.Xls.ExcelVersion.Version2013);
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = fileName;
            fileInfo.FullPathFileName = filePathFileName;
            return fileInfo;
        }

        private void SetBorder(int rowIndex)
        {
            for (int i = 1; i <= 8; i++)
            {
                var range = this.spireProcess.Worksheet.Range[rowIndex, i];
                range.BorderAround();
            }
        }
    }
}