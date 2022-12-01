using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using Newtonsoft.Json;
using System.Net.Http;

namespace InvoiceServer.Business.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static void CustomEnsureSuccessStatusCode(this HttpResponseMessage response)
        {

            if (response.IsSuccessStatusCode)
            {
                var strContents = response.Content.ReadAsStringAsync().Result;
                var content = JsonConvert.DeserializeObject<ApiResult>(strContents);
                if (content.Code != 0)
                {
                    throw new BusinessLogicException(content.Code, content.Message);
                }

                return;
            }

            if (response.Content != null)
                response.Content.Dispose();
            throw new BusinessLogicException(ResultCode.UnknownError);
        }
    }
}
