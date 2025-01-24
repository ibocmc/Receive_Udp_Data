using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Unity_Udp
{
    public partial class Tcp_Page : Form
    {
        private TcpListener server;
        private Thread listenerThread;
        private bool isReceiving;

        private SerialPort serialPort;
        private System.Windows.Forms.Timer timer;
        private int dataIndex = 0; 

        string x_send, y_send, z_send;

        public Tcp_Page()
        {
            InitializeComponent();

            LoadAvailablePorts();
            InitializeTimer();
        }


        private void InitializeTimer()
        {
            timer = new System.Windows.Forms.Timer
            {
                Interval = 50
            };
            timer.Tick += Timer_Tick; // Subscribe to the Tick event
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {

                string dataToSend = x_send + " " + y_send + " " + z_send;
                serialPort.WriteLine(dataToSend); // Send the current data


                //// Send data in a loop
                //string[] dataToSend = { label3.Text, label4.Text, label5.Text };
                //if (dataIndex < dataToSend.Length)
                //{
                //    serialPort.WriteLine(dataToSend[dataIndex]); // Send the current data
                //    dataIndex++; // Move to the next data
                //}
                //else
                //{
                //    dataIndex = 0; // Reset index to start over
                //}
            }
            else
            {
                MessageBox.Show("Serial port is not open.");
                timer.Stop(); // Stop the timer if the port is not open
            }
        }

        private void LoadAvailablePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(ports);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (serialPort != null)
            {
                serialPort.Close();
            }

            serialPort = new SerialPort(comboBox1.SelectedItem.ToString(), 115200);
            serialPort.Open();


            if (serialPort.IsOpen)
            {
                button4.BackColor = Color.Green;
                button3.Enabled = true;
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                timer.Start();
            }
        }

       

        private void StartServer()
        {
            listenerThread = new Thread(new ThreadStart(ListenForClients))
            {
                IsBackground = true

            };

            listenerThread.Start();    

            label2.Text = "Server started and listening on port 12345...";
            
        }

        private void ListenForClients()
        {
            server = new TcpListener(IPAddress.Any, 12345);
            server.Start();
        
            while (isReceiving)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Client connected.");
                    Thread clientThread = new Thread(new ParameterizedThreadStart(ClientCommunication))
                    {
                        IsBackground = true
                    };
                    clientThread.Start(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private void ClientCommunication(object clientObj)
        {
            TcpClient tcpClient = (TcpClient)clientObj;
            NetworkStream clientStream = tcpClient.GetStream();
            byte[] message = new byte[4096];
            int bytesRead;

            while (isReceiving)
            {
                try
                {
                    bytesRead = clientStream.Read(message, 0, 4096);
                    if (bytesRead == 0) break; // Client disconnected

                    string rotationData = Encoding.UTF8.GetString(message, 0, bytesRead);

                    Invoke(new Action(() =>
                    {
                        // Process the received data
                        ProcessReceivedData(rotationData);// string e çevrilmiş olan datayı işleme sokuyoruz.
                    }));

                    
                    
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                
            }

            tcpClient.Close();
        }

        private void ProcessReceivedData(string data)// 
        {

            try
            {
               
                string[] parts = data.Split(' '); // Split by space

                float x = 0, y = 0, z = 0;

                foreach (var part in parts)
                {
                    if (part.StartsWith("X:"))
                    {
                        x = float.Parse(part.Substring(2)); // Get the value after "X:"
                        label3.Text = "Pitch:" + x.ToString();
                        x_send = x.ToString();
                    }
                    else if (part.StartsWith("Y:"))
                    {
                        y = float.Parse(part.Substring(2)); // Get the value after "Y:"
                        label4.Text = "Yaw:" + y.ToString();
                        y_send = y.ToString();
                    }
                    else if (part.StartsWith("Z:"))
                    {
                        z = float.Parse(part.Substring(2)); // Get the value after "Z:"
                        label5.Text = "Roll:" + z.ToString();
                        z_send = z.ToString();
                    }
                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing rotation data: {ex.Message}");
            }
        }
        
        private void UpdateState(string state)
        {

            if (label2.InvokeRequired)
            {
                label2.Invoke(new Action<string>(UpdateState), state);
            }
            else
            {
                label2.Text = state;
            }
        }
       
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources
            isReceiving = false;
            server.Stop();   
            base.OnFormClosing(e);

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartServer();
            isReceiving = true;
            // UpdateState(isReceiving.ToString());
        }

        private void button2_Click_1(object sender, EventArgs e)
        {

            isReceiving = false;

            server.Stop();
        }

       
    }
}
