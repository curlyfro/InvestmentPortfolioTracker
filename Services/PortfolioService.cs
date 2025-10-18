using InvestmentPortfolioTracker.Models;
using InvestmentPortfolioTracker.Repositories;

namespace InvestmentPortfolioTracker.Services;

public class PortfolioService
{
    private readonly IHoldingRepository _holdingRepository;

    public PortfolioService(IHoldingRepository holdingRepository)
    {
        _holdingRepository = holdingRepository;
    }

    public async Task<IEnumerable<Holding>> GetAllHoldingsAsync()
    {
        return await _holdingRepository.GetAllAsync();
    }

    public async Task<Holding?> GetHoldingByIdAsync(int id)
    {
        return await _holdingRepository.GetByIdAsync(id);
    }

    public async Task<Holding> AddHoldingAsync(Holding holding)
    {
        // Validation
        ValidateHolding(holding);
        
        holding.CreatedAt = DateTime.UtcNow;
        return await _holdingRepository.AddAsync(holding);
    }

    public async Task<Holding> UpdatePriceAsync(int id, decimal newPrice)
    {
        if (newPrice < 0)
        {
            throw new ArgumentException("Price cannot be negative.", nameof(newPrice));
        }

        // Use optimized stored procedure for price updates
        return await _holdingRepository.UpdatePriceAsync(id, newPrice);
    }

    public async Task<bool> DeleteHoldingAsync(int id)
    {
        return await _holdingRepository.DeleteAsync(id);
    }

    public async Task<PortfolioSummary> GetPortfolioSummaryAsync()
    {
        var holdings = await _holdingRepository.GetAllAsync();
        var holdingsList = holdings.ToList();

        var summary = new PortfolioSummary
        {
            TotalInvested = holdingsList.Sum(h => h.CostBasis),
            CurrentValue = holdingsList.Where(h => h.CurrentValue.HasValue).Sum(h => h.CurrentValue!.Value),
            HoldingsCount = holdingsList.Count,
            HoldingsWithPrices = holdingsList.Count(h => h.CurrentPrice.HasValue)
        };

        summary.TotalGainLoss = summary.CurrentValue - summary.TotalInvested;
        summary.TotalGainLossPercent = summary.TotalInvested > 0 
            ? (summary.TotalGainLoss / summary.TotalInvested) * 100 
            : 0;

        // Get top holdings by current value
        summary.TopHoldings = holdingsList
            .Where(h => h.CurrentValue.HasValue)
            .OrderByDescending(h => h.CurrentValue!.Value)
            .Take(5)
            .ToList();

        // Get best/worst performers
        summary.BestPerformer = holdingsList
            .Where(h => h.GainLossPercent.HasValue)
            .OrderByDescending(h => h.GainLossPercent!.Value)
            .FirstOrDefault();

        summary.WorstPerformer = holdingsList
            .Where(h => h.GainLossPercent.HasValue)
            .OrderBy(h => h.GainLossPercent!.Value)
            .FirstOrDefault();

        return summary;
    }

    private void ValidateHolding(Holding holding)
    {
        if (string.IsNullOrWhiteSpace(holding.Symbol))
        {
            throw new ArgumentException("Symbol is required.", nameof(holding.Symbol));
        }

        if (string.IsNullOrWhiteSpace(holding.AssetName))
        {
            throw new ArgumentException("Asset name is required.", nameof(holding.AssetName));
        }

        if (holding.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(holding.Quantity));
        }

        if (holding.PurchasePrice <= 0)
        {
            throw new ArgumentException("Purchase price must be greater than zero.", nameof(holding.PurchasePrice));
        }

        if (holding.PurchaseDate > DateTime.Now)
        {
            throw new ArgumentException("Purchase date cannot be in the future.", nameof(holding.PurchaseDate));
        }

        var validAssetTypes = new[] { "Stock", "ETF", "Crypto", "Bond" };
        if (!validAssetTypes.Contains(holding.AssetType))
        {
            throw new ArgumentException($"Asset type must be one of: {string.Join(", ", validAssetTypes)}", nameof(holding.AssetType));
        }
    }
}

public class PortfolioSummary
{
    public decimal TotalInvested { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal TotalGainLoss { get; set; }
    public decimal TotalGainLossPercent { get; set; }
    public int HoldingsCount { get; set; }
    public int HoldingsWithPrices { get; set; }
    public List<Holding> TopHoldings { get; set; } = new();
    public Holding? BestPerformer { get; set; }
    public Holding? WorstPerformer { get; set; }
}