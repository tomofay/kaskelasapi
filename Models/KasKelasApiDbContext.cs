using Microsoft.EntityFrameworkCore;
using KasKelasApi.Models;

public class KasKelasApiDbContext : DbContext
{
    public KasKelasApiDbContext(DbContextOptions<KasKelasApiDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<KasSetting> KasSettings { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User-Parent (self reference)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Parent)
            .WithMany(u => u.Children)
            .HasForeignKey(u => u.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // User-Class
        modelBuilder.Entity<User>()
            .HasOne(u => u.Class)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.ClassId)
            .OnDelete(DeleteBehavior.SetNull);

        // Payment-User
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Payment-Verifier (User)
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Verifier)
            .WithMany()
            .HasForeignKey(p => p.VerifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Expense-Class
        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Class)
            .WithMany()
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        // Expense-Creator (User)
        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Creator)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // KasSetting-Class
        modelBuilder.Entity<KasSetting>()
            .HasOne(k => k.Class)
            .WithMany()
            .HasForeignKey(k => k.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        // Notification-User
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ActivityLog-User
        modelBuilder.Entity<ActivityLog>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        base.OnModelCreating(modelBuilder);
    }
}
