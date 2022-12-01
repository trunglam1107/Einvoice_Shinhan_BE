using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class SystemLogRepository : GenericRepository<SYSTEMLOG>, ISystemLogRepository
    {
        public SystemLogRepository(IDbContext context)
            : base(context)
        {

        }

        public IEnumerable<SystemLogInfo> FilterSystemLog(ConditionSearchSystemLog condition)
        {
            var systemLogs = GetList();

            if (condition.UserName.IsNotNullOrEmpty())
            {
                systemLogs = systemLogs.Where(p => p.USERNAME.ToUpper().Contains(condition.UserName.ToUpper()));
            }

            var funtions = this.context.Set<FUNCTION>().Where(p => p.ID >= 0);
            if (condition.DateFrom.HasValue)
            {
                systemLogs = systemLogs.Where(p => DbFunctions.TruncateTime(p.LOGDATE) >= DbFunctions.TruncateTime(condition.DateFrom));
            }

            if (condition.DateTo.HasValue)
            {
                systemLogs = systemLogs.Where(p => DbFunctions.TruncateTime(p.LOGDATE) <= DbFunctions.TruncateTime(condition.DateTo));
            }
            var systemLogsInfo = (from systemLog in systemLogs
                                  join loginUser_ in this.context.Set<LOGINUSER>() on systemLog.USERNAME.ToLower() equals loginUser_.USERID.ToLower()
                                  join funtion_ in funtions on systemLog.FUNCTIONCODE equals funtion_.FUNCTIONNAME into funtionTemp
                                  from funtion in funtionTemp.DefaultIfEmpty()
                                  where (!funtion.USERLEVELSID.HasValue)
                                     || (funtion.USERLEVELSID == loginUser_.USERLEVELSID)
                                  select new SystemLogInfo
                                  {
                                      FunctionCode = systemLog.FUNCTIONCODE,
                                      //FunctionName = funtion.DETAILFUNCTION == null ? systemLog.FUNCTIONCODE : funtion.DETAILFUNCTION,
                                      FunctionName = funtion.DETAILFUNCTION,
                                      LogSummary = systemLog.LOGSUMMARY,
                                      LogType = systemLog.LOGTYPE,
                                      LogDate = systemLog.LOGDATE,
                                      LogDetail = systemLog.LOGDETAIL,
                                      IP = systemLog.IP,
                                      UserName = systemLog.USERNAME,
                                  });
            if (condition.FucntionName.IsNotNullOrEmpty())
            {
                systemLogsInfo = systemLogsInfo.Where(p => p.FunctionName != null && p.FunctionName.ToUpper().Contains(condition.FucntionName.ToUpper()));
            }
            return systemLogsInfo.OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take);
        }
        public int Count(ConditionSearchSystemLog condition)
        {
            var systemLogs = GetList();

            if (condition.UserName.IsNotNullOrEmpty())
            {
                systemLogs = systemLogs.Where(p => p.USERNAME.ToUpper().Contains(condition.UserName.ToUpper()));
            }

            var funtions = this.context.Set<FUNCTION>().Where(p => p.ID >= 0);
            if (condition.DateFrom.HasValue)
            {
                systemLogs = systemLogs.Where(p => p.LOGDATE >= condition.DateFrom);
            }

            if (condition.DateTo.HasValue)
            {
                var dateTo = ((DateTime)condition.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                systemLogs = systemLogs.Where(p => p.LOGDATE <= dateTo);
            }

            var systemLogsInfo = (from systemLog in systemLogs
                                  join loginUser_ in this.context.Set<LOGINUSER>() on systemLog.USERNAME.ToLower() equals loginUser_.USERID.ToLower()
                                  join funtion_ in funtions on systemLog.FUNCTIONCODE equals funtion_.FUNCTIONNAME into funtionTemp
                                  from funtion in funtionTemp.DefaultIfEmpty()
                                  where (!funtion.USERLEVELSID.HasValue)
                                     || (funtion.USERLEVELSID == loginUser_.USERLEVELSID)
                                  select new SystemLogInfo
                                  {
                                      FunctionCode = systemLog.FUNCTIONCODE,
                                      FunctionName = funtion.DETAILFUNCTION
                                  }).AsEnumerable();
            if (condition.FucntionName.IsNotNullOrEmpty())
            {
                systemLogsInfo = systemLogsInfo.Where(p => p.FunctionName != null && p.FunctionName.ToUpper().Contains(condition.FucntionName.ToUpper()));
            }
            return systemLogsInfo.Count();
        }

        private IQueryable<SYSTEMLOG> GetList()
        {
            return dbSet.Where(s => s.ID >= 0);
        }
    }
}
