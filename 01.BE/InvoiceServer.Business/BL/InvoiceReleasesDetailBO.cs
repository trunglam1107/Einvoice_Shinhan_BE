using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.BL
{
    public class InvoiceReleasesDetailBO : IInvoiceReleasesDetailBO
    {
        #region Fields, Properties

        private readonly IInvoiceConcludeDetailRepository invoiceConcludeDetailRepository;

        #endregion

        #region Contructor

        public InvoiceReleasesDetailBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.invoiceConcludeDetailRepository = repoFactory.GetRepository<IInvoiceConcludeDetailRepository>();
        }

        #endregion

        #region Methods



        public InvoiceReleasesDetailInfo GetInvoiceReleasesDetailInfo(long id)
        {
            INVOICERELEASESDETAIL invoiceReleasesDetail = GetInvoiceConcludeDetail(id);
            return new InvoiceReleasesDetailInfo(invoiceReleasesDetail);
        }

        private INVOICERELEASESDETAIL GetInvoiceConcludeDetail(long id)
        {
            INVOICERELEASESDETAIL invoiceReleasesDetail = this.invoiceConcludeDetailRepository.GetById(id);
            if (invoiceReleasesDetail == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.ResouceIdNotFound);
            }

            return invoiceReleasesDetail;

        }

        #endregion
    }
}