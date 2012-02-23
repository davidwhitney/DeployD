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

            Bind<IPackageRepository>().ToMethod(context =>
                                                    {
                                                        var factory = new PackageRepositoryFactory();
                                                        return factory.CreateRepository("http://packages.nuget.org");
                                                    });
        }
    }
}
