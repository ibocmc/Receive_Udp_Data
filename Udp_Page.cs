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
    public partial class Udp_Page : Form
    {
        private UdpClient udpClient;
        private Thread receiveThread;
        private bool isReceiving;

        private SerialPort serialPort;
        private System.Windows.Forms.Timer timer;
        private int dataIndex = 0;

        string roll_send, pitch_send, yaw_send,heave_send,surge_send,sway_send;
       

        public Udp_Page()
        {
            InitializeComponent();
            LoadAvailablePorts();
            InitializeTimer();

        }

        // karta gönderme işlmleri başlangıç
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
               
                // string dataToSend =  x_send + " " + y_send + " " + z_send ;
                //serialPort.WriteLine(dataToSend); // Send the current data

                string dataToSend = pitch_send + " " + yaw_send + " " + roll_send + " " 
                    + sway_send + " " + surge_send + " " + heave_send + " " ;
                byte[] byteData = Encoding.UTF8.GetBytes(dataToSend);
                serialPort.Write(byteData, 0, byteData.Length); // Send the byte array

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

        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                timer.Start();
            }
        }

        private void button4_Click(object sender, EventArgs e)
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

       

        private void StartReceiving() 
        {
                 
           udpClient = new UdpClient(12345); 
           receiveThread = new Thread(ReceiveData); 
           receiveThread.IsBackground = true;

        }

        private void ReceiveData()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 12345);
           
            while (isReceiving)
            {
                try
                {
                    // Receive data from the sender
                    byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);
                    string receivedData = Encoding.UTF8.GetString(receivedBytes);

                    // Update the Label on the UI thread
                    Invoke(new Action(() =>
                    {
                        // Process the received data
                        ProcessReceivedData(receivedData);
                    }));
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., socket closed)
                    MessageBox.Show("Error receiving data: " + ex.Message);
                }
            }
        }

        private void Udp_Page_Load(object sender, EventArgs e)
        {

        }

        private void ProcessReceivedData(string data)
        {
            try
            {
                
                string[] parts = data.Split(' '); // Split by space

                float x,y,z,a,b,c;

                foreach (var part in parts)
                {
                    if (part.StartsWith("X:"))
                    {
                        x = float.Parse(part.Substring(2)); // Get the value after "X:"
                        label3.Text ="Pitch:" + x.ToString();
                        pitch_send = x.ToString();
                    }
                    else if (part.StartsWith("Y:"))
                    {
                        y = float.Parse(part.Substring(2)); // Get the value after "Y:"
                        label4.Text = "Yaw:" + y.ToString();
                        yaw_send = y.ToString();
                    }
                    else if (part.StartsWith("Z:"))
                    {
                        z = float.Parse(part.Substring(2)); // Get the value after "Z:"
                        label5.Text = "Roll:" + z.ToString();
                        roll_send = z.ToString();
                    }
                    //--//
                    else if (part.StartsWith("A:"))
                    {
                        a = float.Parse(part.Substring(2)); // Get the value after "Z:"
                        label12.Text = "Sway:" + a.ToString();
                        sway_send = a.ToString();
                    }

                    else if (part.StartsWith("B:"))
                    {
                        b = float.Parse(part.Substring(2)); // Get the value after "Z:"
                        label11.Text = "Surge:" + b.ToString();
                        surge_send = b.ToString();
                    }

                    else if (part.StartsWith("C:"))
                    {
                        c = float.Parse(part.Substring(2)); // Get the value after "Z:"
                        label10.Text = "Heave:" + c.ToString();
                        heave_send = c.ToString();
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
             
        private void button1_Click(object sender, EventArgs e)
        {
            StartReceiving();
            isReceiving = true;
            UpdateState(isReceiving.ToString());
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            udpClient?.Close();
            isReceiving = false; 
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources
           
            udpClient?.Close();
            receiveThread?.Join();
            isReceiving = false;// Wait for the thread to finish
            base.OnFormClosing(e);

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }
       
        
    }
}
