using TcpComms.Server;

AppDomain.CurrentDomain.UnhandledException += ExceptionHandler;

var server = new TcpServer("127.0.0.1", 8080);
await server.StartAsync();

void ExceptionHandler(object sender, UnhandledExceptionEventArgs e)
{
    Console.Error.WriteLine(e.ExceptionObject.ToString());
    Environment.Exit(1);
}
