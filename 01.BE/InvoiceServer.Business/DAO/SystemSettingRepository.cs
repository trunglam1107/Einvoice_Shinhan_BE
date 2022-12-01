using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class SystemSettingRepository : GenericRepository<SYSTEMSETTING>, ISystemSettingRepository
    {
        private readonly MyCompanyRepository myCompanyRepository;
        private readonly Logger logger = new Logger();

        public SystemSettingRepository(IDbContext context)
            : base(context)
        {
            myCompanyRepository = new MyCompanyRepository(context);
        }

        public IEnumerable<SYSTEMSETTING> FilterSystemSettings(ConditionSearchSystemSetting condition)
        {
            var systemSettings = GetSystemSettingtActive();
            return systemSettings;
        }

        private IQueryable<SYSTEMSETTING> GetSystemSettingtActive()
        {
            return dbSet.Select(s => s);
        }

        public SystemSettingInfo FilterSystemSetting(long companyId)
        {
            var systemSettings = GetSystemSettingtActive()
                        .FirstOrDefault();

            var myCompany = myCompanyRepository.GetById(companyId);
            if (myCompany != null && myCompany.LEVELCUSTOMER == CustomerLevel.Customer && systemSettings == null)
            {
                systemSettings = new SYSTEMSETTING()
                {
                    STEPTOCREATEINVOICENO = StepToCreateInvoiceNo.WhenSign,
                };

                this.Insert(systemSettings);
            }

            var systemSettingsInfo = new SystemSettingInfo(systemSettings);
            return systemSettingsInfo;
        }
        public SystemSettingInfo FilterSystemSettingJob()
        {
            var systemSettings = GetSystemSettingtActive().FirstOrDefault();
            //logger.Error("Setting amount before : " + systemSettings.AMOUNT, new Exception("setting amount before"));
            var systemSettingsInfo = new SystemSettingInfo(systemSettings);
            //logger.Error("Setting amount after : " + systemSettingsInfo.Amount, new Exception("setting amount after"));
            return systemSettingsInfo;

        }
        public SYSTEMSETTING GetByCompany(long companyId)
        {
            return this.dbSet.FirstOrDefault();
        }
        public SYSTEMSETTING CheckSlots(int? slot, long companyId)
        {
            return this.dbSet.FirstOrDefault();
        }
    }
}
