using InvoiceServer.Business.Models;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class RegisterTemplatesRepository : GenericRepository<REGISTERTEMPLATE>, IRegisterTemplatesRepository
    {
        public RegisterTemplatesRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<REGISTERTEMPLATE> GetList()
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false));
        }

        public IEnumerable<REGISTERTEMPLATE> GetRegisterTemplateCompanyUse(long companyId)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false) && p.COMPANYID == companyId);
        }
        public REGISTERTEMPLATE GetByCompanyId(long companyId, string code)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false) && p.COMPANYID == companyId && p.CODE == code).FirstOrDefault();
        }
        public IEnumerable<RegisterTemplateDenominator> GetDenominator(long companyId, long? branch)
        {
            var getDenominator = this.dbSet.Where(p => !(p.DELETED ?? false));

            if (branch.HasValue && branch > 0)
            {
                getDenominator = getDenominator.Where(p => p.COMPANYID == branch);
            }
            else if (branch.HasValue && branch == 0)
            {
                getDenominator = getDenominator.Where(p => p.COMPANYID == companyId);
            }

            var getListDenominator = (from notificationUseInvoice in this.context.Set<NOTIFICATIONUSEINVOICE>()
                                      join notificationUseInvoiceDetail in this.context.Set<NOTIFICATIONUSEINVOICEDETAIL>()
                                            on notificationUseInvoice.ID equals notificationUseInvoiceDetail.NOTIFICATIONUSEINVOICEID
                                      join registerTemplate in getDenominator
                                            on notificationUseInvoiceDetail.REGISTERTEMPLATESID equals registerTemplate.ID
                                      where notificationUseInvoice.STATUS == 2
                                      select new RegisterTemplateDenominator()
                                      {
                                          Id = registerTemplate.ID,
                                          Code = registerTemplate.CODE,
                                          InvoiceSampleId = (registerTemplate.INVOICESAMPLE.INVOICETEMPLATESAMPLEID ?? 0)
                                      }).Distinct();

            return getListDenominator;
        }

        public REGISTERTEMPLATE GetById(long Id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == Id);
        }

        public IEnumerable<TemplateAccepted> GetInvoiceApproved(long companyId)
        {
            var registerTemplate = this.dbSet.Where(p => !(p.DELETED ?? false) && p.COMPANYID == companyId);
            var invoiceReleasesDetail = this.context.Set<INVOICERELEASESDETAIL>().Where(p => !(p.DELETED ?? false)
                    && !(p.INVOICERELEAS.DELETED ?? false) && p.INVOICERELEAS.STATUS == (int)RecordStatus.Approved && p.INVOICERELEAS.COMPANYID == companyId);
            var InvoiceAccepteds = (from template in registerTemplate
                                    join ivReleaseDetail in invoiceReleasesDetail
                                        on template.ID equals ivReleaseDetail.REGISTERTEMPLATESID
                                    select new TemplateAccepted
                                    {
                                        Id = template.ID,
                                        CompanyId = template.COMPANYID,
                                        Code = template.CODE,
                                        Name = template.INVOICESAMPLE.NAME,
                                        Description = template.DESCRIPTION,
                                        InvoiceTemplateSampleId = (template.INVOICESAMPLE.INVOICETEMPLATESAMPLEID ?? 0)
                                    }).Distinct();

            return InvoiceAccepteds;
        }

        public IEnumerable<RegisterTemplateMaster> Filter(ConditionSearchRegisterTemplate condition, int skip = 0, int take = int.MaxValue)
        {
            var registerTemplateCompanyUses = this.dbSet.Where(p => !(p.DELETED ?? false));
            if (condition.Branch != 0)
            {
                registerTemplateCompanyUses = registerTemplateCompanyUses.Where(p => p.COMPANYID == condition.Branch);

            }
            var invoiceTypes = this.context.Set<INVOICETYPE>().Select(i => i);

            if (condition.Key.IsNotNullOrEmpty())
            {
                // Bug #22651
                registerTemplateCompanyUses = registerTemplateCompanyUses.Where(p => p.INVOICESAMPLE.INVOICETYPE.NAME.ToUpper().Contains(condition.Key.ToUpper())
                    || p.CODE.ToUpper().Contains(condition.Key.ToUpper()) || p.INVOICESAMPLE.NAME.ToUpper().Contains(condition.Key.ToUpper()));
            }

            var registerTemplates = (from register in registerTemplateCompanyUses
                                     join invoiceType in invoiceTypes
                                         on register.INVOICESAMPLE.INVOICETYPEID equals invoiceType.ID
                                     join myCompany in this.context.Set<MYCOMPANY>()
                                        on register.COMPANYID equals myCompany.COMPANYSID
                                     //join noiticeDetail in this.context.Set<NotificationUseInvoiceDetail>()
                                     //    on register.Id equals noiticeDetail.RegisterTemplatesId

                                     select new RegisterTemplateMaster()
                                     {
                                         Id = register.ID,
                                         CompanyId = (register.COMPANYID ?? 0),
                                         CompanyName = (myCompany.COMPANYNAME),
                                         BranchId = myCompany.BRANCHID,
                                         Prefix = register.PREFIX,
                                         Suffix = register.SUFFIX,
                                         Code = register.CODE,
                                         Name = register.INVOICESAMPLE.NAME,
                                         Description = register.DESCRIPTION,
                                         CreatedDate = register.CREATEDDATE,
                                         //ContractStatus = (noiticeDetail.Status ?? 0),
                                         InvoiceTypeName = invoiceType.NAME,
                                         InvoiceTypeId = invoiceType.ID
                                     });

            return registerTemplates;
        }

        public IEnumerable<REGISTERTEMPLATE> GetList(long companyId)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false) && p.COMPANYID == companyId);
        }

        public bool ContainCode(string code, long companyId)
        {
            bool result = true;
            if (!code.IsNullOrEmpty())
            {
                result = Contains(p => ((p.CODE ?? string.Empty).ToUpper()).Equals(code.ToUpper()) && !(p.DELETED ?? false) && p.COMPANYID == companyId);
            }
            return result;
        }

        public REGISTERTEMPLATE GetByParttern(string parttern, long companyId)
        {
            return this.dbSet.FirstOrDefault(p => p.CODE.Equals(parttern) && p.COMPANYID == companyId && !(p.DELETED ?? false));
        }

        public List<long> getListCompanyId(long companyId)
        {
            var result = new List<long> { };
            MYCOMPANY myCompany = this.context.Set<MYCOMPANY>().Where(p => p.COMPANYSID == companyId && !(p.DELETED ?? false) && (p.ACTIVE ?? false)).FirstOrDefault();
            if (myCompany != null)
            {
                if (myCompany.LEVELCUSTOMER == "PGD")
                {
                    result.Add(companyId);
                }
                else if (myCompany.LEVELCUSTOMER == "CN")
                {
                    result = this.context.Set<MYCOMPANY>().Where(p => p.COMPANYID == companyId && !(p.DELETED ?? false) && (p.ACTIVE ?? false)).Select(p => p.COMPANYSID).ToList();
                    result.Add(companyId);
                }
            }
            return result;
        }
        public IEnumerable<RegisterTemplateSample> GetRegisterTemplateSamplesByCompanyID(long companyId)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false) && p.COMPANYID == companyId)
                .ToList()
                .Select(x => new RegisterTemplateSample(x))
                .ToList();
        }
        #region  apply according to T78
        public IEnumerable<RegisterTemplateMaster> GetDenominatorTT78(long companyId, long? branch)
        {
            var getDenominator = this.dbSet.Where(p => !(p.DELETED ?? false));

            if (branch.HasValue && branch > 0)
            {
                getDenominator = getDenominator.Where(p => p.COMPANYID == branch);
            }
            else if (branch.HasValue && branch == 0)
            {
                getDenominator = getDenominator.Where(p => p.COMPANYID == companyId);
            }

            //var getListDenominator = (from notificationUseInvoice in this.context.Set<NOTIFICATIONUSEINVOICE>()
            //                          join notificationUseInvoiceDetail in this.context.Set<NOTIFICATIONUSEINVOICEDETAIL>()
            //                                on notificationUseInvoice.ID equals notificationUseInvoiceDetail.NOTIFICATIONUSEINVOICEID
            //                          join registerTemplate in getDenominator
            //                                on notificationUseInvoiceDetail.REGISTERTEMPLATESID equals registerTemplate.ID
            //                          where notificationUseInvoice.STATUS == 2
            //                          select new RegisterTemplateDenominator()
            //                          {
            //                              Id = registerTemplate.ID,
            //                              Code = registerTemplate.CODE,
            //                              InvoiceSampleId = (registerTemplate.INVOICESAMPLE.INVOICETEMPLATESAMPLEID ?? 0)
            //                          }).Distinct();
            var getListDenominator = (
                                      from registerTemplate in getDenominator
                                      join notificationUseInvoiceDetail in this.context.Set<NOTIFICATIONUSEINVOICEDETAIL>()
                                       on registerTemplate.ID equals notificationUseInvoiceDetail.REGISTERTEMPLATESID
                                       into temp1
                                      from notificationUseInvoiceDetail in temp1.DefaultIfEmpty()
                                      join notificationUseInvoice in this.context.Set<NOTIFICATIONUSEINVOICE>()
                                         on notificationUseInvoiceDetail.NOTIFICATIONUSEINVOICEID equals notificationUseInvoice.ID
                                        into temp2
                                      from notificationUseInvoice in temp2.DefaultIfEmpty()
                                      join invoiceTemplate in this.context.Set<INVOICETEMPLATE>()
                                            on registerTemplate.INVOICETEMPLATEID equals invoiceTemplate.ID
                                      select new RegisterTemplateMaster()
                                      {
                                          Id = registerTemplate.ID,
                                          Code = registerTemplate.CODE,
                                          InvoiceSampleId = (registerTemplate.INVOICESAMPLE.INVOICETEMPLATESAMPLEID ?? 0),
                                          IsMultiTax = (invoiceTemplate.ISMULTITAX ?? false) ? 1 : 0,
                                          IsDiscount = (invoiceTemplate.ISDISCOUNT ?? false) ? 1 : 0,
                                          NumberRecord = invoiceTemplate.NUMBERRECORD ?? 0,
                                          NumberCharacterInLine = invoiceTemplate.NUMBERCHARACTERINLINE ?? 0
                                      }).Distinct();

            return getListDenominator;
        }
        #endregion  apply according to T78` 
    }
}
