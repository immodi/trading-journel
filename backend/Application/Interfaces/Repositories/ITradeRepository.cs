using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ITradeRepository : IRepository<Trade>
{
    /// <summary>
    /// Returns every trade owned by the given user.
    /// </summary>
    Task<IEnumerable<Trade>> GetByUserIdAsync(int userId);
}
