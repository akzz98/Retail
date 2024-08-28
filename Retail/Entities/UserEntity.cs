using Azure.Data.Tables;
using Azure;

namespace Retail.Entities
{
    public class UserEntity : ITableEntity
    {

        //Azure Properties
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Additional properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } 
        public string Role { get; set; } = "User";
    }
}
