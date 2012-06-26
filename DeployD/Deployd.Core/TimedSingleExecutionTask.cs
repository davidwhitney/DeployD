using System;
using System.Threading;
using System.Timers;
using Ninject.Extensions.Logging;
using Timer = System.Timers.Timer;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Core
{
    public class TimedSingleExecutionTask
    {
        private readonly Action _action;
        private readonly bool _runWhenCreated;
        private readonly ILogger _logger;

        private readonly Timer _cacheUpdateTimer;
        protected readonly object OneSyncAtATimeLock;

        public bool IsRunning { get; private set; }
        
        public TimedSingleExecutionTask(int timerIntervalInMs, Action action, ILogger logger, bool runWhenCreated = false)
        {
            _action = action;
            _logger = logger;
            _runWhenCreated = runWhenCreated;
            _cacheUpdateTimer = new Timer(timerIntervalInMs) { Enabled = true };
            OneSyncAtATimeLock = new object();
        }

        public void Start(string[] args)
        {
            IsRunning = true;
            _cacheUpdateTimer.Elapsed += Perform;
            _cacheUpdateTimer.Start();

            if (_runWhenCreated)
            {
                _action();
            }
        }

        public void Stop()
        {
            IsRunning = false;
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
                _logger.Info("Skipping sync operation because a previous sync is still running.");
                return;
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                _logger.Error(ex,"Sync Failed");
            }
            finally
            {
                Monitor.Exit(OneSyncAtATimeLock);
            }
        }
    }
}
