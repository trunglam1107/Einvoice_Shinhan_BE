using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InvoiceServer.Common.Tasks
{
    public interface ITaskBase
    {
        void Start();
        void Stop();
        bool IsRunning();
        bool IsStart();
    }

    public abstract class TaskBase
    {
        #region AutoStart
        public int Intervals = 100; // The time in milliseconds
        public string TaskName = string.Empty;
        
        private readonly object runningLock = new object();
        private bool running = false;
        private Timer pollTimer;
        public string FileLog
        {
            get { return string.Format("{0}{1}.txt", (string.IsNullOrEmpty(TaskName) ? "" : TaskName + "_"), DateTime.Today.ToString("yyyyMMdd")); }
        }

        public bool IsStart()
        {
            return (pollTimer != null);
        }

        public void Start()
        {
            if (Intervals <= 0)
            {
                throw new ArgumentException("intervals <= 0", "intervals");
            }

            if (pollTimer != null) return;

            TimerCallback callbackMethod = new TimerCallback(this.CallExecute);
            pollTimer = new Timer(callbackMethod, null, Intervals, Intervals);
        }

        public virtual void Stop()
        {
            if (pollTimer == null)
            {
                throw new InvalidOperationException("Invalid polling stop operation");
            }
            pollTimer.Dispose();
            pollTimer = null;
        }

        public bool IsRunning()
        {
            return this.running;
        }

        private void CallExecute(object state)
        {
            lock (runningLock)
            {
                if (this.running == true) return;
            }
            this.running = true;
            try
            {
                this.Execute();
            }
            catch (Exception ex)
            {
               // this.LogInfo("Execute-Error:" + ex.ToJson());
            }
            finally
            {
                this.running = false;
            }
        }
        #endregion

        public TaskBase()
        {
        }
        public TaskBase(string taskName)
        {
            this.TaskName = taskName;
        }

        public abstract void Execute();

        protected void LogInfo(string msg)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
           // WebLog.Log.Data(now + "\t:\t" + msg, true, this.FileLog);
        }
    }
}
