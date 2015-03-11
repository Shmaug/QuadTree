using System;
using System.Threading;

namespace QuadTree
{
#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
            Thread backThread = new Thread(new ThreadStart(delegate()
            {
                using (Main game = new Main())
                {
                    game.Run();
                }
            }));
            backThread.Start();
            backThread.Join();

            QuadTree.Main.inGame = false;
            NetworkHost.shutdown();
            NetworkClient.shutdown();
        }
    }
#endif
}

