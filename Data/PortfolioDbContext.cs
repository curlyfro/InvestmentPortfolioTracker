using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioTracker.Models;

namespace InvestmentPortfolioTracker.Data;

public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
        : base(options)
    {
    }

    public DbSet<Holding> Holdings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Holding>(entity =>
        {
            entity.ToTable("Holdings");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Symbol)
                .IsRequired()
                .HasMaxLength(10);
            
            entity.Property(e => e.AssetName)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.AssetType)
                .IsRequired()
                .HasMaxLength(20);
            
            entity.Property(e => e.Quantity)
                .IsRequired()
                .HasColumnType("decimal(18,8)");
            
            entity.Property(e => e.PurchasePrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            
            entity.Property(e => e.PurchaseDate)
                .IsRequired();
            
            entity.Property(e => e.CurrentPrice)
                .HasColumnType("decimal(18,2)");
            
            entity.Property(e => e.LastPriceUpdate);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Create index on Symbol for faster lookups
            entity.HasIndex(e => e.Symbol);
        });
    }
}