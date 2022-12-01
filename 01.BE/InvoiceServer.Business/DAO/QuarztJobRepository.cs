using InvoiceServer.Business.DAO.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    class QuarztJobRepository : GenericRepository<QUARZTJOB>, IQuarztJobRepository
    {
        public QuarztJobRepository(IDbContext context)
       : base(context)
        {
        }
        public QUARZTJOB findByID(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }
        public QUARZTJOB findByName(string name)
        {
            return this.dbSet.FirstOrDefault(p => p.JOBNAME == name);
        }
        public IEnumerable<QuarztJob> getAllJob(ConditionSearchQuartzJob condition)
        {
            var Jobs = this.context.Set<JOB>().AsQueryable();
            var qurzts = this.dbSet.ToList();
            var result = (from quart in qurzts
                          join job in Jobs on quart.JOBID equals job.ID
                          select new QuarztJob
                          {
                              ID = quart.ID,
                              JOBID = job.ID,
                              JOBNAME = quart.JOBNAME,
                              STATUS = quart.STATUS ?? false,
                              SCHEDULENAME = quart.SCHEDULENAME,
                              METHODNAME = job.METHODNAME,
                              SCRONEXPRESSION = quart.SCRONEXPRESSION,
                              LASTTIMERUN = (quart.LASTTIMERUN ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
                              NEXTTIMERUN = (quart.NEXTTIMERUN ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
                              PROCESSING = quart.PROCESSING ?? false,
                          }).ToList();
            return result;
        }

        #region Working Calendar
        public QUARZTJOB GetWorkingCalendar(long quarztJobId = 0)
        {
            if (quarztJobId == 0)
            {
                var quarzt = this.dbSet.FirstOrDefault(x => x.WORKINGCALENDAR != null && x.WORKINGCALENDAR != "");
                if (quarzt != null) return quarzt;
                return this.dbSet.FirstOrDefault();
            }
            return this.dbSet.FirstOrDefault(x => x.ID == quarztJobId);
        }
        #endregion Working Calendar
    }
}
