using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Account;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ExportClientAccountView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ClientAccountExport clientAccountExport;
        private const int rowStartFillData = 3;
        private readonly PrintConfig config;
        public ExportClientAccountView(ClientAccountExport dataReports, PrintConfig config)
        {
            this.clientAccountExport = dataReports;
            this.config = config;
            string FullPathFileTemplate = Path.Combine(config.FullPathFileAsset, "TemplateExport_ClientAccount.xlsx");
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }

        private void CreateRowByData()
        {
            var itemReport = this.clientAccountExport.Items;
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
            return SaveFile(this.config.FullPathFileAsset);
        }

        private void CopyRow(int targetRow)
        {
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:I{0}", rowStartFillData - 1)], this.spireProcess.Worksheet.Range[string.Format("A{0}:H{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData - 1;
            this.clientAccountExport.Items.ForEach(p =>
            {
                SetDataToCells(p, order, index);
                index++;
                order++;
            });
        }

        private void SetDataToCells(AccountViewModel item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.UserID.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.UserName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.Email.ConvertToString();
            string active = "";
            if (item.IsActive)
            {
                active = "Đã kích hoạt";
            }
            else
            {
                active = "Chưa kích hoạt";
            }
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = active.ConvertToString();
            if (item.CreatedDate.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].DateTimeValue = item.CreatedDate.Value;
                this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
            }
            if (item.UpdatedDate.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].DateTimeValue = item.UpdatedDate.Value;
                this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
            }
            if (item.LastAccessedTime.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].DateTimeValue = item.LastAccessedTime.Value;
                this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
            }
            if (item.LastChangedpasswordTime.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].DateTimeValue = item.LastChangedpasswordTime.Value;
                this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
            }
            SetBorder(rowIndex);

        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "export-ClientAccount", DateTime.Now.ToString(Formatter.DateTimeFormat));
            string filePathFileName = string.Format("{0}\\Reports\\{1}", fullPathFile, fileName);
            this.spireProcess.Workbook.SaveToFile(filePathFileName, Spire.Xls.ExcelVersion.Version2013);
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = fileName;
            fileInfo.FullPathFileName = filePathFileName;
            return fileInfo;
        }

        private void SetBorder(int rowIndex)
        {
            for (int i = 1; i <= 9; i++)
            {
                var range = this.spireProcess.Worksheet.Range[rowIndex, i];
                range.BorderAround();
            }
        }
    }
}

