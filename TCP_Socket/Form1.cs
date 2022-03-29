using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace TCP_Socket
{
    public partial class Form1 : Form
    {

        private Socket serverSocket;
        private Socket clientSocket;
        private byte[] buffer;
        //private bool connected;
        private int localPort = 2000;

        public Form1()
        {
            InitializeComponent();

            toolStripStatusLabel2.Text = "";
            toolStripStatusLabel3.Text = "";
            toolStripStatusLabel4.Text = "";
            timer1.Enabled = true;

            StartServer();

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            toolStripStatusLabel1.Text = $"Local IP: {Convert.ToString(ipHostInfo.AddressList.FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork))}:{localPort} ";
        }


        private static void ShowErrorDialog(string message)
        {
            MessageBox.Show(message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private void StartServer()
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, localPort));
                serverSocket.Listen(10);
                serverSocket.BeginAccept(AcceptCallback, null);
                
            }

            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }
        }

        private void AcceptCallback(IAsyncResult AR)
        {

            try
            {

                clientSocket = serverSocket.EndAccept(AR);
                buffer = new byte[clientSocket.ReceiveBufferSize];

                // Listen for client data
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);

                Invoke((Action)delegate
                {
                    richTextBox1.AppendText("Cliend Connected " + clientSocket.RemoteEndPoint.ToString() + "\n");
                    
                });


            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }

        }

        private void ButonLedOn(object sender, EventArgs e)
        {

            try
            {
                // Send a message to the newly connected client.
                var sendData = Encoding.ASCII.GetBytes("led 1\n");
                clientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, SendCallback, null);

                Invoke((Action)delegate
                {
                    toolStripStatusLabel4.Text = "Send " + sendData.Length.ToString() + " bytes";
                });

            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }




        }

        private void SendCallback(IAsyncResult AR)
        {
            try
            {
                clientSocket.EndSend(AR);
            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }
        }


        private void ReceiveCallback(IAsyncResult AR)
        {

            try
            {
                int received = clientSocket.EndReceive(AR);

                if (received == 0)
                {
                    return;
                }


                //Byte[] data = new Byte[buffer.Length];

                //Array.Copy(buffer, data, buffer.Length);

                Invoke((Action)delegate
                {
                    richTextBox1.AppendText(System.Text.Encoding.ASCII.GetString(buffer, 0, buffer.Length));
                    toolStripStatusLabel4.Text = "Received " + buffer.Length.ToString() + " bytes";
                });


                Array.Clear(buffer, 0, buffer.Length);

                // Start receiving data again.
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);


            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }


        }


        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Send a message to the newly connected client.
                // led 0<LF>
                var sendData = Encoding.ASCII.GetBytes("led 0\n");
                clientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, SendCallback, null);


            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {


            if (clientSocket != null)
            {
                clientSocket.Close();
                clientSocket.Dispose();
            }
                
            
        }


        // je server spusteny , je client pripojeny ?
        private bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            if (serverSocket != null && timer1.Enabled)
            {
                if (IsConnected(serverSocket))
                {
                    toolStripStatusLabel2.Text = "Server run ";
                    toolStripStatusLabel2.BackColor = Color.GreenYellow;
                }
                else
                {
                    toolStripStatusLabel2.Text = "Server stopped ";
                    toolStripStatusLabel2.BackColor = Color.LightGray;
                }
            }

            

            if (clientSocket != null && timer1.Enabled)
            {
                if (IsConnected(clientSocket))
                {
                    toolStripStatusLabel3.Text = "Client connected " + clientSocket.RemoteEndPoint.ToString();
                    toolStripStatusLabel3.BackColor = Color.GreenYellow;
                }
                else
                {
                    toolStripStatusLabel3.Text = "Client disconnected ";
                    toolStripStatusLabel3.BackColor = Color.LightGray;

                    serverSocket.BeginAccept(AcceptCallback, null);
                }
            }
                
        }
    }
}
