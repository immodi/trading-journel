using Application.DTOs;
using Application.Requests.Market;

namespace Application.Interfaces.Services;

public interface IMarketService
{
    Task<IEnumerable<MarketDto>> GetAllAsync(int userId);
    Task<MarketDto?> GetByIdAsync(int userId, int id);
    Task<MarketDto> AddAsync(int userId, CreateMarketRequest request);
    Task UpdateAsync(int userId, int id, UpdateMarketRequest request);
    Task DeleteAsync(int userId, int id);
}
