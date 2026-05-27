using Microsoft.EntityFrameworkCore;
using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Data;

public class SavingsStoreContext(DbContextOptions<SavingsStoreContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
        });
    }
}