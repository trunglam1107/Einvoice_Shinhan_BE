using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.DAO.Interface
{
    public class CompanySymbolRepository : GenericRepository<SYMBOL>, ICompanySymbolRepository
    {
        private readonly DataClassesDataContext _dbSet = new DataClassesDataContext();
        public CompanySymbolRepository(IDbContext context)
            : base(context)
        {
        }
        public int CountComSymbol(long id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CompanySymbolInfo> FilterComSymbol(long id)
        {
            throw new NotImplementedException();
        }

        public SYMBOL GetById(long id, long? detailId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SYMBOL> GetList(long id)
        {
            return GetCompanySymbolList(id);
        }

        private IEnumerable<SYMBOL> GetCompanySymbolList(long id)
        {
            var listCompanySymbol = dbSet.Where(P => P.REFID == id);

            return listCompanySymbol;
        }

        public bool DeleteCompanySymbol(long companyId)
        {
            try
            {
                this.dbSet.Remove(this.dbSet.FirstOrDefault(x => x.COMPANYID == companyId));
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
