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
    public class EmailActiveRepository : GenericRepository<EMAILACTIVE>, IEmailActiveRepository
    {
        public EmailActiveRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<EmailActiveInfo> Filter(ConditionSearchEmailActive condition)
        {
            IQueryable<EmailActiveInfo> emailActiveInfos = QueryFilter(condition);

            var emailActive = emailActiveInfos.OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take);

            return emailActive;
        }

        private IQueryable<EmailActiveInfo> QueryFilter(ConditionSearchEmailActive condition)
        {
            IQueryable<EMAILACTIVE> emailActives = this.dbSet;
            var invoices = this.context.Set<INVOICE>();
            var clients = this.context.Set<CLIENT>();
            var myCompanys = this.context.Set<MYCOMPANY>().Where(p => !(p.DELETED ?? false));
            var emailGroupInvoice = from email in emailActives
                                    join myCompany in myCompanys on email.COMPANYID equals myCompany.COMPANYSID into myCompanyTemp
                                    from myCompany in myCompanyTemp.DefaultIfEmpty()
                                    join emailReference in this.context.Set<EMAILACTIVEREFERENCE>() on email.ID equals emailReference.EMAILACTIVEID into emailReferenceTemp
                                    from emailReference in emailReferenceTemp.DefaultIfEmpty()
                                    join invoice in invoices on emailReference.REFID equals invoice.ID into invoiceTemp
                                    from invoice in invoiceTemp.DefaultIfEmpty()
                                    join client in clients on invoice.CLIENTID equals client.ID into clientTemp
                                    from client in clientTemp.DefaultIfEmpty()
                                    where email.TYPEEMAIL == (int)TypeEmailActive.DailyEmail || email.TYPEEMAIL == (int)TypeEmailActive.MonthlyEmail
                                    select new { email, invoice, client, myCompany };
            var emailGroupAccount = from email in emailActives
                                    join myCompany in myCompanys on email.COMPANYID equals myCompany.COMPANYSID into myCompanyTemp
                                    from myCompany in myCompanyTemp.DefaultIfEmpty()
                                    join emailReference in this.context.Set<EMAILACTIVEREFERENCE>() on email.ID equals emailReference.EMAILACTIVEID into emailReferenceTemp
                                    from emailReference in emailReferenceTemp.DefaultIfEmpty()
                                    join client in clients on emailReference.REFID equals client.ID into clientTemp
                                    from client in clientTemp.DefaultIfEmpty()
                                    where email.TYPEEMAIL == (int)TypeEmailActive.ProvideAccountEmail
                                    select new { email, client, myCompany };
            var emailGroupAnnouncement = from email in emailActives
                                         join myCompany in myCompanys on email.COMPANYID equals myCompany.COMPANYSID into myCompanyTemp
                                         from myCompany in myCompanyTemp.DefaultIfEmpty()
                                         join emailReference in this.context.Set<EMAILACTIVEREFERENCE>() on email.ID equals emailReference.EMAILACTIVEID into emailReferenceTemp
                                         from emailReference in emailReferenceTemp.DefaultIfEmpty()
                                         join announcement in this.context.Set<ANNOUNCEMENT>() on emailReference.REFID equals announcement.ID into announcementTemp
                                         where email.TYPEEMAIL == (int)TypeEmailActive.AnnouncementEmail
                                         from announcement in announcementTemp.DefaultIfEmpty()
                                         join invoice in invoices on announcement.INVOICEID equals invoice.ID into invoiceTemp
                                         from invoice in invoiceTemp.DefaultIfEmpty()
                                         join client in clients on invoice.CLIENTID equals client.ID into clientTemp
                                         from client in clientTemp.DefaultIfEmpty()
                                         select new { email, announcement, client, myCompany };
            var emailActiveInfo = from email in emailActives
                                  join gInvoice in emailGroupInvoice on email.ID equals gInvoice.email.ID into gInvoiceTemp
                                  from gInvoice in gInvoiceTemp.DefaultIfEmpty()
                                  join gAnnouncement in emailGroupAnnouncement on email.ID equals gAnnouncement.email.ID into gAnnouncementTemp
                                  from gAnnouncement in gAnnouncementTemp.DefaultIfEmpty()
                                  join gAccount in emailGroupAccount on email.ID equals gAccount.email.ID into gAccountTemp
                                  from gAccount in gAccountTemp.DefaultIfEmpty()
                                  select new
                                  {
                                      email,
                                      gInvoice.myCompany,
                                      gInvoice.invoice,
                                      gInvoice.client,
                                      gAnnouncement.announcement,
                                      annCompany = gAnnouncement.myCompany,
                                      annClient = gAnnouncement.client,
                                      annAccountClient = gAccount.client,
                                      mycompanyAccountClient = gAccount.myCompany
                                  };

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                if (condition.EmailType.HasValue)
                {
                    if (condition.EmailType.Value == (int)TypeEmailActive.ProvideAccountEmail && (condition.CustomerCode.IsNotNullOrEmpty() || condition.EmailTo.IsNotNullOrEmpty()))
                    {
                        emailActiveInfo = emailActiveInfo;
                    }
                    else
                    {
                        emailActiveInfo = emailActiveInfo.Where(p => p.email.COMPANYID == condition.Branch);
                    }
                }
                else
                {
                    emailActiveInfo = emailActiveInfo.Where(p => p.email.COMPANYID == condition.Branch);
                }
                

            }

            if (condition.Teller.IsNotNullOrEmpty())
            {
                emailActiveInfo = emailActiveInfo.Where(x => x.invoice.TELLER.ToUpper().Contains(condition.Teller.ToUpper()));
            }

            if (condition.InvoiceNo.IsNotNullOrEmpty())
            {
                var no = Decimal.Parse(condition.InvoiceNo);
                var noString = no.ToString().PadLeft(7, '0');
                emailActiveInfo = emailActiveInfo.Where(x => x.invoice.INVOICENO == no || x.announcement.INVOICENO == noString);
            }

            if (condition.DateInvoice.HasValue)
            {
                DateTime startDay = condition.DateInvoice.Value.Date;
                DateTime endDay = condition.DateInvoice.Value.Date.AddDays(1);
                emailActiveInfo = emailActiveInfo.Where(x => (x.invoice.RELEASEDDATE >= startDay && x.invoice.RELEASEDDATE < endDay) || (x.announcement.INVOICEDATE >= startDay && x.announcement.INVOICEDATE < endDay));
            }

            if (condition.DateFrom.HasValue)
            {
                if (condition.EmailType.HasValue)
                {
                    if (condition.EmailType.Value == (int)TypeEmailActive.ProvideAccountEmail
                        && (condition.CustomerCode.IsNotNullOrEmpty() || condition.EmailTo.IsNotNullOrEmpty()))
                    {
                        emailActiveInfo = emailActiveInfo;
                    }
                    else
                    {
                        emailActiveInfo = emailActiveInfo.Where(p => p.email.CREATEDDATE >= condition.DateFrom.Value);
                    }
                }
                else
                {
                    emailActiveInfo = emailActiveInfo.Where(p => p.email.CREATEDDATE >= condition.DateFrom.Value);
                }
                    
            }

            if (condition.DateTo.HasValue)
            {
                if (condition.EmailType.HasValue)
                {
                    if (condition.EmailType.Value == (int)TypeEmailActive.ProvideAccountEmail 
                        && (condition.CustomerCode.IsNotNullOrEmpty() || condition.EmailTo.IsNotNullOrEmpty()))
                    {
                        emailActiveInfo = emailActiveInfo;
                    }
                    else
                    {
                        emailActiveInfo = emailActiveInfo.Where(p => p.email.CREATEDDATE <= condition.DateTo.Value);
                    }
                }
                else
                {
                    emailActiveInfo = emailActiveInfo.Where(p => p.email.CREATEDDATE <= condition.DateTo.Value);
                }
                
            }

            if (condition.Status.HasValue)
            {
                emailActiveInfo = emailActiveInfo.Where(p => p.email.SENDSTATUS == condition.Status.Value);
            }

            if (condition.EmailTo.IsNotNullOrEmpty())
            {
                emailActiveInfo = emailActiveInfo.Where(p => p.email.EMAILTO.ToUpper().Contains(condition.EmailTo.ToUpper()));
            }

            if (condition.EmailType.HasValue)
            {
                if (condition.EmailType.Value == (int)TypeEmailActive.DailyEmail)
                {
                    emailActiveInfo = emailActiveInfo.Where(p => p.email.TYPEEMAIL == condition.EmailType.Value || p.email.TYPEEMAIL == (int)TypeEmailActive.MonthlyEmail);
                }
                else
                {
                    emailActiveInfo = emailActiveInfo.Where(p => p.email.TYPEEMAIL == condition.EmailType.Value);
                }
            }

            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                emailActiveInfo = emailActiveInfo.Where(p => p.client.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper().Trim()) 
                || p.annClient.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper().Trim()) || 
                p.annAccountClient.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper().Trim()));
            }

            var res = emailActiveInfo.Select(x => new EmailActiveInfo
            {
                Id = x.email.ID,
                ContentEmail = x.email.CONTENTEMAIL,
                Title = x.email.TITLE,
                EmailTo = x.email.EMAILTO,
                SendStatus = x.email.SENDSTATUS ?? 0,
                SendtedDate = x.email.SENDTEDDATE,
                TypeEmail = x.email.TYPEEMAIL ?? 0,
                CreatedDate = x.email.CREATEDDATE,
                CompanyId = x.email.COMPANYID,
                CompanyName = x.myCompany.COMPANYNAME ?? x.annCompany.COMPANYNAME,
                CustomerCode = x.client.CUSTOMERCODE ?? x.annClient.CUSTOMERCODE ?? x.annAccountClient.CUSTOMERCODE,
                BranchId = x.myCompany.BRANCHID ?? x.mycompanyAccountClient.BRANCHID
            });
            return res;
        }

        //
        public long Count(ConditionSearchEmailActive condition)
        {
            return QueryFilter(condition).Count();
        }

        public EMAILACTIVE GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }

        public void InsertMailInvoice(EmailInfo emailInfo)
        {
            var db = new DataClassesDataContext();
            //var listEmail = (emailInfo.EmailTo ?? "").Split(new char[] { ',' });
            //foreach (var email in listEmail)
            //{
            db.SP_INSERT_EMAIL_INVOICE(emailInfo.TemplateMailId, emailInfo.CustomerCode, emailInfo.EmailTo, emailInfo.EmailTo,
                        emailInfo.SenderName, emailInfo.SenderEmail, emailInfo.InvoiceData, emailInfo.CountPathFile, emailInfo.PathFile1, null, null, emailInfo.EmailActiveId);
            //}
        }

        public void AddRange(List<EMAILACTIVE> emailActives)
        {
            var dbContext = this.context as DataClassesDataContext;
            dbContext.BulkInsert(emailActives, bulk =>
            {
                bulk.SkipPropagationAndAcceptChanges = false;
                bulk.BatchSize = 1000;
                bulk.BatchTimeout = 3600;
            });
        }

        public long? checkEmailExists(long refId)
        {
            var emailquery =
                from emailActive in this.dbSet
                join emailReference in this.context.Set<EMAILACTIVEREFERENCE>() on emailActive.ID equals emailReference.EMAILACTIVEID
                where emailActive.TYPEEMAIL == (int)TypeEmailActive.ProvideAccountEmail
                    && emailReference.REFID == refId
                select emailActive;
            var email = emailquery.FirstOrDefault();
            if (!email.IsNullOrEmpty())
            {
                return email.ID;
            }

            return null;
        }

        public long GetRefIdByEmailActiveId(long emailActiveId)
        {
            var query = from emailActive in this.dbSet
                        join emailReference in this.context.Set<EMAILACTIVEREFERENCE>() on emailActive.ID equals emailReference.EMAILACTIVEID
                        where emailActive.ID == emailActiveId && emailReference.REFID != 0
                        select emailReference.REFID;
            return query.Distinct().FirstOrDefault() ?? 0;
        }

        public List<long> GetRefIdsByEmailActiveId(long emailActiveId)
        {
            var query = from emailActive in this.dbSet
                        join emailReference in this.context.Set<EMAILACTIVEREFERENCE>() on emailActive.ID equals emailReference.EMAILACTIVEID
                        where emailActive.ID == emailActiveId && emailReference.REFID != 0
                        select emailReference.REFID ?? 0;
            return query.Distinct().ToList();
        }

        public Dictionary<long, EmailActiveInfo> GetListInvoiceNoDateFromInvoice(List<long> listEmailId)
        {
            var query = from emailActive in this.dbSet
                        join emailReference in this.context.Set<EMAILACTIVEREFERENCE>() on emailActive.ID equals emailReference.EMAILACTIVEID into emailReferenceTemp
                        from emailReference in emailReferenceTemp.DefaultIfEmpty()
                        join invoice in this.context.Set<INVOICE>() on emailReference.REFID equals invoice.ID into invoiceTemp
                        from invoice in invoiceTemp.DefaultIfEmpty()
                        where listEmailId.Contains(emailActive.ID) && invoice.RELEASEDDATE != null
                        group invoice by emailActive.ID into g
                        select new { EmailId = g.Key, Invoice = g.Select(inv => new { inv.NO, inv.RELEASEDDATE, inv.TELLER }) };
            var listNoByEmailId = query.ToList();   // List InvoiceNo, ReleaseDate theo từng email
            var res = listNoByEmailId
                .Select(x => new
                {
                    x.EmailId,
                    InvoiceNoString = string.Join(",", x.Invoice.Where(inv => !string.IsNullOrEmpty(inv.NO)).Select(inv => inv.NO)),
                    DateInvoice = string.Join(",", x.Invoice.Where(inv => inv.RELEASEDDATE.HasValue).Select(inv => inv.RELEASEDDATE.Value.ToString("dd/MM/yyy"))),
                    Teller = string.Join(",", x.Invoice.Where(inv => !string.IsNullOrEmpty(inv.TELLER)).Select(inv => inv.TELLER))
                }) // Remove InvoiceNo rỗng trong từng email, rồi join lại
                .ToDictionary((key => key.EmailId), (value => new EmailActiveInfo() { InvoiceNo = value.InvoiceNoString, DateInvoice = value.DateInvoice, Teller = value.Teller }));
            return res;
        }

        public Dictionary<long, EmailActiveInfo> GetListInvoiceNoDateFromAnnouncement(List<long> listEmailId)
        {
            var query = from emailActive in this.dbSet
                        join emailReference in this.context.Set<EMAILACTIVEREFERENCE>() on emailActive.ID equals emailReference.EMAILACTIVEID into emailReferenceTemp
                        from emailReference in emailReferenceTemp.DefaultIfEmpty()
                        join announcement in this.context.Set<ANNOUNCEMENT>() on emailReference.REFID equals announcement.ID into announcementTemp
                        from announcement in announcementTemp.DefaultIfEmpty()
                        where listEmailId.Contains(emailActive.ID) && announcement.INVOICEDATE != null
                        group announcement by emailActive.ID into g
                        select new { EmailId = g.Key, Announcement = g.Select(ann => new { ann.INVOICENO, ann.INVOICEDATE }) };
            var listNoByEmailId = query.ToList();   // List InvoiceNo, ReleaseDate theo từng email
            var res = listNoByEmailId
                .Select(x => new
                {
                    x.EmailId,
                    InvoiceNoString = string.Join(",", x.Announcement.Where(ann => !string.IsNullOrEmpty(ann.INVOICENO)).Select(ann => ann.INVOICENO)),
                    DateInvoice = string.Join(",", x.Announcement.Select(inv => inv.INVOICEDATE.ToString("dd/MM/yyy"))),
                }) // Remove InvoiceNo rỗng trong từng email, rồi join lại
                .ToDictionary((key => key.EmailId), (value => new EmailActiveInfo() { InvoiceNo = value.InvoiceNoString, DateInvoice = value.DateInvoice }));
            return res;
        }
    }
}
