using Microsoft.EntityFrameworkCore;
using Retail.Entities;

namespace Retail.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<CategorySqlEntity> Categories { get; set; }
    }
}