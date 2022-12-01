using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace InvoiceServer.API.Config
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting'
    public class ApplicationSetting
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting'
    {
        #region Web.app config keys

        /// <summary>
        /// Secret key to append to login information when calculate hash string
        /// </summary>
        [Config("LoginSecretKey")]
        public string LoginSecretKey { get; private set; }

        /// <summary>
        /// Login session timeout in minutes (default 30 minutes)
        /// </summary>
        [Config("SessionTimeout", "30", false)]
        public int SessionTimeout { get; private set; }

        /// <summary>
        /// Flag Use LDAP authentication when login
        /// </summary>
        [Config("AuthLDAP", "true", false)]
        public bool IsUseLDAPAuth { get; private set; }

        [Config("Enable CreateTokenRandom", "true", true)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.EnableCreateTokenRandom'
        public bool EnableCreateTokenRandom { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.EnableCreateTokenRandom'

        #region Key Config For Email

        [Config("UrlResetPassword")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.UrlResetPassword'
        public string UrlResetPassword { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.UrlResetPassword'

        [Config("ResetPasswordTimeOut", "15", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.ResetPasswordTimeOut'
        public int ResetPasswordTimeOut { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.ResetPasswordTimeOut'

        [Config("EmailTemplateFilePath")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.EmailTemplateFilePath'
        public string EmailTemplateFilePath { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.EmailTemplateFilePath'

        [Config("RetryNumberSendEmail", "2", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.RetryNumberSendEmail'
        public int RetryNumberSendEmail { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.RetryNumberSendEmail'
        #endregion

        #region Config TemplateExport
        [Config("ExportTemplateFilePath")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.ExportTemplateFilePath'
        public string ExportTemplateFilePath { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.ExportTemplateFilePath'
        #endregion

        #region Key Config For Schedule Job

        /// <summary>
        /// Enable/Disable Send email notitication to Dealer Admin about: Dealer Users long time no login
        /// </summary>
        [Config("EnableSendEmailNotificationUserNotLogin", "false", false)]
        public bool EnableSendEmailNotificationUserNotLogin { get; set; }

        /// <summary>
        /// Enable/Disable Remind Dealer Admin about: Review dealer user account
        /// </summary>
        [Config("EnableSendEmailRemindDealerAdmin", "false", false)]
        public bool EnableSendEmailRemindDealerAdmin { get; set; }

        /// <summary>
        /// Config Number of day which dealer account not login
        /// </summary>
        [Config("NumberDayScanSendEmail", "5", false)]
        public int NumberDayScanSendEmail { get; set; }

        [Config("NumberSecondScanExportInvoce", "5", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.NumberSecondScanExportInvoce'
        public int NumberSecondScanExportInvoce { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.NumberSecondScanExportInvoce'


        [Config("RecuringJobHour", "0", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.RecuringJobHour'
        public int RecuringJobHour { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.RecuringJobHour'

        [Config("RecuringJobMinute", "0", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.RecuringJobMinute'
        public int RecuringJobMinute { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.RecuringJobMinute'

        /// <summary>
        /// Config time cron job interval: Send email notifcation to Dealer Admin
        /// </summary>
        [Config("CronTimeSendEmailNotification", "1", false)]
        public int CronTimeSendEmailNotification { get; set; }

        /// <summary>
        /// Enable/Disable Automatic reset password after a while
        /// </summary>
        [Config("EnableAutomaticResetPassword", "false", false)]
        public bool EnableAutomaticResetPassword { get; set; }

        /// <summary>
        /// Number of day which Dealer must change password, if not, the password will be automatic reset
        /// </summary>
        [Config("NumberDayMustChangePassword", "180", false)]
        public int NumberDayMustChangePassword { get; set; }

        /// <summary>
        /// Length of new password when automatic reset. Must small than 36 (Becase used Guid string - Max Gui String Length when ToString() is 36)
        /// </summary>
        [Config("LengthOfRandomPassword", "6", false)]
        public int LengthOfRandomPassword { get; set; }

        /// <summary>
        /// Config Interval recuring job remind admin
        /// </summary>
        [Config("DayIntervalJobRemind", "30", false)]
        public int DayIntervalJobRemind { get; set; }

        /// <summary>
        /// Config Interval recuring job reset password
        /// </summary>
        [Config("DayIntervalJobResetPassword", "1", false)]
        public int DayIntervalJobResetPassword { get; set; }
        /// <summary>
        /// Max size of image upload
        /// </summary>
        [Config("MaxSizeImage", "1000", false)]
        public int MaxSizeImage { get; set; }

        [Config("FolderInvoiceFile", @"\\10.0.0.5\01_Public\shared\DataInvoice\Shinhan", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FolderInvoiceFile'
        public string FolderInvoiceFile { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FolderInvoiceFile'

        [Config("FolderExportDataOfCompany", "~/Data/ExportData", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FolderExportDataOfCompany'
        public string FolderExportDataOfCompany { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FolderExportDataOfCompany'

        /// <summary>
        /// Folder to save image uploaded
        /// </summary>
        [Config("FolderAssetOfCompany", "~/Data/Asset", false)]
        public string FolderAssetOfCompany { get; set; }

        /// <summary>
        /// Folder to save image uploaded
        /// </summary>
        [Config("FolderExportTemplateOfCompany", "~/Data/ExportData", false)]
        public string FolderExportTemplateOfCompany { get; set; }

        [Config("FolderSaveReportOfCompany", "~Data/ExportData/Reports", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FolderSaveReportOfCompany'
        public string FolderSaveReportOfCompany { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FolderSaveReportOfCompany'
        #endregion

        [Config("TimeOutUploadFile", "3", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TimeOutUploadFile'
        public int TimeOutUploadFile { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TimeOutUploadFile'

        [Config("VrvXpressFileMaxSize", "2048", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.VrvXpressFileMaxSize'
        public int VrvXpressFileMaxSize { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.VrvXpressFileMaxSize'

        [Config("VrvXpressFileMaxRowData", "100", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.VrvXpressFileMaxRowData'
        public int VrvXpressFileMaxRowData { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.VrvXpressFileMaxRowData'

        [Config("FileFullPathOpenOffice", @"E:\Office\LibreOfficePortable\App\libreoffice\program\soffice.exe", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FileFullPathOpenOffice'
        public string FileFullPathOpenOffice { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FileFullPathOpenOffice'

        [Config("FolderProductImage", "~/Data/ProductImage", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FolderProductImage'
        public string FolderProductImage { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FolderProductImage'

        [Config("MaxCharactersOfRow", "27", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxCharactersOfRow'
        public int MaxCharactersOfRow { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxCharactersOfRow'

        [Config("MaxCharactersOfRowPdf", "34", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxCharactersOfRowPdf'
        public int MaxCharactersOfRowPdf { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxCharactersOfRowPdf'

        [Config("RatioConvertUppercase", "1.5", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.RatioConvertUppercase'
        public double RatioConvertUppercase { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.RatioConvertUppercase'

        [Config("MaxNumberOfRow", "96", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxNumberOfRow'
        public int MaxNumberOfRow { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxNumberOfRow'

        [Config("MaxNumberOfRowNote", "96", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxNumberOfRowNote'
        public int MaxNumberOfRowNote { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxNumberOfRowNote'

        [Config("MaxNumberOfRowNotePdf", "140", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxNumberOfRowNotePdf'
        public int MaxNumberOfRowNotePdf { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxNumberOfRowNotePdf'


        [Config("ImportFileMaxSize", "2048", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.ImportFileMaxSize'
        public int ImportFileMaxSize { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.ImportFileMaxSize'

        [Config("MaxLengthBuffer", "1024", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxLengthBuffer'
        public int MaxLengthBuffer { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxLengthBuffer'

        [Config("MaxSizeFileUpload", "62914560", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxSizeFileUpload'
        public int MaxSizeFileUpload { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MaxSizeFileUpload'

        [Config("Folder UploadTemp")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.UploadTempFolder'
        public string UploadTempFolder { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.UploadTempFolder'

        [Config("UploadFolderImport", "~/Data/Asset/MasterData", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.UploadFolderImport'
        public string UploadFolderImport { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.UploadFolderImport'

        [Config("UploadValidator", "~/Data/ValidatorInvoice", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.UploadValidator'
        public string UploadValidator { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.UploadValidator'

        
#pragma warning disable CS1570 // XML comment has badly formed XML -- 'End tag was not expected at this location.'
/// </summary>
        [Config("MessageSendEmailSuccess", "Gửi email cho khách hàng thành công", false)]
#pragma warning restore CS1570 // XML comment has badly formed XML -- 'End tag was not expected at this location.'
        public string MessageSendEmailSuccess { get; set; }

        [Config("MessageSendEmailError", "Có lỗi trong quá trình gửi email cho khách hàng", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MessageSendEmailError'
        public string MessageSendEmailError { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.MessageSendEmailError'

        [Config("TemplateInvoiceFolder", "~/Data/InvoiceTemplate", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TemplateInvoiceFolder'
        public string TemplateInvoiceFolder { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TemplateInvoiceFolder'

        [Config("TokenKeyResearch", "NewInvoice", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TokenKeyResearch'
        public string TokenKeyResearch { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TokenKeyResearch'

        [Config("UrlServerSign", "http://localhost:350/api/SignInvoice/SignFile", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.UrlServerSign'
        public string UrlServerSign { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.UrlServerSign'

        [Config("CompanyName", "Ngân hàng Shinhan Việt Nam", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterCompanyName'
        public string FooterCompanyName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterCompanyName'

        [Config("TaxCode", "", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterTaxCode'
        public string FooterTaxCode { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterTaxCode'

        [Config("Phone", "", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterPhone'
        public string FooterPhone { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterPhone'

        [Config("CompanyLink", "https://einvoice-shinhan.unit.vn", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterLink'
        public string FooterLink { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterLink'

        [Config("CompanyLinkText", "https://einvoice-shinhan.unit.vn", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterLinkText'
        public string FooterLinkText { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterLinkText'

        [Config("PortalCompanyLink", "https://einvoice-portal.unit.vn/", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterPortalLink'
        public string FooterPortalLink { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterPortalLink'

        [Config("PortalCompanyLinkText", "https://einvoice-portal.unit.vn/", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterPortalLinkText'
        public string FooterPortalLinkText { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.FooterPortalLinkText'

        [Config("Email", "shinhanvietnam@shinhan.com", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.Email'
        public string Email { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.Email'


        [Config("PTCompanyName", "NGÂN HÀNG TNHH MTV SHINHAN VIỆT NAM", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.PTCompanyName'
        public string PTCompanyName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.PTCompanyName'

        [Config("PTCompanyNameEN", "SHINHAN BANK VIET NAM LIMITED", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.PTCompanyNameEN'
        public string PTCompanyNameEN { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.PTCompanyNameEN'

        [Config("AddressStreet", "Tòa nhà Empress, 138-142 Hai Bà Trưng,", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.AddressStreet'
        public string AddressStreet { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.AddressStreet'

        [Config("AddressStreetEN", "Empress Tower, 138-142 Hai Ba Trung,", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.AddressStreetEN'
        public string AddressStreetEN { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.AddressStreetEN'

        [Config("AddressProvince", "Phường Đa Kao, Quận 1, TP.HCM", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.AddressProvince'
        public string AddressProvince { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.AddressProvince'

        [Config("AddressProvinceEN", "Da Kao ward, District 1, Ho Chi Minh City", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.AddressProvinceEN'
        public string AddressProvinceEN { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.AddressProvinceEN'

        [Config("PhoneEmergency", "19001577", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.PhoneEmergency'
        public string PhoneEmergency { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.PhoneEmergency'

        [Config("Youtube", "https://www.youtube.com/channel/UCJWBvAAZCfYEuxVOOVDpkoA", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.Youtube'
        public string Youtube { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.Youtube'

        [Config("Facebook", "https://www.facebook.com/Shinhanbankvn/", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.Facebook'
        public string Facebook { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.Facebook'

        [Config("LinkAddressTransaction", "https://online.shinhan.com.vn/global.shinhan", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.LinkAddressTransaction'
        public string LinkAddressTransaction { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.LinkAddressTransaction'

        [Config("TextCopyright", "Copyright © 2019 by SHINHAN Bank (Vietnam),. All rights reserved.", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TextCopyright'
        public string TextCopyright { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TextCopyright'

        [Config("LinkPrivacyPolicy", "https://shinhan.com.vn/vi/page/chinh-sach-bao-mat.html", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.LinkPrivacyPolicy'
        public string LinkPrivacyPolicy { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.LinkPrivacyPolicy'

        [Config("LinkTermsOfUse", "https://shinhan.com.vn/vi/page/dieu-khoan-su-dung.html", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.LinkTermsOfUse'
        public string LinkTermsOfUse { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.LinkTermsOfUse'

        // Hua Nan Bank header info
        [Config("CompanyNameCn", "華 南 商 業 銀 行", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyNameCn'
        public string CompanyNameCn { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyNameCn'

        [Config("CompanyBranchCn", "胡 志 明 市 分 行", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyBranchCn'
        public string CompanyBranchCn { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyBranchCn'

        [Config("CompanyNameVn", "", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyNameVn'
        public string CompanyNameVn { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyNameVn'

        [Config("CompanyBranchVn", "", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyBranchVn'
        public string CompanyBranchVn { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyBranchVn'

        [Config("CompanyAddressRow1", "", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyAddressRow1'
        public string CompanyAddressRow1 { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyAddressRow1'

        [Config("CompanyAddressRow2", "", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyAddressRow2'
        public string CompanyAddressRow2 { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CompanyAddressRow2'

        [Config("FolderInvoiceFile", @"D:\eInvoice\MigrationData", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.PathFolder'
        public string PathFolder { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.PathFolder'


        [Config("NumOfPackage", "UAA", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.NumOfPackage'
        public string NumOfPackage { get; private set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.NumOfPackage'

        //Run Quartz job
        [Config("RunQuartzJob", "true", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.RunQuartzJob'
        public string RunQuartzJob { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.RunQuartzJob'

        [Config("CustomerIndex", "0", true)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CustomerIndex'
        public string CustomerIndex { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.CustomerIndex'

        [Config("TemplateFooterCompanyName", "CÔNG TY CỔ PHẦN CÔNG NGHỆ UNIT", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TemplateFooterCompanyName'
        public string TemplateFooterCompanyName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TemplateFooterCompanyName'

        [Config("TemplateFooterCompanyTaxCode", "0", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TemplateFooterCompanyTaxCode'
        public string TemplateFooterCompanyTaxCode { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TemplateFooterCompanyTaxCode'

        [Config("TemplateDeClarationFolder", "~/Data/DeclarationTemplate", false)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TemplateDeClarationFolder'
        public string TemplateDeClarationFolder { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.TemplateDeClarationFolder'

        #endregion Web.app config keys

        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private static readonly ApplicationSetting instance = new ApplicationSetting();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.Instance'
        public static ApplicationSetting Instance
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationSetting.Instance'
        {
            get
            {
                return instance;
            }
        }

        #endregion Fields, Properties

        #region Methods

        /// <summary>
        /// Prevent new object of this class from outside
        /// </summary>
        private ApplicationSetting()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                // Get objects's properties from memory cache
                var configProperties = typeof(ApplicationSetting).GetProperties();

                foreach (var property in configProperties)
                {
                    var attr = property.GetCustomAttribute<ConfigAttribute>();
                    if (attr == null)
                    {
                        continue;
                    }

                    var configKey = attr.Key;
                    var configValueStr = attr.DefaultValue;

                    if (ConfigurationManager.AppSettings.AllKeys.Contains(configKey))
                    {
                        configValueStr = ConfigurationManager.AppSettings[configKey];
                    }

                    var configValue = configValueStr.ConvertDataToType(property.PropertyType);
                    property.SetValue(this, configValue);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Get configuration in Web.config failed.");
                throw;
            }
        }

        #endregion Methods
    }
}