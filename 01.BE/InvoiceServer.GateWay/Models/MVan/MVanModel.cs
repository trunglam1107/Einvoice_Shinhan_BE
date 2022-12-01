using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.GateWay.Models.MVan
{
    class VanModel
    {
    }
    public class VanSignUpRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Sdt { get; set; }
        public string Ma_dvcs { get; set; }
        public string Ten_nguoi_sd { get; set; }
        public string Dien_giai { get; set; }
    }

    public class VanLoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Ma_dvcs { get; set; }
        public string UserId { get; set; }
    }

    public class VanDefaulRequest
    {
        //public VanLoginRequest LoginInfo { get; set; }
        public string XmlData { get; set; }
        public string MstNnt { get; set; }
        public string MstTcgp { get; set; }
        public long InvoiceId { get; set; }
        public long HistoryReportId { get; set; }
        public long DeclarationId { get; set; }
        public long InvoiceNotiType { get; set; }
        public List<long> InvoiceIds { get; set; }
    }

    public class VanResponseRequest
    {
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string Mltdiep { get; set; }
        public string Mst { get; set; }
        public string Thop { get; set; }
        public string Mtdiep { get; set; }
        public string Mngui { get; set; }
        public string Ttxncqt { get; set; }
        public string Type { get; set; }
        public string Mtdtchieu { get; set; }
        public string Mnnhan { get; set; }
        public string Ltbao { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }

    public class VanResponseData
    {
        public string Id { get; set; }
        public string Pban { get; set; }
        public string Mngui { get; set; }
        public string Mnnhan { get; set; }
        public string Mltdiep { get; set; }
        public string Mtdiep { get; set; }
        public string Mtdtchieu { get; set; }
        public string Mst { get; set; }
        public string Mgddtu { get; set; }
        public string Datenew { get; set; }
        public string Xml { get; set; }
        public string Fileid { get; set; }
        public string Type { get; set; }
        public string Tttnhan { get; set; }
        public string Msgtimestamp { get; set; }
        public string Offset { get; set; }
        public string Ltbao { get; set; }
        public string Thop { get; set; }
        public string Ttxncqt { get; set; }
        public string Mtdiep999 { get; set; }
        public string Msttcgp { get; set; }
    }

    public class VanSignUpResult
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public SignUpData Data { get; set; }
    }

    public class VanResponseResult
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public List<VanResponseData> Data { get; set; }
    }

    public class SignUpData
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Wb_nhomquyen_id { get; set; }
        public string Dien_giai { get; set; }
        public string Email { get; set; }
        public string Chucvu { get; set; }
        public string Sdt { get; set; }
        public string Isviewuser { get; set; }
        public string Isedituser { get; set; }
        public string Isdeluser { get; set; }
        public string Issigninvoice { get; set; }
        public string IsLock { get; set; }
        public string TimeLock { get; set; }
        public string AttempLogin { get; set; }
        public string Ten_nguoi_sd { get; set; }
        public string Ma_dvcs { get; set; }
    }

    public class VanLoginResult
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
    }

    public class VanDefaulResult
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public DefaulData Data { get; set; }
    }

    public class DefaulData
    {
        public string MaThongdiep { get; set; }
    }
}