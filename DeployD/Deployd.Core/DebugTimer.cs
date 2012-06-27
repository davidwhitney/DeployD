using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Core
{
    public class DebugTimer : Stopwatch, IDisposable
    {
        private readonly string _name;
        protected static ILog Log = LogManager.GetLogger(typeof(DebugTimer));

        public DebugTimer(string name = null)
        {
            _name = name ?? Guid.NewGuid().ToString();
            Log.Info(string.Format("{0} started.", _name));
            Start();
        }

        public void Dispose()
        {
            Log.Info(string.Format("{0} took {1} ms", _name, ElapsedMilliseconds));
            Stop();
        }
    }
}