using Application.DTOs;
using Application.Requests.Trade;

namespace Application.Interfaces.Services;

public interface ITradeService
{
    Task<IEnumerable<TradeDto>> GetAllAsync();
    Task<TradeDto?> GetByIdAsync(int id);
    Task AddAsync(CreateTradeRequest request);
    Task UpdateAsync(int id, UpdateTradeRequest request);
    Task DeleteAsync(int id);
}