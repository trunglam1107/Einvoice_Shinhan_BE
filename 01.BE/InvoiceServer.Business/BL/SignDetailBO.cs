using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using System;

namespace InvoiceServer.Business.BL
{
    public class SignDetailBO : ISignDetailBO
    {
        #region Fields, Properties
        private readonly ISignDetailRepository signDetailRepository;
        #endregion

        #region Contructor
        public SignDetailBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.signDetailRepository = repoFactory.GetRepository<ISignDetailRepository>();
        }

        public SIGNDETAIL GetByInvoiceId(int invoiceId, bool isClientSign = false)
        {
            var signDetail = this.signDetailRepository.GetByInvoiceId(invoiceId, isClientSign);
            return signDetail;
        }

        public SIGNDETAIL GetByAnnouncementId(int announcementId, bool isClientSign = false)
        {
            var signDetail = this.signDetailRepository.GetByAnnouncementId(announcementId, isClientSign);
            return signDetail;
        }

        public ResultCode Create(SignDetailInfo signDetailInfo)
        {
            if (signDetailInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            SIGNDETAIL signDetail = new SIGNDETAIL();
            signDetail.CopyData(signDetailInfo);
            signDetail.CREATEDDATE = DateTime.Now;
            bool result = this.signDetailRepository.Insert(signDetail);
            return result ? ResultCode.NoError : ResultCode.UnknownError;
        }

        #endregion
    }
}