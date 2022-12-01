using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InvoiceServer.Common.TaskScheduler
{
    public interface IBackgroundQueue
    {
        void Execute();
        //void Enqueue();
    }

    public class BackgroundQueue: InvoiceServer.Common.TaskScheduler.ITask
    {

        const string SERVICE_KEY = "BACKGROUND_QUEUE";
        private static object runningLock = new object();
        private static bool isRunning = false;
        private static Queue<IBackgroundQueue> queues = new Queue<IBackgroundQueue>();

        private static string FileLog
        {
            get { return string.Format("BackgroundQueue_{0}.txt", DateTime.Today.ToString("yyyyMM")); }
        }

        public static void Start()
        {
            int seconds =5;
            BackgroundQueue task = new BackgroundQueue();
            BackgroundScheduler.Start(SERVICE_KEY, task, seconds);
        }

        public static void Enqueue(IBackgroundQueue item)
        {
            Monitor.Enter(queues);
            try
            {
                queues.Enqueue(item);
            }
            catch (Exception ex)
            {
               // WebLog.Log.Error("BackgroundQueue.Enqueue:" + item.ToJson(), ex.Message, FileLog);
            }
            Monitor.Exit(queues);
        }

        private static IBackgroundQueue Dequeue()
        {
            Monitor.Enter(queues);
            IBackgroundQueue result = null;
            try
            {
                if (queues.Count > 0)
                {
                    result = queues.Dequeue();
                }
            }
            catch (Exception)
            {
                //WebLog.Log.Error("BackgroundQueue.Dequeue", ex.Message, FileLog);
            }
            Monitor.Exit(queues);

            return result;
        }

        public void Run(object stateInfo)
        {
            lock (runningLock)
            {
                if (isRunning) return;
            }

            isRunning = true;
            try
            {
                IBackgroundQueue item = Dequeue();
                int hashCode = (item == null ? 0 : item.GetHashCode());

                // process all queue
                while (item != null)
                {
                    // execute
                    item.Execute();

                    // dequeue
                    item = Dequeue();
                    if (item != null)
                    {
                        // loop queue
                        if (hashCode == item.GetHashCode())
                        {
                            Enqueue(item);
                            break;
                        }
                        hashCode = item.GetHashCode();
                    }
                    Thread.Sleep(10);
                }
            }
            catch(Exception ex)
            {
                //WebLog.Log.Error("BackgroundQueue.Run", ex.Message, FileLog);
            }
            isRunning = false;
        }
    }

    public class BackgroundQueueBase : IBackgroundQueue
    {
        public virtual string FileLog()
        {
            return string.Format("Queue_{0}.txt", DateTime.Today.ToString("yyyyMM"));
        }

        public virtual string FileFailed()
        {
            return string.Format("Queue_Failed_{0}.txt", DateTime.Today.ToString("yyyyMM"));
        }

        private DateTime _NextProcess = DateTime.Today;
        public DateTime NextProcess
        {
            get { return _NextProcess; }
            set { _NextProcess = value; }
        }

        private int _TryCounter = 0;
        public int TryCounter
        {
            get { return _TryCounter; }
            set { _TryCounter = value; }
        }

        public void WriteQueue(string data)
        {
           // WebLog.Log.Data(new string[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), this.ToString(), data }, true, this.FileFailed());
        }

        public bool IsWaiting()
        {
            return (DateTime.Now < this.NextProcess);
        }

        public bool IsWaiting(bool enqueue)
        {
            bool res = (DateTime.Now < this.NextProcess);
            if (res && enqueue)
            {
                BackgroundQueue.Enqueue(this);
            }
            return res;
        }
        public void Enqueue()
        {
            BackgroundQueue.Enqueue(this);
        }
        public virtual void Execute()
        {
        }
        public bool TryEnQueue(int seconds)
        {
            if (this.TryCounter >= 5) return false;

            this.TryCounter++;
            this.NextProcess = DateTime.Now.AddSeconds(seconds);
            this.Enqueue(); // Retry

            return true;
        }
    }
}
