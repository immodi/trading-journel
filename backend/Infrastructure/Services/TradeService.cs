using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappers;
using Application.Requests.Trade;
using Domain.Entities;

namespace Infrastructure.Services;

public class TradeService(
    ITradeRepository repository,
    IMarketRepository marketRepository) : ITradeService
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
        var market = await ResolveMarketAsync(userId, request.MarketId);
        ValidateCommissionPerUnit(request.CommissionPerUnit);

        var trade = request.ToEntity(userId, market);

        await repository.AddAsync(trade);
        await repository.SaveChangesAsync();

        return trade.ToDto();
    }

    public async Task UpdateAsync(int userId, int id, UpdateTradeRequest request)
    {
        var trade = await GetOwnedTradeAsync(userId, id);

        if (request.MarketId.HasValue)
        {
            var market = await ResolveMarketAsync(userId, request.MarketId.Value);
            trade.Market = market;
        }

        if (request.CommissionPerUnit.HasValue)
            ValidateCommissionPerUnit(request.CommissionPerUnit.Value);

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
        var trade = await repository.GetWithMarketAsync(id);

        if (trade is null || trade.UserId != userId)
            throw new KeyNotFoundException($"Trade with id {id} was not found.");

        return trade;
    }

    /// <summary>
    /// Resolves a market that must exist and belong to the requesting user.
    /// </summary>
    private async Task<Market> ResolveMarketAsync(int userId, int marketId)
    {
        var market = await marketRepository.GetByIdAsync(marketId);

        if (market is null || market.UserId != userId)
            throw new KeyNotFoundException($"Market with id {marketId} was not found.");

        return market;
    }

    private static void ValidateCommissionPerUnit(decimal commissionPerUnit)
    {
        if (commissionPerUnit < 0)
            throw new ArgumentException("Commission per unit must be a non-negative number.");
    }
}
