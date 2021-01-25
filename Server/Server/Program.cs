using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static int id = 1;
        const int port = 8888;

        static TcpListener listener;
        static public Dictionary<string, Task<byte[]>> Tasks = new Dictionary<string, Task<byte[]>>();
        static void Main(string[] args)
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);

                listener.Start();
                Console.WriteLine("Старт...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Connection connection = new Connection(client, Tasks, id++);

                    new Thread(new ThreadStart(connection.Start)).Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
}
