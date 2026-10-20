using Application.DTOs;
using Application.Requests.Trade;
using Domain.Entities;
using Domain.Enum;

namespace Application.Mappers;

public static class TradeMapper
{
    public static TradeDto ToDto(this Trade trade)
    {
        return new TradeDto
        {
            Id = trade.Id,
            UserId = trade.UserId,
            MarketId = trade.MarketId,
            MarketName = trade.Market?.Name ?? string.Empty,
            Currency = trade.Market?.Currency ?? string.Empty,
            Instrument = trade.Instrument,
            EntryTime = trade.EntryTime,
            ExitTime = trade.ExitTime,
            CommissionPerUnit = trade.CommissionPerUnit,
            Commission = trade.Commission,
            Direction = trade.Direction,
            EntryPrice = trade.EntryPrice,
            CreatedAt = trade.CreatedAt,
            UpdatedAt = trade.UpdatedAt,
            ExitPrice = trade.ExitPrice,
            Quantity = trade.Quantity,
            PointValue = trade.PointValue,
            ProfitLoss = trade.ProfitLoss
        };
    }

    public static Trade ToEntity(this CreateTradeRequest request, int userId, Market market)
    {
        var commission = CalculateCommission(request.CommissionPerUnit, request.Quantity);
        return new Trade
        {
            Instrument = request.Instrument,
            UserId = userId,
            MarketId = market.Id,
            Market = market,
            Direction = request.Direction,
            EntryPrice = request.EntryPrice,
            ExitPrice = request.ExitPrice,
            Quantity = request.Quantity,
            PointValue = request.PointValue,
            EntryTime = request.EntryTime,
            ExitTime = request.ExitTime,
            CommissionPerUnit = request.CommissionPerUnit,
            Commission = commission,
            ProfitLoss = CalculateProfitLoss(
                request.Direction, 
                request.EntryPrice, 
                request.ExitPrice, 
                request.Quantity,
                request.PointValue,
                commission)
        };
    }

    /// <summary>
    /// Builds a trade from an imported export row using the export meta's point
    /// value and per-side commission. The stored per-unit rate is the per-side
    /// rate doubled because a round trip is charged on both sides.
    /// </summary>
    public static Trade ToEntity(
        this ImportTradeItem request,
        int userId,
        Market market,
        decimal pointValue,
        decimal commissionPerSide) =>
        new CreateTradeRequest
        {
            Instrument = request.Instrument,
            Direction = request.Direction,
            EntryPrice = request.EntryPrice,
            ExitPrice = request.ExitPrice,
            Quantity = request.Quantity,
            PointValue = pointValue,
            EntryTime = request.EntryTime,
            ExitTime = request.ExitTime,
            MarketId = market.Id,
            CommissionPerUnit = commissionPerSide * CommissionSidesPerRoundTrip,
        }.ToEntity(userId, market);
    
    public static void UpdateTrade(this UpdateTradeRequest request, Trade trade)
    {
        if (request.Instrument is not null)
            trade.Instrument = request.Instrument;

        if (request.Direction.HasValue)
            trade.Direction = request.Direction.Value;

        if (request.EntryPrice.HasValue)
            trade.EntryPrice = request.EntryPrice.Value;

        if (request.ExitPrice.HasValue)
            trade.ExitPrice = request.ExitPrice.Value;

        if (request.Quantity.HasValue)
            trade.Quantity = request.Quantity.Value;

        if (request.PointValue.HasValue)
            trade.PointValue = request.PointValue.Value;

        if (request.EntryTime.HasValue)
            trade.EntryTime = request.EntryTime.Value;

        if (request.ExitTime.HasValue)
            trade.ExitTime = request.ExitTime.Value;

        if (request.MarketId.HasValue)
            trade.MarketId = request.MarketId.Value;

        if (request.CommissionPerUnit.HasValue)
            trade.CommissionPerUnit = request.CommissionPerUnit.Value;

        // The server is the source of truth for the total commission; it is
        // always re-derived from the stored per-unit rate and quantity.
        trade.Commission = CalculateCommission(trade.CommissionPerUnit, trade.Quantity);

        trade.ProfitLoss = CalculateProfitLoss(
            trade.Direction,
            trade.EntryPrice,
            trade.ExitPrice,
            trade.Quantity,
            trade.PointValue,
            trade.Commission);

        trade.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// A round trip pays commission on both the entry and the exit, so a
    /// per-side rate is doubled to get the per-unit rate the server stores.
    /// </summary>
    private const int CommissionSidesPerRoundTrip = 2;

    /// <summary>
    /// Total commission for a trade. The server derives this from the user
    /// supplied per-unit rate so clients never send a total they could tamper
    /// with.
    /// </summary>
    private static decimal CalculateCommission(decimal commissionPerUnit, int quantity) =>
        commissionPerUnit * quantity;

    private static decimal CalculateProfitLoss(
        Direction direction,
        decimal entryPrice,
        decimal exitPrice,
        int quantity,
        decimal pointValue,
        decimal commission)
    {
        var priceMove = direction switch
        {
            Direction.Long => (exitPrice - entryPrice) * quantity,
            Direction.Short => (entryPrice - exitPrice) * quantity,
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };

        return priceMove * pointValue - commission;
    }
}
