using Azure.Data.Tables;
using Retail.Entities;

namespace Retail.Services
{
    public class UserStorageService
    {
        private readonly TableClient _tableClient;

        public UserStorageService(string connectionString, string tableName)
        {
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists(); 
        }

        public async Task AddUserAsync(UserEntity user)
        {
            user.RowKey = Guid.NewGuid().ToString(); 
            await _tableClient.AddEntityAsync(user);
        }

        public async Task<UserEntity> GetUserByUsernameAsync(string username)
        {
            var queryResults = _tableClient.QueryAsync<UserEntity>(u => u.Username == username);
            await foreach (var user in queryResults)
            {
                return user;
            }
            return null;
        }

        public async Task<UserEntity> ValidateUserAsync(string username, string passwordHash)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user != null && user.PasswordHash == passwordHash)
            {
                return user;
            }
            return null;
        }
    }
}