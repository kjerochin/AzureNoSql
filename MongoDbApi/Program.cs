using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDbApi
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

            #region Insert Sample Data
            // Create sample documents
            var user1 = new BsonDocument { { "pmfid", "kjerochi" }, { "id", 52491 }, { "City", "Prague" } };
            var user2 = new BsonDocument { { "pmfid", "marekp" }, { "id", 32462 }, { "City", "Prague" } };
            var user3 = new BsonDocument { { "pmfid", "mbogdanova" }, { "id", 29506 }, { "City", "Prague" } };
            var user4 = new BsonDocument { { "pmfid", "anstaske" }, { "id", 72834 }, { "City", "Tallinn" } };
            var user5 = new BsonDocument { { "pmfid", "gregsp" }, { "id", 1732 }, { "City", "Redmond" } };
            List<BsonDocument> users = new List<BsonDocument> { user1, user2, user3, user4, user5 };

            // Insert sample data into the collection
            await collection.InsertManyAsync(users);

            Console.WriteLine("Data uploaded successfully to Azure Cosmos DB MongoDB API.");
            #endregion

            #region Verify Data Insertion
            // Verify data insertion
            var documents = await collection.Find(new BsonDocument()).ToListAsync();

            Console.WriteLine("📌 Verifying Data from Azure Cosmos DB:\n");
            foreach (var doc in documents)
            {
                Console.WriteLine(doc.ToJson());
            }
            #endregion
        }
    }
}