namespace Deployd.Core.Notifications
{
    public interface INotificationService
    {
        void NotifyAll(EventType eventType, string message);
    }
}