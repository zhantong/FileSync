using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using FileSync.Core.Connectors.Aliyun;
using Microsoft.Extensions.Configuration;

namespace FileSync.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var aliyunClient = new Client(config["Endpoint"], config["AccessKeyId"], config["AccessKeySecret"]);
            var allObjectMeta = aliyunClient.GetAllObjectMeta(config.GetSection("BucketNames").Get<string[]>());
            File.WriteAllText("aliyunResult.json",
                JsonSerializer.Serialize(allObjectMeta,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    }));
        }
    }
}