using System.Net.Sockets;
using System.Net;
using System.IO;
using static VideoCallApplication.Communication;
using System.Drawing.Printing;
using System.Diagnostics;
namespace VideoCallApplication
{
    public class Client
    {
        public bool IsConnected { get; private set; }

        public delegate void OnConnectResponseCallback(ConnectionCode code, Client client);
        
        public delegate void OnDisconnectCallback(Client client);

        public delegate void OnMessageReceivedCallback(MemoryStream message, Client client);

        public event OnConnectResponseCallback? OnConnectResponse;

        public event OnDisconnectCallback? OnDisconnect;

        public event OnMessageReceivedCallback? OnMessageReceived;

        public IPEndPoint? LocalEndPoint { get; private set; }

        public IPEndPoint? RemoteEndPoint { get; private set; }

        private readonly TcpClient _client;

        private readonly Thread messageListeningThread;

        public Client()
        {
            IsConnected = false;
            _client = new TcpClient();
            messageListeningThread = new Thread(ListenForMessages);
        }

        public Client(TcpClient client)
        {
            IsConnected = client.Connected;
            _client = client;
            messageListeningThread = new Thread(ListenForMessages);
            if (IsConnected)
            {
                UpdateEndPoints();
                messageListeningThread.Start();
            }
        }

        public void Connect(IPEndPoint endPoint)
        {
            RequireDisconnection();
            new Thread(() => ConnectToServer(endPoint)).Start();
        }

        public void Connect(IPAddress address, int port)
        {
            RequireDisconnection();
            new Thread(() => ConnectToServer(new IPEndPoint(address, port))).Start();
        }

        private void ConnectToServer(IPEndPoint endPoint)
        {
            ConnectionCode response;
            try
            {
                _client.Connect(endPoint);
                response = ReceiveConnectionCode(_client.GetStream());
            }
            catch (SocketException)
            {
                response = ConnectionCode.Unresponsive;
            }
            
            if (response == ConnectionCode.Accepted)
            {
                IsConnected = true;
                UpdateEndPoints();
                TriggerConnectResponseEvent(response);
                messageListeningThread.Start();
            }
            else if (response == ConnectionCode.Rejected)
            {
                _client.Close();
                _client.Dispose();
                TriggerConnectResponseEvent(response);
            }
        }

        public void Disconnect()
        {
            RequireConnection();
            _client.Close();
            Debug.Print($"Is client disconnected: {_client.Connected}");
            messageListeningThread.Join();
        }

        public void Send(MemoryStream message)
        {
            RequireConnection();
            WriteToNetworkStream(_client.GetStream(), message);
        }

        private void UpdateEndPoints()
        {
            LocalEndPoint = (IPEndPoint)_client.Client.LocalEndPoint!;
            RemoteEndPoint = (IPEndPoint)_client.Client.RemoteEndPoint!;
        }

        private void ListenForMessages()
        {
            NetworkStream networkStream = _client.GetStream();
            while (IsConnected)
            {
                using (MemoryStream memoryStream = ReadFromNetworkStream(networkStream))
                {
                    if (memoryStream.Length > 0)
                    {
                        TriggerMessageReceivedEvent(memoryStream);
                    }
                    else
                    {
                        IsConnected = false;
                    }
                }
            }

            // TODO: This if block works off a hunch. Make sure to double check logic.
            if (_client.Connected) // Make sure the disconnection handler is not called when the connection is terminated locally.
            {
                TriggerDisconnectEvent();
            }
            _client.Dispose();
        }
        
        private void RequireConnection()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Client must be connected to a server in order to excecute this operation.");
            }
        }

        private void RequireDisconnection()
        {
            if (IsConnected)
            {
                throw new InvalidOperationException("Client must not be connected to a server in order to excecute this operation.");
            }
        }
    
        private void TriggerConnectResponseEvent(ConnectionCode status)
        {
            if (OnConnectResponse != null)
            {
                // Get all attached event handlers
                Delegate[] handlers = OnConnectResponse.GetInvocationList();

                // Invoke each event handler in it's own thread
                foreach (Delegate handler in handlers)
                {
                    handler.DynamicInvoke(status, this);
                }
            }
        }
        
        private void TriggerDisconnectEvent()
        {
            if (OnDisconnect != null)
            {
                // Get all attached event handlers
                Delegate[] handlers = OnDisconnect.GetInvocationList();

                // Invoke each event handler in it's own thread
                foreach (Delegate handler in handlers)
                {
                    handler.DynamicInvoke(this);
                }
            }
        }
        
        private void TriggerMessageReceivedEvent(MemoryStream stream)
        {
            if (OnMessageReceived != null)
            {
                // Get all attached event handlers
                Delegate[] handlers = OnMessageReceived.GetInvocationList();

                // Invoke each event handler in it's own thread
                foreach (Delegate handler in handlers)
                {
                    handler.DynamicInvoke(stream, this);
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
        }
    }
}
