using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CacheForRedis
{
    class Program
    {
        private static ConnectionMultiplexer _redis;
        private static IDatabase _db;

        #region Main Method
        static async Task Main(string[] args)
        {
            // Load configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Read settings from configuration
            string redisConnectionString = config["Redis:ConnectionString"];
            string redisKey = config["Redis:KeyToStore"];

            // Initialize Redis Connection
            await InitializeRedis(redisConnectionString);

            // Insert sample data
            await InsertSampleData(redisKey);

            // Read and verify data
            await ReadData(redisKey);
        }
        #endregion

        #region Initialize Redis Connection
        private static async Task InitializeRedis(string connectionString)
        {
            _redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
            _db = _redis.GetDatabase();
            Console.WriteLine("✅ Connected to Azure Cache for Redis");
        }
        #endregion

        #region Insert Sample Data
        private static async Task InsertSampleData(string redisKey)
        {
            string sampleData = "Hello from Azure Cache for Redis!";
            await _db.StringSetAsync(redisKey, sampleData);
            Console.WriteLine($"✅ Data inserted into Redis with key: {redisKey}");
        }
        #endregion

        #region Read Data from Redis
        private static async Task ReadData(string redisKey)
        {
            Console.WriteLine("\n📌 Retrieving data from Azure Cache for Redis:");

            string cachedValue = await _db.StringGetAsync(redisKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                Console.WriteLine($"📨 Retrieved Value: {cachedValue}");
            }
            else
            {
                Console.WriteLine("❌ No data found in Redis for the given key.");
            }

            Console.WriteLine("✅ Data retrieval complete.");
        }
        #endregion
    }
}
