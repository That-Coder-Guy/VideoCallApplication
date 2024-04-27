using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace VideoCallApplication
{
    public class Client
    {
        public IPAddress Address { get; }

        public int Port { get; private set; }

        public bool IsConnected { get => _client.Connected; }

        private TcpClient _client = new TcpClient();

        public Client()
        {

        }

        public void AttemptConnection(IPAddress address, int port)
        {
            try
            {
                if (!_client.ConnectAsync(address, port).Wait(1000))
                {
                    // Connection succeeded
                    Debug.Print("Connection established.");
                    // Continue with further operations using the connected client
                }
                else
                {
                    // Connection timed out
                    _client.Close();
                    Debug.Print("Connection attemped was rejected because the server is full.");
                    // Handle the timeout error as needed
                }
            }
            catch (SocketException exc)
            {
                Debug.Print(exc.ToString());
            }
        }
    }
}
