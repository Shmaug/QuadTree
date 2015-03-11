using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Xna.Framework;

namespace QuadTree
{
    class NetworkHost : Network
    {
        public static void listenForConnections(int listenPort)
        {
            try
            {
                port = listenPort;
                client = new UdpClient(port);
                client.Client.ReceiveTimeout = 100;
                client.Client.SendTimeout = 100;

                networkThread = new Thread(new ThreadStart(ReceiveLoop));
                networkThread.IsBackground = true;
                networkThread.Name = "Receive";
                networkThread.Start();
            }
            catch { }
        }

        public static void ReceiveLoop()
        {
            while (client != null)
            {
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

        public static void shutdown()
        {
            if (client != null)
                client.Close();
            client = null;
        }
    }
}
