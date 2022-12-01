using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InvoiceServer.Business.Helper
{
    public static class PdfGetTextData
    {
        // 
        private static readonly int _numberTimesToSearchNext = 5;
        public static PdfTextLocation FindFirstLocationOfTextReverse(string filePath, string textDetect)
        {
            // Temp File To Find Location Of Text
            var tempPath = filePath.Replace(".pdf", $"_Temp{DateTime.Now.Ticks.ToString()}.pdf");

            var reader = new PdfReader(filePath);
            var numberOfPage = reader.NumberOfPages;
            reader.Close();

            var res = new PdfTextLocation();

            for (int i = numberOfPage; i >= 1; i--)
            {
                // bản free chỉ cho đọc được file có số trang giới hạn, tách thành từng file nhỏ hơn để xử lý được
                ExtractPages(filePath, tempPath, i, i);

                using (var doc = new BitMiracle.Docotic.Pdf.PdfDocument(tempPath))
                {
                    var page = doc.Pages[0];
                    foreach (BitMiracle.Docotic.Pdf.PdfTextData textData in page.Canvas.GetTextData())
                    {
                        var text = textData.Text;
                        if (ContainMultiSpace(text, textDetect))
                        {
                            res.X = (float)textData.Position.X;
                            res.Y = (float)(page.Height - textData.Position.Y);
                            res.Page = i;

                            break;
                        }
                    }
                }
            }
            File.Delete(tempPath);

            return res;
        }

        // Tìm vị trí xuất hiện của phần tử cuối cùng trong listTextDetect, tuy nhiên các phần tử phải xuất hiện liên tục theo đúng thứ tự trong list
        public static PdfTextLocation FindLastLocationOfListTextReverse(string filePath, List<string> listTextDetect, int numberTimesToSearchNextCustom = -1)
        {
            if (listTextDetect.Count == 0)
            {
                return new PdfTextLocation();
            }
            listTextDetect.Reverse();
            var numberTimesSearchNextLocal = numberTimesToSearchNextCustom != -1 ? numberTimesToSearchNextCustom : _numberTimesToSearchNext;

            // Temp File To Find Location Of Text
            var tempPath = filePath.Replace(".pdf", $"_Temp{DateTime.Now.Ticks.ToString()}.pdf");

            var reader = new PdfReader(filePath);
            var numberOfPage = reader.NumberOfPages;
            reader.Close();

            var res = new PdfTextLocation();

            for (int i = numberOfPage; i >= 1; i--)
            {
                // bản free chỉ cho đọc được file có số trang giới hạn, tách thành từng file nhỏ hơn để xử lý được
                ExtractPages(filePath, tempPath, i, i);

                using (var doc = new BitMiracle.Docotic.Pdf.PdfDocument(tempPath))
                {
                    var page = doc.Pages[0];
                    var numberTimesToSearchNext = 0;
                    var index = 0;
                    var canvasTextData = page.Canvas.GetTextData();
                    for (var j = canvasTextData.Count - 1; j >= 0; j--)
                    {
                        var textData = canvasTextData[j];
                        var text = textData.Text;
                        if (ContainMultiSpace(text, listTextDetect[index]))
                        {
                            numberTimesToSearchNext = 0;
                            index++;
                            if (index == listTextDetect.Count)
                            {
                                res.X = (float)textData.Position.X;
                                res.Y = (float)(page.Height - textData.Position.Y);
                                res.Page = i;
                                break;
                            }
                        }
                        else
                        {
                            numberTimesToSearchNext++;
                            if (numberTimesToSearchNext >= numberTimesSearchNextLocal)
                            {
                                index = 0;
                            }
                        }
                    }
                }
            }
            File.Delete(tempPath);
            listTextDetect.Reverse();
            return res;
        }


        public static PdfTextLocation FindFirstLocationOfTextInPage(string filePath, string textDetect, int pageIndex)
        {
            // Temp File To Find Location Of Text
            var tempPath = filePath.Replace(".pdf", $"_Temp{DateTime.Now.Ticks.ToString()}.pdf");
            var reader = new PdfReader(filePath);
            reader.Close();

            var res = new PdfTextLocation();

            // bản free chỉ cho đọc được file có số trang giới hạn, tách thành từng file nhỏ hơn để xử lý được
            ExtractPages(filePath, tempPath, pageIndex, pageIndex);

            using (var doc = new BitMiracle.Docotic.Pdf.PdfDocument(tempPath))
            {
                var page = doc.Pages[0];
                foreach (BitMiracle.Docotic.Pdf.PdfTextData textData in page.Canvas.GetTextData())
                {
                    var text = textData.Text;
                    if (ContainMultiSpace(text, textDetect))
                    {
                        res.X = (float)textData.Position.X;
                        res.Y = (float)(page.Height - textData.Position.Y);
                        res.Page = pageIndex;

                        break;
                    }
                }
            }

            File.Delete(tempPath);

            return res;
        }

        // Tạo 1 file PDF mới với page từ startpage đến endpage
        private static void ExtractPages(string sourcePDFpath, string outputPDFpath, int startpage, int endpage)
        {
            PdfReader reader = null;
            Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage = null;

            reader = new PdfReader(sourcePDFpath);
            sourceDocument = new Document(reader.GetPageSizeWithRotation(startpage));
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPDFpath, System.IO.FileMode.Create));

            sourceDocument.Open();

            for (int i = startpage; i <= endpage; i++)
            {
                importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                pdfCopyProvider.AddPage(importedPage);
            }
            sourceDocument.Close();
            reader.Close();
        }

        /// <summary>
        /// Is text1 contain text2 with multispace between words
        /// </summary>
        /// <param name="text1"></param>
        /// <param name="text2"></param>
        /// <returns></returns>
        private static bool ContainMultiSpace(string text1, string text2)
        {
            var text1Joined = StandardText(text1);
            var text2Joined = StandardText(text2);

            return text1Joined.Contains(text2Joined);
        }


        private static string StandardText(string text)
        {
            StringBuilder bld1 = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    bld1 = bld1.Append(" ");
                }
                else
                {
                    bld1 = bld1.Append(text[i]);
                }
            }
            string textSameSpaceStyle = bld1.ToString();
            var listText = textSameSpaceStyle.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", listText);
        }
    }

    public class PdfTextLocation
    {
        public int Page { get; set; }

        public float X { get; set; }

        public float Y { get; set; }
    }
}
