using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappers;
using Application.Requests.Market;
using Domain.Entities;

namespace Infrastructure.Services;

public class MarketService(IMarketRepository repository) : IMarketService
{
    public async Task<IEnumerable<MarketDto>> GetAllAsync(int userId)
    {
        var markets = await repository.GetByUserIdAsync(userId);
        return markets.Select(m => m.ToDto());
    }

    public async Task<MarketDto?> GetByIdAsync(int userId, int id)
    {
        var market = await repository.GetOwnedAsync(userId, id);
        return market?.ToDto();
    }

    public async Task<MarketDto> AddAsync(int userId, CreateMarketRequest request)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        var currency = request.Currency?.Trim() ?? string.Empty;

        Validate(name, currency);

        if (await repository.NameExistsAsync(userId, name))
            throw new ConflictException($"A market named '{name}' already exists.");

        var market = request.ToEntity(userId);

        await repository.AddAsync(market);
        await repository.SaveChangesAsync();

        return market.ToDto();
    }

    public async Task UpdateAsync(int userId, int id, UpdateMarketRequest request)
    {
        var market = await repository.GetOwnedAsync(userId, id)
            ?? throw new KeyNotFoundException($"Market with id {id} was not found.");

        var name = request.Name?.Trim();
        var currency = request.Currency?.Trim();

        Validate(name ?? market.Name, currency ?? market.Currency);

        if (name is not null && await repository.NameExistsAsync(userId, name, id))
            throw new ConflictException($"A market named '{name}' already exists.");

        request.UpdateMarket(market);

        await repository.UpdateAsync(market);
        await repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var market = await repository.GetOwnedAsync(userId, id)
            ?? throw new KeyNotFoundException($"Market with id {id} was not found.");

        // Never silently discard trade history: refuse to delete a market that
        // still has trades and tell the caller how to proceed.
        if (market.Trades.Count > 0)
            throw new ConflictException(
                $"Market '{market.Name}' has {market.Trades.Count} associated trade(s) " +
                "and cannot be deleted. Delete or reassign those trades first.");

        await repository.DeleteAsync(market.Id);
        await repository.SaveChangesAsync();
    }

    private static void Validate(string name, string currency)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Market name is required.");

        if (name.Length > 100)
            throw new ArgumentException("Market name must be 100 characters or fewer.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.");

        if (currency.Length != 3 || !currency.All(char.IsLetter))
            throw new ArgumentException("Currency must be a 3-letter ISO code, for example USD.");
    }
}
