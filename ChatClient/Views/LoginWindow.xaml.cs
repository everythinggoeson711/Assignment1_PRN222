using System.Windows;
using ChatClient.Services;

namespace ChatClient.Views
{
    public partial class LoginWindow : Window
    {
        private ChatService chatService;

        public LoginWindow()
        {
            InitializeComponent();
            chatService = new ChatService();
        }

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServerIp.Text))
            {
                MessageBox.Show("Please enter server IP address.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtPort.Text, out int port))
            {
                MessageBox.Show("Please enter a valid port number.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string username = txtUsername.Text.Trim();

            btnConnect.IsEnabled = false;
            btnConnect.Content = "Connecting...";

            bool success = await chatService.ConnectAsync(txtServerIp.Text, port, username);

            if (success)
            {
                // Open Chat Window
                var mainWindow = new MainWindow(chatService, username);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                btnConnect.IsEnabled = true;
                btnConnect.Content = "START MESSAGING";
                MessageBox.Show("Failed to connect to server.", "Connection Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
