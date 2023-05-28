using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace udpclient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task task = new Task(() =>
            {
                //for testing
                byte[] testbuffer = { 2, 3, 4 };
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                try
                {
                    while (true)
                    {
                        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27001);
                        client.SendTo(testbuffer, remoteEndPoint);

                        byte[] buffer = new byte[1024];
                        MemoryStream receivedData = new MemoryStream();

                        while (true)
                        {
                            int bytesReceived = client.Receive(buffer, buffer.Length, SocketFlags.None);
                            receivedData.Write(buffer, 0, bytesReceived);
                            if (bytesReceived < buffer.Length)
                            {
                                break;
                            }
                        }

                        byte[] data = receivedData.ToArray();
                        Image myImage = ConvertBytesToJpg(data);
                        pictureBox1.Image = myImage;
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error!!! " + ex);
                }
                finally
                {
                    client.Close();
                }
            });
            task.Start();
        }

        public Image ConvertBytesToJpg(byte[] bytes)
        {
            Image image = null;

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                image = Image.FromStream(stream);
            }

            return image;
        }
    }
}