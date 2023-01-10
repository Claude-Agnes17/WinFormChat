using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormClient
{
    public partial class Form1 : Form
    {
        TcpClient clientSocket = new();
        NetworkStream stream = default(NetworkStream);
        string message = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                clientSocket.Connect(textBox1.Text, 9999);
                stream = clientSocket.GetStream();
            }
            catch (Exception e2)
            {
                MessageBox.Show("서버가 실행중이 아닙니다.", "연결 실패!");
                Application.Exit();
            }
            message = "채팅 서버에 연결 되었습니다.";
            DisplayText(message);
            byte[] buffer = Encoding.Unicode.GetBytes(textBox3.Text + "$");
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
            Thread t_handler = new Thread(GetMessage);
            t_handler.IsBackground = true;
            t_handler.Start();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            textBox4.Focus();
            byte[] buffer = Encoding.Unicode.GetBytes(textBox4.Text + "$");
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
            textBox4.Text = "";
        }
        private void GetMessage()
        {
            while (true)
            {
                stream = clientSocket.GetStream();
                int buffersize = clientSocket.ReceiveBufferSize;
                byte[] buffer = new byte[buffersize];
                int bytes = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.Unicode.GetString(buffer, 0, bytes);
                DisplayText(message);
            }
        }
        private void DisplayText(string message)
        {
            if (textBox3.InvokeRequired)
            {
                textBox3.BeginInvoke(new MethodInvoker(delegate
                {
                    textBox3.Text = message + Environment.NewLine;
                }));
            }
            else
                textBox3.AppendText(message + Environment.NewLine);
        }

        private void textBox4_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button2_Click(this, e);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            byte[] buffer = Encoding.Unicode.GetBytes("leaveChat" + "$");
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
            Application.ExitThread();
            Environment.Exit(0);
        }
    }
}
