using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using Spire.Xls;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ReportInvoiceDataSummaryView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportCombineExport combineReport;
        private const int rowStartFillData = 11;
        private readonly PrintConfig config;
        private readonly int rowTempalteItem = 11;
        private readonly SystemSettingInfo systemSettingInfo;
        private readonly string prefixView = "tháng";
        private readonly int times = 0;
        private readonly int timesNo = 0;
        private readonly int timesUpdate = 0;
        private readonly string lang;
        private readonly string dayTimeHeader = "";
        private readonly string dayTimeFooter = "";
        public ReportInvoiceDataSummaryView(ReportCombineExport dataReports, SystemSettingInfo _systemSettingInfo, PrintConfig config, ConditionReportDetailUse condition)
        {
            this.combineReport = dataReports;
            this.config = config;
            this.times = condition.Time;
            this.timesNo = condition.TimeNo;
            this.timesUpdate = condition.TimesUpdate;
            var choooseFile = "";
            if (condition.Language == "en")
            {
                choooseFile = "Template_THDLHoaDon_EN.xlsx";
                this.lang = "en";
                if (condition.IsMonth > 0)
                {
                    this.dayTimeHeader = getMonthConvert(this.combineReport.Month.ToString(), condition.Language, 1, true) + getYearConvert(this.combineReport.Year.ToString(), condition.Language);
                }
                else
                {
                    this.dayTimeHeader = getMonthConvert(this.combineReport.Month.ToString(), condition.Language, 1, false) + getYearConvert(this.combineReport.Year.ToString(), condition.Language);
                }
                this.dayTimeFooter = getDayConvert(DateTime.Now.Day.ToString(), condition.Language) + getMonthConvert(DateTime.Now.Month.ToString(), condition.Language, 0, true) + getYearConvert(DateTime.Now.Year.ToString(), condition.Language);
            }
            else
            {
                choooseFile = "Template_THDLHoaDon.xlsx";
                this.lang = "vi";
                if (condition.IsMonth > 0)
                {
                    this.dayTimeHeader = getMonthConvert(this.combineReport.Month.ToString(), condition.Language, 0, true) + getYearConvert(this.combineReport.Year.ToString(), condition.Language);
                }
                else
                {
                    this.dayTimeHeader = getMonthConvert(this.combineReport.Month.ToString(), condition.Language, 0, false) + getYearConvert(this.combineReport.Year.ToString(), condition.Language);
                }
                this.dayTimeFooter = getDayConvert(DateTime.Now.Day.ToString(), condition.Language) + getMonthConvert(DateTime.Now.Month.ToString(), condition.Language, 0, true) + getYearConvert(DateTime.Now.Year.ToString(), condition.Language);
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

        public ExportFileInfo ExportFile()
        {
            ReplateAlInvoiceHeader();
            CreateRowByData();
            FillDataToDetailInvoice();
            return SaveFile(this.config.FullFolderAssetOfCompany);
        }

        private void CopyRow(int targetRow, int rowTemplate)
        {
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:M{0}", rowTemplate)], this.spireProcess.Worksheet.Range[string.Format("A{0}:M{0}", targetRow)], true);
        }

        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData;
            this.combineReport.Items.ForEach(p =>
            {
                CopyRow(index, rowTempalteItem);
                SetDataToCells(p, order, index);
                index++;
                order++;
            });
        }

        private void SetDataToCells(ReportInvoiceDetail item, int order, int rowIndex)
        {
            var formatAmount = "#,##0";
            //this.systemSettingInfo.Amount > 0
            //if (item.Currency != "VND")
            //{
            //    formatAmount += ".";
            //    formatAmount = formatAmount.PadRight(6 + this.systemSettingInfo.Amount, '0');
            //}

            //this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order.ToString();
            //this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceCode.ConvertToString() + ", " + item.InvoiceSymbol.ConvertToString();
            //this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.InvoiceNo.ConvertToString();
            //if (item.Created.HasValue)
            //{
            //    this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].DateTimeValue = item.ReleasedDate.Value;
            //    this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
            //}
            //if (item.IsOrg == true)
            //{
            //    this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.CustomerName.ConvertToString();
            //}
            //else
            //{
            //    this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.PersonContact.ConvertToString();
            //}
            //this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.TaxCode.ConvertToString();

            //this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.ProductName.ConvertToString();

            //this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = item.Unit.ConvertToString();
            //this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Value2 = item.Quantity;
            //this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Value2 = item.TotalAmount;
            //this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].NumberFormat = formatAmount;
            //this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].Text = item.TaxName;
            //this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].Value2 = item.AmountTax;
            //this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].NumberFormat = formatAmount;
            //this.spireProcess.Worksheet.Range[string.Format("M{0}", rowIndex)].Value2 = item.Sum;
            //this.spireProcess.Worksheet.Range[string.Format("M{0}", rowIndex)].NumberFormat = formatAmount;

            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order.ToString();
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.InvoiceSymbol.ConvertToString(); //item.InvoiceCode.ConvertToString() + ", " +
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.InvoiceNo.ConvertToString();
            if (item.Created.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].DateTimeValue = item.ReleasedDate.Value;
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
            }
            if (item.IsOrg == true)
            {
                this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.CustomerName.ConvertToString();
            }
            else
            {
                this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.PersonContact.ConvertToString();
            }
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.TaxCode.ConvertToString();

            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.ProductName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Value2 = item.Quantity;//Số lượng
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Value2 = item.Total;//Tổng giá trị hàng hóa, dịch vụ bán ra chưa có thuế GTGT
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Text = item.TaxName;
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].Value2 = item.TotalTax;
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].Value2 = item.SumAmountInvoice;
            this.spireProcess.Worksheet.Range[string.Format("L{0}", rowIndex)].NumberFormat = formatAmount;
            this.spireProcess.Worksheet.Range[string.Format("M{0}", rowIndex)].Text = InvoiceTypeName(item.InvoiceType).ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("N{0}", rowIndex)].Text = item.InvoiceType == 0 ? " "  : (item.ParentSymbol.ConvertToString()  + ", " + item.ParentNo.ConvertToString());
            this.spireProcess.Worksheet.Range[string.Format("N{0}", rowIndex)].HorizontalAlignment = HorizontalAlignType.Left;
            this.spireProcess.Worksheet.Range[string.Format("O{0}", rowIndex)].Text = item.Note.ConvertToString();

            SetBorder(rowIndex);
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "invoice-data-summary-report", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
            ReplaceData("#ThangQuy", this.dayTimeHeader);
            ReplaceData("#NgayThangNam", this.dayTimeFooter);
            ReplaceData("#times", this.times == 0 ? "x" : "");
            ReplaceData("#timeNo", this.timesNo == 0 ? "" : this.timesNo.ToString());
            ReplaceData("#updateTimes", this.timesUpdate == 0 ? "" : this.timesUpdate.ToString());

        }

        private string InvoiceTypeName(int? invoiceType)
        {
            string typeName = "";
            switch (invoiceType)
            {
                case 0:
                    if (this.lang == "en")
                    {
                        typeName = "New";
                    }
                    else
                    {
                        typeName = "Mới";
                    }
                    break;
                case 1:
                    if (this.lang == "en")
                    {
                        typeName = "Canceled";
                    }
                    else
                    {
                        typeName = "Hủy";
                    }
                    break;
                case 2:
                    if (this.lang == "en")
                    {
                        typeName = "Adjustment";
                    }
                    else
                    {
                        typeName = "Điều chỉnh";
                    }
                    break;
                case 3:
                    if (this.lang == "en")
                    {
                        typeName = "Replaced";
                    }
                    else
                    {
                        typeName = "Thay thế";
                    }
                    break;
                case 4:
                    if (this.lang == "en")
                    {
                        typeName = "Explanation";
                    }
                    else
                    {
                        typeName = "Giải trình";
                    }
                    break;
            }

            return typeName;
        }

        private string getDayConvert(string day, string language)
        {
            string resultDay = "";
            if (language == "en")
            {
                switch (day)
                {
                    case "1":
                        resultDay = "1st";
                        break;
                    case "2":
                        resultDay = "2nd";
                        break;
                    case "3":
                        resultDay = "3rd";
                        break;
                    case "4":
                        resultDay = "21st";
                        break;
                    case "5":
                        resultDay = "22nd";
                        break;
                    case "6":
                        resultDay = "23rd";
                        break;
                    default:
                        resultDay = day + "th";
                        break;
                }
            }
            else
            {
                resultDay = "Ngày " + day;
            }
            return resultDay;
        }

        private string getMonthConvert(string month, string language, int type, Boolean isMonth)
        {
            string resultMonth = "";
            if (isMonth)
            {
                if (language == "en")
                {
                    switch (month)
                    {
                        case "1":
                            resultMonth = " January";
                            break;
                        case "2":
                            resultMonth = " February";
                            break;
                        case "3":
                            resultMonth = " March";
                            break;
                        case "4":
                            resultMonth = " April";
                            break;
                        case "5":
                            resultMonth = " May";
                            break;
                        case "6":
                            resultMonth = " June";
                            break;
                        case "7":
                            resultMonth = " July";
                            break;
                        case "8":
                            resultMonth = " August";
                            break;
                        case "9":
                            resultMonth = " September";
                            break;
                        case "10":
                            resultMonth = " October";
                            break;
                        case "11":
                            resultMonth = " November";
                            break;
                        case "12":
                            resultMonth = " December";
                            break;
                    }
                    if (type == 1)
                    {
                        resultMonth += " of";
                    }
                }
                else
                {
                    if (type == 1)
                    {
                        resultMonth = " tháng " + month;
                    }
                    else
                    {
                        resultMonth = " tháng " + month;
                    }

                }
            }
            else
            {
                if (language == "en")
                {
                    switch (month)
                    {
                        case "1":
                            resultMonth = " 1st quarter of";
                            break;
                        case "2":
                            resultMonth = " 2nd quarter of";
                            break;
                        case "3":
                            resultMonth = " 3rd quarter of";
                            break;
                        case "4":
                            resultMonth = " 4th quarter of";
                            break;
                    }
                }
                else
                {
                    resultMonth = " Quý " + month;
                }
            }

            return resultMonth;
        }
        private string getYearConvert(string year, string language)
        {
            String resultYear = "";
            if (language == "vi")
            {
                resultYear = " năm " + year;
            }
            else
            {
                resultYear = " " + year;
            }
            return resultYear;
        }
    }
}

