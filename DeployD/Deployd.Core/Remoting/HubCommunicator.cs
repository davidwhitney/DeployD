using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
            if (string.IsNullOrWhiteSpace(_agentSettingsManager.Settings.HubAddress))
                return;


            HttpWebRequest _pingRequest = null;
            try
            {
                _pingRequest = HttpWebRequest.Create(string.Format("{0}/api/agent/{1}/status",
                                                                   _agentSettingsManager.Settings.HubAddress,
                                                                   Environment.MachineName)) as HttpWebRequest;
            } catch (Exception ex)
            {
                _log.Warn("{0} doesn't appear to be a valid address for the DeployD hub", _agentSettingsManager.Settings.HubAddress);
                return;
            }
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