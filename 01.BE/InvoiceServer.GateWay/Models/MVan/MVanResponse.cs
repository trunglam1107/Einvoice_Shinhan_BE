using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.GateWay.Models.MVan
{
    public class MVanResponse
    {
        public string Mst { get; set; }
        public string MstNnt { get; set; }
        public string Xml { get; set; }
        public long CompanyId { get; set; }
        public string FolderStore { get; set; }
    }
    public class MVanResult
    {
        public string code { get; set; }
        public string message { get; set; }
        public string data { get; set; }
    }
    public class MVanSynthesisResult
    {
        public string Error { get; set; }
        public string ErrorCode { get; set; }
        public string KHHDon { get; set; }
        public string KHMSHDon { get; set; }
        public string SHDon { get; set; }
    }
}
