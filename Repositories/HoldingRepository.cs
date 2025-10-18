using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioTracker.Data;
using InvestmentPortfolioTracker.Models;

namespace InvestmentPortfolioTracker.Repositories;

public class HoldingRepository : IHoldingRepository
{
    private readonly PortfolioDbContext _context;

    public HoldingRepository(PortfolioDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Holding>> GetAllAsync()
    {
        return await _context.Holdings
            .FromSqlRaw("EXEC sp_GetAllHoldings")
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Holding?> GetByIdAsync(int id)
    {
        var holdings = await _context.Holdings
            .FromSqlRaw("EXEC sp_GetHoldingById @Id={0}", id)
            .AsNoTracking()
            .ToListAsync();
        
        return holdings.FirstOrDefault();
    }

    public async Task<Holding> AddAsync(Holding holding)
    {
        var result = await _context.Holdings
            .FromSqlRaw(@"
                EXEC sp_InsertHolding 
                    @Symbol={0}, 
                    @AssetName={1}, 
                    @AssetType={2}, 
                    @Quantity={3},
                    @PurchasePrice={4}, 
                    @PurchaseDate={5}, 
                    @CurrentPrice={6}, 
                    @LastPriceUpdate={7}",
                holding.Symbol,
                holding.AssetName,
                holding.AssetType,
                holding.Quantity,
                holding.PurchasePrice,
                holding.PurchaseDate,
                holding.CurrentPrice,
                holding.LastPriceUpdate)
            .AsNoTracking()
            .ToListAsync();
        
        return result.First();
    }

    public async Task<Holding> UpdateAsync(Holding holding)
    {
        var result = await _context.Holdings
            .FromSqlRaw(@"
                EXEC sp_UpdateHolding 
                    @Id={0},
                    @Symbol={1}, 
                    @AssetName={2}, 
                    @AssetType={3}, 
                    @Quantity={4},
                    @PurchasePrice={5}, 
                    @PurchaseDate={6}, 
                    @CurrentPrice={7}, 
                    @LastPriceUpdate={8}",
                holding.Id,
                holding.Symbol,
                holding.AssetName,
                holding.AssetType,
                holding.Quantity,
                holding.PurchasePrice,
                holding.PurchaseDate,
                holding.CurrentPrice,
                holding.LastPriceUpdate)
            .AsNoTracking()
            .ToListAsync();
        
        return result.First();
    }

    public async Task<Holding> UpdatePriceAsync(int id, decimal currentPrice)
    {
        var result = await _context.Holdings
            .FromSqlRaw(@"
                EXEC sp_UpdateHoldingPrice 
                    @Id={0}, 
                    @CurrentPrice={1}",
                id,
                currentPrice)
            .AsNoTracking()
            .ToListAsync();
        
        return result.First();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rowsAffected = await _context.Database
            .ExecuteSqlRawAsync("EXEC sp_DeleteHolding @Id={0}", id);
        
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_HoldingExists @Id";
        
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@Id";
        parameter.Value = id;
        command.Parameters.Add(parameter);
        
        var result = await command.ExecuteScalarAsync();
        
        return result != null && Convert.ToBoolean(result);
    }
}