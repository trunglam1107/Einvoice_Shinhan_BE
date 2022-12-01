using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class ExportSymbol : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly List<CompanySymbolInfo> dataSymbols;
        private const int rowStartFillData = 2;
        private readonly int rowTempalteItem = 10;

        public ExportSymbol(List<CompanySymbolInfo> dataSymbol)
        {
            this.dataSymbols = dataSymbol;
            var choooseFile = "export-symbols.xlsx";
            string FullPathFileTemplate = string.Format("{0}\\{1}", DefaultFields.GET_TEMPLATE_EXCEL_FOLDER, choooseFile);
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }
        private void CreateRowByData()
        {
            var invoiceItems = dataSymbols;
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
        private void CopyRow(int targetRow, int rowTemplate)
        {
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:J{0}", rowTemplate)], this.spireProcess.Worksheet.Range[string.Format("A{0}:J{0}", targetRow)], true);
        }
        private void FillDataToCells()
        {
            int order = 1;
            int index = rowStartFillData;
            this.dataSymbols.ForEach(p =>
            {
                CopyRow(index, rowTempalteItem);
                SetDataItemInvoiceToCells(p, order, index);

                index++;
                order++;
            });

        }
        private void SetDataItemInvoiceToCells(CompanySymbolInfo symbolInfo, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = symbolInfo.BranchCode.ConvertToString();//Tên người nộp thuế
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = symbolInfo.CompanyName.ConvertToString();//Mã số thuế
           
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = symbolInfo.Symbol.ConvertToString(); //trạng thái cqt
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = symbolInfo.Taxcode.ConvertToString();
            this.SetBorder(rowIndex);
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
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "list-symbol", DateTime.Now.ToString(Formatter.DateTimeFormat));
            string filePathFileName = string.Format("{0}\\Reports\\{1}", fullPathFile, fileName);
            this.spireProcess.Workbook.SaveToFile(filePathFileName, Spire.Xls.ExcelVersion.Version2013);
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = fileName;
            fileInfo.FullPathFileName = filePathFileName;
            return fileInfo;
        }

        public ExportFileInfo ExportFile()
        {
            CreateRowByData();
            FillDataToCells();
            string FullPathFileTemplate = string.Format("{0}", DefaultFields.SAVE_EXCEL_FOLDER);
            return SaveFile(FullPathFileTemplate);
        }
    }
}
