using System.ComponentModel;
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

    private enum Event
    {
        ClientConnected,
        ClientDisconnected
    }

    private NetworkStream _stream = default!;
    private readonly IProgress<string> _messages;
    private readonly IProgress<Event> _events;
    private readonly Side _side;
    private readonly Side _oppositeSide;
    
    public Chat(Side side, string tempId)
    {
        _side = side;
        _oppositeSide = (Side)Math.Abs((int)side - 1);
        
        InitializeComponent();
        Title = $"Chat ({Enum.GetName(side)} --> {Enum.GetName(_oppositeSide)} [{tempId}])";
        _messages = new Progress<string>(message =>
        {
            MessageView.Items.Add(message.Trim());
        });
        _events = new Progress<Event>(@event =>
        {
            switch (@event)
            {
                case Event.ClientConnected:
                    MessageView.Items.Add($"{_oppositeSide} connected.");
                    break;
                case Event.ClientDisconnected:
                    MessageBox.IsReadOnly = true;
                    SendButton.IsEnabled = false;
                    MessageView.Items.Add($"{_oppositeSide} disconnected.");
                    break;
                default:
                    MessageView.Items.Add($"An unknown event occured: {@event}");
                    break;
            }
        });
        _ = Task.Run(() => HandleClientAsync(_messages), CancellationToken);
    }
    
    private async Task HandleClientAsync(IProgress<string> messages)
    {
        using (Client)
        {
            var buffer = new byte[1024];
            _stream = Client.GetStream();
            
            _ = Task.Run(() =>
            {
                _events.Report(Event.ClientConnected);
                while (true)
                {
                    Task.Delay(1000, CancellationToken);
                    if (_stream.CanWrite) continue;
                    
                    _events.Report(Event.ClientDisconnected);
                    
                    _stream.Dispose();
                    return Task.CompletedTask;
                }
            }, CancellationToken);
            
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
                if (e.InnerException is SocketException)
                {
                    // The connection was closed
                    Progress.Report($"{_side} disconnected.");
                    return;
                }
                
                Progress.Report($"Error processing {_oppositeSide.ToString().ToLower()}: {e.Message}");
#if DEBUG
                Progress.Report(e.ToString());
#endif
            }
        }
    }
    
    private void MessageSend(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(MessageBox.Text.Trim())) return;
        if (!_stream.CanWrite) return;
        
        var payload = $"{MessageBox.Text}\n";
        var response = Encoding.UTF8.GetBytes(payload);
        
        _messages.Report($"[{_side} ({DateTime.Now.Hour}:{DateTime.Now.Minute})]: {MessageBox.Text}");
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

    private void CloseConnection(object? sender, CancelEventArgs e)
    {
        _stream.Flush();
        _stream.Close();
    }
}