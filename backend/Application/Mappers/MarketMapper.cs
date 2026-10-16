using Application.DTOs;
using Application.Requests.Market;
using Domain.Entities;

namespace Application.Mappers;

public static class MarketMapper
{
    public static MarketDto ToDto(this Market market)
    {
        return new MarketDto
        {
            Id = market.Id,
            UserId = market.UserId,
            Name = market.Name,
            Currency = market.Currency,
            TradeCount = market.Trades.Count,
            CreatedAt = market.CreatedAt,
            UpdatedAt = market.UpdatedAt,
        };
    }

    public static Market ToEntity(this CreateMarketRequest request, int userId)
    {
        return new Market
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Currency = request.Currency.Trim().ToUpperInvariant(),
        };
    }

    public static void UpdateMarket(this UpdateMarketRequest request, Market market)
    {
        if (request.Name is not null)
            market.Name = request.Name.Trim();

        if (request.Currency is not null)
            market.Currency = request.Currency.Trim().ToUpperInvariant();

        market.UpdatedAt = DateTime.UtcNow;
    }
}
