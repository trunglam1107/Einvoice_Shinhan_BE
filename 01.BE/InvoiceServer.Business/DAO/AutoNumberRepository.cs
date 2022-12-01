using InvoiceServer.Data.DBAccessor;
using System.Data.Entity.Migrations;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class AutoNumberRespository : GenericRepository<AUTONUMBERSETTING>, IAutoNumberRespository
    {
        public AutoNumberRespository(IDbContext context)
            : base(context)
        {
        }

        public IQueryable<AUTONUMBERSETTING> GetList()
        {
            return this.dbSet;
        }
        public AUTONUMBERSETTING GetById(string id)
        {
            return this.dbSet.Where(x => x.TYPEID.Equals(id)).FirstOrDefault();
        }
        public bool Update(string code, AUTONUMBERSETTING autoNumber)
        {
            try
            {
                dbSet.AddOrUpdate(autoNumber);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool checkActive(string id)
        {
            int result = this.context.Set<AUTONUMBERSETTING>().Where(x => x.TYPEID.Equals(id) && x.ACTIVED).Count();
            if (result > 0)
                return true;
            return false;
        }

        public int Length(string id)
        {
            return this.dbSet.Where(x => x.TYPEID.Equals(id)).Select(x => x.OUTPUTLENGTH ?? 0).FirstOrDefault();
        }
    }
}
