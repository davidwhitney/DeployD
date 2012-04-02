namespace DeployD.Hub.Areas.Api.Code
{
    public interface IRepresentationBuilder
    {
        string BuildRepresentationOf<T>(T resource);
        string ContentType { get; }
    }
}