using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System.IO;

namespace InvoiceServer.Business.Models
{
    public class PrintConfig
    {
        public string FullFolderAssetOfCompany { get; set; }
        public string FullPathFolderExportInvoice { get; set; }
        public string FullPathFileNameExport { get; set; }
        public string FullPathLogo { get; private set; }
        public string FullPathFileAsset { get; private set; }
        public string FullPathFileExportData { get; private set; }
        public string FullPathFileReleaseExport { get; private set; }
        public string FullPathFolderTemplateInvoice { get; set; }
        public string FullPathFileTemplateInvoice { get; set; }
        public string FullPathFileTemplateStatistical { get; set; }
        public string FullPathFileTemplateInvoiceGift { get; set; }
        public string FullPathInvoiceFileOfCompany { get; set; }
        public string PathInvoiceFile { get; set; }
        public string FolderExportInvoice { get; set; }
        public string FomatNumber { get; private set; }
        public string FullPathFolderExportDeclaration { get; set; }
        public string FullPathFolderTemplateDeclaration { get; set; }

        public PrintConfig(string fullPathFileAsset, string fullPathInvoiceFile = null, string fullPathFileExportData = null)
        {
            FullPathFileAsset = fullPathFileAsset;
            PathInvoiceFile = fullPathInvoiceFile;
            FullPathFileExportData = fullPathFileExportData;
        }
        public void BuildAssetByCompany(CompanyInfo company)
        {
            if (company == null)
            {
                return;
            }

            FullFolderAssetOfCompany = Path.Combine(FullPathFileAsset, company.Id.ToString());
            FullPathFolderExportInvoice = Path.Combine(FullFolderAssetOfCompany, AssetSignInvoice.Invoice);
            FullPathFileReleaseExport = Path.Combine(FullFolderAssetOfCompany, AssetSignInvoice.Release);
            FullPathInvoiceFileOfCompany = Path.Combine(PathInvoiceFile ?? "", company.Id.ToString());
            FolderExportInvoice = Path.Combine(FullPathInvoiceFileOfCompany, AssetSignInvoice.Invoice);
            FomatNumber = company.NumberFormat.IsNullOrEmpty() ? "{0:#,##0.###}" : company.NumberFormat;
            if (!company.Logo.IsNullOrEmpty())
            {
                FullPathLogo = Path.Combine(FullFolderAssetOfCompany, company.Logo);
            }
            else
            {
                FullPathLogo = string.Empty;
            }

        }

    }
}
