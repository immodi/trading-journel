using Domain.Enum;

namespace Application.Requests.Trade;

public class UpdateTradeRequest
{    
    public string? Instrument { get; init; } 

    public Direction? Direction { get; init; }

    public decimal? EntryPrice { get; init; }
    public decimal? ExitPrice { get; init; }
    public int? Quantity { get; init; }

    /// <summary>Value of one price point per unit; must be greater than zero.</summary>
    public decimal? PointValue { get; init; }

    public DateTime? EntryTime { get; init; }
    public DateTime? ExitTime { get; init; }

    public int? MarketId { get; init; }

    /// <summary>Flat commission charged per unit; the server derives the total.</summary>
    public decimal? CommissionPerUnit { get; init; }
}
