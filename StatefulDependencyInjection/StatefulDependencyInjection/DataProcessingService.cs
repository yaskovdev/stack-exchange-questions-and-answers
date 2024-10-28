using System.Collections.Concurrent;

namespace StatefulDependencyInjection;

public class DataProcessingService(ILogger<DataProcessingService> logger) : IDataProcessingService
{
    private readonly ConcurrentDictionary<int, SocketHandler> _socketHandlers = new();

    public void StartProcessing(int socketId)
    {
        _socketHandlers.GetOrAdd(socketId, _ => CreateSocketHandler(socketId));
    }

    private SocketHandler CreateSocketHandler(int socketId)
    {
        // I have to either manually create or fetch from the container all the SocketHandler dependencies here.
        return new SocketHandler(socketId, logger);
    }

    public void StopProcessing(int socketId)
    {
        if (_socketHandlers.TryRemove(socketId, out var session))
        {
            session.Dispose();
        }
        else
        {
            logger.LogWarning("Socket handler for socket ID {SocketId} not found", socketId);
        }
    }
}