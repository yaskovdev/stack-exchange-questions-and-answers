namespace StatefulDependencyInjection;

public class DataProcessor(ILogger<DataProcessingService> logger)
{
    public void ProcessData(int socketId, byte[] data)
    {
        logger.LogInformation("Processing {Length} bytes data from socket {SocketId}", data.Length, socketId);
    }
}