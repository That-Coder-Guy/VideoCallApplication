/*
 * Server.cs
 * Author: Henry Glenn
 */

using System.Diagnostics;
using System.Net;

namespace VideoCallApplication
{
    public class Server
    {
        public IPAddress Address => _listener.Address;

        public int Port => _listener.Port;

        public List<Client> Clients = new List<Client>();

        public bool IsListening {  get; private set; }

        public int MaxClients { get; }

        public bool HasMaxClients { get => Clients.Count == MaxClients; }

        public delegate void OnClientConnectCallback(Client client);

        public event OnClientConnectCallback? OnClientConnect;

        public event Client.OnDisconnectCallback? OnClientDisconnect;

        public event Client.OnMessageReceivedCallback? OnMessageReceived;

        private ClientListener _listener;

        private Thread _listeningThread;

        public Server(int maxClients) : this(IPAddress.Any, maxClients) { }

        public Server(IPAddress address, int maxClients)
        {
            MaxClients = maxClients;
            _listener = new ClientListener(address);
            _listeningThread = new Thread(ListenForConnections);
            _listeningThread.Name = "Connection Listener";
        }

        public void Listen()
        {
            _listener.Listen();
            _listeningThread.Start();
            IsListening = true;
            Debug.Print("Server started listening.");
        }

        public void Deafen()
        {
            IsListening = false;
            _listeningThread.Join();
            _listener.Deafen();
            Debug.Print("Server stopped listening.");
        }

        public void Close()
        {
            if (_listener.IsListening)
            {
                throw new InvalidOperationException("Must stop listening for connection before losing the server.");
            }

            while (Clients.Count > 0)
            {
                Client client = Clients.First();
                Clients.Remove(client);
                client.Disconnect();
            }
        }

        private void RemoveClient(Client client)
        {
            Clients.Remove(client);
        }

        private void ListenForConnections()
        {
            while (IsListening)
            {
                if (_listener.Pending())
                {
                    if (!HasMaxClients)
                    {
                        Client client = _listener.AcceptPending();
                        client.OnDisconnect += RemoveClient + OnClientDisconnect;
                        client.OnMessageReceived += OnMessageReceived;
                        Clients.Add(client);

                        TriggerClientConnectEvent(client);

                        Debug.Print($"Client accepted: {client.RemoteEndPoint}");
                    }
                    else
                    {
                        IPEndPoint rejectedConnection = _listener.RejectPending();

                        Debug.Print($"Client rejected: {rejectedConnection}");
                    }
                }
            }
        }

        private void TriggerClientConnectEvent(Client client)
        {
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
        }
    }
}
