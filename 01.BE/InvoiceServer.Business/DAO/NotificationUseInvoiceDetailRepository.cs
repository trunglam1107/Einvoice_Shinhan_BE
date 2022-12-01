using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class NotificationUseInvoiceDetailRepository : GenericRepository<NOTIFICATIONUSEINVOICEDETAIL>, INotificationUseInvoiceDetailRepository
    {
        private readonly MyCompanyRepository myCompanyRepository;
        public NotificationUseInvoiceDetailRepository(IDbContext context)
            : base(context)
        {
            myCompanyRepository = new MyCompanyRepository(context);
        }

        public IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> Filter(long invoiceUseId)
        {
            return this.GetInvoiceConcludeActive().Where(p => p.NOTIFICATIONUSEINVOICEID == invoiceUseId);
        }

        public NOTIFICATIONUSEINVOICEDETAIL GetById(long id)
        {
            return GetInvoiceConcludeActive().FirstOrDefault(p => p.ID == id);
        }

        public IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> FilterInvoiceSymbol(long companyId, long invoiceTemplateId)
        {
            IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> notificationUseInvoiceDetails;

            var levelCustomer = this.myCompanyRepository.GetLevelCustomer(companyId);
            notificationUseInvoiceDetails = GetInvoiceConcludeActive().Where(p => p.REGISTERTEMPLATESID == invoiceTemplateId
                                                    && !(p.NOTIFICATIONUSEINVOICE.DELETED ?? false)
                                                    && p.NOTIFICATIONUSEINVOICE.STATUS == (int)RecordStatus.Approved
                                                    && p.USEDDATE <= DateTime.Now
                                                    && !(p.DELETED ?? false));
            if (levelCustomer.LEVELCUSTOMER != LevelCustomerInfo.HO)
            {
                notificationUseInvoiceDetails = notificationUseInvoiceDetails.Where(p => p.COMPANYID == companyId);
            }
            return notificationUseInvoiceDetails;
        }

        private IQueryable<NOTIFICATIONUSEINVOICEDETAIL> GetInvoiceConcludeActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false));
        }

        public bool ContainSymbol(long companyId, string code)
        {
            return Contains(p => !(p.DELETED ?? false) && p.NOTIFICATIONUSEINVOICE.COMPANYID == companyId && p.CODE.Equals(code));
        }

        public IEnumerable<NoticePrintInvoice> FilterSeftPrint(ConditionReportUse codition)
        {
            var NotificationUseInvoiceDetail = this.dbSet.Where(p => !(p.DELETED ?? false) && p.NOTIFICATIONUSEINVOICE.COMPANYID == codition.CompanyId && !(p.NOTIFICATIONUSEINVOICE.DELETED ?? false));

            if (codition.DateFrom.HasValue)
            {
                NotificationUseInvoiceDetail = NotificationUseInvoiceDetail.Where(p => p.USEDDATE >= codition.DateFrom.Value);
            }

            if (codition.DateTo.HasValue)
            {
                NotificationUseInvoiceDetail = NotificationUseInvoiceDetail.Where(p => p.USEDDATE <= codition.DateTo.Value);
            }

            var noticePrintInfo = from notice in NotificationUseInvoiceDetail
                                  join templateCompanyUse in this.context.Set<REGISTERTEMPLATE>()
                                      on notice.REGISTERTEMPLATESID equals templateCompanyUse.ID
                                  select new NoticePrintInvoice
                                  {
                                      InvoiceSample = templateCompanyUse.INVOICESAMPLE.NAME,
                                      InvoiceCode = templateCompanyUse.INVOICESAMPLE.CODE,
                                      InvoiceSymbol = notice.CODE,
                                      InvoiceSampleCode = templateCompanyUse.INVOICESAMPLE.CODE,
                                      NumberToUse = notice.NUMBERTO,
                                      NumberFromUse = notice.NUMBERFROM,
                                      NumberTotall = notice.NUMBERUSE

                                  };
            return noticePrintInfo;

        }

        public IEnumerable<NoticeCancelInvoice> FilterCancelInvoice(ConditionReportCancelInvoice codition)
        {
            var NotificationUseInvoiceDetail = this.dbSet.Where(p => !(p.DELETED ?? false) && p.NOTIFICATIONUSEINVOICE.COMPANYID == codition.CompanyId && !(p.NOTIFICATIONUSEINVOICE.DELETED ?? false));

            if (codition.InvoiceSampleId.HasValue)
            {
                NotificationUseInvoiceDetail = NotificationUseInvoiceDetail.Where(p => p.ID == codition.InvoiceSampleId.Value);
            }

            if (codition.DateFrom.HasValue)
            {
                NotificationUseInvoiceDetail = NotificationUseInvoiceDetail.Where(p => p.USEDDATE <= codition.DateFrom.Value);
            }

            var noiticeCancelInvoice = from notice in NotificationUseInvoiceDetail
                                       join registerTemplate in this.context.Set<REGISTERTEMPLATE>()
                                      on notice.REGISTERTEMPLATESID equals registerTemplate.ID
                                       select new NoticeCancelInvoice
                                       {
                                           InvoiceSample = registerTemplate.INVOICESAMPLE.NAME,
                                           InvoiceCode = registerTemplate.INVOICESAMPLE.CODE,
                                           InvoiceSymbol = notice.CODE,
                                           InvoiceSampleCode = registerTemplate.INVOICESAMPLE.CODE,
                                           NumberToUse = notice.NUMBERTO,
                                           NumberFromUse = notice.NUMBERFROM,
                                           NumberTotall = notice.NUMBERUSE,
                                           NotificationUseInvoiceDetailId = notice.ID
                                       };
            return noiticeCancelInvoice;

        }

        public IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetAllNotificationUseInvoiceDetails(long RegisterTemplatesId, string symbol, DateTime? dateTo)
        {
            var notificationUseInvoiceDetail = this.dbSet.Where(p => !(p.DELETED ?? false)
                && p.NOTIFICATIONUSEINVOICE.STATUS == (int)RecordStatus.Approved
                && !(p.NOTIFICATIONUSEINVOICE.DELETED ?? false)
                && p.REGISTERTEMPLATESID == RegisterTemplatesId
                && p.CODE.Replace("/", "").Equals(symbol)
                && p.USEDDATE <= dateTo);

            return notificationUseInvoiceDetail;
        }

        public IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNotificationUseInvoiceDetails(long companyId, long RegisterTemplatesId, string symbol, DateTime? dateTo)
        {
            var rpCancel = from rp in this.context.Set<REPORTCANCELLINGDETAIL>()
                           where rp.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved
                           select rp.SYMBOL;
            var notificationUseInvoiceDetail = this.dbSet.Where(p => !(p.DELETED ?? false)
                && p.NOTIFICATIONUSEINVOICE.STATUS == (int)RecordStatus.Approved
                && p.NOTIFICATIONUSEINVOICE.COMPANYID == companyId
                && !(p.NOTIFICATIONUSEINVOICE.DELETED ?? false)
                && p.REGISTERTEMPLATESID == RegisterTemplatesId
                && p.CODE.Replace("/", "").Equals(symbol)
                && !(rpCancel.Contains(p.CODE)));

            return notificationUseInvoiceDetail;
        }
        public IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNotificationUseInvoiceDetailsRpUs(long companyId, long RegisterTemplatesId, string symbol, DateTime? dateFrom, DateTime? dateTo)
        {
            var rpCancel = from rpdt in this.context.Set<REPORTCANCELLINGDETAIL>()
                           join rp in this.context.Set<REPORTCANCELLING>() on rpdt.REPORTCANCELLINGID equals rp.ID
                           where rp.APPROVEDDATE < dateFrom && rp.STATUS == (int)ReportCancellingStatus.Approved
                           select rpdt.SYMBOL;
            var notificationUseInvoiceDetail = this.dbSet.Where(p => !(p.DELETED ?? false)
                && p.NOTIFICATIONUSEINVOICE.STATUS == (int)RecordStatus.Approved
                && p.NOTIFICATIONUSEINVOICE.COMPANYID == companyId
                && !(p.NOTIFICATIONUSEINVOICE.DELETED ?? false)
                && p.REGISTERTEMPLATESID == RegisterTemplatesId
                && p.CODE.Replace("/", "").Equals(symbol)
                && p.USEDDATE < dateTo
                && !(rpCancel.Contains(p.CODE)));
            return notificationUseInvoiceDetail;
        }
        public IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNotificationUseInvoiceDetails(long companyId, long RegisterTemplatesId, string symbol)
        {
            symbol = symbol.Replace(" ", "");
            var notificationUseInvoiceDetail = this.dbSet.Where(p => !(p.DELETED ?? false)
                && p.NOTIFICATIONUSEINVOICE.STATUS == (int)RecordStatus.Approved
                && p.COMPANYID == companyId && !(p.NOTIFICATIONUSEINVOICE.DELETED ?? false)
                && p.REGISTERTEMPLATESID == RegisterTemplatesId
                && p.CODE.Replace("/", "").Equals(symbol));

            return notificationUseInvoiceDetail;
        }

        public IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNoticeUseInvoiceDetailOfCompany(long companyId)
        {
            var notificationUseInvoiceDetail = this.dbSet.Where(p => !(p.DELETED ?? false)
               && p.NOTIFICATIONUSEINVOICE.STATUS == (int)RecordStatus.Approved
               && p.NOTIFICATIONUSEINVOICE.COMPANYID == companyId && !(p.NOTIFICATIONUSEINVOICE.DELETED ?? false)
               );

            return notificationUseInvoiceDetail;
        }

        public IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetAllNoticeUseInvoiceDetail()
        {
            var notificationUseInvoiceDetail = this.dbSet.Where(p => !(p.DELETED ?? false)
               && p.NOTIFICATIONUSEINVOICE.STATUS == (int)RecordStatus.Approved
               && !(p.NOTIFICATIONUSEINVOICE.DELETED ?? false)
               );
            return notificationUseInvoiceDetail;
        }

        public IEnumerable<RegisterTemplateCancelling> FilterNotificationUseInvoiceDetailsUse(long companyId, long invoiceSampleId)
        {

            var registerTemplateCancelling = from notificationDetail in this.dbSet
                                             join notification in this.context.Set<NOTIFICATIONUSEINVOICE>()
                                             on notificationDetail.NOTIFICATIONUSEINVOICEID equals notification.ID
                                             join template in this.context.Set<REGISTERTEMPLATE>()
                                             on notificationDetail.REGISTERTEMPLATESID equals template.ID

                                             where notification.COMPANYID == companyId && notification.STATUS == (int)RecordStatus.Approved
                                             && template.INVOICESAMPLEID == invoiceSampleId
                                             select new RegisterTemplateCancelling()
                                             {
                                                 CompanyId = notification.COMPANYID,
                                                 RegisterTemplatesId = template.ID,
                                                 Code = template.CODE,
                                                 Symbol = notificationDetail.CODE,
                                                 NumberFrom = notificationDetail.NUMBERFROM,
                                                 NumberTo = notificationDetail.NUMBERTO,
                                             };

            return registerTemplateCancelling;
        }

        public IEnumerable<RegisterTemplateReport> FilterNotificationUseInvoiceDetailsUseReport(long companyId, long? branch, DateTime dateFrom, DateTime? dateTo)
        {
            long? branchChosen = 0;
            if (branch.HasValue && branch > 0)
            {
                branchChosen = branch;
            }
            else
            {
                branchChosen = companyId;
            }

            var registerTemplateReport = from notificationDetail in this.dbSet
                                         join notification in this.context.Set<NOTIFICATIONUSEINVOICE>()
                                         on notificationDetail.NOTIFICATIONUSEINVOICEID equals notification.ID
                                         join template in this.context.Set<REGISTERTEMPLATE>()
                                         on notificationDetail.REGISTERTEMPLATESID equals template.ID
                                         join invoiceSample in this.context.Set<INVOICESAMPLE>()
                                        on template.INVOICESAMPLEID equals invoiceSample.ID
                                         join invoiceType in this.context.Set<INVOICETYPE>()
                                         on invoiceSample.INVOICETYPEID equals invoiceType.ID
                                         where (notification.COMPANYID == branchChosen)
                                                && notification.STATUS == (int)RecordStatus.Approved
                                                && notificationDetail.USEDDATE < dateTo
                                         select new RegisterTemplateReport()
                                         {
                                             CompanyId = notification.COMPANYID,
                                             RegisterTemplatesId = template.ID,
                                             RegisterTemplatesCode = template.CODE,
                                             NotificationUseCode = notificationDetail.CODE,
                                             InvoiceSampleName = invoiceType.NAME,
                                             TypeCodeInvoice = invoiceType.CODE,
                                         };

            return registerTemplateReport.GroupBy(p => new { p.CompanyId, p.RegisterTemplatesId, p.RegisterTemplatesCode, p.NotificationUseCode, p.InvoiceSampleName, p.TypeCodeInvoice })
                                                          .Select(g => g.FirstOrDefault()).OrderBy(p => p.RegisterTemplatesId)
                                                          .ToList();
        }

        public IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNumberInvoiceRegisterBetweenDate(long companyId, long registerId, string symbol, DateTime? dateFrom, DateTime? dateTo)
        {
            var rpCancel = from rp in this.context.Set<REPORTCANCELLINGDETAIL>()
                           where rp.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved
                           select rp.SYMBOL;
            return this.dbSet.Where(p => (p.NOTIFICATIONUSEINVOICE.COMPANYID == companyId)
                && p.REGISTERTEMPLATESID == registerId
                && p.CODE.Equals(symbol)
                && p.NOTIFICATIONUSEINVOICE.STATUS == (int)RecordStatus.Approved
                && !(rpCancel.Contains(p.CODE)));
        }
        public IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNumberInvoiceRegisterBetweenDateReport(long companyId, long registerId, string symbol, DateTime? dateFrom, DateTime? dateTo)
        {
            var rpCancel = from rp in this.context.Set<REPORTCANCELLINGDETAIL>()
                           select rp.SYMBOL;
            return this.dbSet.Where(p => (p.NOTIFICATIONUSEINVOICE.COMPANYID == companyId)
                && p.REGISTERTEMPLATESID == registerId
                && p.CODE.Equals(symbol)
                && p.NOTIFICATIONUSEINVOICE.STATUS == (int)RecordStatus.Approved);
            //&& !(rpCancel.Contains(p.CODE)));
        }

        public NOTIFICATIONUSEINVOICEDETAIL GetByCompanyId(long companyId, long registerTemplatesId)
        {
            return this.dbSet.Where(p => (p.NOTIFICATIONUSEINVOICE.COMPANYID == companyId)
                   && p.NOTIFICATIONUSEINVOICE.STATUS == (int)RecordStatus.Approved
                   && p.REGISTERTEMPLATESID == registerTemplatesId).FirstOrDefault();
        }

        public List<NoticeUseInvoiceDetailInfo> GetByRelease(long releaseId)
        {
            var noti = (from notificationDetail in this.dbSet
                        join releaseDetail in this.context.Set<INVOICERELEASESDETAIL>() on notificationDetail.REGISTERTEMPLATESID equals releaseDetail.REGISTERTEMPLATESID
                        join notification in this.context.Set<NOTIFICATIONUSEINVOICE>() on notificationDetail.NOTIFICATIONUSEINVOICEID equals notification.ID
                        where releaseDetail.INVOICECONCLUDEID == releaseId
                        select new NoticeUseInvoiceDetailInfo
                        {
                            NotificationId = notificationDetail.NOTIFICATIONUSEINVOICEID ?? 0
                        }).ToList();
            return noti;
        }

        public bool UsedByInvoices(long NotiId)
        {
            var invoiceBySymbol = from useInvoiceDetail in this.dbSet
                                  join invoice in this.context.Set<INVOICE>().Where(p => !(p.DELETED ?? false))
                                  on useInvoiceDetail.CODE equals invoice.SYMBOL
                                  where useInvoiceDetail.NOTIFICATIONUSEINVOICEID == NotiId && useInvoiceDetail.REGISTERTEMPLATESID == invoice.REGISTERTEMPLATEID
                                  select invoice.ID;
            if (invoiceBySymbol?.Any() == true)
            {
                return true;
            }

            return false;
        }
    }
}
