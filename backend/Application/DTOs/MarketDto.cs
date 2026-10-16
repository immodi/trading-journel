namespace Application.DTOs;

public class MarketDto
{
    public int Id { get; init; }

    public int UserId { get; init; }

    public required string Name { get; init; }

    /// <summary>ISO 4217 currency code inherited by every trade in the market.</summary>
    public required string Currency { get; init; }

    /// <summary>Number of trades currently associated with the market.</summary>
    public int TradeCount { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
