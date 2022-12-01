using InvoiceServer.Business.BL;
using InvoiceServer.Business.DAO.Interface;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceReplaceRepository : GenericRepository<INVOICEREPLACE>, IInvoiceReplaceRepository
    {
        private readonly DataClassesDataContext _dbSet = new DataClassesDataContext();
        private static readonly Func<string, string> GetConfig = key => WebConfigurationManager.AppSettings[key];
        public InvoiceReplaceRepository(IDbContext context)
            : base(context)
        {
        }

        public IQueryable<INVOICEREPLACE> GetInvoiceReplaceByCompany(long companyId)
        {
            return this.dbSet.Where(x => x.COMPANYID == companyId);
        }

        public IQueryable<InvoiceReplace> GetAllInvoiceReplace()
        {
            var listInfo = (from invoiceReplace in this.context.Set<INVOICEREPLACE>()
                            join invoice in this.context.Set<INVOICE>() on invoiceReplace.REFNUMBER equals invoice.REFNUMBER
                            select new InvoiceReplace
                            {
                                ID = invoiceReplace.ID,
                                TOTALTAX = invoiceReplace.TOTALTAX,
                                TOTAL = invoiceReplace.TOTAL,
                                SUM = invoiceReplace.SUM,
                                REFNUMBER = invoiceReplace.REFNUMBER,
                                YEARMONTH = invoiceReplace.YEARMONTH,
                                CURRENCYID = invoiceReplace.CURRENCYID,
                                COMPANYID = invoiceReplace.COMPANYID,
                                VAT_RATE = invoiceReplace.VAT_RATE,
                                REPORT_CLASS = invoiceReplace.REPORT_CLASS,
                                VAT_INVOICE_TYPE = invoiceReplace.VAT_INVOICE_TYPE,
                                CIF_NO = invoiceReplace.CIF_NO,
                                RELEASEDDATE = invoiceReplace.RELEASEDDATE,
                                FEE_BEN_OUR = invoiceReplace.FEE_BEN_OUR
                            });
            return listInfo;
        }

        public IQueryable<InvoiceReplace> GetAllInvoiceReplaceMonthly()
        {
            var listInfo = (from invoiceReplace in this.context.Set<INVOICEREPLACE>()
                            where (invoiceReplace.VAT_INVOICE_TYPE != null &&  invoiceReplace.VAT_INVOICE_TYPE.Contains("3"))
                            select new InvoiceReplace
                            {
                                ID = invoiceReplace.ID,
                                TOTALTAX = invoiceReplace.TOTALTAX,
                                TOTAL = invoiceReplace.TOTAL,
                                SUM = invoiceReplace.SUM,
                                REFNUMBER = invoiceReplace.REFNUMBER,
                                YEARMONTH = invoiceReplace.YEARMONTH,
                                CURRENCYID = invoiceReplace.CURRENCYID,
                                COMPANYID = invoiceReplace.COMPANYID,
                                VAT_RATE = invoiceReplace.VAT_RATE,
                                REPORT_CLASS = invoiceReplace.REPORT_CLASS,
                                VAT_INVOICE_TYPE = invoiceReplace.VAT_INVOICE_TYPE,
                                CIF_NO = invoiceReplace.CIF_NO,
                                RELEASEDDATE = invoiceReplace.RELEASEDDATE,
                                FEE_BEN_OUR = invoiceReplace.FEE_BEN_OUR
                            });
            return listInfo;
        }
    }
}
