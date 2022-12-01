using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;

namespace InvoiceServer.API.Controllers
{
    [RoutePrefix("service")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ServiceSendByMonthController'
    public class ServiceSendByMonthController : BaseController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ServiceSendByMonthController'
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly ReleaseInvoiceBusiness business;
        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ServiceSendByMonthController.ServiceSendByMonthController()'
        public ServiceSendByMonthController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ServiceSendByMonthController.ServiceSendByMonthController()'
        {
            business = new ReleaseInvoiceBusiness(GetBOFactory(), true);
        }

        #endregion Contructor
        #region API methods
        [HttpGet]
        [Route("sendbymonth")]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ServiceSendByMonthController.ServiceCallServerSign()'
        public IHttpActionResult ServiceCallServerSign()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ServiceSendByMonthController.ServiceCallServerSign()'
        {

            var response = new ApiResult<ServerSignResult>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                this.business.ServiceSendMailByMonth();
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return Ok(response);
        }
        [Route("createInvoice")]
        [AllowAnonymous]
        [ResponseType(typeof(HttpResponseMessage))]
        [HttpPost]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ServiceSendByMonthController.XmlImportInvoice(XmlElement)'
        public async Task<HttpResponseMessage> XmlImportInvoice([FromBody] XmlElement value)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ServiceSendByMonthController.XmlImportInvoice(XmlElement)'
        {
            var response = new ApiResult();
            string resultInvoice = String.Empty;
            string invoiceInfo = String.Empty;
            try
            {
                var stream = await this.Request.Content.ReadAsStreamAsync();
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                StreamReader reader = new StreamReader(stream);
                string textInvoice = reader.ReadToEnd();
                invoiceInfo = this.business.ImportInvoiceXml(textInvoice);
                response.Message = "Successfuly";
                response.Code = ResultCode.NoError;

            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = ex.Message;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            resultInvoice = "<ResultInvoice>" +
                                "<Status>" + response.Code + "</Status>" +
                                "<Message>" + response.Message + "</Message>" +
                                invoiceInfo +
                            "</ResultInvoice>";
            return new HttpResponseMessage()
            {
                Content = new StringContent(resultInvoice, Encoding.UTF8, "application/xml")
            };
        }
        #endregion
    }
}
