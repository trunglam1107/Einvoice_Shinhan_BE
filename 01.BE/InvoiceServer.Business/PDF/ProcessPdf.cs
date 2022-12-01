namespace InvoiceServer.Business
{
    public static class ProcessPdf
    {
        //public static void SignatureFile(PrintConfig config)
        //{
        //    string fileNameSignature = string.Empty;
        //    using (MemoryStream stream = new System.IO.MemoryStream())
        //    {
        //        PdfReader reader = new PdfReader(config.FullPathFileNameExport);
        //        PdfStamper stemper = new PdfStamper(reader, new FileStream(config.FullPathFileNameExport.Replace(".pdf", "_Sign.pdf"), FileMode.Create));
        //        PdfContentByte over = stemper.GetOverContent(1);

        //        if (File.Exists(config.FullPathSignaturePicture))
        //        {
        //            float signPositionX = over.PdfDocument.PageSize.Width - 160f;
        //            float signPositionY = 90f;
        //            iTextSharp.text.Image singer = iTextSharp.text.Image.GetInstance(config.FullPathSignaturePicture);
        //            singer.ScaleToFit(150, 80);
        //            singer.Alignment = iTextSharp.text.Image.BOTTOM_BORDER;
        //            singer.SetAbsolutePosition(signPositionX, signPositionY);
        //            over.AddImage(singer);
        //        }

        //        stemper.Close();
        //        reader.Close();
        //    }
        //}
    }
}
