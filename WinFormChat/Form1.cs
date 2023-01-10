using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormChat
{
    public partial class Form1 : Form
    {
        TcpListener server = null;
        TcpClient clientSocket = null;
        static int counter = 0;
        string date;

        public Dictionary<TcpClient, string> clientList = new();
        public Form1()
        {
            InitializeComponent();

            Thread t = new(InitSocket);
            t.IsBackground = true;
            t.Start();
        }
        private void InitSocket()
        {
            server = new(IPAddress.Any, 9999);
            clientSocket = default(TcpClient);
            server.Start();

            DisplayText(">> Server Started");

            while (true)
            {
                try
                {
                    counter++;
                    clientSocket = server.AcceptTcpClient();
                    DisplayText(">> Accept connection from client");

                    NetworkStream stream = clientSocket.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    string user_name = Encoding.Unicode.GetString(buffer, 0, bytes);

                    user_name = user_name.Substring(0, user_name.IndexOf("$"));
                    clientList.Add(clientSocket, user_name);
                    SendMessageAll(user_name + " 님이 입장하셨습니다.", "", false);

                    Class1 h_client = new();
                    h_client.OnReceived += new Class1.MessageDisplayHandler(OnReceived);
                    h_client.OnDisconnected += new Class1.DisconnectedHandler(h_client_OnDisconnected);
                    h_client.startClient(clientSocket, clientList);
                }
                catch(SocketException SocketError) { break; }
                catch(Exception Error) { break; }
            }
        }

        void h_client_OnDisconnected(TcpClient clientSocket)
        {
            if (clientList.ContainsKey(clientSocket))
            {
                clientList.Remove(clientSocket);
            }
        }

        private void OnReceived(string message, string user_name)
        {
            if (message.Equals("leaveChat"))
            {
                string displayMessage = "leave user : " + user_name;
                DisplayText(displayMessage);
                SendMessageAll("leaveChat", user_name, true);
            }
            else
            {
                string displatMessage = "From client : " + user_name + " : " + message;
                DisplayText(displatMessage);
                SendMessageAll(message, user_name, true);
            }
        }

        public void SendMessageAll(string message, string user_name, bool flag)
        {
            foreach(var pair in clientList)
            {
                date = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss");
                TcpClient client = pair.Key as TcpClient;
                NetworkStream stream = client.GetStream();
                byte[] buffer = null;

                if (flag)
                {
                    if (message.Equals("leaveChat"))
                        buffer = Encoding.Unicode.GetBytes(user_name + "님이 대화방을 나갔습니다.");
                    else
                        buffer = Encoding.Unicode.GetBytes("[ " + date + " ]" + user_name + " : " + message);
                }
                else
                {
                    buffer = Encoding.Unicode.GetBytes(message);
                }
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }

        private void DisplayText(string text)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    textBox1.AppendText(text + Environment.NewLine);
                }));
            }
            else
                textBox1.AppendText(text + Environment.NewLine);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}
