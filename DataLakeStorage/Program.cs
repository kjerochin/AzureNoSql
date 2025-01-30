using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Microsoft.Extensions.Configuration;

namespace DataLakeStorage
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region Load Configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string storageAccountName = config["AzureDataLake:StorageAccountName"];
            string storageAccountKey = config["AzureDataLake:StorageAccountKey"];
            string fileSystemName = config["AzureDataLake:FileSystemName"];
            string directoryName = config["AzureDataLake:DirectoryName"];
            string fileName = "sample.txt";
            string fileContent = "Hello, Azure Data Lake!";
            #endregion

            #region Create DataLakeServiceClient
            string dfsUri = $"https://{storageAccountName}.dfs.core.windows.net";
            var serviceClient = new DataLakeServiceClient(new Uri(dfsUri), new StorageSharedKeyCredential(storageAccountName, storageAccountKey));
            #endregion

            #region Create FileSystem, Directory, and Upload File
            var fileSystemClient = serviceClient.GetFileSystemClient(fileSystemName);
            await fileSystemClient.CreateIfNotExistsAsync();
            Console.WriteLine($"✅ File System '{fileSystemName}' is ready.");

            var directoryClient = fileSystemClient.GetDirectoryClient(directoryName);
            await directoryClient.CreateIfNotExistsAsync();
            Console.WriteLine($"✅ Directory '{directoryName}' is ready.");

            var fileClient = directoryClient.GetFileClient(fileName);
            using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)))
            {
                await fileClient.UploadAsync(stream, overwrite: true);
            }
            Console.WriteLine("✅ File uploaded successfully.");
            #endregion

            #region Read File Content
            Console.WriteLine("\n📌 Retrieving File from Azure Data Lake:");
            var downloadResponse = await fileClient.ReadAsync();
            using (StreamReader reader = new StreamReader(downloadResponse.Value.Content))
            {
                string content = await reader.ReadToEndAsync();
                Console.WriteLine($"📄 File Content: {content}");
            }
            #endregion
        }
    }
}
