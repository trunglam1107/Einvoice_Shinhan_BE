using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace InvoiceServer.Common.TaskScheduler
{
    /// <summary>
    /// 
    /// </summary>
    public class BackgroundScheduler
    {
        private static Hashtable HScheduler = Hashtable.Synchronized(new Hashtable());

        private ExpirationPollTimer pollTimer;
        /// <summary>
        /// 
        /// </summary>
        public BackgroundScheduler()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="task"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static bool Start(string serviceKey, ITask task, int seconds)
        {
            bool success = false;
            serviceKey = (serviceKey ?? "default").ToLower();
            if (HScheduler.ContainsKey(serviceKey))
            {
                success = true;
                return success;
            }
            try
            {
                BackgroundScheduler bgScheduler = new BackgroundScheduler();
                bgScheduler.pollTimer = new ExpirationPollTimer();
                bgScheduler.pollTimer.StartPolling(new TimerCallback(task.Run), seconds * 1000);
                HScheduler.Add(serviceKey, bgScheduler);
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                //WebLog.Log.Error("BackgroundScheduler.Start", ex.Message);
            }
            return success;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <returns></returns>
        public static bool Stop(string serviceKey)
        {
            bool success = false;
            serviceKey = (serviceKey ?? "default").ToLower();
            if (HScheduler.ContainsKey(serviceKey) == false)
            {
                success = true;
                return success;
            }
            try
            {
                BackgroundScheduler bgScheduler = (BackgroundScheduler)HScheduler[serviceKey];
                bgScheduler.pollTimer.StopPolling();
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                //WebLog.Log.Error("BackgroundScheduler.Stop", ex.Message);
            }
            return success;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// 
        /// </summary>
        void Run(object notUsed);
    }

    /// <summary>
    /// Represents an expiration poll timer.
    /// </summary>
    public sealed class ExpirationPollTimer : IDisposable
    {
        private Timer pollTimer;

        /// <summary>
        /// Start the polling process.
        /// </summary>
        /// <param name="callbackMethod">The method to callback when a cycle has completed.</param>
        /// <param name="pollCycleInMilliseconds">The time in milliseconds to poll.</param>
        public void StartPolling(TimerCallback callbackMethod, int pollCycleInMilliseconds)
        {
            if (callbackMethod == null)
            {
                throw new ArgumentNullException("callbackMethod");
            }
            if (pollCycleInMilliseconds <= 0)
            {
                throw new ArgumentException("pollCycleInMilliseconds <= 0", "pollCycleInMilliseconds");
            }

            pollTimer = new Timer(callbackMethod, null, pollCycleInMilliseconds, pollCycleInMilliseconds);
        }

        /// <summary>
        /// Stop the polling process.
        /// </summary>
        public void StopPolling()
        {
            if (pollTimer == null)
            {
                throw new InvalidOperationException("Invalid polling stop operation");
            }

            pollTimer.Dispose();
            pollTimer = null;
        }

        void IDisposable.Dispose()
        {
            if (pollTimer != null) pollTimer.Dispose();
        }
    }
}
