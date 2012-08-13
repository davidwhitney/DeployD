namespace Deployd.Core.AgentConfiguration
{
    public static class PackageGroupConfigurationFactory
    {
        public static IPackageGroupConfiguration Build()
        {
            // todo: need defaults
            return new PackageGroupConfiguration()
                       {
                           Groups = new[]
                                        {
                                            new PackageGroup("FrontOfficeWeb", 
                                                             new[]
                                                                 {
                                                                     "GG.Web.Website",
                                                                     "GG.Web.Website.Charity"
                                                                 }), 
                                        }
                       };
        }
    }
}