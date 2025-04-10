using System.Net;
using System.Net.Sockets;
using TcpCommsWpf.Shared;

namespace TcpCommsWpf.Client;

public class Client
{
    private readonly TcpClient _client;
    private readonly IProgress<string> _progress;
    private readonly IPAddress _host;
    private readonly int _port;

    public Client(string host, int port)
    {
        if (!IPAddress.TryParse(host, out _host!))
            throw new ArgumentException($"Invalid IP address ({host})!", nameof(host));
        
        if (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
            throw new ArgumentException($"Invalid port {port}!", nameof(port));
        
        _port = port;
        _client = new TcpClient();
        _progress = new Progress<string>(Console.WriteLine);
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _client.ConnectAsync(_host, _port, cancellationToken);
        
        var chat = new Chat(Chat.Side.Client, $"{_host}:{_port}")
            { Client = _client, Progress = _progress, CancellationToken = cancellationToken };
        chat.Show();
    }
}