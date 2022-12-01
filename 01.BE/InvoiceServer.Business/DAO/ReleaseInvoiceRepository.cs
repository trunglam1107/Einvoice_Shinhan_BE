using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class ReleaseInvoiceRepository : GenericRepository<RELEASEINVOICE>, IReleaseInvoiceRepository
    {
        public ReleaseInvoiceRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<ReleaseInvoices> FilterReleaseInvoice(ConditionSearchReleaseInvoice condition)
        {
            var releaseInvoice = this.dbSet.AsQueryable();
            var invoices = this.context.Set<INVOICE>().Where(p => p.COMPANYID == condition.CompanyId && !(p.DELETED ?? false));
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));

            if (condition.InvoiceSampleId.HasValue)
            {
                invoices = invoices.Where(p => p.REGISTERTEMPLATEID == condition.InvoiceSampleId.Value);
            }

            if (condition.InvoiceStatus.HasValue)
            {
                invoices = invoices.Where(p => p.INVOICESTATUS == condition.InvoiceStatus.Value);
            }

            if (condition.InvoiceNo.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.NO.Equals(condition.InvoiceNo));
            }

            if (condition.NumberAccout.IsNotNullOrEmpty())
            {
                invoices = invoices.Where(p => p.NUMBERACCOUT.Equals(condition.NumberAccout));
            }

            if (condition.InvoiceSample.HasValue)
            {
                invoices = invoices.Where(p => p.INVOICETYPE == condition.InvoiceSample);
            }

            if (condition.DateFrom.HasValue)
            {
                releaseInvoice = releaseInvoice.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                releaseInvoice = releaseInvoice.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
            }
            if (condition.IsInvoiceChange)
            {
                invoices = invoices.Where(p => (p.INVOICETYPE ?? 0) == 0);
            }

            if (condition.CustomerName.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.Equals(condition.CustomerName));
            }

            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.Equals(condition.TaxCode));
            }

            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var releaseInvoices = from release in releaseInvoice
                                  join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>()
                                      on release.ID equals releaseDetail.RELEASEINVOICEID
                                  join invoice in invoices
                                      on releaseDetail.INVOICEID equals invoice.ID
                                  join registerTempale in this.context.Set<REGISTERTEMPLATE>()
                                      on invoice.REGISTERTEMPLATEID equals registerTempale.ID
                                  join invoicetemp in this.context.Set<INVOICESAMPLE>()
                                      on registerTempale.INVOICESAMPLEID equals invoicetemp.ID
                                  join client in clients
                                      on invoice.CLIENTID equals client.ID
                                  let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                  select new ReleaseInvoices
                                  {
                                      Id = invoice.ID,
                                      InvoiceCode = invoicetemp.CODE,
                                      InvoiceSymbol = invoice.SYMBOL,
                                      InvoiceNo = invoice.NO,
                                      CustomerName = notCurrentClient ? client.CUSTOMERNAME : invoice.CUSTOMERNAME,
                                      InvoiceDate = invoice.CREATEDDATE,
                                      InvoiceNote = invoice.NOTE,
                                      Status = releaseDetail.MESSAGES,
                                      ReleaseId = releaseDetail.ID,
                                  };

            return releaseInvoices;
        }

        public IEnumerable<RELEASEINVOICE> FilterInvoice(ReleaseGroupInvoice condition)
        {
            throw new System.NotImplementedException();
        }

        public RELEASEINVOICE GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }

        public RELEASEINVOICE GetById(long id, long companyId)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id && p.COMPANYID == companyId);
        }

        public IEnumerable<RELEASEINVOICE> SendEmailNotifice(IList<int> status)
        {
            return this.dbSet.Where(p => status.Contains((p.STATUS ?? 0)));
        }

        public RELEASEINVOICE FilterInvoice(long id, IList<int> status)
        {
            return this.dbSet.FirstOrDefault(p => status.Contains((p.STATUS ?? 0)) && p.ID == id);
        }

        public IEnumerable<ReleaseListInvoice> FilterReleaseInvoice(ConditionSearchReleaseList condition)
        {
            var releaseInvoices = this.dbSet.Where(p => p.COMPANYID == condition.CompanyId);

            if (condition.Status.HasValue)
            {
                if (condition.Status.Value < 2)
                {
                    releaseInvoices = releaseInvoices.Where(p => (p.STATUS ?? 0) == condition.Status.Value);
                }
                else
                {
                    releaseInvoices = releaseInvoices.Where(p => (p.STATUS ?? 0) > condition.Status.Value);
                }
            }

            if (condition.DateFrom.HasValue)
            {
                releaseInvoices = releaseInvoices.Where(p => p.RELEASEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                releaseInvoices = releaseInvoices.Where(p => p.RELEASEDDATE <= condition.DateTo.Value);
            }

            var invoiceRelease = from release in releaseInvoices
                                 select new ReleaseListInvoice
                                 {
                                     Id = release.ID,
                                     Description = release.DESCRIPTION,
                                     ReleasedDate = release.RELEASEDDATE,
                                     Status = (release.STATUS ?? 0) > 1 ? 2 : (release.STATUS ?? 0),
                                     UserRelease = release.LOGINUSER == null ? string.Empty : release.LOGINUSER.USERNAME ?? string.Empty,
                                 };

            return invoiceRelease;
        }

        public IEnumerable<InvoicesReleaseDetail> ReleaseInvoiceDetail(ConditionSearchReleaseListDetail condition)
        {
            var releaseInvoices = this.dbSet.Where(p => p.COMPANYID == condition.CompanyId && p.ID == condition.ReleaseId);
            var releaseInvoiceDetail = from release in releaseInvoices
                                       join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>()
                                       on release.ID equals releaseDetail.RELEASEINVOICEID
                                       join invoice in this.context.Set<INVOICE>()
                                       on releaseDetail.INVOICEID equals invoice.ID
                                       select new InvoicesReleaseDetail()
                                       {
                                           Id = invoice.ID,
                                           InvoiceCode = invoice.REGISTERTEMPLATE.CODE,
                                           InvoiceSymbol = invoice.SYMBOL,
                                           InvoiceNo = invoice.NO,
                                           CustomerName = invoice.CLIENT.CUSTOMERNAME,
                                           TaxCode = invoice.CLIENT.TAXCODE,
                                           InvoiceDate = invoice.CREATEDDATE,
                                       };

            return releaseInvoiceDetail;
        }

        public RELEASEINVOICE GetByInvoiceId(long invoiceId)
        {
            var releaseInfo = from release in this.dbSet
                              join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>()
                              on release.ID equals releaseDetail.RELEASEINVOICEID
                              where releaseDetail.INVOICEID == invoiceId
                              select release;
            return releaseInfo.FirstOrDefault();
        }

        public long? GetCompanyIdByVerifiCode(string VerifiCod)
        {
            //var releaseInfo = from release in this.dbSet
            //                  join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>()
            //                  on release.ID equals releaseDetail.RELEASEINVOICEID
            //                  join invoice in this.context.Set<INVOICE>()
            //                  on releaseDetail.INVOICEID equals invoice.ID
            //                  where invoice.VERIFICATIONCODE == VerifiCod
            //                  select release;

            var releaseInfo = from invoice in this.context.Set<INVOICE>()
                              where invoice.VERIFICATIONCODE == VerifiCod
                              select invoice;

            if (releaseInfo.Count() > 0)
            {
                return releaseInfo.FirstOrDefault().COMPANYID;
            }
            else
            {
                return 0;
            }

            
        }
        public IEnumerable<InvoicesReleaseDetail> GetInvoiceByReleasedId(long releaseId)
        {
            var releaseInvoices = this.dbSet.Where(p => p.ID == releaseId);
            var releaseInvoiceDetail = from release in releaseInvoices
                                       join releaseDetail in this.context.Set<RELEASEINVOICEDETAIL>()
                                       on release.ID equals releaseDetail.RELEASEINVOICEID
                                       join invoice in this.context.Set<INVOICE>()
                                       on releaseDetail.INVOICEID equals invoice.ID
                                       select new InvoicesReleaseDetail()
                                       {
                                           Id = invoice.ID,
                                           InvoiceCode = invoice.REGISTERTEMPLATE.CODE,
                                           InvoiceSymbol = invoice.SYMBOL,
                                           InvoiceNo = invoice.NO,
                                           CustomerName = invoice.CLIENT.CUSTOMERNAME,
                                           TaxCode = invoice.CLIENT.TAXCODE,
                                           InvoiceDate = invoice.RELEASEDDATE ?? DateTime.Now,
                                           CompanyId = invoice.COMPANYID
                                       };

            return releaseInvoiceDetail;
        }
        public IEnumerable<InvoiceMaster> getResponeCQT(long InvoiceId)
        {
            var invoices = this.context.Set<INVOICE>().Where(p => !(p.DELETED ?? false));
            var invoiceInfo = (from invoice in invoices
                               join minvoice in this.context.Set<MINVOICE_DATA>()
                               on invoice.MESSAGECODE equals minvoice.MESSAGECODE
                               where invoice.ID == InvoiceId
                               select new InvoiceMaster()
                               {
                                   statusCQT = minvoice.STATUS,
                                   MessageError = minvoice.ERROR,
                                   codeCQT = minvoice.MCQT
                               }).AsQueryable();

            return invoiceInfo;
        }
    }
}
