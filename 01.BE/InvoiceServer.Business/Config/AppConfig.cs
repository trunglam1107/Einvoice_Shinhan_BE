using DQSServer.Common;
using DQSServer.Common.Extensions;
using DQSServer.Business.Cache;
using DQSServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace DQSServer.Business.Config
{
    public class AppConfig
    {
        #region Config keys

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

        [Config("Enable CreateTokenRandom", "true", true)]
        public bool EnableCreateTokenRandom { get; private set; }

        #region Key Config For Email

        [Config("UrlResetPassword")]
        public string UrlResetPassword { get; private set; }

        [Config("ResetPasswordTimeOut", "15", false)]
        public int ResetPasswordTimeOut { get; private set; }

        [Config("EmailTemplateFilePath")]
        public string EmailTemplateFilePath { get; private set; }

        [Config("RetryNumberSendEmail", "2", false)]
        public int RetryNumberSendEmail { get; private set; }

        [Config("MailServer")]
        public string MailServer { get; private set; }

        [Config("MailPort")]
        public int MailPort { get; private set; }

        [Config("MailEnableSSL", "true", true)]
        public bool MailEnableSSL { get; private set; }

        [Config("MailFromAddress")]
        public string MailFromAddress { get; private set; }

        [Config("MailAuthUser")]
        public string MailAuthUser { get; private set; }

        [Config("MailAuthPass")]
        public string MailAuthPass { get; private set; }

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
        [Config("NumberDayScanLogin", "30", false)]
        public int NumberDayScanLogin { get; set; }

        [Config("RecuringJobHour", "0", false)]
        public int RecuringJobHour { get; set; }

        [Config("RecuringJobMinute", "0", false)]
        public int RecuringJobMinute { get; set; }

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

        /// <summary>
        /// Folder to save image uploaded
        /// </summary>
        [Config("UploadFolderImage", "~/Data/MyCompanyImages", false)]
        public string UploadFolderImage { get; set; }

        /// Folder to save image uploaded
        /// </summary>
        [Config("QuotationTemplateExcelFilePath", "~/Data/ExcelTemplate", false)]
        public string TemplateExcelFilePath { get; set; }

        /// Folder to save image uploaded
        /// </summary>
        [Config("QuotationFolderPathExportFile", "~/Data/ExportTemplate", false)]
        public string FolderPathExportFile { get; set; }

        /// Folder to save image uploaded
        /// </summary>
        [Config("TemplateExportExcel", "Quotation.xlsx", false)]
        public string TemplateExportExcel { get; set; }

        [Config("FileLogoOfDaiKin", "Logo.jpg", false)]
        public string FileLogoOfDaiKin { get; set; }

        #endregion

        [Config("TimeOutUploadFile", "3", false)]
        public int TimeOutUploadFile { get; private set; }

        [Config("VrvXpressFileMaxSize", "2048", false)]
        public int VrvXpressFileMaxSize { get; private set; }

        [Config("VrvXpressFileMaxRowData", "100", false)]
        public int VrvXpressFileMaxRowData { get; private set; }

        [Config("FileFullPathOpenOffice", @"E:\Office\LibreOfficePortable\App\libreoffice\program\soffice.exe", false)]
        public string FileFullPathOpenOffice { get; private set; }

        [Config("FolderProductImage", "~/Data/ProductImage", false)]
        public string FolderProductImage { get; private set; }

        [Config("MaxCharactersOfRow", "15", false)]
        public int MaxCharactersOfRow { get; private set; }

        [Config("MaxNumberOfRow", "15", false)]
        public int MaxNumberOfRow { get; private set; }

        #endregion Web.app config keys

        #region Fields, Properties

        private static Logger logger = new Logger();
        private static readonly AppConfig instance = new AppConfig();

        public static AppConfig Instance
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
        private AppConfig()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                // Get objects's properties from memory cache
                var configProperties = typeof(AppConfig).GetProperties();
                var configData = GetSettingData();

                foreach (var property in configProperties)
                {
                    var attr = property.GetCustomAttribute<ConfigAttribute>();
                    if (attr == null)
                    {
                        continue;
                    }

                    var configKey = attr.Key;
                    var configValueStr = attr.DefaultValue;

                    if (configData.ContainsKey(configKey))
                    {
                        configValueStr = configData[configKey].SettingValue;
                    }

                    var configValue = configValueStr.ConvertDataToType(property.PropertyType);
                    property.SetValue(this, configValue);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Get configuration in database failed.");
                throw;
            }
        }

        private Dictionary<string, FunctionalSetting> GetSettingData()
        {
            var dicSetting = new Dictionary<string, FunctionalSetting>();
            var settings = CacheManagement.Instance.GetApplicationSettingCache().ToList();
            foreach (var setting in settings)
            {
                if (!dicSetting.ContainsKey(setting.SettingKey))
                {
                    dicSetting.Add(setting.SettingKey, setting);
                }
            }

            return dicSetting;
        }

        #endregion Methods
    }
}
