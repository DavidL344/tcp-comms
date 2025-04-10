using System.Windows;

namespace TcpCommsWpf.Shared;

public static class Utils
{
    public static void ExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        Console.Error.WriteLine(e.ExceptionObject.ToString());
        MessageBox.Show($"An exception has occured: {e.ExceptionObject}",
            "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        Environment.Exit(1);
    }
}