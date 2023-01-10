using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinFormChat
{
    class Class1
    {
        TcpClient clientSocket = null;
        public Dictionary<TcpClient, string> clientList = null;

        public void startClient(TcpClient clientSocket, Dictionary<TcpClient, string> clientList)
        {
            this.clientSocket = clientSocket;
            this.clientList = clientList;

            Thread t_handler = new(doChat);
        }

        public delegate void MessageDisplayHandler(string message, string user_name);
        public event MessageDisplayHandler OnReceived;

        public delegate void DisconnectedHandler(TcpClient clientSocket);
        public event DisconnectedHandler OnDisconnected;

        private void doChat()
        {
            NetworkStream stream = null;
            try
            {
                byte[] buffer = new byte[1024];
                string msg = string.Empty;
                int bytes = 0;
                int MessageCount = 0;

                while (true)
                {
                    MessageCount++;
                    stream = clientSocket.GetStream();
                    bytes = stream.Read(buffer, 0, buffer.Length);
                    msg = Encoding.Unicode.GetString(buffer, 0, bytes);
                    msg = msg.Substring(0, msg.IndexOf("$"));

                    if(OnReceived != null)
                    {
                        OnReceived(msg, clientList[clientSocket].ToString());
                    }
                }
            }
            catch(SocketException SocketError)
            {
                Trace.WriteLine(string.Format($"doChat - SocketException : {SocketError.Message}"));

                if(clientSocket != null)
                {
                    if(OnDisconnected != null)
                    {
                        OnDisconnected(clientSocket);
                        clientSocket.Close();
                        stream.Close();
                    }
                }
            }
            catch(Exception Error)
            {
                Trace.WriteLine(string.Format($"doChat - Exception : {Error.Message}"));

                if(clientSocket != null)
                {
                    if(OnDisconnected != null)
                    {
                        OnDisconnected(clientSocket);

                        clientSocket.Close();
                        stream.Close();
                    }
                }
            }
        }
    }
}
