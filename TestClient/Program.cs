using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    internal class Program
    {
        private static string _ip = "127.0.0.1";
        private static int _port = 7984;

        private static IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);

        private static async Task Main(string[] args)
        {
            Console.Write("Введите путь к папке с файлами, которые необходимо перевести: ");
            string path = Console.ReadLine();

            List<Task> tasks = new List<Task>();

            foreach (var filePath in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(filePath);
                byte[] fileBytes = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(File.ReadAllBytes(filePath)));

                tasks.Add(Task.Run(async () => await SendRequest(fileName, fileBytes)));
            };

            await Task.WhenAll(tasks);

            Console.WriteLine("Все файлы обработаны");

            Console.Read();
        }

        private static async Task SendRequest(string fileName, byte[] fileBytes)
        {
            bool isSuccessfull = false;
            string result = string.Empty;

            while (!isSuccessfull)
            {
                try
                {
                    byte[] transferData = fileBytes;

                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(ipPoint);
                    socket.Send(transferData);

                    //Responce
                    {
                        int bytesCount = 0;
                        transferData = new byte[256];

                        StringBuilder builder = new StringBuilder();

                        do
                        {
                            bytesCount = socket.Receive(transferData, transferData.Length, 0);
                            builder.Append(Encoding.Unicode.GetString(transferData, 0, bytesCount));
                        }
                        while (socket.Available > 0);

                        result = builder.ToString() == "True" ? "палиндром" : "не палиндром";

                        Console.WriteLine(builder.ToString());
                        //Console.WriteLine("ответ сервера: " + builder.ToString());
                    }

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();

                    isSuccessfull = true;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }

            Console.WriteLine($"{fileName} - {result}");
        }
    }
}
