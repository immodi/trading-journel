using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class MarketRepository(AppDbContext context) : Repository<Market>(context), IMarketRepository
{
    public async Task<IEnumerable<Market>> GetByUserIdAsync(int userId) =>
        await Context.Markets
            .Include(m => m.Trades)
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Name)
            .ToListAsync();

    public async Task<Market?> GetOwnedAsync(int userId, int id) =>
        await Context.Markets
            .Include(m => m.Trades)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

    public async Task<bool> NameExistsAsync(int userId, string name, int? excludeId = null) =>
        await Context.Markets
            .AnyAsync(m =>
                m.UserId == userId &&
                m.Name == name &&
                (!excludeId.HasValue || m.Id != excludeId.Value));
}
