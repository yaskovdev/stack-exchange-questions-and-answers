namespace AzureStorageBlobsBlockSize;

using System.Collections.Immutable;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

internal static class Program
{
    private const long WriteBufferSizeInBytes = 100 * 1024 * 1024;

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
        var appendBlobClient = blobContainerClient.GetAppendBlobClient($"blob-{Guid.NewGuid()}");
        var stream = await appendBlobClient.OpenWriteAsync(false, new AppendBlobOpenWriteOptions { BufferSize = WriteBufferSizeInBytes }, CancellationToken.None);

        // Write 8000 bytes to the append blob, expect it to be written in one block.
        // Actually, it will be written in 1000 blocks of 8 bytes each, see the ContentLength and the CommittedBlockCount of the resulting blob.
        for (var i = 0; i < 1000; i++)
        {
            await stream.WriteAsync(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }, CancellationToken.None);
            await stream.FlushAsync();
        }
    }
}
