using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence;

/// <summary>
/// SQLite has no native date/time type and loses <see cref="DateTimeKind"/> on
/// round-trips. This converter makes sure every value is written as UTC and read
/// back with <see cref="DateTimeKind.Utc"/> so the API always emits ISO-8601 "Z"
/// timestamps.
/// </summary>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            value => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime(),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}

/// <inheritdoc cref="UtcDateTimeConverter"/>
public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter()
        : base(
            value => value.HasValue && value.Value.Kind != DateTimeKind.Utc
                ? value.Value.ToUniversalTime()
                : value,
            value => value.HasValue
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value)
    {
    }
}
