using Microsoft.EntityFrameworkCore;
using SheridanBankingTeamProject.Models.Entities;

namespace SheridanBankingTeamProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Account { get; set; }
    }
}