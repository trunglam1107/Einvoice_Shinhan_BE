using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceConcludeRepository : GenericRepository<INVOICERELEAS>, IInvoiceConcludeRepository
    {
        private readonly DataClassesDataContext _dbSet = new DataClassesDataContext();

        public InvoiceConcludeRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<InvoiceReleasesMaster> FilterInvoiceConclude(ConditionSearchInvoiceReleases condition)
        {
            var query = this.FilterInvoiceReleaseQuery(condition);
            var invoiceReleases = query.AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();

            var invoiceReleasesMaster =
                from invoiceRelease in invoiceReleases
                join company in this._dbSet.MYCOMPANies
                    on invoiceRelease.COMPANYID equals company.COMPANYSID
                select new InvoiceReleasesMaster(invoiceRelease)
                {
                    CompanyName = invoiceRelease.STATUS == (int)RecordStatus.Created ? company.COMPANYNAME : invoiceRelease.COMPANYNAME,
                };

            var res = invoiceReleasesMaster.ToList();

            return res;
        }

        public long CountInvoiceConclude(ConditionSearchInvoiceReleases condition)
        {
            var query = this.FilterInvoiceReleaseQuery(condition);
            return query.Count();
        }

        private IEnumerable<INVOICERELEAS> FilterInvoiceReleaseQuery(ConditionSearchInvoiceReleases condition)
        {
            var invoiceConcludes = this.GetInvoiceConcludeActive();
            if (condition.Branch.HasValue)
            {
                invoiceConcludes = invoiceConcludes.Where(p => p.COMPANYID == condition.Branch.Value);
            }

            if (condition.Status.HasValue)
            {
                invoiceConcludes = invoiceConcludes.Where(p => p.STATUS == condition.Status);
            }

            return invoiceConcludes;
        }

        public INVOICERELEAS GetById(long id)
        {
            return this.GetInvoiceConcludeActive().FirstOrDefault(p => p.ID == id);
        }

        private IQueryable<INVOICERELEAS> GetInvoiceConcludeActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false));
        }
        public List<InvoiceReleasesDetailInfo> GetByNotification(long notiID)
        {
            var noti = (from invoiceRelease in this.dbSet
                        join releaseDetail in this.context.Set<INVOICERELEASESDETAIL>() on invoiceRelease.ID equals releaseDetail.INVOICECONCLUDEID
                        join notificationDetail in this.context.Set<NOTIFICATIONUSEINVOICEDETAIL>() on releaseDetail.REGISTERTEMPLATESID equals notificationDetail.REGISTERTEMPLATESID
                        where notificationDetail.NOTIFICATIONUSEINVOICEID == notiID && invoiceRelease.STATUS == 1 && invoiceRelease.DELETED != true
                        select new InvoiceReleasesDetailInfo
                        {
                            InvoiceConcludeId = releaseDetail.INVOICECONCLUDEID ?? 0
                        }).ToList();
            return noti;
        }
    }
}