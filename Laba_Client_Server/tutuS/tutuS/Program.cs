using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static ConcurrentDictionary<int, string> idToFileMap = new ConcurrentDictionary<int, string>();
    static int nextId = 1;

    static void Main(string[] args)
    {
        Console.WriteLine("Сервер запущен!");

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Any, 904));
        socket.Listen(5);

        while (true)
        {
            Socket client = socket.Accept();

            // Запуск нового потока для обработки запроса
            ThreadPool.QueueUserWorkItem(state =>
            {
                // Получение запроса
                byte[] buffer = new byte[1024];
                int bytesReceived = client.Receive(buffer);
                string request = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

                // Обработка запроса
                string response = "";
                string[] parts = request.Split(' ');
                switch (parts[0])
                {
                    case "GET":
                        string filename = parts[2];
                        if (parts[1] == "BY_NAME")
                        {
                            if (File.Exists(Path.Combine("server", "data", filename)))
                            {
                                response = "200 " + File.ReadAllText(Path.Combine("server", "data", filename));
                            }
                            else
                            {
                                response = "404";
                            }
                        }
                        else if (parts[1] == "BY_ID")
                        {
                            int id = Convert.ToInt32(parts[2]);
                            if (idToFileMap.TryGetValue(id, out filename))
                            {
                                response = "200 " + File.ReadAllText(Path.Combine("server", "data", filename));
                            }
                            else
                            {
                                response = "404";
                            }
                        }
                        break;
                    case "PUT":
                        filename = parts[1];
                        string content = parts[2];
                        File.WriteAllText(Path.Combine("server", "data", filename), content);
                        response = "200";
                        break;
                    case "DELETE":
                        if (parts[1] == "BY_NAME")
                        {
                            filename = parts[2];
                            if (File.Exists(Path.Combine("server", "data", filename)))
                            {
                                File.Delete(Path.Combine("server", "data", filename));
                                response = "200";
                            }
                            else
                            {
                                response = "404";
                            }
                        }
                        else if (parts[1] == "BY_ID")
                        {
                            int id = Convert.ToInt32(parts[2]);
                            if (idToFileMap.TryRemove(id, out filename))
                            {
                                File.Delete(Path.Combine("server", "data", filename));
                                response = "200";
                            }
                            else
                            {
                                response = "404";
                            }
                        }
                        break;
                }

                // Отправка ответа
                buffer = Encoding.ASCII.GetBytes(response);
                client.Send(buffer);

                // Закрытие соединения
                client.Close();
            });
        }
    }
}
