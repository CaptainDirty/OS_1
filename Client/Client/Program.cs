using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {

        static TcpClient client = null;
        const int port = 8888;
        const string address = "127.0.0.1";

        static NetworkStream networkStream;

        static void Main(string[] args)
        {

            try
            {
                client = new TcpClient(address, port);
                networkStream = client.GetStream();

                Console.Write("Имя: Клиент");
                Console.WriteLine();

                new Thread(new ThreadStart(GotoServer)).Start();

                while (true)
                {
                    var information = new byte[64];

                    StringBuilder Strbuilder = new StringBuilder();

                    int bytes = 0;
                    do
                    {
                        bytes = networkStream.Read(information, 0, information.Length);
                        Strbuilder.Append(Encoding.Unicode.GetString(information, 0, bytes));
                    }
                    while (networkStream.DataAvailable);

                    var message = Strbuilder.ToString();
                    Console.WriteLine("Сервер: {0}", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        static void GotoServer()
        {

            while (true)
            {
                try
                {
                    int[] numbers = { 7, 13, 25, 34, 46, 78, 83 };
                    int random = new Random().Next(0, 6);

                    int randomNumber = numbers[random];

                    Console.WriteLine("Клиент: {0}", randomNumber);
                    string message = randomNumber.ToString();
                    byte[] data = Encoding.Unicode.GetBytes(message);

                    networkStream.Write(data, 0, data.Length);


                    Thread.Sleep(new Random().Next(2900, 9000));
                }
                catch
                {
                    Console.WriteLine("Ошибка");
                    break;
                }
            }
        }




    }
}
