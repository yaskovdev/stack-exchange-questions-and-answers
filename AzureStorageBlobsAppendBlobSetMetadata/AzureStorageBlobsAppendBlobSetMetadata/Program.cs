namespace AzureStorageBlobsAppendBlobSetMetadata;

using System.Collections.Immutable;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var blobContainerName = $"container-{Guid.NewGuid()}";
        var blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
        var containerCreateResponse = await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None, ImmutableDictionary<string, string>.Empty, CancellationToken.None);
        if (containerCreateResponse == null)
        {
            throw new Exception($"Unable to create blob container: container {blobContainerName} already exists");
        }

        var blob = blobContainerClient.GetAppendBlobClient($"blob-{Guid.NewGuid()}");
        var stream = await blob.OpenWriteAsync(false, new AppendBlobOpenWriteOptions { BufferSize = 4 }, CancellationToken.None);

        for (var i = 0; i < 1000; i++)
        {
            byte[] payload = [0, 1, 2, 3, 4, 5, 6, 7];
            Console.WriteLine($"Appending {payload.Length} bytes to blob");
            await stream.WriteAsync(payload, CancellationToken.None); // throws RequestFailedException once SetMetadataAsync is called
            Console.WriteLine("Setting metadata for blob");
            await blob.SetMetadataAsync(ImmutableDictionary<string, string>.Empty.Add("key", "value"));
            await Task.Delay(1000);
        }
    }
}
