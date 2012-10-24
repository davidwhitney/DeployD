using System.Collections.Generic;
using System.Threading;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Agent.Services.PackageDownloading;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Installation;
using Deployd.Core.Notifications;
using Deployd.Core.PackageCaching;
using Deployd.Core.PackageTransport;
using Deployd.Core.Remoting;
using Moq;
using NUnit.Framework;
using NuGet;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Agent.Test.Unit.Services.PackageDownloading
{
    [TestFixture]
    public class PackageDownloadingServiceTests
    {
        private Mock<IInstalledPackageArchive> _installCached;
        private Mock<IRetrievePackageQuery> _packageRepoMock;
        private Mock<ILocalPackageCache> _packageCacheMock;
        private PackageDownloadingService _pds;
        private Mock<IAgentConfigurationManager> _agentConfigManagerMock;
        private Mock<IAgentSettingsManager> _agentSettings;
        private Mock<ILogger> _logger = new Mock<ILogger>();
        private Mock<IHubCommunicator> _hubCommunicator;
        private Mock<IPackageRepositoryFactory> _packageRepositoryFactory = new Mock<IPackageRepositoryFactory>();
        private Mock<ICurrentlyDownloadingList> _currentlyDownloadingList = new Mock<ICurrentlyDownloadingList>();
        private Mock<CompletedInstallationTaskList> _installationResultDictionary = new Mock<CompletedInstallationTaskList>();
        protected Mock<IAgentWatchList> _agentWatchList = new Mock<IAgentWatchList>();
        protected Mock<IInstallationManager> _installationManager = new Mock<IInstallationManager>();
        private IPackagesList _allPackagesList;
        private const string PACKAGE_ID = "packageId";

        [SetUp]
        public void SetUp()
        {
            _hubCommunicator = new Mock<IHubCommunicator>();
            _agentSettings = new Mock<IAgentSettingsManager>();
            _agentSettings.SetupGet(s=>s.Settings).Returns(new AgentSettings { DeploymentEnvironment = "Staging", PackageSyncIntervalMs = 1 });
            _agentConfigManagerMock = new Mock<IAgentConfigurationManager>();
            _agentConfigManagerMock.Setup(x => x.GetWatchedPackages(_agentSettings.Object.Settings.DeploymentEnvironment)).Returns(new List<WatchPackage> { new WatchPackage() { Name = PACKAGE_ID } });
            _packageRepoMock = new Mock<IRetrievePackageQuery>();
            _packageCacheMock = new Mock<ILocalPackageCache>();
            _installCached = new Mock<IInstalledPackageArchive>();


            _pds = new PackageDownloadingService(_agentSettings.Object, _packageRepoMock.Object, _packageCacheMock.Object,
                _agentConfigManagerMock.Object, _logger.Object, _hubCommunicator.Object, _installCached.Object, _packageRepositoryFactory.Object, _allPackagesList, _currentlyDownloadingList.Object, _installationResultDictionary.Object,
                _agentWatchList.Object, _installationManager.Object, new Mock<INotificationService>().Object);
            _allPackagesList = new AllPackagesList(_agentConfigManagerMock.Object, _agentSettings.Object.Settings);
        }

        [Test]
        public void LocallyCachePackages_CallsPackageRepoForPackageList()
        {
            IPackage items = null;
            _packageRepoMock.Setup(x => x.GetLatestPackage(PACKAGE_ID)).Returns(items);

            _pds.FetchPackages();

            _packageRepoMock.Verify(x => x.GetLatestPackage(PACKAGE_ID));
        }

        [Test]
        public void Start_WhenInvoked_UsesTimerToCallDownloadConfiguration()
        {
            IPackage items = null;
            _packageRepoMock.Setup(x => x.GetLatestPackage(PACKAGE_ID)).Returns(items);

            _pds.Start(new string[0]);
            Thread.Sleep(100);

            _packageRepoMock.VerifyAll();
        }

        [Test]
        public void Stop_WhenInvoked_StopsTask()
        {
            _agentSettings.SetupGet(s => s.Settings.ConfigurationSyncIntervalMs).Returns(10000);

            _pds.Start(new string[0]);
            Assert.That(_pds.TimedTask.IsRunning, Is.True);

            _pds.Stop();
            Assert.That(_pds.TimedTask.IsRunning, Is.False);
        }

    }
}
