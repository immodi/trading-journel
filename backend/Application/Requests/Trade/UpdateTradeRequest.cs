using Domain.Enum;

namespace Application.Requests.Trade;

public class UpdateTradeRequest
{    
    public string? Instrument { get; init; } 

    public Direction? Direction { get; init; }

    public decimal? EntryPrice { get; init; }
    public decimal? ExitPrice { get; init; }
    public int? Quantity { get; init; }

    public DateTime? EntryTime { get; init; }
    public DateTime? ExitTime { get; init; }

    public decimal? Commission { get; init; }
}