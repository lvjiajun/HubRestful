using System.Collections.Generic;
using HubRestful.Model;

namespace HubRestful.OpcClientServer.Client
{
    public interface OpcuaClient
    {
        public void opcUaServerInit();

        public string WriteNode<T>(string writeNode,T writeData);

        public string ReadNode(string readNode);
        
        public void AddSubscrip(string SubscripNode);

        public ScanDataBaseServer MongoDataBaseServer();
    }
}