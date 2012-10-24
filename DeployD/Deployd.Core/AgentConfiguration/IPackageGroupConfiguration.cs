namespace Deployd.Core.AgentConfiguration
{
    public interface IPackageGroupConfiguration
    {
        PackageGroup[] Groups { get; set; }
    }
}