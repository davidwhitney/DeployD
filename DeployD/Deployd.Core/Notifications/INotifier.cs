namespace Deployd.Core.Notifications
{
    public interface INotifier
    {
        void Notify(string message);
        bool Handles(EventType eventType);
        void OpenConnections();
    }
}
