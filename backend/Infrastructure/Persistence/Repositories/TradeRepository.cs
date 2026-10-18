using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TradeRepository(AppDbContext context) : Repository<Trade>(context), ITradeRepository
{
    public async Task<IEnumerable<Trade>> GetByUserIdAsync(int userId) =>
        await Context.Trades
            .Include(t => t.Market)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.EntryTime)
            .ToListAsync();

    public async Task<Trade?> GetWithMarketAsync(int id) =>
        await Context.Trades
            .Include(t => t.Market)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task AddRangeAsync(IEnumerable<Trade> trades) =>
        await Context.Trades.AddRangeAsync(trades);

    public async Task<bool> AnyForMarketAsync(int marketId) =>
        await Context.Trades.AnyAsync(t => t.MarketId == marketId);
}
