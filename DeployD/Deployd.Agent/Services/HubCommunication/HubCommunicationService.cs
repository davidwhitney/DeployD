using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using log4net;

namespace Deployd.Agent.Services.HubCommunication
{
    public class HubCommunicationService : IWindowsService
    {
        private readonly IAgentSettings _agentSettings;
        private readonly ILog _log;
        private Timer _pingTimer = null;

        public HubCommunicationService(IAgentSettings agentSettings, ILog log)
        {
            _agentSettings = agentSettings;
            _log = log;
        }

        public void Start(string[] args)
        {
            _pingTimer = new Timer(10000);
            _pingTimer.Elapsed += PingHub;
            _pingTimer.Enabled = true;
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
                    _log.Warn("Unknown error registering with hub", webException);

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
