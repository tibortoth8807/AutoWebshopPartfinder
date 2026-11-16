using Microsoft.EntityFrameworkCore;
using PartFinder.Core.Catalog;

namespace PartFinder.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Part> Parts => Set<Part>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>(v =>
        {
            v.ToTable("vehicles");
            v.Property(x => x.Make).HasMaxLength(64);
            v.Property(x => x.Model).HasMaxLength(64);
            v.Property(x => x.Generation).HasMaxLength(64);
            v.Property(x => x.Engine).HasMaxLength(64);
            v.Property(x => x.Fuel).HasConversion<string>().HasMaxLength(16);
            v.Property(x => x.VinWmi).HasMaxLength(3).IsFixedLength();
            v.Property(x => x.VinModelCode).HasMaxLength(8);
            v.Ignore(x => x.DisplayName);
            v.HasIndex(x => new { x.Make, x.Model });
            v.HasIndex(x => x.VinWmi);
        });

        modelBuilder.Entity<Category>(c =>
        {
            c.ToTable("categories");
            c.Property(x => x.Name).HasMaxLength(128);
            c.Property(x => x.Slug).HasMaxLength(128);
            c.Property(x => x.DiagramId).HasMaxLength(32);
            c.HasIndex(x => x.Slug).IsUnique();
            c.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId);
        });

        modelBuilder.Entity<Part>(p =>
        {
            p.ToTable("parts");
            p.Property(x => x.Sku).HasMaxLength(32);
            p.Property(x => x.Name).HasMaxLength(256);
            p.Property(x => x.Brand).HasMaxLength(64);
            p.Property(x => x.Description).HasMaxLength(2000);
            p.Property(x => x.Position).HasMaxLength(32);
            p.Property(x => x.DiagramKey).HasMaxLength(64);
            p.Property(x => x.Price).HasPrecision(12, 2);
            p.HasIndex(x => x.Sku).IsUnique();
            p.HasIndex(x => x.DiagramKey);
            p.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId);

            // Fitment table: which part fits which vehicle
            p.HasMany(x => x.Vehicles)
                .WithMany(x => x.Parts)
                .UsingEntity(j => j.ToTable("part_fitments"));
        });
    }
}
