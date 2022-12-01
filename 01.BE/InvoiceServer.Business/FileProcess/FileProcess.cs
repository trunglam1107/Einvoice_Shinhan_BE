using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace InvoiceServer.Business
{
    public static class FileProcess
    {
        private const string keyGetCompany = "CN=";
        private static readonly Logger logger = new Logger();
        public static string CreateFolderRelease(PrintConfig config, int ReleaseId, List<INVOICE> invoices)
        {
            string fullPathFile = Path.Combine(config.FullPathFileReleaseExport, ReleaseId.ToString());
            CreateFolder(fullPathFile);
            invoices.ForEach(p =>
            {
                string fileInvoice = Path.Combine(config.FullPathFolderExportInvoice, string.Format("{0}_{1}.pdf", p.ID, p.NO));
                string fileInvoiceRelease = Path.Combine(fullPathFile, string.Format("{0}_{1}_Sign.pdf", p.ID, p.NO));
                File.Copy(fileInvoice, fileInvoiceRelease, true);
            });

            return ZipFIle(fullPathFile);
        }

        public static string CreateFolderRelease(PrintConfig config, int ReleaseId, List<ReleaseInvoiceInfo> invoices)
        {
            string fullPathFile = Path.Combine(config.FullPathFileReleaseExport, ReleaseId.ToString());
            CreateFolder(fullPathFile);
            invoices.ForEach(p =>
            {
                string fileInvoice = Path.Combine(config.FullPathFolderExportInvoice, string.Format("{0}_{1}.pdf", p.InvoiceId, p.InvoiceNo));
                string fileInvoiceRelease = Path.Combine(fullPathFile, string.Format("{0}_{1}_Sign.pdf", p.InvoiceId, p.InvoiceNo));
                File.Copy(fileInvoice, fileInvoiceRelease, true);
            });

            return ZipFIle(fullPathFile);
        }

        public static void ExtractFile(string fullPathSourceFile, string fullPathDesfile)
        {
            string fullPathDesfileTemp = fullPathSourceFile.Replace(".zip", "_temp");
            ZipFile.ExtractToDirectory(fullPathSourceFile, fullPathDesfileTemp);
            string[] filePaths = Directory.GetFiles(fullPathDesfileTemp);
            foreach (var item in filePaths)
            {
                string result = Path.GetFileName(item);
                File.Copy(item, Path.Combine(fullPathDesfile, result), true);
                logger.UserAction("Phuongdd", "CopyFile", string.Format("File Copy {0} File des {1}", item, Path.Combine(fullPathDesfile, result)));
            }
        }

        private static void CreateFolder(string fullPathFile)
        {
            if (!File.Exists(fullPathFile))
            {
                Directory.CreateDirectory(fullPathFile);
            }
        }

        private static string ZipFIle(string fullPathFile)
        {
            string zipFile = string.Format("{0}.zip", fullPathFile);
            ZipFile.CreateFromDirectory(fullPathFile, zipFile);
            Directory.Delete(fullPathFile, true);
            return zipFile;
        }

        public static string VerifyPdfFile(string fullPathFile)
        {
            PdfReader reader = new PdfReader(fullPathFile);
            AcroFields af = reader.AcroFields;
            List<string> names = af.GetSignatureNames();
            StringBuilder signBy = new StringBuilder();
            for (int i = 0; i < names.Count; ++i)
            {
                String name = names[i];
                PdfPKCS7 pk = af.VerifySignature(name);
                signBy.AppendFormat("Hóa đơn xác thực bởi: {0}", GetInforeCompany(pk.SigningCertificate.SubjectDN.ToString(), keyGetCompany).ToLower());
                signBy.AppendFormat(", Ngày ký :{0}", pk.SignDate.ToString("dd-MM-yyyy"));
            }

            if (signBy.Length == 0)
            {
                return "Hóa đơn chưa được xác thực";
            }

            return signBy.ToString();
        }

        private static string GetInforeCompany(string subJect, string key)
        {
            string companyName = string.Empty;
            foreach (var str in subJect.ConvertToList(','))
            {
                string item = str.Trim();
                if (item.Trim().Contains(key))
                {
                    companyName = item.Replace(key, "");
                    break;
                }
            }

            return companyName;
        }


    }
}
