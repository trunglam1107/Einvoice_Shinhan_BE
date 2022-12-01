using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Account;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("accounts")]
    public class AccountController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly AccountBusiness business;
        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.AccountController()'
        public AccountController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.AccountController()'
        {
            business = new AccountBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods

        
#pragma warning disable CS1572 // XML comment has a param tag for 'loginInfo', but there is no parameter by that name
/// <summary>
        /// Request login to new session
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        [HttpPost]
#pragma warning restore CS1572 // XML comment has a param tag for 'loginInfo', but there is no parameter by that name
        [Route("{id}/change-password")]
        [AllowAnonymous]
#pragma warning disable CS1573 // Parameter 'id' has no matching param tag in the XML comment for 'AccountController.ChangePassword(long, PasswordInfo)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'passwordInfo' has no matching param tag in the XML comment for 'AccountController.ChangePassword(long, PasswordInfo)' (but other parameters do)
        public IHttpActionResult ChangePassword(long id, PasswordInfo passwordInfo)
#pragma warning restore CS1573 // Parameter 'passwordInfo' has no matching param tag in the XML comment for 'AccountController.ChangePassword(long, PasswordInfo)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'id' has no matching param tag in the XML comment for 'AccountController.ChangePassword(long, PasswordInfo)' (but other parameters do)
        {
            if (!ModelState.IsValid || passwordInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.ChangePassword(id, passwordInfo);
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
        [Route("change-language")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.ChangeLanguage(ChangeLanguage)'
        public IHttpActionResult ChangeLanguage(ChangeLanguage input)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.ChangeLanguage(ChangeLanguage)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.ChangeLanguage(input.Language);
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
        [Route("{keyword?}/{roleId?}/{branchId?}/{skip?}/{take?}/{branch?}")]
        [CustomAuthorize(Roles = UserPermission.AccountManagement_Read + ", " + UserPermission.RoleManagement_Update
            + ", " + UserPermission.EmployeesManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.FillterUser(string, long?, string, string, string, int, int, string)'
        public IHttpActionResult FillterUser(string keyword = null, long? roleId = null, string branchId = null,string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue, string branch = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.FillterUser(string, long?, string, string, string, int, int, string)'
        {
            var response = new ApiResultList<AccountDetail>();
            try
            {
                var condition = new ConditionSearchUser(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    Keyword = keyword.DecodeUrl(),
                    RoleId = roleId,
                    Branch = branch.ToInt(),
                    ChildLevels = new string[] { RoleInfo.BRANCH },
                    BranchId = branchId
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterUser(condition);
                response.Message = MsgApiResponse.ExecuteSeccessful;
                Dictionary<string, string> responHeaders
                     = new Dictionary<string, string>(){
                     {CustomHttpRequestHeader.AccessControlExposeHeaders, "X-Collection-Total, X-Collection-Skip, X-Collection-Take"},
                     {CustomHttpRequestHeader.CollectionTotal, condition.TotalRecords.ToString()},
                     {CustomHttpRequestHeader.CollectionSkip, skip.ToString()},
                     {CustomHttpRequestHeader.CollectionTake, take.ToString()},
                };

                SetResponseHeaders(responHeaders);
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
        [Route("out-role/{keyword?}/{roleId?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.AccountManagement_Read + ", " + UserPermission.RoleManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.FillterUserOutRole(string, long?, string, string, int, int)'
        public IHttpActionResult FillterUserOutRole(string keyword = null, long? roleId = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.FillterUserOutRole(string, long?, string, string, int, int)'
        {
            var response = new ApiResultList<AccountDetailLogin>();
            try
            {
                var condition = new ConditionSearchUser(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    Keyword = keyword.DecodeUrl(),
                    RoleId = roleId,
                    ChildLevels = new string[] { RoleInfo.BRANCH },
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterUserOutRole(condition);
                response.Message = MsgApiResponse.ExecuteSeccessful;
                Dictionary<string, string> responHeaders
                     = new Dictionary<string, string>(){
                     {CustomHttpRequestHeader.AccessControlExposeHeaders, "X-Collection-Total, X-Collection-Skip, X-Collection-Take"},
                     {CustomHttpRequestHeader.CollectionTotal, condition.TotalRecords.ToString()},
                     {CustomHttpRequestHeader.CollectionSkip, skip.ToString()},
                     {CustomHttpRequestHeader.CollectionTake, take.ToString()},
                };

                SetResponseHeaders(responHeaders);
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
        [CustomAuthorize(Roles = UserPermission.AccountManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetAccoutInfo(long)'
        public IHttpActionResult GetAccoutInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetAccoutInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<AccountInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetAccoutInfo(id);
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
        [Route("uploadAccount")]
        [CustomAuthorize(Roles = UserPermission.AccountManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.UploadData(FileImport)'
        public IHttpActionResult UploadData(FileImport fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.UploadData(FileImport)'
        {
            if (fileImport == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ResultImportSheet>();

            try
            {
                var resultImport = this.business.ImportFile(fileImport.Data);
                response.Message = resultImport.Message;
                response.Code = resultImport.ErrorCode;
                response.Data = resultImport;
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
        [Route("upload")]
        [CustomAuthorize(Roles = UserPermission.UploadInvoice_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.UploadData(UploadInvoice)'
        public IHttpActionResult UploadData(UploadInvoice fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.UploadData(UploadInvoice)'
        {
            if (fileImport == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ResultImportSheet>();

            try
            {
                var resultImport = this.business.ImportData(fileImport);
                response.Message = resultImport.Message;
                response.Code = resultImport.ErrorCode;
                response.Data = resultImport;
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
        [Route("")]
        [CustomAuthorize(Roles = UserPermission.AccountManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.CreateUser(AccountInfo)'
        public IHttpActionResult CreateUser(AccountInfo userInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.CreateUser(AccountInfo)'
        {
            if (!ModelState.IsValid || userInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.CreateUser(userInfo);
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
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.AccountManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.UpdateUser(long, AccountInfo)'
        public IHttpActionResult UpdateUser(long id, AccountInfo userInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.UpdateUser(long, AccountInfo)'
        {
            if (!ModelState.IsValid || userInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();
            try
            {
                response.Code = this.business.UpdateUser(id, userInfo);
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

        [HttpDelete]
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.AccountManagement_Delete + ", " + UserPermission.EmployeesManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.DeleteUser(long)'
        public IHttpActionResult DeleteUser(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.DeleteUser(long)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.DeleteUser(id);
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
        [Route("customer-management/{customerCode?}/{taxCode?}/{userId?}/{userName?}/{email?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.ClientAccountManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetListAccount(string, string, string, string, string, string, string, int, int)'
        public IHttpActionResult GetListAccount(string customerCode = null, string taxCode = null, string userId = null, string userName = null, string email = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetListAccount(string, string, string, string, string, string, string, int, int)'
        {

            var response = new ApiResult<IEnumerable<AccountViewModel>>();

            try
            {
                var condition = new ConditionSearchUser(orderBy, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    CustomerCode = customerCode.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    UserId = userId.DecodeUrl(),
                    UserName = userName.DecodeUrl(),
                    Email = email.DecodeUrl(),
                };
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetListAccount(condition);

                Dictionary<string, string> responHeaders
                     = new Dictionary<string, string>(){
                     {CustomHttpRequestHeader.AccessControlExposeHeaders, "X-Collection-Total, X-Collection-Skip, X-Collection-Take"},
                     {CustomHttpRequestHeader.CollectionTotal, condition.TotalRecords.ToString()},
                     {CustomHttpRequestHeader.CollectionSkip, skip.ToString()},
                     {CustomHttpRequestHeader.CollectionTake, take.ToString()},
                };

                SetResponseHeaders(responHeaders);
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
        [Route("customer-managementById/{id}")]
        [CustomAuthorize(Roles = UserPermission.ClientAccountManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetAccountById(long)'
        public IHttpActionResult GetAccountById(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetAccountById(long)'
        {
            var response = new ApiResult<AccountViewModel>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetAccountById(id);
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
        [Route("downloadClientAccount/{customerCode?}/{taxCode?}/{userId?}/{userName?}/{email?}/{skip?}/{take?}")]
        [ResponseType(typeof(InvoiceFile))]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.DownloadClientAccount(string, string, string, string, string, int, int)'
        public IHttpActionResult DownloadClientAccount(string customerCode = null, string taxCode = null, string userId = null, string userName = null, string email = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.DownloadClientAccount(string, string, string, string, string, int, int)'
        {
            try
            {
                var condition = new ConditionSearchUser(null, null)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    CustomerCode = customerCode.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    UserId = userId.DecodeUrl(),
                    UserName = userName.DecodeUrl(),
                    Email = email.DecodeUrl(),
                };
                ExportFileInfo fileInfo = this.business.DownloadClientAccount(condition);
                if (!File.Exists(fileInfo.FullPathFileName))
                {
                    return NotFound();
                }

                InvoiceFile file = new InvoiceFile()
                {
                    FilePath = fileInfo.FullPathFileName,
                    FileName = fileInfo.FileName,
                };

                SetResponseHeaders("Content-Disposition", "inline; filename=" + fileInfo.FileName);
                return Ok(file);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file");
            }
        }

        [HttpPost]
        [Route("reset-password")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.ResetPassword(ResetPassword)'
        public async Task<IHttpActionResult> ResetPassword(ResetPassword resetPasswordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.ResetPassword(ResetPassword)'
        {
            if (!ModelState.IsValid || resetPasswordInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();
            try
            {
                response.Code = await this.business.ResetPassword(resetPasswordInfo) ? ResultCode.NoError : ResultCode.UnknownError;
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
        [Route("reset-clientpassword")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.ResetClientPassword(ResetPassword)'
        public IHttpActionResult ResetClientPassword(ResetPassword resetPasswordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.ResetClientPassword(ResetPassword)'
        {
            if (!ModelState.IsValid || resetPasswordInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();
            try
            {
                this.business.ResetClientPassword(resetPasswordInfo);
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(resetPasswordInfo.UserId, ex);
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