using System.Collections.Generic;

namespace Deployd.Agent.WebUi.Models
{
    public class LogListViewModel
    {
        public string Group { get; set; }
        public List<LogViewModel> Logs { get; set; }
        public LogListViewModel()
        {
            Logs = new List<LogViewModel>();
        }

        public LogListViewModel(IEnumerable<LogViewModel> logs )
        {
            Logs = new List<LogViewModel>(logs);
        }
    }
}