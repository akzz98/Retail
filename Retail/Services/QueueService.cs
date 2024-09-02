using Azure.Storage.Queues;
using System.Text.Json;

namespace Retail.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(string connectionString, string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task SendMessageAsync(InventoryUpdateMessage message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonMessage)));
        }
    }

    public class InventoryUpdateMessage
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public int Quantity { get; set; }
    }
}