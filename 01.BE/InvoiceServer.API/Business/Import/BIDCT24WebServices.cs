using InvoiceServer.API.T24ServiceReference;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Configuration;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'T24WebServices'
    public class T24WebServices
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'T24WebServices'
    {
        private readonly InqueryWebServiceClient client;
        private static readonly Func<string, string> GetConfig = key => WebConfigurationManager.AppSettings[key];

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'T24WebServices.T24WebServices()'
        public T24WebServices()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'T24WebServices.T24WebServices()'
        {
            client = new InqueryWebServiceClient();
            string certificatePath = HttpContext.Current.Server.MapPath(GetConfig("CertificatePart"));
            X509Certificate2 cer = new X509Certificate2(certificatePath);
            client.ClientCredentials.ServiceCertificate.DefaultCertificate = cer;
            client.ClientCredentials.ClientCertificate.Certificate = cer;
            client.ClientCredentials.UserName.UserName = GetConfig("T24WebServiceUsername");
            client.ClientCredentials.UserName.Password = GetConfig("T24WebServicePassword");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'T24WebServices.Get(string)'
        public string Get(string strXml)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'T24WebServices.Get(string)'
        {
            string outputString = string.Empty;
            outputString = client.InqueryService(strXml);
            return outputString;
        }
    }
}