using Domain.Enum;

namespace Application.DTOs;

public class TradeDto
{
    public int Id { get; init; }

    public int UserId { get; init; }

    public required string Instrument { get; init; }

    public Direction Direction { get; init; }

    public decimal EntryPrice { get; init; }
    public decimal ExitPrice { get; init; }
    public int Quantity { get; init; }

    public DateTime EntryTime { get; init; }
    public DateTime ExitTime { get; init; }

    public decimal Commission { get; init; }
    public decimal ProfitLoss { get; init; }
    
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

