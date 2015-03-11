using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace QuadTree
{
    class NetworkClient : Network
    {
        public static void tryConnect(string host, int portNumber)
        {
            port = portNumber;

            Main.error = "connecting...";

            client = new UdpClient();
            client.Client.ReceiveTimeout = 100;
            client.Client.SendTimeout = 100;

            connectedTo = new IPEndPoint(IPAddress.Parse(host), portNumber);
            client.Connect(connectedTo);

            Main.error = "connected";

            networkThread = new Thread(new ThreadStart(ReceiveLoop));
            networkThread.IsBackground = true;
            networkThread.Name = "Receive";
            networkThread.Start();
        }

        public static void shutdown()
        {
            if (client != null)
                client.Close();
            client = null;
        }

        public static void ReceiveLoop()
        {
            while (client != null)
            {
                if (!Main.loading && Main.world == null)
                {
                    Main.error = "requesting world..." + totalTime;
                    client.Send(new byte[] { 0 }, 1);
                }
                try
                {
                    IPEndPoint ip = connectedTo;
                    int time = totalTime;
                    IAsyncResult a = client.BeginReceive(null, null);
                    a.AsyncWaitHandle.WaitOne(100);
                    try
                    {
                        byte[] buffer = client.EndReceive(a, ref ip);
                        deltaTime = totalTime - time;
                        processPacket(buffer, ip);
                    }
                    catch { }
                    if (client.Client.Connected)
                    {
                        if (sendPlyrDat)
                            sendPlayerData(ip);
                    }
                }
                catch { }
            }
        }
    }
}
