using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignDetailBusiness'
    public class SignDetailBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignDetailBusiness'
    {
        #region Fields, Properties

        private readonly ISignDetailBO signDetailBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignDetailBusiness.SignDetailBusiness(IBOFactory)'
        public SignDetailBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignDetailBusiness.SignDetailBusiness(IBOFactory)'
        {
            this.signDetailBO = boFactory.GetBO<ISignDetailBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignDetailBusiness.Create(SignDetailInfo)'
        public ResultCode Create(SignDetailInfo signDetailInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignDetailBusiness.Create(SignDetailInfo)'
        {
            if (signDetailInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode result = this.signDetailBO.Create(signDetailInfo);
            return result;
        }

        #endregion
    }
}