using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class Connection
    {
        public Dictionary<string, Task<byte[]>> Tasks;
        public int Id;
        public TcpClient client;

        public Connection(TcpClient сlient, Dictionary<string, Task<byte[]>> tasks, int id)
        {
            client = сlient;
            Tasks = tasks;
            Id = id;
            Console.WriteLine("Клиент " + Id + " подключился");
        }

        public int Jobing(string numStr)
        {
            int number;
            if (!int.TryParse(numStr, out number))
            {
                Console.WriteLine("Ошибка");
            }
            Thread.Sleep(9500);

            Tasks.Remove(numStr);

            return number * 10;
        }

        public void Send(byte[] answer, NetworkStream stream)
        {
            try
            {
                stream.Write(answer, 0, answer.Length);
            }
            catch
            {

            }
        }

        public void Start()
        {
            Task<byte[]> MyTask = null;
            NetworkStream stream = null;
            bool inJobing = false;

            try
            {
                stream = client.GetStream();

                byte[] data = new byte[64];

                while (true)
                {
                    StringBuilder constructor = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        constructor.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    string message = constructor.ToString();
                    Console.WriteLine("Клиент " + Id + " - " + message + ": " + DateTime.Now.ToLocalTime().ToString());

                    // Если за работой
                    if (inJobing == true)
                    {
                        
                        // Если в общих задачах есть задача по принятому запросу, присоединяемся!
                        if (Tasks.ContainsKey(message))
                        {
                            MyTask = Tasks[message];
                        }
                        else
                        {
                            // Говорим клиенту что мы заняты
                            Send(Encoding.Unicode.GetBytes("Ожидаем"), stream);

                            // Подготавливаем задачу для обработки этого запроса
                            var t = new Task<byte[]>(() => Encoding.Unicode.GetBytes(Jobing(message).ToString()));


                            // При завершении впереди стоящей задачи:
                            MyTask.ContinueWith(tsk =>
                            {
                                // Если в момент завершения другие клиенты не добавили задачу для этого запроса,
                                if (!Tasks.ContainsKey(message))
                                {
                                    // Добавляем её в общие задачи сервера, чтобы другие клиенты могли её видеть
                                    Tasks.Add(message, t);
                                }

                                // Ставм флаг за работой
                                inJobing = true;

                                // И запускаем подготовленную к запуску следующую задачу 
                                t.Start();

                            });

                            // Для следующего запроса, впереди стоящая задача будет последней подготовленной
                            MyTask = t;
                        }

                        // При завершении задачи убираем флаг за работой и отправляем результат этой задачи на клиента
                        MyTask.ContinueWith(task =>
                        {
                            inJobing = false;
                            var answer = MyTask.Result;
                            Send(answer, stream);
                        });

                        // Пропускаем итерацию
                        continue;
                    }

                    // Если же мы не за работой:

                    // Если в задачах сервера есть задача по этому запросу, присоединяемся и ставим флаг за работой
                    if (Tasks.ContainsKey(message))
                    {
                        MyTask = Tasks[message];
                        inJobing = true;
                    }
                    else // Если в задачах сервера не оказалось нашей, 
                    {
                        // Создаем новую задачу, запускаем её и добавляем в общие задачи, чтобы другие клиенты видели, ставим флаг за работой
                        Tasks.Add(message, Task.Factory.StartNew(() => Encoding.Unicode.GetBytes(Jobing(message).ToString())));
                        MyTask = Tasks[message];
                        inJobing = true;

                    }

                    // После всей логики:

                    // При завершении этой задачи, убираем флаг за работой, получаем результат от задачи и отправляем его на клиента
                    MyTask.ContinueWith(task => 
                    {
                        inJobing = false;
                        var answer = MyTask.Result;
                        Send(answer, stream);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Клиент " + Id + " отключился");
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }
    }
}
