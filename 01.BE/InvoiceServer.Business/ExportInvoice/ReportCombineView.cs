using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ReportCombineView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportCombineExport combineReport;
        private const int rowStartFillData = 9;
        private readonly PrintConfig config;
        private readonly SystemSettingInfo systemSettingInfo;
        public ReportCombineView(ReportCombineExport dataReports, PrintConfig config, SystemSettingInfo _systemSettingInfo, string language)
        {
            this.combineReport = dataReports;
            this.config = config;
            var choooseFile = "";
            if (language == "en")
            {
                choooseFile = "Report-Combine-En.xlsx";
            }
            else
            {
                choooseFile = "Report-Combine-Vn.xlsx";
            }
            string FullPathFileTemplate = Path.Combine(config.FullPathFileAsset, choooseFile);
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
            this.systemSettingInfo = _systemSettingInfo;
        }

        private void CreateRowByData()
        {
            var itemReport = this.combineReport.Items;
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
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:X{0}", rowStartFillData - 1)], this.spireProcess.Worksheet.Range[string.Format("A{0}:X{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData - 1;
            this.combineReport.Items.ForEach(p =>
            {
                SetDataToCells(p, order, index);
                index++;
                order++;
            });
        }

        private void SetDataToCells(ReportInvoiceDetail item, int order, int rowIndex)
        {
            var formatAmount = "#,##0";
            if (this.systemSettingInfo.Amount != 0)
            {
                formatAmount = "#,##0.".PadRight(6 + this.systemSettingInfo.Amount, '0');
            }
            var formatPrice = "#,##0";
            if (this.systemSettingInfo.Price != 0)
            {
                formatPrice = "#,##0.".PadRight(6 + this.systemSettingInfo.Price, '0');
            }
            var formatQuantity = "#,##0";
            if (this.systemSettingInfo.Quantity != 0)
            {
                formatQuantity = "#,##0.".PadRight(6 + this.systemSettingInfo.Quantity, '0');
            }

            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            string typePayment = "";
            if (item.TypePayment == 1)
            {
                typePayment = "Thanh toán tiền mặt";
            }
            else if (item.TypePayment == 2)
            {
                typePayment = "Thanh toán chuyển khoản";
            }
            else
            {
                typePayment = "Thanh toán tiền mặt hoặc chuyển khoản";
            }
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = typePayment;
            string invoiceStatus = "";
            if (item.InvoiceStatus == 1)
            {
                invoiceStatus = "Tạo mới";
            }
            else if (item.InvoiceStatus == 2)
            {
                invoiceStatus = "Đã duyệt";
            }
            else
            {
                invoiceStatus = "Đã ký";
            }
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = invoiceStatus;
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.InvoiceCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.InvoiceSymbol.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.InvoiceNo.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = (item.Created ?? DateTime.Now).ToString("dd/MM/yyyy");
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = (item.ReleasedDate ?? DateTime.Now).ToString("dd/MM/yyyy");
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Text = item.CustomerCode.ConvertToString();
            string customerName = item.CustomerName.EmptyNull();
            if (String.IsNullOrEmpty(customerName))
            {
                customerName = item.PersonContact;
            }
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Text = customerName;
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].Text = item.Address.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].Text = item.TaxCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("M{0}", rowIndex)].Text = item.Note.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("N{0}", rowIndex)].Text = item.ProductId.ConvertToString();
            string productName = item.ProductName.ConvertToString();
            if (item.Discount)
            {
                productName = item.DiscountDescription.ConvertToString();
            }
            this.spireProcess.Worksheet.Range[string.Format("O{0}", rowIndex)].Text = productName;

            string discount = "False";
            if (item.Discount)
            {
                discount = "True";
            }
            this.spireProcess.Worksheet.Range[string.Format("Q{0}", rowIndex)].Text = discount;
            this.spireProcess.Worksheet.Range[string.Format("R{0}", rowIndex)].Text = item.Unit.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("S{0}", rowIndex)].Value2 = item.Quantity;
            this.spireProcess.Worksheet.Range[string.Format("S{0}", rowIndex)].NumberFormat = formatQuantity;
            this.spireProcess.Worksheet.Range[string.Format("T{0}", rowIndex)].Value2 = item.UnitPriceAfterTax;
            this.spireProcess.Worksheet.Range[string.Format("T{0}", rowIndex)].NumberFormat = formatPrice;
            this.spireProcess.Worksheet.Range[string.Format("U{0}", rowIndex)].Value2 = item.Price;
            this.spireProcess.Worksheet.Range[string.Format("U{0}", rowIndex)].NumberFormat = formatPrice;
            this.spireProcess.Worksheet.Range[string.Format("V{0}", rowIndex)].Value2 = item.TotalAmount;
            this.spireProcess.Worksheet.Range[string.Format("V{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("W{0}", rowIndex)].Text = item.TaxName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("X{0}", rowIndex)].Value2 = item.AmountTax;
            this.spireProcess.Worksheet.Range[string.Format("X{0}", rowIndex)].NumberFormat = formatAmount;

            SetBorder(rowIndex);

        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "report-Combine", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
            ReplaceData("#TenCongTy", this.combineReport.Company.CompanyName);
            ReplaceData("#MaSoThue", this.combineReport.Company.TaxCode);
        }

        private void SetBorder(int rowIndex)
        {
            for (int i = 1; i <= 24; i++)
            {
                var range = this.spireProcess.Worksheet.Range[rowIndex, i];
                range.BorderAround();
            }
        }
    }
}

