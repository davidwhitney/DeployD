using System;

namespace Deployd.Agent.WebUi.Models
{
    public class LogViewModel
    {
        public string LogFilePath { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime DateCreated { get; set; }
        public string LogFileName { get; set; }
        public string Group { get; set; }
        public string LogContents { get; set; }
    }
}