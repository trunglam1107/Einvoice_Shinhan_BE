using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models.Portal
{
    public class ReportInvoicesPortalView
    {
        private readonly SpireProcess spireProcess;
        private readonly InvoiceExportPortal reportInvoice;
        private const int rowStartFillData = 2;
        private readonly PrintConfig config;
        private readonly int rowTempalteItem = 8;
        private readonly SystemSettingInfo systemSettingInfo;
        public ReportInvoicesPortalView(InvoiceExportPortal dataReports, SystemSettingInfo _systemSettingInfo)
        {
            this.reportInvoice = dataReports;
            //this.config = config;
            var choooseFile = "";
            //if (language == "en")
            //{
            //    choooseFile = "report-invoices-en.xlsx";
            //}
            //else
            //{
                
            //}

            choooseFile = "report-invoices-portal.xlsx";
            string FullPathFileTemplate = string.Format("{0}\\{1}", DefaultFields.GET_TEMPLATE_EXCEL_FOLDER, choooseFile);
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
            string FullPathFileTemplate = string.Format("{0}", DefaultFields.SAVE_EXCEL_FOLDER);
            return SaveFile(FullPathFileTemplate);
        }
        private void CopyRow(int targetRow, int rowTemplate)
        {
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:I{0}", rowTemplate)], this.spireProcess.Worksheet.Range[string.Format("A{0}:I{0}", targetRow)], true);
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

        private void SetDataItemInvoiceToCells(PTInvoice item, int order, int rowIndex)
        {
            var formatAmount = "#,##0";
            if (item.CurrencyCode != "VND")
            {
                formatAmount = "#,##0.".PadRight(6 + this.systemSettingInfo.Amount, '0');
            }
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceType_Name.ConvertToString();//Loại hóa đơn
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.TaxCodeBuyer.ConvertToString();//Mã số thuế người bán
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.Symbol.ConvertToString();//Ký hiệu
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.InvoiceNo.ConvertToString();//Số hóa đơn
            if (item.ReleasedDate.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].DateTimeValue = item.ReleasedDate.Value;//Ngày hóa đơn
                this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
                this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].HorizontalAlignment = HorizontalAlignType.Center;
            }
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.CurrencyCode.ConvertToString();//Loại tiền
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Value2 = item.Sum;//Tổng số tiền
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Text = GetInvoiceType(item.InvoiceType).ConvertToString();

            this.SetBorder(rowIndex);
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "report-invoice-portal-vn", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
        }

        public string GetInvoiceType(int? type)
        {
            string invoicetypename = null;
            switch (type)
            {
                case 0:
                    invoicetypename = "Đã ký";
                    break;
                case 1:
                    invoicetypename = "Điều chỉnh";
                    break;
                case 2:
                    invoicetypename = "Hủy";
                    break;
            }
            return invoicetypename;
        }
    }
}
