using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentPortfolioTracker.Models;

public class Holding
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AssetName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string AssetType { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PurchasePrice { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CurrentPrice { get; set; }

    public DateTime? LastPriceUpdate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Calculated properties
    [NotMapped]
    public decimal CostBasis => Quantity * PurchasePrice;

    [NotMapped]
    public decimal? CurrentValue => CurrentPrice.HasValue ? Quantity * CurrentPrice.Value : null;

    [NotMapped]
    public decimal? GainLoss => CurrentValue.HasValue ? CurrentValue.Value - CostBasis : null;

    [NotMapped]
    public decimal? GainLossPercent => CostBasis > 0 && GainLoss.HasValue ? (GainLoss.Value / CostBasis) * 100 : null;
}