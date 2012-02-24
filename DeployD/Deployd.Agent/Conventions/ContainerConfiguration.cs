using Deployd.Core.Caching;
using Deployd.Core.Queries;
using Ninject.Modules;
using NuGet;

namespace Deployd.Agent.Conventions
{
    public class ContainerConfiguration : NinjectModule
    {
        public override void Load()
        {
            Bind<IRetrieveAllAvailablePackageManifestsQuery>().To<RetrieveAllAvailablePackageManifestsQuery>();
            Bind<IPackageRepositoryFactory>().To<PackageRepositoryFactory>();
            Bind<INuGetPackageCache>().To<NuGetPackageCache>();

            Bind<FeedLocation>().ToMethod(context => new FeedLocation
                                                         {
                                                             Source = "http://packages.nuget.org"
                                                         });
        }
    }
}
