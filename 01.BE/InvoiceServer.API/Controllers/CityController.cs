using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("cities")]
    public class CityController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly CityBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CityController.CityController()'
        public CityController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CityController.CityController()'
        {
            business = new CityBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CityController.GetCity()'
        public IHttpActionResult GetCity()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CityController.GetCity()'
        {
            var response = new ApiResultList<CityInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetList();
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
        [Route("{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CityController.GetCity(long)'
        public IHttpActionResult GetCity(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CityController.GetCity(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<CityInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetById(id);
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

        #endregion API methods
    }
}