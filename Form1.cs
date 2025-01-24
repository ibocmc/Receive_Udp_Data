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

namespace Unity_Udp
{
    public partial class Form1 : Form
    {
        private UdpClient udpClient;
        private Thread receiveThread;
        private bool isReceiving;

        public Form1()
        {
            InitializeComponent();

        }
        private void StartReceiving()
        {
                 
           udpClient = new UdpClient(12345); // Same port as Unity
           receiveThread = new Thread(ReceiveData);
           receiveThread.IsBackground = true;
           receiveThread.Start();
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

        private void ProcessReceivedData(string data)
        {
            // Trim the data to remove any leading or trailing whitespace
            data = data.Trim();

            label1.Text = data;

            // Check if the data starts with the expected prefixes
            if (data.StartsWith("X:"))
            {
                string xValue = data.Substring(2).Trim(); // Get the value after "X:"

                label3.Text = "Pitch:" + xValue;
             

                //if (float.TryParse(xValue, out float xRotation))
                //{
                //    // Update the label with the x rotation
                //    label3.Text = xRotation.ToString();
                //}
                //else
                //{
                //    label8.Text = xValue;
                //}
            }
            else if (data.StartsWith("Y:"))
            {
                string yValue = data.Substring(2).Trim(); // Get the value after "Y:"
                label4.Text = "Yaw:" + yValue;
                //if (float.TryParse(yValue, out float yRotation))
                //{
                //    // Update the label with the y rotation
                //    //lblRotationData.Text += $"\n{yRotation}";
                //    label4.Text = $"\n{yRotation}";
                //}
                //else
                //{
                //    label7.Text = $"\n{yValue}";
                //}
            }
            else if (data.StartsWith("Z:"))
            {
                string zValue = data.Substring(2).Trim(); // Get the value after "Z:"
                label5.Text ="Roll:" + zValue;

                //if (float.TryParse(zValue, out float zRotation))
                //{
                //    // Update the label with the z rotation
                //    label5.Text = $"\n{zRotation}";
                //}
                //else
                //{
                //    label6.Text = $"\n{zValue}";
                //}
            }
            else
            {
                label2.Text = data;
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
            udpClient?.Close();
            receiveThread?.Join(); // Wait for the thread to finish
            base.OnFormClosing(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartReceiving();
            isReceiving = true;
            UpdateState(isReceiving.ToString());
           
        }
    }
}
