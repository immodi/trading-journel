namespace Application.Requests.Market;

public class CreateMarketRequest
{
    public required string Name { get; init; }

    /// <summary>ISO 4217 currency code, for example <c>USD</c>.</summary>
    public required string Currency { get; init; }
}
