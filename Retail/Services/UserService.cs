using Microsoft.EntityFrameworkCore;
using Retail.Data;
using Retail.Entities;

namespace Retail.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddUserAsync(UserEntity user)
        {
            user.Id = Guid.NewGuid();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserEntity> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
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