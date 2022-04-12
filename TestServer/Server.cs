using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    internal class Server
    {
        public event Action<Exception> OnError;

        private Settings _serverSettings { get; set; }
        private IPEndPoint _ipEndPoint { get; set; }
        private Socket _listeningSocket { get; set; }

        private Dictionary<Socket, Task> _requestProcessings = new Dictionary<Socket, Task>();

        public Server(Settings settings)
        {
            _serverSettings = settings;
        }

        public async Task Start()
        {
            try
            {
                _ipEndPoint = _serverSettings.GetEndPoint();

                _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _listeningSocket.Bind(_ipEndPoint);

                _listeningSocket.Listen(100);

                //Print server settings
                {
                    Program.PrintSeparation();

                    Console.WriteLine($"Стартовал сервер с настройками:\n{_serverSettings}");

                    Program.PrintSeparation();
                }

                //Print server load
                {
                    _ = Task.Run(async () =>
                    {
                        while (true)
                        {
                            UpdateUI();

                            await Task.Delay(100);
                        }
                    });
                }

                while (true)
                {
                    var newSocket = await _listeningSocket.AcceptAsync();

                    if (_requestProcessings.Count >= _serverSettings.maximumRequests)
                        newSocket.Shutdown(SocketShutdown.Both);
                    else
                        _requestProcessings.Add(newSocket, Task.Run(() => ProcessRequest(newSocket)));
                }
            }
            catch (Exception ex)
            {
                OnError(ex);

                Console.WriteLine("\nПрекращение работы...");
            }
        }

        private async void ProcessRequest(Socket handler)
        {
            int bytesCount = 0;
            byte[] loadedData = new byte[256];

            StringBuilder builder = new StringBuilder();
            try
            {
                do
                {
                    bytesCount = handler.Receive(loadedData);
                    builder.Append(Encoding.UTF8.GetString(loadedData, 0, bytesCount));
                }
                while (handler.Available > 0);

                //Console.WriteLine($"\n{builder}\n"); //вывод полученных сообщений

                await Task.Delay(_serverSettings.delayForRequests);

                string message = $"{isPalindrom(builder.ToString())}";
                loadedData = Encoding.Unicode.GetBytes(message);

                handler.Send(loadedData);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            _requestProcessings.Remove(handler);
        }

        private void UpdateUI()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Используется запросов: {_requestProcessings.Count}/{_serverSettings.maximumRequests}");
        }

        private bool isPalindrom(string word)
        {
            word = word.ToLower();

            for (int i = 0; i < word.Length / 2; i++)
                if (word[i] != word[word.Length - i - 1])
                    return false;

            return true;
        }

        internal struct Settings
        {
            #region[Server]
            public string ip;
            public ushort port;
            #endregion

            #region[Debug]
            public int delayForRequests;
            public int maximumRequests;
            #endregion

            public override string ToString()
            {
                return
                    $"IP : {ip}\n" +
                    $"Порт : {port}\n" +
                    $"Задержка для обработки запроса : {delayForRequests}\n" +
                    $"Максимальное количество обрабатываемых запросов : {maximumRequests}";
            }

            public IPEndPoint GetEndPoint() => new IPEndPoint(IPAddress.Parse(ip), port);
        }
    }
}
