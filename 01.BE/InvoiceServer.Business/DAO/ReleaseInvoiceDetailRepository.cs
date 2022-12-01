using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class ReleaseInvoiceDetailRepository : GenericRepository<RELEASEINVOICEDETAIL>, IReleaseInvoiceDetaiRepository
    {
        public ReleaseInvoiceDetailRepository(IDbContext context)
            : base(context)
        {
        }
        public IEnumerable<InvoicesRelease> FilteInvoiceRelease(long releaseInvoiceId, long companyId, List<int> releaseStatus)
        {
            var invoicesRelease = from releaseDetail in this.dbSet
                                  join invoice in this.context.Set<INVOICE>()
                                  on releaseDetail.INVOICEID equals invoice.ID
                                  join registerTempalate in this.context.Set<REGISTERTEMPLATE>()
                                  on invoice.REGISTERTEMPLATEID equals registerTempalate.ID
                                  join tempalate in this.context.Set<INVOICETEMPLATE>()
                                  on registerTempalate.INVOICETEMPLATEID equals tempalate.ID
                                  where releaseDetail.RELEASEINVOICEID == releaseInvoiceId && releaseDetail.RELEASEINVOICE.COMPANYID == companyId
                                  && releaseStatus.Contains(releaseDetail.RELEASEINVOICE.STATUS ?? 0)
                                  select new InvoicesRelease()
                                  {
                                      InvoiceId = (releaseDetail.INVOICEID ?? 0),
                                      Signed = (releaseDetail.SIGNED ?? false),
                                      InvoiceNo = invoice.NO,
                                      ReleasedDate = (invoice.RELEASEDDATE ?? DateTime.Now),
                                      Printed = (releaseDetail.PRINTED ?? false),
                                      LocationSignLeft = (double)tempalate.LOCATIONSIGNLEFT,
                                      LocationSignRight = (double)(tempalate.LOCATIONSIGNRIGHT ?? 0),
                                      LocationSignButton = (double)tempalate.LOCATIONSIGNBUTTON,

                                  };
            return invoicesRelease;
        }
        public RELEASEINVOICEDETAIL GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }

        public RELEASEINVOICEDETAIL FilteReleaseInvoiceDetail(long releaseId, long invoiceId)
        {
            return this.dbSet.FirstOrDefault(p => p.RELEASEINVOICEID == releaseId && p.INVOICEID == invoiceId);
        }
        /// <summary>
        ///  Lấy danh sách hóa đơn release detail
        /// </summary>
        /// <param name="invoiceIds"></param>
        /// <returns></returns>
        public List<RELEASEINVOICEDETAIL> GetByInvoiceIds(List<long> invoiceIds)
        {
            return this.dbSet.Where(p => p.INVOICEID.HasValue && invoiceIds.Contains(p.INVOICEID.Value)).ToList();
        }
        public RELEASEINVOICEDETAIL FilteReleaseInvoiceDetail(long invoiceId)
        {
            return this.dbSet.FirstOrDefault(p => p.INVOICEID == invoiceId && p.VERIFICATIONCODE != null);
        }
        public IEnumerable<RELEASEINVOICEDETAIL> GetListByReleaseInvoiceID(long releaseId)
        {
            return this.dbSet.Where(p => p.RELEASEINVOICEID == releaseId);
        }

       
        public void InsertMultipe(List<RELEASEINVOICEDETAIL> releaseInvoiceDetails)
        {
            var dbContext = this.context as DataClassesDataContext;
            dbContext.BulkInsert(releaseInvoiceDetails);
        }
    }
}
