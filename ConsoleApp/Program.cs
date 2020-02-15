using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using AliyunConnector = FileSync.Core.Connectors.Aliyun;
using AzureConnector = FileSync.Core.Connectors.Azure;
using Microsoft.Extensions.Configuration;

namespace FileSync.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            await RunAzure(config.GetSection("Azure"));
        }

        static void RunAliyun(IConfiguration config)
        {
            var aliyunClient =
                new AliyunConnector.Client(config["Endpoint"], config["AccessKeyId"], config["AccessKeySecret"]);
            var allObjectMeta = aliyunClient.GetAllObjectMeta(config.GetSection("BucketNames").Get<string[]>());
            File.WriteAllText("aliyunResult.json",
                JsonSerializer.Serialize(allObjectMeta,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    }));
        }

        static async Task RunAzure(IConfiguration config)
        {
            var azureClient = new AzureConnector.Client(config["ConnectionString"]);
            var allBlobMeta = await azureClient.GetAllBlobMetaAsync(config["BlobContainerName"]);
            File.WriteAllText("azureResult.json",
                JsonSerializer.Serialize(allBlobMeta,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    }));
        }
    }
}