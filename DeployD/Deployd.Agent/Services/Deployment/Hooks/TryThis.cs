using System;
using log4net;

namespace Deployd.Agent.Services.Deployment.Hooks
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
        private readonly ILog _logger = LogManager.GetLogger("TryThis");
        private int _times = 1;

        public TryThis(Action action)
        {
            _action = action;
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
                        _logger.Fatal("Failed to clean destination");
                        throw;
                    }
                    
                    _logger.Warn("Could not clean destination", ex);
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