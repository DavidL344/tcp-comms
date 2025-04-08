using System.Net.Sockets;
using System.Text;

var client = new TcpClient();

try
{
    await client.ConnectAsync("127.0.0.1", 8080);
}
catch (Exception e)
{
    Console.WriteLine($"An error has occured while connecting to the server: {e.Message}");
    Environment.Exit(1);
}

Console.WriteLine("Connection established.");
await using var stream = client.GetStream();

// Send a payload to the server
var message = Encoding.UTF8.GetBytes("Hello from the client!");
stream.Write(message, 0, message.Length);

// Read the response
var buffer = new byte[1024];
var totalBytes = stream.Read(buffer, 0, buffer.Length);

var received = Encoding.UTF8.GetString(buffer, 0, totalBytes);
Console.WriteLine($"Message received: \"{received.Trim()}\"");

// Close the connection
stream.Close();
client.Close();
Console.WriteLine("Connection closed.");
Environment.Exit(0);
