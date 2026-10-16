using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappers;
using Application.Requests.Trade;
using Domain.Entities;

namespace Infrastructure.Services;

public class TradeService(ITradeRepository repository) : ITradeService
{
    public async Task<IEnumerable<TradeDto>> GetAllAsync(int userId)
    {
        var trades = await repository.GetByUserIdAsync(userId);
        return trades.Select(t => t.ToDto());
    }

    public async Task<TradeDto?> GetByIdAsync(int userId, int id)
    {
        var trade = await GetOwnedTradeAsync(userId, id);
        return trade.ToDto();
    }

    public async Task<TradeDto> AddAsync(int userId, CreateTradeRequest request)
    {
        var trade = request.ToEntity(userId);

        await repository.AddAsync(trade);
        await repository.SaveChangesAsync();

        return trade.ToDto();
    }

    public async Task UpdateAsync(int userId, int id, UpdateTradeRequest request)
    {
        var trade = await GetOwnedTradeAsync(userId, id);

        request.UpdateTrade(trade);

        await repository.UpdateAsync(trade);
        await repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var trade = await GetOwnedTradeAsync(userId, id);

        await repository.DeleteAsync(trade.Id);
        await repository.SaveChangesAsync();
    }

    /// <summary>
    /// Loads a trade and guarantees it belongs to the requesting user. A trade
    /// owned by somebody else is reported as not found so the API never leaks
    /// the existence of another user's records.
    /// </summary>
    private async Task<Trade> GetOwnedTradeAsync(int userId, int id)
    {
        var trade = await repository.GetByIdAsync(id);

        if (trade is null || trade.UserId != userId)
            throw new KeyNotFoundException($"Trade with id {id} was not found.");

        return trade;
    }
}
