# TcpComms

---

### A simple client-server communication implementation using TCP/IP

---

#### Architecture description

Since this is my first exposure to TCP/IP sockets, I've decided to prototype the solution first, which can be found in the [Console directory](src/console).
Then, I took what I learned, and re-used as much code as possible for the WPF projects,
extracting shared UI and logic into a [separate library](src/wpf/TcpCommsWpf.Shared).
This includes not only the chat window and its logic, but also an exception handler.

Client/server-specific code is hosted in their respective projects in their respective classes,
and is called from their WPF (*.xaml.cs) counterparts. The chat UI and logic are shared across the projects.

---

#### Unspecified requirements

Aside from traditional error-handling, I wanted to give the user feedback in a different way as well.
When the opposite side (be it the client or the server) disconnects, the side remaining in the chat
gets a system message about the event. Moreover, it also locks the textbox and the send button
to prevent the user from sending messages to a disconnected party.

- That decision was partially reversed, as the requirements state that the user
needs to be notified with an error message - in the end, I modified the code, so that the user
would receive a pop-up when the other party disconnects, and only disabled the send button
to make sure the user could still see the error when sending a message using the enter key.

For the UI, I decided to use [ModernWpfUi](https://www.nuget.org/packages/ModernWpfUI/)
by [Kinnara](https://github.com/Kinnara/ModernWpf), which greatly simplified my UI design
([SimpleStackPanel](https://github.com/Kinnara/ModernWpf/blob/master/ModernWpf.SampleApp/ControlPages/SimpleStackPanelPage.xaml)),
and also added support for dark mode based on the user's theme.

---

#### Encountered challenges and their resolutions (pt. 1)

One of the biggest challenges for me was delegating the processing outside the main thread, which is something
I only discovered after testing the client when the server wasn't running - the connection timeout
would never happen, and the UI would freeze for the whole duration of the app running. To fix this,
I first experimented with `Task.Run()`, which wouldn't wait for the process to finish executing:
```csharp
// Doesn't pause execution until finished
_ = Task.Run(() => ...).ConfigureAwait(false);
```
I had to find a way to use the `await` keyword (or pause execution without hanging the main thread) inside a method
linked to an event inside a XAML design (which means the return type has to be `void`). Since I could use `async Task`,
I had to use `async void` despite it generally being considered an anti-pattern.
```csharp
// Can't await connecting to the server
private void Login(object sender, RoutedEventArgs e) { ... }

// Pauses execution but won't compile - invalid signature because of the `Task` keyword
private async Task Login(object sender, RoutedEventArgs e) { ... }

// An anti-pattern, but will compile and pause execution until completed without hanging the main thread
private async void Login(object sender, RoutedEventArgs e) { ... }
```

---

#### Encountered challenges and their resolutions (pt. 2)

Another big challenge was reporting information asynchronously to the main thread (UI).
In the process, I learned about the `IProgress<T>` data structure,
allowing me to use its `Report()` method to send information back to the main thread. 
```csharp
// public Chat(Side side, string tempId)
_messages = new Progress<string>(message =>
{
    MessageView.Items.Add(message.Trim());
});

// After constructor initialisation
_ = Task.Run(() => HandleClientAsync(_messages), CancellationToken);

// private async Task HandleClientAsync(IProgress<string> messages)
_messages.Report($"Message received: {received}");
```
This was especially useful when displaying chat messages, but it also helped me a lot
with logging events in the client login UI:
```csharp
// public partial class Chat : Window
private enum Event
{
    ClientConnected,
    ClientDisconnected
}

// public MainWindow()
_progress = new Progress<string>(UpdateStatus);
_progress.Report("Ready");

// private async void Login(object sender, RoutedEventArgs e)
try
{
    _progress.Report("Connecting...");
    await Task.Delay(1); // The login happens so fast the message wouldn't appear
    
    var client = new Client(Ip.Text.Trim(), port);
    await client.ConnectAsync();
}
catch (Exception ex)
{
    _progress.Report("An error has occured! Please check the popup box for more information.");
}
finally
{
    _progress.Report("Ready");
}

// private void UpdateStatus(string text)
StatusBar.Text = text;
```

My favourite way of utilising them has to be for event logging (`Progress<Event>`):
```csharp
// public Chat(Side side, string oppositeSideId)
_events = new Progress<Event>(@event =>
{
    switch (@event)
    {
        case Event.ClientConnected:
            // Log a message
            break;
        case Event.ClientDisconnected:
            // Log a message
            // Disable the send button
            // Display an error message
            break;
        default:
            MessageView.Items.Add($"[System] An unknown event occured: {@event}");
            break;
    }
});

// private async Task HandleClientAsync(IProgress<string> messages)
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
```

---

#### Conclusion

Thanks to this exercise, I learned how to implement a network communication over TCP/IP,
but also figured out how to notify the main thread of changes without blocking it!

---

#### Building

1) Clone the repository
```bash
git clone https://github.com/DavidL344/tcp-comms.git
```

2) Change into the directory
```bash
cd tcp-comms/src/wpf
```

3) Build and run the projects
```bash
# Needs `dotnet restore` as well if a --no-restore flag is provided
dotnet build TcpCommsWpf.Client\TcpCommsWpf.Client.csproj
dotnet build TcpCommsWpf.Server\TcpCommsWpf.Server.csproj
```

4) Run the projects
```bash
dotnet run --project TcpCommsWpf.Client
dotnet run --project TcpCommsWpf.Server
```
