using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("companies")]
    public class CompanyController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly CompanyBusiness business;

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.CompanyController()'
        public CompanyController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.CompanyController()'
        {
            business = new CompanyBusiness(GetBOFactory());
        }

        #endregion

        [HttpGet]
        [Route("")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetList()'
        public IHttpActionResult GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetList()'
        {
            var response = new ApiResultList<MyCompanyInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetListMyCompanyInfo();
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

        #region API methods
        [HttpGet]
        [Route("{keyWord?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.CompanyManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.FillterCompany(string, string, string, int, int)'
        public IHttpActionResult FillterCompany(string keyword = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.FillterCompany(string, string, string, int, int)'
        {
            var response = new ApiResultList<MasterCompanyInfo>();
            try
            {
                var condition = new ConditionSearchCompany(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    Keyword = keyword.DecodeUrl(),
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterCompanies(condition);
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
        [AllowAnonymous]
        //[CustomAuthorize(Roles = UserPermission.CompanyManagement_Read)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetCompanyInfo(long)'
        public IHttpActionResult GetCompanyInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetCompanyInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<CompanyInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetCompanyInfo(id);
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
        [Route("using/{id}")]
        [AllowAnonymous]
        //[CustomAuthorize(Roles = UserPermission.CompanyManagement_Read)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.MyCompanyUsing(long)'
        public IHttpActionResult MyCompanyUsing(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.MyCompanyUsing(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<MyUsingModel>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.MyCompanyUsing(id);
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
        [Route("companylogin")]
        //[CustomAuthorize(Roles = UserPermission.CompanyManagement_Read)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetCompanyLogin()'
        public IHttpActionResult GetCompanyLogin()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetCompanyLogin()'

        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<CompanyInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetCompanyLogin();
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
        [CustomAuthorize(Roles = UserPermission.CompanyManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.Create(CompanyInfo)'
        public IHttpActionResult Create(CompanyInfo companyInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.Create(CompanyInfo)'
        {
            if (!ModelState.IsValid || companyInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.CreateCompanyInfo(companyInfo);
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
        [CustomAuthorize(Roles = UserPermission.CompanyManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.Update(long, CompanyInfo)'
        public IHttpActionResult Update(long id, CompanyInfo companyInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.Update(long, CompanyInfo)'
        {
            if (!ModelState.IsValid || companyInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.UpdateCompanyInfo(id, companyInfo);
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
        [CustomAuthorize(Roles = UserPermission.CompanyManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.Delete(long)'
        public IHttpActionResult Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.Delete(long)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.Delete(id);
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
        [Route("mycompany/{id}")]
        [CustomAuthorize(Roles = UserPermission.DivisionInformation_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetMyCompanyInfo(long)'
        public IHttpActionResult GetMyCompanyInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetMyCompanyInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<MyCompanyInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetMyCompanyInfo(id);
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
        [Route("mycompanyAnnoun/{id}")]
        [CustomAuthorize(Roles = UserPermission.AnnouncementManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetMyCompanyInfoByAnnoun(long)'
        public IHttpActionResult GetMyCompanyInfoByAnnoun(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetMyCompanyInfoByAnnoun(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<MyCompanyInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetMyCompanyInfo(id);
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
        [Route("get-company-child/{id?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetCompanyChild(long)'
        public IHttpActionResult GetCompanyChild(long id = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetCompanyChild(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResultList<CompanyInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = business.GetCompanyChill(id);
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
        [Route("updatemycompany/{id}")]
        [CustomAuthorize(Roles = UserPermission.DivisionInformation_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.UpdateMyCompany(long, MyCompanyInfo)'
        public IHttpActionResult UpdateMyCompany(long id, MyCompanyInfo companyInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.UpdateMyCompany(long, MyCompanyInfo)'
        {
            if (!ModelState.IsValid || companyInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.UpdateMyCompany(id, companyInfo);
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
        [Route("get-footer-info")]
        [AllowAnonymous]
        // Get từ web.config để insert vào footer phần xem hóa đơn mẫu
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetFooterInfo()'
        public IHttpActionResult GetFooterInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetFooterInfo()'
        {
            var response = new ApiResult<FooterInvoiceInfo>();

            try
            {
                response.Data = this.business.GetFooterInfo();
                response.Message = MsgApiResponse.ExecuteSeccessful;
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
                response.Message = MsgInternalServerError;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return Ok(response);
        }

        [HttpGet]
        [Route("get-header-info")]
        [AllowAnonymous]
        // Get phần tên công ty của Hua Nan Bank
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetHeaderInfo()'
        public IHttpActionResult GetHeaderInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyController.GetHeaderInfo()'
        {
            var response = new ApiResult<dynamic>();

            try
            {
                response.Data = this.business.GetHeaderInfo();
                response.Message = MsgApiResponse.ExecuteSeccessful;
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
                response.Message = MsgInternalServerError;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return Ok(response);
        }

        #endregion
    }
}