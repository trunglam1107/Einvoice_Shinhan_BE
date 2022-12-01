using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.InvoiceTemplate;
using InvoiceServer.Common;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class InvoiceTemplateSampleBO : IInvoiceTemplateSampleBO
    {
        #region Fields, Properties

        private readonly IInvoiceTemplateSampleRepository invoiceTemplateSampleRepository;
        #endregion

        #region Contructor

        public InvoiceTemplateSampleBO(IRepositoryFactory repoFactory, PrintConfig config)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.invoiceTemplateSampleRepository = repoFactory.GetRepository<IInvoiceTemplateSampleRepository>();
        }

        #endregion

        #region Methods

        public IEnumerable<InvoiceTemplateInfo> GetList()
        {
            var invoiceTemplates = this.invoiceTemplateSampleRepository.GetList();
            return invoiceTemplates.Select(p => new InvoiceTemplateInfo(p)).ToList();
        }

        public IEnumerable<InvoiceTemplateInfo> GetListByInvoiceTypeId(long invoiceTypeId)
        {
            var invoiceTemplates = this.invoiceTemplateSampleRepository.GetListByInvoiceTypeId(invoiceTypeId);
            return invoiceTemplates.Select(p => new InvoiceTemplateInfo(p)).ToList();
        }

        public InvoiceTemplateInfo GetById(long id)
        {
            var invoiceTemplateSample = this.invoiceTemplateSampleRepository.GetById(id);
            return new InvoiceTemplateInfo(invoiceTemplateSample);
        }

        public InvoiceTemplateViewModel GetViewModelById(long id)
        {
            var invoiceTemplateSample = this.invoiceTemplateSampleRepository.GetById(id);
            return new InvoiceTemplateViewModel(invoiceTemplateSample);
        }

        public InvoiceTemplateSampleName GetTemplateSampleNameById(long id)
        {
            var invoiceTemplateSample = this.invoiceTemplateSampleRepository.GetById(id);
            return new InvoiceTemplateSampleName()
            {
                TemplateSampleName = invoiceTemplateSample?.NAME,
                TemplateSampleFileName = invoiceTemplateSample?.URLFILE,
            };
        }


        #endregion
    }
}