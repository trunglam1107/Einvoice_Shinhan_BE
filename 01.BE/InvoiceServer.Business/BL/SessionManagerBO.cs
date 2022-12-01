using InvoiceServer.Business.Cache;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;


namespace InvoiceServer.Business.BL
{
    public class SessionManagerBO : ISessionManagerBO
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        // get config data in Web.config file
        private const int Keysize = 256;
        private const int DerivationIterations = 1000;
        private readonly TimeSpan loginSessionTimeout;
        private readonly TimeSpan tokenResetPasswordExpire;
        private readonly bool createTokenRandom;

        private readonly ILoginUserRepository loginUserRepository;
        private readonly IConfigEmailServerRepository configEmailServerRepository;
        private readonly IUVGetFunctionOfUserRepository uV_GetFunctionOfUserRepository;
        private readonly ISystemSettingRepository systemSettingRepository;
        private readonly IClientRepository clientRepository;

        #endregion

        #region Contructor

        public SessionManagerBO(IRepositoryFactory repoFactory, SessionConfig config)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            Ensure.Argument.ArgumentNotNull(config, "config");

            this.loginUserRepository = repoFactory.GetRepository<ILoginUserRepository>();
            this.configEmailServerRepository = repoFactory.GetRepository<IConfigEmailServerRepository>();
            this.uV_GetFunctionOfUserRepository = repoFactory.GetRepository<IUVGetFunctionOfUserRepository>();
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
            clientRepository = repoFactory.GetRepository<IClientRepository>();
            this.loginSessionTimeout = config.SessionTimeout;
            this.createTokenRandom = config.CreateTokenRandom;
            this.tokenResetPasswordExpire = config.SessionResetPasswordExpire;
        }

        #endregion

        #region ISessionManagerBO Methods

        public UserSessionInfo Login(LoginInfo loginInfo, string keyOfPackage = null)
        {
            if (WebConfigurationManager.AppSettings["AuthLDAP"].ToString().Equals("true") && loginInfo.UserId != DefaultFields.ADMIN_USER_ID)
            {
                return this.LoginLDAP(loginInfo, keyOfPackage);
            }
            else
            {
                return this.LoginNotLDAP(loginInfo, keyOfPackage);
            }
        }

        private UserSessionInfo LoginLDAP(LoginInfo loginInfo, string keyOfPackage = null)
        {
            if (SingletonAuthenticationLdap.Instance.IsAuthenticated(loginInfo.UserId, loginInfo.Password))
            {
                ResultCode errorCode;
                string errorMessage;
                if (!loginInfo.IsValid(out errorCode, out errorMessage))
                {
                    throw new BusinessLogicException(errorCode, errorMessage);
                }

                // Get User information
                var loginUser = this.loginUserRepository.LoginLDAP(loginInfo.UserId);
                if (loginUser == null || loginUser.CLIENTID.HasValue)
                {
                    throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                       string.Format("Login faild because userId [{0}] not found.", loginInfo.UserId));
                }
                if (loginUser.MYCOMPANY != null && ((loginUser.MYCOMPANY.DELETED == true) || !(loginUser.MYCOMPANY.ACTIVE ?? false)))
                {
                    throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                      string.Format("Login faild because userId [{0}] not found.", loginInfo.UserId));
                }

                if (Utility.IsTrue(loginUser.DELETED))
                {
                    throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                        string.Format("Login failed failed because user with USER_ID = [{0}] has been deleted.", loginInfo.UserId));
                }
                UserSessionInfo userSessionInfo = GetSessionInfo(loginUser);
                userSessionInfo.EmailServer = GetEmailServer();
                userSessionInfo.NumberInvoicePackage = getNumberOfPackage(keyOfPackage);
                UserSessionCache.Instance.SaveUserSession(userSessionInfo, this.loginSessionTimeout);
                return userSessionInfo;
            }
            else
            {
                throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                        string.Format("Login failed failed because userId [{0}] not found.", loginInfo.UserId));
            }
        }

        private UserSessionInfo LoginNotLDAP(LoginInfo loginInfo, string keyOfPackage = null)
        {
            ResultCode errorCode;
            string errorMessage;
            if (!loginInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            // Get User information
            var loginUser = this.loginUserRepository.LoginAdmin(loginInfo.UserId, loginInfo.Password);
            if (loginUser == null || loginUser.CLIENTID.HasValue)
            {
                throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                   string.Format("Login faild because userId [{0}] not found.", loginInfo.UserId));
            }
            if (loginUser.MYCOMPANY != null && ((loginUser.MYCOMPANY.DELETED == true) || !(loginUser.MYCOMPANY.ACTIVE ?? false)))
            {
                throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                  string.Format("Login faild because userId [{0}] not found.", loginInfo.UserId));
            }

            if (Utility.IsTrue(loginUser.DELETED))
            {
                throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                    string.Format("Login failed failed because user with USER_ID = [{0}] has been deleted.", loginInfo.UserId));
            }


            // Cache login information
            UserSessionInfo userSessionInfo = GetSessionInfo(loginUser);
            userSessionInfo.EmailServer = GetEmailServer();
            userSessionInfo.NumberInvoicePackage = getNumberOfPackage(keyOfPackage);
            UserSessionCache.Instance.SaveUserSession(userSessionInfo, this.loginSessionTimeout);
            return userSessionInfo;
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        public long getNumberOfPackage(string keyOfPackage)
        {
            if (keyOfPackage.IsNullOrEmpty())
            {
                return 0;
            }
            try
            {
                var password = Resources.EnCodePassword;
                var key = Decrypt(keyOfPackage, password);
                switch (key)
                {
                    case "UAA":
                        return (long)PackageType.UAA;
                    case "UAB":
                        return (long)PackageType.UAB;
                    case "UAC":
                        return (long)PackageType.UAC;
                    case "UAD":
                        return (long)PackageType.UAD;
                    case "UAE":
                        return (long)PackageType.UAE;
                    case "UAF":
                        return (long)PackageType.UAF;
                    case "UAG":
                        return (long)PackageType.UAG;
                    case "UAH":
                        return (long)PackageType.UAH;
                    case "UAI":
                        return (long)PackageType.UAI;
                    case "UAJ":
                        return (long)PackageType.UAJ;
                    case "UBA":
                        return (long)PackageType.UBA;
                    case "UBB":
                        return (long)PackageType.UBB;
                    case "UBC":
                        return (long)PackageType.UBC;
                    case "UBD":
                        return (long)PackageType.UBD;
                    case "UBE":
                        return (long)PackageType.UBE;
                    case "UBF":
                        return (long)PackageType.UBF;
                    case "UBG":
                        return (long)PackageType.UBG;
                    case "UBH":
                        return (long)PackageType.UBH;
                    case "UBI":
                        return (long)PackageType.UBI;
                    case "UBJ":
                        return (long)PackageType.UBJ;
                    case "UAX":
                        return (long)PackageType.UAX;
                    default:
                        return 0;
                }
            }
            catch (Exception ex)
            {
                logger.Error("get number invoice of package is fail", ex);
                return 0;
            }
        }

        public UserSessionInfo JobLogin(string keyOfPackage = null)
        {
            var loginUser = this.loginUserRepository.JobLogin();
            //logger.QuarztJob(false, $"loginUser is : {loginUser.USERID}");
            UserSessionInfo userSessionInfo = GetSessionInfo(loginUser);
            //logger.QuarztJob(false, $"UserSessionInfo is : {userSessionInfo.UserId}");
            userSessionInfo.EmailServer = GetEmailServer();
            userSessionInfo.NumberInvoicePackage = getNumberOfPackage(keyOfPackage);
            UserSessionCache.Instance.SaveUserSession(userSessionInfo, this.loginSessionTimeout);
            return userSessionInfo;
        }

        public UserSessionInfo ClientLogin(LoginInfo loginInfo)
        {
            ResultCode errorCode;
            string errorMessage;
            if (!loginInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }
            loginInfo.Password = encryptPassword(loginInfo.Password);
            // Get User information
            var loginUser = this.loginUserRepository.ClientLogin(loginInfo.UserId, loginInfo.Password);
            if (loginUser == null || !loginUser.CLIENTID.HasValue)
            {
                throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                   string.Format("Login faild because userId [{0}] not found.", loginInfo.UserId));
            }
            if (Utility.IsTrue(loginUser.DELETED))
            {
                throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                    string.Format("Login failed failed because user with USER_ID = [{0}] has been deleted.", loginInfo.UserId));
            }

            var client = this.clientRepository.GetById(loginUser?.CLIENTID ?? 0) ?? new CLIENT();

            loginUser.MYCOMPANY = new MYCOMPANY()
            {
                COMPANYNAME = client.CUSTOMERNAME,
                TAXCODE = client.TAXCODE,
                ADDRESS = client.ADDRESS,
                MOBILE = client.MOBILE,
                FAX = client.FAX,
                BANKACCOUNT = client.BANKACCOUNT,
                ACCOUNTHOLDER = client.ACCOUNTHOLDER,
                BANKNAME = client.BANKNAME,
            };
            // Cache login information
            UserSessionInfo userSessionInfo = GetSessionInfo(loginUser);
            UserSessionCache.Instance.SaveUserSession(userSessionInfo, this.loginSessionTimeout);
            return userSessionInfo;
        }

        public UserSessionInfo Logout(string token)
        {
            // Delete token in session
            var sessionInfo = UserSessionCache.Instance.RemoveUserSession(token);
            return sessionInfo;
        }

        public UserSessionInfo GetUserSession(string token)
        {
            return UserSessionCache.Instance.GetUserSession(token);
        }

        public UserSessionInfo ResetPassword(ResetPassword resetPasswordInfo)
        {
            ResultCode resultCode;
            string errorDetail;
            if (!resetPasswordInfo.IsValid(out resultCode, out errorDetail))
            {
                throw new BusinessLogicException(resultCode, errorDetail);
            }

            var loginUser = GetUserByEmail(resetPasswordInfo.Email);
            UserSessionInfo userSessionInfo = GetSessionInfo(loginUser);
            userSessionInfo.EmailServer = GetEmailServer();
            UserSessionCache.Instance.SaveUserSession(userSessionInfo, this.tokenResetPasswordExpire);

            return userSessionInfo;
        }

        public ResultCode UpdatePassword(long userId, ChangePassword updatePasswordInfo)
        {
            ResultCode resultCode;
            string errorDetail;
            if (!updatePasswordInfo.IsValid(out resultCode, out errorDetail))
            {
                throw new BusinessLogicException(resultCode, errorDetail);
            }
            var loginUser = GetUserById(userId);
            if (updatePasswordInfo.IsForgot != true && loginUser.PASSWORD != updatePasswordInfo.OldPassword)
            {
                throw new BusinessLogicException(ResultCode.OldPasswordIncorrect, "Mật khẩu cũ không chính xác.");
            }
            // Update Pasword
            var currentTime = DateTime.Now;
            loginUser.PASSWORD = updatePasswordInfo.NewPassword;
            loginUser.UPDATEDDATE = currentTime;
            loginUser.LASTCHANGEDPASSWORDTIME = currentTime;
            return loginUserRepository.Update(loginUser) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public ResultCode UpdateClientPassword(long userId, ChangePassword updatePasswordInfo)
        {
            // Validate data
            if (updatePasswordInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            ResultCode resultCode;
            string errorDetail;
            if (!updatePasswordInfo.IsValid(out resultCode, out errorDetail))
            {
                throw new BusinessLogicException(resultCode, errorDetail);
            }
            var loginUser = GetClientById(userId);
            var oldPassword = encryptPassword(updatePasswordInfo.OldPassword);
            //if (loginUser.PASSWORD != oldPassword)
            //{
            //    throw new BusinessLogicException(ResultCode.OldPasswordIncorrect, "Mật khẩu cũ không chính xác.");
            //}
            // Update Pasword
            var currentTime = DateTime.Now;
            loginUser.PASSWORD = encryptPassword(updatePasswordInfo.NewPassword);
            loginUser.PASSWORDSTATUS = (int)PasswordStatus.Changed;
            loginUser.UPDATEDDATE = currentTime;
            loginUser.LASTCHANGEDPASSWORDTIME = currentTime;
            return loginUserRepository.Update(loginUser) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public ResultCode ResetClientPassword(ResetPassword updatePasswordInfo)
        {
            ResultCode resultCode;
            string errorDetail;
            if (!updatePasswordInfo.IsValid(out resultCode, out errorDetail))
            {
                throw new BusinessLogicException(resultCode, errorDetail);
            }
            var loginUser = loginUserRepository.GetByUserIdAndEmail(updatePasswordInfo.UserId, updatePasswordInfo.Email);

            if (loginUser == null)
            {
                throw new BusinessLogicException(ResultCode.AccountInfoIncorrect, "Thông tin tài khoản không chính xác.");
            }
            updatePasswordInfo.ClientId = (long)loginUser.CLIENTID;
            // Update Pasword
            var newPassword = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
            var currentTime = DateTime.Now;
            loginUser.PASSWORD = newPassword;
            loginUser.PASSWORDSTATUS = (int)PasswordStatus.NotEncript;
            loginUser.UPDATEDDATE = currentTime;
            loginUser.LASTCHANGEDPASSWORDTIME = currentTime;
            return loginUserRepository.Update(loginUser) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        private string encryptPassword(string password)
        {
            //encrypt MD5
            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(password));
            byte[] resultHash = md5.Hash;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < resultHash.Length; i++)
            {
                //change it into 2 hexadecimal digits for each byte  
                strBuilder.Append(resultHash[i].ToString("x2"));
            }
            return strBuilder.ToString();
        }

        /// <summary>
        /// A user has been changed (updated/deleted).
        /// </summary>
        /// <param name="user">The user changed</param>
        /// <param name="isDeleted">true: user is deleted, false: user is updated.</param>

        #endregion ISessionManagerBO Methods

        #region Private methods

        private UserSessionInfo GetSessionInfo(LOGINUSER loginUser)
        {
            string token = this.createTokenRandom ? Guid.NewGuid().ToString("N") : loginUser.USERID;
            UserSessionInfo userSessionInfo = new UserSessionInfo(loginUser, loginUser.USERLEVEL, loginUser.MYCOMPANY, token);
            var roleOfUser = this.uV_GetFunctionOfUserRepository.GetByUserSID(loginUser.USERSID, loginUser.COMPANYID);
            userSessionInfo.RoleUser.Permissions = GetPermissions(roleOfUser);
            userSessionInfo.EmailServer = GetEmailServer();
            userSessionInfo.SystemSetting = this.systemSettingRepository.FilterSystemSetting(loginUser.COMPANYID ?? 0);
            GetSystemSettingListRowNumber(userSessionInfo.SystemSetting);
            userSessionInfo.Language = loginUser.LANGUAGE;
            return userSessionInfo;
        }
        private void GetSystemSettingListRowNumber(SystemSettingInfo SystemSetting)
        {
            var i = 1;
            SystemSetting.ListRowNumber = new List<int>();
            while (i < 30)
            {
                string value = WebConfigurationManager.AppSettings["RowNumber_" + i];
                if (value != null)
                {
                    SystemSetting.ListRowNumber.Add(int.Parse(value));
                    i++;
                }
                else
                {
                    break;
                }
            }
        }
        private List<string> GetPermissions(IEnumerable<UV_GETFUNCTIONOFUSER> roleOfUsers)
        {
            List<string> permissionOfUser = new List<string>();
            if (roleOfUsers == null)
            {
                return permissionOfUser;
            }

            roleOfUsers.Where(x => !(x.DELETED ?? false)).ForEach(p =>
            {

                if (p.ACTION.IsNotNullOrEmpty())
                {
                    foreach (var item in p.ACTION.ToArray())
                    {
                        permissionOfUser.Add(string.Format("{0}_{1}", p.FUNCTIONNAME, item));
                    }
                }
            });

            return permissionOfUser;
        }

        private LOGINUSER GetUserByEmail(string email)
        {
            var loginUser = this.loginUserRepository.GetUserByEmail(email);
            if (loginUser == null)
            {
                throw new BusinessLogicException(ResultCode.LoginEmailNotExist,
                                   string.Format("Email does not exist: {0}", email));
            }

            return loginUser;
        }

        private LOGINUSER GetUserById(long userId)
        {
            var loginUser = this.loginUserRepository.GetById(userId);
            if (loginUser == null)
            {
                throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                   string.Format("User id does not exist: {0}", userId));
            }

            return loginUser;
        }

        private LOGINUSER GetClientById(long userId)
        {
            var loginUser = this.loginUserRepository.GetById(userId);
            if (loginUser == null)
            {
                throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                   string.Format("User id does not exist: {0}", userId));
            }
            //check is client
            if (loginUser.CLIENTID.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                   string.Format("User id does not exist: {0}", userId));
            }

            return loginUser;
        }
        public EmailServerInfo GetEmailServer()
        {
            var emailServer = this.configEmailServerRepository.GetByCompany();
            if (emailServer == null)
            {
                return null;
            }

            return new EmailServerInfo(emailServer);
        }
        #endregion
    }
}