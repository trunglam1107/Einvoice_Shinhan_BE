using InvoiceServer.Business.DAO.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class ServicesJobRepository : GenericRepository<INVOICE>, IServicesJobRepository
    {
        #region Properties
        public ServicesJobRepository(IDbContext context)
            : base(context)
        {
        }
        #endregion
        #region Lay Danh Sach Hoa Don Can Duyet
        public IEnumerable<JobApprovedInvoice> ListInvoiceApprove(int? flgCreate = 0)
        {
            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));
            var registers = this.context.Set<REGISTERTEMPLATE>();
            var invoices = FilterCreated(flgCreate);
            invoices = (from invoice in invoices
                        where (from regis in registers select regis.ID).Contains(invoice.REGISTERTEMPLATEID ?? 1)
                            && (from client in clients select client.ID).Contains(invoice.CLIENTID ?? 0)
                        select invoice);
            var invoiceInfos = (from invoice in invoices
                                select new JobApprovedInvoice
                                {
                                    CompanyId = invoice.COMPANYID,
                                    InvoiceId = invoice.ID,
                                    InvoiceNo = invoice.INVOICENO ?? 0
                                }).ToList();
            return invoiceInfos;
        }

        private IQueryable<INVOICE> FilterCreated(int? flagCreated = 0)
        {
            var invoices = GetInvoiceActive();
            var systemSetting = this.context.Set<SYSTEMSETTING>().FirstOrDefault();

            if (systemSetting?.STEPTOCREATEINVOICENO != 2 && flagCreated != 1 && systemSetting?.STEPTOCREATEINVOICENO == 1)        // flagCreated == 1 :Create Invoice
            {
                var datefrom = (DateTime.Now).Date.AddDays(-30);
                invoices = invoices.Where(p => p.CREATEDDATE >= datefrom);

                var dateTo = (DateTime.Today).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                invoices = invoices.Where(p => p.CREATEDDATE <= dateTo);
            }
            return invoices;
        }
        private IQueryable<INVOICE> GetInvoiceActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false) && p.INVOICESTATUS == 1 && p.PARENTID == null);
        }
        #endregion
    }
}
