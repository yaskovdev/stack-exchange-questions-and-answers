using Timer = System.Timers.Timer;

namespace StatefulDependencyInjection;

public class Socket : IDisposable
{
    private readonly Timer _timer;
    public int Id { get; }
    private Action<int, byte[]> OnData { get; }

    public Socket(int id, Action<int, byte[]> onData)
    {
        Id = id;
        OnData = onData;
        _timer = new Timer(TimeSpan.FromSeconds(1));
        _timer.Elapsed += (_, _) =>
        {
            var data = new byte[1024];
            Random.Shared.NextBytes(data);
            OnData(id, data);
        };
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
}