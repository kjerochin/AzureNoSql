using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System.Collections.Concurrent;
using System.ComponentModel;
using Container = Microsoft.Azure.Cosmos.Container;
using System.Text.Json;

namespace CoreSqlApi
{
    class Program
    {
        #region Main Method
        static async Task Main(string[] args)
        {
            // Load configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Read settings from configuration
            string endpointUri = config["CosmosDb:EndpointUri"];
            string primaryKey = config["CosmosDb:PrimaryKey"];
            string databaseName = config["CosmosDb:DatabaseName"];
            string containerName = config["CosmosDb:ContainerName"];

            // Initialize Cosmos DB client
            using CosmosClient cosmosClient = new CosmosClient(endpointUri, primaryKey);
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            Container container = await database.CreateContainerIfNotExistsAsync(containerName, "/id");

            Console.WriteLine("✅ Connected to Azure Cosmos DB Core SQL API");

            // Insert sample data
            await InsertSampleData(container);

            // Read and verify data
            await ReadData(container);
        }
        #endregion

        #region Insert Sample Data
        private static async Task InsertSampleData(Container container)
        {
            var user1 = new { pmfid = "kjerochi", id = 52491, City = "Prague" };
            var user2 = new { pmfid = "marekp", id = 32462, City = "Prague" };
            var user3 = new { pmfid = "mbogdanova", id = 29506, City = "Prague" };
            var user4 = new { pmfid = "anstaske", id = 72834, City = "Tallinn" };
            var user5 = new { pmfid = "gregsp", id = 1732, City = "Redmond" };

            await container.UpsertItemAsync(user1, new PartitionKey(user1.id));
            await container.UpsertItemAsync(user2, new PartitionKey(user2.id));
            await container.UpsertItemAsync(user3, new PartitionKey(user3.id));
            await container.UpsertItemAsync(user4, new PartitionKey(user3.id));
            await container.UpsertItemAsync(user5, new PartitionKey(user3.id));

            Console.WriteLine("✅ Data inserted successfully into the database.");
        }
        #endregion

        #region Read Data from Cosmos DB
        private static async Task ReadData(Container container)
        {
            Console.WriteLine("\n📌 Retrieving data from Azure Cosmos DB:");

            var query = new QueryDefinition("SELECT * FROM c");
            using FeedIterator<dynamic> resultSetIterator = container.GetItemQueryIterator<dynamic>(query);

            while (resultSetIterator.HasMoreResults)
            {
                foreach (var item in await resultSetIterator.ReadNextAsync())
                {
                    Console.WriteLine($"📨 {item}");
                }
            }
            Console.WriteLine("✅ Data retrieval complete.");
        }
        #endregion
    }
}
