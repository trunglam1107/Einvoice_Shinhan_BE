using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    [RoutePrefix("quazt")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController'
    public class QuarztJobController : BaseController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController'
    {
        #region Fields, Properties
        private static readonly Logger logger = new Logger();
        private readonly QuarztJobBussiness business;
        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.QuarztJobController()'
        public QuarztJobController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.QuarztJobController()'
        {
            business = new QuarztJobBussiness(GetBOFactory());
        }

        #endregion Contructor

        #region API method
        [HttpGet]
        [Route("GetAll")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.GetAll(string, string, int, int)'
        public IHttpActionResult GetAll(string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.GetAll(string, string, int, int)'

        {
            var response = new ApiResultList<QuarztJob>();
            try
            {
                var condition = new ConditionSearchQuartzJob(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                };
                response.Code = ResultCode.NoError;
                response.Data = business.GetAll(condition);
                response.Message = MsgApiResponse.ExecuteSeccessful;
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
        [HttpGet]
        [Route("getById/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.getById(int)'
        public IHttpActionResult getById(int id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.getById(int)'
        {
            var response = new ApiResult<QuarztJob>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.getById(id);
                response.Message = MsgApiResponse.ExecuteSeccessful;
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

        [HttpPost]
        [Route("onUpdate")]
        [CustomAuthorize(Roles = UserPermission.quarztJobManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onUpdate(QuarztJob)'
        public IHttpActionResult onUpdate(QuarztJob model)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onUpdate(QuarztJob)'
        {
            if (!ModelState.IsValid || model == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult();
            try
            {
                response.Code = business.UpdateCron(model);
                response.Message = MsgApiResponse.ExecuteSeccessful;
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

        [HttpGet]
        [Route("onStart/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onStart(string)'
        public IHttpActionResult onStart(string id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onStart(string)'
        {
            var response = new ApiResult<string>();
            try
            {

                response.Code = ResultCode.NoError;
                response.Data = onStartJob(long.Parse(id)).ToString();
                response.Message = MsgApiResponse.ExecuteSeccessful;


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
        [HttpGet]
        [Route("onPause/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onPause(string)'
        public IHttpActionResult onPause(string id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onPause(string)'
        {
            var response = new ApiResult<string>();
            try
            {

                response.Code = ResultCode.NoError;
                response.Data = onStoptJob(long.Parse(id)).ToString();
                response.Message = MsgApiResponse.ExecuteSeccessful;


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

        #endregion

        #region private method
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onload()'
        public bool onload()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onload()'
        {
            var condition = new ConditionSearchQuartzJob(null, null)
            {
                CurrentUser = this.CurrentUser,
            };
            var list = business.GetAll(condition);
            if (list == null)
            {
                return false;
            }
            else
            {
                foreach (QuarztJob loop in list)
                {
                    business.excuteJob(true, loop);
                    logger.QuarztJob(false, "Khoi tao: " + loop.JOBNAME + " thanh cong");
                    if (!loop.STATUS)
                    {
                        onStoptJob(loop.ID);
                    }
                    this.business.UpdateProcessByName(loop.JOBNAME, false);
                }
                return true;
            }

        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onStartJob(long)'
        public bool onStartJob(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onStartJob(long)'
        {
            bool result = false;
            string jobname = business.findJobName(id);
            try
            {
                business.updateStatus(id, true);
                QuarztSingleton.Instance.onResumeJob(jobname);
                logger.QuarztJob(false, "run Job: " + jobname + " sucessfull");
            }
            catch (Exception ex)
            {
                logger.QuarztJob(true, ex.Message);
            }
            return result;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onStoptJob(long)'
        public bool onStoptJob(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobController.onStoptJob(long)'
        {
            bool result = false;
            string jobname = business.findJobName(id);
            try
            {
                QuarztSingleton.Instance.onStopJob(jobname);
                business.updateStatus(id, false);
                logger.QuarztJob(false, "stop Job: " + jobname + " sucessfull");
                result = true;
            }
            catch (Exception ex)
            {
                logger.QuarztJob(true, ex.Message);
            }
            return result;
        }

        #endregion
    }
}