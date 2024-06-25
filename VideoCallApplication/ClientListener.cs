/*
 * ClientListener.cs
 * Author: Henry Glenn
 */

using System.Net;
using System.Net.Sockets;
using static VideoCallApplication.Communication;

namespace VideoCallApplication
{
    public class ClientListener
    {
        public IPAddress Address { get; }

        public int Port { get; private set; } = -1;

        public bool IsListening { get => _listener != null; }

        private TcpListener? _listener;

        public ClientListener(IPAddress address)
        {
            Address = address;
        }

        public void Listen()
        {
            if (_listener != null)
            {
                throw new InvalidOperationException("Already listening");
            }
            _listener = StartListener(Address, out int port);
            Port = port;
        }

        public void Deafen()
        {
            if (_listener == null)
            {
                throw new InvalidOperationException("Listening was never started.");
            }
            Port = -1;
            _listener.Stop();
            _listener.Dispose();
            _listener = null;
        }
        public bool Pending()
        {
            if (_listener == null)
            {
                throw new InvalidOperationException("Cannot stop listening before starting.");
            }
            return _listener.Pending();
        }

        public Client AcceptPending()
        {
            if (_listener == null)
            {
                throw new InvalidOperationException("Cannot stop listening before starting.");
            }

            if (!_listener.Pending())
            {
                throw new ArgumentOutOfRangeException("No pending connection was found");
            }
            TcpClient client = _listener.AcceptTcpClient();
            SendConnectionCode(client.GetStream(), ConnectionCode.Accepted);
            return new Client(client);
        }

        public IPEndPoint RejectPending()
        {
            if (_listener == null)
            {
                throw new InvalidOperationException("Cannot stop listening before starting.");
            }

            if (!_listener.Pending())
            {
                throw new ArgumentOutOfRangeException("No pending connection was found");
            }

            IPEndPoint rejectedConnection;
            using (TcpClient client = _listener.AcceptTcpClient())
            {
                rejectedConnection = (IPEndPoint)client.Client.RemoteEndPoint!;
                SendConnectionCode(client.GetStream(), ConnectionCode.Rejected);
                client.Close();
            }
            return rejectedConnection;
        }
    }
}
