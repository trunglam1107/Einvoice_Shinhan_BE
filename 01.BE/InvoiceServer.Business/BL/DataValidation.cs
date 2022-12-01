using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Text;

namespace InvoiceServer.Business.BL
{
    public static class DataValidation
    {
        private const string ErrorMsgUserIdIsNull = "UserID cannot be blank";

        public static StringBuilder AppendLineFormat(this StringBuilder sb, string format, params object[] args)
        {
            return sb.AppendFormat(format, args).AppendLine();
        }

        public static bool IsValid(this LoginInfo loginInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            if (loginInfo.UserId.IsNullOrEmpty())
            {
                errorDetail = ErrorMsgUserIdIsNull;
                errorCode = ResultCode.LoginUserIdIsEmpty;
                return false;
            }

            if (loginInfo.Password.IsNullOrEmpty())
            {
                errorDetail = Resources.ErrorMsgPasswordIsNull;
                errorCode = ResultCode.LoginPasswordIsEmpty;
                return false;
            }

            return true;
        }

        public static bool IsValid(this PasswordInfo passwordInfo, out ResultCode errorCode, out string errorDetail)
        {

            errorDetail = "";
            errorCode = ResultCode.NoError;
            if (passwordInfo.CurrentPassword.IsNullOrEmpty())
            {
                errorDetail = Resources.CurrentPasswordIsBlank;
                errorCode = ResultCode.CountryAdminMgtPasswordIsEmpty;
                return false;
            }

            if (passwordInfo.CurrentPassword.IsOverLength(LoginUserInfo.CurrentPasswordMaxLength))
            {
                errorDetail = Resources.CurrentPasswordOverMaxLenght;
                errorCode = ResultCode.CountryAdminMgtPasswordExceedMaxLength;
                return false;
            }

            if (passwordInfo.NewPassword.IsNullOrEmpty())
            {
                errorDetail = Resources.NewPasswordIsBlank;
                errorCode = ResultCode.CountryAdminMgtPasswordIsEmpty;
                return false;
            }

            if (passwordInfo.NewPassword.IsOverLength(LoginUserInfo.NewPasswordMaxLength))
            {
                errorDetail = Resources.NewPasswordOverMaxLenght;
                errorCode = ResultCode.CountryAdminMgtPasswordExceedMaxLength;
                return false;
            }

            return true;
        }

        public static bool IsValid(this LOGINUSER userInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            if (userInfo.USERID.IsNullOrEmpty())
            {
                errorDetail = LoginUserInfo.MsgUserIdIsEmpty;
                errorCode = ResultCode.UserAccountMgtUserIdIsEmpty;
                return false;
            }

            if (userInfo.USERID.IsOverLength(LoginUserInfo.UserIdMaxLength))
            {
                errorDetail = LoginUserInfo.MsgUserIdMaxLength;
                errorCode = ResultCode.UserAccountMgtUserIdExceedMaxLength;
                return false;
            }

            if (userInfo.USERNAME.IsOverLength(LoginUserInfo.UserNameMaxLength))
            {
                errorDetail = LoginUserInfo.MsgUserNameMaxLength;
                errorCode = ResultCode.UserAccountMgtUsernameExceedMaxLength;
                return false;
            }

            if (userInfo.EMAIL.IsNullOrEmpty())
            {
                errorDetail = LoginUserInfo.MsgEmailIsEmpty;
                errorCode = ResultCode.UserAccountMgtEmailIsEmpty;
                return false;
            }

            if (userInfo.EMAIL.IsOverLength(LoginUserInfo.EmailMaxLength))
            {
                errorDetail = LoginUserInfo.MsgEmailMaxLength;
                errorCode = ResultCode.UserAccountMgtEmailExceedMaxLength;
                return false;
            }

            if (!userInfo.EMAIL.IsEmail())
            {
                errorDetail = LoginUserInfo.MsgEmailInvalid;
                errorCode = ResultCode.UserAccountMgtEmailInvalid;
                return false;
            }

            return true;
        }

        public static bool IsValid(this ResetPassword resetPwdInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            bool isSuccess = true;
            if (!resetPwdInfo.Email.IsNotNullOrEmpty())
            {
                errorDetail = LoginUserInfo.MsgEmailIsEmpty;
                errorCode = ResultCode.UserAccountMgtEmailIsEmpty;
                isSuccess = false;
            }
            else
            {
                string[] arrListEmail = resetPwdInfo.Email.Split(',');
                foreach (string email in arrListEmail)
                {
                    if (!email.Trim().IsEmail())
                    {
                        errorDetail = LoginUserInfo.MsgEmailInvalid;
                        errorCode = ResultCode.UserAccountMgtEmailInvalid;
                        isSuccess = false;
                    }
                }
            }

            return isSuccess;
        }

        public static bool IsValid(this ChangePassword updatePwdInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            if (updatePwdInfo.NewPassword.IsNullOrEmpty())
            {
                errorDetail = Resources.MsgPasswordIsEmpty;
                errorCode = ResultCode.UserAccountMgtPasswordIsEmpty;
                return false;
            }

            if (updatePwdInfo.NewPassword.IsOverLength(LoginUserInfo.PasswordMaxLength))
            {
                errorDetail = Resources.MsgPasswordMaxLength;
                errorCode = ResultCode.UserAccountMgtPasswordExceedMaxLength;
                return false;
            }

            return true;
        }

        public static bool IsValid(this CompanyInfo companyInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            return true;
        }

        public static bool IsValid(this CustomerInfo companyInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            return true;
        }

        public static bool IsValid(this MyCompanyInfo companyInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            return true;
        }

        public static bool IsValid(this CompanyAccount account, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this ProductInfo product, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this ClientAddInfo product, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this InvoiceReleasesInfo invoiceConclude, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this InvoiceReleasesDetailInfo concludeDetail, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this UseInvoiceInfo useInvoiceInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this UseInvoiceDetailInfo useInvoiceDetailInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this InvoiceInfo invoiceInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this InvoiceDetailInfo invoiceInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this RegisterTemplateInfo invoiceCompanyUseInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this ReleaseInvoiceMaster releaseInvoice, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this ReleaseAnnouncementMaster releaseAnnouncement, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;
            return true;
        }

        public static bool IsValid(this EmailServerInfo emailConfigServerInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            return true;
        }

        public static bool IsValid(this ReportCancellingInfo companyInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            return true;
        }

        public static bool IsValid(this RegisterTemplateCancelling companyInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            return true;
        }

        public static bool IsValid(this DeclarationInfo companyInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            return true;
        }
        public static bool IsValid(this DeclarationReleaseInfo companyInfo, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            return true;
        }
        public static bool IsValid(this CompanySymbolInfo compSymbol, out ResultCode errorCode, out string errorDetail)
        {
            errorDetail = "";
            errorCode = ResultCode.NoError;

            return true;
        }


    }
}