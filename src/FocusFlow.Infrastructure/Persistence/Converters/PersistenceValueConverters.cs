using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FocusFlow.Infrastructure.Persistence.Converters;

internal sealed class DateTimeOffsetToUtcTicksConverter
    : ValueConverter<DateTimeOffset, long>
{
    public DateTimeOffsetToUtcTicksConverter()
        : base(
            value => value.UtcDateTime.Ticks,
            value => new DateTimeOffset(
                value,
                TimeSpan.Zero))
    {
    }
}

internal sealed class NullableDateTimeOffsetToUtcTicksConverter
    : ValueConverter<DateTimeOffset?, long?>
{
    public NullableDateTimeOffsetToUtcTicksConverter()
        : base(
            value =>
                value.HasValue
                    ? value.Value.UtcDateTime.Ticks
                    : null,
            value =>
                value.HasValue
                    ? new DateTimeOffset(
                        value.Value,
                        TimeSpan.Zero)
                    : null)
    {
    }
}

internal sealed class TimeSpanToTicksConverter
    : ValueConverter<TimeSpan, long>
{
    public TimeSpanToTicksConverter()
        : base(
            value => value.Ticks,
            value => TimeSpan.FromTicks(value))
    {
    }
}

internal sealed class NullableTimeSpanToTicksConverter
    : ValueConverter<TimeSpan?, long?>
{
    public NullableTimeSpanToTicksConverter()
        : base(
            value =>
                value.HasValue
                    ? value.Value.Ticks
                    : null,
            value =>
                value.HasValue
                    ? TimeSpan.FromTicks(value.Value)
                    : null)
    {
    }
}