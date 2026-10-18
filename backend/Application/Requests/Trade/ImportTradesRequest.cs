using System.ComponentModel.DataAnnotations;
using Domain.Enum;

namespace Application.Requests.Trade;

/// <summary>
/// Bulk import of trades into a single market, used by the "import trades"
/// feature. Every trade is filed under <see cref="MarketId"/>, which must
/// belong to the requesting user.
/// </summary>
public class ImportTradesRequest
{
    /// <summary>The market every imported trade is filed under.</summary>
    public required int MarketId { get; init; }

    /// <summary>The trades to add, in the order the export listed them.</summary>
    [Required]
    public required IReadOnlyList<ImportTradeItem> Trades { get; init; }
}

/// <summary>
/// A single trade from an export. The server derives the total commission and
/// profit/loss from the user supplied per-side rate.
/// </summary>
public class ImportTradeItem
{
    public required string Instrument { get; init; }

    public required Direction Direction { get; init; }

    public required int Quantity { get; init; }

    public required decimal EntryPrice { get; init; }
    public required decimal ExitPrice { get; init; }

    public required DateTime EntryTime { get; init; }
    public required DateTime ExitTime { get; init; }

    /// <summary>Commission charged per side; the round trip doubles it.</summary>
    public required decimal CommissionPerSide { get; init; }
}
