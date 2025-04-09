using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpComms.Server;

public class TcpServer
{
    private readonly TcpListener _tcpListener;

    public TcpServer(TcpListener tcpListener)
    {
        _tcpListener = tcpListener;
    }

    public TcpServer(string ip, int port)
    { 
        if (!IPAddress.TryParse(ip, out var host))
            throw new ArgumentException($"Invalid IP address ({ip})!", nameof(ip));

        if (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
            throw new ArgumentException($"Invalid port {port}!", nameof(port));
        
        _tcpListener = new TcpListener(host, port);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _tcpListener.Start();
        Console.WriteLine("Server listening on {0}...", _tcpListener.LocalEndpoint);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
            Console.WriteLine("Connection established.");

            _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
        }
    }
    
    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
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
                    Console.WriteLine($"Message received: \"{received.Trim()}\"");
                    
                    var payload = $"Hello from the server! Received message: {received}";
                    var response = Encoding.UTF8.GetBytes(payload);
                    await stream.WriteAsync(response, cancellationToken);
                }
                Console.WriteLine("Connection closed.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error processing client: {e.Message}");
            }
        }
    }
}
