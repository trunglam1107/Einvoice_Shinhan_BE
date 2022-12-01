using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;

namespace InvoiceServer.Common.Constants
{
    public static class DefaultFields
    {
        public const string ADMIN_USER_ID = "admin";
        public const string CURRENCY_VND = "VND";
        public const string INVOICE_NO_DEFAULT_VALUE = "00000000";
        public static string BE_FOLDER { get; private set; } = "";
        public static string ASSET_FOLDER { get; private set; } = "";
        public static string LOGO_FILE { get; private set; } = "";
        public static string LOGO_MAIL_FILE { get; private set; } = "";
        public static string FILE_RECEPT_FOLDER { get; private set; } = "";
        public static string EXPORT_TEMPLATE_FOLDER { set; get; } = "";
        public static string DECIMAL_SEPARATOR { get; set; } = ".";
        public static string EXPORT_DATA_FOLDER { set; get; } = "";
        public static string IVOICE_SERVER_API_PATH { get; private set; } = "";
        public const int INVOICECODE = 1;
        public const string InvoiceName = "Hóa đơn Giá trị gia tăng";
        public static string NAME_FILE_REPORT_MONTHLY = "Bảng tổng hợp dữ liệu hóa đơn điện tử";
        public static string INVOICE_NAME = "Hóa đơn Giá trị gia tăng";
        public static string GET_TEMPLATE_EXCEL_FOLDER { get; private set; } = "";

        public static string SAVE_EXCEL_FOLDER { get; private set; } = "";



        static DefaultFields()
        {
            BE_FOLDER = HttpContext.Current.Server.MapPath("~");
            ASSET_FOLDER = HttpContext.Current.Server.MapPath(CommonUtil.GetConfig("FolderAssetOfCompany", "~/Data/Asset"));
            GET_TEMPLATE_EXCEL_FOLDER = HttpContext.Current.Server.MapPath(CommonUtil.GetConfig("FolderExportTemplateOfCompany", "~/Data/ExportData"));
            SAVE_EXCEL_FOLDER = HttpContext.Current.Server.MapPath(CommonUtil.GetConfig("FolderSaveReportOfCompany", "~/Data/ExportData/Reports"));
            LOGO_FILE = HttpContext.Current.Server.MapPath(CommonUtil.GetConfig("LogoSrc"));
            LOGO_MAIL_FILE = HttpContext.Current.Server.MapPath(CommonUtil.GetConfig("MailImageLocalPath", CommonUtil.GetConfig("LogoSrc")));
            FILE_RECEPT_FOLDER = System.IO.Path.Combine(CommonUtil.GetConfig("FolderInvoiceFile"), "FileRecipt");
            EXPORT_TEMPLATE_FOLDER = HttpContext.Current.Server.MapPath(CommonUtil.GetConfig("ExportTemplateFilePath", "~/Data/ExportTemplate"));
            DECIMAL_SEPARATOR = CommonUtil.GetConfig("DecimalSeparator", ".");
            EXPORT_DATA_FOLDER = CommonUtil.GetConfig("FolderInvoiceFile");
            IVOICE_SERVER_API_PATH = HttpContext.Current.Server.MapPath("~/");
        }
    }

    public static class BIDCDefaultFields
    {
        public const string CURRENT_CLIENT = "VANGLAI"; // Khách hàng vãng lai
    }

    public static class GateWayType
    {
        public const int SendInvoiceWithCode = 1;
        public const int SendInvoiceNotCode = 2;
        public const int VanRegister = 3;
        public const int VanCancel = 4;
        public const int VanSynthesis = 5;
        public const int VanDelete = 6;
        public const int VanAdjustment = 7;
    }

    public static class InvoiceNotiType
    {
        public const int VanCancel = 1;
        public const int VanDelete = 2;
        public const int VanAdjustment = 3;
    }

    public static class InvoiceSystemsettingKey
    {
        public const string MVAN = "MVAN";
        public const string MVAN_URL = "MVAN_URL";
        public const string MVAN_INVOICENOTCODE = "MVAN_INVOICENOTCODE";
        public const string MVAN_INVOICEWITHCODE = "MVAN_INVOICEWITHCODE";
        public const string MVAN_CANCEL = "MVAN_CANCEL";
        public const string MVAN_LOGIN = "MVAN_LOGIN";
        public const string MVAN_REGISTER = "MVAN_REGISTER";
        public const string MVAN_SIGNUP = "MVAN_SIGNUP";
        public const string MVAN_SYNTHESIS = "MVAN_SYNTHESIS";
        public const string MVAN_MSTNNT = "MVAN_MSTNNT";
        public const string MVAN_MDVCS = "MVAN_MDVCS";
        public const string MVAN_MSTTCGP = "MVAN_MSTTCGP";
        public const string MVAN_ADMINURL = "MVAN_ADMINURL";
        public const string MVAN_SEARCH = "MVAN_SEARCH";
        public const string MVAN_ADMINUSERID = "MVAN_ADMINUSERID";
        public const string MVAN_ADMINPASSWORD = "MVAN_ADMINPASSWORD";
        public const string MVAN_ADMINLOGIN = "MVAN_ADMINLOGIN";
        public const string MVAN_USERNAME = "MVAN_USERNAME";
        public const string MVAN_PASSWORD = "MVAN_PASSWORD";
    }

    public static class CustomHttpRequestHeader
    {
        public const string AuthorizationToken = "X-Authorization-Token";
        public const string CollectionTotal = "X-Collection-Total";
        public const string CollectionSkip = "X-Collection-Skip";
        public const string CollectionTake = "X-Collection-Take";
        public const string AccessControlExposeHeadersContent = "X-Collection-Total, X-Collection-Skip, X-Collection-Take";
        public const string AccessControlExposeHeaders = "Access-Control-Expose-Headers";
    }
    public static class AES
    {
        public const string Key = "01234567891011121314151617181920";
        public const string IV = "0123456789101112";
    }
    public static class ApiSignInvoice
    {
        public const string ActionServerSign = "serverSign";
        public const string ActionSignInvoice = "signInvoice";
        public const string ActionGetSlot = "getSlot";
        public const string ActionGetSerialBySlot = "getSerialBySlot";
        public const string ActionSignInvoiceMultipe = "signNow";
        public const string ActionSignInvoiceMultipeTestHSM = "signTestConnectionHSM";
        public const string token = "147852369";
        public const string ActionServerSignXml = "signXmlCommon";
        public const string ActionGetCertBySlot = "getCertBySlot";
        public const string ActionGetCertInfo = "getInfoCert";
    }

    public static class GateWayLogName
    {
        public const string SendInvoiceNotCode = "SendInvoiceNotCode";
        public const string SendInvoiceWithCode = "SendInvoiceWithCode";
        public const string VanCancel = "VanCancel";
        public const string VanDelete = "VanDelete";
        public const string VanAdjustment = "VanAdjustment";
        public const string VanLogin = "VanLogin";
        public const string VanRegister = "VanRegister";
        public const string VanSignUpAC = "VanSignUpAC";
        public const string VanSynthesis = "VanSynthesis";
        public const string VanReceived = "VanReceived";
        public const string VanAdminLogin = "VanAdminLogin";
        public const string ObjDefaultName = "response";
    }
    public static class ApiSignAnnouncement
    {
        public const string ActionServerSign = "serverSignAnnoun";
        public const string ActionSignAnnoun = "signAnnoun";
        public const string token = "147852369";
    }

    public static class FileUpload
    {
        public const string FileExtension = ".zip";
    }

    public static class Characters
    {
        public const string Slash = @"/";
        public const string Underscore = "_";
    }

    public static class HTHDon
    {
        public const int CoMa = 1;
        public const int KhongCoMa = 2;
        public static Dictionary<int?, string> ListHTHDon { set; get; } = new Dictionary<int?, string>()
        {
            { 1, "C"},
            { 2, "K"},
        };
    }
    public static class HTGDLHDDT
    {
        public const int NNTDBKKhan = 1;
        public const int NNTKTDNUBND = 2;
        public const int CDLTTDCQT = 3;
        public const int CDLQTCTN = 4;
    }
    public static class PThuc
    {
        public const int CDDu = 1;
        public const int CBTHop = 2;
    }
    public static class LHDSDung
    {
        public const int HDGTGT = 1;
        public const int HDBHang = 2;
        public const int HDBTSCong = 3;
        public const int HDBHDTQGia = 4;
        public const int HDKhac = 5;
        public const int CTu = 6;
    }
    public static class DeclarationStatus
    {
        public const int New = 1;
        public const int Approve = 2;
        public const int Waiting = 3;
        public const int Signed = 4;
        public const int Completed = 5;
    }
    public static class Formatter
    {
        public const string DateTimeFormat = "yyyyMMddHHmmss";
    }

    public static class FilterPattern
    {
        public const string AllValues = "*";
    }

    public static class ContentType
    {
        public const string Original = "original";
        public const string User = "user";
    }

    public static class UserPermission
    {
        //R: Read
        //C: Create
        //U: Update
        //D: Delete
        //P: Approval
        //S: Sign

        //Quản lý công ty 
        public const string CompanyManagement_Read = "companyManagement_R";
        public const string CompanyManagement_Create = "companyManagement_C";
        public const string CompanyManagement_Update = "companyManagement_U";
        public const string CompanyManagement_Delete = "companyManagement_D";

        //Số tự tăng
        public const string AutoNumberManagement_Read = "autoNumberSetting_R";
        public const string AutoNumberManagement_Create = "autoNumberSetting_C";
        public const string AutoNumberManagement_Update = "autoNumberSetting_U";
        public const string AutoNumberManagement_Delete = "autoNumberSetting_D";

        //Đăng ký mẫu
        public const string RegisterTemplate_Read = "registerTemplate_R";
        public const string RegisterTemplate_Create = "registerTemplate_C";
        public const string RegisterTemplate_Update = "registerTemplate_U";
        public const string RegisterTemplate_Delete = "registerTemplate_D";

        //Đăng ký sử dụng hóa đơn
        public const string InvoiceRegister_Read = "invoiceRegister_R";
        public const string InvoiceRegister_Create = "invoiceRegister_C";
        public const string InvoiceRegister_Update = "invoiceRegister_U";
        public const string InvoiceRegister_Delete = "invoiceRegister_D";
        public const string InvoiceRegister_Sign = "invoiceRegister_S";
        public const string InvoiceRegister_Approve = "invoiceRegister_P";

        //Quyết định phát hành
        public const string InvoiceRelease_Read = "invoiceRelease_R";
        public const string InvoiceRelease_Create = "invoiceRelease_C";
        public const string InvoiceRelease_Update = "invoiceRelease_U";
        public const string InvoiceRelease_Delete = "invoiceRelease_D";
        public const string InvoiceRelease_Approve = "invoiceRelease_P";

        //Thông báo phát hành
        public const string InvoiceNotification_Read = "invoiceNotification_R";
        public const string InvoiceNotification_Create = "invoiceNotification_C";
        public const string InvoiceNotification_Update = "invoiceNotification_U";
        public const string InvoiceNotification_Delete = "invoiceNotification_D";
        public const string InvoiceNotification_Approve = "invoiceNotification_P";

        //Lập hóa đơn
        public const string CreateInvoice_Read = "createInvoice_R";
        public const string CreateInvoice_Create = "createInvoice_C";

        //Duyệt hóa đơn
        public const string InvoiceCreated_Read = "invoiceCreated_R";
        public const string InvoiceCreated_Approve = "invoiceCreated_P";

        //Ký hóa đơn
        public const string InvoiceApproved_Read = "invoiceApproved_R";
        public const string InvoiceApproved_Sign = "invoiceApproved_S";

        //Ký hóa đơn theo lô
        public const string BundleRelease_Read = "bundleRelease_R";
        public const string BundleRelease_Sign = "bundleRelease_S";

        //Thanh toán hóa đơn
        public const string InvoiceCheckout_Read = "invoiceCheckout_R";
        public const string InvoiceCheckout_Create = "invoiceCheckout_C";

        //Lập biên bản
        public const string AnnouncementCreated_Read = "announcementCreated_R";
        public const string AnnouncementCreated_Create = "announcementCreated_C";
        public const string AnnouncementCreated_Update = "announcementCreated_U";
        public const string AnnouncementCreated_Approve = "announcementCreated_P";

        //Điểu chỉnh hóa đơn
        public const string InvoiceAdjustment_Read = "invoiceAdjustment_R";
        public const string InvoiceAdjustment_Create = "invoiceAdjustment_C";

        //Thay thế hóa đơn
        public const string SubstituteInvoice_Read = "substituteInvoice_R";
        public const string SubstituteInvoice_Create = "substituteInvoice_C";

        //Hủy hóa đơn
        public const string InvoiceCancel_Read = "invoiceCancel_R";
        public const string InvoiceCancel_Delete = "invoiceCancel_D";

        //Lập báo cáo hủy
        public const string ReportCancelling_Read = "reportCancelling_R";
        public const string ReportCancelling_Create = "reportCancelling_C";
        public const string ReportCancelling_Approve = "reportCancelling_P";
        public const string ReportCancelling_Update = "reportCancelling_U";
        public const string ReportCancelling_Delete = "reportCancelling_D";

        //Danh sách hóa đơn
        public const string InvoiceManagement_Read = "invoiceManagement_R";
        public const string InvoiceManagement_Create = "invoiceManagement_C";
        public const string InvoiceManagement_Update = "invoiceManagement_U";
        public const string InvoiceManagement_Delete = "invoiceManagement_D";
        public const string InvoiceManagement_Approve = "invoiceManagement_P";
        public const string InvoiceManagement_Sign = "invoiceManagement_S";

        //Upload hóa đơn
        public const string UploadInvoice_Read = "uploadInvoice_R";

        public const string UploadInvoice_Create = "uploadInvoice_C";

        //Danh sách biên bản
        public const string AnnouncementManagement_Read = "announcementManagement_R";
        public const string AnnouncementManagement_Create = "announcementManagement_C";
        public const string AnnouncementManagement_Update = "announcementManagement_U";
        public const string AnnouncementManagement_Delete = "announcementManagement_D";
        public const string AnnouncementManagement_Approve = "announcementManagement_P";
        public const string AnnouncementManagement_Sign = "announcementManagement_S";

        //Danh sách hóa đơn điều chỉnh
        public const string AdjustedInvoice_Read = "adjustedInvoice_R";

        //Danh sách hóa đơn thay thế
        public const string InvoiceReplacement_Read = "invoiceReplacement_R";

        //Danh sách hóa đơn đã hủy
        public const string CanceledInvoice_Read = "canceledInvoice_R";

        //Danh sách hóa đơn chuyển đổi
        public const string ConvertedInvoice_Read = "convertedInvoice_R";

        //Tình hình sử dụng hóa đơn tháng
        public const string ReportSituationOneMonth_Read = "reportSituationOneMonth_R";

        //Tình hình sử dụng hóa đơn quý
        public const string ReportSituationThreeMonths_Read = "reportSituationThreeMonths_R";

        //Bảng kê hóa đơn tháng
        public const string ReportStatisticOneMonth_Read = "reportStatisticOneMonth_R";

        //Bảng kê hóa đơn quý
        public const string ReportStatisticThreeMonths_Read = "reportStatisticThreeMonths_R";

        //Thống kê hóa đơn chuyển đổi
        public const string ReportInvoiceConverted_Read = "reportInvoiceConverted_R";

        //Thống kê chi tiết hóa đơn
        public const string ReportInvoiceDetail_Read = "reportInvoiceDetail_R";

        //Báo cáo công nợ phải thu
        public const string ReportInvoiceDebt_Read = "reportInvoiceDebt_R";

        //Báo cáo tích hợp
        public const string ReportInvoiceCombine_Read = "reportInvoiceCombine_R";

        //Báo cáo tổng hợp dữ liệu hóa đơn
        public const string ReportGeneralInvoice_Read = "reportGeneralInvoice_R";

        //Quản lý khách hàng
        public const string ClientManagement_Read = "clientManagement_R";
        public const string ClientManagement_Create = "clientManagement_C";
        public const string ClientManagement_Update = "clientManagement_U";
        public const string ClientManagement_Delete = "clientManagement_D";

        //Upload khách hàng
        public const string UploadClient_Read = "uploadClient_R";
        public const string UploadClient_Create = "uploadClient_C";

        //Hàng hóa, dịch vụ
        public const string ProductManagement_Read = "productManagement_R";
        public const string ProductManagement_Create = "productManagement_C";
        public const string ProductManagement_Update = "productManagement_U";
        public const string ProductManagement_Delete = "productManagement_D";

        //Upload hàng hóa, dịch vụ
        public const string UploadProduct_Read = "uploadProduct_R";
        public const string UploadProduct_Create = "uploadProduct_C";

        //Đơn vị tính
        public const string UnitManagement_Read = "unitManagement_R";
        public const string UnitManagement_Create = "unitManagement_C";
        public const string UnitManagement_Update = "unitManagement_U";
        public const string UnitManagement_Delete = "unitManagement_D";

        //Tiền tệ
        public const string CurrencyManagement_Read = "currencyManagement_R";
        public const string CurrencyManagement_Create = "currencyManagement_C";
        public const string CurrencyManagement_Update = "currencyManagement_U";
        public const string CurrencyManagement_Delete = "currencyManagement_D";

        //Loại hóa đơn
        public const string InvoiceTypeManagement_Read = "invoiceTypeManagement_R";
        public const string InvoiceTypeManagement_Create = "invoiceTypeManagement_C";
        public const string InvoiceTypeManagement_Update = "invoiceTypeManagement_U";
        public const string InvoiceTypeManagement_Delete = "invoiceTypeManagement_D";

        //Mẫu hóa đơn
        public const string InvoiceSampleManagement_Read = "invoiceSampleManagement_R";
        public const string InvoiceSampleManagement_Create = "invoiceSampleManagement_C";
        public const string InvoiceSampleManagement_Update = "invoiceSampleManagement_U";
        public const string InvoiceSampleManagement_Delete = "invoiceSampleManagement_D";

        //Download
        public const string DownloadSoft_Read = "downloadSoft_R";

        //Email thông báo
        public const string EmailActiveManagement_Read = "emailActiveManagement_R";
        public const string EmailActiveManagement_Update = "emailActiveManagement_U";

        //Cấu hình Email server
        public const string EmailServerManagement_Read = "emailServerManagement_R";
        public const string EmailServerManagement_Update = "emailServerManagement_U";

        //Thông tin đơn vị
        public const string DivisionInformation_Read = "divisionInformation_R";
        public const string DivisionInformation_Update = "divisionInformation_U";

        //Thiết lập hệ thống
        public const string SystemSetting_Read = "systemSetting_R";
        public const string SystemSetting_Update = "systemSetting_U";

        //Nhật kí hệ thống
        public const string SystemLog_Read = "systemLog_R";
        //File nhật kí hệ thống
        public const string FileSystemLog_Read = "fileSystemLog_R";
        //Nhật ký TVan
        public const string SystemLogTVan_Read = "systemLogTVAN_R";

        //Quản lý người dùng
        public const string AccountManagement_Read = "accountManagement_R";
        public const string AccountManagement_Create = "accountManagement_C";
        public const string AccountManagement_Update = "accountManagement_U";
        public const string AccountManagement_Delete = "accountManagement_D";

        //Quản lý quyền
        public const string RoleManagement_Read = "roleManagement_R";
        public const string RoleManagement_Create = "roleManagement_C";
        public const string RoleManagement_Update = "roleManagement_U";
        public const string RoleManagement_Delete = "roleManagement_D";

        //Quản lý nhân viên
        public const string EmployeesManagement_Read = "employeesManagement_R";
        public const string EmployeesManagement_Create = "employeesManagement_C";
        public const string EmployeesManagement_Update = "employeesManagement_U";
        public const string EmployeesManagement_Delete = "employeesManagement_D";

        //Quản lý đại lý
        public const string AgenciesManagement_Read = "agenciesManagement_R";
        public const string AgenciesManagement_Create = "agenciesManagement_C";
        public const string AgenciesManagement_Update = "agenciesManagement_U";
        public const string AgenciesManagement_Delete = "agenciesManagement_D";

        //Quản lý khách hàng
        public const string CustomerManagement_Read = "customerManagement_R";
        public const string CustomerManagement_Create = "customerManagement_C";
        public const string CustomerManagement_Delete = "customerManagement_D";
        public const string CustomerManagement_Update = "customerManagement_U";

        //Upload chi nhánh
        public const string UploadCustomer_Create = "uploadCustomer_C";
        public const string uploadCustomer_Read = "uploadCustomer_R";

        //Quản lý hợp đồng
        public const string ContractManagement_Read = "contractManagement_R";
        public const string ContractManagement_Create = "contractManagement_C";
        public const string ContractManagement_Update = "contractManagement_U";
        public const string ContractManagement_Delete = "contractManagement_D";
        public const string ContractManagement_Approve = "contractManagement_P";

        //Báo cáo quản trị
        public const string ReportManagement_Read = "reportManagement_R";

        //Giữ lại hóa đơn
        public const string HoldInvoiceManagement_Read = "holdInvoiceManagement_R";
        public const string HoldInvoiceManagement_Create = "holdInvoiceManagement_C";
        public const string HoldInvoiceManagement_Update = "holdInvoiceManagement_U";
        public const string HoldInvoiceManagement_Delete = "holdInvoiceManagement_D";

        //Quản lý tài khoản khách hàng
        public const string ClientAccountManagement_Read = "clientAccountManagement_R";

        // Quan lý Job
        public const string quarztJobManagement_Read = "quartzJobManagement_R";
        public const string quarztJobManagement_Update = "quartzJobManagement_U";

        //Quản lý chữ ký
        public const string CertificateAuthorityManagement_Read = "certificateAuthorityManagement_R";
        public const string CertificateAuthorityManagement_Create = "certificateAuthorityManagement_C";
        public const string CertificateAuthorityManagement_Delete = "certificateAuthorityManagement_D";
        public const string CertificateAuthorityManagement_Update = "certificateAuthorityManagement_U";


        //Báo cáo dữ liệu hóa đơn
        public const string ReportInvoicesSummary_Read = "reportInvoicesSummary_R";

        //Thiết lập lịch
        public const string WorkingCalendar_Read = "workingCalendar_R";
        public const string WorkingCalendar_Create = "workingCalendar_C";
    }

    public static class MsgApiResponse
    {
        public const string ExecuteSeccessful = "Successful";
        public const string DataInvalid = "Data information is invalid.";
        public const string NotAuthorized = "You are not authorized to use this function";
        public const string ResouceIdNotFound = "Id is not found in data of client";
        public const string DataNotFound = "Data not found";
        public const string HaveNotPermissionData = "Have not permission data";
        public const string CacheNotDataResponse = "Cache not data response";
        public const string FileNotFound = "File Not Found";
        public const string FileUploadNotTheSameType = "File upload chưa đúng định dạng";
    }

    public static class Authentication
    {
        public const string LoginSuccessful = "Login successful.";
        public const string LogoutSuccessful = "Logout successful.";
        public const string TokenInvalid = "Token is invalid";
        public const string TokenEnded = "Token is expired.";
        public const string NotAuthentication = "Token not authentication";
        public const string LoginFaild = "Login failed because username or password not match";
        public const string TokenBanned = "Token  was banned";
    }
    public static class HistoryReportGeneralInfo
    {
        public const string NotPermissionData = "You have not permission to access this gatewaylog";

        public const string OrderByColumnDefault = "ID";

        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
            {"ID","ID"},

        };
    }
    public static class ClientManagementInfo
    {
        public const int ClientNameMaxLength = 150;
        public const int AddressMaxLength = 250;
        public const int TelMaxLength = 25;
        public const int FaxMaxLength = 25;
        public const int ContactPersionMaxLength = 50;
        public const int EmailMaxLength = 50;

        public const string ErrorMsgCheckMaxLength = "Number of characters in the [{0}] exceeded {1} character";

        public const string ClientClientNameIsEmpty = "You have to enter at least a name of client";
        public const string ClientAddressIsEmpty = "You have to enter at least a address of client";
        public const string ClientEmailInvalidFormat = "Format of email is invalid";
        public const string NotPermissionData = "You have not permission to access this client";

        public const string OrderByColumnDefault = "COMPANYNAME";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
            {"COMPANYNAME","COMPANYNAME"},
            {"CUSTOMERCODE","CUSTOMERCODE"},
            {"TAXCODE","TAXCODE"},
            {"ADDRESS","ADDRESS"},
            {"MOBILE","MOBILE"},
            {"EMAIL","EMAIL"},
            {"PERSONCONTACT","PERSONCONTACT"},
            {"CUSTOMERNAME","CUSTOMERNAME"},
            {"RECEIVEDINVOICEEMAIL","RECEIVEDINVOICEEMAIL"},
        };
    }

    public static class AdditionalItemManagementInfo
    {
        public const int CategoryMaxLength = 150;
        public const int ItemNameMaxLength = 150;
        public const int DescriptionMaxLength = 500;

        public const string ErrorMsgCheckMaxLength = "Number of characters in the [{0}] exceeded {1} character";
        public const string SalesPriceMustBeAPositiveNumber = "Sale price must be a positive number";
        public const string NotPermissionData = "You have not permission to access this addtional item";
        public const string ErrItemNameIsEmpty = "You have to enter at least a name of item";
        public const string ErrItemNameIsExists = "Item name is exists";

        public const string OrderByColumnDefault = "ADDITIONALITEMID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
            {"category","CATEGORY"},
            {"englishitemname","ITEMNAME"},
            {"localitemname","ITEMNAMELOCAL"},
            {"englishdescription","DESCRIPTION"},
            {"localdescription","DESCRIPTIONLOCAL"},
            {"SALESPRICE","SALESPRICE"},
            {"DISPLAYITEMNAME","DISPLAYITEMNAME"},
            {"DISPLAYDESCRIPTION","DISPLAYDESCRIPTION"},
            {"UNITNAME","UNITNAME"}
        };
    }

    public static class GatewaylogManagementInfo
    {
        public const string NotPermissionData = "You have not permission to access this gatewaylog";

        public const string OrderByColumnDefault = "ID";

        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
            {"ID","ID"},
            {"NAME","NAME"},
            {"BODY","BODY"},
            {"OBJECTNAME","OBJECTNAME"},
            {"IP","IP"},
            {"CREATEDDATE","CREATEDDATE"},
            {"CREATEDBY","CREATEDBY"},
        };
    }
    public static class HistoryReportGeneralInfoo
    {
        public const string NotPermissionData = "You have not permission to access this gatewaylog";

        public const string OrderByColumnDefault = "ID";

        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
            {"ID","ID"},

        };
    }
    public static class LoginUserInfo
    {
        public const int UserIdMaxLength = 50;
        public const string MsgUserIdMaxLength = "User id exceeds maximum length";
        public const string MsgUserIdIsEmpty = "User name cannot be blank";

        public const int UserNameMaxLength = 50;
        public const string MsgUserNameMaxLength = "User's name exceeds maximum length";
        public const string MsgUserNameIsEmpty = "User name cannot be blank";

        public const int NewPasswordMaxLength = 50;

        public const int CurrentPasswordMaxLength = 50;

        public const int EmailMaxLength = 50;
        public const string MsgEmailMaxLength = "Email address exceeds maximum length";
        public const string MsgEmailIsEmpty = "Email cannot be blank";
        public const string MsgEmailInvalid = "Format of email is invalid";

        public const int PasswordMaxLength = 50;
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>(){
                {"USERID", "USERID"},
                {"EMAIL", "EMAIL"},
                {"USERNAME", "USERNAME"},
                {"STATUS", "ISACTIVE"},
                {"CREATEDDATE", "CREATEDDATE"},
                {"UPDATEDDATE", "UPDATEDDATE"},
                {"LASTACCESSEDTIME", "LASTACCESSEDTIME"},
                {"LASTCHANGEDPASSWORDTIME", "LASTCHANGEDPASSWORDTIME"},
            };

        public const string OrderByColumnDefault = "USERID";
        public static Dictionary<string, string[]> PermissionDelete { get; set; } = new Dictionary<string, string[]>();
    }

    public static class EmailType
    {
        public const string UpdateUserAccount = "UPDATE_USERACCOUNT";
        public const string CreateUserAccount = "CREATE_USERACCOUNT";
        public const string NotificationUserNotLogin = "NOTIFICATION_USERNOTLOGIN";
        public const string RemindDealerAdmin = "REMIND_DEALERADMIN";
        public const string NoticeReleaseInvoice = "Notice_Release_Invoice";

        // Place Holder
        public const string PlaceHolderUsername = "{Username}";
        public const string PlaceHolderUserId = "{UserId}";
        public const string PlaceHolderUrl = "{Url}";
        public const string PlaceHolderEmail = "{Email}";
        public const string PlaceHolderCompanyUrl = "{CompanyUrl}";
        public const string PlaceHolderOldEmail = "{OldEmail}";
        public const string PlaceHolderOldUsername = "{OldUsername}";
        public const string PlaceHolderLastAccessedTime = "{LastAccessedTime}";

        public const string PlaceHolderCompanyName = "$CompanyName";
        public const string PlaceHolderCustomerName = "$CustomerName";
        public const string PlaceHolderVerificationCode = "$VerificationCode";
        public const string PlaceHolderInvoiceNo = "$invNumber";
        public const string PlaceHolderInvoicePattern = "$pattern";
        public const string PlaceHolderInvoiceSeria = "$serial";
        public const string PlaceHolderClientId = "$clientId";
        public const string PlaceHolderLogo = "$logo";
        public const string PlaceHolderBankAddress = "{BankAddress}";
        public const string PlaceHolderBankAddressEn = "{BankAddressEn}";
        public const string PlaceHolderBankName = "{BankName}";
        public const string PlaceHolderBankNameEn = "{BankNameEn}";
        public const string PlaceHolderBankTnhh = "{Tnhh}";
        public const string PlaceHolderBankPhone = "{BankPhone}";

        public const string PlaceHolderInvoiceNoOld = "$NewInvMumber";
        public const string PlaceHolderInvoicePatternOld = "$Newpattern";
        public const string PlaceHolderInvoiceSeriaOld = "$NewSerial";
        public const string PlaceHolderInvoiceLinkDowLoad = "$DestroyRecordUrl";

        public const string PlaceHolderInvoiceDate = "@InvoiceDate";
        public const string PlaceHolderInvoiceTotal = "$Total";
        public const string PlaceHolderInvoiceTotalTax = "$TaxAmount";
        public const string PlaceHolderInvoiceSum = "$Sum";

        public const string PlaceHolderInvoiceDetails = "$details";
        public const string PlaceHolderIsHavePortal = "$isHavePublicPortal";
        public const string PlaceHolderLinkPortalShow = "{linkPortalShow}";
        public const string PlaceHolderLinkPortal = "$linkPortal";

        public const string PlaceHolderFooterCompanyName = "$FooterCompanyName";
        public const string PlaceHolderFooterTaxCode = "$FooterTaxCode";
        public const string PlaceHolderFooterPhone = "$FooterPhone";
        public const string PlaceHolderFooterCompanyLink = "$FooterCompanyLink";
        public const string PlaceHolderFooterCompanyLinkShow = "{FooterCompanyLinkShow}";
        public const string PlaceHolderParaPortal = "{para}";
        public const string PlaceHolderLinkAgencyLogin = "$LinkAgencyLogin";
        public const string PlaceHolderTextPass = "{TextPass}";
        public const string PlaceHolderTextPassEn1 = "{TextPassEn1}";
        public const string PlaceHolderTextPassEn2 = "{TextPassEn2}";
        public const string PlaceHolderValuePass = "{ValuePass}";
        public const string PlaceHolderValuePassEn = "{ValuePassEn}";

        public static string TextPass()
        {
            return "Để xem chi tiết tệp đính kèm, Quý khách vui lòng sử dụng mật khẩu là";
        }
        public static string TextPassEn1()
        {
            return "To open the attachment, Please use";
        }
        public static string TextPassEn2()
        {
            return "as its password.";
        }
        public static string ValuePass(bool isOrg)
        {
            if (isOrg == true)
                return "mã số thuế của doanh nghiệp";
            return "số chứng minh nhân dân của Quý Khách";
        }
        public static string ValuePassEn(bool isOrg)
        {
            if (isOrg == true)
                return "the company’s tax code number";
            return "your identity card number";
        }
    }


    public static class EmailTypeAccountInfo
    {
        public const string PlaceHolderUsername = "$Username";
        public const string PlaceHolderUserId = "$UserId";
        public const string PlaceHolderCompanyName = "$CompanyName";
        public const string PlaceHolderCustomerName = "$CustomerName";
    }

    public static class EmailTypeAnnoun
    {
        public const string PlaceHolderCustomerName = "$customerName";
        public const string PlaceHolderNo = "$minutesNumber";
        public const string PlaceHolderMinutesDate = "@minutesDate";
        public const string PlaceHolderMinutesType = "$minutesType";
        public const string PlaceHolderVerificationCode = "$verificationCode";

        public const string PlaceHolderCompanyName = "$CompanyName";
        public const string PlaceHolderType = "$Type";
        public const string PlaceHolderInvoiceNo = "$invNumber";
        public const string PlaceHolderInvoiceDate = "@InvoiceDate";
        public const string PlaceHolderServiceName = "$ServiceName";
        public const string PlaceHolderClientId = "$clientId";
        public const string PlaceHolderEmail = "{Email}";
        public const string IsVisibleInvoiceChangedNumber = "{IsVisibleInvoiceChangedNumber}";
        public const string AdjustedInvoiceNumber = "{AdjustedInvoiceNumber}";

        public const string Visible = "visible";
        public const string Hidden = "hidden";
    }

    public static class ProductInfoAction
    {
        public const string OrderByColumnDefault = "PRODUCTNAME";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>(){
                {"PRODUCTNAME", "PRODUCTNAME"},
                {"PRODUCTCODE", "PRODUCTCODE"},
                {"UNIT", "UNIT"},
                {"TAXID", "TAXID"},
                {"PRICE", "PRICE"},
            };
    }

    public static class CompanyManagementInfo
    {
        public const int EnglishCompanyNameMaxLength = 150;
        public const string MsgEnglishCompanyNameMaxLength = "English name exceeds maximum length";

        public const int LocalCompanyNameMaxLength = 150;
        public const string MsgLocalCompanyNameMaxLength = "Local name exceeds maximum length";

        public const string MsgCompanyNameCannotBeBlank = "Company name cannot blank";

        public const int EnglishAddressMaxLength = 150;
        public const string MsgEnglishAddressMaxLength = "English address exceeds maximum length";

        public const int LocalAddressMaxLength = 150;
        public const string MsgLocalAddressMaxLength = "Local address exceeds maximum length";

        public const string MsgAddressCannotBeBlank = "Company address cannot blank";

        public const int PhoneNumberMaxLength = 25;
        public const string MsgPhoneNumberMaxLength = "Phone number exceeds maximum length";
        public const string MsgPhoneNumberInvalid = "Phone number is invalid";

        public const int FaxNumberMaxLength = 25;
        public const string MsgFaxNumberMaxLength = "Fax number exceeds maximum length";
        public const string MsgFaxNumberInvalid = "Fax number is invalid";

        public const int HomePageMaxLength = 150;
        public const string MsgHomePageMaxLength = "URL exceeds maximum length";

        public const decimal MinTax = 0;
        public const decimal MaxTax = 99.99M; // Base on database design (Decimal(4,2))
        public const string TaxNotInRange = "Tax must be a positive number and range from 0 to 99.99";

        public const int AdminNameMaxLength = 50;
        public const string MsgAdminNameMaxLength = "Admin's name exceeds maximum length";
        public const string MsgAdminNameCannotBeBlank = "Admin's name cannot be blank";

        public const int AdminEmailMaxLength = 50;
        public const string MsgAdminEmailMaxLength = "Admin's email address exceeds maximum length";
        public const string MsgAdminEmailCannotBeBlank = "Admin's email address cannot be blank";
        public const string MsgAdminEmailInvalid = "Admin's email address is invalid";

        public const string MsgCountryOfUserNotExits = "The Country of user does not exists";
        public const string MsgRoleDoesNotExists = "The Role of User does not exists";
        public const string MsgDealerAdminNotFound = "Cannot get dealer admin account of the Company";
        public const string NotPermissionData = "You have not permission to access this company";

        public const int CompanyNameMaxLength = 150;
        public const string MsgCompanyNameMaxLength = "Company name exceeds maximum length";
    }

    public static class CompanySortColumn
    {
        public const string OrderByColumnDefault = "COMPANYSID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"NAME","COMPANYNAME"},
           {"TAX","TAXCODE"},
           {"EMAIL","EMAIL"},
           {"PERSONCONTACT","PERSONCONTACT"},
           {"TEL","TEL1"},
        };
    }

    public static class UseInvoiceAction
    {
        public const string OrderByColumnDefault = "MYCOMPANY.COMPANYNAME";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>(){
                {"COMPANYNAME", "MYCOMPANY.CompanyName"},
                {"NAMETAXDEPARTMENT", "TAXDEPARTMENT.Name"},
                {"CREATEDDATE", "CREATEDDATE"},
                {"Status", "STATUS"},
            };
    }

    public static class OrderTypeConst
    {
        public const string Asc = "ASC";
        public const string Desc = "DESC";
        public static Dictionary<string, string> DcType { get; set; } = new Dictionary<string, string>()
            {
                {"ASC", "ASC"},
                {"DESC", "DESC"},
            };
    }

    // Đổi bussiness cho ngân hàng, chỉ còn 2 mục
    public static class RoleInfo
    {
        public const string BRANCH = "BRANCH";
        public const string CLIENT = "CLIENT";
        public static Dictionary<string, int> Id { get; set; } = new Dictionary<string, int>()
        {
            { BRANCH, 1 },
            { CLIENT, 2 },
        };
    }


    public enum FileTypeUpload
    {
        Logo,
        SignaturePicture
    }

    public static class InvoiceReleasesSortColumn
    {
        public const string OrderByColumnDefault = "CREATEDDATE";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"NO","CODE"},
           {"PERSIONSUGGEST","PERSIONSUGGEST"},
           {"DATERELEASE","RELEASEDDATE"},
           {"STATUS","STATUS"},
        };
    }

    public static class NoticeUseInvoiceSortColumn
    {
        public const string OrderByColumnDefault = "CREATEDDATE";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"COMPANYNAME","MYCOMPANY.COMPANYNAME"},
           {"TAXDEPARTMENT","TAXDEPARTMENT.NAME"},
           {"CREATEDDATE","CREATEDDATE"},
           {"STATUS","STATUS"},
        };
    }

    public static class CacheResponseRegion
    {
        public const string MergeItem = "Merge-Item";
        public const string AppSetting = "App-Setting";
    }

    public enum DownloadFileType
    {
        Excel,
        Pdf
    }

    public static class NotificationManagementInfo
    {
        public const int SubjectMaxLength = 150;
        public const string MsgSubjectIsEmpty = "Subject is required";
        public const string MsgSubjectMaxLength = "Subject exceeds maximum length";

        public const int ContentNameMaxLength = 500;
        public const string MsgContentIsEmpty = "Content is required";
        public const string MsgContentMaxLength = "Content exceeds maximum length";
    }

    public static class ReleaseInvoiceSortColumn
    {
        public const string OrderByColumnDefault = "ID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"CODE","INVOICECODE"},
           {"SYMBOL","INVOICESYMBOL"},
           {"NO","INVOICENO"},
           {"CUSTOMERNAME","CUSTOMERNAME"},
           {"RELEASESTATUS","STATUS"},
           {"DATEINVOICE","INVOICEDATE"},
           {"NOTE","INVOICENOTE"},
        };
    }

    public static class ReleaseListInvoiceSortColumn
    {
        public const string OrderByColumnDefault = "ID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"DESCRIPTION","DESCRIPTION"},
           {"ID","ID"},
           {"DATERELEASE","RELEASEDDATE"},
        };
    }

    public static class InvoiceSortColumn
    {
        public const string OrderByColumnDefault = "ID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"CODE","INVOICECODE"},
           {"SYMBOL","INVOICESYMBOL"},
           {"NO","INVOICENO"},
           {"CUSTOMERNAME","CUSTOMERNAME"},
           {"CUSTOMERCODE","CUSTOMERCODE"},
           {"NUMBERACCOUNT","NUMBERACCOUNT"},
           {"DATEINVOICE","INVOICEDATE"},
           {"STATUS","INVOICESTATUS"},
           {"PAYMENTED","PAYMENTED"},
           {"NOTE","INVOICENOTE"},
           {"TOTAL","TOTAL"},
           {"TOTALTAX","TOTALTAX"},
           {"ID","ID"},
        };
    }
    public static class Announcementsortcolumn
    {
        public const string OrderByColumnDefault = "ID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"DENOMINATOR","DENOMINATOR"},
           {"SYMBOL","SYMBOL"},
           {"INVOICENO","INVOICENO"},
           {"INVOICEDATE","INVOICEDATE"},
           {"ANNOUNCEMENTDATE","ANNOUNCEMENTDATE"},
           {"CLIENTNAME","CLIENTNAME"},
           {"ANNOUNCEMENTSTATUS","ANNOUNCEMENTSTATUS"},
           {"MINUTESNO","MINUTESNO"},
           {"ID","ID"},
           //{"CUSTOMERCODE","CUSTOMERCODE"},
        };
    }
    public static class FirstLoad
    {
        public const string ID = "ID";
    }

    public static class ReportDetailUseSortColumn
    {
        public const string OrderByColumnDefault = "INVOICECODE";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"CODE","INVOICECODE"},
           {"SYMBOL","INVOICESYMBOL"},
           {"NO","INVOICENO"},
           {"CUSTOMERNAME","CUSTOMERNAME"},
           {"CUSTOMERCODE","CUSTOMERCODE"},
           {"DATERELEASE","RELEASEDDATE"},
           {"INVOICETYPE","INVOICETYPE"},
           {"TOTAL","TOTAL"},
           {"DATEINVOICE","CREATED" },
           {"STATUS","INVOICESTATUS" },
           {"TAXCODE","TAXCODE"},
           {"CURRENCY","CURRENCY"},
           {"EXCHANGERATE","CURRENCYEXCHANGERATE"},
           {"TOTALAMOUNT","TOTALAMOUNT"},
           {"TAXVAT","TAX"},
           {"TOTALTAX","AMOUNTTAX"},
           {"SUMMARY","SUM"},
           {"ID","ID"},
           //DETAIL INVOICE SORT
            {"PRODUCTNAME","PRODUCTNAME"},
        };
    }


    public static class ReplacesInvoiceSortColumn
    {
        public const string OrderByColumnDefault = "CODE";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"CODE","CODE"},
           {"CODESUBSTITUTE","CODESUBSTITUTE"},
           {"SYMBOL","SYMBOL"},
           {"SYMBOLSUBSTITUTE","SYMBOLSUBSTITUTE"},
           {"NO","NO"},
           {"NOSUBSTITUTE","NOSUBSTITUTE"},
           {"NOTE","NOTE"},
           {"NOTESUBSTITUTE","NOTESUBSTITUTE"},
        };
    }

    public static class InvoiceCompanyUseSortColumn
    {
        public const string OrderByColumnDefault = "CODE";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"CODE","CODE"},
           {"ADDRESS","NAME"},
        };
    }

    public static class HoldInvoiceSortColumn
    {
        public const string OrderByColumnDefault = "ID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"CODE","CODE"},
           {"SYMBOL","SYMBOL"},
           {"INVOICEDATE","INVOICEDATE"},
        };
    }

    public static class RegisterTemplateUseSortColumn
    {
        public const string OrderByColumnDefault1 = "INVOICETYPEID";
        public const string OrderByColumnDefault2 = "CODE";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"INVOICETYPEID","INVOICETYPEID"},
           {"CODE","CODE"},
           {"NAME","NAME"},
        };
    }

    public static class InvoiceSampleUseSortColumn
    {
        public const string OrderByColumnDefault1 = "INVOICETYPEID";
        public const string OrderByColumnDefault2 = "INVOICETEMPLATESAMPLEID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"INVOICETYPEID","INVOICETYPEID"},
           {"CODE","CODE"},
           {"DENOMINATOR","DENOMINATOR"},
           {"NAME","NAME"},
           {"NOTE","NOTE"},
        };
    }
    public static class SystemLogSort
    {
        public const string OrderByColumnDefault = "LOGDATE";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"FUNCTIONNAME","FUNCTIONNAME"},
           {"LOGSUMMARY","LOGSUMMARY"},
           {"LOGTYPE","LOGTYPE"},
           {"LOGDATE","LOGDATE"},
           {"LOGDETAIL","LOGDETAIL"},
           {"USERNAME","USERNAME"},
        };
    }

    public static class FileSystemLogSort
    {
        public const string OrderByColumnDefault = "MODIFYDATE";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"MODIFYDATE","MODIFYDATE"},
           {"FILENAME","FILENAME"},
           {"FILESIZE","FILESIZE"},
        };
    }

    public static class ImportProduct
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
            {ProductCode},
            {ProductName},
            {Price},
            {UnitCode},
            {Unit},
            {TaxId},
            {Tax},
            {Description},
        };

        public const string ProductCode = "ProductCode";
        public const string ProductName = "ProductName";
        public const string Price = "Price";
        public const string UnitCode = "UnitCode";
        public const string Unit = "Unit";
        public const string TaxId = "TaxId";
        public const string Tax = "Tax";
        public const string Description = "Describe";
        public const string SheetName = "Sheet1";

        public static List<string> SheetNames
        {
            get
            {
                List<string> lst = new List<string>();
                lst.Add(SheetName);
                return lst;
            }
        }
        public const int MaxLength10 = 10;
        public const int MaxLength15 = 15;
        public const int MaxLength50 = 50;
        public const int MaxLength256 = 256;

    }
    public static class MinvoiceStatus
    {
        public const string NotificationApproved = "301";
        public const string DeclarationApproved = "103";
        public const string DeclarationRecieved = "102";
    }
    public static class TTXNCQT
    {
        public const string CQTCNTK = "1";
        public const string CQTKCNTK = "2";
    }

    public static class DetailFile
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
            {STT },
            {HangHoaDichVu },
            {ThanhTien }
        };

        public const string STT = "STT";
        public const string HangHoaDichVu = "Nội dung chi phí";
        public const string ThanhTien = "Số tiền";
        public const string SheetName = "ChiTietHoaDon";
        public static List<string> SheetNames
        {
            get
            {
                List<string> lst = new List<string>();
                lst.Add(SheetName);
                return lst;
            }
        }
    }
    public static class GiftFile
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
            {BranchID},
            {NgayHachToan},
            {SoRef},
            {Sequence},
            {KyThue},
            {TenQuaTang},
            {Remark},
            {DonViTien},
            {TienTruocThue},
            {ThueSuat},
            {ThueGTGT},
            {PhanLoaiQuaTang},
        };

        public const string BranchID = "BranchID";
        public const string NgayHachToan = "NgayHachToan";
        public const string SoRef = "SoRef";
        public const string Sequence = "Sequence";
        public const string KyThue = "KyThue";
        public const string TenQuaTang = "TenQuaTang";
        public const string Remark = "Remark";
        public const string DonViTien = "DonViTien";
        public const string TienTruocThue = "TienTruocThue";
        public const string ThueSuat = "ThueSuat";
        public const string ThueGTGT = "ThueGTGT";
        public const string PhanLoaiQuaTang = "PhanLoaiQuaTang";
        public const string SheetName = "Sheet1";

        public static List<string> SheetNames
        {
            get
            {
                List<string> lst = new List<string>();
                lst.Add(SheetName);
                return lst;
            }
        }
    }
    public static class ImportClient
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
            {CustomerCode},
            {CustomerName},
            {TaxCode},
            {Address},
            {Mobile},
            {Fax},
            {Email},
            {PersonContact},
            {BankName},
            {BankAccount},
            {AccountHolder},
            {CustomerType},
            {Description},
            {TypeReceptionInvoice},
            {Organization},
        };

        public const string CustomerName = "TenKH";
        public const string TaxCode = "MaSoThue";
        public const string CustomerCode = "MaKH";
        public const string Address = "Diachi";
        public const string Mobile = "SoDienThoai";
        public const string Fax = "Fax";
        public const string Email = "Email";
        public const string PersonContact = "Nguoidaidien";
        public const string BankName = "Nganhang";
        public const string BankAccount = "Sotaikhoan";
        public const string AccountHolder = "ChuTaikhoan";
        public const string CustomerType = "KhacHangKetoan";
        public const string Description = "MoTa";
        public const string TypeReceptionInvoice = "Hinhthucnhanhoadon";
        public const string Organization = "KhachHangToChuc";
        public const string SheetName = "Sheet1";
        public static List<string> SheetNames
        {
            get
            {
                List<string> lst = new List<string>();
                lst.Add(SheetName);
                return lst;
            }
        }
        public const int MaxLength25 = 25;
        public const int MaxLength50 = 50;
        public const int MaxLength12 = 12;
        public const int MaxLength256 = 256;

    }
    //Add import Customer
    public static class ImportCustomer
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
            {BrandId },
            {CustomerName},
            {TaxCode},
            {LevelCustomer},
            {Company2},
            {Address},
            {Delegate},
            {Position},
            {Tel},
            {Fax},
            {Email},
            {Website},
            {BankAccount},
            {AccountHolder},
            {BankName},
            {City},
            {TaxDepartment},
            {Active},
            {PersonContact},
            {Mobile},
            {EmailOfContact},
        };
        public const string BrandId = "MaChiNhanh/PGD";
        public const string CustomerName = "ChiNhanh/PGD";
        public const string TaxCode = "MaSoThue";
        public const string LevelCustomer = "CapBac";
        public const string Company2 = "TrucThuoc";
        public const string Address = "DiaChi";
        public const string Delegate = "NguoiDaiDien";
        public const string Position = "ChucVu";
        public const string Tel = "SoDienThoai";
        public const string Fax = "Fax";
        public const string Email = "Email";
        public const string Website = "Website";
        public const string BankAccount = "SoTaiKhoan";
        public const string AccountHolder = "ChuTaikhoan";
        public const string BankName = "NganHang";
        public const string City = "ThanhPho";
        public const string TaxDepartment = "CoQuanThue";
        public const string Active = "TrangThai";
        public const string PersonContact = "NguoiLienHe";
        public const string Mobile = "Mobile";
        public const string EmailOfContact = "EmailLienHe";
        public const string SheetName = "Sheet1";
        public static List<string> SheetNames
        {
            get
            {
                List<string> lst = new List<string>();
                lst.Add(SheetName);
                return lst;
            }
        }
        public const int MaxLength10 = 10;
        public const int MaxLength12 = 12;
        public const int MaxLength25 = 25;
        public const int MaxLength50 = 50;
        public const int MaxLength256 = 256;

    }

    public static class ImportAccount
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
            {STT},
            {Branch},
            {AccountName},
            {Username},
            {Phone},
            {Email},
            {RoleName},
            {ActiveDate},
            {ExpireDate},
            {Status},
        };
        public const string STT = "STT";
        public const string Branch = "ChiNhanh";
        public const string AccountName = "TenNhanVien";
        public const string Username = "TenTaiKhoan";
        public const string Phone = "SoDienThoai";
        public const string Email = "Email";
        public const string RoleName = "TenQuyen";
        public const string ActiveDate = "NgayKichHoat";
        public const string ExpireDate = "NgayHetHieuLuc";
        public const string Status = "TrangThai";
        public const string SheetName = "Sheet1";
        public const string SheetName2 = "Upload USER";
        public static List<string> SheetNames { get; set; } = new List<string>
        {
            {SheetName},
            {SheetName2}
        };
    }

    public static class ImportInvoice
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
            {InvoiceIndentity},
            {Client},
            {CompanyName},
            {ClientTaxCode},
            {ClientAddress},
            {InvoiceDate},
            {Phone},
            {ClientEmail},
            {ClientAccount},
            {TypePayment},
            {PaymentStatus},
            {Decription},
            {InvoiceDetailProductName},
            {InvoiceDetailUnit},
            {InvoiceDetailQuantity},
            {InvoiceDetailPrice},
            {InvoiceDetailTotal},
            {InvoiceTax},
            {CurrencyCode },
            {CurrencyName },
            {CurrencyExchangeRate },
        };

        public const string InvoiceIndentity = "IDChungTu";
        public const string InvoiceisOrg = "ToChuc";
        public const string Client = "HoTenNguoiMua";
        public const string CompanyName = "TenDonVi";
        public const string ClientTaxCode = "MaSoThue";
        public const string ClientAddress = "DiaChi";
        public const string InvoiceDate = "NgayThangNamHD";
        public const string Phone = "DienThoai";
        public const string ClientEmail = "Email";
        public const string ClientBankName = "TenNganHang";
        public const string ClientAccount = "SoTaiKhoan";
        public const string TypePayment = "HinhThucTT";
        public const string PaymentStatus = "TrangThaiTT";
        public const string Decription = "GhiChu";
        public const string InvoiceDetailProductName = "TenHangHoaDichVu";
        public const string InvoiceDetailUnitCode = "MaDonViTinh";
        public const string InvoiceDetailUnit = "DonViTinh";
        public const string InvoiceDetailQuantity = "SoLuong";
        public const string InvoiceDetailPrice = "DonGia";
        public const string InvoiceDetailTotal = "ThanhTien";
        public const string InvoiceTax = "ThueGTGT";
        public const string InvoiceDetailAmountTax = "TienThue";
        public const string DiscountPercent = "CK%";
        public const string DiscountAmount = "TienCK";
        public const string InvoiceIsDiscount = "ChietKhau";
        public const string InvoiceNotMoney = "KhongThuTien";
        public const string CurrencyCode = "MaLoaiTien";
        public const string CurrencyName = "TenLoaiTien";
        public const string CurrencyExchangeRate = "TyGia";
        public const string SheetName = "Sheet1";
        public static List<string> SheetNames { get; set; } = new List<string>
        {
            {SheetName},
        };

        public const int MaxLength25 = 25;
        public const int MaxLength50 = 50;
        public const int MaxLength12 = 12;

    }

    public static class ImportInvoiceNew
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
            {STT },
            {MSCN },
            {ReportClass },
            {InvoiceType },
            {CIFKH },
            {ReleasedDate },
            {CurrencyCode },
            {TyGia },
            {Feeamount },
            {TaxRate },
            {VATAmount },
            {ThanhTien },
            {ProductName }
        };
        public const string STT = "STT";
        public const string MSCN = "Mã số chi nhánh";
        public const string ReportClass = "Report Class";
        public const string InvoiceType = "Loại hóa đơn";
        public const string CIFKH = "CIF Khách hàng";
        public const string ReleasedDate = "Ngày hóa đơn";
        public const string CurrencyCode = "Loại tiền";
        public const string TyGia = "Tỷ giá";
        public const string Feeamount = "Fee amount";
        public const string TaxRate = "Tax rate";
        public const string VATAmount = "VAT amount";
        public const string ThanhTien = "Thành tiền";
        public const string ProductName = "Nội dung hóa đơn";
        public const string SheetName = "Sheet1";
        public static List<string> SheetNames { get; set; } = new List<string>
        {
            {SheetName},
        };

        public const int MaxLength25 = 25;
        public const int MaxLength50 = 50;
        public const int MaxLength12 = 12;


    }

    public static class ImportInvoiceCancel
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
             {STT },
             {MaCN },
             {KyHieu },
             {NgayHoaDon },
             {SoHD },
             {CifKH },
             {NoiDungHuy },
        };

        public const string STT = "STT";
        public const string MaCN = "Mã chi nhánh";
        public const string KyHieu = "Ký hiệu";
        public const string NgayHoaDon = "Ngày hoá đơn";
        public const string SoHD = "Số hoá đơn";
        public const string CifKH = "CIF khách hàng";
        public const string NoiDungHuy = "Nội dung huỷ";
        public const string SheetName = "TempInvoiceCancel";
        public static List<string> SheetNames { get; set; } = new List<string>
        {
            {SheetName},
        };
        
        public const int MaxLength25 = 25;
        public const int MaxLength50 = 50;
        public const int MaxLength12 = 12;

    }

    public static class ImportInvoiceAdjustedImp
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
             {STT },
             {MaCN },
             {KyHieu },
             {NgayHoaDon },
             {SoHD },
             {CifKH },
             {NoiDungDieuChinh },
        };

        public const string STT = "STT";
        public const string MaCN = "Mã chi nhánh";
        public const string KyHieu = "Ký hiệu";
        public const string NgayHoaDon = "Ngày hoá đơn";
        public const string SoHD = "Số hoá đơn";
        public const string CifKH = "CIF khách hàng";
        public const string NoiDungDieuChinh = "Nội dung điều chỉnh";
        public const string SheetName = "TempInvoiceAdjusted";
        public static List<string> SheetNames { get; set; } = new List<string>
        {
            {SheetName},
        };

        public const int MaxLength25 = 25;
        public const int MaxLength50 = 50;
        public const int MaxLength12 = 12;

    }

    public static class ImportInvoiceReplace
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
            {STT },
            {MaCN },
            {ReportClass },
            {KyHieu },
            {LoaiHoaDon },
            {CIFKhachHang },
            {NgayHoaDon },
            {LoaiTien },
            {TyGia },
            {Feeamount },
            {Taxrate },
            {VATAmount },
            {ThanhTien },
            {Noidunghoadon },
            {SoHDDH },
            {NgayHDDH }
        };

        public const string STT = "STT";
        public const string MaCN = "Mã chi nhánh";
        public const string ReportClass = "Report Class";
        public const string KyHieu = "Ký hiệu";
        public const string LoaiHoaDon = "Loại hoá đơn";
        public const string CIFKhachHang = "CIF khách hàng";
        public const string NgayHoaDon = "Ngày hoá đơn";
        public const string LoaiTien = "Loại tiền";
        public const string TyGia = "Tỷ giá";
        public const string Feeamount = "Fee amount";
        public const string Taxrate = "Tax rate";
        public const string VATAmount = "VAT Amount";
        public const string ThanhTien = "Thành tiền";
        public const string Noidunghoadon = "Nội dung hoá đơn";
        public const string SoHDDH = "Số hoá đơn đã huỷ";
        public const string NgayHDDH = "Ngày của hoá đơn đã huỷ";
        public const string SheetName = "TmpInvoiceReplace";

        public static List<string> SheetNames { get; set; } = new List<string>
        {
            {SheetName},
        };

        public const int MaxLength25 = 25;
        public const int MaxLength50 = 50;
        public const int MaxLength12 = 12;
    }

    public static class ImportInvoiceAdjustForInvAdjusted
    {
        public static List<string> ColumnImport { get; set; } = new List<string>
        {
            {STT },
            {MaCN },
            {ReportClass },
            {KyHieu },
            {LoaiHoaDon },
            {CIFKhachHang },
            {NgayHoaDon },
            {LoaiTien },
            {TyGia },
            {Feeamount },
            {Taxrate },
            {VATAmount },
            {ThanhTien },
            {Noidunghoadon },
            {SoHDBDC },
            {NgayHDBDC }
        };

        public const string STT = "STT";
        public const string MaCN = "Mã chi nhánh";
        public const string ReportClass = "Report Class";
        public const string KyHieu = "Ký hiệu";
        public const string LoaiHoaDon = "Loại hoá đơn";
        public const string CIFKhachHang = "CIF khách hàng";
        public const string NgayHoaDon = "Ngày hoá đơn";
        public const string LoaiTien = "Loại tiền";
        public const string TyGia = "Tỷ giá";
        public const string Feeamount = "Fee amount";
        public const string Taxrate = "Tax rate";
        public const string VATAmount = "VAT Amount";
        public const string ThanhTien = "Thành tiền";
        public const string Noidunghoadon = "Nội dung hoá đơn";
        public const string SoHDBDC = "Số hoá đơn bị điều chỉnh";
        public const string NgayHDBDC = "Ngày của hoá đơn bị điều chỉnh";
        public const string SheetName = "TmpInvoiceReplaceForAdjusted";

        public static List<string> SheetNames { get; set; } = new List<string>
        {
            {SheetName},
        };

        public const int MaxLength25 = 25;
        public const int MaxLength50 = 50;
        public const int MaxLength12 = 12;
    }

    public static class InvoiceHeaderInfo
    {
        public const string CompanyName = "{CompanyName}";
        public const string TaxCode = "{TaxCode}";
        public const string Address = "{Address}";
        public const string Mobile = "{Mobile}";
        public const string Tel = "{Tel}";
        public const string SpaceIfNotExistsTel = "{SpaceIfNotExistsTel}";
        public const string Fax = "{Fax}";
        public const string EmailContract = "{EmailContract}";
        public const string Email = "{Email}";
        public const string Website = "{Website}";
        public const string BankAccount = "{BankAccount}";
        public const string BankName = "{BankName}";
        public const string Currency = "{Currency}";
        public const string LogoSrc = "{LogoSrc}";
        public const string ExchangeRate = "{ExchangeRate}";
        public const string PortalSearchInfo = "{PortalSearchInfo}";
        public const string PortalSearchInfoEn = "{PortalSearchInfoEn}";
        public const string FooterCompanyName = "{FooterCompanyName}";
        public const string FooterTaxCode = "{FooterTaxCode}";
        public const string FooterPhone = "{FooterPhone}";
        public const string FooterLink = "{FooterLink}";
        public const string FooterLinkText = "{FooterLinkText}";

        public const string InvoiceNo = "{InvoiceNo}";
        public const string InvoiceCode = "{InvoiceCode}";
        public const string Symbol = "{symbol}";
        public const string CustomerName = "{CustomerName}";
        public const string CustomerCode = "{CustomerCode}";
        public const string CustomerCompanyName = "{CustomerCompanyName}";
        public const string CustomerTaxCode = "{CustomerTaxCode}";
        public const string ReferenceNumber = "{ReferenceNumber}";
        public const string CustomerAddress = "{CustomerAddress}";
        public const string CustomerBankAccount = "{CustomerBankAccount}";
        public const string TypePayment = "{TypePayment}";
        public const string TypePaymentCode = "{TypePaymentCode}";
        public const string DayInvoice = "{Day}";
        public const string MonthInvoice = "{Month}";
        public const string YearInvoice = "{Year}";
        public const string AmountInwords = "{AmountInwords}";
        public const string TaxOfInvoice = "{TaxOfInvoice}";
        public const string Total = "{Total}";
        public const string AmountTax = "{AmountTax}";
        public const string AmountTax5 = "{AmountTax5}";
        public const string AmountTax10 = "{AmountTax10}";
        public const string AmountDiscount = "{AmountDiscount}";
        public const string displayDiscount = "{displayDiscount}";
        public const string TotalDiscountTax = "{TotalDiscountTax}";
        public const string AmountTotal = "{AmountTotal}";
        public const string NoticeChangeInVoice = "{NoticeChangeInVoice}";
        public const string ReportWebsite = "{ReportWebsite}";
        public const string ReportTel = "{ReportTel}";
        public const string CurrentPage = "{CurrentPage}";
        public const string isUpDowTotal = "{isUpDowTotal}";
        public const string isUpDowTax = "{isUpDowTax}";
        public const string isUpDowSum = "{isUpDowSum}";
        public const string isUpDowTotalBilingual = "{isUpDowTotalBilingual}";
        public const string isUpDowTaxBilingual = "{isUpDowTaxBilingual}";
        public const string isUpDowTax5Bilingual = "{isUpDowTax5Bilingual}";
        public const string isUpDowTax10Bilingual = "{isUpDowTax10Bilingual}";
        public const string isUpDowSumBilingual = "{isUpDowSumBilingual}";
        public const string CompanyUrl = "{CompanyUrl}";
        public const string IsShowSignDraft = "{IsShowSignDraft}";
        public const string CompanyNameSignDraft = "{CompanyNameSignDraft}";
        public const string ImgSignDraft = "{ImgSignDraft}";
        public const string InvoiceHaveCode = "{InvoiceHaveCode}";
        public const string SignDate = " {SignDate}";
        public const string ShowSign = "{showSign}";
        //Hua Nan, Mega
        public const string CompanyNameCn = "{CompanyNameCn}";
        public const string CompanyBranchCn = "{CompanyBranchCn}";
        public const string CompanyNameVn = "{CompanyNameVn}";
        public const string CompanyBranchVn = "{CompanyBranchVn}";
        public const string CompanyAddressRow1 = "{CompanyAddressRow1}";
        public const string CompanyAddressRow2 = "{CompanyAddressRow2}";
        public const string DisplayTypeInvoice = "{DisplayTypeInvoice}";
        public const string TypeInvoice = "{ProductNameMonthly}";
    }
    public static class ConfigInvoiceStatistical
    {
        public const string Logo = "{LogoSrc}";
        public const string CompanyName = "{companyName}";
        public const string CompanyAddress = "{companyAddress}";
        public const string CompanyTaxCode = "{companyTaxCode}";
        public const string InvoiceNo = "{InvoiceNo}";
        public const string InvoieDate = "{InvoiceDate}";
        public const string MonthYear = "{MonthYear}";
        public const string EnMonthYear = "{EnMonthYear}";
        public const string CustomerName = "{CustomerName}";
        public const string CustomerTaxCode = "{CustomerTaxCode}";
        public const string CustomerAddress = "{CustomerAddress}";
        public const string Currency = "{Currency}";
        public const string Amount = "{Amount}";
        public const string AmountTax = "{AmountTax}";
        public const string Rate = "{Rate}";
        public const string Total = "{total}";
        public const string TotalTax = "{totalTax}";
        public const string Details = "{details}";
    }
    public static class ConfigInvoiceDetaiInfo
    {
        public const string Identity = "{Identity}";
        public const string ProductName = "{ProductName}";
        public const string Unit = "{Unit}";
        public const string Quantity = "{Quantity}";
        public const string Price = "{Price}";
        public const string Total = "{Total}";
        public const string HeaderInvoice = "{HeaderInvoice}";
        public const string FooterInvoice = "{FooterInvoice}";
        public const string ContentInvoice = "{ContentInvoice}";
        public const string MarkImage = "{MarkImageByInvoiceStatus}";
        public const string Logo = "{Logo}";
        public const string MessageSwitchInvoice = "{MessageSwitchInvoice}";
        public const string NoticeSwichInvoice = "{NoticeSwichInvoice}";
        public const string Discount = "{Discount}";
        public const string AmountDiscount = "{AmountDiscount}";
        public const string DiscountPercent = "{DiscountPercent}";
        public const string Tax = "{ItemTax}";
        public const string AmountTax = "{AmountTaxItem}";
        public const string AmountNoneTax = "{AmountNoneTax}";
        public const string AmountTax0 = "{AmountTax0}";
        public const string AmountTax5 = "{AmountTax5}";
        public const string AmountTax10 = "{AmountTax10}";
        public const string AmountTotal = "{AmountTotalItem}";
        public const string Reference = "{Reference}";
        public const string PathFont = "{PathFont}";
    }
    public static class ReportConfig
    {
        public const string MergeCell = "Merge_Cell";
        public const string RangeFillInvoice = "Range_FillInvoice";

        public const string ItemOrder = "Item_Order";

        public const string ItemName = "Item_Name";

        public const string ItemUnit = "Item_Unit";

        public const string Config = "Config";

        public const string ItemQuantity = "Item_Quantity";
        public const string ItemQuantityFormat = "Item_QuantityFormat";

        public const string ItemPrice = "Item_Price";
        public const string ItemPriceFormat = "Item_PriceFormat";

        public const string ItemTotal = "Item_Total";
        public const string ItemTotalFormat = "Item_TotalFormat";

        public const string HideRowNotice = "HideRowNotice";

        public const string GroupMergeCell = "Merge";
        public const string GroupInvoiceDetail = "InvoiceDetail";

        public const string MaxCharator = "MaxCharator";
        public const string RatioConvertUpperCase = "RatioConvertUpperCase";

    }

    public static class ScheduleJobInfo
    {
        public const int RoleIdDealer = 5;//DEALER_USER
        public const int RoleDealerAdmin = 4;//DEALER_ADMIN

        public const string RecurringJobQueue = "RECURRING";
        public const string BackgroundJobQueue = "BACKGROUNDJOB";
        public const string DelayedJobQueue = "DELAYED";

        public const string DailyCheckUserLoginJobId = "ScheduleJobBusiness.NotificationUsersNotLogin";
        public const string DailyReminDealerAdmin = "ScheduleJobBusiness.RemindCheckUser";

        public const string RecuringDaily = "Daily";
        public const string RecuringMonthly = "Monthly";
        public const string Minutely = "Minutely";
    }

    public static class AssetSignInvoice
    {
        public const string Release = "Releases";
        public const string Invoice = "Invoice";
        public const string SignFile = "Sign";
        public const string TempSignFile = "Temp";
        public const string TemplateInvoiceFolder = "InvoiceTemplate";
    }

    public static class AssetSignAnnoucement
    {
        public const string Release = "Releases";
        public const string Invoice = "Invoice";
        public const string SignFile = "SignAnnouncement";
        public const string TempSignFile = "Temp";
        public const string TemplateInvoiceFolder = "InvoiceTemplate";
        public const string Contracts = "Contracts";

    }
    public static class AssetSignNotification
    {
        public const string Release = "Releases";
        public const string Invoice = "Invoice";
        public const string SignFile = "SignNotification";
        public const string TempSignFile = "Temp";
        public const string TemplateInvoiceFolder = "NotificationInvoiceTemplate";
        public const string Contracts = "Contracts";

    }
    // Old, bỏ dần
    public static class CustomerLevel
    {
        public const string Customer = "Cus";
        public const string Sellers = "Seller";
    }
    // New, dành cho SHINHAN
    public static class LevelCustomerInfo
    {
        public const string HO = "HO";  // HO
        public const string Branch = "CN";  // Chi nhánh
        public const string TransactionOffice = "PGD";  // Phòng giao dịch

        public static List<string> LevelCustomer
        {
            get
            {
                List<string> lst = new List<string>();
                lst.Add(HO);
                lst.Add(Branch);
                lst.Add(TransactionOffice);
                return lst;
            }
        }
    }

    public static class ContractSortColumn
    {
        public const string OrderByColumnDefault = "ID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"COMPANYID","COMPANYID"},
           {"CUSTOMERNAME","CUSTOMERNAME"},
           {"TAXTCODE","TAXTCODE"},
           {"NO","CONTRACTNO"},
           {"NUMBERINVOICE","NUMBERINVOICE"},
           {"PAID","PAID"},
        };
    }

    public static class CustomerSortColumn
    {
        public const string OrderByColumnDefault = "COMPANYSID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"ID","COMPANYSID"},
           {"NAME","COMPANYNAME"},
           {"TAX","TAXCODE"},
           {"ADDRESS","ADDRESS"},
           {"DELEGATE","DELEGATE"},
           {"TEL","TEL"},
           {"FAX","FAX"},
           {"BANKACCOUNT","BANKACCOUNT"},
           {"BANKNAME","BANKNAME"},
           {"PERSONCONTACT","PERSONCONTACT"},
           {"MOBILE","MOBILE"},
           {"EMAIL","EMAIL"},
           {"ACTIVE","ACTIVE"},
        };
    }

    public static class SelllerSortColumn
    {
        public const string OrderByColumnDefault = "COMPANYSID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"NAME","COMPANYNAME"},
           {"ADDRESS","ADDRESS"},
           {"TAX","TAXCODE"},
           {"PERSONCONTACT","PERSONCONTACT"},
           {"TEL","TEL"},
           {"ACTIVE","ACTIVE"},
        };
    }

    public static class ActiveEmailSortColumn
    {
        public const string OrderByColumnDefault = "ID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"TITLE","TITLE"},
           {"EMAILTO","EMAILTO"},
           {"CONTENT","CONTENTEMAIL"},
           {"CREATEDDATE","CREATEDDATE"},
           {"SENDTEDDATE","SENDTEDDATE"},
           {"STATUS","SENDSTATUS"},
        };
    }

    public static class EmployeelSortColumn
    {
        public const string OrderByColumnDefault = "CREATEDDATE";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"NAME","USERNAME"},
           {"LOGINID","USERID"},
           {"PASSWORD","PASSWORD"},
           {"EMAIL","EMAIL"},
           {"CREATEDATE","CREATEDDATE"},
        };
    }

    public static class RoleSortColumn
    {
        public const string OrderByColumnDefault = "ID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
            {"NAME", "NAME" },
            {"ID", "ID" },
        };
    }

    public static class AutoNumberSortColumn
    {
        public const string OrderByColumnDefault = "TYPEID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
            {"ID","TYPEID"},
            {"CODE","TYPEID"},
            {"NAME","TYPENAME"},
            {"TYPE1","S1TYPE"},
            {"TYPE2","S2TYPE"},
            {"TYPE3","S3TYPE"},
            {"SEPARATED","SEPARATORS"},
            {"LENGTH","OUTPUTLENGTH"},
            {"TYPEDISPLAY","OUTPUTORDERS"},
            {"STATUS","ACTIVED"},
        };
    }

    public static class QuartzJobSortColumn
    {
        public const string OrderByColumnDefault = "ID";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
            {"ID","ID"},
            {"JOBID","JOBID"},
            {"SCHEDULENAME","SCHEDULENAME"},
            {"TRIGGERNAME","JOBNAME"},
            {"TRIGGERSTATE","STATUS"},
            {"CRONEXPRESSION","SCRONEXPRESSION"},
            {"PREVFIRETIME","LASTTIMERUN"},
            {"NEXTFIRETIME","NEXTTIMERUN"},
        };
    }

    public static class CharacterAction
    {
        public const char Create = 'C';
        public const char Update = 'U';
        public const char Delete = 'D';
        public const char Read = 'R';
        public const char Active = 'A';
        public const char Approve = 'P';
        public const char Sign = 'S';
        public const char Rejected = 'J';
        public const char Pay = 'Y';
    }

    public static class StepToCreateInvoiceNo
    {
        public const int WhenApprove = 1;
        public const int WhenSign = 2;
    }

    public static class InvoicePrintInfoConst
    {
        public const string MessageSwitchInvoice = "(HÓA ĐƠN CHUYỂN ĐỔI TỪ HÓA ĐƠN ĐIỆN TỬ/CONVERTED FROM EINVOICE)";

        public const string NoticeSwitchInvoice = "<b>Ghi chú:</b> In chuyển đổi lúc <i> {0} Ngày {1}</i>;<b> Người chuyển đổi</b>  <i> (ký ghi rõ họ tên)</i> :";
        public const string NoticeSwitchInvoiceBilingual = "<b>Ghi chú:</b> In chuyển đổi lúc <i> {0} Ngày {1}</i>;<b> Người chuyển đổi <i class=\"f-weight-nomal\">(Converter)</i></b>  <i> (ký ghi rõ họ tên)</i> :";

        public const string NoticeChangeInvoice = "({0} <b>{1}</b> ký hiệu <b>{2}</b>, ngày {3} tháng {4} năm {5})";

        public const string SignOfConverterId = "<div class=\"col33-percent\" id=\"sign-of-converter\">";
     
        public const string SignOfConverter = @"
        <div class=""col33-percent"" id=""sign-of-converter"">
            <p class=""text-center"">Người chuyển đổi (Converter)</p>
            <div class=""col14 line"" style=""margin: 5px 15% !important""></div>";

        public const string SignOfConverterId_2 = "<div class=\"col6\" id=\"sign-of-converter\">";
        public const string SignOfConverter_2 = @"
        <div class=""col6"" id=""sign-of-converter"">
            <p class=""text-center"">Người chuyển đổi (Converter)</p>
            <div class=""col14 line"" style=""margin: 5px 15% !important""></div>";
    }

    public static class ExportInvoiceFileName
    {
        public const string Shinhan_Group = "TemplateGroup_Shinhan.html";
        public const string HuaNan_Detail = "TemplateDetail_HuaNan.html";
        public const string Mega_Detail = "TemplateDetail_Mega.html";
        public const string BIDC_Group = "TemplateGroup_BIDC.html";
        public const string Sino_Detail = "TemplateDetail_Sino.html";
        public static bool IsTemplateShinhan(string templateName)
        {
            if (templateName == Shinhan_Group)
            {
                return true;
            }
            return false;
        }

        public static bool IsTemplateHuaNan(string templateName)
        {
            if (templateName == HuaNan_Detail)
            {
                return true;
            }
            return false;
        }

        public static bool IsTemplateMega(string templateName)
        {
            if (templateName == Mega_Detail)
            {
                return true;
            }
            return false;
        }

        public static bool IsTemplateBIDC(string templateName)
        {
            if (templateName == BIDC_Group)
            {
                return true;
            }
            return false;
        }
        public static bool IsTemplateSINO(string templateName)
        {
            if (templateName == Sino_Detail)
            {
                return true;
            }
            return false;
        }
    }

    public static class CustomerIndex
    {
        public const int Shinhan = 1;
        public const int HuaNan = 2;
        public const int Mega = 3;
        public const int Sino = 4;
        public const int BIDC = 5;

        public static bool IsShinhan { set; get; }
        public static bool IsHuaNan { set; get; }
        public static bool IsMega { set; get; }
        public static bool IsSino { set; get; }
        public static bool IsBIDC { set; get; }

        private static readonly Func<string, string> getConfig = key => WebConfigurationManager.AppSettings[key];

        static CustomerIndex()
        {
            int customerIndex = GetCustomerIndex();
            IsShinhan = Shinhan == customerIndex;
            IsHuaNan = HuaNan == customerIndex;
            IsMega = Mega == customerIndex;
            IsSino = Sino == customerIndex;
            IsBIDC = BIDC == customerIndex;
        }

        public static int GetCustomerIndex()
        {
            int CustomerIndex = 0;
            var CustomerIndexString = getConfig("CustomerIndex");
            CustomerIndex = int.Parse(CustomerIndexString);
            if (CustomerIndex <= 0)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, "CustomerIndex must be positive integers!");
            }
            return CustomerIndex;
        }
    }

    public static class TemplateType
    {
        public static bool OneTax { set; get; } = false;
        public static bool MultiTax { set; get; } = false;

        static TemplateType()
        {
            if (CustomerIndex.IsShinhan || CustomerIndex.IsBIDC)
            {
                OneTax = true;
            }
            if (CustomerIndex.IsHuaNan || CustomerIndex.IsMega || CustomerIndex.IsSino)
            {
                MultiTax = true;
            }
        }
    }

    public static class CustomerDraft
    {
        public const string CustomerDraftString = "DRAFT";
    }

    public static class GetConnectionString
    {
        public static string GetByName(string connectionName)
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DataClassesDataContext"].ConnectionString;
            var connectionStringUpper = connectionString.ToUpper();
            var searchPatten = "connection string".ToUpper();
            if (!connectionStringUpper.Contains(searchPatten))
            {
                return connectionString;
            }

            var searchPatten2 = "\"";
            var posSearchPatten = connectionStringUpper.IndexOf(searchPatten);
            var posStart = connectionStringUpper.IndexOf(searchPatten2, posSearchPatten);
            var posEnd = connectionStringUpper.IndexOf(searchPatten2, posStart + 1);

            var res = connectionString.Substring(posStart + 1, posEnd - posStart - 1);
            return res;
        }
    }

    public static class TextDetectLocationConverterSign
    {
        public static List<string> Texts { set; get; }
        static TextDetectLocationConverterSign()
        {
            Texts = new List<string>()
                {
                    "Người mua",
                    "(Customer)",
                };

            if (CustomerIndex.IsShinhan)
            {
                Texts = new List<string>()
                {
                    "Người mua",
                    "(Customer)",
                };
                return;
            }
            if (CustomerIndex.IsHuaNan)
            {
                Texts = new List<string>()
                {
                    "KHÁCH HÀNG",
                    "CHỮ KÝ THẨM QUYỀN",
                    "(Customer Signature and full name)",
                };
                return;
            }
            if (CustomerIndex.IsBIDC)
            {
                Texts = new List<string>()
                {
                    "Khách hàng",
                    "(Ký, ghi rõ họ tên)",
                };
                return;
            }
            if (CustomerIndex.IsSino)
            {
                Texts = new List<string>()
                {
                    "Khách hàng",
                    "Cần kiểm tra đối chiếu khi",
                };
                return;
            }
            if (CustomerIndex.IsMega)
            {
                Texts = new List<string>()
                {
                    "KHÁCH HÀNG",
                    "(Customer Signature and full name)",
                };
                return;
            }
        }
    }

    public static class BIDCApiTag
    {
        public const string RETURNSTR = "RETURNSTR";
        public const string TXNREFCODE = "TXNREFCODE";
        public const string STATUS = "STATUS";
        public const string content = "content";
        public const string BRANCHID = "BRANCHID";
        public const string INVOICENO = "INVOICENO";
        public const string INVOICEDATE = "INVOICEDATE";
        public const string CUSTOMERCODE = "CUSTOMERCODE";
        public const string CUSTOMERNAME = "CUSTOMERNAME";
        public const string PAYMENTMETHOD = "PAYMENTMETHOD";
        public const string CURRENCYCODE = "CURRENCYCODE";
        public const string EXCHANGERATE = "EXCHANGERATE";
        public const string DESCRIPTION = "DESCRIPTION";
        public const string QUANTITY = "QUANTITY";
        public const string AMOUNT = "AMOUNT";
        public const string VATRATE = "VATRATE";
        public const string TAXAMOUNT = "TAXAMOUNT";
        public const string TOTALAMOUNT = "TOTALAMOUNT";
        public const string BANKREF = "BANKREF";
        public const string CHARGEACCTNO = "CHARGEACCTNO";
        public const string ISORG = "ISORG";
        public const string TAXCODE = "TAXCODE";
        public const string ADDRESS = "ADDRESS";
        public const string SENDINVOICEBYMONTH = "SENDINVOICEBYMONTH";
        public const string DATESENDINVOICE = "DATESENDINVOICE";
        public const string MOBILE = "MOBILE";
        public const string EMAIL = "EMAIL";
        public const string DELEGATE = "DELEGATE";
        public const string PERSONALCONTACT = "PERSONALCONTACT";
        public const string BANKACCOUNT = "BANKACCOUNT";
        public const string ACCOUNTHOLDER = "ACCOUNTHOLDER";
        public const string BANKNAME = "BANKNAME";
        public const string ISCREATEACCOUNT = "ISCREATEACCOUNT";
    }

    public static class BIDCImportInvoiceExternalApiStatus
    {
        public const string TransactionApproved = "00";
        public const string NoDataWasFound = "01";
        public const string InStringIsInValid = "02";
        public const string BIDCSystemError = "96";
        public static Dictionary<string, ResultCode> ListErrorCode { set; get; } = new Dictionary<string, ResultCode>()
        {
            { NoDataWasFound, ResultCode.ImportAPIStatusReturnErrorNoDataWasFound },
            { InStringIsInValid, ResultCode.ImportAPIStatusReturnErrorInStringIsInValid },
            { BIDCSystemError, ResultCode.ImportAPIStatusReturnErrorBIDCSystemError },
        };
    }

    public static class TaxCodes
    {
        public const string _0 = "T00";
        public const string _5 = "T05";
        public const string _8 = "T08";
        public const string _10 = "T10";
        public const string noTax = "TS0";
        public static Dictionary<string, string> ListTax { set; get; } = new Dictionary<string, string>()
        {
            { _0, "0%"},
            { _5, "5%"},
            { _8, "8%"},
            { _10, "10%"},
            {noTax, "KCT" }
        };
    }


    public static class TypeRepayment
    {
        public const int TM = 1;
        public const int CK = 2;
        public const int TM_TC = 3;
    }

    public static class DraftCustomer
    {
        public const string CustomerName = "KHACH HANG KHONG LAY HOA DON";
        public const string CustomerCode = "VANGLAI";
    }
    public static class NarString
    {
        public const string Inward = "Inward SWIFT - Foreigner";
        public const string LCCourier = "LC Courier Charges Other Area";
        public const string LCNegotiation = "LC Negotiation Commission";
        public const string LCAdvising = "LC Advising Commission";
    }
    public static class ImportFields
    {
        public const string CITAD_CODE = "75658001";
        public const string DEFAULT_DESCRIPTION = "Phi dich vu";
        public const int CORE_VALUE_OF_CURRENCY = 4;
        public const int VALUE_IMPORT_CURRENCY = 1;
        public const string TYPE_PAYMENT_CODE = "CK";

        public static readonly string[] CURRENCIES = { "JPY", "EUR", "GBP", "USD" };
    }

    public static class INVOICEDECLARATIONSortColumn
    {
        public const string OrderByColumnDefault = "DeclarationDate";
        public static Dictionary<string, string> OrderByColumn { get; set; } = new Dictionary<string, string>
        {
           {"DeclarationType","DeclarationType"}
        };
    }
    public static class TypeINVOICEDECLARATION
    {
        public const int T = 1;
        public const int D = 2;
        public const int L = 3;
        public const int M = 4;
        public const int N = 5;
        public const int B = 6;
        public const int G = 7;
        public const int H = 8;
        public static Dictionary<int?, string> ListTypeInvoice { set; get; } = new Dictionary<int?, string>()
            {
                { 1, "T"},
                { 2, "D"},
                { 3, "L"},
                { 4, "M" },
                { 5, "N"},
                { 6, "B"},
                { 7, "G"},
                { 8, "H" }
            };
    }
    public static class CheckCodeCQT
    {
        public const string messageErrorr = "Not have code CQT";
    }
    public static class NotCodeCQT
    {
        public const string ErrorrTwan = "NotCodeCQT";
    }
    public static class StatusCQT
    {
        public const int success = 1;
        public const int error = 0;
    };
    public static class LKDLieu
    {
        public const int Q = 0;
        public const int T = 1;
        public const int N = 2;

        public static Dictionary<int?, string> ListLKDLieu { set; get; } = new Dictionary<int?, string>()
        {
            { 0, "Q"},
            { 1, "T"},
            { 2, "N"},
        };
    };
    public static class ReportVATField
    {
        public static readonly string[] INCOMEACCOUNTS =
            { "Domestic", "Outward", "Inward", "Cash withdrawal","LC SWIFT", "LC Payment", "LC Negotiation",
                "LC Courier", "Service fee", "Balance", "Consulting fee","LC Inwar", "Account", "LC Advising",
                "LC Amendment", "LC Opening" };
        public static Dictionary<string, string> GetIncomeAccount { get; set; } = new Dictionary<string, string>
            {
                {"Domestic","7110001"},
                {"Outward","7110019"},
                {"Inward","7110011"},
                {"Cash withdrawal","7110011"},
                {"LC SWIFT","7110014"},
                {"LC Payment","7110003"},
                {"LC Negotiation","7110002"},
                {"LC Courier","7110013"},
                {"Service fee","7190007"},
                {"Balance","7110019"},
                {"Consulting fee","7150002"},
                {"LC Inwar","7110009"},
                {"Account","7110019"},
                {"LC Advising","7110004"},
                {"LC Amendment","7110005"},
                {"LC Opening","7110006"},
            };
    }

    public static class StatusGDT
    {
        public const int Status = 1;
        public const string Mltbao2 = "2";
        public const string Mltbao4 = "4";
        public const string MLTDIEP = "204";
    }
}