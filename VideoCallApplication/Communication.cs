/*
 * Communication.cs
 * Author: Henry Glenn
 */

using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace VideoCallApplication
{
    /// <summary>
    /// A collection of methods to standardize sending and receiving data over TCP connections.
    /// </summary>
    public static class Communication
    {
        /// <summary>
        /// A variable to store the length of a message header
        /// </summary>
        public static int HeaderSize = 8; // Long length in bytes

        /// <summary>
        /// The minimum port usable for a TCP connection.
        /// </summary>
        public static int MinPort = 1025; // 2^10 + 1

        /// <summary>
        /// The maximum port usable for a TCP connection
        /// </summary>
        public static int MaxPort = 65535; // 2^16

        /// <summary>
        /// The buffer size for receiving data.
        /// </summary>
        public static int BufferSize = 16384; // 16KB of data

        /// <summary>
        /// The maximum amount of time to wait for a connection to respond.
        /// </summary>
        public static int ConnectionTimeout = 1000;

        /// <summary>
        /// Create an enumeration to hold server connection request response bytes
        /// </summary>
        public enum ConnectionCode : byte
        {
            Accepted = 0,
            Rejected = 1,
            Unresponsive = 2
        }

        /// <summary>
        /// Starts a TCP listener on the first available port
        /// </summary>
        /// <param name="address"> The address the start the listener on. </param>
        /// <param name="port"> The port that the listener was started on. </param>
        /// <returns> A TCP listener listening on the given address and port. </returns>
        /// <exception cref="SocketException"> If no available port was found. </exception>
        public static TcpListener StartListener(IPAddress address, out int port)
        {
            for (int testPort = MinPort; testPort <= MaxPort; testPort++)
            {
                TcpListener listener = new TcpListener(address, testPort);
                try
                {
                    listener.Start();
                    port = testPort;
                    return listener;
                }
                catch (SocketException)
                {
                    listener.Stop();
                }
            }
            throw new SocketException(); // No available port was found.
        }

        /// <summary>
        /// Finds all IPv4 addresses associated with the local machine.
        /// </summary>
        /// <returns> An array of all IPV4 addresses. </returns>
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

        /// <summary>
        /// Writes the provided memory stream to the given network stream.
        /// </summary>
        /// <param name="stream"> The network stream to write to. </param>
        /// <param name="data"> The memory stream to write. </param>
        public static void WriteToNetworkStream(NetworkStream stream, MemoryStream data)
        {
            // Ensure data stream read position to the beginning.
            data.Position = 0;

            try
            {
                // Write data stream length to aid in decoding.
                byte[] lengthBytes = BitConverter.GetBytes(data.Length);
                stream.Write(lengthBytes, 0, lengthBytes.Length);

                // Write provided data to the network stream.
                data.CopyTo(stream);

                // Ensures all buffered data is sent through the network stream.
                stream.Flush();
            }
            catch (IOException)
            {
                Debug.Print("Client abruptly disconnected.");
            }
            finally
            {
                // Reset data stream read position to the beginning.
                data.Position = 0;
            }
        }

        /// <summary>
        /// Reads a single data packet from a network stream.
        /// </summary>
        /// <param name="stream"> The network stream to read from. </param>
        /// <returns> The received data memory stream. </returns>
        public static MemoryStream ReadFromNetworkStream(NetworkStream stream)
        {
            byte[] messageLengthBytes = new byte[HeaderSize];
            try
            {
                if (stream.Read(messageLengthBytes, 0, HeaderSize) == 0)
                {
                    Debug.Print("Connection was ended by peer machine");
                    return new MemoryStream(); // Connection ended by peer machine
                }
                else
                {
                    // Parse length header
                    long bytesRemaining = BitConverter.ToInt64(messageLengthBytes);

                    // Create an stream to contain received data
                    MemoryStream data = new MemoryStream();

                    byte[] buffer = new byte[BufferSize];

                    // Note: If a foreign connection is closed while in this loop it causes it to be infinite.
                    while (bytesRemaining > 0)
                    {
                        int bufferSize = (int)Math.Min(BufferSize, bytesRemaining);
                        int bytesRead = stream.Read(buffer, 0, bufferSize);
                        bytesRemaining -= bytesRead;
                        data.Write(buffer, 0, bytesRead);
                    }
                    return data;
                }
            }
            catch (IOException exc)
            {
                Debug.Print(exc.Message);
                Debug.Print("Connection was ended by local machine");
                return new MemoryStream(); // Connection was ended by local machine or forcably closed by peer
            }
        }

        /// <summary>
        /// Sends a single byte connection code to a client attempting to validate there connection.
        /// </summary>
        /// <param name="stream"> The network stream to send the connection code to. </param>
        /// <param name="code"> The connection code to send. </param>
        public static void SendConnectionCode(NetworkStream stream, ConnectionCode code)
        {
            stream.WriteByte((byte)code);
        }

        /// <summary>
        /// Waits to receive a single byte connection code to validate a server-client connection.
        /// </summary>
        /// <param name="stream"> The network stream to listen from the connection code on. </param>
        /// <returns> The connection code received from the server. </returns>
        public static ConnectionCode ReceiveConnectionCode(NetworkStream stream)
        {
            ConnectionCode code;
            stream.ReadTimeout = ConnectionTimeout;
            try
            {
                code = (ConnectionCode)stream.ReadByte();
            }
            catch (SocketException)
            {
                code = ConnectionCode.Unresponsive;
            }
            stream.ReadTimeout = -1;
            return code;
        }
    }
}
