using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Deployd.Core.AgentConfiguration;
using Ninject.Extensions.Logging;

namespace Deployd.Core.Remoting
{
    public class HubCommunicator : IHubCommunicator
    {
        private readonly IAgentSettingsManager _agentSettingsManager;
        private readonly ILogger _log;
        private readonly HubCommunicationsQueue _communicationsQueue;

        public HubCommunicator(IAgentSettingsManager agentSettingsManager, ILogger log, HubCommunicationsQueue communicationsQueue)
        {
            _agentSettingsManager = agentSettingsManager;
            _log = log;
            _communicationsQueue = communicationsQueue;
        }

        public void SendStatusToHubAsync(AgentStatusReport status)
        {
            Task task = new Task(()=>
            {
                var status1 = status;
                SendStatusToHub(status1);
            });
            _communicationsQueue.Enqueue(task);
        }

        public void SendStatusToHub(AgentStatusReport status)
        {
            var _pingRequest = HttpWebRequest.Create(string.Format("{0}/api/agent/{1}/status",
                                                                   _agentSettingsManager.Settings.HubAddress,
                                                                   Environment.MachineName)) as HttpWebRequest;
            _pingRequest.Method = "POST";
            _pingRequest.ContentType = "application/json";

            try
            {
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(status.GetType());
                using (MemoryStream ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, status);
                    _pingRequest.ContentLength = ms.Length;
                    using (var requestStream = _pingRequest.GetRequestStream())
                    {
                        serializer.WriteObject(requestStream, status);
                        requestStream.Flush();
                        requestStream.Close();
                    }

                    ms.Position = 0;
                    using (var streamReader = new StreamReader(ms))
                    {
                        _log.Debug("{0}", streamReader.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn("Could not load agent status", ex);
                return;
            }


            _pingRequest.ContentType = "application/json";

            try
            {
                using (var response = _pingRequest.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        RegisterWithHub();
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _log.Info("Agent has not been authorised by hub");
                    }
                }
            }
            catch (WebException exception)
            {
                var response = exception.Response as HttpWebResponse;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string content = reader.ReadToEnd();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _log.Info("Agent has not been authorised by hub");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    RegisterWithHub();
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    _log.Debug("Hub had internal error");
                }
                else
                {
                    _log.Warn("Unknown web error sending status to hub", exception);
                    using (var responseStream = response.GetResponseStream())
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        _log.Warn(streamReader.ReadToEnd());
                    }
                }
            }
            catch (Exception exception)
            {
                _log.Warn("Unknown error sending status to hub", exception);
            }
        }
        /*
        private AgentStatusReport GetAgentStatus()
        {
            return new AgentStatusReport
                       {
                           packages = _cache.AvailablePackages.Select(name => new LocalPackageInformation()
                                                                                  {
                                                                                      PackageId = name,
                                                                                      InstalledVersion = _installCache.GetCurrentInstalledVersion(name) != null ? _installCache.GetCurrentInstalledVersion(name).Version.ToString() : "",
                                                                                      LatestAvailableVersion = _cache.GetLatestVersion(name) != null ? _cache.GetLatestVersion(name).Version.ToString() : "",
                                                                                      AvailableVersions = _cache.AvailablePackageVersions(name).ToList(),
                                                                                      CurrentTask = _runningTasks.Where(t => t.PackageId == name)
                                                                                          .Select(t => new InstallTaskViewModel()
                                                                                                           {
                                                                                                               Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                                                                                                               Status = Enum.GetName(typeof(TaskStatus), t.Task.Status),
                                                                                                               PackageId = t.PackageId,
                                                                                                               Version = t.Version,
                                                                                                               LastMessage = t.ProgressReports.Count > 0 ? t.ProgressReports.LastOrDefault().Message : ""
                                                                                                           }).FirstOrDefault()
                                                                                  }).ToList(),
                           currentTasks = _runningTasks.Select(t => new InstallTaskViewModel()
                                                                        {
                                                                            Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                                                                            Status = Enum.GetName(typeof(TaskStatus), t.Task.Status),
                                                                            PackageId = t.PackageId,
                                                                            Version = t.Version,
                                                                            LastMessage = t.ProgressReports.Count > 0 ? t.ProgressReports.LastOrDefault().Message : ""
                                                                        }).ToList(),
                           availableVersions = _cache.AllCachedPackages().Select(p => p.Version.ToString()).Distinct().OrderByDescending(s => s).ToList(),
                           environment = _agentSettingsManager.Settings.DeploymentEnvironment
                       };
        }*/

        private void RegisterWithHub()
        {
            HttpWebRequest _registerRequest =
                HttpWebRequest.Create(string.Format("{0}/api/agent/{1}",
                                                    _agentSettingsManager.Settings.HubAddress,
                                                    Environment.MachineName)) as HttpWebRequest;


            _registerRequest.Method = "PUT";
            _registerRequest.ContentLength = 0;

            _log.Debug("register with " + _agentSettingsManager.Settings.HubAddress);
            try
            {
                using (var response = _registerRequest.GetResponse())
                {

                }
            }
            catch (WebException webException)
            {
                var response = webException.Response as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    _log.Info("Agent with the same machine name has already been registered with hub");
                }
                else
                {
                    _log.Warn("Unknown web error registering with hub", webException);
                    using (var responseStream = response.GetResponseStream())
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        _log.Warn(streamReader.ReadToEnd());
                    }

                }
            }
            catch (Exception exception)
            {
                _log.Warn("Unknown error registering with hub", exception);
            }
        }
    }
}