namespace StatefulDependencyInjection;

public class Session : IDisposable
{
    private readonly Socket _socket;

    public Session(int socketId, DataProcessor dataProcessor)
    {
        _socket = new Socket(socketId, dataProcessor.ProcessData);
    }

    public void Dispose()
    {
        _socket.Dispose();
    }
}