using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TradeRepository(AppDbContext context) : Repository<Trade>(context), ITradeRepository
{
    public async Task<IEnumerable<Trade>> GetByUserIdAsync(int userId) =>
        await Context.Trades
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.EntryTime)
            .ToListAsync();
}
