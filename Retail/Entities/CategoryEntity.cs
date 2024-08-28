using Azure;
using Azure.Data.Tables;

namespace Retail.Entities
{
    public class CategoryEntity : ITableEntity
    {
        public CategoryEntity()
        { }

        public CategoryEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        // Azure Table Storage properties
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Category properties
        public string Name { get; set; }
    }
}