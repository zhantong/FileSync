using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aliyun.OSS;
using Microsoft.Extensions.Logging;

namespace FileSync.Core.Connectors.Aliyun
{
    public class Client
    {
        private readonly ILogger _logger = CoreLogger.CoreLoggerFactory.CreateLogger<Client>();
        private readonly OssClient _ossClient;

        public Client(string endpoint, string accessKeyId, string accessKeySecret)
        {
            _ossClient = new OssClient(endpoint, accessKeyId, accessKeySecret);
        }

        public IEnumerable<FileMeta> GetAllObjectMeta(string bucketName)
        {
            _logger.LogInformation("Getting all Object meta from bucket: {BucketName}", bucketName);
            var keys = new List<string>();
            var nextMarker = string.Empty;
            ObjectListing result;
            do
            {
                var listObjectRequest = new ListObjectsRequest(bucketName)
                {
                    Marker = nextMarker,
                    MaxKeys = 100
                };
                result = _ossClient.ListObjects(listObjectRequest);
                keys.AddRange(result.ObjectSummaries.Select(summary => summary.Key));
                nextMarker = result.NextMarker;
            } while (result.IsTruncated);

            return GetObjectMetaWithRetry(bucketName, keys);
        }

        public IEnumerable<FileMeta> GetAllObjectMeta(IEnumerable<string> bucketNames)
        {
            _logger.LogInformation("Getting all Object meta from buckets: {BucketNames}", bucketNames);
            var result = new List<FileMeta>();
            foreach (var bucketName in bucketNames)
            {
                result.AddRange(GetAllObjectMeta(bucketName));
            }

            return result;
        }

        private IEnumerable<FileMeta> GetObjectMetaWithRetry(string bucketName, IEnumerable<string> keys)
        {
            var result = new List<FileMeta>();
            var failedKeys = new List<string>();
            foreach (var key in keys)
            {
                try
                {
                    _logger.LogDebug("Getting Object meta for {Key}", key);
                    var fileMeta = GetObjectMeta(bucketName, key);
                    result.Add(fileMeta);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to get Object meta for {Key}", key);
                    failedKeys.Add(key);
                }
            }

            if (failedKeys.Count != 0)
            {
                _logger.LogInformation("Retrying get object meta for failed keys: {Keys}", failedKeys);
                result.AddRange(GetObjectMetaWithRetry(bucketName, failedKeys));
            }

            return result;
        }

        private FileMeta GetObjectMeta(string bucketName, string key)
        {
            var metaData = _ossClient.GetObjectMetadata(bucketName, key);
            return new FileMeta
            {
                Path = key,
                Name = Path.GetFileName(key),
                Size = metaData.ContentLength,
                ETag = metaData.ETag,
                LastModified = metaData.LastModified,
                Crc64 = metaData.Crc64,
                Md5 = metaData.ContentMd5
            };
        }
    }
}