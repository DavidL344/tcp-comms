using System.Windows;

namespace TcpCommsWpf.Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Login(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!int.TryParse(Port.Text.Trim(), out var port))
                throw new ArgumentException($"The port {port} must be a valid integer!", nameof(port));

            var client = new Client(Ip.Text.Trim(), port);
            client.ConnectAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error has occured while connecting to the server: {ex.Message}");
            MessageBox.Show($"An error has occured while connecting to the server: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}