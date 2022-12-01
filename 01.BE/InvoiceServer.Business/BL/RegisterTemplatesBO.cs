using InvoiceServer.Business.DAO;
using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class RegisterTemplatesBO : IRegisterTemplatesBO
    {
        #region Fields, Properties

        private readonly IRegisterTemplatesRepository registerTemplatesRepository;
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly PrintConfig config;
        private readonly UserSessionInfo currentUser;
        private readonly IReleaseInvoiceDetaiRepository releaseInvoiceDetaiRepository;
        private readonly IDeclarationRepository declarationRepository;
        #endregion

        #region Contructor

        public RegisterTemplatesBO(IRepositoryFactory repoFactory, PrintConfig config)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.registerTemplatesRepository = repoFactory.GetRepository<IRegisterTemplatesRepository>();
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.config = config;
            this.releaseInvoiceDetaiRepository = repoFactory.GetRepository<IReleaseInvoiceDetaiRepository>();
            this.declarationRepository = repoFactory.GetRepository<IDeclarationRepository>();
        }

        public RegisterTemplatesBO(IRepositoryFactory repoFactory, PrintConfig config, UserSessionInfo userSessionInfo)
            : this(repoFactory, config)
        {
            this.currentUser = userSessionInfo;
        }

        #endregion
        public IEnumerable<RegisterTemplateMaster> GetList(long CompanyId)
        {
            var invoiceCompanyUse = this.registerTemplatesRepository.GetAll().Where(p => p.COMPANYID == CompanyId && !(p.DELETED ?? false)).ToList();
            return invoiceCompanyUse.Select(p => new RegisterTemplateMaster(p));
        }
        public IEnumerable<RegisterTemplateDenominator> GetListDenominator(long companyId, long? branch)
        {
            var invoiceCompanyUse = this.registerTemplatesRepository.GetDenominatorTT78(companyId, branch);
            invoiceCompanyUse = invoiceCompanyUse.OrderBy(p => p.InvoiceSampleId);

            return invoiceCompanyUse.Select(p => new RegisterTemplateDenominator(p));
        }

        public IEnumerable<TemplateAccepted> GetInvoiceApproved(long CompanyId)
        {
            var invoiceCompanyUse = this.registerTemplatesRepository.GetInvoiceApproved(CompanyId).ToList();
            return invoiceCompanyUse;
        }

        public IEnumerable<RegisterTemplateMaster> Filter(ConditionSearchRegisterTemplate condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var registerTemplateMaster = this.registerTemplatesRepository.Filter(condition).AsQueryable()
                .OrderBy(condition.ColumnOrder1, condition.OrderType.Equals(OrderTypeConst.Desc)).ThenBy(condition.ColumnOrder2, condition.OrderType.Equals(OrderTypeConst.Desc))
                .Skip(condition.Skip).Take(condition.Take).ToList();
            return registerTemplateMaster;
        }

        public long Count(ConditionSearchRegisterTemplate condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.registerTemplatesRepository.Filter(condition).Count();
        }

        public RegisterTemplateInfo GetById(long id)
        {
            var registerTemplate = GetTemplateOfCompany(id);
            return new RegisterTemplateInfo(registerTemplate);
        }

        public long Create(RegisterTemplateInfo info)
        {
            long invoiceCompanyUserId = 0;
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!info.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            info.Name = string.Format("{0}0/{1}", info.Prefix, info.Suffix);
            if (IsExistedCode(info.CompanyId.Value, info.Name))
            {
                throw new BusinessLogicException(ResultCode.TemplateCodeIdIsExisted, string.Format("Template [{0}0/{1}] is existed", info.Prefix, info.Suffix));
            }

            REGISTERTEMPLATE templateCompanyUse = new REGISTERTEMPLATE();
            templateCompanyUse.CopyData(info);
            templateCompanyUse.CREATEDDATE = DateTime.Now;
            templateCompanyUse.STATUS = 1;

            if (templateCompanyUse.INVOICETEMPLATEID == null)
            {
                throw new BusinessLogicException(ResultCode.InvoiceTemplateIdCannotNull, string.Format("InvoiceTemplateId is null, check param of url may have slash \"/\" character. InvoiceSampleCode: {0}", info.Code));
            }

            this.registerTemplatesRepository.Insert(templateCompanyUse);
            return invoiceCompanyUserId;
        }

        public ResultCode Update(long id, RegisterTemplateInfo info)
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!info.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            REGISTERTEMPLATE registerTemplate = GetTemplateOfCompany(id);
            info.Name = string.Format("{0}0/{1}", info.Prefix, info.Suffix);
            if (!registerTemplate.CODE.IsEquals(info.Name)
              && IsExistedCode(info.CompanyId.Value, info.Name))
            {
                throw new BusinessLogicException(ResultCode.TemplateCodeIdIsExisted, MsgApiResponse.DataInvalid);
            }


            info.Name = string.Format("{0}0/{1}", info.Prefix, info.Suffix);
            registerTemplate.CopyData(info);
            this.registerTemplatesRepository.Update(registerTemplate);
            return ResultCode.NoError;
        }

        public ExportFileInfo PrintView(long companyId, string parttern, string symbol, string fromNumber)
        {
            var myCompany = GetMyCompany(companyId);
            InvoicePrintInfo invoicePrint = new InvoicePrintInfo(myCompany);
            invoicePrint.Invoice_Status = (int)InvoiceStatus.Draft;
            invoicePrint.InvoiceNo = fromNumber;
            invoicePrint.InvoiceCode = parttern;
            invoicePrint.Symbol = symbol;
            var template = GetTemplateOfCompany(parttern, companyId);
            invoicePrint.Option.Height = template.INVOICETEMPLATE.HEIGHT;
            invoicePrint.Option.Width = template.INVOICETEMPLATE.WIDTH;
            invoicePrint.Option.PageSize = template.INVOICETEMPLATE.PAGESIZE;
            invoicePrint.Option.PageOrientation = template.INVOICETEMPLATE.PAGEORIENTATION;
            invoicePrint.TemplateName = template.INVOICETEMPLATE.URLFILE;

            this.config.FullPathFileTemplateInvoice = Path.Combine(this.config.FullPathFolderTemplateInvoice, template.INVOICETEMPLATE.URLFILE);
            int numberLineOfPage = 15;
            int numberCharacterOfLine = 46;
            if (template.INVOICETEMPLATE.NUMBERRECORD == 8)
            {
                template.INVOICETEMPLATE.NUMBERRECORD = 7;
            }
            if (template.INVOICETEMPLATE != null && template.INVOICETEMPLATE.NUMBERRECORD.HasValue)
            {
                numberLineOfPage = template.INVOICETEMPLATE.NUMBERRECORD.Value;
            }
            if (template != null && template.INVOICETEMPLATE.NUMBERCHARACTERINLINE.HasValue)
            {
                numberCharacterOfLine = template.INVOICETEMPLATE.NUMBERCHARACTERINLINE.Value;
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
                NumberRecord = template.INVOICETEMPLATE.NUMBERRECORD ?? 0,
                DetailInvoice = template.INVOICETEMPLATE.DETAILINVOICE,
                LocationSignLeft = template.INVOICETEMPLATE.LOCATIONSIGNLEFT,
                LocationSignButton = template.INVOICETEMPLATE.LOCATIONSIGNBUTTON,
                NumberCharacterOfLine = numberCharacterOfLine,
                IsShowOrder = template.INVOICETEMPLATE.ISSHOWORDER,
                HeaderInvoice = template.INVOICETEMPLATE.HEADERINVOICE,
                FooterInvoice = template.INVOICETEMPLATE.FOOTERINVOICE,
                UserSessionInfo = currentUser,
                TemplateFileName = template.INVOICETEMPLATE.URLFILE,
            };
            HQExportFile export = new HQExportFile(invoiceExportModel);
            return export.ExportFile();
        }

        public ResultCode Delete(long id)
        {
            REGISTERTEMPLATE invoiceConclude = GetTemplateOfCompany(id);
            if (!CanDelete(id))
            {
                throw new BusinessLogicException(ResultCode.TemplateInvoiceBeingused, string.Format("Invoice template id [{0}] is being used", id));
            }

            invoiceConclude.DELETED = true;
            this.registerTemplatesRepository.Update(invoiceConclude);
            return ResultCode.NoError;
        }

        private bool CanDelete(long id)
        {
            int numberUse = this.declarationRepository.GetDeclarationUseRegisterTemplate(id);
            return numberUse == 0;
        }

        private MYCOMPANY GetMyCompany(long companyId)
        {
            return this.myCompanyRepository.GetById(companyId);

        }

        private bool IsExistedCode(long companyId, string code)
        {
            return this.registerTemplatesRepository.ContainCode(code, companyId);
        }
        private REGISTERTEMPLATE GetTemplateOfCompany(long id)
        {
            REGISTERTEMPLATE invoiceCompanyUse = this.registerTemplatesRepository.GetById(id);
            if (invoiceCompanyUse == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This invoiceSampleId [{0}] not found in data of client", id));
            }

            return invoiceCompanyUse;
        }

        private REGISTERTEMPLATE GetTemplateOfCompany(string parttern, long companyId)
        {
            REGISTERTEMPLATE invoiceCompanyUse = this.registerTemplatesRepository.GetByParttern(parttern, companyId);
            if (invoiceCompanyUse == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This parttern [{0}] not found in data of client", parttern));
            }

            return invoiceCompanyUse;
        }
        public IEnumerable<RegisterTemplateSample> GetRegisterTemplateSampleOfCompany(long companyID)
        {
            return this.registerTemplatesRepository.GetRegisterTemplateSamplesByCompanyID(companyID);
        }
    }
}