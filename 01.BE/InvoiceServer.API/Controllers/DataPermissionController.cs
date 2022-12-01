using InvoiceServer.API.Business;
using InvoiceServer.API.Constants;
using InvoiceServer.Business.Constants;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("datapermissions")]
    public class DataPermissionController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly DataPermissionBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

        public DataPermissionController()
        {
            business = new DataPermissionBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
        public IHttpActionResult GetDataPermission()
        {
            var response = new ApiResultList<DataPermissionInfo>();
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
        #endregion API methods
    }
}