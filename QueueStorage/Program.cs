using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace QueueStorage
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
            string connectionString = config["AzureStorage:ConnectionString"];
            string queueName = config["AzureStorage:QueueName"];
            #endregion

            #region Connect to QueueClient
            // Create a QueueClient
            QueueClient queueClient = new QueueClient(connectionString, queueName);
            
            // Ensure the queue exists
            await queueClient.CreateIfNotExistsAsync();
            Console.WriteLine($"✅ Queue '{queueName}' is ready.");
            #endregion

            #region Insert Sample Data
            // Insert sample messages into the queue
            await queueClient.SendMessageAsync("Message 1: Insert to Azure Queue");
            await queueClient.SendMessageAsync("Message 2: Sample Data");
            await queueClient.SendMessageAsync("Message 3: Another Queue Message");

            Console.WriteLine("✅ Messages added to the queue.\n");
            #endregion

            #region Verify Data Insertion
            // Read messages from the queue
            Console.WriteLine("📌 Retrieving messages from the queue:");
            var messages = await queueClient.ReceiveMessagesAsync(maxMessages: 5);

            foreach (var message in messages.Value)
            {
                Console.WriteLine($"📨 Received: {message.MessageText}");

                // Delete the message after reading to prevent re-processing
                await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            }

            Console.WriteLine("\n✅ Messages processed and deleted.");
            #endregion
        }
    }
}
