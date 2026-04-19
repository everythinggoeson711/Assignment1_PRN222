using System.Net.Sockets;
using System.IO;
using System.Text;

namespace ChatClient.Services
{
    public class ChatService
    {
        private TcpClient? client;
        private StreamReader? reader;
        private StreamWriter? writer;
        private bool isConnected;
        private Task? receiveTask;

        public event Action<string>? MessageReceived;
        public event Action? Disconnected;
        public event Action<string>? ConnectionError;

        public bool IsConnected => isConnected && client?.Connected == true;

        public async Task<bool> ConnectAsync(string serverIp, int port, string username)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(serverIp, port);

                NetworkStream stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                // Send username as first message
                await writer.WriteLineAsync(username);

                isConnected = true;

                // Start receiving messages in background
                receiveTask = Task.Run(ReceiveMessages);

                return true;
            }
            catch (Exception ex)
            {
                ConnectionError?.Invoke($"Connection failed: {ex.Message}");
                return false;
            }
        }

        private async Task ReceiveMessages()
        {
            try
            {
                while (isConnected && reader != null)
                {
                    string? message = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(message))
                    {
                        break;
                    }

                    MessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                if (isConnected)
                {
                    ConnectionError?.Invoke($"Receive error: {ex.Message}");
                }
            }
            finally
            {
                Disconnect();
            }
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                if (writer != null && isConnected)
                {
                    await writer.WriteLineAsync(message);
                }
            }
            catch (Exception ex)
            {
                ConnectionError?.Invoke($"Send error: {ex.Message}");
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (!isConnected) return;

            isConnected = false;

            try
            {
                reader?.Close();
                writer?.Close();
                client?.Close();
            }
            catch { }

            Disconnected?.Invoke();
        }
    }
}
