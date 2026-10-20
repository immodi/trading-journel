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

    /// <summary>
    /// Value of one price point per unit from the export's <c>meta.pointValue</c>.
    /// Defaults to 1 for exports that predate the field (for example shares).
    /// </summary>
    public decimal PointValue { get; init; } = 1m;

    /// <summary>
    /// Flat commission charged per side of a round trip, from the export's
    /// <c>meta.commissionPerSide</c>. Defaults to zero when omitted.
    /// </summary>
    public decimal CommissionPerSide { get; init; }

    /// <summary>The trades to add, in the order the export listed them.</summary>
    [Required]
    public required IReadOnlyList<ImportTradeItem> Trades { get; init; }
}

/// <summary>
/// A single trade from an export. The server derives the total commission and
/// profit/loss from the batch-level point value and per-side commission.
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
}
