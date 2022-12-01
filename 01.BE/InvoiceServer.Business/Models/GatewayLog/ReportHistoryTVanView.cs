using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models.GatewayLog
{
    public class ReportHistoryTVanView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportCompineHistoryTVanExport combineReport;
        private const int rowStartFillData = 4;
        private readonly PrintConfig config;
        private readonly int rowTempalteItem = 4;
        private readonly SystemSettingInfo systemSettingInfo;

        public ReportHistoryTVanView(ReportCompineHistoryTVanExport dataReports, SystemSettingInfo _systemSettingInfo, PrintConfig config, ConditionSearchGatewaylog condition)
        {
            this.combineReport = dataReports;
            this.config = config;
            var choooseFile = "";
            if (condition.language == "en")
            {
                choooseFile = "Template_TVan_en.xlsx";
            }
            else
            {
                choooseFile = "Template_TVan_vn.xlsx";
            }
           
            string FullPathFileTemplate = Path.Combine(DefaultFields.EXPORT_TEMPLATE_FOLDER, choooseFile);
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

        private void CopyRow(int targetRow, int rowTemplate)
        {
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:M{0}", rowTemplate)], this.spireProcess.Worksheet.Range[string.Format("A{0}:M{0}", targetRow)], true);
        }

        private void SetDataToCells(GatewaylogDetail item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order.ToString();
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.Name.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.Body.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.ObjectName.ConvertToString();
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.IP.ConvertToString();
            if (item.CreatedDate.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].DateTimeValue = item.CreatedDate.Value;
                this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
                this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Style.HorizontalAlignment = Spire.Xls.HorizontalAlignType.Center;
            }
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.CreatedBy.ConvertToString();
            SetBorder(rowIndex);
        }
        private void ReplateAlInvoiceHeader()
        {
            
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
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "report-history-tvan", DateTime.Now.ToString(Formatter.DateTimeFormat));
            string filePathFileName = string.Format("{0}\\Reports\\{1}", fullPathFile, fileName);
            this.spireProcess.Workbook.SaveToFile(filePathFileName, Spire.Xls.ExcelVersion.Version2013);
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = fileName;
            fileInfo.FullPathFileName = filePathFileName;
            return fileInfo;
        }
    }
}
