using System;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TableApi
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

            string connectionString = config["AzureTable:ConnectionString"];
            string tableName = config["AzureTable:TableName"];
            #endregion

            #region Create Table Client
            TableServiceClient serviceClient = new TableServiceClient(connectionString);
            TableClient tableClient = serviceClient.GetTableClient(tableName);
            await tableClient.CreateIfNotExistsAsync();
            Console.WriteLine($"✅ Table '{tableName}' is ready.");
            #endregion

            #region Insert Data
            var entity1 = new TableEntity("Prague", "Row1") { { "pmfid", "kjerochi" }, { "id", 52491 }, { "City", "Prague" } };
            var entity2 = new TableEntity("Prague", "Row2") { { "pmfid", "marekp" }, { "id", 32462 }, { "City", "Prague" } };
            var entity3 = new TableEntity("Prague", "Row3") { { "pmfid", "mbogdanova" }, { "id", 29506 }, { "City", "Prague" } };
            var entity4 = new TableEntity("Tallinn", "Row4") { { "pmfid", "anstaske" }, { "id", 72834 }, { "City", "Tallinn" } };
            var entity5 = new TableEntity("Redmond", "Row5") { { "pmfid", "gregsp" }, { "id", 1732 }, { "City", "Redmond" } };

            await tableClient.UpsertEntityAsync(entity1);
            await tableClient.UpsertEntityAsync(entity2);
            await tableClient.UpsertEntityAsync(entity3);
            await tableClient.UpsertEntityAsync(entity4);
            await tableClient.UpsertEntityAsync(entity5);

            Console.WriteLine("✅ Data inserted successfully.");
            #endregion

            #region Retrieve Data
            Console.WriteLine("\n📌 Retrieving Data from Azure Table API:");
            await foreach (var entity in tableClient.QueryAsync<TableEntity>())
            {
                Console.WriteLine($"📝 {entity.RowKey} - {entity["Name"]}, Age: {entity["Age"]}, City: {entity["City"]}");
            }
            #endregion
        }
    }
}
