using InvoiceServer.Business.BL.Interface;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;


namespace InvoiceServer.Business.BL
{
    class ServicesJobBO : IServicesJobBO
    {
        #region Fields, Properties
        protected readonly static object lockObject = new object();
        private readonly ILoginUserRepository loginUserRepository;
        private readonly bool createTokenRandom = false;
        private readonly IUVGetFunctionOfUserRepository uV_GetFunctionOfUserRepository;
        private readonly ISystemSettingRepository systemSettingRepository;
        private readonly IConfigEmailServerRepository configEmailServerRepository;
        #endregion


        #region Contructor
        public ServicesJobBO(IRepositoryFactory repoFactory)
        {
            this.loginUserRepository = repoFactory.GetRepository<ILoginUserRepository>();
            this.uV_GetFunctionOfUserRepository = repoFactory.GetRepository<IUVGetFunctionOfUserRepository>();
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
            this.configEmailServerRepository = repoFactory.GetRepository<IConfigEmailServerRepository>();
        }

        #endregion
        public UserSessionInfo JobLogin()
        {

            var loginUser = this.loginUserRepository.JobLogin();
            UserSessionInfo userSessionInfo = GetSessionInfo(loginUser);
            userSessionInfo.EmailServer = GetEmailServer();
            return userSessionInfo;
        }
        private UserSessionInfo GetSessionInfo(LOGINUSER loginUser)
        {
            string token = this.createTokenRandom ? Guid.NewGuid().ToString("N") : loginUser.USERID;
            UserSessionInfo userSessionInfo = new UserSessionInfo(loginUser, loginUser.USERLEVEL, loginUser.MYCOMPANY, token);
            var roleOfUser = this.uV_GetFunctionOfUserRepository.GetByUserSID(loginUser.USERSID, loginUser.COMPANYID);
            userSessionInfo.RoleUser.Permissions = GetPermissions(roleOfUser);
            userSessionInfo.EmailServer = GetEmailServer();
            userSessionInfo.SystemSetting = this.systemSettingRepository.FilterSystemSetting(loginUser.COMPANYID ?? 0);
            userSessionInfo.Language = loginUser.LANGUAGE;
            return userSessionInfo;
        }
        private List<string> GetPermissions(IEnumerable<UV_GETFUNCTIONOFUSER> roleOfUsers)
        {
            List<string> permissionOfUser = new List<string>();
            if (roleOfUsers == null)
            {
                return permissionOfUser;
            }

            roleOfUsers.ForEach(p =>
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
        private EmailServerInfo GetEmailServer()
        {
            var emailServer = this.configEmailServerRepository.GetByCompany();
            if (emailServer == null)
            {
                return null;
            }

            return new EmailServerInfo(emailServer);
        }
    }
}
