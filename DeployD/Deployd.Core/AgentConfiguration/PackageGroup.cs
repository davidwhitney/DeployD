namespace Deployd.Core.AgentConfiguration
{
    public class PackageGroup
    {
        public PackageGroup(string groupName, string[] packageIds)
        {
            GroupName = groupName;
            PackageIds = packageIds;
        }
        public string GroupName { get; private set; }
        public string[] PackageIds { get; private set; }
    }
}