using InvoiceServer.Common.Constants;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using InvoiceServer.Common.Extensions;
using System.Text;
using System.Linq;
using InvoiceServer.Business.Helper;
using iTextSharp.text.pdf;
using iTextSharp.text;
using InvoiceServer.Common.Enums;

namespace InvoiceServer.Business.ExportInvoice
{
    public class AnnouncementExportFile : IXssf
    {
        private readonly AnnouncementPrintInfo announcementInfo;
        private readonly PrintConfig config;
        public string TemplateInvoiceDetail { get; private set; }
        public string InvoiceDetail { get; private set; }
        public Dictionary<string, string> Header { get; private set; }
        private int NumberLineOfPage { get; set; }
        private int NumberCharacterOfLine { get; set; }
        private bool IsShowOrder { get; set; }
        private double LocationSignLeft { get; set; }
        private double LocationSignButton { get; set; }
        private List<string> DetailInvoice { get; set; }
        private string HeaderInvoice { get; set; }
        private string FooterInvoice { get; set; }

        //private readonly IInvoiceRepository invoiceRepository;

        //protected static object lockObject = new object();
         
        public List<InvoiceItem> ItemDetailInvoice { get; private set; }

        public AnnouncementExportFile(AnnouncementPrintInfo announcementInfo, PrintConfig config, int numberlineOfPage, string templateDetailInvoice, double locationSignLeft, double locationSignButton, int numberCharacterOfLine, bool isShowOrder = false, string headerInvoice = "", string footerInvoice = "")
        {
            this.announcementInfo = announcementInfo;
            this.config = config;
            this.TemplateInvoiceDetail = templateDetailInvoice;
            this.NumberLineOfPage = numberlineOfPage;
            this.IsShowOrder = isShowOrder;
            this.LocationSignLeft = locationSignLeft;
            this.LocationSignButton = locationSignButton;
            this.NumberCharacterOfLine = numberCharacterOfLine;
            this.HeaderInvoice = headerInvoice;
            this.FooterInvoice = footerInvoice;
            this.DetailInvoice = new List<string>();

            InvoiceHeader();
            SetNumberInvoiceItemByConfig();
            BuildItemInvoice();
        }

        public ExportFileInfo ExportFile()
        {
            return SaveFile(this.config.FullPathFolderExportInvoice);
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }
            //string fileName = string.Format("{0}_{1}.pdf", this.invoiceInfo.Id, this.invoiceInfo.InvoiceNo);
            string fileName = string.Format("{0}.pdf", this.announcementInfo.Id);
            ExportFileInfo fileInfo = ExportInvoice(fullPathFile, fileName);
            if (announcementInfo.Invoice_Status == (int)InvoiceStatus.Draft)
            {
                string fullPathFileImage = FullPathFileSignFileDraft(this.announcementInfo.CompanyName);
                AddSignFileDraft(fileInfo, fullPathFileImage);
            }
            return fileInfo;
        }

        private ExportFileInfo ExportInvoice(string fullPathFile, string fileName)
        {
            List<string> fileInvoiceNameExport = new List<string>();
            string contentsPdfHtml = string.Empty;//BuildHtmlInvoice();
            string fileNameInvoice = string.Empty;
            string htmlPdf = GetTemplateFileHtml();
            int identity = 1;
            int numberPage = DetailInvoice.Count;
            if(numberPage == 1)
            {
                string itemInvoiceHtml = DetailInvoice[0];
                string tempHtmlPdf = HtmlInvoice(htmlPdf, this.HeaderInvoice, this.FooterInvoice);
                string htmlPdfOut = BindDataToPagePdf(tempHtmlPdf, identity, numberPage, true);
                contentsPdfHtml = htmlPdfOut.Replace(ConfigInvoiceDetaiInfo.ContentInvoice, itemInvoiceHtml);
                fileNameInvoice = string.Format("{0}\\{1}_{2}.pdf", fullPathFile, "invoice", DateTime.Now.ToString(Formatter.DateTimeFormat));
                SelectExportFile.HtmlToPdfConverter(contentsPdfHtml, fileNameInvoice, this.announcementInfo.Option);

                fileInvoiceNameExport.Add(fileNameInvoice);
            }
            else
            {
                foreach (var item in DetailInvoice)
                {
                    string tempHtmlPdf = htmlPdf;
                    if (identity == 1)
                    {
                        tempHtmlPdf = HtmlInvoice(tempHtmlPdf, this.HeaderInvoice, string.Empty);
                    }
                    else if (identity == numberPage)
                    {
                        tempHtmlPdf = HtmlInvoice(tempHtmlPdf, string.Empty, this.FooterInvoice);
                    }
                    else
                    {
                        tempHtmlPdf = HtmlInvoice(tempHtmlPdf, string.Empty, string.Empty);
                    }
                    bool isFillTotalInvoice = (identity == numberPage);
                    string htmlPdfOut = BindDataToPagePdf(tempHtmlPdf, identity, numberPage, isFillTotalInvoice);
                    contentsPdfHtml = htmlPdfOut.Replace(ConfigInvoiceDetaiInfo.ContentInvoice, item);
                    fileNameInvoice = string.Format("{0}\\{1}_{2}.pdf", fullPathFile, "invoice", DateTime.Now.ToString(Formatter.DateTimeFormat));
                    SelectExportFile.HtmlToPdfConverter(contentsPdfHtml, fileNameInvoice, this.announcementInfo.Option);

                    fileInvoiceNameExport.Add(fileNameInvoice);
                    identity++;

                }
            }
          

            string filePathFileName = string.Format("{0}\\{1}", fullPathFile, fileName);
            string fileInvoicePdf = MergeFile(fileInvoiceNameExport, filePathFileName);
            ExportFileInfo fileInfo = new ExportFileInfo(fileInvoicePdf);
            return fileInfo;
        }

        public string  BindDataToPagePdf(string tempHtmlPdf, int currentPage, int numberPage, bool isFillTotalInvoice)
        {
            bool isShowPage = (numberPage > 1);
            string page = "";
            if (currentPage > 1)
            {
                page = GetPageOfInvoice(currentPage, numberPage, isShowPage);
            }
            string htmlPdfOut = FillTotalInvoice(tempHtmlPdf, page, isFillTotalInvoice);
            return htmlPdfOut;
        }

        public string GetTemplateFileHtml()
        {
            if (!File.Exists(this.config.FullPathFileTemplateInvoice))
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("Not found template {0}", this.config.FullPathFileTemplateInvoice));
            }
            string htmlpdf = string.Empty;
            using (FileStream stream = File.Open(this.config.FullPathFileTemplateInvoice, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    htmlpdf = reader.ReadToEnd();
                }
            }
            return htmlpdf;
        }

        private string HtmlInvoice(string htmlpdf, string headerInvoice, string footerInvoice)
        {
            //string htmlpdf = string.Empty;

            htmlpdf = htmlpdf.Replace(ConfigInvoiceDetaiInfo.HeaderInvoice, headerInvoice);
            htmlpdf = htmlpdf.Replace(ConfigInvoiceDetaiInfo.FooterInvoice, footerInvoice);

            if (!string.IsNullOrWhiteSpace(headerInvoice))
            {
                foreach (var item in this.Header)
                {
                    htmlpdf = htmlpdf.Replace(item.Key, item.Value);
                }

                
            }
            string logo = GetBase64StringImage(this.config.FullPathLogo);
            htmlpdf = htmlpdf.Replace(ConfigInvoiceDetaiInfo.Logo, logo);
            htmlpdf = htmlpdf.Replace(ConfigInvoiceDetaiInfo.MarkImage, BuilderImage());
            htmlpdf = htmlpdf.Replace(ConfigInvoiceDetaiInfo.MessageSwitchInvoice, this.announcementInfo.MessageSwitchInvoice);
            htmlpdf = htmlpdf.Replace(ConfigInvoiceDetaiInfo.NoticeSwichInvoice, this.announcementInfo.NoticeSwitchInvoice);

            return htmlpdf;
        }

        private string GetBase64StringImage(string Url)
        {
            if (!File.Exists(Url)) return string.Empty;
            var bytes = File.ReadAllBytes(Url);
            return Convert.ToBase64String(bytes);
        }
        //private double GetNextInvoiceNo(int companyId, int templateid, string symbol)
        //{
        //    lock (lockObject)
        //    {
        //        double maxInvoiceNo = 0;
        //        var maxInvoice = GetMaxInvoice(companyId, templateid, symbol);
        //        double.TryParse(maxInvoice, out maxInvoiceNo);
        //        return ++maxInvoiceNo;
        //    }
        //}
        //private string GetMaxInvoice(int companyId, int templateid, string symbol)
        //{
        //    string symbols = symbol.Replace("/", "");
        //    var invoices = this.invoiceRepository.FilterInvoice(companyId, templateid, symbols).OrderByDescending(p => p.No).FirstOrDefault();
        //    string maxInvoice = "0";
        //    if (invoices != null && invoices.No.IsNotNullOrEmpty())
        //    {
        //        maxInvoice = invoices.No;
        //    }

        //    return maxInvoice;
        //}

        private void InvoiceHeader()
        {
            this.Header = new Dictionary<string, string>() { 
                {InvoiceHeaderInfo.InvoiceNo, this.announcementInfo.InvoiceNo},
                {InvoiceHeaderInfo.CompanyName, this.announcementInfo.CompanyName},
                {InvoiceHeaderInfo.TaxCode, this.announcementInfo.TaxCode},
                {InvoiceHeaderInfo.Address, this.announcementInfo.Address},
                {InvoiceHeaderInfo.Mobile, this.announcementInfo.Mobile},
                {InvoiceHeaderInfo.Tel, this.announcementInfo.Tel},
                {InvoiceHeaderInfo.Fax, this.announcementInfo.Fax},
                {InvoiceHeaderInfo.Symbol, this.announcementInfo.Symbol},
                {InvoiceHeaderInfo.Email, this.announcementInfo.Email},
                {InvoiceHeaderInfo.BankAccount, this.announcementInfo.BankAccount},
                {InvoiceHeaderInfo.BankName, this.announcementInfo.BankName},
                {InvoiceHeaderInfo.Website, this.announcementInfo.Website},
                {InvoiceHeaderInfo.InvoiceCode, this.announcementInfo.InvoiceCode},
                {InvoiceHeaderInfo.TypePayment,this.announcementInfo.TypePayment},
                {InvoiceHeaderInfo.TypePaymentCode,this.announcementInfo.TypePayment},
                {InvoiceHeaderInfo.CustomerName, this.announcementInfo.CustomerName},
                {InvoiceHeaderInfo.CustomerCompanyName, this.announcementInfo.CustomerCompanyName},
                {InvoiceHeaderInfo.CustomerTaxCode, this.announcementInfo.CustomerTaxCode},
                {InvoiceHeaderInfo.CustomerAddress, this.announcementInfo.CustomerAddress},
                {InvoiceHeaderInfo.CustomerBankAccount, this.announcementInfo.CustomerBankAccount},
                //{InvoiceHeaderInfo.NoticeChangeInVoice,this.announcementInfo.NoticeChangeInvoice},
                {InvoiceHeaderInfo.ReportWebsite,this.announcementInfo.ReportWebsite},
                {InvoiceHeaderInfo.ReportTel,this.announcementInfo.ReportTel},
                {InvoiceHeaderInfo.EmailContract, this.announcementInfo.EmailContract},
            };

            if (announcementInfo.Invoice_Status == (int)InvoiceStatus.Draft)
            {
                this.Header.Add(InvoiceHeaderInfo.DayInvoice, "...");
                this.Header.Add(InvoiceHeaderInfo.MonthInvoice, "...");
                this.Header.Add(InvoiceHeaderInfo.YearInvoice, "...");
            }
            else
            {
                this.Header.Add(InvoiceHeaderInfo.DayInvoice, this.announcementInfo.InvoiceDate.Day.ToString().PadLeft(2, '0'));
                this.Header.Add(InvoiceHeaderInfo.MonthInvoice, this.announcementInfo.InvoiceDate.Month.ToString().PadLeft(2, '0'));
                this.Header.Add(InvoiceHeaderInfo.YearInvoice, this.announcementInfo.InvoiceDate.Year.ToString());
            }
        }

        private string FillTotalInvoice(string htmlInvoiceIn, string page, bool isFillTotal = false)
        {
            string htmlInvoice = htmlInvoiceIn.Replace(InvoiceHeaderInfo.CurrentPage, page);
            if (!isFillTotal)
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.TaxOfInvoice, string.Empty);
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.AmountTax, string.Empty);
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.displayDiscount, string.Empty);
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.AmountDiscount, string.Empty);
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.TotalDiscountTax, string.Empty);
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.AmountInwords, string.Empty);
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.Total, string.Empty);
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.AmountTotal, string.Empty);
                return htmlInvoice;
            }

            if (announcementInfo.Invoice_Status == (int)InvoiceStatus.Draft)
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.TaxOfInvoice, "0");
            }
            else if (this.announcementInfo.Tax.IsNullOrEmpty() || this.announcementInfo.Tax.IsEquals("Không chịu thuế"))
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.TaxOfInvoice, @"0%");
            }
            else
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.TaxOfInvoice, this.announcementInfo.Tax);
            }

            if (announcementInfo.Invoice_Status == (int)InvoiceStatus.Draft)
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.AmountTax, "");
            }
            else if (this.announcementInfo.Tax.IsNullOrEmpty() || !this.announcementInfo.TaxAmout.HasValue)
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.AmountTax, @".......");
            }
            else
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.AmountTax, String.Format(this.config.FomatNumber, this.announcementInfo.TaxAmout.ToDecimal()));
            }
            if (announcementInfo.InvoiceSample != 2)
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.displayDiscount, "<tr>");
            }
            else
            {
                string dis = "<tr style='display:none'>";
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.displayDiscount, dis);
            }
            if (!announcementInfo.DiscountAmount.HasValue)
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.AmountDiscount, "");
            }
            else
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.AmountDiscount, String.Format(this.config.FomatNumber, this.announcementInfo.DiscountAmount.ToDecimal()));
            }
            if (!announcementInfo.TotalDiscountTax.HasValue)
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.TotalDiscountTax, "");
            }
            else
            {
                htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.TotalDiscountTax, String.Format(this.config.FomatNumber, this.announcementInfo.TotalDiscountTax.ToDecimal()));
            }
            htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.AmountInwords, this.announcementInfo.AmountInwords);
            htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.Total, String.Format(this.config.FomatNumber, this.announcementInfo.Total.ToDecimal()));
            htmlInvoice = htmlInvoice.Replace(InvoiceHeaderInfo.AmountTotal, String.Format(this.config.FomatNumber, this.announcementInfo.Sum.ToDecimal()));

            return htmlInvoice;
        }

        private void BuildItemInvoice()
        {
            StringBuilder detailInvoice = new StringBuilder();
            int identity = 0;
            int numberRow = 0;
            int order = 0;
            int row = 0;
            
            this.ItemDetailInvoice.ForEach(p =>
            {
                int numberLine = p.ProductName.NumberLine(this.NumberCharacterOfLine, 1.1);
                row = row + numberLine;
            });
            if (row > this.NumberLineOfPage)
            {
                int number_du = row - this.NumberLineOfPage;
                if (number_du < 21)
                {
                    int number_add = 21 - number_du;
                    for (int i = 0; i < number_add; i++)
                    {
                        InvoiceItem item = new InvoiceItem();
                        this.ItemDetailInvoice.Add(item);
                    }
                }
            }
            int numberItemInvoice = this.ItemDetailInvoice.Count();
            this.ItemDetailInvoice.ForEach(p =>
            {
                identity++;
                order++;
                int numberLine = p.ProductName.NumberLine(this.NumberCharacterOfLine, 1.1);
                numberRow = numberRow + numberLine;
                if (numberRow > this.NumberLineOfPage)
                {
                    numberRow = numberLine;
                    //identity = 1;
                    DetailInvoice.Add(detailInvoice.ToString());
                    detailInvoice = new StringBuilder();
                    detailInvoice.Append(SetDataToItemInvoice(identity, p));
                    
                }
                else if (numberRow == this.NumberLineOfPage && numberRow < 21)
                {
                    numberRow = 0;
                    detailInvoice.Append(SetDataToItemInvoice(identity, p));
                    DetailInvoice.Add(detailInvoice.ToString());
                    detailInvoice = new StringBuilder();
                    //identity = 0;
                    this.NumberLineOfPage = 21;
                }
                else if (order == numberItemInvoice)
                {
                    if (identity == 1)
                    {
                        return;
                    }

                    detailInvoice.Append(SetDataToItemInvoice(identity, p));
                    DetailInvoice.Add(detailInvoice.ToString());
                }
                else
                {
                    detailInvoice.Append(SetDataToItemInvoice(identity, p));
                }

            });
        }

        private string SetDataToItemInvoice(int identity, InvoiceItem p)
        {
            string item;
            item = TemplateInvoiceDetail
                .Replace(ConfigInvoiceDetaiInfo.Identity, p.IsShowOrder ? identity.ToString() : string.Empty)
                .Replace(ConfigInvoiceDetaiInfo.Unit, p.Unit)
                .Replace(ConfigInvoiceDetaiInfo.Quantity, !p.Quantity.HasValue ? string.Empty : p.Quantity.ToDecimalFomat(this.config.FomatNumber))
                .Replace(ConfigInvoiceDetaiInfo.Price, !p.Price.HasValue ? string.Empty : p.Price.ToDecimalFomat(this.config.FomatNumber))
                .Replace(ConfigInvoiceDetaiInfo.Tax, p.DisplayInvoice.IsNullOrEmpty() ? string.Empty : p.DisplayInvoice)
                //.Replace(ConfigInvoiceDetaiInfo.AmountTax, !p.AmountTax.HasValue ? string.Empty : p.AmountTax.ToDecimalFomat(this.config.FomatNumber))
                .Replace(ConfigInvoiceDetaiInfo.Discount, !p.Discount.HasValue ? string.Empty : p.Discount.ToDecimalFomat(this.config.FomatNumber))
                .Replace(ConfigInvoiceDetaiInfo.AmountDiscount, !p.AmountDiscount.HasValue ? string.Empty : p.AmountDiscount.ToDecimalFomat(this.config.FomatNumber))
                .Replace(ConfigInvoiceDetaiInfo.AmountTotal, p.AmountTotal == 0 ? string.Empty : p.AmountTotal.ToDecimalFomat(this.config.FomatNumber));
                //.Replace(ConfigInvoiceDetaiInfo.Total, p.Total == 0 ? string.Empty : p.Total.ToDecimalFomat(this.config.FomatNumber));
            if(p.NotPayment == true)
            {
                item = item.Replace(ConfigInvoiceDetaiInfo.Total, "0");
                item = item.Replace(ConfigInvoiceDetaiInfo.AmountTax, "0");
            }
            else
            {
                item = item.Replace(ConfigInvoiceDetaiInfo.Total, p.Total == 0 ? string.Empty : p.Total.ToDecimalFomat(this.config.FomatNumber));
                item = item.Replace(ConfigInvoiceDetaiInfo.AmountTax, !p.AmountTax.HasValue ? string.Empty : p.AmountTax.ToDecimalFomat(this.config.FomatNumber));
            }
            if (p.Discount == null)
            {
                if(p.AdjustmentType == 1)
                {
                    item = item.Replace(ConfigInvoiceDetaiInfo.ProductName, p.ProductName + " (Điều chỉnh tăng)");
                }
                else if(p.AdjustmentType == 2)
                {
                    item = item.Replace(ConfigInvoiceDetaiInfo.ProductName, p.ProductName + " (Điều chỉnh giảm)");
                }
                else
                {
                    item = item.Replace(ConfigInvoiceDetaiInfo.ProductName, p.ProductName);
                }
                
            }
            else {
                item = item.Replace(ConfigInvoiceDetaiInfo.ProductName, p.DiscountDescription);
            }
            return item;
        }

        private void SetNumberInvoiceItemByConfig()
        {
            if (announcementInfo.InvoiceItems == null)
            {
                this.ItemDetailInvoice = new List<InvoiceItem>();
            }
            else
            {
                this.ItemDetailInvoice = this.announcementInfo.InvoiceItems.ToList();
            }

            int numberLineByItemInvoice = 0;
            this.ItemDetailInvoice.ForEach(p =>
            {
                numberLineByItemInvoice += p.ProductName.NumberLine(this.NumberCharacterOfLine, 1.1);
            });

            int bonus = CaculatorNumberInvoice(numberLineByItemInvoice, this.NumberLineOfPage);
            for (int i = 0; i < bonus; i++)
            {
                this.ItemDetailInvoice.Add(new InvoiceItem(this.IsShowOrder));
            }
        }

        private int CaculatorNumberInvoice(int currentNumberItem, int maxNumberItemConfig)
        {
            if (currentNumberItem == 0 || maxNumberItemConfig == 0)
            {
                return maxNumberItemConfig;
            }

            int numberRow = currentNumberItem;
            int numberItem = numberRow % maxNumberItemConfig;
            int bonus = 0;
            if (numberItem > 0)
            {
                bonus = this.NumberLineOfPage - numberItem;
            }

            return bonus;
        }

        private string BuilderImage()
        {
            string imageMark = string.Empty;

            if (announcementInfo.MarkImage.IsNotNullOrEmpty())
            {
                string fullPathFile = Path.Combine(this.config.FullPathFileAsset, announcementInfo.MarkImage);
                imageMark = string.Format("<img src=\"data:image/png;base64, {0}\" />", GetBase64StringImage(fullPathFile));
            }

            return imageMark;
        }

        private string FullPathFileSignFileDraft(string companyName)
        {
            string imageSignDraft = FileSign.CreateFileSign(companyName, config.FullPathFileAsset, config.FullFolderAssetOfCompany, "fileSignDraft");
            return imageSignDraft;
        }

        private void AddSignFileDraft(ExportFileInfo fileInfo, string imageDraft)
        {
            string fileName = fileInfo.FullPathFileName.Replace(".pdf", "signDraft.pdf");
            using (Stream inputPdfStream = new FileStream(fileInfo.FullPathFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream inputImageStream = new FileStream(imageDraft, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                var pdfContentByte = stamper.GetOverContent(1);
                var image = iTextSharp.text.Image.GetInstance(imageDraft);
                float signPositionX = pdfContentByte.PdfDocument.PageSize.Width - (float)this.LocationSignLeft;
                image.SetAbsolutePosition(signPositionX, (float)this.LocationSignButton);
                pdfContentByte.AddImage(image);
                stamper.Close();
            }

            fileInfo.FullPathFileName = fileName;
        }
        
        private static string MergeFile(List<string> fullPathFileInvoice, string outFile)
        {
            using (FileStream stream = new FileStream(outFile, FileMode.Create))
            using (Document doc = new Document())
            using (PdfCopy pdf = new PdfCopy(doc, stream))
            {
                doc.Open();
                PdfReader reader = null;
                PdfImportedPage page = null;
                foreach (var item in fullPathFileInvoice)
                {
                    reader = new PdfReader(item);
                    for (int i = 0; i < reader.NumberOfPages; i++)
                    {
                        page = pdf.GetImportedPage(reader, i + 1);
                        pdf.AddPage(page);
                    }
                    pdf.FreeReader(reader);
                    reader.Close();
                    File.Delete(item);
                }
            }

            return outFile;
        }

        private string GetPageOfInvoice(int page, int totalPage, bool isShowPage)
        {
            if(!isShowPage)
            {
                return string.Empty;
            }

            return string.Format("<p style=\"margin:0px; color:red;\"> Tiếp theo trang {0}/{1}  </p>",page, totalPage);
        }
    }
}
