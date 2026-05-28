using Microsoft.EntityFrameworkCore;
using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Data;

public class SavingsStoreContext(DbContextOptions<SavingsStoreContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Goal> Goals => Set<Goal>();

    public DbSet<Deposit> Deposits => Set<Deposit>();

    public DbSet<Withdrawal> Withdrawals => Set<Withdrawal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            // entity.HasIndex(g => g.Name).IsUnique();
            entity.HasIndex(g => new { g.CreatedBy, g.Name }).IsUnique();
            entity.Property(g => g.TargetAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(g => g.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(g => g.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade);  
        });
        modelBuilder.Entity<Deposit>(entity =>
        {
            entity.Property(d => d.Amount).HasColumnType("decimal(18,2)");
            entity.HasOne(d => d.Goal)
                .WithMany(g => g.Deposits)
                .HasForeignKey(d => d.GoalId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Withdrawal>(entity =>
        {
            entity.Property(w => w.Amount).HasColumnType("decimal(18,2)");
            entity.HasOne(w => w.Goal)
                .WithMany(g => g.Withdrawals)
                .HasForeignKey(w => w.GoalId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}