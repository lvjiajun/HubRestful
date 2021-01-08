using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using HubRestful.Entity;
using HubRestful.Model;
using MongoDB.Driver;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace HubRestful.OpcClientServer.Client
{
    public class OpcuaClientService:OpcuaClient
    {
        private static volatile int ntCount = 0;
        
        private string url = "opc.tcp://10.128.141.13:4840";

        private List<string> folders { get; set; }

        private List<string> nodes { get; set; }
        
        public Session _session { get; set; }
        
        private string user { get; set; }

        private string pass { get; set; }

        public readonly ScanDataBaseServer scanDataBaseServer;

        
        public ScanDataBaseServer MongoDataBaseServer()
        {
            return scanDataBaseServer;
        }
        
        public OpcuaClientService(ScanDataBaseServer mongoDataBaseServer)
        {
            nodes = new List<string>();
            folders = new List<string>();
            this.scanDataBaseServer = mongoDataBaseServer;
            Thread thread = new Thread(opcUaServerInit);
            thread.Start();
        }
     
        
        private static void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            Interlocked.Increment(ref ntCount);
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0}: {1}, {2}", item.DisplayName, value.Value, value.StatusCode);
                byte[] bytes = (byte[]) value.Value;
                Console.WriteLine(bytes[3]);
            }
            var evts = item.LastValue as EventFieldList;
            if (evts != null)
            {
                var fields = evts.EventFields;
                foreach (var field in fields)
                {
                    Console.WriteLine("事件信息:" + field.Value.ToString());
                }
            }
        }

        private static void OnEventNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            var evts = item.LastValue as EventFieldList;
            if (evts != null)
            {
                var fields = evts.EventFields;
                foreach (var field in fields)
                {
                    Console.WriteLine("事件信息:" + field.Value.ToString());
                }
            }
        }
        
        public void opcUaServerInit()
        {
            string storePath = AppContext.BaseDirectory;
            var config = new ApplicationConfiguration()
            {
                ApplicationName = "Axiu-Opcua",
                ApplicationUri = Utils.Format(@"urn:{0}:Axiu-Opcua", System.Net.Dns.GetHostName()),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = Path.Combine(storePath, @"OPC Foundation/CertificateStores/MachineDefault"), SubjectName = Utils.Format(@"CN={0}, DC={1}", "Axiu-Opcua", System.Net.Dns.GetHostName()) },
                    TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = Path.Combine(storePath, @"OPC Foundation/CertificateStores/UA Certificate Authorities") },
                    TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = Path.Combine(storePath, @"OPC Foundation/CertificateStores/UA Applications") },
                    RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = Path.Combine(storePath, @"OPC Foundation/CertificateStores/RejectedCertificates") },
                    AutoAcceptUntrustedCertificates = true,
                    AddAppCertToTrustedStore = true
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                TraceConfiguration = new TraceConfiguration()
            };
            config.Validate(ApplicationType.Client).GetAwaiter().GetResult();
            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
            }
            var application = new ApplicationInstance
            {
                ApplicationName = "Axiu-Opcua",
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = config
            };
            application.CheckApplicationInstanceCertificate(false, 2048).GetAwaiter().GetResult();
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(url, useSecurity: true, int.MaxValue);
            Console.WriteLine("配置已准备完毕,即将打开链接会话...");
            _session = Session.Create(config, new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(config)),
                false, "", int.MaxValue, null, null).GetAwaiter().GetResult();
            

         //   ReadNodeList("ns=3;s=\"上位通讯\"");
         //   AddSubscrip(null);
            
            Console.WriteLine("完成链接会话...");
            SubscribeToDataChanges();
        }

        public string WriteNode<T>(string writeNode,T writeData)
        {
            #region 写入内容
            WriteValueCollection writeValues = new WriteValueCollection()
            {
                new WriteValue()
               {
                    NodeId=writeNode,
                    AttributeId=Attributes.Value,
                    Value=new DataValue(){ Value=writeData }
                }
            };
            _session.Write(
                null, 
                writeValues, 
                out StatusCodeCollection wresults, 
                out DiagnosticInfoCollection wdiagnosticInfos);
            foreach (var item in wresults)
            {
                Console.WriteLine("写入结果:" + item.ToString());
            }

            return wresults[0].ToString();

            #endregion
        }

        public string ReadNode(string readNode)
        {
            #region 读单个节点
            //读单个节点
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection
            {
                new ReadValueId( )
                {
                    NodeId = readNode,
                    AttributeId = Attributes.Value
                }
            };
            // read the current value
            _session.Read(
                null, 
                0, 
                TimestampsToReturn.Neither, 
                nodesToRead, 
                out DataValueCollection results, 
                out DiagnosticInfoCollection diagnosticInfos);
            
            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);
            
            return results[0].ToString();
            #endregion
        }
        
        public void AddSubscrip(string subscripNode)
        {
            #region 订阅测点
            Console.WriteLine("即将开始订阅固定节点消息...");
            var submeas = new Subscription(_session.DefaultSubscription) { PublishingInterval = 1000 };
            List<MonitoredItem> measlist = new List<MonitoredItem>();
            int index = 0, jcount = 0;
            foreach (var item in nodes)
            {
                index++; jcount++;
                MonitoredItem monitor = new MonitoredItem(submeas.DefaultItem) { DisplayName = "测点:" + item, StartNodeId = item };
                monitor.Notification += OnNotification;
                measlist.Add(monitor);
                if (index > 5999)
                {
                    submeas.AddItems(measlist);
                    _session.AddSubscription(submeas);
                    submeas.Create();
                    index = 0;
                    submeas = new Subscription(_session.DefaultSubscription) { PublishingInterval = 1000 };
                    measlist.Clear();
                }
            }
            if (measlist.Count > 0)
            {
                submeas.AddItems(measlist);
                _session.AddSubscription(submeas);
                submeas.Create();
            }
            Console.WriteLine("订阅节点数:" + jcount);


            var subscription = new Subscription(_session.DefaultSubscription) { PublishingInterval = 1000 };
            List<MonitoredItem> list = new List<MonitoredItem>();
            foreach (var item in nodes.Take(5000))
            {
                MonitoredItem monitor = new MonitoredItem(subscription.DefaultItem) { DisplayName = "测点:" + item, StartNodeId = item };
                list.Add(monitor);
            }
            Console.WriteLine("node == {0}",nodes.Count);
            list.ForEach(i => i.Notification += OnNotification);
            subscription.AddItems(list);
            _session.AddSubscription(subscription);
            subscription.Create();
            #endregion
        }

   


        public void SubscribeToDataChanges()
        {
            if (_session == null || _session.Connected == false)
            {
                Console.WriteLine("Session not connected!");
                return;
            }

            try
            {
                // Create a subscription for receiving data change notifications

                // Define Subscription parameters
                Subscription subscription = new Subscription(_session.DefaultSubscription);

                subscription.DisplayName = "Console ReferenceClient Subscription";
                subscription.PublishingEnabled = true;
                subscription.PublishingInterval = 1000;

                _session.AddSubscription(subscription);

                // Create the subscription on Server side
                subscription.Create();
           //     Console.WriteLine("New Subscription created with SubscriptionId = {0}.", subscription.Id);

                // Create MonitoredItems for data changes

                MonitoredItem boolMonitoredItem = new MonitoredItem(subscription.DefaultItem);
                // Int32 Node - Objects\CTT\Scalar\Simulation\Int32
                boolMonitoredItem.StartNodeId = new NodeId("ns=3;s=\"上位通讯\".\"写入\".\"9#\".\"请求绑定\"");
                boolMonitoredItem.AttributeId = Attributes.Value;
                boolMonitoredItem.DisplayName = "request";
                boolMonitoredItem.SamplingInterval = 1000;
                boolMonitoredItem.Notification += OnMonitoredRequest;

                subscription.AddItem(boolMonitoredItem);
                
                MonitoredItem floatMonitoredItem = new MonitoredItem(subscription.DefaultItem);
                // Float Node - Objects\CTT\Scalar\Simulation\Float
                floatMonitoredItem.StartNodeId = new NodeId("ns=3;s=\"上位通讯\".\"写入\".\"15#\".\"请求匹配\"");
                floatMonitoredItem.AttributeId = Attributes.Value;
                floatMonitoredItem.DisplayName = "equals";
                floatMonitoredItem.SamplingInterval = 1000;
                floatMonitoredItem.Notification += OnMonitoredEquals;

                subscription.AddItem(floatMonitoredItem);

                MonitoredItem stringMonitoredItem = new MonitoredItem(subscription.DefaultItem);
                // String Node - Objects\CTT\Scalar\Simulation\String
                stringMonitoredItem.StartNodeId = new NodeId("ns=3;s=\"上位通讯\".\"写入\".\"15#\".\"应急开关\"");
                stringMonitoredItem.AttributeId = Attributes.Value;
                stringMonitoredItem.DisplayName = "stop";
                stringMonitoredItem.SamplingInterval = 1000;
                stringMonitoredItem.Notification += OnMonitoredStop;

                subscription.AddItem(stringMonitoredItem);

                // Create the monitored items on Server side
                subscription.ApplyChanges();
        //        Console.WriteLine("MonitoredItems created for SubscriptionId = {0}.", subscription.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Subscribe error: {0}", ex.Message);
            }
        }
        /// <summary>
        /// Handle DataChange notifications from Server
        /// </summary>
        private void OnMonitoredStop(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                // Log MonitoredItem Notification event
                MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
    //            Console.WriteLine("Notification Received for Variable \"{0}\" and Value = {1}.", monitoredItem.DisplayName, notification.Value);
                if ((bool) notification.Value.Value)
                {
                    string scan_code = ReadNode("ns=3;s=\"上位通讯\".\"写入\".\"15#\".\"物流篮条码\"");
                    Console.WriteLine(scan_code);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnMonitoredItemNotification error: {0}", ex.Message);
            }
        }
        /// <summary>
        /// Handle DataChange notifications from Server
        /// </summary>
        private void OnMonitoredEquals(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                // Log MonitoredItem Notification event
                MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
          //      Console.WriteLine("Notification Received for Variable \"{0}\" and Value = {1}.", monitoredItem.DisplayName, notification.Value);
                if ((bool) notification.Value.Value)
                {
                    string scan_code = ReadNode("ns=3;s=\"上位通讯\".\"写入\".\"15#\".\"物流篮条码\"");
                    scanDataBaseServer.Create(new ScanEntity()
                    {
                        scanTime = DateTime.Now,
                        scanData = scan_code
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnMonitoredItemNotification error: {0}", ex.Message);
            }
        }
        /// <summary>
        /// Handle DataChange notifications from Server
        /// </summary>
        private void OnMonitoredRequest(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                // Log MonitoredItem Notification event
                MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
            //    Console.WriteLine("Notification Received for Variable \"{0}\" and Value = {1}.", monitoredItem.DisplayName, notification.Value);
                if ((bool)notification.Value.Value)
                {
                    string scan_code = ReadNode("ns=3;s=\"上位通讯\".\"写入\".\"9#\".\"物流篮条码\"");
                    scanDataBaseServer.Create(new ScanEntity()
                    {
                        scanTime = DateTime.Now,
                        scanData = scan_code
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnMonitoredItemNotification error: {0}", ex.Message);
            }
        }
    }
    
    
}
