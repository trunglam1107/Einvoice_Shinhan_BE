using InvoiceServer.Business.BL;
using InvoiceServer.Business.BL.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness'
    public class WorkingCalendarBussiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness'
    {
        private readonly IWorkingCalendarBO wcBO;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.WorkingCalendarBussiness(IBOFactory)'
        public WorkingCalendarBussiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.WorkingCalendarBussiness(IBOFactory)'
        {
            this.wcBO = boFactory.GetBO<IWorkingCalendarBO>();
        }

        #region Working Calendar
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.GetWorkingCalendar(int, long)'
        public List<WorkingCalendarDTO> GetWorkingCalendar(int year, long quarztJobId = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.GetWorkingCalendar(int, long)'
        {
            var list = this.wcBO.GetWorkingCalendar(year, quarztJobId);
            var res = new List<WorkingCalendarDTO>();
            foreach (var workingDay in list)
            {
                var workingDayDto = new WorkingCalendarDTO()
                {
                    Year = year,
                    Month = workingDay.Day.Month - 1,
                    DayOfYear = workingDay.Day.DayOfYear - 1,
                    DayOfMonth = (workingDay.Day.DayOfYear - (new DateTime(year, workingDay.Day.Month, 1)).DayOfYear) - 1,
                    DateType = workingDay.Type,
                };
                res.Add(workingDayDto);
            }
            return res;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.GetYears(long)'
        public FullYearWorkingCalendarDAO GetYears(long quarztJobId = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.GetYears(long)'
        {
            var res = this.wcBO.GetYears(quarztJobId);
            return res;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.UpdateWorkingCalendar(WorkingCalendarDTO, long)'
        public ResultCode UpdateWorkingCalendar(WorkingCalendarDTO calendarDTO, long quarztJobId = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.UpdateWorkingCalendar(WorkingCalendarDTO, long)'
        {
            var holiday = new DateTime(calendarDTO.Year, calendarDTO.Month + 1, calendarDTO.DayOfMonth + 1);
            var workingDayDao = new WorkingCalendarDAO()
            {
                Day = holiday,
                Type = calendarDTO.DateType,
            };
            return this.wcBO.UpdateWorkingCalendar(workingDayDao, quarztJobId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.AddYearWorkingCalendar(long)'
        public FullYearWorkingCalendarDAO AddYearWorkingCalendar(long quarztJobId = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.AddYearWorkingCalendar(long)'
        {
            return this.wcBO.AddYearWorkingCalendar(quarztJobId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.IsRunJob(long)'
        public bool IsRunJob(long quarztId = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarBussiness.IsRunJob(long)'
        {
            var currentDay = DateTime.Now.Date;
            var workingCalendar = this.wcBO.GetWorkingCalendar(currentDay.Year, quarztId);
            if (CommonUtil.IsWeekend(currentDay)
                && workingCalendar.Any(x => x.Day == currentDay && x.Type == ""))
            {
                // Ngày cuối tuần nhưng được cấu hình là ngày làm việc
                return true;
            }

            if (workingCalendar.Any(x => x.Day == currentDay && x.Type == "H"))
            {
                // Ngày lễ
                return false;
            }

            return !CommonUtil.IsWeekend(currentDay);
        }
        #endregion Working Calendar
    }
}