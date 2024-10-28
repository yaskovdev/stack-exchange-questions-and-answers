namespace StatefulDependencyInjection;

public interface IDataProcessor
{
    void ProcessData(int socketId, byte[] data);
}