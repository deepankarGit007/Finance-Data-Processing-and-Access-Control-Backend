using Microsoft.EntityFrameworkCore;
using FinanceBackend.Models;
using FinanceBackend.Core;

namespace FinanceBackend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role)
                  .HasConversion<string>()
                  .HasMaxLength(32);
        });

        // ── Transaction ─────────────────────────────────────────────────────
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(t => t.Type)
                  .HasConversion<string>()
                  .HasMaxLength(16);

            entity.HasOne(t => t.CreatedBy)
                  .WithMany(u => u.Transactions)
                  .HasForeignKey(t => t.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Global query filter: never expose soft-deleted rows
            entity.HasQueryFilter(t => !t.IsDeleted);
        });
    }
}
