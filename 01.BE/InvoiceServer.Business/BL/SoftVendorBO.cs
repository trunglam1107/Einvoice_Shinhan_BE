using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;

namespace InvoiceServer.Business.BL
{
    public class SoftVendorBO : ISoftVendorBO
    {
        #region Fields, Properties

        private readonly ISoftVendorRepository softVendorRepository;
        #endregion

        #region Contructor

        public SoftVendorBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.softVendorRepository = repoFactory.GetRepository<ISoftVendorRepository>();
        }

        #endregion

        public VendorInfo GetSoftVendorInfo()
        {
            var softVendor = this.softVendorRepository.GetSoftVenderInfo();
            return new VendorInfo(softVendor);
        }
        #region Methods

        #endregion
    }
}