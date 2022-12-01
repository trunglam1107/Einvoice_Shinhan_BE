using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class TypePaymentBO : ITypePaymentBO
    {
        #region Fields, Properties

        private readonly ITypePaymentRepository typePaymentRepository;
        #endregion

        #region Contructor

        public TypePaymentBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.typePaymentRepository = repoFactory.GetRepository<ITypePaymentRepository>();
        }

        #endregion
        public IEnumerable<TypePaymentInfo> GetList()
        {
            var typePayments = this.typePaymentRepository.GetAll();
            return typePayments.Select(p => new TypePaymentInfo(p));
        }
        #region Methods

        #endregion
    }
}