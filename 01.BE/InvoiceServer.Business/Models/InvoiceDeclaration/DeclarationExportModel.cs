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
    public class DeclarationExportModel
    {
        public DeclarationExportPdfInfo DataExport { get; set; }
        public string TemplateFile { get; set; }
        public string DetailTemplateFile { get; set; }

        public string FilePath { get; set; }
    }
}



