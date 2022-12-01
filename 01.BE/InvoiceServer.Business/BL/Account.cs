using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace InvoiceServer.Business.BL
{
    public class Account
    {
        private readonly ILoginUserRepository loginRepository;
        private readonly IUserLevelRepository userLevelRepository;
        private readonly Permission permisson;
        public Account(IRepositoryFactory repoFactory)
        {
            this.loginRepository = repoFactory.GetRepository<ILoginUserRepository>();
            this.userLevelRepository = repoFactory.GetRepository<IUserLevelRepository>();
            this.permisson = new Permission(repoFactory);
        }

        public void CreateAccountClient(CLIENT clientInfo)
        {
            if (clientInfo.CUSTOMERTYPE != (int)CustomerType.IsAccounting || OtherExtensions.IsCurrentClient(clientInfo.CUSTOMERCODE))
            {
                return;
            }
            ContractAccount account = BuildClientAccount(clientInfo);
            var loginUser = CreateLogin(account);
            var functionByRole = this.permisson.FunctionByLevel(account.Level_Role);
            this.permisson.CreatePermission(loginUser, functionByRole);
        }
        public void CreateAccountListClient(List<CLIENT> clients)
        {
            var accounts = BuildListClientAccount(clients);
            var loginUsers = CreateLoginListClient(accounts);
            var functionByRole = this.permisson.FunctionByLevel(RoleInfo.CLIENT);
            foreach (var loginUser in loginUsers)
            {
                this.permisson.CreatePermission(loginUser, functionByRole);
            }
        }

        public void UpdateAccountClient(CLIENT clientInfo)
        {
            if (clientInfo.CUSTOMERTYPE != (int)CustomerType.IsAccounting || OtherExtensions.IsCurrentClient(clientInfo.CUSTOMERCODE))
            {
                return;
            }

            var loginUserOfClient = this.loginRepository.GetByClientId(clientInfo.ID);
            if (loginUserOfClient == null)
            {
                ContractAccount account = BuildClientAccount(clientInfo);
                var loginUser = CreateLogin(account);
                var functionByRole = this.permisson.FunctionByLevel(account.Level_Role);
                this.permisson.CreatePermission(loginUser, functionByRole);
            }
            else
            {
                string emailUpdate = (clientInfo.USEREGISTEREMAIL == true ? clientInfo.EMAIL : clientInfo.RECEIVEDINVOICEEMAIL) ?? "";
                if (!emailUpdate.Trim().ToLower().Equals(loginUserOfClient.EMAIL ?? "".Trim().ToLower()))
                {
                    loginUserOfClient.EMAIL = emailUpdate;
                    this.loginRepository.Update(loginUserOfClient);
                }
            }
        }

        public bool UpdateClientPassword(long clientId)
        {
            var loginUser = this.loginRepository.GetByClientId(clientId);
            //encrypt password
            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(loginUser.PASSWORD));
            byte[] resultHash = md5.Hash;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < resultHash.Length; i++)
            {
                //change it into 2 hexadecimal digits for each byte  
                strBuilder.Append(resultHash[i].ToString("x2"));
            }
            loginUser.PASSWORD = strBuilder.ToString();
            //password da duoc ma hoa
            loginUser.PASSWORDSTATUS = (int)PasswordStatus.Encripted;
            return this.loginRepository.Update(loginUser);
        }

        private ContractAccount BuildClientAccount(CLIENT clientInfo)
        {
            string loginId = clientInfo.TAXCODE;
            ContractAccount account = new ContractAccount();
            account.UserName = clientInfo.ISORG != true ? clientInfo.PERSONCONTACT : clientInfo.CUSTOMERNAME;
            account.Email = clientInfo.USEREGISTEREMAIL == true ? clientInfo.EMAIL : clientInfo.RECEIVEDINVOICEEMAIL;
            account.Password = CretePassword();
            account.ClientId = clientInfo.ID;
            //password chua ma hoa, dung de gui cho client
            account.PassWordStatus = 1;

            if (loginId.IsNullOrEmpty())
            {
                loginId = account.Password;
            }
            account.UserId = GetUserId(loginId);
            account.Level_Role = RoleInfo.CLIENT;
            return account;
        }
        private List<ContractAccount> BuildListClientAccount(List<CLIENT> clients)
        {
            var listContractAccount = new List<ContractAccount>();
            foreach (var client in clients)
            {
                if (OtherExtensions.IsCurrentClient(client.CUSTOMERCODE))   // Khách vãng lai thì không cần tạo tài khoản
                {
                    continue;
                }
                string loginId = client.TAXCODE;
                ContractAccount account = new ContractAccount();
                account.UserName = client.ISORG != true ? client.PERSONCONTACT : client.CUSTOMERNAME;
                account.Email = client.USEREGISTEREMAIL == true ? client.EMAIL : client.RECEIVEDINVOICEEMAIL;
                account.Password = CretePassword();
                account.ClientId = client.ID;
                //password chua ma hoa, dung de gui cho client
                account.PassWordStatus = 1;

                if (loginId.IsNullOrEmpty())
                {
                    loginId = account.Password;
                }
                account.UserId = GetUserId(loginId, listContractAccount);
                account.Level_Role = RoleInfo.CLIENT;
                listContractAccount.Add(account);
            }
            return listContractAccount;
        }

        private bool UserIsExisted(string userId, List<ContractAccount> contractAccounts = null)
        {
            var userIdExistedInListImport = false;
            if (contractAccounts != null && contractAccounts.Count > 0 && contractAccounts.Any(x => x.UserId == userId))
            {
                userIdExistedInListImport = true;
            }
            bool isExitUserId = this.loginRepository.ContainUserId(userId);
            if (isExitUserId || userIdExistedInListImport)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceUserId,
                                  string.Format("Create User failed because user with UserId = [{0}] is exist.", userId));
            }

            return isExitUserId;
        }

        private string CretePassword()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
        }

        private string GetUserId(string loginId, List<ContractAccount> contractAccounts = null)
        {
            string userId = loginId.ToAscii();
            if (userId.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.ContractLoginUserIsEmpty, "Tax code or Delegate is null or empty");
            }

            return MergeAccountWithDatabase(userId, contractAccounts);
        }

        private string MergeAccountWithDatabase(string userId, List<ContractAccount> contractAccounts = null)
        {
            string newUserId = userId;
            int numberReCheckAcount = 20000;
            long nextId = 0;
            do
            {
                --numberReCheckAcount;
                try
                {
                    if (!UserIsExisted(newUserId, contractAccounts))
                    {
                        return newUserId;
                    }
                }
                catch (Exception)
                {
                    nextId = nextId + 1;
                    newUserId = string.Format("{0}_{1}", userId, nextId);
                }

            } while (numberReCheckAcount > 0);

            return newUserId;
        }

        private LOGINUSER CreateLogin(ContractAccount account)
        {
            USERLEVEL role = GetUserLevel(account.Level_Role);
            return CreateLoginUser(account, role);
        }
        private List<LOGINUSER> CreateLoginListClient(List<ContractAccount> accounts)
        {
            USERLEVEL role = GetUserLevel(RoleInfo.CLIENT);
            return CreateLoginListUser(accounts, role);
        }

        private USERLEVEL GetUserLevel(string level)
        {
            var userLevel = this.userLevelRepository.FillterUserLevel(level).FirstOrDefault();
            if (userLevel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return userLevel;
        }

        private LOGINUSER CreateLoginUser(ContractAccount userInfo, USERLEVEL userLevel)
        {
            UserIsExisted(userInfo.UserId);
            var loginUserInfo = new LOGINUSER();
            loginUserInfo.CopyData(userInfo);
            loginUserInfo.USERLEVELSID = userLevel.USERLEVELSID;
            loginUserInfo.ISACTIVE = true;
            loginUserInfo.CLIENTID = userInfo.ClientId;
            loginUserInfo.CREATEDDATE = DateTime.Now;
            this.loginRepository.Insert(loginUserInfo);
            return loginUserInfo;
        }
        private List<LOGINUSER> CreateLoginListUser(List<ContractAccount> userInfos, USERLEVEL userLevel)
        {
            List<LOGINUSER> loginUsers = new List<LOGINUSER>();
            foreach (var userInfo in userInfos)
            {
                UserIsExisted(userInfo.UserId);
                var loginUserInfo = new LOGINUSER();
                loginUserInfo.CopyData(userInfo);
                loginUserInfo.USERLEVELSID = userLevel.USERLEVELSID;
                loginUserInfo.ISACTIVE = true;
                loginUserInfo.CLIENTID = userInfo.ClientId;
                loginUserInfo.CREATEDDATE = DateTime.Now;
                loginUsers.Add(loginUserInfo);
            }
            var res = this.loginRepository.AddRange(loginUsers);
            return res;
        }

    }
}