using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace TcpCommsWpf.Shared;

public partial class Chat : Window
{
    public required TcpClient Client { get; init; }
    public required IProgress<string> Progress { get; init; }
    public required CancellationToken CancellationToken { get; init; }
    
    public enum Side
    {
        Client = 0,
        Server = 1
    }

    private NetworkStream _stream = default!;
    private readonly IProgress<string> _messages;
    private readonly Side _side;
    private readonly Side _oppositeSide;
    
    public Chat(Side side)
    {
        _side = side;
        _oppositeSide = (Side)Math.Abs((int)side - 1);
        
        InitializeComponent();
        Title = $"Chat ({Enum.GetName(side)} --> {Enum.GetName(_oppositeSide)})";
        _messages = new Progress<string>(message =>
        {
            MessageView.Items.Add(message.Trim());
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
                    messages.Report($"[{_oppositeSide} ({DateTime.Now.Hour}:{DateTime.Now.Minute})]: {received}");
                }
                Progress.Report("Connection closed.");
            }
            catch (Exception e)
            {
                Progress.Report($"Error processing {_oppositeSide.ToString().ToLower()} ({e}): {e.Message}");
            }
        }
    }
    
    private void MessageSend(object sender, RoutedEventArgs e)
    {
        _messages.Report($"[{_side} ({DateTime.Now.Hour}:{DateTime.Now.Minute})]: {MessageBox.Text}");
        var payload = $"{MessageBox.Text}\n";
        var response = Encoding.UTF8.GetBytes(payload);
        
        _stream.WriteAsync(response, CancellationToken).ConfigureAwait(false);
        Progress.Report($"Message sent: \"{MessageBox.Text}\"");
        MessageBox.Text = string.Empty;
    }

    private void WatchForEnter(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        MessageSend(sender, e);
    }
}