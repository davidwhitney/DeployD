namespace Deployd.Core.AgentManagement
{
    public interface IActionTaskManager
    {
        ActionTask GetActionTaskDetails(string packageId, string action);
    }
}