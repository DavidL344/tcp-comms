using System.Net;
using System.Net.Sockets;
using TcpCommsWpf.Shared;

namespace TcpCommsWpf.Server;

public class Server
{
    private readonly TcpListener _tcpListener;

    public Server(int port = 8080)
    {
        _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
    }
    
    public async Task StartAsync(IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        progress.Report("Starting the server...");
        _tcpListener.Start();
        progress.Report($"Server listening on {_tcpListener.LocalEndpoint}...");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
            var clientId = Guid.NewGuid().ToString();
            progress.Report($"Connection established (GUID {clientId}).");
            
            var chat = new Chat(Chat.Side.Server, clientId)
                { Client = client, Progress = progress, CancellationToken = cancellationToken };
            chat.Show();
        }
    }
}