namespace Deployd.Core.Hosting
{
    public interface IWindowsService
    {
        void Start(string[] args);
        void Stop();
        ApplicationContext AppContext { get; set; }
    }
}