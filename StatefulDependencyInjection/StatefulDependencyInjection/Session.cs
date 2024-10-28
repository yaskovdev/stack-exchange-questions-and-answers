namespace StatefulDependencyInjection;

public class Session(int socketId, DataProcessor dataProcessor) : IDisposable
{
    private readonly Socket _socket = new(socketId, dataProcessor);

    public void Dispose()
    {
        _socket.Dispose();
        GC.SuppressFinalize(this);
    }
}