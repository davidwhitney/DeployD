using System;
using System.Threading;
using System.Timers;
using log4net;
using Timer = System.Timers.Timer;

namespace Deployd.Core
{
    public class TimedSingleExecutionTask
    {
        private readonly Action _action;
        protected static readonly ILog Logger = LogManager.GetLogger("TimedSingleExecutionTask");

        private readonly Timer _cacheUpdateTimer;
        protected readonly object OneSyncAtATimeLock;
        
        public TimedSingleExecutionTask(int timerIntervalInMs, Action action)
        {
            _action = action;
            _cacheUpdateTimer = new Timer(timerIntervalInMs) { Enabled = true };
            OneSyncAtATimeLock = new object();
        }

        public void Start(string[] args)
        {
            _cacheUpdateTimer.Elapsed += Perform;
            _cacheUpdateTimer.Start();
            _action();
        }

        public void Stop()
        {
            _cacheUpdateTimer.Elapsed -= Perform;
            _cacheUpdateTimer.Stop();
        }

        private void Perform(object sender, ElapsedEventArgs e)
        {
            OneAtATime(_action);
        }

        public void OneAtATime(Action action)
        {
            if (!Monitor.TryEnter(OneSyncAtATimeLock))
            {
                Logger.Info("Skipping sync operation because a previous sync is still running.");
                return;
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                Monitor.Exit(OneSyncAtATimeLock);
            }
        }
    }
}
