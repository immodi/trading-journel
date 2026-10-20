using Domain.Enum;

namespace Application.DTOs;

public class TradeDto
{
    public int Id { get; init; }

    public int UserId { get; init; }

    public int MarketId { get; init; }

    public string MarketName { get; init; } = string.Empty;

    /// <summary>ISO 4217 currency code inherited from the trade's market.</summary>
    public string Currency { get; init; } = string.Empty;

    public required string Instrument { get; init; }

    public Direction Direction { get; init; }

    public decimal EntryPrice { get; init; }
    public decimal ExitPrice { get; init; }
    public int Quantity { get; init; }

    /// <summary>Value of one price point per unit; multiplies the price move.</summary>
    public decimal PointValue { get; init; }

    public DateTime EntryTime { get; init; }
    public DateTime ExitTime { get; init; }

    /// <summary>Flat commission charged per unit.</summary>
    public decimal CommissionPerUnit { get; init; }

    /// <summary>Total commission, computed by the server as <c>CommissionPerUnit × Quantity</c>.</summary>
    public decimal Commission { get; init; }
    public decimal ProfitLoss { get; init; }
    
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
