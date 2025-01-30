using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Messages;
using Gremlin.Net.Structure.IO.GraphSON;
using System.Collections.Generic;

namespace GremlinApi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region Configuration
            // Load configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Read settings from configuration
            string hostname = config["CosmosDb:Hostname"];
            string databaseName = config["CosmosDb:DatabaseName"];
            string primaryKey = config["CosmosDb:PrimaryKey"];
            #endregion

            #region Connect to Gremlin
            // Set up Gremlin client connection
            var gremlinServer = new GremlinServer(hostname, 443, enableSsl: true,
                username: "/dbs/" + databaseName + "/colls/GraphCollection",
                password: primaryKey);

            using var gremlinClient = new GremlinClient(
                gremlinServer,
                new GraphSON2Reader(),
                new GraphSON2Writer(),
                "application/vnd.gremlin-v2.0+json");

            Console.WriteLine("✅ Connected to Azure Cosmos DB (Gremlin API)");
            #endregion

            #region Insert Sample Data
            // Define Gremlin queries to insert data
            var gremlinQueries = new Dictionary<string, string>
            {
                { "AddVertex1", "g.addV('person').property('pmfid', 'kjerochi').property('id', 52491).property('city', 'Prague')" },
                { "AddVertex2", "g.addV('person').property('pmfid', 'marekp').property('id', 32462).property('city', 'Prague')" },
                { "AddVertex3", "g.addV('person').property('pmfid', 'mbogdanova').property('id', 29506).property('city', 'Prague')" },
                { "AddVertex4", "g.addV('person').property('pmfid', 'anstaske').property('id', 72834).property('city', 'Tallinn')" },
                { "AddVertex5", "g.addV('person').property('pmfid', 'gregsp').property('id', 1732).property('city', 'Redmond')" }
            };


            // Insert data into the graph
            foreach (var query in gremlinQueries)
            {
                await gremlinClient.SubmitAsync<dynamic>(query.Value);
                Console.WriteLine($"✅ {query.Key} inserted successfully.");
            }
            #endregion

            #region Verify Data Insertion
            // Read data to verify insertion
            var readQuery = "g.V().valueMap(true)";
            var results = await gremlinClient.SubmitAsync<dynamic>(readQuery);

            Console.WriteLine("\n📌 Verifying Data from Azure Cosmos DB (Gremlin API):\n");
            foreach (var result in results)
            {
                Console.WriteLine(result);
            }
            #endregion
        }
    }
}