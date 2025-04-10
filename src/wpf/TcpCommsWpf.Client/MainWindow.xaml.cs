using System.Net;
using System.Net.Sockets;
using System.Windows;
using TcpComms.Shared;

namespace TcpCommsWpf.Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly TcpClient _client;
    
    public MainWindow()
    {
        InitializeComponent();
        _client = new TcpClient();
    }

    private async Task ConnectAsync(IPAddress? host = null, int port = 8080,
        CancellationToken cancellationToken = default)
    {
        host ??= IPAddress.Parse("127.0.0.1");
        IProgress<string> progress = new Progress<string>(message =>
        {
            // TODO: Notify the UI thread of any changes
        });
        
        try
        {
            await _client.ConnectAsync(host, port, cancellationToken);
            
            var chat = new Chat(Chat.Side.Client)
                { Client = _client, Progress = progress, CancellationToken = cancellationToken };
            chat.Show();
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error has occured while connecting to the server: {e.Message}");
            MessageBox.Show($"An error has occured while connecting to the server: {e.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Login(object sender, RoutedEventArgs e)
    {
        if (!IPAddress.TryParse(Ip.Text.Trim(), out var host))
            throw new ArgumentException($"Invalid IP address ({Ip.Text.Trim()})!", nameof(Ip.Text));
        
        if (!int.TryParse(Port.Text.Trim(), out var port) || port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
            throw new ArgumentException($"Invalid port {Port.Text.Trim()}!", nameof(Port.Text));
        
        ConnectAsync(host, port).ConfigureAwait(false);
    }
}