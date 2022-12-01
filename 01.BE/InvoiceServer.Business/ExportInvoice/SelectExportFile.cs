using HiQPdf;
using InvoiceServer.Business.Models;
//using SelectPdf;
using System;
using System.IO;

namespace InvoiceServer.Business.ExportInvoice
{
    public static class SelectExportFile
    {

        public static void HtmlToPdfConverter(string contentsPdfHtml, string filePathFileName, ExportOption option = null)
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
            pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(contentsPdfHtml, baseUrl);
            File.WriteAllBytes(filePathFileName, pdfBuffer);

        }
        //public static void HtmlToPdfConverter(string contentsPdfHtml, string filePathFileName, ExportOption option = null)
        //{
        //    //PdfPageSize pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize), option.PageSize, true);
        //    //PdfPageOrientation pdfOrientation = (PdfPageOrientation)Enum.Parse(typeof(PdfPageOrientation), option.PageOrientation, true);
        //    //HtmlToPdf converter = new HtmlToPdf();
        //    converter.Options.WebPageWidth = 824;
        //    converter.Options.WebPageHeight = 595;
        //    converter.Options.PdfPageSize = pageSize;
        //    converter.Options.PdfPageOrientation = pdfOrientation;
        //    converter.Options.MarginRight = 0;
        //    converter.Options.MarginTop = 0;
        //    converter.Options.MarginLeft = 0;
        //    converter.Options.MarginBottom = 0;
        //    SavePdf(contentsPdfHtml, filePathFileName, converter);
        //}

        //private static void SavePdf(string contentsPdfHtml, string filePathFileName, HtmlToPdf converter)
        //{
        //    doc = converter.ConvertHtmlString(contentsPdfHtml);
        //    doc.Save(filePathFileName);
        //    doc.Close();
        //}
    }
}
