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

    public void Start()
    {
        var buffer = new byte[1024];
        _tcpListener.Start();

        while (true)
        {
            using var client = _tcpListener.AcceptTcpClient();
            Console.WriteLine("Connection established.");
        
            var stream = client.GetStream();
            int totalBytes;

            while ((totalBytes = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var received = Encoding.UTF8.GetString(buffer, 0, totalBytes);
            
                var payload = "Hello from the server!\n";
                Console.WriteLine($"Message received: \"{received.Trim()}\"");
                var response = Encoding.UTF8.GetBytes(payload);
            
                stream.Write(response, 0, response.Length);
            }
            Console.WriteLine("Connection closed.");
        }
    }
}
