using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HubRestful.Configure;
using HubRestful.Model;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace HubRestful.OpcClientServer
{
    public class OpcClient
    {
        private const int ReconnectPeriod = 10;
        private Session _session;
        private SessionReconnectHandler _reconnectHandler;
        private string _endpointUrl;//
        private int _clientRunTime;  //
        private bool _autoAccept = false;//
        private ExitCode _exitCode;
        private IOpcSetting _opcSetting;

        /// <summary>
        /// OPCClient 构造函数
        /// </summary>
        /// <param name="opcSetting"></param>
        public OpcClient(IOpcSetting opcSetting)
        {
            _opcSetting = opcSetting;
            _endpointUrl = opcSetting.endpointUrl;
            _clientRunTime = opcSetting.stopTimeout;
        }
        
        public async Task Start()
        {
            try
            {
                OpcClientServer().Wait();
            }
            catch (Exception ex)
            {
                Utils.Trace("ServiceResultException:" + ex.Message);
                Console.WriteLine("Exception: {0}", ex.Message);
                return;
            }

            ManualResetEvent quitEvent = new ManualResetEvent(false);
            try
            {
                Console.CancelKeyPress += (sender, eArgs) =>
                {
                    quitEvent.Set();
                    eArgs.Cancel = true;
                };
            }
            catch
            {
                
            }

            // wait for timeout or Ctrl-C
            quitEvent.WaitOne(_clientRunTime);

            // return error conditions
            if (_session.KeepAliveStopped)
            {
                _exitCode = ExitCode.ErrorNoKeepAlive;
                return;
            }

            _exitCode = ExitCode.Ok;
        }
        
        public ExitCode ExitCode { get => _exitCode; }

        private async Task OpcClientServer()
        {
            Console.WriteLine("1 - Create an Application Configuration.");
            _exitCode = ExitCode.ErrorCreateApplication;

            ApplicationInstance application = new ApplicationInstance
            {
                ApplicationName = "UA Core Sample Client",
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = Utils.IsRunningOnMono() ? "Opc.Ua.MonoSampleClient" : "Opc.Ua.SampleClient"
            };

            // load the application configuration.
            ApplicationConfiguration config = await application.LoadApplicationConfiguration(false);

            // check the application certificate.
            bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);
            if (!haveAppCertificate)
            {
                throw new Exception("Application instance certificate invalid!");
            }

            if (haveAppCertificate)
            {
                config.ApplicationUri = Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);
                if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    _autoAccept = true;
                }
                config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
            }
            else
            {
                Console.WriteLine("    WARN: missing application certificate, using unsecure connection.");
            }

            Console.WriteLine("2 - Discover endpoints of {0}.", _endpointUrl);
            _exitCode = ExitCode.ErrorDiscoverEndpoints;
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(_endpointUrl, haveAppCertificate, 15000);
            Console.WriteLine("    Selected endpoint uses: {0}",
                selectedEndpoint.SecurityPolicyUri.Substring(selectedEndpoint.SecurityPolicyUri.LastIndexOf('#') + 1));

            Console.WriteLine("3 - Create a session with OPC UA server.");
            _exitCode = ExitCode.ErrorCreateSession;
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
            _session = await Session.Create(config, endpoint, false, "OPC UA Console Client", 60000, new UserIdentity(new AnonymousIdentityToken()), null);

            // register keep alive handler
            _session.KeepAlive += Client_KeepAlive;

            Console.WriteLine("4 - Browse the OPC UA server namespace.");
            _exitCode = ExitCode.ErrorBrowseNamespace;
            ReferenceDescriptionCollection references;
            Byte[] continuationPoint;

            references = _session.FetchReferences(ObjectIds.ObjectsFolder);

            _session.Browse(
                null,
                null,
                ObjectIds.ObjectsFolder,
                0u,
                BrowseDirection.Forward,
                ReferenceTypeIds.HierarchicalReferences,
                true,
                (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                out continuationPoint,
                out references);

            Console.WriteLine(" DisplayName, BrowseName, NodeClass");
            foreach (var rd in references)
            {
                Console.WriteLine(" {0}, {1}, {2}", rd.DisplayName, rd.BrowseName, rd.NodeClass);
                ReferenceDescriptionCollection nextRefs;
                byte[] nextCp;
                _session.Browse(
                    null,
                    null,
                    ExpandedNodeId.ToNodeId(rd.NodeId, _session.NamespaceUris),
                    0u,
                    BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences,
                    true,
                    (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                    out nextCp,
                    out nextRefs);

                foreach (var nextRd in nextRefs)
                {
                    Console.WriteLine("   + {0}, {1}, {2}", nextRd.DisplayName, nextRd.BrowseName, nextRd.NodeClass);
                }
            }

            Console.WriteLine("5 - Create a subscription with publishing interval of 1 second.");
            _exitCode = ExitCode.ErrorCreateSubscription;
            var subscription = new Subscription(_session.DefaultSubscription) { PublishingInterval = 1000 };

            Console.WriteLine("6 - Add a list of items (server current time and status) to the subscription.");
            _exitCode = ExitCode.ErrorMonitoredItem;
            var list = new List<MonitoredItem> {
                new MonitoredItem(subscription.DefaultItem)
                {
                    DisplayName = "ServerStatusCurrentTime", StartNodeId = "i="+Variables.Server_ServerStatus_CurrentTime.ToString()
                }
            };
            list.ForEach(i => i.Notification += OnNotification);
            subscription.AddItems(list);

            Console.WriteLine("7 - Add the subscription to the session.");
            _exitCode = ExitCode.ErrorAddSubscription;
            _session.AddSubscription(subscription);
            subscription.Create();

            Console.WriteLine("8 - Running...Press Ctrl-C to exit...");
            _exitCode = ExitCode.ErrorRunning;
        }
        
        private void Client_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                Console.WriteLine("{0} {1}/{2}", e.Status, sender.OutstandingRequestCount, sender.DefunctRequestCount);

                if (_reconnectHandler == null)
                {
                    Console.WriteLine("--- RECONNECTING ---");
                    _reconnectHandler = new SessionReconnectHandler();
                    _reconnectHandler.BeginReconnect(sender, ReconnectPeriod * 1000, Client_ReconnectComplete);
                }
            }
        }
        
        private void Client_ReconnectComplete(object sender, EventArgs e)
        {
            // ignore callbacks from discarded objects.
            if (!Object.ReferenceEquals(sender, _reconnectHandler))
            {
                return;
            }

            _session = _reconnectHandler.Session;
            _reconnectHandler.Dispose();
            _reconnectHandler = null;

            Console.WriteLine("--- RECONNECTED ---");
        }
        
        private static void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0}: {1}, {2}, {3}", item.DisplayName, value.Value, value.SourceTimestamp, value.StatusCode);
            }
        }
        
        private void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = _autoAccept;
                if (_autoAccept)
                {
                    Console.WriteLine("Accepted Certificate: {0}", e.Certificate.Subject);
                }
                else
                {
                    Console.WriteLine("Rejected Certificate: {0}", e.Certificate.Subject);
                }
            }
        }
    }
}