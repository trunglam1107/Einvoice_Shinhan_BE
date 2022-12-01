using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class SignDetailRepository : GenericRepository<SIGNDETAIL>, ISignDetailRepository
    {
        public SignDetailRepository(IDbContext context)
            : base(context)
        {
        }

        public SIGNDETAIL GetByInvoiceId(int invoiceId, bool isClientSign = false)
        {
            var signDetails = this.dbSet.Where(x => x.INVOICEID == invoiceId && x.ISCLIENTSIGN == isClientSign).OrderByDescending(x => x.CREATEDDATE);
            return signDetails.FirstOrDefault();
        }

        public SIGNDETAIL GetByAnnouncementId(int announcementId, bool isClientSign = false)
        {
            var signDetails = this.dbSet.Where(x => x.INVOICEID == announcementId && x.ISCLIENTSIGN == isClientSign && x.TYPESIGN == (int)SignDetailTypeSign.Announcement).OrderByDescending(x => x.CREATEDDATE);
            return signDetails.FirstOrDefault();
        }
        public void InsertMultipe(List<SIGNDETAIL> signDetails)
        {
            var dbContext = this.context as DataClassesDataContext;
            dbContext.BulkInsert(signDetails);
        }
    }
}
