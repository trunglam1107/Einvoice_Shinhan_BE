using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class AnnouncementRepository : GenericRepository<ANNOUNCEMENT>, IAnnouncementRepository
    {
        public AnnouncementRepository(IDbContext context)
            : base(context)
        {
        }

        public IQueryable<AnnouncementMaster> OrderByDefault(IQueryable<AnnouncementMaster> announcementMasters, bool desc = true)
        {
            //return desc
            //        ? announcementMasters.OrderByDescending(i => i.InvoiceNo == DefaultFields.INVOICE_NO_DEFAULT_VALUE)
            //          .ThenByDescending(x => x.Symbol)
            //          .ThenByDescending(x => x.InvoiceNo)
            //          .ThenByDescending(x => x.InvoiceDate)
            //          .ThenByDescending(x => x.Id)
            //        : announcementMasters.OrderBy(i => i.InvoiceNo == DefaultFields.INVOICE_NO_DEFAULT_VALUE)
            //          .ThenBy(x => x.Symbol)
            //          .ThenBy(x => x.InvoiceNo)
            //          .ThenBy(x => x.InvoiceDate)
            //          .ThenBy(x => x.Id);
            return announcementMasters.OrderByDescending(i => i.AnnouncementDate)
                      .ThenByDescending(x => x.MinutesNo);
        }

        public IEnumerable<AnnouncementMaster> FilterAnnouncement(ConditionSearchAnnouncement condition)
        {
            var orderByDefault = false;
            var announcements = Filter(condition);
            var invoices = this.context.Set<INVOICE>().AsQueryable();
            if (condition.InvoiceNo.IsNotNullOrEmpty())
            {
                var No = Decimal.Parse(condition.InvoiceNo);
                invoices = invoices.Where(p => p.INVOICENO == No);
            }
            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.CLIENT.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }
            var announcementInfos = (from announcement in announcements
                                     join invoice in invoices
                                     on announcement.INVOICEID equals invoice.ID
                                     join mycompany in this.context.Set<MYCOMPANY>()
                                     on announcement.COMPANYID equals mycompany.COMPANYSID
                                     orderby announcement.UPDATEDDATE descending
                                     select new AnnouncementMaster
                                     {
                                         Id = announcement.ID,
                                         Denominator = announcement.DENOMINATOR,
                                         Symbol = announcement.SYMBOL,
                                         InvoiceNo = announcement.INVOICENO,
                                         ClientName = announcement.CLIENTNAME,
                                         InvoiceDate = invoice.RELEASEDDATE ?? announcement.INVOICEDATE,
                                         Updated = announcement.UPDATEDDATE,
                                         AnnouncementStatus = announcement.ANNOUNCEMENTSTATUS,
                                         AnnouncementType = announcement.ANNOUNCEMENTTYPE,
                                         ClientTaxCode = announcement.CLIENTTAXCODE,
                                         AnnouncementDate = announcement.ANNOUNCEMENTDATE,
                                         InvoiceId = announcement.INVOICEID,
                                         IsCompanySign = (announcement.COMPANYSIGN ?? false),
                                         IsClientSign = (announcement.CLIENTSIGN ?? false),
                                         CompanyId = announcement.COMPANYID,
                                         MinutesNo = announcement.MINUTESNO,
                                         CustomerCode = invoice.CLIENT.CUSTOMERCODE,
                                         BranchId = mycompany.BRANCHID
                                     });


            return announcementInfos;
        }
        private IQueryable<ANNOUNCEMENT> Filter(ConditionSearchAnnouncement condition)
        {

            var announcements = this.dbSet.Where(p => p.COMPANYID > 0);

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                announcements = announcements.Where(p => p.COMPANYID == condition.Branch);
            }
            else if (!condition.Branch.HasValue)
            {
                announcements = announcements.Where(p => p.COMPANYID == condition.CompanyId);
            }

            if (condition.ClientName.IsNotNullOrEmpty())
            {
                announcements = announcements.Where(p => p.CLIENTNAME.ToUpper().Contains(condition.ClientName.ToUpper()));
            }
            //Search MinutesNo
            if (condition.MinutesNo.IsNotNullOrEmpty())
            {
                announcements = announcements.Where(p => p.MINUTESNO.ToUpper().Contains(condition.MinutesNo.ToUpper()));
            }

            if (condition.DateFrom.HasValue)
            {
                announcements = announcements.Where(p => p.ANNOUNCEMENTDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                var dateTo = ((DateTime)condition.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                announcements = announcements.Where(p => p.ANNOUNCEMENTDATE <= dateTo);
            }

            if (condition.ClientTaxCode.IsNotNullOrEmpty())
            {
                announcements = announcements.Where(p => p.CLIENTTAXCODE.Contains(condition.ClientTaxCode));
            }

            if (condition.AnnouncementType.HasValue)
            {
                announcements = announcements.Where(p => p.ANNOUNCEMENTTYPE == condition.AnnouncementType.Value);
            }

            if (condition.Denominator.IsNotNullOrEmpty())
            {
                announcements = announcements.Where(p => p.DENOMINATOR.Contains(condition.Denominator));
            }

            if (condition.Symbol.IsNotNullOrEmpty())
            {
                announcements = announcements.Where(p => p.SYMBOL == condition.Symbol);
            }

            if (condition.AnnouncementStatus.Count > 0)
            {
                announcements = announcements.Where(p => condition.AnnouncementStatus.Contains(p.ANNOUNCEMENTSTATUS));
            }

            if (condition.InvoiceNo.IsNotNullOrEmpty())
            {
                announcements = announcements.Where(p => p.INVOICENO.Contains(condition.InvoiceNo));
            }

            if (condition.AnnouncementType.HasValue)
            {
                announcements = announcements.Where(p => p.ANNOUNCEMENTTYPE == condition.AnnouncementType);
            }

            return announcements;
        }

        public AnnouncementDownload GetAnnouncementPrintInfo(long announcementId, MYCOMPANY companyInfo)
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai

            var invoicePrintInfos = from announcement in this.dbSet
                                    join announcementReleaseDetail in this.context.Set<RELEASEANNOUNCEMENTDETAIL>() on announcement.ID equals announcementReleaseDetail.ANNOUNCEMENTID into announcementReleaseDetailTemp
                                    from announcementReleaseDetail in announcementReleaseDetailTemp.DefaultIfEmpty()
                                    join invoice in this.context.Set<INVOICE>() on announcement.INVOICEID equals invoice.ID into invoiceTemp
                                    from invoice in invoiceTemp.DefaultIfEmpty()
                                    join client in this.context.Set<CLIENT>() on invoice.CLIENTID equals client.ID into clientTemp
                                    from client in clientTemp.DefaultIfEmpty()
                                    join currency in this.context.Set<CURRENCY>() on invoice.CURRENCYID equals currency.ID into currencyTemp
                                    from currency in currencyTemp.DefaultIfEmpty()
                                    where announcement.ID == announcementId
                                    let announcementNew = announcement.ANNOUNCEMENTSTATUS == (int)AnnouncementStatus.New
                                    let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                    let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
                                    select new AnnouncementDownload()
                                    {
                                        Id = announcement.ID,
                                        Title = announcement.TITLE,
                                        LegalGrounds = announcement.LEGALGROUNDS,
                                        AnnouncementDate = announcement.ANNOUNCEMENTDATE,
                                        CompanyId = announcement.COMPANYID,
                                        CompanyName = announcementNew ? companyInfo.COMPANYNAME : announcement.COMPANYNAME,
                                        CompanyAddress = announcementNew ? companyInfo.ADDRESS : announcement.COMPANYADDRESS,
                                        CompanyTel = announcementNew ? companyInfo.TEL1 : announcement.COMPANYTEL,
                                        CompanyTaxCode = announcementNew ? companyInfo.TAXCODE : announcement.COMPANYTAXCODE,

                                        CompanyRepresentative = announcement.COMPANYREPRESENTATIVE,
                                        CompanyPosition = announcement.COMPANYPOSITION,
                                        ClientName = announcementNew ? (isOrgCurrentClient == false ? (notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT) : (notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME)) : announcement.CLIENTNAME,
                                        ClientAddress = announcementNew ? (notCurrentClient ? client.ADDRESS : invoice.CUSTOMERADDRESS) : announcement.CLIENTADDRESS,
                                        ClientTel = announcementNew ? client.MOBILE : announcement.CLIENTTEL,
                                        ClientTaxCode = announcementNew ? (notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE) : announcement.CLIENTTAXCODE,
                                        ClientRepresentative = announcement.CLIENTREPRESENTATIVE,
                                        ClientPosition = announcement.CLIENTPOSITION,
                                        InvoiceId = announcement.INVOICEID,
                                        RegisterTemplateId = announcement.REGISTERTEMPLATEID,
                                        Denominator = announcement.DENOMINATOR,
                                        Symbol = announcement.SYMBOL,
                                        InvoiceNo = announcement.INVOICENO,
                                        InvoiceDate = invoice.RELEASEDDATE ?? announcement.INVOICEDATE,
                                        TotalAmount = announcement.TOTALAMOUNT,
                                        ServiceName = announcement.SERVICENAME,
                                        Reasion = announcement.REASION,
                                        BeforeContain = announcement.BEFORECONTAIN,
                                        ChangeContain = announcement.CHANGECONTAIN,
                                        AnnouncementType = announcement.ANNOUNCEMENTTYPE,
                                        AnnouncementStatus = announcement.ANNOUNCEMENTSTATUS,
                                        CreatedBy = announcement.CREATEDBY,
                                        CreatedDate = announcement.CREATEDDATE,
                                        UpdatedBy = announcement.UPDATEDBY,
                                        UpdatedDate = announcement.UPDATEDDATE,
                                        ApprovedBy = announcement.APPROVEDBY,
                                        ApprovedDate = announcement.APPROVEDDATE,
                                        ProcessBy = announcement.PROCESSBY,
                                        ProcessDate = announcement.PROCESSDATE,
                                        Minutesno = announcement.MINUTESNO,
                                        CurrencyCode = currency.CODE,
                                        VerificationCode = announcementReleaseDetail.VERIFICATIONCODE,
                                    };
            return invoicePrintInfos.FirstOrDefault();
        }

        public ANNOUNCEMENT GetById(long id, long companyId)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id && p.COMPANYID == companyId);
        }

        public bool getMinus(string MINUTESNO, long? id, long companyId)
        {
            if (id == null)
            {
                if (this.dbSet.Where(p => p.MINUTESNO.ToUpper().Equals(MINUTESNO.ToUpper()) && p.COMPANYID == companyId).Count() > 0)
                    return true;
                return false;
            }
            else
            {
                var result = this.dbSet.Where(p => p.MINUTESNO.ToUpper().Equals(MINUTESNO.ToUpper()) && p.COMPANYID == companyId && p.ID == id).FirstOrDefault();
                if (result != null)
                {
                    return false;
                }
                else if (this.dbSet.Where(p => p.MINUTESNO.ToUpper().Equals(MINUTESNO.ToUpper()) && p.COMPANYID == companyId && p.ID != id).Count() > 0)
                { return true; }
                else
                {
                    return false;
                }
            }
        }

        public ANNOUNCEMENT GetAnnounApprovedByInvoiceId(long invoiceId)
        {
            return this.dbSet.FirstOrDefault(p => p.INVOICEID == invoiceId && p.ANNOUNCEMENTSTATUS >= (int)AnnouncementStatus.Approved && p.ANNOUNCEMENTTYPE != 1);
        }

        public ANNOUNCEMENT GetByInvoiceId(long invoiceId, int announType, long? companyId)
        {
            return this.dbSet.FirstOrDefault(p => p.INVOICEID == invoiceId && p.ANNOUNCEMENTTYPE == announType);
        }
        public bool CheckAnnount(long invoiceId, int type)
        {
            var Announcement = from announ in this.dbSet
                               join invoice in this.context.Set<INVOICE>() on announ.INVOICEID equals invoice.ID
                               where announ.INVOICEID == invoiceId && announ.ANNOUNCEMENTTYPE == type
                               select announ;
            if (Announcement.Any())
            {
                return true;
            }
            return false;
        }
        public AnnouncementInfo CheckExistAnnoun(long invoiceId, bool invoiceType, int announType)
        {
            if (!invoiceType)
            {
                return null;
            }
            var annou = (from an in this.dbSet
                         where an.INVOICEID == invoiceId && an.ANNOUNCEMENTTYPE == announType
                         select new AnnouncementInfo()
                         {
                             Id = an.ID,
                             AnnouncementType = an.ANNOUNCEMENTTYPE,
                             AnnouncementStatus = an.ANNOUNCEMENTSTATUS,
                             CompanySign = an.COMPANYSIGN,
                             ClientSign = an.CLIENTSIGN,
                         }).FirstOrDefault();
            if (annou == null)
            {
                return new AnnouncementInfo();
            }
            var invoiceChild = this.context.Set<INVOICE>().Where(p => p.PARENTID == invoiceId && p.DELETED != true);
            if (annou != null && invoiceChild.Count() == 0)
            {
                annou.Used = false;
            }
            else
            {
                annou.Used = true;
            }
            return annou;
        }
        public ANNOUNCEMENT GetByOnlyId(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }

        public SendEmailVerificationCodeAnnoun GetVerificationCodeInfomation(AnnouncementVerificationCode announcementVerificationCode)
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai

            var informationEmailVerificationCode = from announcement in dbSet
                                                   join invoice in this.context.Set<INVOICE>() on announcement.INVOICEID equals invoice.ID
                                                   join releaseDetail in this.context.Set<RELEASEANNOUNCEMENTDETAIL>() on announcement.ID equals releaseDetail.ANNOUNCEMENTID into releaseDetailTemp
                                                   join client in this.context.Set<CLIENT>() on invoice.CLIENTID equals client.ID
                                                   join company in this.context.Set<MYCOMPANY>() on invoice.COMPANYID equals company.COMPANYSID
                                                   join template in this.context.Set<REGISTERTEMPLATE>() on invoice.REGISTERTEMPLATEID equals template.ID into templateTemp
                                                   join login in this.context.Set<LOGINUSER>() on client.ID equals login.CLIENTID into loginTemp
                                                   from releaseDetail in releaseDetailTemp.DefaultIfEmpty()
                                                   from template in templateTemp.DefaultIfEmpty()
                                                   from login in loginTemp.DefaultIfEmpty()
                                                   where releaseDetail.VERIFICATIONCODE.Equals(announcementVerificationCode.VerificationCode)
                                                   let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                                   let isOrgCurrentClient = notCurrentClient ? client.ISORG : invoice.CUSTOMERTAXCODE != null
                                                   select new SendEmailVerificationCodeAnnoun()
                                                   {
                                                       AnnouncementId = announcement.ID,
                                                       AnnouncementTitle = announcement.TITLE,
                                                       AnnouncementType = announcement.ANNOUNCEMENTTYPE,
                                                       Ression = announcement.REASION,
                                                       BeforeContain = announcement.BEFORECONTAIN,
                                                       ChangeContain = announcement.CHANGECONTAIN,
                                                       InvoiceId = (invoice.ID),
                                                       VerificationCode = releaseDetail.VERIFICATIONCODE,
                                                       CompanyName = company.COMPANYNAME,
                                                       CompanyEmail = company.EMAIL,
                                                       CustomerName = notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                                       CustomerCode = client.CUSTOMERCODE,
                                                       PersonContact = notCurrentClient ? client.PERSONCONTACT : invoice.PERSONCONTACT,
                                                       InvoiceCode = template.CODE,
                                                       InvoiceNo = invoice.NO,
                                                       Symbol = invoice.SYMBOL,
                                                       //InvoiceDate = (invoice.RELEASEDDATE ?? announcement.INVOICEDATE),
                                                       InvoiceDate = announcement.ANNOUNCEMENTDATE,
                                                       Total = (invoice.TOTAL ?? 0),
                                                       TotalTax = (invoice.TOTALTAX ?? 0),
                                                       Sum = (invoice.SUM ?? 0),
                                                       ClientUser = login.USERID,
                                                       ClientPassword = login.PASSWORD,
                                                       IsOrg = isOrgCurrentClient ?? false,
                                                       ServiceName = announcement.SERVICENAME,
                                                       MinutesNo = announcement.MINUTESNO,
                                                       AnnouncementDate = announcement.ANNOUNCEMENTDATE,
                                                       TaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                                       Identity = client.IDENTITY,
                                                       CompanyId = announcement.COMPANYID
                                                   };

            return informationEmailVerificationCode.FirstOrDefault();
        }
    }
}

