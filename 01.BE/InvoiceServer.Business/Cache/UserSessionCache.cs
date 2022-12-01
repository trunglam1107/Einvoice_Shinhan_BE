using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace InvoiceServer.Business.Cache
{
    public class UserSessionCache
    {
        private static readonly Logger logger = new Logger();

        private readonly System.Web.Caching.Cache innerCache = HttpContext.Current.Cache;
        private static readonly UserSessionCache instance = new UserSessionCache();

        public static UserSessionCache Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Get UserSessionInfo of a login user by token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public UserSessionInfo GetUserSession(string token)
        {
            if (this.innerCache.Get(token) != null)
            {
                return this.innerCache.Get(token) as UserSessionInfo;
            }
            return null;
        }

        /// <summary>
        /// Save user session information
        /// </summary>
        /// <param name="userSessionInfo"></param>
        /// <param name="expireTimeout"></param>
        /// <returns></returns>
        public void SaveUserSession(UserSessionInfo userSessionInfo, TimeSpan expireTimeout)
        {
            if (userSessionInfo == null)
            {
                return;
            }

            var cacheItemRemovedCallback = new CacheItemRemovedCallback(this.CacheItemRemovedCallback);
            this.innerCache.Insert(userSessionInfo.Token,
                                userSessionInfo,
                                null,
                                System.Web.Caching.Cache.NoAbsoluteExpiration,
                                expireTimeout,
                                CacheItemPriority.Default,
                                cacheItemRemovedCallback);
            logger.Trace("[SESSION] LOGIN An user logged-in to system. SessionId: [{0}]; UserId: [{1}].",
                            userSessionInfo.SessionId, userSessionInfo.Id);
        }

        public void SaveMaxUserId(long idCompany, double currentUserId)
        {
            this.innerCache.Insert(idCompany.ToString(),
                                currentUserId,
                                null,
                                System.Web.Caching.Cache.NoAbsoluteExpiration,
                                TimeSpan.FromHours(6),
                                CacheItemPriority.Default,
                                null);
            logger.Trace("[USER ID] Max current id inserted to system. UsertId: [{0}]; Company id: [{1}].",
                            idCompany, currentUserId);

        }

        public void SaveMaxInvoiceNo(long companyId, long templateid, string symbol, double maxInvoiceNo)
        {
            string key = string.Format("{0}_{1}_{2}", companyId, templateid, symbol);
            this.innerCache.Insert(key,
                                maxInvoiceNo,
                                null,
                                System.Web.Caching.Cache.NoAbsoluteExpiration,
                                TimeSpan.FromHours(6),
                                CacheItemPriority.Default,
                                null);
            logger.Trace("[Max Invoice No] Max current id inserted to system. UsertId: [{0}]; Company id: [{1}].",
                            templateid, companyId);

        }

        public double GetMaxInvoiceNo(long companyId, long templateid, string symbol)
        {
            string key = string.Format("{0}_{1}_{2}", companyId, templateid, symbol);
            if (this.innerCache.Get(key) != null)
            {
                return double.Parse(this.innerCache.Get(key).ToString());
            }

            return 0;
        }

        /// <summary>
        /// Remove session of a login user by token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public UserSessionInfo RemoveUserSession(string token)
        {
            if (this.innerCache.Get(token) != null)
            {
                return this.innerCache.Remove(token) as UserSessionInfo;
            }

            return null;
        }

        public IEnumerable<UserSessionInfo> FindUserSessions(string userName)
        {
            var sessionInfos = new List<UserSessionInfo>();
            var cacheEnum = this.innerCache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                var si = cacheEnum.Value as UserSessionInfo;
                if (si == null) continue;

                if (Utility.Equals(si.UserName, userName))
                {
                    sessionInfos.Add(si);
                }
            }

            return sessionInfos;
        }

        /// <summary>
        /// A user has been deleted.
        /// </summary>
        /// <param name="userId">The condition get session</param>
        public void RemoveSessionByUserId(long userId)
        {
            var sessionInfos = UserSessionCache.Instance.FindUserSessionsByUserId(userId).ToList();
            if (sessionInfos == null || sessionInfos.Count == 0)
            {
                return;
            }

            sessionInfos.ForEach(p =>
                 {
                     UserSessionCache.Instance.RemoveUserSession(p.Token);
                     logger.Trace("[LOGOUT] An logged-in user has just deleted from system. SessionId: [{0}]; UserId: [{1}].", p.SessionId, p.UserName);
                 });
        }

        public IEnumerable<UserSessionInfo> FindUserSessionsByUserId(long userId)
        {
            var sessionInfos = new List<UserSessionInfo>();
            var cacheEnum = this.innerCache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                var si = cacheEnum.Value as UserSessionInfo;
                if (si == null) continue;

                if (Utility.Equals(si.Id, userId))
                {
                    sessionInfos.Add(si);
                }
            }

            return sessionInfos;
        }

        public double GetMaxUserId(long idCompany)
        {
            if (this.innerCache.Get(idCompany.ToString()) != null)
            {
                return double.Parse(this.innerCache.Get(idCompany.ToString()).ToString());
            }

            return 0;
        }

        /// <summary>
        /// Callback method which is called when a login session is removed
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="reason"></param>
        private void CacheItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            var sessionInfo = value as UserSessionInfo;
            switch (reason)
            {
                case CacheItemRemovedReason.Expired:
                    {
                        logger.Trace("[SESSION] EXPIRED An user session is timeout. SessionId: [{0}]; UserId: [{1}].",
                                    sessionInfo.SessionId, sessionInfo.UserName);
                        break;
                    }
                case CacheItemRemovedReason.Removed:
                    {
                        logger.Trace("[SESSION] LOGOUT An user session is logged out.  SessionId: [{0}]; UserId: [{1}].",
                                    sessionInfo.SessionId, sessionInfo.UserName);
                        break;
                    }
                default:
                    {
                        logger.Trace("[SESSION] END An user session is end.  SessionId: [{0}]; UserId: [{1}]; Reason: [{2}].",
                                    sessionInfo.SessionId, sessionInfo.UserName, reason);
                        break;
                    }
            }
        }

        public IEnumerable<UserSessionInfo> FindUserSessionsByCompanyId(long companyId)
        {
            var sessionInfos = new List<UserSessionInfo>();
            var cacheEnum = this.innerCache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                var si = cacheEnum.Value as UserSessionInfo;
                if (si == null || si.Company == null) continue;

                if (Utility.Equals(si.Company.Id, companyId))
                {
                    sessionInfos.Add(si);
                }
            }

            return sessionInfos;
        }

        public void UpdateCompanyInfo(long companyId, CompanyInfo newCompanyInfo)
        {
            var cacheEnum = this.innerCache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                var si = cacheEnum.Value as UserSessionInfo;
                if (si == null || si.Company == null) continue;

                if (Utility.Equals(si.Company.Id, companyId))
                {
                    si.Company = newCompanyInfo;
                }
            }

        }
    }
}