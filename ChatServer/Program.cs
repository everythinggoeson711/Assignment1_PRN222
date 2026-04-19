using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    class Program
    {
        private static List<ClientHandler> clients = new List<ClientHandler>();
        private static object lockObj = new object();

        static void Main(string[] args)
        {
            Console.WriteLine("Starting server on port 8888...");

            TcpListener server = new TcpListener(IPAddress.Any, 8888);
            server.Start();

            Console.WriteLine("Server started. Waiting for clients...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");

                ClientHandler handler = new ClientHandler(client);
                lock (lockObj)
                {
                    clients.Add(handler);
                }

                Task.Run(() => handler.Handle());
            }
        }

        public static void BroadcastMessage(string message, ClientHandler sender)
        {
            lock (lockObj)
            {
                foreach (var client in clients)
                {
                    if (client.IsConnected)
                    {
                        client.SendMessage(message);
                    }
                }
            }
        }

        public static void RemoveClient(ClientHandler client)
        {
            lock (lockObj)
            {
                clients.Remove(client);
            }
        }
    }

    class ClientHandler
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        public bool IsConnected { get; private set; }
        private string username;

        public ClientHandler(TcpClient tcpClient)
        {
            client = tcpClient;
            NetworkStream stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            IsConnected = true;
        }

        public void Handle()
        {
            try
            {
                username = reader.ReadLine();
                if (string.IsNullOrEmpty(username))
                {
                    username = "Anonymous";
                }

                Console.WriteLine($"User '{username}' has joined the chat");
                Program.BroadcastMessage($"[System] {username} has joined the chat", this);

                while (IsConnected)
                {
                    string message = reader.ReadLine();
                    if (string.IsNullOrEmpty(message))
                    {
                        break;
                    }

                    string formattedMessage = $"[{username}]: {message}";
                    Console.WriteLine(formattedMessage);
                    Program.BroadcastMessage(formattedMessage, this);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                if (IsConnected)
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                Disconnect();
            }
        }

        private void Disconnect()
        {
            if (IsConnected)
            {
                IsConnected = false;
                Console.WriteLine($"User '{username}' has left the chat");
                Program.BroadcastMessage($"[System] {username} has left the chat", this);
                Program.RemoveClient(this);

                reader?.Close();
                writer?.Close();
                client?.Close();
            }
        }
    }
}
