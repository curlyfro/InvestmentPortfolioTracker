using InvestmentPortfolioTracker.Models;

namespace InvestmentPortfolioTracker.Repositories;

public interface IHoldingRepository
{
    Task<IEnumerable<Holding>> GetAllAsync();
    Task<Holding?> GetByIdAsync(int id);
    Task<Holding> AddAsync(Holding holding);
    Task<Holding> UpdateAsync(Holding holding);
    Task<Holding> UpdatePriceAsync(int id, decimal currentPrice);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}