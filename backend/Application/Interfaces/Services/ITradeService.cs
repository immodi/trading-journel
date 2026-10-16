using Application.DTOs;
using Application.Requests.Trade;

namespace Application.Interfaces.Services;

public interface ITradeService
{
    Task<IEnumerable<TradeDto>> GetAllAsync(int userId);
    Task<TradeDto?> GetByIdAsync(int userId, int id);
    Task<TradeDto> AddAsync(int userId, CreateTradeRequest request);
    Task UpdateAsync(int userId, int id, UpdateTradeRequest request);
    Task DeleteAsync(int userId, int id);
}
