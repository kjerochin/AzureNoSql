using System;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Cassandra;
using Microsoft.Extensions.Configuration;

namespace CassandraApi
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
            string contactPoint = config["CosmosDb:ContactPoint"];
            int port = int.Parse(config["CosmosDb:Port"]);
            string keyspace = config["CosmosDb:Keyspace"];
            string username = config["CosmosDb:Username"];
            string password = config["CosmosDb:Password"];
            string tableName = config["CosmosDb:TableName"];

            // Validate configuration values
            if (string.IsNullOrEmpty(contactPoint) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(keyspace))
            {
                throw new ArgumentNullException("One or more configuration values are null or empty.");
            }

            // Initialize Cassandra session
            using var session = await InitializeCassandraSession(contactPoint, port, username, password, keyspace);
            Console.WriteLine("✅ Connected to Azure Cosmos DB Cassandra API");

            // Ensure the keyspace and table exist
            await InitializeDatabase(session, keyspace, tableName);

            // Insert sample data
            await InsertSampleData(session, keyspace, tableName);

            // Read and verify data
            await ReadData(session, keyspace, tableName);
        }
        #endregion

        #region Initialize Cassandra Session
        private static async Task<ISession> InitializeCassandraSession(string contactPoint, int port, string username, string password, string keyspace)
        {
            var cluster = Cluster.Builder()
                .AddContactPoint(contactPoint)
                .WithPort(port)
                .WithCredentials(username, password)
                .WithSSL() // Cosmos DB requires SSL/TLS
                .Build();

            var session = await cluster.ConnectAsync();
            await session.ExecuteAsync(new SimpleStatement($"USE {keyspace};"));
            return session;
        }
        #endregion

        #region Initialize Database
        private static async Task InitializeDatabase(ISession session, string keyspace, string tableName)
        {
            await session.ExecuteAsync(new SimpleStatement($@"
                    CREATE KEYSPACE IF NOT EXISTS {keyspace}
                    WITH replication = {{'class': 'SimpleStrategy', 'replication_factor': 1 }};
                "));

            await session.ExecuteAsync(new SimpleStatement($@"
                    CREATE TABLE IF NOT EXISTS {keyspace}.{tableName} (
                        id UUID PRIMARY KEY,
                        name TEXT,
                        age INT,
                        city TEXT
                    );
                "));

            Console.WriteLine("✅ Keyspace and Table initialized.");
        }
        #endregion

        #region Insert Sample Data
        private static async Task InsertSampleData(ISession session, string keyspace, string tableName)
        {
            var insertQuery = $"INSERT INTO {keyspace}.{tableName} (id, name, age, city) VALUES (?, ?, ?, ?)";

            var preparedStatement = await session.PrepareAsync(insertQuery);

            var boundStatements = new[]
            {
                    preparedStatement.Bind(Guid.NewGuid(), "kjerochi", 52491, "Prague"),
                    preparedStatement.Bind(Guid.NewGuid(), "marekp", 32462, "Prague"),
                    preparedStatement.Bind(Guid.NewGuid(), "mbogdanova", 29506, "Prague"),
                    preparedStatement.Bind(Guid.NewGuid(), "anstaske",  72834, "Tallinn" ),
                    preparedStatement.Bind(Guid.NewGuid(), "gregsp", 1732, "Redmond") 
            };

            foreach (var boundStatement in boundStatements)
            {
                await session.ExecuteAsync(boundStatement);
            }

            Console.WriteLine("✅ Data inserted successfully into the database.");
        }
        #endregion

        #region Read Data from Cassandra
        private static async Task ReadData(ISession session, string keyspace, string tableName)
        {
            Console.WriteLine("\n📌 Retrieving data from Azure Cosmos DB Cassandra API:");

            var query = $"SELECT * FROM {keyspace}.{tableName}";
            var rows = await session.ExecuteAsync(new SimpleStatement(query));

            foreach (var row in rows)
            {
                Console.WriteLine($"📨 {row.GetValue<Guid>("id")}, {row.GetValue<string>("name")}, " +
                                  $"{row.GetValue<int>("age")}, {row.GetValue<string>("city")}");
            }

            Console.WriteLine("✅ Data retrieval complete.");
        }
        #endregion
    }
}
