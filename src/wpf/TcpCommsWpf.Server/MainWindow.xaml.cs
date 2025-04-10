using System.Windows;
using TcpCommsWpf.Shared;

namespace TcpCommsWpf.Server;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AppDomain.CurrentDomain.UnhandledException += Utils.ExceptionHandler;
        
        // Make sure the progress is updated on the UI thread
        IProgress<string> progress = new Progress<string>(UpdateInfo);

        var server = new Server();
        server.StartAsync(progress).ConfigureAwait(false);
    }

    private void UpdateInfo(string text)
    {
        Info.Text += Environment.NewLine + text;
        Console.WriteLine(text);
    }
}