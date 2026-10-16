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

    /// <summary>
    /// Total commission for the trade. When omitted the server applies the
    /// default (<c>CommissionPerUnit × Quantity</c>).
    /// </summary>
    public decimal? Commission { get; init; }
}