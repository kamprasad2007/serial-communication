using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace Serial
{
    public partial class Form1 : Form
    {
        private Thread processThread;
        private Thread sendingThread;
        public Form1()
        {
            InitializeComponent();
            getAvailablePorts();
        }
        void getAvailablePorts()
        {
            String[] ports = SerialPort.GetPortNames();
            cmdPort.Items.AddRange(ports);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    prbStatus.Value = 0;
                    btnConnect.Text = "Connect";
                    txtMessage.Enabled = false;
                    txtDelay.Enabled = false;
                    txtMessage.Enabled = false;
                    processThread.Abort();
                    sendingThread.Abort();

                    txtMessage.Clear();
                    txtOutput.Clear();
                }
                else
                {
                    if (cmdPort.Text == "" || cmbRate.Text == "")
                    {
                        txtOutput.Text = "Please select port settings";
                    }
                    else
                    {
                        serialPort.PortName = cmdPort.Text;
                        serialPort.BaudRate = Convert.ToInt32(cmbRate.Text);
                        serialPort.Open();
                        txtMessage.Enabled = true;
                        txtDelay.Enabled = true;
                        txtMessage.Enabled = true;
                        btnConnect.Text = "Disconnect";
                        processThread = new Thread(() =>
                        {
                            while (true)
                            {
                                if (serialPort.IsOpen)
                                {
                                    try
                                    {
                                        string value = serialPort.ReadLine();
                                        if (value != null)
                                        {
                                            this.Invoke((Action)delegate ()
                                            {
                                                txtOutput.AppendText(value);
                                            });
                                        }
                                    }
                                    catch (TimeoutException)
                                    {
                                        txtOutput.AppendText("Timeout Exception");
                                    }
                                }
                                Thread.Sleep(1500);
                            }
                        });
                        processThread.Start();
                    }
                }
               
            }
            catch(UnauthorizedAccessException)
            {
                txtOutput.Text = "Unauthorized Access";

            }
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyData == (Keys.Control | Keys.Enter)))
            {
                int lines = txtMessage.Lines.Length;
                int second = Int32.Parse(txtDelay.Text);
                prbStatus.Maximum = lines;

                sendingThread = new Thread(() =>
                {
                    for (int i = 0; i < lines; i++)
                    {
                        serialPort.WriteLine(txtMessage.Lines[i]);
                        this.Invoke((Action)delegate ()
                        {
                            prbStatus.Value = i+1;
                        });
                        Thread.Sleep(second * 1000);
                    }

                    this.Invoke((Action)delegate ()
                    {
                        prbStatus.Value = 0;
                        txtMessage.Text = "";
                        txtMessage.Enabled = true;
                    });
                  
                });
                sendingThread.Start();
                txtMessage.Enabled = false;
            }
        }
    }
}
