using InvoiceServer.Business.DAO;
using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.InvoiceTemplate;
using InvoiceServer.Common;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class InvoiceTemplateBO : IInvoiceTemplateBO
    {
        #region Fields, Properties

        private readonly IInvoiceTemplateRepository invoiceTemplateRepository;
        private readonly IInvoiceTemplateSampleRepository invoiceTemplateSampleRepository;
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly PrintConfig config;
        private readonly UserSessionInfo currentUser;
        private readonly IReleaseInvoiceDetaiRepository releaseInvoiceDetaiRepository;
        private readonly IRegisterTemplatesRepository registerTemplatesRepository;
        #endregion

        #region Contructor

        public InvoiceTemplateBO(IRepositoryFactory repoFactory, PrintConfig config)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.invoiceTemplateRepository = repoFactory.GetRepository<IInvoiceTemplateRepository>();
            this.invoiceTemplateSampleRepository = repoFactory.GetRepository<IInvoiceTemplateSampleRepository>();
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.config = config;
            this.releaseInvoiceDetaiRepository = repoFactory.GetRepository<IReleaseInvoiceDetaiRepository>();
            this.registerTemplatesRepository = repoFactory.GetRepository<IRegisterTemplatesRepository>();
        }

        public InvoiceTemplateBO(IRepositoryFactory repoFactory, PrintConfig config, UserSessionInfo userSessionInfo)
            : this(repoFactory, config)
        {
            this.currentUser = userSessionInfo;
        }

        #endregion

        #region Methods

        public IEnumerable<InvoiceTemplateInfo> GetList(long companySID)
        {
            var invoiceTemplates = this.invoiceTemplateRepository.GetAll(companySID);
            return invoiceTemplates.Select(p => new InvoiceTemplateInfo(p));
        }

        public ExportFileInfo PrintViewTemplate(long templateId, long companyId)
        {
            var myCompany = GetMyCompany(companyId);
            InvoicePrintInfo invoicePrint = new InvoicePrintInfo(myCompany);
            invoicePrint.Invoice_Status = (int)InvoiceStatus.Draft;
            INVOICETEMPLATESAMPLE template = GetTemplateSample(templateId);
            invoicePrint.Option.Height = template.HEIGHT;
            invoicePrint.Option.Width = template.WIDTH;
            invoicePrint.Option.PageSize = template.PAGESIZE;
            invoicePrint.Option.PageOrientation = template.PAGEORIENTATION;
            invoicePrint.isMultiTax = (bool)template.ISMULTITAX;
            invoicePrint.TemplateName = template.URLFILE;
            this.config.FullPathFileTemplateInvoice = Path.Combine(this.config.FullPathFolderTemplateInvoice, template.URLFILE);
            int numberLineOfPage = 15;
            int numberCharacterOfLine = 46;
            if (template != null && template.NUMBERRECORD.HasValue)
            {
                numberLineOfPage = (int)template.NUMBERRECORD;
            }

            if (template != null && template.NUMBERCHARACTERINLINE.HasValue)
            {
                numberCharacterOfLine = template.NUMBERCHARACTERINLINE.Value;
            }

            // Get VerificationCode
            var releaseInvoiceDetail = this.releaseInvoiceDetaiRepository.FilteReleaseInvoiceDetail(invoicePrint.Id);
            invoicePrint.VerificationCode = releaseInvoiceDetail?.VERIFICATIONCODE;

            var invoiceExportModel = new InvoiceExportModel()
            {
                InvoicePrintInfo = invoicePrint,
                InvoiceStatisticals = null,
                InvoiceGift = null,
                PrintConfig = config,
                NumberLineOfPage = numberLineOfPage,
                NumberRecord = template.NUMBERRECORD ?? 0,
                DetailInvoice = template.DETAILINVOICE,
                LocationSignLeft = template.LOCATIONSIGNLEFT.ToDecimal() ?? 0,
                LocationSignButton = template.LOCATIONSIGNBUTTON.ToDecimal() ?? 0,
                NumberCharacterOfLine = numberCharacterOfLine,
                IsShowOrder = template.ISSHOWORDER,
                HeaderInvoice = template.HEADERINVOICE,
                FooterInvoice = template.FOOTERINVOICE,
                UserSessionInfo = currentUser,
                TemplateFileName = template.URLFILE,
            };
            HQExportFile export = new HQExportFile(invoiceExportModel);
            return export.ExportFile();
        }

        private MYCOMPANY GetMyCompany(long companyId)
        {
            return this.myCompanyRepository.GetById(companyId);
        }

        public INVOICETEMPLATE GetTemplate(long id)
        {
            return this.invoiceTemplateRepository.GetById(id);
        }
        public INVOICETEMPLATESAMPLE GetTemplateSample(long id)
        {
            return this.invoiceTemplateSampleRepository.GetById(id);
        }

        public IEnumerable<InvoiceTemplateInfo> Filter(long invoiceSampleId, long companySID)
        {
            var invoiceTemplates = this.invoiceTemplateRepository.Filter(invoiceSampleId, companySID);
            return invoiceTemplates.Select(p => new InvoiceTemplateInfo(p));
        }
        public long CreateInvoiceTemplateByInvoiceSample(InvoiceTemplateViewModel invoiceTemplateViewModel, long companyId, long sampleId)
        {
            var invoiceTemplate = new INVOICETEMPLATE()
            {
                //Id = invoiceTemplateViewModel.Id,
                //INVOICESAMPLEID = invoiceTemplateSampleViewModel.InvoiceTypeId,
                INVOICESAMPLEID = sampleId,
                NAME = invoiceTemplateViewModel.Name,
                DELETED = invoiceTemplateViewModel.Deleted,
                DETAILINVOICE = invoiceTemplateViewModel.DetailInvoice,
                URLFILE = invoiceTemplateViewModel.UrlFile,
                HEADERINVOICE = invoiceTemplateViewModel.HeaderInvoice,
                FOOTERINVOICE = invoiceTemplateViewModel.FooterInvoice,
                IMAGETEMPLATE = invoiceTemplateViewModel.ImageTemplate,
                NUMBERRECORD = invoiceTemplateViewModel.NumberRecord,
                ISSHOWORDER = invoiceTemplateViewModel.IsShowOrder,
                ORDERS = invoiceTemplateViewModel.Orders,
                WIDTH = invoiceTemplateViewModel.Width,
                HEIGHT = invoiceTemplateViewModel.Height,
                LOCATIONSIGNLEFT = invoiceTemplateViewModel.LocationSignLeft,
                LOCATIONSIGNBUTTON = invoiceTemplateViewModel.LocationSignButton,
                NUMBERCHARACTERINLINE = invoiceTemplateViewModel.NumberCharacterInLine,
                LOCATIONSIGNRIGHT = invoiceTemplateViewModel.LocationSignRight,
                PAGESIZE = invoiceTemplateViewModel.PageSize,
                PAGEORIENTATION = invoiceTemplateViewModel.PageOrientation,
                ISMULTITAX = invoiceTemplateViewModel.IsMultiTax,
                ISDISCOUNT = invoiceTemplateViewModel.IsDiscount,
                NUMBERLINEOFPAGE = invoiceTemplateViewModel.NumberLineOfPage,
            };

            var invoiceTemplateDB = this.invoiceTemplateRepository.GetByInvoiceSampleId(invoiceTemplate.INVOICESAMPLEID ?? 0, companyId);
            if (invoiceTemplateDB != null)
            {
                invoiceTemplate.ID = invoiceTemplateDB.ID;
            }

            return invoiceTemplateRepository.CreateInvoiceTemplate(invoiceTemplate);
        }

        public long GetByInvoiceSampleCode(string invoiceSampleCode, long companyId)
        {
            var existingInvoiceTemplate = invoiceTemplateRepository.GetByInvoiceSampleCode(invoiceSampleCode, companyId);
            if (existingInvoiceTemplate != null) return existingInvoiceTemplate.ID;
            return 0;
        }

        public InvoiceTemplateInfo GetByRegisterTemplateId(long registerTemplateId)
        {
            var registerTemplate = this.registerTemplatesRepository.GetById(registerTemplateId);
            if (registerTemplate != null)
            {
                return new InvoiceTemplateInfo(registerTemplate.INVOICETEMPLATE);
            }
            return new InvoiceTemplateInfo();
        }
        public List<INVOICETEMPLATE> GetAll()
        {
           return this.invoiceTemplateRepository.GetAll().ToList();           
        }
        
        #endregion
    }
}