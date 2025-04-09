using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace TcpCommsWpf.Server;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly TcpListener _tcpListener;
    
    public MainWindow()
    {
        InitializeComponent();
        AppDomain.CurrentDomain.UnhandledException += ExceptionHandler;
        
        _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
        StartAsync().ConfigureAwait(false);
    }

    private async Task StartAsync(CancellationToken cancellationToken = default)
    {
        // Make sure the progress is updated on the UI thread
        IProgress<string> progress = new Progress<string>(message =>
        {
            UpdateInfo(message);
        });

        progress.Report("Starting the server...");
        
        _tcpListener.Start();
        progress.Report($"Server listening on {_tcpListener.LocalEndpoint}...");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
            progress.Report("Connection established.");

            _ = Task.Run(() => HandleClientAsync(client, progress, cancellationToken), cancellationToken);
            new Chat().Show();
        }
    }
    
    private async Task HandleClientAsync(TcpClient client,
        IProgress<string> progress, CancellationToken cancellationToken)
    {
        using (client)
        {
            var buffer = new byte[1024];
            var stream = client.GetStream();
            
            try
            {
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    var received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (received.Trim() == string.Empty) continue;
                    
                    progress.Report($"Message received: \"{received.Trim()}\"");
                    var payload = $"Hello from the server! Received message: {received}\n";
                    
                    var response = Encoding.UTF8.GetBytes(payload);
                    await stream.WriteAsync(response, cancellationToken);
                }
                progress.Report("Connection closed.");
            }
            catch (Exception e)
            {
                progress.Report($"Error processing client: {e.Message}");
            }
        }
    }
    
    private void ExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        Console.Error.WriteLine(e.ExceptionObject.ToString());
        MessageBox.Show($"An exception has occured: {e.ExceptionObject}",
            "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        Environment.Exit(1);
    }

    private void UpdateInfo(string text)
    {
        Info.Text += Environment.NewLine + text;
        Console.WriteLine(text);
    }
}