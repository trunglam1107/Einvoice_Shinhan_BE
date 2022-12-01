using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace InvoiceServer.Business.ExportXml
{
    public class InvoiceXmlJob
    {
        public readonly InvoicePrintModel invoice;
        public readonly PrintConfig Config;
        public readonly SystemSettingInfo systemSetting;
        private readonly ParseXMLJob xml;
        public InvoiceXmlJob(InvoiceExportXmlModel invoiceExportXmlModel)
        {
            this.Config = invoiceExportXmlModel.PrintConfig;
            this.invoice = invoiceExportXmlModel.InvoicePrintModel;
            this.systemSetting = invoiceExportXmlModel.SystemSettingInfo;
            xml = new ParseXMLJob(invoiceExportXmlModel);
        }

        public ExportFileInfo ExportReport()
        {
            string fullPathFile = this.Config.FolderExportInvoice;
            if (!File.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }

            string fileName = string.Format("{0}.xml", this.invoice.ID);
            string filePathFileName = string.Format("{0}\\{1}", fullPathFile, fileName);
            string reportStringXml = string.Empty;
            //if (invoice.HTHDon == HTHDon.CoMa)
            //{
            //    reportStringXml = xml.GenerateXMLWithCode().ToString();
            //}
            //else
            //{
            reportStringXml = xml.GenerateXMLWithoutCode().ToString();
            //}
            //string reportStringXml = WriteXMLReport();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(reportStringXml);
            xmlDoc.Save(filePathFileName);
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = fileName;
            fileInfo.FullPathFileName = filePathFileName;
            return fileInfo;
        }
        public string ExportXmlJob()
        {
            return xml.GenerateXMLWithoutCode().ToString();
        }


        //private string WriteXMLReport()
        //{
        //    StringBuilder xmlReport = new StringBuilder();
        //    xmlReport.Append("<UnitInvoiceDataXMLSign xmlns:inv=\"https://newInvoice/invoicexml/v1\">");
        //    xmlReport.AppendFormat("<InvoiceDataXML Id=\"{0}\">", "Invoice_" + invoice.Id);
        //    xmlReport.Append("<InvoiceData>");
        //    xmlReport.AppendFormat("<Id>{0}</Id>", invoice.Id);
        //    xmlReport.AppendFormat("<parentId>{0}</parentId>", invoice.ParentId);
        //    xmlReport.AppendFormat("<code>{0}</code>", invoice.InvoiceCode.EscapeXMLValue());
        //    xmlReport.AppendFormat("<symbol>{0}</symbol>", invoice.Symbol.EscapeXMLValue());
        //    xmlReport.AppendFormat("<InvoiceNo>{0}</InvoiceNo>", invoice.InvoiceNo);
        //    xmlReport.AppendFormat("<CompanyName>{0}</CompanyName>", invoice.CompanyName.EscapeXMLValue());
        //    xmlReport.AppendFormat("<CompanyId>{0}</CompanyId>", invoice.CompanyId);
        //    xmlReport.AppendFormat("<TaxCode>{0}</TaxCode>", invoice.TaxCode.EscapeXMLValue());
        //    xmlReport.AppendFormat("<Tel>{0}</Tel>", invoice.Tel.EscapeXMLValue());
        //    xmlReport.AppendFormat("<Fax>{0}</Fax>", invoice.Fax.EscapeXMLValue());
        //    xmlReport.AppendFormat("<Address>{0}</Address>", invoice.Address.EscapeXMLValue());
        //    xmlReport.AppendFormat("<isOrg>{0}</isOrg>", invoice.IsOrg);
        //    xmlReport.AppendFormat("<status>{0}</status>", invoice.Invoice_Status);
        //    xmlReport.AppendFormat("<CustomerName>{0}</CustomerName>", invoice.CustomerName.EscapeXMLValue());
        //    xmlReport.AppendFormat("<PersonContact>{0}</PersonContact>", invoice.PersonContact.EscapeXMLValue());
        //    xmlReport.AppendFormat("<CustomerTaxCode>{0}</CustomerTaxCode>", invoice.CustomerTaxCode.EscapeXMLValue());
        //    xmlReport.AppendFormat("<CustomerCompanyName>{0}</CustomerCompanyName>", invoice.CustomerCompanyName.EscapeXMLValue());
        //    xmlReport.AppendFormat("<CustomerAddress>{0}</CustomerAddress>", invoice.CustomerAddress.EscapeXMLValue());
        //    xmlReport.AppendFormat("<CustomerBankAccount>{0}</CustomerBankAccount>", invoice.CustomerBankAccount.EscapeXMLValue());
        //    xmlReport.AppendFormat("<BankAccount>{0}</BankAccount>", invoice.BankAccount.EscapeXMLValue());
        //    xmlReport.AppendFormat("<TypePaymentCode>{0}</TypePaymentCode>", invoice.TypePaymentCode.EscapeXMLValue());
        //    xmlReport.AppendFormat("<VerificationCode>{0}</VerificationCode>", invoice.VerificationCode.EscapeXMLValue());
        //    xmlReport.AppendFormat("<ExchangeRate>{0}</ExchangeRate>", invoice.ExchangeRate);
        //    xmlReport.AppendFormat("<DateRelease>{0}</DateRelease>", invoice.DateRelease.HasValue? invoice.DateRelease.Value.ToString("dd/MM/yyyy").EscapeXMLValue():string.Empty);
        //    xmlReport.AppendFormat("<InvoiceSample>{0}</InvoiceSample>", invoice.InvoiceSample);
        //    xmlReport.AppendFormat("<ReplacedInvoiceCode>{0}</ReplacedInvoiceCode>", invoice.ReplacedInvoiceCode.EscapeXMLValue());
        //    xmlReport.AppendFormat("<ReplacedInvoiceNo>{0}</ReplacedInvoiceNo>", invoice.ReplacedInvoiceNo.EscapeXMLValue());
        //    xmlReport.AppendFormat("<ReplacedSymbol>{0}</ReplacedSymbol>", invoice.ReplacedSymbol.EscapeXMLValue());
        //    xmlReport.AppendFormat("<ReplacedReleaseDate>{0}</ReplacedReleaseDate>", invoice.ReplacedReleaseDate !=null? invoice.ReplacedReleaseDate.ToString("dd/MM/yyyy").EscapeXMLValue():string.Empty);
        //    xmlReport.AppendFormat("<ReportTel>{0}</ReportTel>", invoice.ReportTel.EscapeXMLValue());
        //    xmlReport.AppendFormat("<ReportWebsite>{0}</ReportWebsite>", invoice.ReportWebsite.EscapeXMLValue());
        //    xmlReport.AppendFormat("<CurrenCyName>{0}</CurrenCyName>", invoice.CurrenCyName.EscapeXMLValue());
        //    xmlReport.AppendFormat("<Total>{0}</Total>", invoice.Total);
        //    xmlReport.AppendFormat("<Tax>{0}</Tax>", invoice.TaxDisplay.EscapeXMLValue());
        //    xmlReport.AppendFormat("<TaxAmout5>{0}</TaxAmout5>", invoice.TaxAmout5);
        //    xmlReport.AppendFormat("<TaxAmout10>{0}</TaxAmout10>", invoice.TaxAmout10);
        //    xmlReport.AppendFormat("<TaxAmout>{0}</TaxAmout>", invoice.TaxAmout);
        //    xmlReport.AppendFormat("<Sum>{0}</Sum>", invoice.Sum);
        //    xmlReport.AppendFormat("<AmountInwords>{0}</AmountInwords>", invoice.AmountInwords.EscapeXMLValue());
        //    xmlReport.AppendFormat("<SignBy>{0}</SignBy>", invoice.Signature.EscapeXMLValue());
        //    xmlReport.AppendFormat("<SignDate>{0}</SignDate>", invoice.DateSign.EscapeXMLValue());
        //    xmlReport.AppendFormat("<statisticalSumTaxAmount>{0}</statisticalSumTaxAmount>", invoice.StatisticalSumTaxAmount);
        //    xmlReport.AppendFormat("<statisticalSumAmount>{0}</statisticalSumAmount>", invoice.StatisticalSumAmount);
        //    xmlReport.AppendFormat("<giftSumTaxAmount>{0}</giftSumTaxAmount>", invoice.GiftSumTaxAmount);
        //    xmlReport.AppendFormat("<giftSumAmount>{0}</giftSumAmount>", invoice.GiftSumAmount);
        //    xmlReport.AppendFormat("<RefNumber>{0}</RefNumber>", invoice.RefNumber);
        //    xmlReport.AppendFormat("<CurrencyCode>{0}</CurrencyCode>", invoice.CurrencyCode);
        //    xmlReport.Append("</InvoiceData>");
        //    xmlReport.Append("<InvoiceDetails>");
        //    xmlReport.Append(CreateItemReport());
        //    xmlReport.Append("</InvoiceDetails>");
        //    //xmlReport.Append("<Bills>");
        //    //xmlReport.Append(CreateBills());
        //    //xmlReport.Append("</Bills>");
        //    //xmlReport.Append("<Gifts>");
        //    //xmlReport.Append(CreateGifts());
        //    //xmlReport.Append("</Gifts>");
        //    xmlReport.Append("</InvoiceDataXML>");
        //    xmlReport.Append("</UnitInvoiceDataXMLSign>");
        //    return xmlReport.ToString();
        //}


        //private string CreateItemReport()
        //{
        //    StringBuilder itemReport = new StringBuilder();
        //    if (this.invoice.InvoiceItems.Count == 0)
        //    {
        //        return itemReport.ToString();
        //    }
        //    foreach (var item in this.invoice.InvoiceItems)
        //    {
        //        StringBuilder productName = new StringBuilder();
        //        if (item.Discount == null || item.Discount == false)
        //        {
        //            productName.Append(item.ProductName);
        //        }
        //        else
        //        {
        //            productName = productName.Append(item.DiscountDescription);
        //        }
        //        if (item.TaxId == 4)
        //        {
        //            item.AmountTax = null;
        //        }
        //        item.ProductName = productName.ToString();
        //        itemReport.Append("<InvoiceDetail>");
        //        itemReport.AppendFormat("<productName><![CDATA[{0}]]></productName>", item.ProductName.EscapeXMLValue());
        //        itemReport.AppendFormat("<Unit>{0}</Unit>", item.Unit);
        //        itemReport.AppendFormat("<Quantity>{0}</Quantity>", item.Quantity);
        //        itemReport.AppendFormat("<Price>{0}</Price>", FormatNumber.SetFractionDigit(item.Price, this.systemSetting.Amount));
        //        itemReport.AppendFormat("<Total>{0}</Total>", FormatNumber.SetFractionDigit(item.Total, this.systemSetting.Amount));
        //        itemReport.AppendFormat("<TaxName>{0}</TaxName>", item.DisplayInvoice);
        //        itemReport.AppendFormat("<AmountTax>{0}</AmountTax>", FormatNumber.SetFractionDigit(item.AmountTax, this.systemSetting.Amount));
        //        var AmountTotal = FormatNumber.SetFractionDigit(item.AmountTax, this.systemSetting.Amount) + FormatNumber.SetFractionDigit(item.Total, this.systemSetting.Amount);
        //        itemReport.AppendFormat("<AmountTotal>{0}</AmountTotal>", AmountTotal);
        //        itemReport.Append("</InvoiceDetail>");

        //    }

        //    return itemReport.ToString();
        //}

    }
}
