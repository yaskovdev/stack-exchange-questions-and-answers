using Timer = System.Timers.Timer;

namespace StatefulDependencyInjection;

public class Socket : IDisposable
{
    private readonly Timer _timer;

    public Socket(int id, Action<int, byte[]> processData)
    {
        _timer = new Timer(TimeSpan.FromSeconds(1));
        _timer.Elapsed += (_, _) =>
        {
            var data = new byte[1024];
            Random.Shared.NextBytes(data);
            processData(id, data);
        };
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
}
