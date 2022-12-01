using InvoiceServer.Common.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace InvoiceServer.Business.Helper
{
    public static class FileSign
    {
        private static readonly string templateImageSingNormal = "temp_sign.bmp";
        private static readonly string templateImageSingSmall = "temp_sign_Small.bmp";
        private static readonly string templateImageSingBig = "temp_sign_big.bmp";
        private static readonly string prefixFileSign = ".png";
        private const int NumbercharaterInLineSign = 40;
        private const double RatioConverToUpperToToLower = 1.2;
        public static string CreateFileSign(string companyName, string folderAsset, string folderAssetOfCompany, string fileNameSign)
        {
            using (Bitmap bitmap = CreateImages(companyName, folderAsset, folderAssetOfCompany))
            {
                string pathContentFileTemp = Path.Combine(folderAssetOfCompany, string.Format("{0}{1}", fileNameSign, prefixFileSign));
                bitmap.Save(pathContentFileTemp, ImageFormat.Png);//save the image file
                return pathContentFileTemp;
            }
        }

        public static Bitmap CreateImages(string companyName, string folderAsset, string folderAssetOfCompany)
        {
            string signCompanyname = String.Format("Ký bởi: {0}", companyName);
            string contentsDateSign = string.Format("{0}\nNgày ký:......................", signCompanyname);
            int numberLine = contentsDateSign.NumberLine(NumbercharaterInLineSign, RatioConverToUpperToToLower);
            Bitmap bitmap = CreateImageTemplateFileByLineOfText(numberLine, folderAsset);
            PointF firstSign = new PointF(5f, 5f);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {

                using (Font arialFont = new Font("Arial", 9, FontStyle.Bold))
                {
                    RectangleF r = new RectangleF(5, 17, (bitmap.Width - 4), bitmap.Height);

                    graphics.DrawString("Signature Valid", arialFont, Brushes.Blue, firstSign);
                    graphics.DrawString(contentsDateSign, arialFont, Brushes.Blue, r);
                }
            }

            return bitmap;
        }

        private static Bitmap CreateImageTemplateFileByLineOfText(int line, string folderAsset)
        {
            string imageFilePath = CalculatorImageTemplateByNumberLine(line);
            string pathContentFileTemp = Path.Combine(folderAsset, imageFilePath);
            Bitmap bitmap = (Bitmap)Image.FromFile(pathContentFileTemp);//load the image file
            return bitmap;
        }

        private static string CalculatorImageTemplateByNumberLine(int numberLine)
        {
            string imageFilePath;
            switch (numberLine)
            {
                case 1:
                    imageFilePath = templateImageSingSmall;
                    break;
                case 2:
                    imageFilePath = templateImageSingNormal;
                    break;
                default:
                    imageFilePath = templateImageSingBig;
                    break;
            }
            return imageFilePath;
        }
    }
}
