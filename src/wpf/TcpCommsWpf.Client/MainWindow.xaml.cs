using System.Windows;

namespace TcpCommsWpf.Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IProgress<string> _progress;

    public MainWindow()
    {
        InitializeComponent();
        _progress = new Progress<string>(UpdateStatus);
        _progress.Report("Ready");
    }

    private async void Login(object sender, RoutedEventArgs e)
    {
        // The methods using "async void" need to be under the try-catch block:
        // any exceptions unhandled by the method might lead to the process crash (ReSharper: AsyncVoidMethod)
        try
        {
            EnableForm(false);
            _progress.Report("Connecting...");
            await Task.Delay(1000); // To show the connecting status
            
            if (!int.TryParse(Port.Text.Trim(), out var port))
                throw new ArgumentException($"The port {port} must be a valid integer!", nameof(port));

            var client = new Client(Ip.Text.Trim(), port);
            await client.ConnectAsync();
        }
        catch (Exception ex)
        {
            _progress.Report("An error has occured! Please check the popup box for more information.");
            await Console.Error.WriteLineAsync($"An error has occured while connecting to the server: {ex.Message}");
            MessageBox.Show($"An error has occured while connecting to the server: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            EnableForm(true);
            _progress.Report("Ready");
        }
    }

    private void UpdateStatus(string text)
    {
        // Console.WriteLine(text);
        StatusBar.Text = text;
    }

    private void EnableForm(bool enable)
    {
        Ip.IsEnabled = enable;
        Port.IsEnabled = enable;
        Submit.IsEnabled = enable;
    }
}