using InvoiceServer.Business.Extensions;
using InvoiceServer.Common;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace InvoiceServer.Business.ExternalData
{
    public class RestClient : HttpClient
    {
        private static readonly Logger logger = new Logger();
        public RestClient(string baseApiUri)
        {
            if (string.IsNullOrEmpty(baseApiUri))
            {
                throw new ArgumentNullException("baseApiUri");
            }

            this.BaseAddress = new Uri(baseApiUri);
            this.DefaultRequestHeaders.Accept.Clear();
            this.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public T Get<T>(string requestUri)
            where T : class
        {
            HttpResponseMessage response = this.GetAsync(requestUri).Result;

            // Throw exception if HTTP status code is not Success (2xx)
            response.CustomEnsureSuccessStatusCode();

            var resultData = response.Content.ReadAsAsync<T>().Result;
            logger.Trace("API Get {0}, response body {1}", requestUri, JsonConvert.SerializeObject(resultData));

            return resultData;
        }

        public U Post<T, U>(string requestUri, T t)
            where T : class
            where U : class
        {
            HttpResponseMessage response = this.PostAsJsonAsync<T>(requestUri, t).Result;

            // Throw exception if HTTP status code is not Success (2xx)
            response.CustomEnsureSuccessStatusCode();
            var resultData = response.EnsureSuccessStatusCode();
            logger.Trace("API Post {0}, request body {1}, response body {2}", requestUri, JsonConvert.SerializeObject(t), JsonConvert.SerializeObject(resultData));

            return response.Content.ReadAsAsync<U>().Result;
        }

        public bool Delete(string requestUri)
        {
            HttpResponseMessage response = base.DeleteAsync(requestUri).Result;

            //// Throw exception if HTTP status code is not Success (2xx)
            response.CustomEnsureSuccessStatusCode();

            return response.IsSuccessStatusCode;
        }
    }
}
