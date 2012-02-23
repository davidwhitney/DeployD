using Deployd.Core.FileFormatAdapters;
using NUnit.Framework;

namespace Deployd.Core.Test.Unit.FileFormatAdapters
{
    [TestFixture]
    public class NuGetFeedAdapterTests
    {
        private NuGetFeedAdapter _packageReader;

        [SetUp]
        public void SetUp()
        {
            _packageReader = new NuGetFeedAdapter();
        }
    }
}
