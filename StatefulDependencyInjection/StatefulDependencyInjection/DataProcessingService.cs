using System.Collections.Concurrent;

namespace StatefulDependencyInjection;

public class DataProcessingService(ILogger<DataProcessingService> logger) : IDataProcessingService
{
    private readonly ConcurrentDictionary<int, Session> _sessions = new();

    public void StartProcessing(int socketId)
    {
        _sessions.GetOrAdd(socketId, new Session(socketId, CreateDataProcessor()));
    }

    private DataProcessor CreateDataProcessor()
    {
        // What if the Decoder and Encoder have dependencies themselves?
        // What if the dependencies in turn need their own dependencies?
        // What if one of the leaves of this potentially big dependency tree needs a dependency from the dependency
        // container, say, the logger? Then the logger becomes a pass-through dependency.
        return new DataProcessor(
            logger /*, new Decoder(new DecoderDependency(new StatisticsCollector(), logger)), new Encoder()*/);
    }

    public void StopProcessing(int socketId)
    {
        if (_sessions.TryRemove(socketId, out var session))
        {
            session.Dispose();
        }
        else
        {
            logger.LogWarning("Session with socket ID {SocketId} not found", socketId);
        }
    }
}