using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace FileSync.Core.Connectors.Azure
{
    public class Client
    {
        private readonly ILogger _logger = CoreLogger.CoreLoggerFactory.CreateLogger<Client>();

        private BlobServiceClient blobServiceClient;

        public Client(string connectionString)
        {
            blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<IEnumerable<FileMeta>> GetAllBlobMetaAsync(string blobContainerName)
        {
            var result = new List<FileMeta>();

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
            await foreach (var item in blobContainerClient.GetBlobsAsync())
            {
                _logger.LogDebug("Getting Blob meta for {Key}", item.Name);
                result.Add(new FileMeta
                {
                    Path = item.Name,
                    Name = Path.GetFileName(item.Name),
                    Size = (long) item.Properties.ContentLength,
                    ETag = item.Properties.ETag.ToString(),
                    LastModified = item.Properties.LastModified.Value,
                    Md5 = BitConverter.ToString(item.Properties.ContentHash).Replace("-", "")
                });
            }

            return result;
        }
    }
}