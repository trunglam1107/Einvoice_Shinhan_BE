using InvoiceServer.Business.BL;
using InvoiceServer.Business.Constants;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
    public class SellerBusiness : BaseBusiness
    {
        #region Fields, Properties

        private ISellerBO sellerBO;

        #endregion Fields, Properties

        #region Contructor

        public SellerBusiness(IBOFactory boFactory)
        {
            this.sellerBO = boFactory.GetBO<ISellerBO>();
        }

        #endregion Contructor

        #region Methods

        public IEnumerable<SellerInfo> FillterSellers(out int totalRecords, string sellerName = null, string mobile = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
        {
            var condition = new ConditionSearchSeller(this.CurrentUser, sellerName, mobile, orderType, orderby);
            totalRecords = this.sellerBO.CountFillterSeller(condition);
            var sellers = this.sellerBO.FilterSeller(condition, skip, take);
            return sellers;
        }

        public SellerInfo GetSellerInfo(int companyId)
        {
            return this.sellerBO.GetSellerInfo(companyId, RoleInfo.SystemAdmin);
        }

        public ResultCode Create(SellerInfo sellerInfo)
        {
            if (sellerInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            sellerInfo.SessionInfo = this.CurrentUser;
            ResultCode result = this.sellerBO.Create(sellerInfo);
            return result;
        }

        public ResultCode Update(int companyId, SellerInfo sellerInfo)
        {
            if (sellerInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            sellerInfo.SessionInfo = this.CurrentUser;
            return this.sellerBO.Update(companyId, sellerInfo);
        }

        public ResultCode Delete(int id)
        {
            string roleOfCurrentUser = this.CurrentUser.RoleUser.Level;
            return this.sellerBO.Delete(id, roleOfCurrentUser);
        }
        #endregion
    }
}