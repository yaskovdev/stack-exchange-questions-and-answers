namespace StatefulDependencyInjection;

public class SocketHandler : IDisposable
{
    private readonly Socket _socket;
    private readonly ILogger<DataProcessingService> _logger;

    // What if the SocketHandler has more dependencies?
    // What if the dependencies in turn need their own dependencies?
    // What if one of the leaves of this potentially big dependency tree needs a dependency from the dependency
    // container, say, the logger? Then the logger becomes a pass-through dependency.
    public SocketHandler(int socketId, ILogger<DataProcessingService> logger)
    {
        _logger = logger;
        _socket = new Socket(socketId, ProcessData);
    }

    private void ProcessData(int socketId, byte[] data)
    {
        _logger.LogInformation("Processing {Length} bytes data from socket {SocketId}", data.Length, socketId);
    }

    public void Dispose()
    {
        _socket.Dispose();
        GC.SuppressFinalize(this);
    }
}