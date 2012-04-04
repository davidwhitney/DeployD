using System;
using log4net;

namespace Deployd.Core.Installation.Hooks
{
    public interface ITryThisNow
    {
        void Go();
    }

    public interface ISayTimes
    {
        ITryThisNow Times { get; }
    }

    public class TryThis : ITryThisNow, ISayTimes
    {
        private readonly Action _action;
        private readonly ILog _logger = null;
        private int _times = 1;

        public TryThis(Action action)
        {
            _action = action;
            _logger = LogManager.GetLogger("TryThis");
        }

        public TryThis(Action action, ILog logger)
        {
            _action = action;
            _logger = logger;
        }

        public void Go()
        {
            var retryCount = _times;
            var success = false;

            while (!success && retryCount-- > 0)
            {
                try
                {
                    _action();
                    success = true;
                }
                catch (Exception ex)
                {
                    if (retryCount == 0)
                    {
                        _logger.Fatal("Failed to execute a task", ex);
                        throw;
                    }
                    
                    _logger.Warn("Could not execute a task", ex);
                    _logger.WarnFormat("Will retry {0} more times", retryCount);
                    
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        public ITryThisNow Once()
        {
            _times = 1;
            return this;
        }

        public ITryThisNow Times
        {
            get{ return this; }
        }

        public ISayTimes UpTo(int times)
        {
            _times = times;
            return this;
        }

    }
}