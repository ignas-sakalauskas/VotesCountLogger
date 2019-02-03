using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace VotesCounter
{
    public class Worker
    {
        private BackgroundWorker worker;
        private readonly Action action;
        private readonly long workerNumber;
        private readonly TimeSpan sleepBetweenAction;

        public Worker(long workerNumber, TimeSpan sleepBetweenAction, Action action)
        {
            this.sleepBetweenAction = sleepBetweenAction;
            this.workerNumber = workerNumber;
            this.action = action;
        }

        public virtual void Start()
        {
            if (worker == null)
            {
                worker = new BackgroundWorker();
                worker.DoWork += PollingWork;
                worker.WorkerReportsProgress = false;
                worker.WorkerSupportsCancellation = true;
                worker.RunWorkerCompleted += PollingWorkCompleted;
                worker.RunWorkerAsync();
            }
        }

        public void Stop()
        {
            if (worker != null)
            {
                worker.CancelAsync();
                Trace.WriteLine(string.Format("Worker {0} was canceled at {1}.", workerNumber, DateTime.Now));
            }
        }

        private void PollingWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (action != null)
                    {
                        action();
                    }
                    
                    Thread.Sleep(sleepBetweenAction);
                }
                catch (ThreadAbortException)
                {
                    Trace.WriteLine(string.Format("Worker {0} was shutdown by ThreadAbortException at {1}.", workerNumber, DateTime.Now));
                    return;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Worker {0} failed {1}. Exception: {2}", workerNumber, DateTime.Now, ex));
                    Thread.Sleep(sleepBetweenAction);
                }
            }
        }

        private void PollingWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Do nothing
        }        
    }
}