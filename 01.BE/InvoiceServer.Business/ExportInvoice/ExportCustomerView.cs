using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ExportCustomerView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly CustomerExport customerExport;
        private const int rowStartFillData = 3;
        private readonly PrintConfig config;
        public ExportCustomerView(CustomerExport dataReports, PrintConfig config, EmailConfig emailConfig)
        {
            this.customerExport = dataReports;
            this.config = config;
            string FullPathFileTemplate = Path.Combine(emailConfig.FolderExportTemplate, "TemplateExport_Company.xlsx");
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }



        private void CreateRowByData()
        {
            var itemReport = this.customerExport.Items;
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
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:U{0}", rowStartFillData - 1)], this.spireProcess.Worksheet.Range[string.Format("A{0}:U{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData - 1;
            this.customerExport.Items.ForEach(p =>
            {
                SetDataToCells(p, order, index);
                index++;
                order++;
            });
        }

        private void SetDataToCells(CustomerInfo item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.BranchId.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.CompanyName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.TaxCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.LevelCustomer.ConvertToString();
            if (item.Company2 != null)
            {
                this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.Company2.BranchId.ConvertToString();
            }
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.Address.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = item.Delegate.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Text = item.Position.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Text = item.Tel.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].Text = item.Fax.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].Text = item.Email.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("M{0}", rowIndex)].Text = item.WebSite.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("N{0}", rowIndex)].Text = item.BankAccount.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("O{0}", rowIndex)].Text = item.AccountHolder.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("P{0}", rowIndex)].Text = item.BankName.ConvertToString();

            if (item.City != null)
            {
                this.spireProcess.Worksheet.Range[string.Format("Q{0}", rowIndex)].Text = item.City.Name.ConvertToString();
            }
            if (item.TaxDepartment != null)
            {
                this.spireProcess.Worksheet.Range[string.Format("R{0}", rowIndex)].Text = item.TaxDepartment.Name.ConvertToString();
            }
            if (item.Active)
            {
                this.spireProcess.Worksheet.Range[string.Format("S{0}", rowIndex)].Text = "Kích hoạt";
            }
            else
            {
                this.spireProcess.Worksheet.Range[string.Format("S{0}", rowIndex)].Text = "Không kích hoạt";
            }
            this.spireProcess.Worksheet.Range[string.Format("T{0}", rowIndex)].Text = item.PersonContact.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("U{0}", rowIndex)].Text = item.Mobile.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("V{0}", rowIndex)].Text = item.EmailContract.ConvertToString();

            SetBorder(rowIndex);

        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "export-Customer", DateTime.Now.ToString(Formatter.DateTimeFormat));
            string filePathFileName = string.Format("{0}\\Reports\\{1}", fullPathFile, fileName);
            this.spireProcess.Workbook.SaveToFile(filePathFileName, Spire.Xls.ExcelVersion.Version2013);
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = fileName;
            fileInfo.FullPathFileName = filePathFileName;
            return fileInfo;
        }

        private void SetBorder(int rowIndex)
        {
            for (int i = 1; i <= 22; i++)
            {
                var range = this.spireProcess.Worksheet.Range[rowIndex, i];
                range.BorderAround();
            }
        }
    }
}