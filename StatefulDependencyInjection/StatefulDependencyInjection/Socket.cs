using Timer = System.Timers.Timer;

namespace StatefulDependencyInjection;

public class Socket : IDisposable
{
    private readonly Timer _timer;

    public Socket(int id, IDataProcessor dataProcessor)
    {
        _timer = new Timer(TimeSpan.FromSeconds(1));
        _timer.Elapsed += (_, _) =>
        {
            var data = new byte[1024];
            Random.Shared.NextBytes(data);
            dataProcessor.ProcessData(id, data);
        };
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
}