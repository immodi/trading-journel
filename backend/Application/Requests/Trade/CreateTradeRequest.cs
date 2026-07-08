using Domain.Enum;

namespace Application.Requests.Trade;

public class CreateTradeRequest
{    
    public required string Instrument { get; init; } 
    public required int UserId { get; init; } 

    public required Direction Direction { get; init; }

    public required decimal EntryPrice { get; init; }
    public required decimal ExitPrice { get; init; }
    public required int Quantity { get; init; }

    public required DateTime EntryTime { get; init; }
    public required DateTime ExitTime { get; init; }
    // public required decimal Commission { get; init; }
}