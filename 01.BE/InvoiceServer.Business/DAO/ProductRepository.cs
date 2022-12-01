using InvoiceServer.Business.Models;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class ProductRepository : GenericRepository<PRODUCT>, IProductRepository
    {
        public ProductRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<PRODUCT> GetList()
        {
            return GetClientActive();
        }

        public PRODUCT GetById(long id)
        {
            return GetClientActive().FirstOrDefault(p => p.ID == id);
        }

        public PRODUCT GetById(long id, long companyId)
        {
            return GetClientActive().FirstOrDefault(p => p.ID == id);
        }

        public IEnumerable<PRODUCT> FilterProducts(ConditionSearchProduct condition)
        {
            var products = GetClientActive();

            if (!condition.productCode.IsNullOrEmpty())
            {
                products = products.Where(p => p.PRODUCTCODE.ToUpper().Contains(condition.productCode.ToUpper()));
            }

            if (!condition.productName.IsNullOrEmpty())
            {
                products = products.Where(p => p.PRODUCTNAME.ToUpper().Contains(condition.productName.ToUpper()));
            }

            return products;
        }

        private IQueryable<PRODUCT> GetClientActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false) && !(p.NOTDISPLAY ?? false));
        }

        public PRODUCT FilterProduct(long companyId, string name, string unitName)
        {
            return GetClientActive().FirstOrDefault(p => p.PRODUCTNAME.Equals(name) && p.UNIT.Equals(unitName));
        }

        public PRODUCT FilterProductNameUnitCode(string name, string unitCode)
        {
            return GetClientActive().FirstOrDefault(p => p.PRODUCTNAME.ToUpper().Equals(name.ToUpper())
                && p.UNITLIST.CODE.ToUpper().Equals(unitCode.ToUpper()));
        }

        public PRODUCT FilterProductCode(string code)
        {
            return GetClientActive().FirstOrDefault(p => p.PRODUCTCODE.Equals(code));
        }

        public PRODUCT FilterProductNameUnitId(long companyId, string name, long unitId)
        {
            return GetClientActive().FirstOrDefault(p => p.PRODUCTNAME.Equals(name) && p.UNITID == unitId);
        }

        public bool ContainNameUnitId(string name, long unitId, bool isCreate, long id)
        {
            bool result = false;
            if (!name.IsNullOrEmpty() && !unitId.IsNullOrEmpty())
            {
                if (isCreate)
                {
                    result = Contains(p => p.DELETED == null && ((p.PRODUCTNAME ?? string.Empty).ToUpper()).Equals(name.ToUpper()) && !(p.DELETED ?? false) && p.UNITID == unitId);
                }
                else
                {
                    result = Contains(p => p.DELETED == null && ((p.PRODUCTNAME ?? string.Empty).ToUpper()).Equals(name.ToUpper()) && !(p.DELETED ?? false) && p.UNITID == unitId && p.ID != id);
                }
            }
            return result;
        }

        public bool ExistsDifferTax(long companyId, string name, long taxId, bool isCreate, long id)
        {
            bool result = false;
            if (!taxId.IsNullOrEmpty())
            {
                if (isCreate)
                {
                    result = Contains(p => p.DELETED == null && ((p.PRODUCTNAME ?? string.Empty).ToUpper()).Equals(name.ToUpper()) && p.TAXID != null && p.TAXID != taxId);
                }
                else
                {
                    result = Contains(p => p.DELETED == null && ((p.PRODUCTNAME ?? string.Empty).ToUpper()).Equals(name.ToUpper()) && p.TAXID != null && p.TAXID != taxId && p.ID != id);
                }
            }
            return result;
        }

        public bool ContainCode(long companyId, string code, bool isCreate, long id)
        {
            bool result = false;
            if (isCreate)
            {
                result = Contains(p => p.DELETED == null && ((p.PRODUCTCODE ?? string.Empty).ToUpper()).Equals(code.ToUpper()));
            }
            else
            {
                result = Contains(p => p.DELETED == null && ((p.PRODUCTCODE ?? string.Empty).ToUpper()).Equals(code.ToUpper()) && p.ID != id);
            }
            return result;
        }

        public bool MyProductUsing(long id)
        {
            var db = new DataClassesDataContext();
            var outputparam = new ObjectParameter("SP_RESULTOUTPUT", typeof(decimal));
            db.SP_PRODUCT_USING(id, outputparam);
            var result = outputparam.Value.ToDecimal();
            if (result == 1)
                return true;
            return false;

        }
    }
}
