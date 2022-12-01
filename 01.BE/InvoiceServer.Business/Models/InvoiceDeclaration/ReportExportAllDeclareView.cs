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
    public class ReportExportAllDeclareView : IXssf
    {
        private readonly SpireProcess spireProcess;
        private readonly ReportExcelDeclare dataDeclare;
        private const int rowStartFillData = 2;
        private readonly int rowTempalteItem = 10;

        public ReportExportAllDeclareView(ReportExcelDeclare dataDeclare)
        {
            this.dataDeclare = dataDeclare;
            var choooseFile = "Template_ExcelDeclare.xlsx";
            string FullPathFileTemplate = string.Format("{0}\\{1}", DefaultFields.GET_TEMPLATE_EXCEL_FOLDER, choooseFile);
            this.spireProcess = new SpireProcess(FullPathFileTemplate);
        }
        private void CreateRowByData()
        {
            var invoiceItems = this.dataDeclare.Items;
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
            this.dataDeclare.Items.ForEach(p =>
            {
                CopyRow(index, rowTempalteItem);
                SetDataItemInvoiceToCells(p, order, index);

                index++;
                order++;
            });

        }
        private void SetDataItemInvoiceToCells(DeclarationInfo declareInfo, int order, int rowIndex)
        {
            this.spireProcess.Worksheet.Range[string.Format("A{0}", rowIndex)].Value2 = order;
            this.spireProcess.Worksheet.Range[string.Format("B{0}", rowIndex)].Text = declareInfo.CompanyName.ConvertToString();//Tên người nộp thuế
            this.spireProcess.Worksheet.Range[string.Format("C{0}", rowIndex)].Text = declareInfo.CompanyTaxCode.ConvertToString();//Mã số thuế
            if (declareInfo.DeclarationDate != null)
            {
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].DateTimeValue = declareInfo.DeclarationDate;//Ngày lập
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
                this.spireProcess.Worksheet.Range[string.Format("D{0}", rowIndex)].HorizontalAlignment = HorizontalAlignType.Center;
            }
            this.spireProcess.Worksheet.Range[string.Format("E{0}", rowIndex)].Text = this.DeclarationTypeStr(declareInfo.DeclarationType).ConvertToString();//Hình thức tờ khai
            this.spireProcess.Worksheet.Range[string.Format("F{0}", rowIndex)].Text = this.HTHDonStr(declareInfo.HTHDon).ConvertToString();//Hình thức hóa đơn
            this.spireProcess.Worksheet.Range[string.Format("G{0}", rowIndex)].Text = declareInfo.MessageCode.ConvertToString();//Mã thông điệp
            this.spireProcess.Worksheet.Range[string.Format("H{0}", rowIndex)].Text = this.DeclareStatus(declareInfo.Status).ConvertToString();//Tình trạng
            if (declareInfo.ApprovedDate.HasValue)
            {
                this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].DateTimeValue = declareInfo.ApprovedDate.Value;//Ngày approved của cơ quan thuế
                this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].NumberFormat = "dd/mm/yyyy";
                this.spireProcess.Worksheet.Range[string.Format("I{0}", rowIndex)].HorizontalAlignment = HorizontalAlignType.Center;
            }
            this.spireProcess.Worksheet.Range[string.Format("J{0}", rowIndex)].Text = declareInfo.SendStatusMessage.ConvertToString(); //trạng thái cqt

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
        }

        public string DeclarationTypeStr(int? declareType)
        {
            string str = null;
            switch (declareType)
            {
                case 1:
                    str = "Đăng ký mới";
                    break;
                case 2:
                    str = "Thay đổi thông tin";
                    break;
            }
            return str;
        }

        public string HTHDonStr(int? hthdon)
        {
            string str = null;
            switch (hthdon)
            {
                case 1:
                    str = "Có mã của cơ quan thuế";
                    break;
                case 2:
                    str = "Không có mã của cơ quan thuế";
                    break;
            }
            return str;
        }

        public string DeclareStatus(int? status)
        {
            string str = null;
            switch (status)
            {
                case 1:
                    str = "Mới";
                    break;
                case 2:
                    str = "Đã duyệt";
                    break;
                case 3:
                    str = "Đang chờ";
                    break;
                case 4:
                    str = "Đã ký";
                    break;
                case 5:
                    str = "Đã hoàn thành";
                    break;
                case 6:
                    str = "Đã hoàn thành";
                    break;
                case 7:
                    str = "Thất bại";
                    break;
                case 8:
                    str = "Đã ký";
                    break;
            }
            return str;
        }
        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}_{1}.xlsx", "report-declaration", DateTime.Now.ToString(Formatter.DateTimeFormat));
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
