
using DuckDB.EFCore;
using DuckDB.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DuckDbExcelImport
{
    public class AppDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {            
            optionsBuilder.UseDuckDB("Data Source=analytics_data.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>().HasKey(t => t.Id);
            modelBuilder.Entity<Transaction>().Property(t => t.Id).ValueGeneratedOnAdd();
        }
    }
}
