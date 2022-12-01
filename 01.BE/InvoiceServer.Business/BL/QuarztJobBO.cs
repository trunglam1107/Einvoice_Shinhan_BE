using InvoiceServer.Business.DAO;
using InvoiceServer.Business.DAO.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class QuarztJobBO : IQuarztJobBO
    {
        private readonly IQuarztJobRepository quarztRespository;
        private readonly IDbTransactionManager transaction;

        public QuarztJobBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.quarztRespository = repoFactory.GetRepository<IQuarztJobRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
        }

        public IEnumerable<QuarztJob> getallJob(ConditionSearchQuartzJob condition)
        {
            return this.quarztRespository.getAllJob(condition);
        }
        public void UpdateProcessByName(string name, bool processing)
        {
            var quarzt = findByName(name);
            quarzt.PROCESSING = processing;
            this.quarztRespository.Update(quarzt);
        }
        public QUARZTJOB findByName(string name)
        {
            return this.quarztRespository.findByName(name);
        }
        public bool updateStatus(long id, bool status)
        {
            try
            {

                var quarzt = this.quarztRespository.findByID(id);
                quarzt.STATUS = status;
                this.quarztRespository.Update(quarzt);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ResultCode UpdateCron(QuarztJob model)
        {
            if (model == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            try
            {
                transaction.BeginTransaction();
                var quarzt = this.quarztRespository.findByID(model.ID);
                quarzt.SCRONEXPRESSION = model.SCRONEXPRESSION;
                this.quarztRespository.Update(quarzt);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        public IEnumerable<WorkingCalendarDAO> GetWorkingCalendar(int year)
        {
            var quarzt = this.quarztRespository.GetWorkingCalendar(0);
            var holidays = GenerateFullYearWorkingCalendarFromJson(quarzt.WORKINGCALENDAR).WorkingCalendars
                                    .Where(x => x.Day.Year == year || x.Day.Year == year - 1);
            return holidays;
        }

        private FullYearWorkingCalendarDAO GenerateFullYearWorkingCalendarFromJson(string json)
        {
            FullYearWorkingCalendarDAO fullYearWorkingCalendar = new FullYearWorkingCalendarDAO();
            try
            {
                fullYearWorkingCalendar = JsonConvert.DeserializeObject<FullYearWorkingCalendarDAO>(json);
            }
            catch
            {
                // do nothing
            }
            return fullYearWorkingCalendar;
        }
    }
}
