using InvoiceServer.Business.Extensions;
using InvoiceServer.Business.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;  // for class Encoding

namespace InvoiceServer.Business.ExternalData
{
    public class ClientApiService
    {
        const string ApiGetCompany = "https://dichvuthongtin.dkkd.gov.vn/auth/Public/Srv.aspx/GetSearch";
        const string ApiBaseUrl = "https://dichvuthongtin.dkkd.gov.vn/inf/default.aspx";
        private readonly string hdParameter;
        public ClientApiService()
        {
            HttpClient httpClient = new HttpClient();
            var result = httpClient.GetStringAsync(ApiBaseUrl).Result;
            //get hdParameter
            int startFrom = result.IndexOf("ctl00$hdParameter");
            int startTo = result.IndexOf("/>", startFrom);
            hdParameter = result.Substring(startFrom, (startTo - startFrom)).TrimEnd();
            startFrom = hdParameter.IndexOf("value=");
            hdParameter = hdParameter.Substring(startFrom + 7, hdParameter.Length - startFrom - 8);
        }

        public ExternalCustomer GetCustomerInfo(string taxCode)
        {
            HttpClient httpClient = new HttpClient();
            string myJson = "{'searchField': '" + taxCode + "','h': '" + hdParameter + "'}";
            var response = httpClient.PostAsync(ApiGetCompany, new StringContent(myJson, Encoding.UTF8, "application/json")).Result;
            response.CustomEnsureSuccessStatusCode();
            response.EnsureSuccessStatusCode();
            var responseString = response.Content.ReadAsStringAsync().Result;
            ExternalCustomer2 customer2 = JsonConvert.DeserializeObject<ExternalCustomer2>(responseString);
            ExternalCustomer customer = null;
            if (customer2.d.Count > 0 && customer2.d.FindAll(p => p.Enterprise_Gdt_Code == taxCode).Count > 0)
            {
                customer = customer2.d.FindAll(p => p.Enterprise_Gdt_Code == taxCode)[0];
                customer.MaSoThue = customer.Enterprise_Gdt_Code;
                customer.Title = customer.Name;
                customer.TitleEn = customer.Name_F;
                customer.DiaChiCongTy = customer.Ho_Address;
                customer.GiamDoc = customer.Legal_First_Name;
                customer.NoiNopThue_DienThoai = "";
                customer.NoiNopThue_Fax = "";
            }
            return customer;
        }
    }
}
