using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.API.Domain;
using ServiceDesk.API.Infrastructure.Identity;

namespace ServiceDesk.API.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionOperation> TransactionOperations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TransactionOperation>()
    .HasOne(to => to.Transaction)
    .WithMany()
    .HasForeignKey(to => to.TransactionId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TransactionOperation>()
    .HasOne(to => to.User)
    .WithMany()
    .HasForeignKey(to => to.UserId)
    .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TransactionOperation>()
    .HasOne(to => to.Assignee)
    .WithMany()
    .HasForeignKey(to => to.AssigneeId)
    .OnDelete(DeleteBehavior.Restrict);
        base.OnModelCreating(builder);

        builder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        builder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 Исправленная связь для контролёра
        builder.Entity<Transaction>()
            .HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<Transaction>()
            .HasIndex(t => t.TransactionDate);
    }
}