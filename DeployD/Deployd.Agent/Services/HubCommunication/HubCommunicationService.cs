using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Deployd.Core.PackageCaching;
using Ninject.Extensions.Logging;
using log4net;

namespace Deployd.Agent.Services.HubCommunication
{
    public class HubCommunicationService : IWindowsService
    {
        private static bool _communicating=false;
        private readonly IAgentSettings _agentSettings;
        private readonly ILogger _log;
        private readonly ILocalPackageCache _cache;
        private readonly RunningInstallationTaskList _runningTasks;
        private readonly IInstalledPackageArchive _installCache;
        private Timer _pingTimer = null;
        private int _pingIntervalInMilliseconds = 5000;

        public HubCommunicationService(IAgentSettings agentSettings, ILogger log, ILocalPackageCache cache, RunningInstallationTaskList runningTasks, IInstalledPackageArchive installCache)
        {
            _agentSettings = agentSettings;
            _log = log;
            _cache = cache;
            _runningTasks = runningTasks;
            _installCache = installCache;
        }

        public void Start(string[] args)
        {
            _pingTimer = new Timer(_pingIntervalInMilliseconds);
            _pingTimer.Elapsed += SendStatusToHub;
            _pingTimer.Enabled = true;
        }

        private void SendStatusToHub(object sender, ElapsedEventArgs e)
        {
            if (_communicating)
                return;

            _communicating = true;
            var _pingRequest = HttpWebRequest.Create(string.Format("{0}/api/agent/{1}/status",
                _agentSettings.HubAddress,
                Environment.MachineName)) as HttpWebRequest;
            _pingRequest.Method = "POST";
            _pingRequest.ContentType = "application/json";
            AgentStatusReport status = null;
            try
            {
                status = GetAgentStatus();

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
                        //System.Diagnostics.Debug.WriteLine(streamReader.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn("Could not load agent status", ex);
                SlowPingIntervalUpToFiveMinutes();
                _communicating = false;
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
                    SetPingIntervalToDefault();
                }
            }
            catch (WebException exception)
            {
                var response = exception.Response as HttpWebResponse;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string content = reader.ReadToEnd();
                    _log.Debug(content);
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _log.Info("Agent has not been authorised by hub");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    RegisterWithHub();
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
                SlowPingIntervalUpToFiveMinutes();
            }
            catch (Exception exception)
            {
                _log.Warn("Unknown error sending status to hub", exception);
                SlowPingIntervalUpToFiveMinutes();
            }
            _communicating = false;
        }

        private void SetPingIntervalToDefault()
        {
            _pingTimer.Interval = _pingIntervalInMilliseconds;
        }

        private void SlowPingIntervalUpToFiveMinutes()
        {
            _pingTimer.Interval *= 2;
            if (_pingTimer.Interval > 5*60*1000)
                _pingTimer.Interval = 5*60*1000;
        }

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
                environment = _agentSettings.DeploymentEnvironment
            };
        }


        private void PingHub(object sender, ElapsedEventArgs e)
        {
            var _pingRequest = HttpWebRequest.Create(string.Format("{0}/api/agent/{1}/ping", 
                _agentSettings.HubAddress,
                Environment.MachineName)) as HttpWebRequest;
            _pingRequest.Method = "GET";
            _pingRequest.ContentLength = 0;
            _log.Debug("ping " + _agentSettings.HubAddress);
            try
            {
                using (var response = _pingRequest.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        RegisterWithHub();
                    } else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _log.Info("Agent has not been authorised by hub");
                    }
                }
            } catch (WebException exception)
            {
                var response = exception.Response as HttpWebResponse;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())){
                    string content = reader.ReadToEnd();
                    _log.Debug(content);
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _log.Info("Agent has not been authorised by hub");
                } else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    RegisterWithHub();
                }
                else
                {
                    _log.Warn("Unknown error pinging hub", exception);
                    using (var responseStream = response.GetResponseStream())
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        _log.Warn(streamReader.ReadToEnd());
                    }
                }
            } catch (Exception exception)
            {
                _log.Warn("Unknown error pinging hub", exception);
            }

        }

        private void RegisterWithHub()
        {
            HttpWebRequest _registerRequest =
                HttpWebRequest.Create(string.Format("{0}/api/agent/{1}", 
                _agentSettings.HubAddress,
                Environment.MachineName)) as HttpWebRequest;

            
            _registerRequest.Method = "PUT";
            _registerRequest.ContentLength = 0;

            _log.Debug("register with " + _agentSettings.HubAddress);
            try
            {
                using (var response = _registerRequest.GetResponse())
                {

                }
            }catch (WebException webException)
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
            }catch (Exception exception)
            {
                _log.Warn("Unknown error registering with hub", exception);
            }
        }

        public void Stop()
        {
            _pingTimer.Enabled = false;
            _pingTimer.Dispose();
            _pingTimer = null;
        }

        public ApplicationContext AppContext { get; set; }
    }
}
