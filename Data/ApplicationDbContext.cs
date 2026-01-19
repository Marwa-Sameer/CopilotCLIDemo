using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HouseholdNeedsManager.Models;

namespace HouseholdNeedsManager.Data;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Households in the system
    /// </summary>
    public DbSet<Household> Households { get; set; }

    /// <summary>
    /// Household members (join table)
    /// </summary>
    public DbSet<HouseholdMember> HouseholdMembers { get; set; }

    /// <summary>
    /// Categories for items
    /// </summary>
    public DbSet<Category> Categories { get; set; }

    /// <summary>
    /// Items in the system
    /// </summary>
    public DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Household
        modelBuilder.Entity<Household>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Name).IsRequired().HasMaxLength(100);
            entity.Property(h => h.OwnerId).IsRequired();

            entity.HasOne(h => h.Owner)
                .WithMany(u => u.OwnedHouseholds)
                .HasForeignKey(h => h.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(h => h.OwnerId);
        });

        // Configure HouseholdMember
        modelBuilder.Entity<HouseholdMember>(entity =>
        {
            entity.HasKey(hm => hm.Id);
            entity.Property(hm => hm.UserId).IsRequired();
            entity.Property(hm => hm.Role).IsRequired();

            entity.HasOne(hm => hm.User)
                .WithMany(u => u.HouseholdMemberships)
                .HasForeignKey(hm => hm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(hm => hm.Household)
                .WithMany(h => h.Members)
                .HasForeignKey(hm => hm.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(hm => new { hm.UserId, hm.HouseholdId }).IsUnique();
        });

        // Configure Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(50);

            entity.HasOne(c => c.Household)
                .WithMany(h => h.Categories)
                .HasForeignKey(c => c.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => new { c.HouseholdId, c.Name }).IsUnique();
        });

        // Configure Item
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Name).IsRequired().HasMaxLength(100);
            entity.Property(i => i.Quantity).IsRequired();
            entity.Property(i => i.CreatedByUserId).IsRequired();
            entity.Property(i => i.EstimatedPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(i => i.CreatedBy)
                .WithMany(u => u.CreatedItems)
                .HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.Household)
                .WithMany(h => h.Items)
                .HasForeignKey(i => i.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(i => i.CreatedByUserId);
            entity.HasIndex(i => i.HouseholdId);
            entity.HasIndex(i => i.CategoryId);
            entity.HasIndex(i => i.IsUrgent);
            entity.HasIndex(i => i.CreatedAt);
        });

        // Seed roles
        SeedRoles(modelBuilder);
    }

    /// <summary>
    /// Seeds default roles for households
    /// </summary>
    private void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "1",
                Name = "Owner",
                NormalizedName = "OWNER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new IdentityRole
            {
                Id = "2",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new IdentityRole
            {
                Id = "3",
                Name = "Member",
                NormalizedName = "MEMBER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        );
    }
}
