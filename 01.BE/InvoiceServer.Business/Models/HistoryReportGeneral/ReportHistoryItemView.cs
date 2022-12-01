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

namespace InvoiceServer.Business.Models
{
    public class ReportHistoryItemView: IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly List<HistoryReportItem> data;
        private const int rowStartFillData = 2;
        private readonly int rowTempalteItem = 13;

        public ReportHistoryItemView(List<HistoryReportItem> dataHGR)
        {
            this.data = dataHGR;
            var choooseFile = "Template_SendGDT.xlsx";
            string FullPathFileTemplate = string.Format("{0}\\{1}", DefaultFields.GET_TEMPLATE_EXCEL_FOLDER, choooseFile);
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }

        private void CreateRowByData()
        {
            var invoiceItems = this.data;
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
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:M{0}", rowTemplate)], this.spireProcess.Worksheet.Range[string.Format("A{0}:M{0}", targetRow)], true);
        }
        private void FillDataToCells()
        {
            int order = 1;
            int index = rowStartFillData;
            this.data.ForEach(p =>
            {
                CopyRow(index, rowTempalteItem);
                SetDataItemInvoiceToCells(p, order, index);

                index++;
                order++;
            });

        }

        private void SetDataItemInvoiceToCells(HistoryReportItem hrginfo, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = hrginfo.CompanyName.ConvertToString();//Mã cn
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].AutoFitColumns();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = hrginfo.InvoiceNo.ConvertToString();//Tên chi nhánh
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].AutoFitColumns();
            //this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = hrginfo.PeriodsReport == 0 ? "Quý" : "Tháng";//Kỳ tổng hợp
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = hrginfo.Symbol.ConvertToString();//Tháng bao nhiêu
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].AutoFitColumns();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = hrginfo.InvoiceDate.ToString("dd/MM/yyyy").ConvertToString();//Năm bao nhiêu
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].AutoFitColumns();
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = hrginfo.SBTHDLIEU.ConvertToString();//Lần đầu
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = GetSendGDT(hrginfo.CQTStatus).ConvertToString();//Lần bổ sung
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].AutoFitColumns();
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = hrginfo.MessageCode.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].AutoFitColumns();

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
        }

        private string GetSendGDT(int? Status)
        {
            string statusStr = "";
            switch (Status)
            {
                case 1:
                    statusStr = "Thất bại";
                    break;
                case 2:
                    statusStr = "Thành công";
                    break;
                case 3:
                    statusStr = "Đang xử lý";
                    break;
            }
            return statusStr;
        }
    }
}
