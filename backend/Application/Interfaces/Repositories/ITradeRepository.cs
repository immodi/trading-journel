using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ITradeRepository : IRepository<Trade>
{
    /// <summary>
    /// Returns every trade owned by the given user, with its market loaded.
    /// </summary>
    Task<IEnumerable<Trade>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Stages several trades for insertion in one batch. Changes are persisted
    /// by the caller via <see cref="IRepository{T}.SaveChangesAsync"/>.
    /// </summary>
    Task AddRangeAsync(IEnumerable<Trade> trades);

    /// <summary>
    /// Returns a single trade with its market loaded, or <c>null</c> when it
    /// does not exist.
    /// </summary>
    Task<Trade?> GetWithMarketAsync(int id);

    /// <summary>Whether any trade is associated with the given market.</summary>
    Task<bool> AnyForMarketAsync(int marketId);
}
