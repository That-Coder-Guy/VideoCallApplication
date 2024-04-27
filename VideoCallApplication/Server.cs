using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Themes;
using System.IO;

namespace VideoCallApplication
{
    public class Server
    {
        public IPAddress Address { get; } = IPAddress.Any;

        public int Port
        {
            get
            {
                IPEndPoint endPoint = (IPEndPoint)_listener.LocalEndpoint;
                return endPoint.Port;
            }
        }

        public bool IsListening { get; private set; }

        public List<TcpClient> Clients = new List<TcpClient>();

        public int MaxClients { get; }

        public bool HasMaxClients { get => Clients.Count == MaxClients; }

        public int ClientCount { get => Clients.Count; }

        public delegate void OnClientConnectCallback(TcpClient client);

        public delegate void OnClientDisconnectCallback(EndPoint endpoint);

        public delegate void OnMessageReceivedCallback(TcpClient client, MemoryStream message);

        public event OnClientConnectCallback? OnClientConnect;

        public event OnClientDisconnectCallback? OnClientDisconnect;

        public event OnMessageReceivedCallback? OnMessageReceived;

        private TcpListener _listener;

        public Server(int maxClients)
        {
            MaxClients = maxClients;
            _listener = new TcpListener(Address, 0);
        }

        public void Listen()
        {
            //  Find an available port and start listening
            _listener = Communication.StartListener(Address);
            IsListening = true;

            // Start a thread to accept connections
            new Thread(ListenForClientConnections).Start();
        }

        public void Deafen()
        {
            IsListening = false;
            _listener.Stop();
        }

        public void Close()
        {
            if (IsListening)
            {
                Deafen();
            }

            foreach (TcpClient client in Clients)
            {
                Clients.Remove(client);
                client.Close();
            }
        }

        private void ListenForClientConnections()
        {
            while (IsListening)
            {
                if (_listener.Pending() && !HasMaxClients)
                {
                    // Accept the new client
                    TcpClient client = _listener.AcceptTcpClient();
                    Debug.Print($"Client connected: {client.Client.RemoteEndPoint}");

                    // Add new client to client list
                    Clients.Add(client);

                    // Call the OnClientConnect event handlers
                    if (OnClientConnect != null)
                    {
                        // Get all attached event handlers
                        Delegate[] handlers = OnClientConnect.GetInvocationList();

                        // Invoke each event handler in it's own thread
                        foreach (Delegate handler in handlers)
                        {
                            handler.DynamicInvoke(client);
                        }
                    }
                    new Thread(() => WaitForMessages(client)).Start();
                }
            }
        }

        private void WaitForMessages(TcpClient client)
        {
            EndPoint clientEndpoint = client.Client.RemoteEndPoint!;
            using (NetworkStream networkStream = client.GetStream())
            {
                bool isConnected = true;
                while (isConnected)
                {
                    using (MemoryStream memoryStream = Communication.ReadFromNetworkStream(networkStream))
                    {
                        if (memoryStream.Length > 0)
                        {
                            if (OnMessageReceived != null)
                            {
                                // Get all attached event handlers
                                Delegate[] handlers = OnMessageReceived.GetInvocationList();

                                // Invoke each event handler in it's own thread
                                foreach (Delegate handler in handlers)
                                {
                                    handler.DynamicInvoke(client, memoryStream);
                                    memoryStream.Seek(0, SeekOrigin.Begin);
                                }
                            }
                        }
                        else
                        {
                            isConnected = false;
                        }
                    }
                }
            }

            if (OnClientDisconnect != null)
            {
                // Get all attached event handlers
                Delegate[] handlers = OnClientDisconnect.GetInvocationList();

                // Remove the client from the client list
                Clients.Remove(client);

                // Invoke each event handler in it's own thread
                foreach (Delegate handler in handlers)
                {
                    handler.DynamicInvoke(clientEndpoint);
                }
            }
        }
    }
}
