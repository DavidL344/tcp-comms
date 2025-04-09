using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace TcpCommsWpf.Server;

public partial class Chat : Window
{
    public required TcpClient Client { get; init; }
    public required IProgress<string> Progress { get; init; }
    public required CancellationToken CancellationToken { get; init; }
    
    public Chat()
    {
        InitializeComponent();
        IProgress<string> messages = new Progress<string>(message =>
        {
            ListView.Items.Add(message);
        });
        _ = Task.Run(() => HandleClientAsync(messages), CancellationToken);
    }
    
    private async Task HandleClientAsync(IProgress<string> messages)
    {
        using (Client)
        {
            var buffer = new byte[1024];
            var stream = Client.GetStream();
            
            try
            {
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, CancellationToken)) > 0)
                {
                    var received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (received.Trim() == string.Empty) continue;
                    
                    Progress.Report($"Message received: \"{received.Trim()}\"");
                    messages.Report($"[Client]: {received}");
                    
                    var payload = $"Hello from the server! Received message: {received}\n";
                    
                    var response = Encoding.UTF8.GetBytes(payload);
                    await stream.WriteAsync(response, CancellationToken);
                    
                }
                Progress.Report("Connection closed.");
            }
            catch (Exception e)
            {
                Progress.Report($"Error processing client: {e.Message}");
            }
        }
    }
}