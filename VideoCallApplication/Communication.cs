using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VideoCallApplication
{
    public static class Communication
    {
        public static int HeaderSize = 8; // Long length in bytes
        public static int BufferSize = 8192; // 13^10
        public static int MinPort = 1025; // 2^10 + 1
        public static int MaxPort = 65535; // 2^16

        public static TcpListener StartListener(IPAddress address)
        {
            for (int port = MinPort; port <= MaxPort; port++)
            {
                TcpListener listener = new TcpListener(address, port);
                try
                {
                    listener.Start();
                    return listener;
                }
                catch (SocketException)
                {
                    listener.Stop();
                }
            }
            throw new Exception("No available port found.");
        }

        public static IPAddress[] GetIPv4Addresses()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> addresses = new List<IPAddress>();
            foreach (IPAddress ip in host.AddressList)
            {
                if (!ip.IsIPv6LinkLocal)
                {
                    addresses.Add(ip);
                }
            }
            return addresses.ToArray();
        }

        public static void WriteToNetworkStream(NetworkStream stream, MemoryStream message)
        {
            try
            {
                // Write length header
                stream.Write(BitConverter.GetBytes(message.Length));

                byte[] buffer = new byte[BufferSize];
                long bytesRemaining = message.Length;
                while (bytesRemaining > 0)
                {
                    int bufferSize = (int)Math.Min(BufferSize, bytesRemaining);
                    message.CopyTo(stream, bufferSize);
                    bytesRemaining -= bufferSize;
                }
            }
            catch (IOException)
            {
                Debug.Print("Client abruptly disconnected.");
            }
            message.Seek(0, SeekOrigin.Begin);
        }

        public static MemoryStream ReadFromNetworkStream(NetworkStream stream)
        {
            byte[] messageLengthBytes = new byte[HeaderSize];
            try
            {
                if (stream.Read(messageLengthBytes, 0, HeaderSize) == 0)
                {
                    return new MemoryStream();
                }
                else
                {
                    // Parse length header
                    long bytesRemaining = BitConverter.ToInt64(messageLengthBytes);

                    // Create an stream to contain received data
                    MemoryStream messageBytes = new MemoryStream();

                    byte[] buffer = new byte[BufferSize];
                    while (bytesRemaining > 0)
                    {
                        int bufferSize = (int)Math.Min(BufferSize, bytesRemaining);
                        bytesRemaining -= stream.Read(buffer, 0, bufferSize);
                        messageBytes.Write(buffer, 0, bufferSize);
                    }
                    return messageBytes;
                }
            }
            catch (IOException)
            {
                Debug.Print("Client abruptly disconnected.");
                return new MemoryStream();
            }
        }
    }
}
