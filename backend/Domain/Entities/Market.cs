namespace Domain.Entities;

/// <summary>
/// A trading market (for example "US Equities") owned by a user. Every market
/// carries a single currency that applies to all of its trades.
/// </summary>
public class Market
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>ISO 4217 currency code (for example <c>USD</c>) inherited by the market's trades.</summary>
    public string Currency { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Trade> Trades { get; set; } = [];
}
