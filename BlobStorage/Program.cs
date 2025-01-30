using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace BlobStorage
{
    class Program
    {
        private static BlobServiceClient _blobServiceClient;
        private static BlobContainerClient _containerClient;

        #region Main Method
        static async Task Main(string[] args)
        {
            // Load configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Read settings from configuration
            string connectionString = config["BlobStorage:ConnectionString"];
            string containerName = config["BlobStorage:ContainerName"];
            string blobName = config["BlobStorage:BlobName"];

            // Initialize Blob Storage Connection
            await InitializeBlobStorage(connectionString, containerName);

            // Upload sample data
            await UploadSampleData(blobName);

            // Read and verify data
            await ReadData(blobName);
        }
        #endregion

        #region Initialize Blob Storage Connection
        private static async Task InitializeBlobStorage(string connectionString, string containerName)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            Console.WriteLine($"✅ Blob Storage container '{containerName}' is ready.");
        }
        #endregion

        #region Upload Sample Data
        private static async Task UploadSampleData(string blobName)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            string sampleContent = "Sample Azure Blob Storage Content";
            byte[] byteArray = Encoding.UTF8.GetBytes(sampleContent);
            using MemoryStream stream = new MemoryStream(byteArray);

            await blobClient.UploadAsync(stream, true);
            Console.WriteLine($"✅ Data uploaded to Blob Storage: {blobName}");
        }
        #endregion

        #region Read Data from Blob Storage
        private static async Task ReadData(string blobName)
        {
            Console.WriteLine("\n📌 Retrieving data from Azure Blob Storage:");

            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            if (await blobClient.ExistsAsync())
            {
                BlobDownloadInfo download = await blobClient.DownloadAsync();
                using StreamReader reader = new StreamReader(download.Content);
                string downloadedContent = await reader.ReadToEndAsync();

                Console.WriteLine($"📨 Retrieved Blob Content: {downloadedContent}");
            }
            else
            {
                Console.WriteLine("❌ No data found in Blob Storage for the given blob.");
            }

            Console.WriteLine("✅ Data retrieval complete.");
        }
        #endregion
    }
}