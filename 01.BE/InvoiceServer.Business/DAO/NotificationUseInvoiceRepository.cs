using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class NotificationUseInvoiceRepository : GenericRepository<NOTIFICATIONUSEINVOICE>, INotificationUseInvoiceRepository
    {
        private readonly DataClassesDataContext _dbSet = new DataClassesDataContext();
        public NotificationUseInvoiceRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<NotificeUseInvoiceMaster> Filter(ConditionSearchUseInvoice condition)
        {
            var query = this.FilterQuery(condition);
            var notificationUserInvoices = query.AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();

            var notificeUseInvoiceMaster =
                from notificationUserInvoice in notificationUserInvoices
                join company in this._dbSet.MYCOMPANies on notificationUserInvoice.COMPANYID equals company.COMPANYSID into companyTemp
                from company in companyTemp.DefaultIfEmpty()
                join taxDepartment in this._dbSet.TAXDEPARTMENTs on notificationUserInvoice.TAXDEPARTMENTID equals taxDepartment.ID into taxDepartmentTemp
                from taxDepartment in taxDepartmentTemp.DefaultIfEmpty()
                select new NotificeUseInvoiceMaster(notificationUserInvoice)
                {
                    CompanyName = notificationUserInvoice.STATUS == (int)RecordStatus.Created ? company?.COMPANYNAME : notificationUserInvoice.COMPANYNAME,
                    TaxDepartment = notificationUserInvoice.STATUS == (int)RecordStatus.Created ? taxDepartment?.NAME : notificationUserInvoice.TAXDEPARTMENTNAME,
                };

            var res = notificeUseInvoiceMaster.ToList();

            return res;

        }

        public long Count(ConditionSearchUseInvoice condition)
        {
            var query = this.FilterQuery(condition);
            var res = query.Count();
            return res;
        }

        private IEnumerable<NOTIFICATIONUSEINVOICE> FilterQuery(ConditionSearchUseInvoice condition)
        {
            var notificationUserInvoice = this.GetInvoiceConcludeActive();


            if (condition.Branch.HasValue)
            {
                notificationUserInvoice = notificationUserInvoice.Where(p => p.COMPANYID == condition.Branch);
            }

            if (condition.Status.HasValue)
            {
                notificationUserInvoice = notificationUserInvoice.Where(p => p.STATUS == condition.Status.Value);
            }

            if (condition.DateFrom.HasValue)
            {
                notificationUserInvoice = notificationUserInvoice.Where(p => p.CREATEDDATE >= condition.DateFrom.Value);
            }

            if (condition.DateTo.HasValue)
            {
                notificationUserInvoice = notificationUserInvoice.Where(p => p.CREATEDDATE <= condition.DateTo.Value);
            }

            return notificationUserInvoice;
        }

        public NOTIFICATIONUSEINVOICE GetById(long id)
        {
            return this.GetInvoiceConcludeActive().FirstOrDefault(p => p.ID == id);
        }

        public UseInvoiceInfo GetNoticeUseInvoiceInfo(long id)
        {
            var useInvoiceList = this.GetInvoiceConcludeActive().Where(x => x.ID == id).ToList();
            var useInvoice = useInvoiceList.FirstOrDefault();

            var useInvoiceInfoList =
                from notificationUseInvoice in useInvoiceList
                join company in this._dbSet.MYCOMPANies on notificationUseInvoice.COMPANYID equals company.COMPANYSID into companyTemp
                from company in companyTemp.DefaultIfEmpty()
                join taxDepartment in this._dbSet.TAXDEPARTMENTs on notificationUseInvoice.TAXDEPARTMENTID equals taxDepartment.ID into taxDepartmentTemp
                from taxDepartment in taxDepartmentTemp.DefaultIfEmpty()
                join city in this._dbSet.CITies on notificationUseInvoice.CITYID equals city.ID into cityTemp
                from city in cityTemp.DefaultIfEmpty()
                select new UseInvoiceInfo(notificationUseInvoice)
                {
                    CompanyName = notificationUseInvoice.STATUS == (int)RecordStatus.Created ? company?.COMPANYNAME : notificationUseInvoice.COMPANYNAME,
                    Address = notificationUseInvoice.STATUS == (int)RecordStatus.Created ? company?.ADDRESS : notificationUseInvoice.COMPANYADDRESS,
                    Delegate = notificationUseInvoice.STATUS == (int)RecordStatus.Created ? company?.DELEGATE : notificationUseInvoice.DELEGATE,
                    TaxCode = notificationUseInvoice.STATUS == (int)RecordStatus.Created ? company?.TAXCODE : notificationUseInvoice.COMPANYTAXCODE,
                    BankAccount = notificationUseInvoice.STATUS == (int)RecordStatus.Created ? company?.BANKACCOUNT : notificationUseInvoice.BANKACCOUNT,
                    Tel = notificationUseInvoice.STATUS == (int)RecordStatus.Created ? company?.TEL1 : notificationUseInvoice.COMPANYPHONE,
                    TaxDepartmentName = notificationUseInvoice.STATUS == (int)RecordStatus.Created ? taxDepartment?.NAME : notificationUseInvoice.TAXDEPARTMENTNAME,
                    CityName = notificationUseInvoice.STATUS == (int)RecordStatus.Created ? city?.NAME : notificationUseInvoice.CITYNAME,
                };
            var useInvoiceInfo = useInvoiceInfoList.FirstOrDefault();

            var useInvoiceDetailInfoList =
                from useInvoiceDetail in useInvoice.NOTIFICATIONUSEINVOICEDETAILs
                join registerTemplate in this._dbSet.REGISTERTEMPLATES on useInvoiceDetail.REGISTERTEMPLATESID equals registerTemplate.ID into registerTemplateTemp
                from registerTemplate in registerTemplateTemp.DefaultIfEmpty()
                join invoiceSample in this._dbSet.INVOICESAMPLEs on registerTemplate.INVOICESAMPLEID equals invoiceSample.ID into invoiceSampleTemp
                from invoiceSample in invoiceSampleTemp.DefaultIfEmpty()
                join invoiceType in this._dbSet.INVOICETYPEs on invoiceSample.INVOICETYPEID equals invoiceType.ID into invoiceTypeTemp
                from invoiceType in invoiceTypeTemp.DefaultIfEmpty()
                select new UseInvoiceDetailInfo(useInvoiceDetail)
                {
                    InvoiceTypeName = useInvoice.STATUS == (int)RecordStatus.Created ? invoiceType?.NAME : useInvoiceDetail.INVOICETYPENAME,
                    RegisterTemplateCode = useInvoice.STATUS == (int)RecordStatus.Created ? registerTemplate?.CODE : useInvoiceDetail.REGISTERTEMPLATECODE,
                    InvoiceSampleName = useInvoice.STATUS == (int)RecordStatus.Created ? invoiceSample?.NAME : useInvoiceDetail.INVOICESAMPLENAME,
                };

            useInvoiceInfo.UseInvoiceDetails = useInvoiceDetailInfoList.ToList();
            return useInvoiceInfo;
        }

        private IQueryable<NOTIFICATIONUSEINVOICE> GetInvoiceConcludeActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false));
        }

        public IEnumerable<long> GetListCompanyUse()
        {
            var listCompanyUse = from notice in dbSet
                                 where (!(notice.DELETED ?? false)
                                     && notice.STATUS == (int)RecordStatus.Approved)
                                 select new
                                 {
                                     CompanyId = notice.COMPANYID
                                 };
            var result = listCompanyUse.GroupBy(x => new { x.CompanyId })
                .Select(p => p.Key.CompanyId ?? 0).AsEnumerable();

            return result;
        }
        public List<MYCOMPANY> GetListCompanyUseShinhan()
        {
            var listCompanyUse = (from company in this.context.Set<MYCOMPANY>() where ((company.LEVELCUSTOMER == "HO") || (company.LEVELCUSTOMER == "CN")) select company).ToList();
            return listCompanyUse;
        }

        /// <summary>
        /// Lấy danh sách companies cần tạo files
        /// </summary>
        /// <returns></returns>
        public List<MYCOMPANY> GetListCompanyNeedCreateFiles() 
        {
          
            var companyIds = this.context.Set<INVOICE>()
                   .Where(p => (p.DELETED == null || !p.DELETED.Value)
                       && (p.INVOICESTATUS == (int)InvoiceStatus.Approved || p.INVOICESTATUS == (int)InvoiceStatus.Releaseding))
                   .GroupBy(f => f.COMPANYID).Select(f => f.Key).ToList();
            var objCompany = this.context.Set<MYCOMPANY>().Where(f => companyIds.Any(t => t == f.COMPANYSID)).ToList();
            
            return objCompany;

        }
        public List<CompanySymbolInfo> GetSymbolCompanyForSign()
        {
            var result = this.context.Set<INVOICE>()
                   .Where(p => (p.DELETED == null || !p.DELETED.Value)
                       && p.INVOICESTATUS == (int)InvoiceStatus.Approved
                       && p.INVOICENO > 0
                       )
                   .GroupBy(f => new { f.COMPANYID, f.SYMBOL})
                   .Select(f => new CompanySymbolInfo { 
                        Symbol = f.Key.SYMBOL,
                        CompanyId = f.Key.COMPANYID
                   }).ToList();
            return result;
        }
        public List<MYCOMPANY> GetListCompanySendMails()
        {
            var companyIds = this.context.Set<INVOICE>()
                   .Where(p => (p.DELETED == null || !p.DELETED.Value)
                       && p.INVOICESTATUS == (int)InvoiceStatus.Released
                       && p.SENDMAILSTATUS != true
                       && p.INVOICENO > 0
                       )
                   .GroupBy(f => f.COMPANYID).Select(f => f.Key).ToList();

            var companyOther = (from invoice in this.context.Set<INVOICE>()
                               join refe in this.context.Set<EMAILACTIVEREFERENCE>() on invoice.ID equals refe.REFID
                               join email in this.context.Set<EMAILACTIVE>() on refe.EMAILACTIVEID equals email.ID
                               where (invoice.SENDMAILSTATUS ?? false) 
                                    && email.SENDSTATUS == 0
                               group invoice by invoice.COMPANYID into g
                               select g.Key).ToList();
            var allList = companyIds.Concat(companyOther).Distinct().ToList();

            var objCompany = this.context.Set<MYCOMPANY>().Where(f => allList.Any(t => t == f.COMPANYSID)).ToList();
            return objCompany;

        }
        /// <summary>
        /// Lấy danh sách công ty có hóa đơn cần approved
        /// </summary>
        /// <returns></returns>
        public List<MYCOMPANY> GetListCompanyNeedApproved()
        {

            var companyIds = this.context.Set<INVOICE>()
                   .Where(p => (p.DELETED == null || !p.DELETED.Value)
                       && p.INVOICESTATUS == (int)InvoiceStatus.New)
                   .GroupBy(f => f.COMPANYID).Select(f => f.Key).ToList();
            var objCompany = this.context.Set<MYCOMPANY>().Where(f => companyIds.Any(t => t == f.COMPANYSID)).ToList();

            return objCompany;

        }

       
        /// <summary>
        /// Lấy danh sách companies đã tạo files
        /// </summary>
        /// <returns></returns>
        public List<MYCOMPANY> GetListCompanyCreatedFiles()
        {

            var companyIds = this.context.Set<INVOICE>()
                   .Where(p => (p.DELETED == null || !p.DELETED.Value)
                       && (p.INVOICESTATUS == (int)InvoiceStatus.Approved || p.INVOICESTATUS == (int)InvoiceStatus.Releaseding)
                       && (p.CREATEDFILE.HasValue && p.CREATEDFILE.Value))
                   .GroupBy(f => f.COMPANYID).Select(f => f.Key).ToList();
            var objCompany = this.context.Set<MYCOMPANY>().Where(f => companyIds.Any(t => t == f.COMPANYSID)).ToList();

            return objCompany;

        }


        public long GetTotalNumberRegisterBySuffix(string suffix)
        {
            var listCompanyUse = from noticeDetail in this._dbSet.NOTIFICATIONUSEINVOICEDETAILs
                                 join notice in this._dbSet.NOTIFICATIONUSEINVOICEs
                                    on noticeDetail.NOTIFICATIONUSEINVOICEID equals notice.ID
                                 where (notice.STATUS == (int)RecordStatus.Approved
                                        && noticeDetail.SUFFIX == suffix)
                                 select new
                                 {
                                     NumberUse = noticeDetail.NUMBERUSE
                                 };

            var result = (long)(listCompanyUse.Sum(p => p.NumberUse) ?? 0);

            return result;
        }

        public long GetTotalRegisterBySuffixExceptCurrentNotice(string suffix, long noticeId)
        {
            var listCompanyUse = from noticeDetail in this._dbSet.NOTIFICATIONUSEINVOICEDETAILs
                                 join notice in this._dbSet.NOTIFICATIONUSEINVOICEs
                                    on noticeDetail.NOTIFICATIONUSEINVOICEID equals notice.ID
                                 where (notice.STATUS == (int)RecordStatus.Approved
                                        && noticeDetail.SUFFIX == suffix
                                        && notice.ID != noticeId)
                                 select new
                                 {
                                     NumberUse = noticeDetail.NUMBERUSE
                                 };

            var result = (long)(listCompanyUse.Sum(p => p.NumberUse) ?? 0);

            return result;
        }



    }
}
