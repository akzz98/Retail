using Azure;
using Azure.Data.Tables;

namespace Retail.Models
{
    public class ProductEntity : ITableEntity
    {
        public ProductEntity()
        { }

        public ProductEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        //Azure Properties
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Product properties
        public string Name { get; set; }

        public double Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public string CategoryRowKey { get; set; }
    }
}