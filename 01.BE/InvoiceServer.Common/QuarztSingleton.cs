using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;

namespace InvoiceServer.Common
{
    public sealed class QuarztSingleton
    {
        private QuarztSingleton() { }
        private static readonly Lazy<QuarztSingleton> lazy = new Lazy<QuarztSingleton>(() => new QuarztSingleton());
        public static QuarztSingleton Instance
        {
            get { return lazy.Value; }
        }
        public static readonly IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();

        #region private method         
        readonly Logger logger = new Logger();
        private static IJobDetail addJob<T>(string nameJob, string nameGroup) where T : IJob
        {
            return JobBuilder.Create<T>()
                    .WithIdentity(nameJob, nameGroup)
                    .Build();
        }
        private static ITrigger addTrigger(string nameTrigger, string nameGroupnameTrigger, int second)
        {
            return TriggerBuilder.Create()
                     .WithIdentity(nameTrigger, nameGroupnameTrigger)
                     .WithDailyTimeIntervalSchedule(s => s
                            .WithIntervalInSeconds(second)
                            .OnEveryDay().
                            StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0)))
                     .Build();
        }
        public static bool addCron<T>(string nameJob, string nameGroup, string nameTrigger, string nameGroupnameTrigger, string cron) where T : IJob
        {
            try
            {
                IJobDetail job = JobBuilder.Create<T>()
                       .WithIdentity(nameJob, nameGroup)
                       .Build();
                ITrigger trig = TriggerBuilder.Create()
                         .WithIdentity(nameTrigger, nameGroupnameTrigger)
                         .WithCronSchedule(cron)
                         .ForJob(job)
                         .StartAt(DateBuilder.FutureDate(30, IntervalUnit.Second))
                         .Build();
                scheduler.ScheduleJob(job, trig);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
        #region public method
        public static string path { get; set; }
        public bool onStart()
        {
            try
            {
                scheduler.Start();
                logger.QuarztJob(false, "Quarzt Start");
                return true;
            }
            catch (Exception ex)
            {
                logger.QuarztJob(true, "Quarzt Start failed. Please Check log error");
                logger.Error("Mr Quarzt", ex);
                return false;
            }
        }
        public bool createJob<T>(string nameJob, string nameGroup, string nameTrigger, string nameGroupnameTrigger, string time) where T : IJob
        {
            try
            {

                int second = int.Parse(time.Substring(6, 2).ToString().TrimStart());

                scheduler.ScheduleJob(addJob<T>(nameJob, nameGroup), addTrigger(nameTrigger, nameGroupnameTrigger, second));
                scheduler.Start();


                return true;
            }
            catch (Exception)
            {


                return false;
            }
        }
        public bool createJobbyCron<T>(string nameTrigger, string nameGroupnameTrigger, string nameJob, string nameGroup, string cron) where T : IJob
        {
            try
            {
                addCron<T>(nameJob, nameGroup, nameTrigger, nameGroupnameTrigger, cron);

                return true;
            }
            catch (Exception)
            {


                return false;
            }
        }
        public static void GetAllJobs()
        {
            var allTriggerKeys = scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
            foreach (var triggerKey in allTriggerKeys.Result)
            {
                var triggerdetails = scheduler.GetTrigger(triggerKey);
                Console.WriteLine(triggerdetails.Result.Key.Name + " Job key -" + triggerdetails.Result.JobKey.Name + " " + triggerdetails.Result.CalendarName);
            }
        }
        public bool onPause()
        {
            try
            {
                scheduler.Shutdown();
                logger.QuarztJob(false, "Quarzt pause");
                return true;
            }
            catch (Exception ex)
            {
                logger.QuarztJob(true, "Quarzt pause failed. Please Check log error");
                logger.Error("Mr Quarzt", ex);
                return false;
            }
        }
        public bool onStopJob(string triggerName)
        {
            bool result = false;
            var allJobKey = scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var key in allJobKey.Result)
            {
                if (key.Name.Equals(triggerName))
                {
                    try
                    {
                        scheduler.PauseJob(key);
                        result = true;
                        logger.QuarztJob(false, "Quarzt stop Job: " + triggerName);
                    }
                    catch (Exception ex)
                    {
                        logger.QuarztJob(true, "Quarzt stop job failed. Please Check log error");
                        logger.Error("Mr Quarzt", ex);
                        result = false;
                    }
                }
            }
            return result;
        }
        public bool onResumeJob(string triggerName)
        {
            bool result = false;
            var allJobKey = scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var key in allJobKey.Result)
            {
                if (key.Name.Equals(triggerName))
                {
                    try
                    {
                        scheduler.ResumeJob(key);
                        result = true;
                        logger.QuarztJob(false, "Quarzt resume Job: " + triggerName);
                    }
                    catch (Exception ex)
                    {
                        logger.QuarztJob(true, "Quarzt stop job failed. Please Check log error");
                        logger.Error("Mr Quarzt", ex);
                        result = false;
                    }
                }
            }
            return result;
        }
        public bool onUpdateTrigger<T>(string nameTrigger, string nameGroupnameTrigger, string nameJob, string nameGroup, string cron) where T : IJob
        {
            try
            {
                onStopJob(nameTrigger);
                scheduler.UnscheduleJob(new TriggerKey(nameTrigger, nameGroupnameTrigger));
                scheduler.DeleteJob(new JobKey(nameJob));
                createJobbyCron<T>(nameTrigger, nameGroupnameTrigger, nameJob, nameGroup, cron);
                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }
        public DateTime next(string id)
        {
            DateTimeOffset? result = null;
            var allTriggerKeys = scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
            foreach (var triggerKey in allTriggerKeys.Result)
            {
                if (triggerKey.Name.Equals(id))
                {
                    ITrigger trigger = scheduler.GetTrigger(triggerKey).Result;
                    result = trigger.GetNextFireTimeUtc().Value;
                    result = trigger.GetFireTimeAfter(result);
                }
            }
            return result.HasValue ? result.Value.LocalDateTime : DateTime.Now;
        }
        public DateTime prev(string id)
        {
            DateTimeOffset? result = null;
            var allTriggerKeys = scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
            foreach (var triggerKey in allTriggerKeys.Result)
            {
                if (triggerKey.Name.Equals(id))
                {
                    ITrigger trigger = scheduler.GetTrigger(triggerKey).Result;
                    if (!trigger.GetPreviousFireTimeUtc().HasValue)
                    {
                        result = trigger.GetPreviousFireTimeUtc();
                        result = trigger.GetFireTimeAfter(result);
                    }
                    else
                    {
                        result = trigger.GetPreviousFireTimeUtc().Value;
                        result = trigger.GetFireTimeAfter(result);
                    }
                }
            }
            return result.HasValue ? result.Value.LocalDateTime : DateTime.Now;
        }
        #endregion
    }
}
