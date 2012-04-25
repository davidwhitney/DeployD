using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Core.AgentManagement
{
    public class AgentActionsRepository : IAgentActionsRepository
    {
        private readonly IFileSystem _fileSystem;
        private readonly IAgentSettings _agentSettings;

        public AgentActionsRepository(IFileSystem fileSystem, IAgentSettings agentSettings)
        {
            _fileSystem = fileSystem;
            _agentSettings = agentSettings;
        }

        public List<AgentAction> GetActionsForPackage(string packageId)
        {
            List<AgentAction> actions = new List<AgentAction>();
            // todo: finalize custom actions installation folder.
            // subdirectory in the application installation path? 
            // or another top-level folder in the nuget package and stored internally
            // by the agent?

            string path = Path.Combine(AgentSettings.AgentProgramDataPath, "customActions\\" +packageId);

            if(!_fileSystem.Directory.Exists(path))
            {
                return new List<AgentAction>();
            }

            var files = _fileSystem.Directory.GetFiles(path);
            foreach(var file in files)
            {
                var fileInfo = _fileSystem.FileInfo.FromFileName(file);
                actions.Add(new AgentAction()
                                {
                                    ScriptName=fileInfo.Name,
                                    ScriptPath = fileInfo.FullName,
                                    PackageId=packageId
                                });
            }

            return actions;
        }

        public AgentAction GetAction(string packageId, string action)
        {
            return GetActionsForPackage(packageId)
                .FirstOrDefault(a => a.PackageId == packageId && a.ScriptName == action);
        }
    }
}