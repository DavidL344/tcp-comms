using System.Net;
using System.Net.Sockets;
using System.Windows;

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

    private async Task ConnectAsync(IPAddress? host = null, int port = 8080)
    {
        host ??= IPAddress.Parse("127.0.0.1");
        try
        {
            await _client.ConnectAsync(host, port);
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