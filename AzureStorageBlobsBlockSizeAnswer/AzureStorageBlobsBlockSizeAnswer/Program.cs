using System.Collections.Immutable;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace AzureStorageBlobsBlockSizeAnswer;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        string connectionString = "UseDevelopmentStorage=true";
        string containerName = $"container-{Guid.NewGuid()}";
        string blobName = $"blob-{Guid.NewGuid()}";
        var blobServiceClient = new BlobServiceClient(connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var containerCreateResponse = await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None, ImmutableDictionary<string, string>.Empty, CancellationToken.None);
        if (containerCreateResponse == null)
        {
            throw new Exception($"Unable to create blob container: container {containerName} already exists");
        }
        var appendBlobClient = blobContainerClient.GetAppendBlobClient(blobName);
        if (!await appendBlobClient.ExistsAsync())
        {
            await appendBlobClient.CreateAsync();
        }

        int blockSize = 4 * 1024 * 1024; 
        int bufferSize = 64 * 1024; 
        byte[] buffer = new byte[blockSize];
        string content = "Hello, this is a test write to an append blob.\n";
        byte[] data = Encoding.UTF8.GetBytes(content);
        long totalBytesWritten = 0;
        int bytesFilled = 0;
        var tasks = new List<Task>();
        while (totalBytesWritten < 12582912) 
        {
            int bytesToCopy = Math.Min(data.Length, blockSize - bytesFilled);
            Array.Copy(data, 0, buffer, bytesFilled, bytesToCopy);
            bytesFilled += bytesToCopy;
            if (bytesFilled == blockSize)
            {
                byte[] writeData = new byte[blockSize];
                Array.Copy(buffer, writeData, blockSize);

                // Simulate a failure right before the AppendBlockAsync call
                throw new Exception($"Application is going to crash here, {bytesFilled} bytes of data will be lost");
                
                tasks.Add(AppendBlockAsync(appendBlobClient, writeData));
                totalBytesWritten += blockSize;
                bytesFilled = 0; 
            }
        }
        if (bytesFilled > 0)
        {
            byte[] finalBlock = new byte[bytesFilled];
            Array.Copy(buffer, finalBlock, bytesFilled);
            tasks.Add(AppendBlockAsync(appendBlobClient, finalBlock));
            totalBytesWritten += bytesFilled;
        }
        await Task.WhenAll(tasks); 
        Console.WriteLine($"Done. Total Bytes Written: {totalBytesWritten}");
    }
    static async Task AppendBlockAsync(AppendBlobClient appendBlobClient, byte[] data)
    {
        using var ms = new MemoryStream(data);
        await appendBlobClient.AppendBlockAsync(ms);
        Console.WriteLine($"Appended {data.Length} bytes.");
    }
}