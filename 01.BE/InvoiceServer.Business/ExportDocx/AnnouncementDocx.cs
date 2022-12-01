using DocumentFormat.OpenXml.Packaging;
using InvoiceServer.Business.Models;
using Spire.Doc;
using System;
using System.IO;
using System.Security;
using System.Text;

namespace InvoiceServer.Business
{
    public class AnnouncementDocx
    {
        public readonly AnnouncementDownload AnnouncementInfo;
        public readonly PrintConfig Config;
        private readonly SystemSettingInfo systemSettingInfo;
        private readonly bool IsPdf;

        public AnnouncementDocx(AnnouncementDownload announcementInfo, PrintConfig config, SystemSettingInfo _systemSettingInfo, bool isPdf)
        {
            this.Config = config;
            this.AnnouncementInfo = announcementInfo;
            this.systemSettingInfo = _systemSettingInfo;
            this.IsPdf = isPdf;
        }
        public ExportFileInfo ExportFileInfo(bool isWord)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();

            string fullPathFileContainContract = CreateFolderContainReport();
            fileInfo.FileName = string.Format("{0}.docx", AnnouncementInfo.Id);
            string fullPathFile = this.Config.FullPathFileAsset;
            string filePathFileName = null;
            if (this.AnnouncementInfo.AnnouncementType == 1)
            {
                filePathFileName = string.Format("{0}\\Mau_bien_ban_dieu_chinh_hoa_don.docx", fullPathFile);
            }
            else if (this.AnnouncementInfo.AnnouncementType == 2)
            {
                filePathFileName = string.Format("{0}\\Bienbanthaythehoadon.docx", fullPathFile);
            }
            else if (this.AnnouncementInfo.AnnouncementType == 3)
            {
                filePathFileName = string.Format("{0}\\Bienbanhuyhoadon.docx", fullPathFile);
            }

            fileInfo.FullPathFileName = Path.Combine(fullPathFileContainContract, fileInfo.FileName);
            File.Copy(filePathFileName, fileInfo.FullPathFileName, true);
            LoadDocument(fileInfo.FullPathFileName, isWord);

            if (this.IsPdf)
            {
                ExportFileInfo fileInfoPdf = new ExportFileInfo();
                CreateFolderContainReport();
                fileInfoPdf.FileName = string.Format("{0}.pdf", AnnouncementInfo.Id);
                fileInfoPdf.FullPathFileName = Path.Combine(fullPathFileContainContract, fileInfoPdf.FileName);
                Spire.Doc.Document document = new Spire.Doc.Document();
                document.LoadFromFile(fileInfo.FullPathFileName);
                document.SaveToFile(fileInfoPdf.FullPathFileName, FileFormat.PDF);

                return fileInfoPdf;
            }

            return fileInfo;
        }


        public ExportFileInfo ExportFileInfoMultipleCancel(bool isWord)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();

            string fullPathFileContainContract = CreateFolderContainReportMultipleCancel();
            fileInfo.FileName = string.Format("{0}.docx", AnnouncementInfo.Id);
            string fullPathFile = this.Config.FullPathFileAsset;
            string filePathFileName = null;
            if (this.AnnouncementInfo.AnnouncementType == 1)
            {
                filePathFileName = string.Format("{0}\\Mau_bien_ban_dieu_chinh_hoa_don.docx", fullPathFile);
            }
            else if (this.AnnouncementInfo.AnnouncementType == 2)
            {
                filePathFileName = string.Format("{0}\\Bienbanthaythehoadon.docx", fullPathFile);
            }
            else if (this.AnnouncementInfo.AnnouncementType == 3)
            {
                filePathFileName = string.Format("{0}\\Bienbanhuyhoadon.docx", fullPathFile);
            }

            fileInfo.FullPathFileName = Path.Combine(fullPathFileContainContract, fileInfo.FileName);
            File.Copy(filePathFileName, fileInfo.FullPathFileName, true);
            LoadDocument(fileInfo.FullPathFileName, isWord);

            if (this.IsPdf)
            {
                ExportFileInfo fileInfoPdf = new ExportFileInfo();
                CreateFolderContainReportMultipleCancel();
                fileInfoPdf.FileName = string.Format("{0}.pdf", AnnouncementInfo.Id);
                fileInfoPdf.FullPathFileName = Path.Combine(fullPathFileContainContract, fileInfoPdf.FileName);
                Spire.Doc.Document document = new Spire.Doc.Document();
                document.LoadFromFile(fileInfo.FullPathFileName);
                document.SaveToFile(fileInfoPdf.FullPathFileName, FileFormat.PDF);

                return fileInfoPdf;
            }

            return fileInfo;
        }

        private void LoadDocument(string fullPathFile, bool isWord)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(fullPathFile, true))
            {
                var docText = new StringBuilder();
                var formatAmount = "{0:N" + this.systemSettingInfo.Amount + "}";

                using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                {
                    docText = new StringBuilder(sr.ReadToEnd());
                }

                if (isWord)
                    this.AnnouncementInfo.LegalGrounds = this.AnnouncementInfo.LegalGrounds.Replace("\n", "<w:br/>");
                else
                    this.AnnouncementInfo.LegalGrounds = this.AnnouncementInfo.LegalGrounds.Replace("\n", "\n\r");
                docText = docText.Replace("«$LegalGrounds»", (this.AnnouncementInfo.LegalGrounds ?? string.Empty));
                docText = docText.Replace("«$Minutesno»", (this.AnnouncementInfo.Minutesno ?? string.Empty));
                // Thay đổi là ngày biên bản
                docText = docText.Replace("«$Day»", this.AnnouncementInfo.AnnouncementDate.Day.ToString());
                docText = docText.Replace("«$Month»", this.AnnouncementInfo.AnnouncementDate.Month.ToString());
                docText = docText.Replace("«$Year»", this.AnnouncementInfo.AnnouncementDate.Year.ToString());

                docText = docText.Replace("«$TenCongTyA»", (SecurityElement.Escape(this.AnnouncementInfo.CompanyName) ?? string.Empty));
                docText = docText.Replace("«$DiaChiA»", (SecurityElement.Escape(this.AnnouncementInfo.CompanyAddress) ?? string.Empty));
                docText = docText.Replace("«$DienThoaiA»", (this.AnnouncementInfo.CompanyTel ?? string.Empty));
                docText = docText.Replace("«$DoOngBaA»", (SecurityElement.Escape(this.AnnouncementInfo.CompanyRepresentative) ?? string.Empty));
                docText = docText.Replace("«$MSTA»", (this.AnnouncementInfo.CompanyTaxCode ?? string.Empty));
                docText = docText.Replace("«$ChucVuA»", (this.AnnouncementInfo.CompanyPosition ?? string.Empty));

                docText = docText.Replace("«$TenCongTyB»", (SecurityElement.Escape(this.AnnouncementInfo.ClientName) ?? string.Empty));
                docText = docText.Replace("«$DiaChiB»", (SecurityElement.Escape(this.AnnouncementInfo.ClientAddress) ?? string.Empty));
                docText = docText.Replace("«$DienThoaiB»", (this.AnnouncementInfo.ClientTel ?? string.Empty));
                docText = docText.Replace("«$DoOngBaB»", (SecurityElement.Escape(this.AnnouncementInfo.ClientRepresentative) ?? string.Empty));
                docText = docText.Replace("«$MSTB»", (this.AnnouncementInfo.ClientTaxCode ?? string.Empty));
                docText = docText.Replace("«$ChucVuB»", (this.AnnouncementInfo.ClientPosition ?? string.Empty));

                //docText = docText.Replace("«$MauSo»", (this.AnnouncementInfo.Denominator ?? string.Empty));
                docText = docText.Replace("«$KyHieu»", (this.AnnouncementInfo.Symbol ?? string.Empty));
                docText = docText.Replace("«$So»", (this.AnnouncementInfo.InvoiceNo ?? string.Empty));
                //Thay đổi ngày hóa đơn
                docText = docText.Replace("«$InvoiceDate»", (this.AnnouncementInfo.InvoiceDate.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)));

                var x = this.AnnouncementInfo.TotalAmount - Math.Truncate(this.AnnouncementInfo.TotalAmount ?? 0);
                if (x > 0)
                {
                    docText = docText.Replace("«$GiaTriHoaDon»", String.Format(formatAmount, this.AnnouncementInfo.TotalAmount));
                }
                else
                {
                    docText = docText.Replace("«$GiaTriHoaDon»", String.Format("{0:N0}", this.AnnouncementInfo.TotalAmount));
                }
                docText = docText.Replace("«$TienTe»", (this.AnnouncementInfo.CurrencyCode ?? string.Empty));
                docText = docText.Replace("«$TenDichVu»", (this.AnnouncementInfo.ServiceName ?? string.Empty));
                docText = docText.Replace("«$LyDoDieuChinh»", (SecurityElement.Escape(this.AnnouncementInfo.Reasion) ?? string.Empty));
                docText = docText.Replace("«$TruocGhiLa»", (SecurityElement.Escape(this.AnnouncementInfo.BeforeContain) ?? string.Empty));
                docText = docText.Replace("«$NayDieuChinhLa»", (SecurityElement.Escape(this.AnnouncementInfo.ChangeContain) ?? string.Empty));

                using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create), Encoding.UTF8))
                {
                    sw.Write(docText.ToString());
                }
            }
        }


        private string CreateFolderContainReport()
        {
            string fullPathForlder = Path.Combine(this.Config.FullPathInvoiceFileOfCompany, "Announcements");
            if (!Directory.Exists(fullPathForlder))
            {
                Directory.CreateDirectory(fullPathForlder);
            }

            return fullPathForlder;
        }

        private string CreateFolderContainReportMultipleCancel()
        {
            string fullPathForlder = Path.Combine(this.Config.FullPathInvoiceFileOfCompany, "Announcements");
            if (!Directory.Exists(fullPathForlder))
            {
                Directory.CreateDirectory(fullPathForlder);
            }

            return fullPathForlder;
        }
    }
}
