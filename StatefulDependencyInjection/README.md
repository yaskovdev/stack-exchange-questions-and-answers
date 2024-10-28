# Stateful Dependency Injection

How can I use dependency injection for a tree of stateful objects?

I’ll illustrate the issue with a Web application example. This application handles two requests:

1. `POST /sockets/{id}/subscribe` — subscribes to a socket with a given ID and logs the data it receives.
2. `POST /sockets/{id}/unsubscribe` — unsubscribes from a socket with a given ID.

The application contains a `SocketManager` object that looks like this:

```csharp
public class SocketHandler : IDisposable
{
    private readonly Socket _socket;
    private readonly ILogger<DataProcessingService> _logger;

    public SocketHandler(int socketId, ILogger<DataProcessingService> logger)
    {
        _logger = logger;
        _socket = new Socket(socketId, ProcessData);
    }

    private void ProcessData(int socketId, byte[] data)
    {
        _logger.LogInformation("Processing {Length} bytes data from socket {SocketId}", data.Length, socketId);
    }
}
```

The problem is that I need to create a new instance of the `SocketHandler` object for each request, therefore I cannot really use dependency injection for that.

This is not a big deal while the application is small, but it quickly turns into an issue as the application grows. In particular:

1. Eventually the `SocketHandler` will require additional dependencies to do its job. It means that I now need to create a new instance of each of these dependencies for each request. At some point the majority of my classes will not be managed by the dependency injection container.
2. Suppose that one `SocketHandler` dependency deep in the dependency tree requires a stateless dependency from the dependency injection container, such as a logger. I have to either pass the container itself through all the levels of the dependency tree so that whoever needs the logger could get it from there, or to pass the logger. No matter what I choose, I end up with a pass-through dependency, which is a mess: imagine having to pass dozens of dependencies through the dependency tree.

Has someone faced this issue? How did you solve it?