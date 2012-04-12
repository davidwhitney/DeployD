using System;
using System.Diagnostics;
using System.Reflection;
using log4net;

namespace Deployd.Core
{
    public class DebugTimer : Stopwatch, IDisposable
    {
        private readonly string _name;
        protected static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public DebugTimer(string name = null)
        {
            _name = name ?? Guid.NewGuid().ToString();
            Log.InfoFormat("{0} started.", _name);
            Start();
        }

        public void Dispose()
        {
            Log.InfoFormat("{0} took {1} ms", _name, ElapsedMilliseconds);
            Stop();
        }
    }
}