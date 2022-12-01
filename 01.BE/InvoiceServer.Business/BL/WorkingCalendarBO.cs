using InvoiceServer.Business.BL.Interface;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.DAO.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.BL
{
    public class WorkingCalendarBO : IWorkingCalendarBO
    {
        private readonly IQuarztJobRepository quarztRespository;
        private readonly IDbTransactionManager transaction;
        private readonly PrintConfig printConfig;
        public WorkingCalendarBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.quarztRespository = repoFactory.GetRepository<IQuarztJobRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
        }
        public WorkingCalendarBO(IRepositoryFactory repoFactory, PrintConfig printConfig) : this(repoFactory)
        {
            this.printConfig = printConfig;
        }
        #region Working Calendar
        public IEnumerable<WorkingCalendarDAO> GetWorkingCalendar(int year, long quarztJobId = 0)
        {
            var quarzt = this.quarztRespository.GetWorkingCalendar(quarztJobId);
            var holidays = GenerateFullYearWorkingCalendarFromJson(quarzt.WORKINGCALENDAR).WorkingCalendars
                                    .Where(x => x.Day.Year == year);
            return holidays;
         }

        public FullYearWorkingCalendarDAO GetYears(long quarztJobId = 0)
        {
            var quarzt = this.quarztRespository.GetWorkingCalendar(quarztJobId);
            var res = GenerateFullYearWorkingCalendarFromJson(quarzt.WORKINGCALENDAR);
            if (res.Years.Count == 0)
            {
                this.AddYearWorkingCalendar(quarztJobId, DateTime.Now.Year);
                res = GenerateFullYearWorkingCalendarFromJson(quarzt.WORKINGCALENDAR);
            }

            var maxYearLoad = DateTime.Now.Year + 1000;
            res.Years = res.Years.Where(x => x <= maxYearLoad).ToList();
            res.WorkingCalendars.Clear();
            return res;
        }

        public ResultCode UpdateWorkingCalendar(WorkingCalendarDAO calendarDAO, long quarztJobId = 0)
        {
            var quarzt = this.quarztRespository.GetWorkingCalendar(quarztJobId);
            var fullYearWorkingCalendar = GenerateFullYearWorkingCalendarFromJson(quarzt.WORKINGCALENDAR);
            var holidays = fullYearWorkingCalendar.WorkingCalendars;

            var holidayFound = holidays.FirstOrDefault(x => x.Day == calendarDAO.Day);
            if (holidayFound == null)
            {
                holidays.Add(calendarDAO);
            }
            else
            {
                holidayFound.Type = calendarDAO.Type;
            }
            // Lọc lại list loại bỏ ngày trong tuần mà type == "", ngày cuối tuần mà type == "S"
            holidays = holidays
                        .Where(x =>
                                    (CommonUtil.IsWeekend(x.Day) && x.Type != "S")   // Ngày cuối tuần mà type khác "S"
                                || !CommonUtil.IsWeekend(x.Day) && x.Type != "")  // Ngày trong tuần mà type khác ""
                        .Distinct()
                        .OrderBy(x => x.Day)
                        .ToList();

            fullYearWorkingCalendar.WorkingCalendars = holidays;
            var fullYearWorkingCalendarString = JsonConvert.SerializeObject(fullYearWorkingCalendar);

            if (quarztJobId == 0)
            {
                var quarzts = this.quarztRespository.GetAll().ToList();
                foreach (var q in quarzts)
                {
                    q.WORKINGCALENDAR = fullYearWorkingCalendarString;
                    this.quarztRespository.Update(q);
                }
            }
            else
            {
                var q = this.quarztRespository.GetAll().FirstOrDefault(x => x.ID == quarztJobId);
                q.WORKINGCALENDAR = fullYearWorkingCalendarString;
                this.quarztRespository.Update(q);
            }

            return ResultCode.NoError;
        }

        public List<WorkingCalendarDTO> GetWorkingCalendarForPostingGL(int year, long quarztJobId = 0)
        {
            var list = this.GetWorkingCalendar(year, quarztJobId);
            var res = new List<WorkingCalendarDTO>();
            foreach (var workingDay in list)
            {
                var workingDayDto = new WorkingCalendarDTO()
                {
                    Year = year,
                    Month = workingDay.Day.Month,
                    DayOfYear = workingDay.Day.DayOfYear,
                    DayOfMonth = workingDay.Day.Day,
                    DateType = workingDay.Type,
                };
                res.Add(workingDayDto);
            }
            return res;
        }

        public FullYearWorkingCalendarDAO AddYearWorkingCalendar(long quarztJobId = 0, int year = 0)
        {
            var quarzt = this.quarztRespository.GetWorkingCalendar(quarztJobId);
            var fullYearWorkingCalendar = GenerateFullYearWorkingCalendarFromJson(quarzt.WORKINGCALENDAR);

            if (year == 0)
            {
                int lastYear = fullYearWorkingCalendar.Years.Count == 0 ? DateTime.Now.Year : fullYearWorkingCalendar.Years.OrderByDescending(x => x).First();
                lastYear += 1;
                fullYearWorkingCalendar.Years.Add(lastYear);
            }
            else
            {
                fullYearWorkingCalendar.Years.Add(year);
            }
            fullYearWorkingCalendar.Years = fullYearWorkingCalendar.Years.Distinct().OrderBy(x => x).ToList();

            var fullYearWorkingCalendarString = JsonConvert.SerializeObject(fullYearWorkingCalendar);

            if (quarztJobId == 0)
            {
                var quarzts = this.quarztRespository.GetAll().ToList();
                foreach (var q in quarzts)
                {
                    q.WORKINGCALENDAR = fullYearWorkingCalendarString;
                    this.quarztRespository.Update(q);
                }
            }
            else
            {
                var q = this.quarztRespository.GetAll().FirstOrDefault(x => x.ID == quarztJobId);
                q.WORKINGCALENDAR = fullYearWorkingCalendarString;
                this.quarztRespository.Update(q);
            }

            var maxYearLoad = DateTime.Now.Year + 1000;
            fullYearWorkingCalendar.Years = fullYearWorkingCalendar.Years.Where(x => x <= maxYearLoad).ToList();
            fullYearWorkingCalendar.WorkingCalendars.Clear();
            return fullYearWorkingCalendar;
        }

        public IEnumerable<WorkingCalendarDAO> GetAllMaxWorkingDayFromToDay(int maxDay, long quarztJobId = 0)
        {
            var currentDay = DateTime.Now.Date;
            var quarzt = this.quarztRespository.GetWorkingCalendar(quarztJobId);
            var workingClendar = this.GenerateFullYearWorkingCalendarFromJson(quarzt.WORKINGCALENDAR).WorkingCalendars;

            var count = 0;
            var daySub = 0;
            while (count < maxDay)
            {
                var day = currentDay.AddDays(-daySub);
                var workingCalendarDao = workingClendar.FirstOrDefault(x => x.Day == day);
                if (workingCalendarDao == null)
                {
                    workingCalendarDao = new WorkingCalendarDAO()
                    {
                        Day = day,
                        Type = CommonUtil.IsWeekend(day) ? "S" : ""
                    };
                }
                yield return workingCalendarDao;
                if (workingCalendarDao.Type == "")
                {
                    count++;
                }
                daySub++;
            }
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
        #endregion Working Calendar
    }
}
