using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IMarketRepository : IRepository<Market>
{
    /// <summary>Returns every market owned by the given user, with its trades loaded.</summary>
    Task<IEnumerable<Market>> GetByUserIdAsync(int userId);

    /// <summary>Returns a market owned by the user with its trades loaded, or <c>null</c>.</summary>
    Task<Market?> GetOwnedAsync(int userId, int id);

    /// <summary>Whether the user already has a market with the given name.</summary>
    Task<bool> NameExistsAsync(int userId, string name, int? excludeId = null);
}
