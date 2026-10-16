using Domain.Enum;

namespace Application.Requests.Trade;

public class CreateTradeRequest
{    
    public required string Instrument { get; init; } 

    public required Direction Direction { get; init; }

    public required decimal EntryPrice { get; init; }
    public required decimal ExitPrice { get; init; }
    public required int Quantity { get; init; }

    public required DateTime EntryTime { get; init; }
    public required DateTime ExitTime { get; init; }

    /// <summary>The market this trade belongs to; its currency is inherited by the trade.</summary>
    public required int MarketId { get; init; }

    /// <summary>
    /// Flat commission charged per unit. The server computes the trade's total
    /// commission as <c>CommissionPerUnit × Quantity</c>.
    /// </summary>
    public required decimal CommissionPerUnit { get; init; }
}
