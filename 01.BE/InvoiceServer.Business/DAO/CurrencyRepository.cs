using InvoiceServer.Business.DAO.Interface;
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
    public class CurrencyRepository : GenericRepository<CURRENCY>, ICurrencyRepository
    {
        public CurrencyRepository(IDbContext context)
            : base(context)
        {
        }

        /// <summary>
        /// GetList
        /// </summary>
        /// <returns>IEnumerable<CURRENCY></returns>
        public IEnumerable<CURRENCY> GetList()
        {
            return this.dbSet.OrderBy(x => x.CODE);
        }

        /// <summary>
        /// Filter
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns>IEnumerable<CURRENCY></returns>
        public IEnumerable<CURRENCY> Filter(ConditionSearchInvoiceType condition, int skip = 0, int take = Int32.MaxValue)
        {
            if (condition.Key.IsNotNullOrEmpty())
            {
                string searchData = Regex.Replace(condition.Key, @"\t|\n|\r", "");
                return dbSet.Where(u => (u.CODE.ToUpper().Contains(searchData.ToUpper().Trim()) || u.NAME.ToUpper().Contains(searchData.ToUpper().Trim()))).OrderBy(x => x.CODE);
            }

            return this.dbSet.OrderBy(x => x.CODE);
        }

        /// <summary>
        /// AddList
        /// </summary>
        /// <param name="currencyLists"></param>
        /// <returns>bool</returns>
        public bool AddList(List<CURRENCY> currencyLists)
        {
            foreach (var currencyList in currencyLists)
            {
                dbSet.AddOrUpdate(currencyList);
                context.SaveChanges();
            }
            return true;
        }

        /// <summary>
        /// ContainCode
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <param name="create"></param>
        /// <param name="companySID"></param>
        /// <returns>bool</returns>
        public bool ContainCode(string code, bool create)
        {
            bool result = false;
            if (create)
            {
                result = Contains(p => ((p.CODE ?? string.Empty).ToUpper()).Equals(code.ToUpper()));
            }
            return result;
        }

        /// <summary>
        /// GetByCode
        /// </summary>
        /// <param name="code"></param>
        /// <returns>CURRENCY</returns>
        public CURRENCY GetByCode(string code)
        {
            return dbSet.FirstOrDefault(x => x.CODE == code);
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="code"></param>
        /// <returns>bool</returns>
        public bool Delete(string code)
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

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="code"></param>
        /// <param name="currencyList"></param>
        /// <returns>bool</returns>
        public bool Update(string code, CURRENCY currencyList)
        {
            try
            {
                dbSet.AddOrUpdate(currencyList);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool MyCurrencyUsing(long currencyId)
        {
            var db = new DataClassesDataContext();
            var outputparam = new ObjectParameter("SP_RESULTOUTPUT", typeof(decimal));
            db.SP_CURRENCY_USING(currencyId, outputparam);
            var result = outputparam.Value.ToDecimal();
            if (result == 1)
                return true;
            return false;
        }
    }
}
