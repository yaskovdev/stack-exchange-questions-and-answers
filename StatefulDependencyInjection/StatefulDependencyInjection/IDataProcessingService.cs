namespace StatefulDependencyInjection;

public interface IDataProcessingService
{
    void StartProcessing(int socketId);

    void StopProcessing(int socketId);
}
