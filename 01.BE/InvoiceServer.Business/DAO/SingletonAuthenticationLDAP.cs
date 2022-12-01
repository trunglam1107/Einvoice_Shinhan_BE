using InvoiceServer.Common;
using System;
using System.DirectoryServices;

namespace InvoiceServer.Business.DAO
{
    public class SingletonAuthenticationLdap
    {
        SingletonAuthenticationLdap() { }
        private static readonly Lazy<SingletonAuthenticationLdap> lazy = new Lazy<SingletonAuthenticationLdap>(() => new SingletonAuthenticationLdap());
        public static SingletonAuthenticationLdap Instance
        {
            get
            {
                return lazy.Value;
            }
        }
        public string RootPath { get; set; }
        public bool IsAuthenticated(string username, string password)
        {
            DirectoryEntry entry = new DirectoryEntry(RootPath, username, password);
            entry.AuthenticationType = AuthenticationTypes.Secure;
            try
            {
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");
                search.FindOne();//fix sonar k được bỏ dòng này
                return true;
            }
            catch (Exception ex)
            {
                Logger logger = new Logger();
                logger.Error("LDAP", ex);
                return false;
            }

        }

    }
}
