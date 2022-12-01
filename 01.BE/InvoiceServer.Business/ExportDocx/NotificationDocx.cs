using DocumentFormat.OpenXml.Packaging;
using InvoiceServer.Business.Models;
using Spire.Doc;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;

namespace InvoiceServer.Business
{
    public class NotificationDocx
    {
        public readonly UseInvoiceInfo NotificationInfo;
        public readonly PrintConfig Config;
        private readonly SystemSettingInfo systemSettingInfo;
        private readonly bool IsPdf;

        public NotificationDocx(UseInvoiceInfo NotificationInfo, PrintConfig config, SystemSettingInfo _systemSettingInfo, bool isPdf)
        {
            this.Config = config;
            this.NotificationInfo = NotificationInfo;
            this.systemSettingInfo = _systemSettingInfo;
            this.IsPdf = isPdf;
        }
        public ExportFileInfo ExportFileInfo(bool isWord)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();

            string fullPathFileContainContract = CreateFolderContainReport();
            fileInfo.FileName = string.Format("{0}.docx", NotificationInfo.Id);
            string fullPathFile = this.Config.FullPathFileAsset;
            string filePathFileName = string.Format("{0}\\Thongbaophathanh.docx", fullPathFile);

            fileInfo.FullPathFileName = Path.Combine(fullPathFileContainContract, fileInfo.FileName);
            File.Copy(filePathFileName, fileInfo.FullPathFileName, true);
            LoadDocument(fileInfo.FullPathFileName, isWord);

            if (this.IsPdf)
            {
                ExportFileInfo fileInfoPdf = new ExportFileInfo();
                CreateFolderContainReport();
                fileInfoPdf.FileName = string.Format("{0}.pdf", NotificationInfo.Id);
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
                docText = docText.Replace("«$CompanyName»", (this.NotificationInfo.CompanyName ?? string.Empty));
                docText = docText.Replace("«$City»", (this.NotificationInfo.CityName ?? string.Empty));
                docText = docText.Replace("«$TaxCode»", (this.NotificationInfo.TaxCode ?? string.Empty));
                docText = docText.Replace("«$Tel»", (this.NotificationInfo.Tel ?? string.Empty));
                docText = docText.Replace("«$Day»", this.NotificationInfo.CreatedDate.Value.ToString("dd"));
                docText = docText.Replace("«$Month»", this.NotificationInfo.CreatedDate.Value.ToString("MM"));
                docText = docText.Replace("«$Year»", this.NotificationInfo.CreatedDate.Value.ToString("yyyy"));
                docText = docText.Replace("«$Address»", (SecurityElement.Escape(this.NotificationInfo.Address) ?? string.Empty));
                docText = docText.Replace("«$TaxDepartmentName»", (SecurityElement.Escape(this.NotificationInfo.TaxDepartmentName) ?? string.Empty));

                using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create), Encoding.UTF8))
                {
                    sw.Write(docText.ToString());
                }

                DocumentFormat.OpenXml.Wordprocessing.Table table = wordDoc.MainDocumentPart.Document.Body.Elements<DocumentFormat.OpenXml.Wordprocessing.Table>().Last();
                int Stt = 0;
                foreach (var item in this.NotificationInfo.UseInvoiceDetails)
                {
                    Stt = ++Stt;
                    DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                    DocumentFormat.OpenXml.Wordprocessing.TableCell tablecellService1 =
                        new DocumentFormat.OpenXml.Wordprocessing.TableCell
                        (new DocumentFormat.OpenXml.Wordprocessing.Paragraph
                        (new DocumentFormat.OpenXml.Wordprocessing.Run
                        (new DocumentFormat.OpenXml.Wordprocessing.Text
                        (Stt.ToString()))));
                    DocumentFormat.OpenXml.Wordprocessing.TableCell tablecellService2 =
                        new DocumentFormat.OpenXml.Wordprocessing.TableCell
                        (new DocumentFormat.OpenXml.Wordprocessing.Paragraph
                        (new DocumentFormat.OpenXml.Wordprocessing.Run
                        (new DocumentFormat.OpenXml.Wordprocessing.Text
                        (item.InvoiceTypeName ?? string.Empty))));
                    DocumentFormat.OpenXml.Wordprocessing.TableCell tablecellService3 =
                        new DocumentFormat.OpenXml.Wordprocessing.TableCell
                        (new DocumentFormat.OpenXml.Wordprocessing.Paragraph
                        (new DocumentFormat.OpenXml.Wordprocessing.Run
                        (new DocumentFormat.OpenXml.Wordprocessing.Text
                        (item.RegisterTemplateCode ?? string.Empty))));
                    DocumentFormat.OpenXml.Wordprocessing.TableCell tablecellService4 =
                        new DocumentFormat.OpenXml.Wordprocessing.TableCell
                        (new DocumentFormat.OpenXml.Wordprocessing.Paragraph
                        (new DocumentFormat.OpenXml.Wordprocessing.Run
                        (new DocumentFormat.OpenXml.Wordprocessing.Text
                        (item.Code ?? string.Empty))));
                    DocumentFormat.OpenXml.Wordprocessing.TableCell tablecellService5 =
                        new DocumentFormat.OpenXml.Wordprocessing.TableCell
                        (new DocumentFormat.OpenXml.Wordprocessing.Paragraph
                        (new DocumentFormat.OpenXml.Wordprocessing.Run
                        (new DocumentFormat.OpenXml.Wordprocessing.Text
                        (item.NumberUse.ToString()))));
                    DocumentFormat.OpenXml.Wordprocessing.TableCell tablecellService6 =
                        new DocumentFormat.OpenXml.Wordprocessing.TableCell
                        (new DocumentFormat.OpenXml.Wordprocessing.Paragraph
                        (new DocumentFormat.OpenXml.Wordprocessing.Run
                        (new DocumentFormat.OpenXml.Wordprocessing.Text
                        (item.NumberFrom ?? string.Empty))));
                    DocumentFormat.OpenXml.Wordprocessing.TableCell tablecellService7 =
                        new DocumentFormat.OpenXml.Wordprocessing.TableCell
                        (new DocumentFormat.OpenXml.Wordprocessing.Paragraph
                        (new DocumentFormat.OpenXml.Wordprocessing.Run
                        (new DocumentFormat.OpenXml.Wordprocessing.Text
                        (item.NumberTo ?? string.Empty))));
                    DocumentFormat.OpenXml.Wordprocessing.TableCell tablecellService8 =
                        new DocumentFormat.OpenXml.Wordprocessing.TableCell
                        (new DocumentFormat.OpenXml.Wordprocessing.Paragraph
                        (new DocumentFormat.OpenXml.Wordprocessing.Run
                        (new DocumentFormat.OpenXml.Wordprocessing.Text
                        (item.UsedDate.ToString("dd/MM/yyyy")))));
                    tr.Append(tablecellService1, tablecellService2, tablecellService3,
                        tablecellService4, tablecellService5, tablecellService6,
                        tablecellService7, tablecellService8);
                    table.AppendChild(tr);
                }
            }
        }

        private string CreateFolderContainReport()
        {
            string fullPathForlder = Path.Combine(this.Config.FullPathInvoiceFileOfCompany, "Notifications");
            if (!Directory.Exists(fullPathForlder))
            {
                Directory.CreateDirectory(fullPathForlder);
            }

            return fullPathForlder;
        }
    }
}
