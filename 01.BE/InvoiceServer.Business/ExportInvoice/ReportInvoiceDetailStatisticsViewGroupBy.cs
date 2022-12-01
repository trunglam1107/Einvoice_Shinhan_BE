using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.ExportInvoice.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ReportInvoiceDetailStatisticsViewGroupBy : IExportInvoiceBaseOnLang
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportCombineExport combineReport;
        private const int rowStartFillData = 5;
        private readonly PrintConfig config;
        private readonly int rowTempalteItem = 10;
        private readonly SystemSettingInfo systemSettingInfo;
        public ReportInvoiceDetailStatisticsViewGroupBy(ReportCombineExport dataReports, SystemSettingInfo _systemSettingInfo, PrintConfig config, string language)
        {
            this.combineReport = dataReports;
            this.config = config;
            var choooseFile = "";
            if (language == "en")
            {
                choooseFile = "invoice-detail-statistics-groupby-en.xlsx";
            }
            else
            {
                choooseFile = "invoice-detail-statistics-groupby-vn.xlsx";
            }
            string FullPathFileTemplate = string.Format("{0}\\{1}", config.FullPathFileAsset, choooseFile);
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
        }

        public ExportFileInfo ExportFile(string lang)
        {
            CreateRowByData();
            FillDataToDetailInvoice(lang);
            return SaveFile(this.config.FullFolderAssetOfCompany);
        }

        private void CopyRow(int targetRow, int rowTemplate)
        {
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:M{0}", rowTemplate)], this.spireProcess.Worksheet.Range[string.Format("A{0}:M{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice(string lang)
        {
            int order = 1;
            int index = rowStartFillData;
            this.combineReport.Items.ForEach(p =>
            {
                CopyRow(index, rowTempalteItem);
                SetDataToCells(p, order, index, lang);
                index++;
                order++;
            });
        }

        private void SetDataToCells(ReportInvoiceDetail item, int order, int rowIndex, string lang)
        {
            var formatAmount = "#,##0";
            if (this.systemSettingInfo.Amount > 0)
            {
                formatAmount += ".";
                formatAmount = formatAmount.PadRight(6 + this.systemSettingInfo.Amount, '0');
            }

            var formatExchangeRate = "#,##0";
            if (this.systemSettingInfo.ExchangeRate > 0)
            {
                formatExchangeRate += ".";
                formatExchangeRate = formatExchangeRate.PadRight(6 + this.systemSettingInfo.ExchangeRate, '0');
            }

            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order.ToString();
            //this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceSymbol.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.InvoiceNo.ConvertToString();
            if (item.Created.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].DateTimeValue = item.ReleasedDate.Value;
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].VerticalAlignment = Spire.Xls.VerticalAlignType.Center;
            }
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.CustomerCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.TaxCode.ConvertToString();
            if (item.IsOrg == true)
            {
                this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.CustomerName.ConvertToString();
            }
            else
            {
                this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.PersonContact.ConvertToString();
            }
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = item.Address.ConvertToString();

            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Text = item.Currency.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Value2 = item.CurrencyExchangeRate;
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].NumberFormat = formatExchangeRate;
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].Value2 = item.TotalAmount;
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].Text = item.TaxName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("M{0}", rowIndex)].Value2 = item.AmountTax;
            this.spireProcess.Worksheet.Range[string.Format("M{0}", rowIndex)].NumberFormat = formatAmount;

            this.spireProcess.Worksheet.Range[string.Format("N{0}", rowIndex)].Value2 = item.Sum;
            this.spireProcess.Worksheet.Range[string.Format("N{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("O{0}", rowIndex)].Text = InvoiceProperties.getInvoiceStatus((InvoiceStatus)item.InvoiceStatus, lang);

            SetBorder(rowIndex);
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "invoice-detail-statistics-general", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
            range = this.spireProcess.Worksheet.Range[rowIndex, 8];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 9];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 10];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 11];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 12];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 13];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 14];
            range.BorderAround();
            range = this.spireProcess.Worksheet.Range[rowIndex, 15];
            range.BorderAround();
            //range = this.spireProcess.Worksheet.Range[rowIndex, 16];
            //range.BorderAround();
        }
    }
}

