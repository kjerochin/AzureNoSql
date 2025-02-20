using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AdfDataIntegrationBlobToMongo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region Configuration
            var builder = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();

            // Read settings from configuration
            string connectionString = config["CosmosDb:ConnectionString"];
            string databaseName = config["CosmosDb:DatabaseName"];
            string collectionName = config["CosmosDb:CollectionName"];
            #endregion

            #region Connect to MongoDB
            // Connect to MongoDB
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            #endregion

            #region Verify Data Insertion
            // Verify data insertion
            var documents = await collection.Find(new BsonDocument()).ToListAsync();

            Console.WriteLine("📌 Verifying Data Integration from Azure Cosmos DB:\n");
            foreach (var doc in documents)
            {
                Console.WriteLine(doc.ToJson());
            }
            #endregion
        }
    }
}
