using System.Net;
using System.Net.Sockets;
using System.Windows;
using TcpCommsWpf.Shared;

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
            
            var chat = new Chat(Chat.Side.Server)
                { Client = client, Progress = progress, CancellationToken = cancellationToken };
            chat.Show();
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