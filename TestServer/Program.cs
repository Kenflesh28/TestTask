using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    internal class Program
    {
        private async static Task Main(string[] args)
        {
            Console.Write("Введите максимальное количество одновременно обрабатываемых запросов сервером: ");

            var maxRequest = int.Parse(Console.ReadLine());

            Server.Settings settings = new Server.Settings()
            {
                ip = "127.0.0.1",
                port = 7984,
                delayForRequests = 1500,
                maximumRequests = maxRequest
            };

            Server server = new Server(settings);

            //Handlers
            {
                server.OnError += (exception) =>
                {
                    string errorMessage = $"\n{exception.Message}";

                    Console.Beep();
                    Console.WriteLine(errorMessage);
                    new Exception(errorMessage);
                };
            }

            await server.Start();

            Console.WriteLine("\nСервер завершил свою работу. Нажмите любую кнопку, чтобы продолжить...");
            Console.ReadKey();
        }

        public static void PrintSeparation() => Console.WriteLine("\n===========================\n");
    }
}
