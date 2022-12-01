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
    public class UnitListRepository : GenericRepository<UNITLIST>, IUnitListRepository
    {
        public UnitListRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<UNITLIST> GetList()
        {
            return this.dbSet.OrderBy(x => x.CODE);
        }

        public IEnumerable<UNITLIST> Filter(ConditionSearchUnitList condition, int skip = 0, int take = Int32.MaxValue)
        {
            if (condition.Key.IsNotNullOrEmpty())
            {
                string searchData = Regex.Replace(condition.Key, @"\t|\n|\r", "");
                return dbSet.Where(u => (u.CODE.ToUpper().Contains(searchData.ToUpper().Trim()) || u.NAME.ToUpper().Contains(searchData.ToUpper().Trim()))).OrderBy(x => x.CODE);
            }
            return this.dbSet.OrderBy(x => x.CODE);
        }

        public UNITLIST GetByCode(string code, long companySID)
        {
            return dbSet.FirstOrDefault(x => x.CODE == code);
        }

        public bool Update(string code, UNITLIST unitList)
        {
            try
            {
                dbSet.AddOrUpdate(unitList);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ContainCode(string code, string name, bool create, long companySID)
        {
            bool result = false;
            if (create)
            {
                result = Contains(p => ((p.CODE ?? string.Empty).ToUpper()).Equals(code.ToUpper()) || ((p.NAME ?? string.Empty).ToUpper()).Equals(name.ToUpper()));
            }
            else
            {
                var unitList = GetByCode(code, companySID);
                if (unitList.NAME == name)
                {
                    result = false;
                }
                else if (unitList.NAME != name)
                {
                    result = Contains(p => ((p.NAME ?? string.Empty).ToUpper()).Equals(name.ToUpper()));
                }
            }
            return result;
        }

        public bool DeleteTypeTax(string code, long companySID)
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

        public UNITLIST GetById(long id)
        {
            return this.dbSet.FirstOrDefault(u => u.ID == id);
        }

        public bool MyUnitUsing(long id)
        {
            var db = new DataClassesDataContext();
            var outputparam = new ObjectParameter("SP_RESULTOUTPUT", typeof(decimal));
            db.SP_UNIT_USING(id, outputparam);
            var result = outputparam.Value.ToDecimal();
            if (result == 1)
                return true;
            return false;
        }
        public bool AddList(List<UNITLIST> unitLists)
        {
            foreach (var unitList in unitLists)
            {
                dbSet.AddOrUpdate(unitList);
                context.SaveChanges();
            }
            return true;
        }

        public IEnumerable<UNITLIST> GetByName(string name, long companyId)
        {
            return this.dbSet.Where(u => u.NAME == name);
        }

        public IEnumerable<UNITLIST> GetByNameOrCode(string code, string name, long companyId)
        {
            return this.dbSet.Where(u => u.NAME == name || u.CODE == code);
        }
        public UNITLIST GetByName(string name)
        {
            return this.dbSet.Where(u => u.NAME == name).FirstOrDefault();
        }
    }
}
