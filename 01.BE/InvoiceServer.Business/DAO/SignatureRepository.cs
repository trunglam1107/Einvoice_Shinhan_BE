using InvoiceServer.Business.Models;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class SignatureRepository : GenericRepository<SIGNATURE>, ISignatureRepository
    {
        public SignatureRepository(IDbContext context)
            : base(context)
        {
        }

        public SIGNATURE GetSignatureByCompany(SignatureInfo signatureInfo)
        {
            return this.dbSet.FirstOrDefault(p => p.COMPANYID == signatureInfo.CompanyId);
        }
        public SIGNATURE GetSignatureInfo(long id)
        {
            return GetSignatureActive().Where(p => p.ID == id).FirstOrDefault();
        }
        public SIGNATURE GetBySlot(int slot, string cert)
        {
            return GetSignatureActive().Where(p => p.SLOTS == slot && p.SERIALNUMBER == cert).FirstOrDefault();
        }
        public IEnumerable<SIGNATURE> GetList(long companyId)
        {
            var curDate = DateTime.Now;
            return this.dbSet.Where(p => p.COMPANYID == companyId && curDate >= p.FROMDATE && curDate <= p.TODATE);
        }
        public IEnumerable<SIGNATURE> GetListByPassword(string password)
        {
            return GetSignatureActive().Where(p => p.PASSWORD == password);
        }
        public SIGNATURE GetByCompany(long companyId)
        {

            return this.dbSet.Where(p => p.COMPANYID == companyId).FirstOrDefault();
        }
        /// <summary>
        ///  Lấy danh sách chữ ký của công ty
        /// </summary>
        /// <param name="companyIds"></param>
        /// <returns></returns>
        public List<SIGNATURE> GetByCompanies(List<long> companyIds)
        {
            return this.dbSet.Where(f => companyIds.Contains(f.COMPANYID) && (f.DELETED == null || !f.DELETED.Value)).ToList();
        }

        public IEnumerable<SIGNATURE> FilterCA(ConditionSearchCA condition)
        {
            var signatures = GetSignatureActive();
            var companys = this.context.Set<MYCOMPANY>().Where(p => !(p.DELETED ?? false) && p.TAXCODE.ToUpper().Contains(condition.TaxCode.ToUpper())).FirstOrDefault();
            if (!condition.Branch.IsNullOrEmpty())
            {
                signatures = signatures.Where(p => p.COMPANYID == condition.Branch);
            }
            if (!condition.TaxCode.IsNullOrEmpty())
            {
                signatures = signatures.Where(p => p.COMPANYID == companys.COMPANYSID);
            }
            return signatures;
        }
        public IQueryable<SIGNATURE> GetSignatureActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false));
        }
    }
}
