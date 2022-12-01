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
    public class InvoiceTypeRepository : GenericRepository<INVOICETYPE>, IInvoiceTypeRepository
    {
        public InvoiceTypeRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<INVOICETYPE> GetList()
        {
            return this.dbSet.OrderBy(x => x.CODE);
        }

        public IEnumerable<INVOICETYPE> Filter(ConditionSearchInvoiceType condition, int skip = 0, int take = Int32.MaxValue)
        {
            if (condition.Key.IsNotNullOrEmpty())
            {
                string searchData = Regex.Replace(condition.Key, @"\t|\n|\r", "");
                return dbSet.Where(p => p.CODE.ToUpper().Contains(searchData.ToUpper().Trim()) || p.NAME.ToUpper().Contains(searchData.ToUpper().Trim())).OrderBy(x => x.CODE);
            }
            return this.dbSet.OrderBy(x => x.CODE);
        }

        public INVOICETYPE GetByCode(string code)
        {
            return dbSet.FirstOrDefault(x => x.CODE == code);
        }

        public bool Update(string code, INVOICETYPE invoiceType)
        {
            try
            {
                dbSet.AddOrUpdate(invoiceType);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ContainCode(string code, string name, string denominator, bool create)
        {
            bool result = false;
            if (create)
            {
                result = Contains(p => ((p.CODE ?? string.Empty).ToUpper()).Equals(code.ToUpper()) || ((p.NAME ?? string.Empty).ToUpper()).Equals(name.ToUpper()) || ((p.DENOMINATOR ?? string.Empty).ToUpper()).Equals(denominator.ToUpper()));
            }
            else
            {
                var invoiceType = GetByCode(code);
                if (invoiceType == null)
                {
                    return result;
                }

                if (invoiceType.NAME == name && invoiceType.DENOMINATOR == denominator)
                {
                    result = false;
                }
                else if (invoiceType.NAME != name && invoiceType.DENOMINATOR != denominator)
                {
                    result = Contains(p => ((p.NAME ?? string.Empty).ToUpper()).Equals(name.ToUpper()) && ((p.DENOMINATOR ?? string.Empty).ToUpper()).Equals(denominator.ToUpper()));
                }
                else if (invoiceType.NAME != name || invoiceType.DENOMINATOR != denominator)
                {
                    result = Contains(p => ((p.NAME ?? string.Empty).ToUpper()).Equals(name.ToUpper()) || ((p.DENOMINATOR ?? string.Empty).ToUpper()).Equals(denominator.ToUpper()));
                }
            }
            return result;
        }

        public bool DeleteInvoiceType(string code)
        {
            try
            {
                this.dbSet.Remove(this.dbSet.FirstOrDefault(x => x.CODE.Equals(code)));
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public INVOICETYPE GetById(long id)
        {
            return this.dbSet.Where(x => x.ID == id).FirstOrDefault();
        }

        public bool MyInvoiceTypeUsing(long invoiceTypeId)
        {
            var db = new DataClassesDataContext();
            var outputparam = new ObjectParameter("SP_RESULTOUTPUT", typeof(decimal));
            db.SP_INVOICETYPE_USING(invoiceTypeId, outputparam);
            var result = outputparam.Value.ToDecimal();
            if (result == 1)
                return true;
            return false;
        }
    }
}
