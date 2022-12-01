using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using Spire.Xls;
using System;
using System.IO;

namespace InvoiceServer.Business
{
    public class ExportLoginUserView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly LoginUserExport loginUserExport;
        private const int rowStartFillData = 3;
        private readonly PrintConfig config;

        public ExportLoginUserView(LoginUserExport dataReports, PrintConfig config)
        {
            this.loginUserExport = dataReports;
            this.config = config;
            string FullPathFileTemplate = Path.Combine(config.FullPathFileAsset, "TemplateExport_LoginUser.xlsx");
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }
        public ExportFileInfo ExportFile()
        {
            ReplateAlInvoiceHeader();
            CreateRowByData();
            FillDataToDetailInvoice();
            return SaveFile(this.config.FullFolderAssetOfCompany);
        }
        private void CreateRowByData()
        {
            var itemReport = this.loginUserExport.Items;
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
        private void CopyRow(int targetRow)
        {
            this.spireProcess.Worksheet.Copy(this.spireProcess.Worksheet.Range[string.Format("A{0}:K{0}", rowStartFillData - 1)], this.spireProcess.Worksheet.Range[string.Format("A{0}:K{0}", targetRow)], true);
        }
        private void SetDataToCells(LoginUserExportDetail item, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = item.BranchId;
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = item.ChiNhanh;
            this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].Text = item.TenNhanVien;
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = item.TenTaiKhoan;
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = item.SoDienThoai;
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = item.Email;
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = item.TenQuyen;
            this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].Text = item.NgayKichHoat;
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Text = item.NgayHetHieuLuc;
            this.spireProcess.Worksheet.Range[string.Format("K{0}", rowIndex)].Text = item.TrangThai == "True" ? "1" : "0";
            SetBorder(rowIndex);
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
            ReplaceData("#TenCongTy", this.loginUserExport.Company.CompanyName);
            ReplaceData("#MaSoThue", this.loginUserExport.Company.TaxCode);
        }
        private void SetBorder(int rowIndex)
        {
            for (int i = 1; i <= 11; i++)
            {
                var range = this.spireProcess.Worksheet.Range[rowIndex, i];
                range.BorderAround();
            }
        }
        private void FillDataToDetailInvoice()
        {
            int order = 1;
            int index = rowStartFillData - 1;
            this.loginUserExport.Items.ForEach(p =>
            {
                SetDataToCells(p, order, index);
                index++;
                order++;
            });
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
    }
}
