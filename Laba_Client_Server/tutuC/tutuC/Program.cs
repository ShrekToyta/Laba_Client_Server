using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        // Запрос действия
        Console.WriteLine("Введите действие (1 - получить файл, 2 - сохранить файл, 3 - удалить файл):");
        int action = Convert.ToInt32(Console.ReadLine());

        // Запрос имени файла
        Console.WriteLine("Введите имя файла:");
        string filename = Console.ReadLine();

        // Запрос содержимого файла (если применимо)
        string content = "";
        if (action == 2)
        {
            Console.WriteLine("Введите содержимое файла:");
            content = Console.ReadLine();
        }

        // Отправка запроса
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("127.0.0.1", 904);

        string request = "";
        switch (action)
        {
            case 1:
                Console.WriteLine("Выберите, как указать файл:");
                Console.WriteLine("1 - по имени, 2 - по ID");
                int choice = Convert.ToInt32(Console.ReadLine());
                if (choice == 1)
                {
                    request = "GET BY_NAME " + filename;
                }
                else
                {
                    Console.WriteLine("Введите ID файла:");
                    int id = Convert.ToInt32(Console.ReadLine());
                    request = "GET BY_ID " + id;
                }
                break;
            case 2:
                request = "PUT " + filename + " " + content;
                break;
            case 3:
                Console.WriteLine("Выберите, как указать файл:");
                Console.WriteLine("1 - по имени, 2 - по ID");
                choice = Convert.ToInt32(Console.ReadLine());
                if (choice == 1)
                {
                    request = "DELETE BY_NAME " + filename;
                }
                else
                {
                    Console.WriteLine("Введите ID файла:");
                    int id = Convert.ToInt32(Console.ReadLine());
                    request = "DELETE BY_ID " + id;
                }
                break;
        }

        byte[] buffer = Encoding.ASCII.GetBytes(request);
        socket.Send(buffer);

        // Получение ответа
        buffer = new byte[1024];
        int bytesReceived = socket.Receive(buffer);
        string response = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

        // Обработка ответа
        if (action == 1)
        {
            // Сохранение файла
            if (response.StartsWith("200"))
            {
                string fileContent = response.Split(' ')[2];
                Console.WriteLine("Файл был получен!");
                Console.WriteLine("Введите имя для сохранения файла:");
                string savedFilename = Console.ReadLine();
                File.WriteAllText(Path.Combine("client", "data", savedFilename), fileContent);
                Console.WriteLine("Файл сохранен на диске!");
            }
            else
            {
                Console.WriteLine("Файл не найден!");
            }
        }
        else if (action == 2)
        {
            // Получение ID файла
            if (response.StartsWith("200"))
            {
                int id = Convert.ToInt32(response.Split(' ')[2]);
                Console.WriteLine("Файл сохранен! ID = " + id);
            }
            else
            {
                Console.WriteLine("Не удалось сохранить файл!");
            }
        }
        else if (action == 3)
        {
            // Удаление файла
            if (response.StartsWith("200"))
            {
                Console.WriteLine("Файл был удален!");
            }
            else
            {
                Console.WriteLine("Файл не найден!");
            }
        }

        // Отключение от сервера
        socket.Close();
    }
}
