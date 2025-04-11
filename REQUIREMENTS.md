Summary:
- Two WPF applications - one listens on a port ([server](src/wpf/TcpCommsWpf.Server)), and one connects to it ([client](src/wpf/TcpCommsWpf.Client))
- Both sides can message each other in real time
- [Shared logic](src/wpf/TcpCommsWpf.Shared) with different roles
- Written in .NET 8, targeting MS Windows

Functionality:
- A login UI for the client
- Bidirectional sending and receiving messages
- Connection status
    - Server
        - the logs' UI (scrollable logs appended in plain text)
        - the console (connection status + messages sent/received)
    - Client
        - the login UI (a status bar below the login button)
        - the console (connection status + messages sent/received)
- Error messages
    - Unsuccessful login: a pop-up message in the client's login UI
    - Interrupted connection: a pop-up message in the remaining side's chat UI
    - Send a message to a disconnected side: a pop-up message in the sender's chat UI
        - previously disabled the chat box and the send button, but changed to conform to the requirements
- Event logging
    - Server: the logs' UI and the console
    - Client: the console
    - Chat (shared): a system entry in the chat when a client/server (dis)connects

Requirements:
- .NET 8, WPF for both projects
- TCP/IP communication (TcpListener, TcpClient)
- Well-structured and readable code (SOLID, responsibility separation)
- Basic error- and exception-handling

To submit:
- Source code
- README.md
  - A short solution and architecture description
  - Building and running instructions
