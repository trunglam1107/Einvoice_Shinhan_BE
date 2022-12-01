using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class ReleaseInvoiceDetailBO : IReleaseInvoiceDetailBO
    {
        #region Fields, Properties

        private readonly IReleaseInvoiceDetaiRepository releseInvoiceDetailRepository;

        #endregion

        #region Contructor

        public ReleaseInvoiceDetailBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");

            this.releseInvoiceDetailRepository = repoFactory.GetRepository<IReleaseInvoiceDetaiRepository>();
        }

        #endregion

        #region Methods

        public IEnumerable<InvoicesRelease> FilterReleaseInvoice(long releaseId, long companyId, List<int> releaseStatus)
        {
            var releaseInvoiceDetails = this.releseInvoiceDetailRepository.FilteInvoiceRelease(releaseId, companyId, releaseStatus).ToList();
            return releaseInvoiceDetails;
        }

        public Result Create(ReleaseInvoiceMaster info)
        {
            Result resultRelease = new Result();
            return resultRelease;
        }

        public ResultCode Update(long releaseId, StatusRelease status)
        {
            return ResultCode.NoError;
        }

        #endregion
    }
}