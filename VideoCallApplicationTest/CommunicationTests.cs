using Microsoft.VisualStudio.TestTools.UnitTesting;
using VideoCallApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VideoCallApplication.Communication;
using System.Net.Sockets;
using System.Net;

namespace VideoCallApplication.Tests
{
    [TestClass()]
    public class CommunicationTests
    {
        [TestMethod()]
        [Timeout(1000)]
        public void SingleConnectionTest()
        {
            // Create server
            Server server = new Server(IPAddress.Loopback, 1);
            server.Listen();
            Assert.AreEqual(0, server.Clients.Count);

            // Create client
            Client client = new Client();
            ConnectionCode? connectionStatus = null;
            bool clientConnected = false;

            // Attach event handlers
            client.OnConnectResponse += (ConnectionCode code, Client client) =>
            {
                connectionStatus = code;
            };

            server.OnClientConnect += (Client client) =>
            {
                clientConnected = true;
            };

            // Connect client to server
            client.Connect(IPAddress.Loopback, server.Port);

            // Wait for the client connection to be recognized by both the client and the server
            while (connectionStatus == null || clientConnected == false) { }

            Assert.AreEqual(ConnectionCode.Accepted, connectionStatus);
            Assert.AreEqual(1, server.Clients.Count);
            server.Deafen();
            server.Close();
        }

        [TestMethod()]
        [Timeout(1000)]
        public void RejectConnectionTest()
        {
            // Create server
            Server server = new Server(IPAddress.Loopback, 1);
            server.Listen();
            Assert.AreEqual(0, server.Clients.Count);

            // Create first client
            Client client1 = new Client();
            ConnectionCode? connectionStatus1 = null;
            bool client1Connected = false;

            // Attach event handlers
            client1.OnConnectResponse += (ConnectionCode code, Client client) =>
            {
                connectionStatus1 = code;
            };

            server.OnClientConnect += (Client client) =>
            {
                client1Connected = true;
            };

            // Connect client to server
            client1.Connect(IPAddress.Loopback, server.Port);

            // Wait for the client connection to be recognized by both the client and the server
            while (connectionStatus1 == null || client1Connected) { }

            Assert.AreEqual(ConnectionCode.Accepted, connectionStatus1);

            // Create second client
            Client client2 = new Client();
            ConnectionCode? connectionStatus2 = null;
            bool client2Connected = false;

            // Attach event handlers
            client2.OnConnectResponse += (ConnectionCode code, Client client) =>
            {
                connectionStatus2 = code;
            };

            server.OnClientConnect += (Client client) =>
            {
                client2Connected = true;
            };

            // Connect client to server
            client2.Connect(IPAddress.Loopback, server.Port);

            // Wait for the client connection to be recognized by both the client and the server
            while (connectionStatus2 == null || client2Connected) { }

            Assert.AreEqual(ConnectionCode.Rejected, connectionStatus2);
            Assert.AreEqual(1, server.Clients.Count);

            server.Deafen();
            server.Close();
        }
    }
}