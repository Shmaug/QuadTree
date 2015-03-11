using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;

namespace QuadTree
{
    class Network
    {
        public static UdpClient client;
        public static Thread networkThread;
        public static IPEndPoint connectedTo;
        public static int port;
        public static bool sendPlyrDat = false;
        public static int deltaTime;
        public static int totalTime;

        public static void processPacket(byte[] buffer, IPEndPoint ip)
        {
            byte type = buffer[0];
            switch (type)
            {
                case 0: // world request
                    connectedTo = ip;
                    client.Connect(ip);
                    sendWorld(ip);
                    Main.world.players[1] = new Player(Main.world, Vector3.Zero, false);
                    break;
                case 1: // player
                    readPlayerData(buffer);
                    break;
                case 2: // world
                    readWorld(buffer);
                    Main.world.players[1] = new Player(Main.world, Vector3.Zero, false);
                    break;
            }
        }

        public static void sendPlayerData(IPEndPoint ip)
        {
            List<byte> buffer = new List<byte>();
            buffer.Add((byte)1);
            buffer.AddRange(BitConverter.GetBytes(Main.world.players[0].position.X));
            buffer.AddRange(BitConverter.GetBytes(Main.world.players[0].position.Y));
            buffer.AddRange(BitConverter.GetBytes(Main.world.players[0].position.Z));
            buffer.AddRange(BitConverter.GetBytes(Main.world.players[0].headRot.X));
            buffer.AddRange(BitConverter.GetBytes(Main.world.players[0].headRot.Y));
            buffer.AddRange(BitConverter.GetBytes(Main.world.players[0].headRot.Z));
            buffer.AddRange(BitConverter.GetBytes((Main.world.players[0].weapon == null) ? -1 : Main.world.players[0].weapon.type));
            buffer.AddRange(BitConverter.GetBytes(Main.world.players[0].legRot));
            if (!client.Client.Connected)
                client.Send(buffer.ToArray(), buffer.Count, ip);
            else
                client.Send(buffer.ToArray(), buffer.Count);
            Main.error = "player data sent ";
            sendPlyrDat = false;
        }

        public static void readPlayerData(byte[] buffer)
        {
            float px = BitConverter.ToSingle(buffer, 1);
            float py = BitConverter.ToSingle(buffer, 5);
            float pz = BitConverter.ToSingle(buffer, 9);
            Vector3 p = new Vector3(px, py, pz);

            float rx = BitConverter.ToSingle(buffer, 13);
            float ry = BitConverter.ToSingle(buffer, 17);
            float rz = BitConverter.ToSingle(buffer, 21);
            Vector3 r = new Vector3(rx, ry, rz);

            int t = BitConverter.ToInt32(buffer, 25);

            float lr = BitConverter.ToSingle(buffer, 29);

            if (Main.world != null)
                if (Main.world.players[1] != null)
                {
                    Main.world.players[1].goalPos = p;
                    Main.world.players[1].headRot = r;
                    Main.world.players[1].lerpTime = deltaTime;
                    Main.world.players[1].totalLerpTime = deltaTime;
                    Main.world.players[1].legRot = lr;

                    if (Main.world.players[1].weapon != null && t != -1)
                    {
                        if (Main.world.players[1].weapon.type != t)
                            Main.world.players[1].weapon = new Weapon(Main.world.players[1].position, Main.world, t, Main.world.players[1]);
                    }
                    else if (Main.world.players[1].weapon != null && t == -1)
                        Main.world.players[1].weapon = null;
                    else if (Main.world.players[1].weapon == null && t != -1)
                        Main.world.players[1].weapon = new Weapon(Main.world.players[1].position, Main.world, t, Main.world.players[1]);
                }
            sendPlyrDat = true;
        }

        public static void sendWorld(IPEndPoint ip)
        {
            List<byte> buffer = new List<byte>();
            buffer.Add((byte)2);
            buffer.AddRange(BitConverter.GetBytes(Main.world.size));
            buffer.AddRange(BitConverter.GetBytes(Main.world.noise.seed));

            if (!client.Client.Connected)
                client.Send(buffer.ToArray(), buffer.Count, ip);
            else
                client.Send(buffer.ToArray(), buffer.Count);
            Main.error = "world sent " + totalTime;
        }

        public static void readWorld(byte[] buffer)
        {
            if (Main.loading || Main.inGame)
                return;
            int size = BitConverter.ToInt32(buffer, 1);
            int seed = BitConverter.ToInt32(buffer, 4);
            Main.world = new World(size, seed, false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(Main.world.load), 16);
            Main.loading = true;
            Main.error = "world read";
            sendPlyrDat = true;
        }
    }
}
