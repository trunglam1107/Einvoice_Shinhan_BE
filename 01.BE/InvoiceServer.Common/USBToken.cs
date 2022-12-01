using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace InvoiceServer.Common
{
    public sealed class UsbToken
    {
        private UsbToken() { }
        private static readonly Lazy<UsbToken> lazy = new Lazy<UsbToken>(() => new UsbToken());
        public static UsbToken Instance
        {
            get { return lazy.Value; }
        }
        public static string Password { get; set; }
        public static List<string> lst { get; set; }
        public string DectectUSBToken()
        {
            lst = lst.ConvertAll(p => p.ToLower());
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            string result = null;
            store.Open(OpenFlags.MaxAllowed);
            foreach (var cer in store.Certificates)
            {
                if (!lst.Contains(cer.SerialNumber.ToLower()) && Verify(cer, Password))
                {
                    result = cer?.SerialNumber;
                }
            }

            store.Close();
            return result;

        }
        private bool Verify(X509Certificate2 cert, string Password)
        {
            try
            {
                var pass = new SecureString();
                char[] array = Password.ToCharArray();
                foreach (char ch in array)
                {
                    pass.AppendChar(ch);
                }
                var privateKey = cert.PrivateKey as RSACryptoServiceProvider;

                CspParameters cspParameters = new CspParameters(privateKey.CspKeyContainerInfo.ProviderType,
                 privateKey.CspKeyContainerInfo.ProviderName,
                 privateKey.CspKeyContainerInfo.KeyContainerName,
                 null,
                 pass);
                var rsa = new RSACryptoServiceProvider(cspParameters);
                if (rsa != null)
                    return true;
            }
            catch
            {
                return false;
            }
            return true;
        }

    }
}
