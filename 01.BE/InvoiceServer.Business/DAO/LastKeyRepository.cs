using InvoiceServer.Data.DBAccessor;
using System.Data.Entity.Migrations;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class LastKeyRepository : GenericRepository<LASTKEY>, ILastKeyRepository
    {
        public LastKeyRepository(IDbContext context)
            : base(context)
        {
        }
        public int GetLastKey(long companyId, string keyString)
        {
            var row = this.dbSet.Where(x => (x.COMPANYID.Equals(companyId) && x.KEYSTRING.Equals(keyString) || keyString == "")).FirstOrDefault();
            if (row != null)
            {
                return row.LASTKEY1;
            }
            return 0;
        }

        public void InsertLastKey(long companyId, string keyString, int lastKey)
        {
            LASTKEY model = new LASTKEY();
            model.COMPANYID = companyId;
            model.KEYSTRING = keyString;
            model.LASTKEY1 = lastKey;
            dbSet.AddOrUpdate(model);
            context.SaveChanges();
        }

        public void UpdateLastKey(long companyId, string keyString, int lastKey)
        {
            var model = dbSet.Where(p => p.COMPANYID == companyId && p.KEYSTRING == keyString).FirstOrDefault();
            if (model != null)
                model.LASTKEY1 = lastKey;
            else
            {
                model = new LASTKEY();
                model.COMPANYID = companyId;
                model.KEYSTRING = keyString;
                model.LASTKEY1 = lastKey;
                dbSet.AddOrUpdate(model);
            }
            context.SaveChanges();
        }



    }
}
