using InvoiceServer.API.Business;
using InvoiceServer.Common;
using System;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    [RoutePrefix("jobReplaceInvoice")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceController'
    public class JobReplaceInvoiceController : BaseController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceController'
    {
        #region Fields, Properties
        private static readonly Logger logger = new Logger();
        private readonly JobReplaceInvoiceBusiness business;
        #endregion
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceController.JobReplaceInvoiceController()'
        public JobReplaceInvoiceController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceController.JobReplaceInvoiceController()'
        {
            business = new JobReplaceInvoiceBusiness(GetBOFactory());
        }

        #region api method

        [HttpGet]
        [Route("replaceInvoice")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceController.ReplaceInvoice()'
        public void ReplaceInvoice()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceController.ReplaceInvoice()'
        {
            try
            {
                this.business.ReplaceInvoice();
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'JobReplaceInvoiceController.ReplaceInvoiceNew()'
        public bool ReplaceInvoiceNew()
        {
            try
            {
                var result = this.business.ReplaceInvoiceNew();
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return false;
            }
        }

        internal void UpdateProcessJob(string v1, bool v2)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
