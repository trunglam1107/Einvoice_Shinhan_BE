using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class DeclarationExportPdfInfo
    {
        public string DeclarationTypeID { get; set; }
        public string TaxPayerName { get; set; }
        public string TaxCode { get; set; }
        public string TaxDepartmentName { get; set; }
        public string PeronalContact { get; set; }
        public string Tel1 { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

        public string HTHDon { get; set; }
        public string HTGDLHDDT { get; set; } 
        public string PThuc { get; set; } 
        public string LHDSDung { get; set; } 

        public List<DeclarationExportPdfDetail> Details { get; set; }
    }

    public class DeclarationExportPdfDetail
    {
        public string STT { get; set; }
        public string ReleaseConpanyName { get; set; }
        public string Seri { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string RegisterType { get; set; }
        public string RegisterTypeID { get; set; }
    }
}



