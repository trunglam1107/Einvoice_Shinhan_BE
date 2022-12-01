using InvoiceServer.Business.Models;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class ReportCancellingDetailRepository : GenericRepository<REPORTCANCELLINGDETAIL>, IReportCancellingDetailRepository
    {
        public ReportCancellingDetailRepository(IDbContext context)
            : base(context)
        {
        }

        public REPORTCANCELLINGDETAIL GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }

        public bool ContainRegisterTemplate(long registerTemplateId, string symbol, long reportCancellingId)
        {
            return Contains(p => reportCancellingId == 0 ?
                        (p.REGISTERTEMPLATESID == registerTemplateId && p.SYMBOL.Equals(symbol))
                        : (p.REGISTERTEMPLATESID == registerTemplateId && p.SYMBOL.Equals(symbol) && p.REPORTCANCELLINGID != reportCancellingId));
        }

        public IEnumerable<REPORTCANCELLINGDETAIL> Filter(long companyId, long registerTemplateId)
        {
            return this.dbSet.Where(p => p.REPORTCANCELLING.COMPANYID == companyId && p.REGISTERTEMPLATESID == registerTemplateId
            && p.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved
            );
        }
        public IEnumerable<REPORTCANCELLINGDETAIL> FilterBy(long companyId, long reportCancellingId)
        {
            return this.dbSet.Where(p => p.REPORTCANCELLING.COMPANYID == companyId && p.REPORTCANCELLINGID == reportCancellingId && p.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved);
        }

        public REPORTCANCELLINGDETAIL Filter(long companyId, long registerTemplateId, string symbol, DateTime? dateFrom, DateTime? dateTo)
        {
            var reportDetail = (from rpd in this.dbSet
                                join rp in this.context.Set<REPORTCANCELLING>() on rpd.REPORTCANCELLINGID equals rp.ID
                                where rp.STATUS == (int)ReportCancellingStatus.Approved && (rpd.REPORTCANCELLING.COMPANYID == companyId)
                                     && rpd.REGISTERTEMPLATESID == registerTemplateId
                                     && rpd.SYMBOL.Equals(symbol)
                                     && rpd.REPORTCANCELLING.APPROVEDDATE >= dateFrom
                                     && rpd.REPORTCANCELLING.APPROVEDDATE < dateTo
                                select rpd).FirstOrDefault();
            return reportDetail;
            //this.dbSet.FirstOrDefault(p => (p.REPORTCANCELLING.COMPANYID == companyId)
            //&& p.REGISTERTEMPLATESID == registerTemplateId
            //&& p.SYMBOL.Equals(symbol)
            //&& p.REPORTCANCELLING.CANCELLINGDATE >= dateFrom);
        }

        public long GetNumberInvoiceCancelling(ConditionInvoiceUse condition)
        {
            var numCancelling = this.dbSet.Where(p => (p.REPORTCANCELLING.COMPANYID == condition.CompanyId)
                                                   && p.REGISTERTEMPLATESID == condition.RegisterTemplateId
                                                   && p.SYMBOL.Equals(condition.Symbol)
                                                   && p.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved)
                                         .GroupBy(p => new { p.REGISTERTEMPLATESID, p.SYMBOL })
                                         .Select(p => p.Sum(s => s.NUMBER)).FirstOrDefault()
                                   ;
            if (numCancelling != null)
            {

                return (long)numCancelling;
            }
            else
            {
                return 0;
            }
        }
    }
}
