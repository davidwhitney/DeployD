using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Timers;
using Deployd.Core.AgentConfiguration;
using agsXMPP;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using log4net;

namespace Deployd.Core.Notifications
{
    public class JabberNotifier : INotifier, IDisposable
    {
        private ILog _logger = LogManager.GetLogger(typeof (JabberNotifier));
        private readonly IAgentSettingsManager _settingsManager;
        private XmppClientConnection _client = new XmppClientConnection();
        private bool _connecting, _connected;
        Queue<string> _messageQueue = new Queue<string>();
        System.Timers.Timer _queueTimer = new System.Timers.Timer(500);
        private string[] _recipients;

        public JabberNotifier(IAgentSettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _queueTimer.Elapsed += SendNextQueuedMessage;
            _queueTimer.Start();

            _recipients = _settingsManager.Settings.NotificationRecipients.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void SendNextQueuedMessage(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_messageQueue.Count == 0)
                return;
            if (_connected)
            {
                foreach (var recipient in _recipients)
                    _client.Send(new Message(recipient, MessageType.chat, _messageQueue.Dequeue()));
            }
        }

        private void Connect()
        {
// connect
            _settingsManager.Settings.XMPPSettings.Port = _settingsManager.Settings.XMPPSettings.Port == 0
                                                              ? 80
                                                              : _settingsManager.Settings.XMPPSettings.Port;

            _client.OnLogin += delegate
                {
                    _logger.DebugFormat("Logged in to {0}", _settingsManager.Settings.XMPPSettings.Host);
                    _connecting = false;
                    _connected = true;
                };
            _client.OnAuthError += delegate
                {
                    _logger.WarnFormat("auth error");
                    _connecting = false;
                    _connected = false;
                };
            _client.OnClose += ClientOnOnClose;
            _client.OnSocketError += ClientOnOnSocketError;
            _client.OnStreamError += ClientOnOnStreamError;

            MailAddress userAddress = null;
            try
            {
                userAddress = new MailAddress(_settingsManager.Settings.XMPPSettings.Username);
            }
            catch (FormatException)
            {
                _logger.Warn("XMPP username should be in the format of an email address");
            }

            _logger.InfoFormat("connecting to {0} as {1}", _settingsManager.Settings.XMPPSettings.Host,
                               _settingsManager.Settings.XMPPSettings.Username);
            _client.Server = userAddress.Host;
            _client.ConnectServer = _settingsManager.Settings.XMPPSettings.Host;
            _client.Username = userAddress.User;
            _client.Password = _settingsManager.Settings.XMPPSettings.Password;
            _client.Port = _settingsManager.Settings.XMPPSettings.Port;
            _connecting = true;
            _client.Open();

            while (_connecting)
            {
                System.Threading.Thread.Sleep(1000); // wait to connect
            }
        }

        ~JabberNotifier()
        {
            _logger.Debug("jabber notifier destroyed");
        }

        private void ClientOnOnClose(object sender)
        {
            _connecting = false;
            _connected = false;
            _logger.InfoFormat("connection closed by {0}", sender);
        }

        public void Notify(string message)
        {
            _messageQueue.Enqueue(message);
        }

        public bool Handles(EventType eventType)
        {
            return true;
        }

        public void OpenConnections()
        {
            Connect();
        }

        private void ClientOnOnStreamError(object sender, Element element)
        {
            _connecting = false;
            _connected = false;
            _logger.WarnFormat("Stream error: {0}", element.ToString());
        }

        private void ClientOnOnSocketError(object sender, Exception exception)
        {
            _connecting = false;
            _connected = false;
            _logger.WarnFormat("Socket error: {0}", exception.Message);
            var e = exception.InnerException;
            while (e != null)
            {
                _logger.WarnFormat("{0}", exception.Message);
                e = e.InnerException;
            }
        }

        public void Dispose()
        {
            _connected = false;
            _queueTimer.Stop();
            _client.Close();
        }
    }
}