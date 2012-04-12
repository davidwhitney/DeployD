using System.Collections.Generic;

namespace Deployd.Agent.WebUi.Models
{
    public class LogListViewModel : List<LogViewModel>
    {
        public string Group { get; set; }
        public LogListViewModel()
        {
        }

        public LogListViewModel(IEnumerable<LogViewModel> logs )
        {
            this.AddRange(logs);
        }
    }
}