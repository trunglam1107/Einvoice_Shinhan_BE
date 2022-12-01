using InvoiceServer.Business.Models;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text.RegularExpressions;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceSampleRepository : GenericRepository<INVOICESAMPLE>, IInvoiceSampleRepository
    {
        public InvoiceSampleRepository(IDbContext context)
            : base(context)
        {

        }

        public INVOICESAMPLE GetById(long id)
        {
            return dbSet.FirstOrDefault(p => p.ID == id);
        }

        public IEnumerable<INVOICESAMPLE> GetList(UserSessionInfo currentUser)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false));
        }

        public INVOICESAMPLE GetInvoiceSampleById(long invoiceSampleId)
        {
            return this.dbSet.FirstOrDefault(p => !(p.DELETED ?? false) && p.ID.Equals(invoiceSampleId));
        }

        public INVOICESAMPLE CreateInvoiceSample(UserSessionInfo currentUser, INVOICESAMPLE invoiceSample)
        {
            IsExists(invoiceSample);
            this.dbSet.AddOrUpdate(invoiceSample);
            context.SaveChanges();
            return invoiceSample;
        }

        public INVOICESAMPLE Update(UserSessionInfo currentUser, INVOICESAMPLE info)
        {
            IsExists(info);
            dbSet.AddOrUpdate(info);
            context.SaveChanges();
            return info;
        }

        public bool ContainCode(string code, string name, bool create, UserSessionInfo currentUser)
        {
            bool result = false;
            if (create)
            {
                result = Contains(p =>
                (((p.CODE ?? string.Empty).ToUpper()).Equals(code.ToUpper()) || ((p.NAME ?? string.Empty).ToUpper()).Equals(name.ToUpper()))
                && !(p.DELETED ?? false));
            }
            return result;
        }

        public IEnumerable<INVOICESAMPLE> GetInvoiceSampleCompanyUse(long companyId)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false));
        }

        public INVOICESAMPLE GetByCode(string code, UserSessionInfo currentUser)
        {
            return this.dbSet.FirstOrDefault(x => x.CODE == code && x.DELETED == false);
        }

        public bool Update(UserSessionInfo currentUser, long id, INVOICESAMPLE invoiceSample)
        {
            try
            {
                var isExists = CheckCodeExists(invoiceSample.CODE);
                if (isExists != null)
                {
                    invoiceSample.ID = isExists.ID;
                }

                dbSet.AddOrUpdate(invoiceSample);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteInvoiceSample(string code, UserSessionInfo currentUser)
        {
            try
            {
                var invoiceSample = GetByCode(code, currentUser);
                if (invoiceSample == null)
                {
                    return false;
                }
                invoiceSample.DELETED = true;

                this.dbSet.AddOrUpdate(invoiceSample);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteInvoiceSampleById(long id)
        {
            try
            {
                var invoiceSample = GetInvoiceSampleById(id);
                if (invoiceSample == null)
                {
                    return false;
                }
                invoiceSample.DELETED = true;

                this.dbSet.AddOrUpdate(invoiceSample);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<INVOICESAMPLE> Filter(UserSessionInfo currentUser, ConditionSearchInvoiceSample condition, int skip = 0, int take = Int32.MaxValue)
        {
            if (condition.Key.IsNotNullOrEmpty())
            {
                string searchData = Regex.Replace(condition.Key, @"\t|\n|\r", "");
                return dbSet.Where(p => p.DELETED == false
                                && (p.CODE.ToUpper().Contains(searchData.ToUpper().Trim()) || p.NAME.ToUpper().Contains(searchData.ToUpper().Trim()))).OrderBy(x => x.CODE);
            }
            return this.dbSet.Where(x => x.DELETED == false)
                                    .OrderBy(x => x.CODE);
        }

        public IEnumerable<INVOICESAMPLE> GetByInvoiceTypeId(UserSessionInfo currentUser, long invoiceTypeId)
        {
            return this.dbSet.Where(x => x.INVOICETYPEID == invoiceTypeId && x.DELETED == false).OrderBy(p => p.INVOICETEMPLATESAMPLEID);
        }
        public bool MyInvoiceSampleUsing(long invoiceID)
        {
            var db = new DataClassesDataContext();
            var outputparam = new ObjectParameter("SP_RESULTOUTPUT", typeof(decimal));
            db.SP_INVOICESAMPLE_USING(invoiceID, outputparam);
            var result = outputparam.Value.ToDecimal();
            if (result == 1)
                return true;
            return false;
        }

        #region Private Methods
        private INVOICESAMPLE CheckCodeExists(string code)
        {
            return this.dbSet.FirstOrDefault(x => x.CODE == code);
        }

        private void IsExists(INVOICESAMPLE invoiceSample)
        {
            var isExists = CheckCodeExists(invoiceSample.CODE);
            if (isExists != null)
            {
                invoiceSample.ID = isExists.ID;
            }
        }

        #endregion Private Methods
    }
}
