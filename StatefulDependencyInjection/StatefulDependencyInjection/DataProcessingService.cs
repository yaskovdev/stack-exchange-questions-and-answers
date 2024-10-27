using System.Collections.Concurrent;

namespace StatefulDependencyInjection;

public class DataProcessingService(ILogger<DataProcessingService> logger) : IDataProcessingService
{
    private readonly ConcurrentDictionary<int, Session> _sessions = new();

    public void StartProcessing(int socketId)
    {
        _sessions[socketId] = new Session(socketId, new DataProcessor(logger));
    }

    public void StopProcessing(int socketId)
    {
        if (_sessions.TryGetValue(socketId, out var session))
        {
            session.Dispose();
        }
        else
        {
            logger.LogWarning("Session with socket ID {SocketId} not found", socketId);
        }
    }
}