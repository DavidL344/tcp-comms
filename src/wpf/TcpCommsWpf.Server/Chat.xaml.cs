using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace TcpCommsWpf.Server;

public partial class Chat : Window
{
    public required TcpClient Client { get; init; }
    public required IProgress<string> Progress { get; init; }
    public required CancellationToken CancellationToken { get; init; }
    private NetworkStream _stream;
    private readonly IProgress<string> _messages;
    
    public Chat()
    {
        InitializeComponent();
        _messages = new Progress<string>(message =>
        {
            MessageView.Items.Add(message);
        });
        _ = Task.Run(() => HandleClientAsync(_messages), CancellationToken);
    }
    
    private async Task HandleClientAsync(IProgress<string> messages)
    {
        using (Client)
        {
            var buffer = new byte[1024];
            _stream = Client.GetStream();
            
            try
            {
                int bytesRead;
                while ((bytesRead = await _stream.ReadAsync(buffer, CancellationToken)) > 0)
                {
                    var received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (received.Trim() == string.Empty) continue;
                    
                    Progress.Report($"Message received: \"{received.Trim()}\"");
                    messages.Report($"[Client ({DateTime.Now.Hour}:{DateTime.Now.Minute})]: {received}");
                }
                Progress.Report("Connection closed.");
            }
            catch (Exception e)
            {
                Progress.Report($"Error processing client: {e.Message}");
            }
        }
    }

    private void MessageSend(object sender, RoutedEventArgs e)
    {
        _messages.Report($"[Server ({DateTime.Now.Hour}:{DateTime.Now.Minute})]: {MessageBox.Text}");
        var payload = $"{MessageBox.Text}\n";
        var response = Encoding.UTF8.GetBytes(payload);
        
        _stream.WriteAsync(response, CancellationToken).ConfigureAwait(false);
        Progress.Report($"Message sent: \"{MessageBox.Text}\"");
        MessageBox.Text = string.Empty;
    }

    private void MessageBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        MessageSend(sender, e);
    }
}