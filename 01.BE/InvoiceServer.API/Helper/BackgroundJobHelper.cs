using Hangfire;
using System;
using System.Linq.Expressions;

namespace InvoiceServer.API.Helper
{
    public static class BackgroundJobHelper
    {
        #region Background Job

        public static void EnqueueBackgroundJob(Expression<Action> methodCall)
        {
            BackgroundJob.Enqueue(methodCall);
        }

        #endregion

        #region Delay Job

        public static void ScheduleBackgroundJob(Expression<Action> methodCall, DateTimeOffset enqueueAt)
        {
            BackgroundJob.Schedule(methodCall, enqueueAt);
        }

        #endregion

        #region Recurring Job

        public static void ClearRecuringJob(string jobJd)
        {
            RecurringJob.RemoveIfExists(jobJd);
        }

        #endregion
    }
}