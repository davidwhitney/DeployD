namespace Deployd.Core.Remoting
{
    public interface IHubCommunicator
    {
        void SendStatusToHubAsync(AgentStatusReport status);
        void SendStatusToHub(AgentStatusReport status);
    }
}