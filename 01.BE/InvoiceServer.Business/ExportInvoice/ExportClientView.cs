using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ExportClientView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ClientExport clientExport;
        private const int rowStartFillData = 3;
        private readonly PrintConfig config;
        public ExportClientView(ClientExport dataReports, PrintConfig config)
        {
            this.clientExport = dataReports;
            this.config = config;
            string FullPathFileTemplate = Path.Combine(config.FullPathFileAsset, "TemplateExport_Client.xlsx");
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }

        private void CreateRowByData()
        {
            var itemReport = this.clientExport.Items;
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
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:L{0}", rowStartFillData - 1)], this.spireProcess.Worksheet.Range[string.Format("A{0}:L{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData - 1;
            this.clientExport.Items.ForEach(p =>
            {
                SetDataToCells(p, order, index);
                index++;
                order++;
            });
        }

        private void SetDataToCells(ClientDetail item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.CustomerCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.TaxCode.ConvertToString();
            if (item.CustomerName != null)
            {
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.CustomerName.ConvertToString();
            }
            else
            {
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.PersonContact.ConvertToString();
            }
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.Address.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.Mobile.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.Email.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = item.ReceivedInvoiceEmail.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Text = item.UseTotalInvoice.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Text = item.TaxIncentives.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].Text = item.SendInvoiceByMonth.ConvertToString();
            if (item.IsOrg != true)
            {
                this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].Text = "Cá nhân";
            }
            else
            {
                this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].Text = "Tổ chức";
            }
            SetBorder(rowIndex);

        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "export-Client", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
            ReplaceData("#TenCongTy", this.clientExport.Company.CompanyName);
            ReplaceData("#MaSoThue", this.clientExport.Company.TaxCode);
        }

        private void SetBorder(int rowIndex)
        {
            for (int i = 1; i <= 12; i++)
            {
                var range = this.spireProcess.Worksheet.Range[rowIndex, i];
                range.BorderAround();
            }
        }
    }
}