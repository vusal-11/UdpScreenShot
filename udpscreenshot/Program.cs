using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace udpscreenshot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int Port = 27001;
            bool flag = true;

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Port);
            listener.Bind(localEndPoint);

            Console.WriteLine($"Listening for UDP packets on port {Port}...\n");

            try
            {
                while (true)
                {
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = new byte[1024];
                    int received = listener.ReceiveFrom(data, ref remoteEndPoint);

                    Console.WriteLine($"Received UDP packet from {remoteEndPoint}");

                    if (flag)
                    {
                        doscreenshot();
                        flag = false;
                    }

                    byte[] buffer = ConvertJpgToBytes($@"..\..\..\capture.jpg");
                    int totalPackets = (int)Math.Ceiling((double)buffer.Length / 1024);
                    List<byte[]> packetList = new List<byte[]>();

                    for (int i = 0; i < totalPackets; i++)
                    {
                        int offset = i * 1024;
                        int remaining = buffer.Length - offset;
                        int packetLength = Math.Min(1024, remaining);
                        byte[] packetData = new byte[packetLength];
                        Array.Copy(buffer, offset, packetData, 0, packetLength);
                        packetList.Add(packetData);
                    }

                    foreach (byte[] packetData in packetList)
                    {
                        listener.SendTo(packetData, remoteEndPoint);
                    }

                    Console.WriteLine($"Sent byte code to {remoteEndPoint}\n");
                    flag = true;
                    continue;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred: {e}");
            }
            finally
            {
                listener.Close();
            }
        }

        static void doscreenshot()
        {
            var captureBmp = new Bitmap(1920, 1024, PixelFormat.Format32bppArgb);
            using (var captureGraphic = Graphics.FromImage(captureBmp))
            {
                captureGraphic.CopyFromScreen(0, 0, 0, 0, captureBmp.Size);
                captureBmp.Save($@"..\..\..\capture.jpg", ImageFormat.Jpeg);
            }
        }

        static byte[] ConvertJpgToBytes(string filePath)
        {
            byte[] fileBytes = null;

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    fileBytes = reader.ReadBytes((int)stream.Length);
                }
            }

            return fileBytes;
        }
    }
}


