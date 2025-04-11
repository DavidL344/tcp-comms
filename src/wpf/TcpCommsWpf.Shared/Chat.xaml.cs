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
    private readonly string _oppositeSideId;
    private bool _closing;
    
    public Chat(Side side, string oppositeSideId)
    {
        _side = side;
        _oppositeSide = (Side)Math.Abs((int)side - 1);
        _oppositeSideId = oppositeSideId;
        
        InitializeComponent();
        Title = $"Chat ({Enum.GetName(side)} --> {Enum.GetName(_oppositeSide)} [{oppositeSideId}])";
        _messages = new Progress<string>(message =>
        {
            MessageView.Items.Add(message.Trim());
        });
        _events = new Progress<Event>(@event =>
        {
            switch (@event)
            {
                case Event.ClientConnected:
                    Progress!.Report($"Connection established ({(Guid.TryParse(oppositeSideId, out _) ? "GUID " : null)}{oppositeSideId}).");
                    MessageView.Items.Add($"[System] {_oppositeSide} connected.");
                    break;
                case Event.ClientDisconnected:
                    //MessageTextBox.IsReadOnly = true;
                    SendButton.IsEnabled = false;
                    MessageView.Items.Add($"[System] {_oppositeSide} disconnected.");
                    if (!_closing)
                        MessageBox.Show($"{_oppositeSide} disconnected.", Title,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    MessageView.Items.Add($"[System] An unknown event occured: {@event}");
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
            
            _ = Task.Run(async () =>
            {
                _events.Report(Event.ClientConnected);
                while (true)
                {
                    await Task.Delay(1000, CancellationToken);
                    if (_stream.CanWrite) continue;
                    
                    _events.Report(Event.ClientDisconnected);
                    
                    await _stream.DisposeAsync();
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
                    
                    Progress.Report($"Message from \"{_oppositeSideId}\" received: \"{received.Trim()}\"");
                    messages.Report($"{_oppositeSide} ({GetFormattedTime()}): {received}");
                }
                Progress.Report($"{_oppositeSide} ({_oppositeSideId}) disconnected.");
            }
            catch (Exception e)
            {
                if (e.InnerException is SocketException)
                {
                    // The connection was closed
                    Progress.Report("Connection closed.");
                    return;
                }
                
                Progress.Report(
                    $"Error processing {_oppositeSide.ToString().ToLower()} ({_oppositeSideId}): {e.Message}");
#if DEBUG
                Progress.Report(e.ToString());
#endif
            }
        }
    }
    
    private string GetFormattedDateTime()
    {
        var dt = DateTime.Now;
        return $"{dt.Day:D2}.{dt.Month:D2}.{DateTime.Now:yy} {dt.Hour:D2}:{dt.Minute:D2}:{dt.Second:D2}";
    }
    
    private string GetFormattedTime()
        => $"{DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2}:{DateTime.Now.Second:D2}";

    private void MessageSend(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(MessageTextBox.Text.Trim())) return;
        if (!_stream.CanWrite)
        {
            MessageBox.Show($"Unable to send the message: {_oppositeSide} disconnected!",
                Title, MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        
        var payload = $"{MessageTextBox.Text}\n";
        var response = Encoding.UTF8.GetBytes(payload);
        
        _messages.Report($"{_side} ({GetFormattedTime()}): {MessageTextBox.Text}");
        _stream.WriteAsync(response, CancellationToken).ConfigureAwait(false);
        
        Progress.Report($"Message to \"{_oppositeSideId}\" sent: \"{MessageTextBox.Text}\"");
        MessageTextBox.Text = string.Empty;
    }

    private void WatchForEnter(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        MessageSend(sender, e);
    }

    private void CloseConnection(object? sender, EventArgs eventArgs)
    {
        _closing = true;
        _stream.Flush();
        _stream.Close();
    }
}