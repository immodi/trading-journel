using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappers;
using Application.Requests.Trade;
using Domain.Entities;
using Domain.Enum;

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
        ValidatePointValue(request.PointValue);

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

        if (request.PointValue.HasValue)
            ValidatePointValue(request.PointValue.Value);

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

    public async Task<int> ImportAsync(int userId, ImportTradesRequest request)
    {
        // Validating the market up front guarantees every trade in the batch is
        // filed under a market the caller owns before anything is written.
        var market = await ResolveMarketAsync(userId, request.MarketId);

        if (request.Trades.Count == 0)
            return 0;

        if (request.Trades.Count > MaxImportSize)
            throw new ArgumentException(
                $"A single import can contain at most {MaxImportSize} trades.");

        ValidatePointValue(request.PointValue);
        ValidateCommissionPerSide(request.CommissionPerSide);

        var trades = new List<Trade>(request.Trades.Count);

        foreach (var item in request.Trades)
        {
            ValidateImportItem(item);
            trades.Add(item.ToEntity(userId, market, request.PointValue, request.CommissionPerSide));
        }

        await repository.AddRangeAsync(trades);
        await repository.SaveChangesAsync();

        return trades.Count;
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

    private static void ValidateCommissionPerSide(decimal commissionPerSide)
    {
        if (commissionPerSide < 0)
            throw new ArgumentException("Commission per side must be a non-negative number.");
    }

    private static void ValidatePointValue(decimal pointValue)
    {
        if (pointValue <= 0)
            throw new ArgumentException("Point value must be a number greater than zero.");
    }

    private static void ValidateImportItem(ImportTradeItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Instrument))
            throw new ArgumentException("Instrument is required.");

        if (item.Direction is not (Direction.Long or Direction.Short))
            throw new ArgumentException("Direction must be Long or Short.");

        if (item.Quantity <= 0)
            throw new ArgumentException("Quantity must be a whole number greater than zero.");

        if (item.EntryPrice < 0 || item.ExitPrice < 0)
            throw new ArgumentException("Prices must be non-negative numbers.");

        if (item.ExitTime < item.EntryTime)
            throw new ArgumentException("Exit time cannot be before the entry time.");
    }

    /// <summary>Upper bound on a single import to keep request payloads sane.</summary>
    private const int MaxImportSize = 5000;
}
