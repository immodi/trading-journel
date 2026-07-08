using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappers;
using Application.Requests.Trade;

namespace Infrastructure.Services;

public class TradeService(ITradeRepository repository) : ITradeService
{
    public async Task<IEnumerable<TradeDto>> GetAllAsync()
    {
        var trades = await repository.GetAllAsync();
        return trades.Select(t => t.ToDto());
    }

    public async Task<TradeDto?> GetByIdAsync(int id)
    {
        var trade = await repository.GetByIdAsync(id);
        var dto = trade?.ToDto();
        return dto;
    }

    public async Task AddAsync(CreateTradeRequest request)
    {
        var trade = request.ToEntity();
        await repository.AddAsync(trade);
        await repository.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, UpdateTradeRequest request)
    {
        var trade = await repository.GetByIdAsync(id);

        if (trade is null)
            throw new KeyNotFoundException($"Trade with id {id} was not found.");

        request.UpdateTrade(trade);

        await repository.UpdateAsync(trade);
        await repository.SaveChangesAsync();
    }


    public async Task DeleteAsync(int id)
    {
        await repository.DeleteAsync(id);
        await repository.SaveChangesAsync();
    }
    
}