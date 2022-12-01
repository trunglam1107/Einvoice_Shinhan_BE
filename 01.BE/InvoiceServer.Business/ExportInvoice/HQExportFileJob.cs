using HiQPdf;
using InvoiceServer.Business.Helper;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;

namespace InvoiceServer.Business.ExportInvoice
{
    public class HQExportFileJob : IXssf
    {
        private readonly InvoicePrintModel invoiceInfo;
        private readonly PrintConfig config;
        public string TemplateInvoiceDetail { get; private set; }
        public string InvoiceDetail { get; private set; }
        public Dictionary<string, string> Header { get; private set; }
        private int NumberLineOfPage { get; set; }
        private int NumberRecord { get; set; }
        private bool IsShowOrder { get; set; }
        private List<string> DetailInvoice { get; set; }
        private string HeaderInvoice { get; set; }
        private string FooterInvoice { get; set; }
        private Dictionary<int, int> PageSplit;
        private UserSessionInfo CurrentUser { get; set; }
        private Dictionary<int, NumberRowInHtml> NumberRowInHTMLs { get; set; }    // Đếm số dòng hiển thị của từng sản phẩm
        private int TotalRowOfName { get; set; }
        private int BonusRow { get; set; }
        private SystemSettingInfo SystemSetting { get; set; }
        private int NumberCharacterOfLine { get; set; }
        private decimal LocationSignLeft { get; set; }
        private decimal LocationSignButton { get; set; }

        // get config data in Web.config file
        private readonly Func<string, string> GetConfig = key => WebConfigurationManager.AppSettings[key];

        public List<InvoiceItem> ItemDetailInvoice { get; private set; }
        private List<STATISTICAL> invoiceStatisticals { get; set; }
        private List<INVOICEGIFT> invoiceGift { get; set; }
        public HQExportFileJob(InvoiceExportModel invoiceExportModel)
        {
            this.invoiceInfo = invoiceExportModel.InvoicePrintModel;
            this.config = invoiceExportModel.PrintConfig;

            this.TemplateInvoiceDetail = invoiceExportModel.DetailInvoice;
            this.NumberLineOfPage = invoiceExportModel.NumberLineOfPage;
            this.NumberRecord = invoiceExportModel.NumberRecord;
            this.IsShowOrder = invoiceExportModel.IsShowOrder;
            this.LocationSignLeft = invoiceExportModel.LocationSignLeft;
            this.LocationSignButton = invoiceExportModel.LocationSignButton;
            this.NumberCharacterOfLine = invoiceExportModel.NumberCharacterOfLine;
            this.HeaderInvoice = invoiceExportModel.HeaderInvoice;
            this.FooterInvoice = invoiceExportModel.FooterInvoice;
            this.DetailInvoice = new List<string>();
            this.CurrentUser = invoiceExportModel.UserSessionInfo;
            this.NumberRowInHTMLs = new Dictionary<int, NumberRowInHtml>();
            this.SystemSetting = invoiceExportModel.SystemSetting ?? new SystemSettingInfo();

            InvoiceHeader();
            this.TotalRowOfName = CountRowForEachInvoiceItem();
            CountRecordsForEachPage();
            SetNumberInvoiceItemByConfig();
            BuildItemInvoice();
        }
        public ExportFileInfo ExportFile()
        {
            return SaveFile(this.config.FolderExportInvoice);
        }

        private ExportFileInfo SaveFile(string fullPathFile)
        {
            if (!Directory.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }
            string fileName = string.Format("{0}.pdf", this.invoiceInfo.ID);
            ExportFileInfo fileInfo = ExportInvoice(fullPathFile, fileName);
            return fileInfo;
        }
        public void ExportFileJob(string fullPathFile, string fileName)
        {
            ExportInvoiceJob(fullPathFile, fileName);
        }
        private string GetMonth(int month)
        {
            string EnMonth = string.Empty;
            switch (month)
            {
                case 1:
                    EnMonth = "January";
                    break;
                case 2:
                    EnMonth = "February";
                    break;
                case 3:
                    EnMonth = "March";
                    break;
                case 4:
                    EnMonth = "April";
                    break;
                case 5:
                    EnMonth = "May";
                    break;
                case 6:
                    EnMonth = "June";
                    break;
                case 7:
                    EnMonth = "July";
                    break;
                case 8:
                    EnMonth = "August";
                    break;
                case 9:
                    EnMonth = "September";
                    break;
                case 10:
                    EnMonth = "October";
                    break;
                case 11:
                    EnMonth = "November";
                    break;
                case 12:
                    EnMonth = "December";
                    break;
            }
            return EnMonth.ToUpper();
        }
        private ExportFileInfo ExportInvoice(string fullPathFile, string fileName)
        {
            List<string> fileInvoiceNameExport = new List<string>();
            StringBuilder htmlPdf = GetTemplateFileHtml(null);
            int identity = 1;
            int numberPage = DetailInvoice.Count;
            string fileNameInvoice;
            if (numberPage == 1)
            {
                string itemInvoiceHtml = DetailInvoice[0];
                HtmlInvoice(htmlPdf, this.HeaderInvoice, this.FooterInvoice);
                BindDataToPagePdf(htmlPdf, identity, numberPage, true);
                htmlPdf.Replace(ConfigInvoiceDetaiInfo.ContentInvoice, itemInvoiceHtml);
                fileNameInvoice = string.Format("{0}\\{1}_{2}.pdf", fullPathFile, "invoice", DateTime.Now.Ticks.ToString());
                SelectExportFile.HtmlToPdfConverter(htmlPdf.ToString(), fileNameInvoice, this.invoiceInfo.Option);
                fileInvoiceNameExport.Add(fileNameInvoice);
            }
            else
            {
                foreach (var item in DetailInvoice)
                {
                    StringBuilder tempHtmlPdf = new StringBuilder(htmlPdf.ToString());
                    if (identity == numberPage)
                    {
                        HtmlInvoice(tempHtmlPdf, this.HeaderInvoice, this.FooterInvoice);
                    }
                    else
                    {
                        HtmlInvoice(tempHtmlPdf, this.HeaderInvoice, string.Empty);
                    }
                    bool isFillTotalInvoice = (identity == numberPage);
                    BindDataToPagePdf(tempHtmlPdf, identity, numberPage, isFillTotalInvoice);
                    tempHtmlPdf.Replace(ConfigInvoiceDetaiInfo.ContentInvoice, item);
                    fileNameInvoice = string.Format("{0}\\{1}_{2}.pdf", fullPathFile, "invoice", DateTime.Now.ToString(Formatter.DateTimeFormat));
                    SelectExportFile.HtmlToPdfConverter(tempHtmlPdf.ToString(), fileNameInvoice, this.invoiceInfo.Option);
                    fileInvoiceNameExport.Add(fileNameInvoice);
                    identity++;

                }
            }


            string filePathFileName = string.Format("{0}\\{1}", fullPathFile, fileName);
            string fileInvoicePdf = MergeFile(fileInvoiceNameExport, filePathFileName);
            ExportFileInfo fileInfo = new ExportFileInfo(fileInvoicePdf);
            //if (invoiceInfo.Invoice_Status != (int)InvoiceStatus.Draft)
            //{
            //    if (this.invoiceStatisticals.Count > 0)
            //    {
            //        ExportStatistical();
            //    }
            //    if (this.invoiceGift.Count > 0)
            //    {
            //        ExportInvoiceGift();
            //    }
            //}
            return fileInfo;
        }
        private void ExportInvoiceJob(string fullPathFile, string fileName)
        {
            //Logger logger = new Logger();
            List<string> fileInvoiceNameExport = new List<string>();
            StringBuilder htmlPdf = GetTemplateFileHtml(null);
            int identity = 1;
            int numberPage = DetailInvoice.Count;
            string fileNameInvoice;
            string pathfileName = Path.Combine(fullPathFile, fileName);
            if (numberPage == 1)
            {
                string itemInvoiceHtml = DetailInvoice[0];
                HtmlInvoice(htmlPdf, this.HeaderInvoice, this.FooterInvoice);
                BindDataToPagePdf(htmlPdf, identity, numberPage, true);
                htmlPdf.Replace(ConfigInvoiceDetaiInfo.ContentInvoice, itemInvoiceHtml);
                PdfSharpConvert(htmlPdf.ToString(), pathfileName);
                //return htmlPdf.ToString();
            }
            else
            {
                foreach (var item in DetailInvoice)
                {
                    StringBuilder tempHtmlPdf = new StringBuilder(htmlPdf.ToString());
                    //logger.Error("identity: " + identity + ", numberPage: " + numberPage, new Exception("ExportInvoiceJob"));
                    if (identity == numberPage)
                    {
                        HtmlInvoice(tempHtmlPdf, this.HeaderInvoice, this.FooterInvoice);
                    }
                    else
                    {
                        HtmlInvoice(tempHtmlPdf, this.HeaderInvoice, string.Empty);
                    }
                    bool isFillTotalInvoice = (identity == numberPage);
                    BindDataToPagePdf(tempHtmlPdf, identity, numberPage, isFillTotalInvoice);
                    tempHtmlPdf.Replace(ConfigInvoiceDetaiInfo.ContentInvoice, item);
                    fileNameInvoice = string.Format("{0}\\{1}_{2}_{3}.pdf", fullPathFile, "invoice", invoiceInfo.ID.ToString(), DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    PdfSharpConvert(tempHtmlPdf.ToString(), fileNameInvoice);
                    //SelectExportFile.HtmlToPdfConverter(tempHtmlPdf.ToString(), fileNameInvoice, this.invoiceInfo.Option);
                    fileInvoiceNameExport.Add(fileNameInvoice);
                    identity++;

                }
                
                string fileInvoicePdf = MergeFile(fileInvoiceNameExport, pathfileName);
                //return htmlPdf.ToString();
            }
        }
        public void PdfSharpConvert(string html, string path)
        {
            HtmlToPdf htmlToPdfConverter = new HtmlToPdf();
            htmlToPdfConverter.Document.PageSize = PdfPageSize.A4;
            htmlToPdfConverter.Document.PageOrientation = PdfPageOrientation.Portrait;
            // set PDF page margins
            htmlToPdfConverter.Document.Margins = new PdfMargins(0);
            //htmlToPdfConverter.Document.PageSize.Width = 824;
            //htmlToPdfConverter.Document.PageSize.Height = 595;
            string baseUrl = "";
            byte[] pdfBuffer = null;
            // convert HTML code to a PDF memory buffer
            pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, baseUrl);
            File.WriteAllBytes(path, pdfBuffer);

        }
        public void BindDataToPagePdf(StringBuilder tempHtmlPdf, int currentPage, int numberPage, bool isFillTotalInvoice)
        {
            bool isShowPage = (numberPage > 1);
            string page = "";
            if (currentPage > 1)
            {
                page = GetPageOfInvoice(currentPage, numberPage, isShowPage);
            }
            FillTotalInvoice(tempHtmlPdf, page, isFillTotalInvoice);
        }

        public StringBuilder GetTemplateFileHtml(string pathTemplate)
        {
            string FullPathFileTemplate;
            if (pathTemplate != null)
            {
                FullPathFileTemplate = pathTemplate;
            }
            else
            {
                FullPathFileTemplate = this.config.FullPathFileTemplateInvoice;
            }

            if (!File.Exists(FullPathFileTemplate))
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("Not found template {0}", FullPathFileTemplate));
            }
            StringBuilder htmlpdf;
            using (FileStream stream = File.Open(FullPathFileTemplate, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                StreamReader reader = new StreamReader(stream);
                htmlpdf = new StringBuilder(reader.ReadToEnd());
            }
            return htmlpdf;
        }

        private void HtmlInvoice(StringBuilder htmlpdf, string headerInvoice, string footerInvoice)
        {
            htmlpdf.Replace(ConfigInvoiceDetaiInfo.HeaderInvoice, headerInvoice);
            htmlpdf.Replace(ConfigInvoiceDetaiInfo.FooterInvoice, footerInvoice);

            if (!string.IsNullOrWhiteSpace(headerInvoice))
            {
                foreach (var item in this.Header)
                {
                    htmlpdf.Replace(item.Key, item.Value);
                }


            }
            string logo = GetBase64StringImage(this.config.FullPathLogo);
            htmlpdf.Replace(ConfigInvoiceDetaiInfo.Logo, logo);
            htmlpdf.Replace(ConfigInvoiceDetaiInfo.MarkImage, BuilderImage());
            htmlpdf.Replace(ConfigInvoiceDetaiInfo.MessageSwitchInvoice, this.invoiceInfo.MessageSwitchInvoice);
            htmlpdf.Replace(ConfigInvoiceDetaiInfo.NoticeSwichInvoice, this.invoiceInfo.NoticeSwitchInvoice);
        }

        private string GetBase64StringImage(string Url)
        {
            return CommonUtil.GetBase64StringImage(Url);
        }

        private readonly Func<string, string> insertSpaceIfNoteixts = value =>
        {
            var spaceIfNotExists = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            if (string.IsNullOrWhiteSpace(value))
                return spaceIfNotExists;
            return string.Empty;
        };
        private void InvoiceHeader()
        {
            var spaces = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            this.Header = new Dictionary<string, string>() {
                {InvoiceHeaderInfo.CompanyName, this.invoiceInfo.COMPANYNAME},
                {InvoiceHeaderInfo.Address, this.invoiceInfo.COMPANYADDRESS},
                {InvoiceHeaderInfo.TaxCode, this.invoiceInfo.COMPANYTAXCODE},
                //{InvoiceHeaderInfo.InvoiceCode, this.invoiceInfo.InvoiceCode},
                {InvoiceHeaderInfo.Symbol, this.invoiceInfo.SYMBOL},
                {InvoiceHeaderInfo.InvoiceNo, this.invoiceInfo.NO},
                {InvoiceHeaderInfo.CustomerName, this.invoiceInfo.ISORG == "1" ? this.invoiceInfo.CUSTOMERNAME : this.invoiceInfo.PERSONCONTACT},
                {InvoiceHeaderInfo.CustomerCode, this.invoiceInfo.CUSTOMERCODE},
                {InvoiceHeaderInfo.CustomerTaxCode, this.invoiceInfo.CUSTOMERTAXCODE},
                {InvoiceHeaderInfo.ReferenceNumber, this.invoiceInfo.REFNUMBER},
                {InvoiceHeaderInfo.ExchangeRate, String.Format(FormatNumber.GetStringFormat(this.SystemSetting?.ExchangeRate), this.invoiceInfo.CURRENCYEXCHANGERATE.ToDecimal())},
                {InvoiceHeaderInfo.CustomerAddress, this.invoiceInfo.CUSTOMERADDRESS},
                {InvoiceHeaderInfo.TypePaymentCode,this.invoiceInfo.TYPEPAYMENTNAME},
                {InvoiceHeaderInfo.CustomerBankAccount, this.invoiceInfo.CUSTOMERBANKACC},
                {InvoiceHeaderInfo.NoticeChangeInVoice,this.invoiceInfo.NoticeChangeInvoice},
                {InvoiceHeaderInfo.Currency,this.invoiceInfo.CURRENCYCODE},

                {InvoiceHeaderInfo.Mobile, this.invoiceInfo.COMMOBILE},
                {InvoiceHeaderInfo.SpaceIfNotExistsTel, insertSpaceIfNoteixts(this.invoiceInfo.TEL1)},
                {InvoiceHeaderInfo.Tel, !string.IsNullOrEmpty(this.invoiceInfo.TEL1) ? this.invoiceInfo.TEL1 : spaces},
                {InvoiceHeaderInfo.Fax, !string.IsNullOrEmpty(this.invoiceInfo.FAX) ? this.invoiceInfo.FAX : spaces},
                {InvoiceHeaderInfo.Email, this.invoiceInfo.EMAIL},
                //{InvoiceHeaderInfo.BankAccount, this.invoiceInfo.CO},
                //{InvoiceHeaderInfo.BankName, this.invoiceInfo.BankName},
                //{InvoiceHeaderInfo.Website, this.invoiceInfo.Website},
                {InvoiceHeaderInfo.TypePayment,this.invoiceInfo.TYPEPAYMENTNAME},
                {InvoiceHeaderInfo.CustomerCompanyName, this.invoiceInfo.COMPANYNAME},
                {InvoiceHeaderInfo.ReportWebsite,this.invoiceInfo.REPORTWEBSITE},
                {InvoiceHeaderInfo.ReportTel,this.invoiceInfo.REPORTTEL},
                //{InvoiceHeaderInfo.EmailContract, this.invoiceInfo.EMAI},
                {InvoiceHeaderInfo.CompanyUrl, $"{this.CurrentUser?.Company?.Id.ConvertToString()}"}
            };
            InvoiceHeaderChild();
            string logoSrc = string.Concat(this.config.FullPathFileAsset.Reverse().Skip(5).Reverse());   // Bỏ chữ Asset
            var LogoSrcConfig = this.GetConfig("LogoSrc").Replace('/', '\\');
            logoSrc += LogoSrcConfig.Substring(7);
            string logoBase64 = GetBase64StringImage(logoSrc);
            this.Header.Add(InvoiceHeaderInfo.LogoSrc, logoBase64);
            this.Header.Add(InvoiceHeaderInfo.FooterCompanyName, this.GetConfig("CompanyName"));
            this.Header.Add(InvoiceHeaderInfo.FooterTaxCode, this.GetConfig("TaxCode"));
            this.Header.Add(InvoiceHeaderInfo.FooterPhone, this.GetConfig("Phone"));
            this.Header.Add(InvoiceHeaderInfo.FooterLink, this.GetConfig("CompanyLink"));
            this.Header.Add(InvoiceHeaderInfo.FooterLinkText, this.GetConfig("CompanyLinkText"));
        }
        private void InvoiceHeaderChild()
        {
            if (invoiceInfo.INVOICESTATUS == (int)InvoiceStatus.Draft)
            {
                this.Header.Add(InvoiceHeaderInfo.DayInvoice, "...");
                this.Header.Add(InvoiceHeaderInfo.MonthInvoice, "...");
                this.Header.Add(InvoiceHeaderInfo.YearInvoice, "...");
            }
            else
            {
                this.Header.Add(InvoiceHeaderInfo.DayInvoice, this.invoiceInfo.RELEASEDDATE == null ? "..." : this.invoiceInfo.RELEASEDDATE?.Day.ToString().PadLeft(2, '0'));
                this.Header.Add(InvoiceHeaderInfo.MonthInvoice, this.invoiceInfo.RELEASEDDATE == null ? "..." : this.invoiceInfo.RELEASEDDATE?.Month.ToString().PadLeft(2, '0'));
                this.Header.Add(InvoiceHeaderInfo.YearInvoice, this.invoiceInfo.RELEASEDDATE == null ? "..." : this.invoiceInfo.RELEASEDDATE?.Year.ToString());
            }

            int invNo = 0;
            if (int.TryParse(this.invoiceInfo.NO, out invNo))
            {
                var verificationCode = this.invoiceInfo.VERIFICATIONCODE.ConvertToString();
                if (string.IsNullOrWhiteSpace(verificationCode))
                {
                    verificationCode = " ... ";
                }
                string UrlPortal = PathUtil.UrlCombine(this.GetConfig("PortalCompanyLink"), $"#/invoice-list?type=true&id={verificationCode}");
                string UrlPortalText = this.GetConfig("PortalCompanyLinkText");

                string portalSearchInfo = string.Format("Hóa đơn điện tử được tra cứu trực tuyến tại: <a target=\"_blank\" href=\"{0}\">{1}</a> Mã tra cứu: {2}", UrlPortal, UrlPortalText, verificationCode);
                //var portalSearchInfo = string.Format("<p>Mã tra cứu <b>{0}</b> tại <a target=\"_blank\" href=\"{1}\"><b>{2}</b></a></p>", verificationCode, UrlPortal, UrlPortalText);
                var portalSearchInfoEn = string.Format("Please access: <a target=\"_blank\" href=\"{0}\">{1}</a> for online inquiry of e-invoices/ Searching code: {2}", UrlPortal, UrlPortalText, verificationCode);
                this.Header.Add(InvoiceHeaderInfo.PortalSearchInfo, portalSearchInfo);
                this.Header.Add(InvoiceHeaderInfo.PortalSearchInfoEn, portalSearchInfoEn);
            }
            else
            {
                this.Header.Add(InvoiceHeaderInfo.PortalSearchInfo, "");
                this.Header.Add(InvoiceHeaderInfo.PortalSearchInfoEn, "");
            }
        }
        private void FillAmountTax(StringBuilder htmlInvoice)
        {

            if (invoiceInfo.INVOICESTATUS == (int)InvoiceStatus.Draft)
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.AmountTax, "");
            }
            else if (!this.invoiceInfo.TOTALTAX.HasValue)
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.AmountTax, null);
            }
            else
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.AmountTax, String.Format(FormatNumber.GetStringFormat(this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting?.Amount), this.invoiceInfo.TOTALTAX.ToDecimal()));
            }
            
        }

        private Tuple<decimal, decimal> CalculateTax510()
        {
            decimal tax5 = 0;
            decimal tax10 = 0;
            foreach (var item in this.invoiceInfo.InvoiceItems)
            {
                if (item.TaxId == 2)
                {
                    tax5 += (decimal)item.AmountTax;
                }
                if (item.TaxId == 3)
                {
                    tax10 += (decimal)item.AmountTax;
                }
            }
            return Tuple.Create(tax5, tax10);
        }

        private void FillTotalInvoice(StringBuilder htmlInvoice, string page, bool isFillTotal = false)
        {
            htmlInvoice.Replace(InvoiceHeaderInfo.CurrentPage, page);
            string checkValidImgSrc = string.Concat(this.config.FullPathFileAsset.Reverse().Skip(5).Reverse());   // Bỏ chữ Asset
            var checkValidSrcConfig = this.GetConfig("CheckValidImage").Replace('/', '\\');
            checkValidImgSrc += checkValidSrcConfig.Substring(7);
            string checkValidImgBase64 = GetBase64StringImage(checkValidImgSrc);

            if (!isFillTotal)
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.TaxOfInvoice, string.Empty);
                htmlInvoice.Replace(InvoiceHeaderInfo.AmountTax, string.Empty);
                htmlInvoice.Replace(InvoiceHeaderInfo.displayDiscount, string.Empty);
                htmlInvoice.Replace(InvoiceHeaderInfo.AmountDiscount, string.Empty);
                htmlInvoice.Replace(InvoiceHeaderInfo.TotalDiscountTax, string.Empty);
                htmlInvoice.Replace(InvoiceHeaderInfo.AmountInwords, string.Empty);
                htmlInvoice.Replace(InvoiceHeaderInfo.Total, string.Empty);
                htmlInvoice.Replace(InvoiceHeaderInfo.AmountTotal, string.Empty);

                string cssVisible = "visibility: hidden;";
                htmlInvoice.Replace(InvoiceHeaderInfo.ShowSign, cssVisible);

                //// Chèn dấu tick
                //htmlInvoice.Replace(InvoiceHeaderInfo.ImgSignDraft, string.Empty);
                //// Chèn tên công ty
                //htmlInvoice.Replace(InvoiceHeaderInfo.CompanyNameSignDraft, string.Empty);

                //// Xóa dòng ẩn
                //htmlInvoice.Replace(this.GetConfig("MarkSellerSign"), string.Empty);
                //htmlInvoice.Replace(this.GetConfig("MarkBuyerSign"), string.Empty);
                //htmlInvoice.Replace(InvoiceHeaderInfo.SignDate, string.Empty);

                return;
            }

            this.FillTax(htmlInvoice);
            FillAmountTax(htmlInvoice);
            this.MessageHaveCode(htmlInvoice);
            if (invoiceInfo.INVOICETYPE == 1 || invoiceInfo.INVOICETYPE == 0)
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.displayDiscount, "<tr>");
                this.FooterInvoiceNotUpDown(htmlInvoice);
            }
            else if (invoiceInfo.INVOICETYPE == (int)InvoiceType.AdjustmentUpDown || 
                invoiceInfo.INVOICETYPE == (int)InvoiceType.AdjustmentTax || 
                invoiceInfo.INVOICETYPE == (int)InvoiceType.AdjustmentTaxCode ||
                invoiceInfo.INVOICETYPE == (int)InvoiceType.AdjustmentElse)
            {
                this.FillAmountUpDown(htmlInvoice);
            }
            else if (invoiceInfo.INVOICETYPE == 3)
            {
                string dis = "<tr style='display:none'>";
                htmlInvoice.Replace(InvoiceHeaderInfo.displayDiscount, dis);
                this.FooterInvoiceNotUpDown(htmlInvoice);
            }
            //Logger logger = new Logger();
            //logger.Error("SystemSetting.Amount: " + this.SystemSetting.Amount, new Exception("CreateItemInvoice"));
            if (!invoiceInfo.TOTALDISCOUNTTAX.HasValue)
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.AmountDiscount, "");
            }
            else
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.AmountDiscount, String.Format(FormatNumber.GetStringFormat(this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Amount), this.invoiceInfo.TOTALDISCOUNT.ToDecimal()));
            }
            if (!invoiceInfo.TOTALDISCOUNTTAX.HasValue)
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.TotalDiscountTax, "");
            }
            else
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.TotalDiscountTax, String.Format(FormatNumber.GetStringFormat(this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Amount), this.invoiceInfo.TOTALDISCOUNTTAX.ToDecimal()));
            }
            htmlInvoice.Replace(InvoiceHeaderInfo.AmountInwords, this.invoiceInfo.AmountInwords);
            var formatDecimalTotal = this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Amount;
            htmlInvoice.Replace(InvoiceHeaderInfo.Total, String.Format(FormatNumber.GetStringFormat(formatDecimalTotal), this.invoiceInfo.TOTAL.ToDecimal()));
            htmlInvoice.Replace(InvoiceHeaderInfo.AmountTotal, String.Format(FormatNumber.GetStringFormat(this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Amount), this.invoiceInfo.SUM.ToDecimal()));

            
            // Chèn dấu tick
            htmlInvoice.Replace(InvoiceHeaderInfo.ImgSignDraft, checkValidImgBase64);
            // Chèn tên công ty
            htmlInvoice.Replace(InvoiceHeaderInfo.CompanyNameSignDraft, this.invoiceInfo.COMPANYNAME);

            // Xóa dòng ẩn
            htmlInvoice.Replace(this.GetConfig("MarkSellerSign"), string.Empty);
            htmlInvoice.Replace(this.GetConfig("MarkBuyerSign"), string.Empty);
            htmlInvoice.Replace(InvoiceHeaderInfo.SignDate, this.invoiceInfo.RELEASEDDATE.Value.ToString("dd/MM/yyyy"));
        }

        private void FillTax(StringBuilder htmlInvoice)
        {
            if (this.invoiceInfo.TAXNAME.IsNullOrEmpty())
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.TaxOfInvoice, @"0%");
            }
            else
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.TaxOfInvoice, this.invoiceInfo.TAXNAME);
            }
        }

        private void FillAmountUpDown(StringBuilder htmlInvoice)
        {
            Action<decimal, decimal, string, string> insertUpDown = (up, down, upDownSign, upDownBilingualSign) =>
            {
                if (up > down)
                {
                    htmlInvoice.Replace(upDownSign, " (tăng)");
                    htmlInvoice.Replace(upDownBilingualSign, " (up)");
                }
                else if (up == down)
                {
                    htmlInvoice.Replace(upDownSign, string.Empty);
                    htmlInvoice.Replace(upDownBilingualSign, string.Empty);
                }
                else
                {
                    htmlInvoice.Replace(upDownSign, " (giảm)");
                    htmlInvoice.Replace(upDownBilingualSign, " (down)");
                }
            };

            string dis = "<tr style='display:none'>";
            htmlInvoice.Replace(InvoiceHeaderInfo.displayDiscount, dis);
            decimal TotalUp = 0;
            decimal TotalDow = 0;
            decimal SumUp = 0;
            decimal SumDow = 0;
            decimal TaxUp = 0;
            decimal TaxDow = 0;
            foreach (var item in this.invoiceInfo.InvoiceItems)
            {
                if (item.AdjustmentType == 1)
                {
                    TotalUp += item.Total ?? 0;
                    SumUp += item.AmountTotal;
                    TaxUp += item.AmountTax ?? 0;
                }
                else
                {
                    TotalDow += item.Total ?? 0;
                    SumDow += item.AmountTotal;
                    TaxDow += item.AmountTax ?? 0;
                }

            }
            //Total
            insertUpDown(TotalUp, TotalDow, InvoiceHeaderInfo.isUpDowTotal, InvoiceHeaderInfo.isUpDowTotalBilingual);
            //Sum
            insertUpDown(SumUp, SumDow, InvoiceHeaderInfo.isUpDowSum, InvoiceHeaderInfo.isUpDowSumBilingual);
            //Total tax
            insertUpDown(TaxUp, TaxDow, InvoiceHeaderInfo.isUpDowTax, InvoiceHeaderInfo.isUpDowTaxBilingual);
        }

        private void FooterInvoiceNotUpDown(StringBuilder htmlInvoice)
        {
            htmlInvoice.Replace(InvoiceHeaderInfo.isUpDowTotal, string.Empty);
            htmlInvoice.Replace(InvoiceHeaderInfo.isUpDowTotalBilingual, string.Empty);
            htmlInvoice.Replace(InvoiceHeaderInfo.isUpDowTax, string.Empty);
            htmlInvoice.Replace(InvoiceHeaderInfo.isUpDowTaxBilingual, string.Empty);
            htmlInvoice.Replace(InvoiceHeaderInfo.isUpDowTax5Bilingual, string.Empty);
            htmlInvoice.Replace(InvoiceHeaderInfo.isUpDowTax10Bilingual, string.Empty);
            htmlInvoice.Replace(InvoiceHeaderInfo.isUpDowSum, string.Empty);
            htmlInvoice.Replace(InvoiceHeaderInfo.isUpDowSumBilingual, string.Empty);
        }

        private void BuildItemInvoice()
        {
            StringBuilder detailInvoice = new StringBuilder();
            int identity = 0;   // Thứ tự của sản phẩm
            int numberRow = 0;
            int realRow = 0;  // Thứ tự của sản phẩm

            int i = 0;
            var pageSplit = new Dictionary<int, int>(this.PageSplit);   // Số lượng page và số record trong mỗi page
            if (pageSplit.Keys.Count > 0)
            {
                pageSplit.Remove(pageSplit.Keys.Last());
            }

            int numberItemInvoice = this.TotalRowOfName + this.BonusRow;
            this.ItemDetailInvoice.ForEach(p =>
            {
                identity++;
                int numberLine = 1;
                NumberRowInHtml numberRowInHTML;
                bool getRowSuccess = this.NumberRowInHTMLs.TryGetValue(identity, out numberRowInHTML);
                if (getRowSuccess)
                {
                    numberLine = numberRowInHTML.NumberOfRow;
                }
                numberRow = numberRow + numberLine;
                realRow += numberLine;

                int split = 0;
                var getOk = pageSplit.TryGetValue(i, out split);
                if (getOk)
                {
                    if (numberRow < split)
                    {
                        detailInvoice.Append(SetDataToItemInvoice(identity, p));
                    }
                    else if (numberRow == split)
                    {
                        i++;
                        numberRow = 0;
                        detailInvoice.Append(SetDataToItemInvoice(identity, p));
                        DetailInvoice.Add(detailInvoice.ToString());
                        detailInvoice = new StringBuilder();
                    }
                    else
                    {
                        i++;
                        // add 1 phần product name vô page cũ
                        numberRow = numberRow - split;
                        var listName = this.NumberRowInHTMLs[identity].NameCuttedByLength;
                        var list1 = listName.GetRange(0, numberLine - numberRow);
                        var list2 = listName.GetRange(numberLine - numberRow, listName.Count - (numberLine - numberRow));
                        detailInvoice.Append(SetDataToItemInvoice(identity, p, list1));
                        DetailInvoice.Add(detailInvoice.ToString());
                        detailInvoice = new StringBuilder();
                        // add phần product name còn lại vô page mới
                        detailInvoice.Append(SetDataToItemInvoice(identity, new InvoiceItem(this.IsShowOrder), list2));
                    }
                }
                else if (realRow == numberItemInvoice)
                {
                    i++;
                    detailInvoice.Append(SetDataToItemInvoice(identity, p));
                    DetailInvoice.Add(detailInvoice.ToString());
                    detailInvoice = new StringBuilder();
                }
                else
                {
                    detailInvoice.Append(SetDataToItemInvoice(identity, p));
                }

            });

            if (this.PageSplit.Keys.Count > 0 && this.PageSplit.Last().Value > this.NumberRecord)
            {
                this.DetailInvoice.Add("");
            }

            for (int j = 0; j < this.DetailInvoice.Count - 1; j++)
            {
                DetailInvoice[j] += "</tbody> </table> </div>";
            }


        }

        readonly Func<decimal?, int, string> formatDecimalBySystemSetting = (value, systemSettingValue) =>
        {
            if (!value.HasValue)
                return string.Empty;
            return value.ToDecimalFomat(FormatNumber.GetStringFormat(systemSettingValue));
        };
        readonly Func<decimal, int, string> formatNumberBySystemSetting = (value, systemSettingValue) =>
        {
            if (value == 0)
                return string.Empty;
            return value.ToDecimalFomat(FormatNumber.GetStringFormat(systemSettingValue));
        };
        readonly Func<bool?, int, string> formatBoolBySystemSetting = (value, systemSettingValue) =>
        {
            if (!value.HasValue)
                return string.Empty;
            return value.ToDecimalFomat(FormatNumber.GetStringFormat(systemSettingValue));
        };

        private string SetDataToItemInvoice(int identity, InvoiceItem item, List<string> listProductName = null)
        {
            StringBuilder htmlInvoice = new StringBuilder();
            string orderRealNum = string.Empty;
            if (item.IsShowOrder)
            {
                 orderRealNum = identity.ToString();
            }

            if (item.ProductName.IsNullOrEmpty() && invoiceInfo.INVOICESTATUS != (int)InvoiceStatus.Draft) orderRealNum = "&nbsp;";
            if (item.FirstRow == true) orderRealNum = "&nbsp;";
            htmlInvoice.Append(TemplateInvoiceDetail
                .Replace(ConfigInvoiceDetaiInfo.Identity, orderRealNum)
                .Replace(ConfigInvoiceDetaiInfo.Unit, item.Unit)
                .Replace(ConfigInvoiceDetaiInfo.Quantity, formatDecimalBySystemSetting(item.Quantity, this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Quantity))
                .Replace(ConfigInvoiceDetaiInfo.Discount, formatBoolBySystemSetting(item.Discount, this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Amount))
                .Replace(ConfigInvoiceDetaiInfo.AmountDiscount, formatDecimalBySystemSetting(item.AmountDiscount, this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Amount)));
            htmlInvoice.Replace(ConfigInvoiceDetaiInfo.Price, formatDecimalBySystemSetting(item.Price, this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Price));
            htmlInvoice.Replace(ConfigInvoiceDetaiInfo.AmountTotal, formatNumberBySystemSetting(item.AmountTotal, this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Amount));

            if (this.invoiceInfo.ISDISCOUNT == 1)
            {
                htmlInvoice.Replace(ConfigInvoiceDetaiInfo.Tax, item.DiscountRatio.ToString());
                htmlInvoice.Replace(ConfigInvoiceDetaiInfo.AmountTax, formatDecimalBySystemSetting(item.AmountDiscount, this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Amount));
            }
            else
            {
                htmlInvoice.Replace(ConfigInvoiceDetaiInfo.Tax, item.DisplayInvoice.IsNullOrEmpty() ? string.Empty : item.DisplayInvoice);
                htmlInvoice.Replace(ConfigInvoiceDetaiInfo.AmountTax, formatDecimalBySystemSetting(item.AmountTax, this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Amount));
            }
            if (invoiceInfo.IMPORTTYPE != 2)
            {
                if ((invoiceInfo.VAT_INVOICE_TYPE != null || !string.IsNullOrEmpty(invoiceInfo.VAT_INVOICE_TYPE)) && identity == 1)
                {
                    if (invoiceInfo.VAT_INVOICE_TYPE.Contains("3"))
                    {
                        //newDetail.ProductCode = oldDetail.ProductCode;
                        //newDetail.Unit = oldDetail.Unit;
                        //newDetail.Quantity = oldDetail.Quantity;

                        if (invoiceInfo.REPORT_CLASS != null || !string.IsNullOrEmpty(invoiceInfo.REPORT_CLASS))
                        {
                            string productTypeInvoice = String.Empty;
                            if (invoiceInfo.REPORT_CLASS.Contains("6"))
                            {
                                productTypeInvoice = "Tiền lãi ngân hàng(interest collection)";
                            }
                            else
                            {
                                productTypeInvoice = "Phí dịch vụ ngân hàng ( Fee commission)";
                            }
                            htmlInvoice.Replace(InvoiceHeaderInfo.DisplayTypeInvoice, null);
                            htmlInvoice.Replace(InvoiceHeaderInfo.TypeInvoice, productTypeInvoice);
                            htmlInvoice.Replace(InvoiceHeaderInfo.AmountTotal, String.Format(FormatNumber.GetStringFormat(this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Amount), this.invoiceInfo.TOTAL.ToDecimal()));//trung nói là tiền chưa thuế, phương k liên quan
                        }
                        else
                        {
                            htmlInvoice.Replace(InvoiceHeaderInfo.DisplayTypeInvoice, "none");
                            htmlInvoice.Replace(InvoiceHeaderInfo.TypeInvoice, null);
                        }
                    }
                    else
                    {
                        htmlInvoice.Replace(InvoiceHeaderInfo.DisplayTypeInvoice, "none");
                        htmlInvoice.Replace(InvoiceHeaderInfo.TypeInvoice, null);
                    }
                }
                else
                {
                    htmlInvoice.Replace(InvoiceHeaderInfo.DisplayTypeInvoice, "none");
                    htmlInvoice.Replace(InvoiceHeaderInfo.TypeInvoice, null);
                }
            }
            else
            {
                htmlInvoice.Replace(InvoiceHeaderInfo.DisplayTypeInvoice, "none");
                htmlInvoice.Replace(InvoiceHeaderInfo.TypeInvoice, null);
            }

            this.SubSetDataToItemInvoice(identity, item, htmlInvoice, listProductName);

            return htmlInvoice.ToString();
        }

        private void SubSetDataToItemInvoice(int identity, InvoiceItem item, StringBuilder htmlInvoice, List<string> listProductName = null)
        {
            if (listProductName == null)
            {
                NumberRowInHtml numberRowInHTML;
                var getNameSuccess = this.NumberRowInHTMLs.TryGetValue(identity, out numberRowInHTML);
                if (getNameSuccess)
                {
                    htmlInvoice.Replace(ConfigInvoiceDetaiInfo.ProductName, string.Join("<br>", this.NumberRowInHTMLs[identity].NameCuttedByLength));
                }
                else
                {
                    htmlInvoice.Replace(ConfigInvoiceDetaiInfo.ProductName, item.ProductName);
                }
            }
            else
            {
                htmlInvoice.Replace(ConfigInvoiceDetaiInfo.ProductName, string.Join("<br>", listProductName));
            }
            htmlInvoice.Replace(ConfigInvoiceDetaiInfo.Total, item.Total == 0 ? string.Empty : item.Total.ToDecimalFomat(FormatNumber.GetStringFormat(this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Amount)));
            htmlInvoice.Replace(ConfigInvoiceDetaiInfo.Price, item.Total == 0 ? string.Empty : item.Total.ToDecimalFomat(FormatNumber.GetStringFormat(this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Price)));
            htmlInvoice.Replace(ConfigInvoiceDetaiInfo.Quantity, item.Quantity == 0 ? string.Empty : item.Quantity.ToDecimalFomat(FormatNumber.GetStringFormat(this.invoiceInfo.CURRENCYCODE == "VND" ? 0 : this.SystemSetting.Quantity)));
            htmlInvoice.Replace(InvoiceHeaderInfo.Currency, item.AmountTotal == 0 ? string.Empty : this.invoiceInfo.CURRENCYCODE);

            // Field References của HuaNan
            htmlInvoice.Replace(ConfigInvoiceDetaiInfo.Reference, string.IsNullOrEmpty(item.ProductName) ? "" : this.invoiceInfo.REFNUMBER);
        }

        private void CountRecordsForEachPage()
        {
            var pageSplit = new Dictionary<int, int>();
            int i = 0;
            int totalRowOfName = this.TotalRowOfName;

            while (totalRowOfName > 0)
            {
                if (totalRowOfName <= this.NumberRecord)
                {
                    pageSplit.Add(i++, totalRowOfName);
                    totalRowOfName = 0;
                }
                else
                {
                    if (totalRowOfName > this.NumberLineOfPage)
                    {
                        pageSplit.Add(i++, this.NumberLineOfPage);
                        totalRowOfName = totalRowOfName - this.NumberLineOfPage;
                    }
                    else
                    {
                        pageSplit.Add(i++, totalRowOfName);
                        totalRowOfName = 0;
                    }
                }
            }

            this.PageSplit = pageSplit;
        }

        private void SetNumberInvoiceItemByConfig()
        {
            if (invoiceInfo.InvoiceItems == null)
            {
                this.ItemDetailInvoice = new List<InvoiceItem>();
            }
            else
            {
                this.ItemDetailInvoice = this.invoiceInfo.InvoiceItems.ToList();
            }

            int numberLineByItemInvoice = this.TotalRowOfName;

            int bonus = CaculatorNumberInvoice(numberLineByItemInvoice, this.NumberLineOfPage);
            for (int i = 0; i < bonus; i++)
            {
                this.ItemDetailInvoice.Add(new InvoiceItem(this.IsShowOrder));
            }
            this.BonusRow = bonus;
        }

        private int CaculatorNumberInvoice(int currentNumberItem, int maxNumberItemConfig)
        {
            if (currentNumberItem == 0 || maxNumberItemConfig == 0)
            {
                return maxNumberItemConfig;
            }
            int bonus = 0;
            // Sửa lại cho InvoiceHeader có mặt trên mỗi page
            int numberItem = this.PageSplit.Last().Value;
            //

            if (numberItem > 0)
            {
                if (numberItem <= this.NumberRecord)
                {
                    bonus = this.NumberRecord - numberItem;
                }
                else
                {
                    bonus = this.NumberLineOfPage - numberItem;
                }
            }

            return bonus;
        }

        private string BuilderImage()
        {
            string imageMark = string.Empty;

            if (invoiceInfo.MarkImage.IsNotNullOrEmpty())
            {
                string fullPathFile = Path.Combine(this.config.FullPathFileAsset, invoiceInfo.MarkImage);
                imageMark = string.Format("<img src=\"data:image/png;base64, {0}\" />", GetBase64StringImage(fullPathFile));
            }

            return imageMark;
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
            if (!isShowPage)
            {
                return string.Empty;
            }

            return string.Format("<p style=\"margin:0px; color:red;\"> Tiếp theo trang {0}/{1}  </p>", page, totalPage);
        }

        //Kiểm tra product name của từng item có cần xuống hàng không, trả về tổng số hàng
        private int CountRowForEachInvoiceItem()
        {
            var totalRow = 0;
            float fieldLength = 4.20f;  // chiều dài field Diễn giải là 11cm ~~ 4.33 inchs (do khác font nên chỉnh lại 4.2 inch) / xem trong file template
            var i = 1;
            foreach (var item in this.invoiceInfo.InvoiceItems)
            {
                var numberRowInHtml = new NumberRowInHtml();
                numberRowInHtml = new NumberRowInHtml(item.ProductName, fieldLength, 16);
                this.NumberRowInHTMLs.Add(i++, numberRowInHtml);
                totalRow += numberRowInHtml.NumberOfRow;
            }

            return totalRow;
        }

        public ExportFileInfo ExportDeclarationFile(DeclarationExportModel exportModel)
        {
            var fullFilePath = exportModel.FilePath;

            var detailTemplate = GetTemplateFileHtml(exportModel.DetailTemplateFile).ToString();
            var detailHtml = "";
            if (exportModel.DataExport?.Details != null
                && exportModel.DataExport.Details.Count > 0)
            {
                foreach (var detail in exportModel.DataExport?.Details)
                {
                    var clonedDetailTemplate = new StringBuilder(detailTemplate);
                    clonedDetailTemplate.Replace("{STT}", detail.STT);
                    clonedDetailTemplate.Replace("{ReleaseConpanyName}", detail.ReleaseConpanyName);
                    clonedDetailTemplate.Replace("{Seri}", detail.Seri);
                    clonedDetailTemplate.Replace("{FromDate}", detail.FromDate);
                    clonedDetailTemplate.Replace("{ToDate}", detail.ToDate);
                    clonedDetailTemplate.Replace("{RegisterTypeID}", ConvertRegisterTypeID(detail.RegisterTypeID));

                    detailHtml += "\n" + clonedDetailTemplate.ToString();
                }
            }

            var mainData = exportModel.DataExport;
            var mainTemplate = GetTemplateFileHtml(exportModel.TemplateFile);
            mainTemplate.Replace("{DeclarationTypeID}", mainData.DeclarationTypeID);
            mainTemplate.Replace("{TaxPayerName}", mainData.TaxPayerName);
            mainTemplate.Replace("{TaxCode}", mainData.TaxCode);
            mainTemplate.Replace("{TaxDepartmentName}", mainData.TaxDepartmentName);
            mainTemplate.Replace("{PeronalContact}", mainData.PeronalContact);
            mainTemplate.Replace("{Tel1}", mainData.Tel1);
            mainTemplate.Replace("{Address}", mainData.Address);
            mainTemplate.Replace("{Email}", mainData.Email);
            mainTemplate.Replace("{HTHDon}", mainData.HTHDon);
            mainTemplate.Replace("{HTGDLHDDT}", mainData.HTGDLHDDT);
            mainTemplate.Replace("{PThuc}", mainData.PThuc);
            mainTemplate.Replace("{LHDSDung}", mainData.LHDSDung);
            mainTemplate.Replace("{ReleaseInfoRows}", detailHtml);

            var option = new ExportOption();
            option.Height = 595;
            option.Width = 824;
            option.PageOrientation = "Portrait";
            option.PageSize = "A4";

            SelectExportFile.HtmlToPdfConverter(mainTemplate.ToString(), fullFilePath, option);
            ExportFileInfo fileInfo = new ExportFileInfo(fullFilePath);

            return fileInfo;
        }

        private void MessageHaveCode(StringBuilder htmlInvoice)
        {

            //if (this.invoiceInfo.HTHDon == HTHDon.CoMa)
            //{
            //    if (this.invoiceInfo.MCQT != null)
            //    {
            //        htmlInvoice.Replace(InvoiceHeaderInfo.InvoiceHaveCode, "Mã của cơ quan Thuế: " + this.invoiceInfo.MCQT);
            //    }

            //    htmlInvoice.Replace(InvoiceHeaderInfo.InvoiceHaveCode, "Mã của cơ quan Thuế: ........................");
            //}
            //else
            //{
                htmlInvoice.Replace(InvoiceHeaderInfo.InvoiceHaveCode, null);
            //}

        }
        

        private string ConvertRegisterTypeID(String registerTypeID)
        {
            if (registerTypeID == "1")
            {
                registerTypeID = "Thêm mới";
            }
            else if (registerTypeID == "2")
            {
                registerTypeID = "Gia hạn";
            }
            else
            {
                registerTypeID = "Ngừng sử dụng";
            }
            return registerTypeID;
        }
    }
}
