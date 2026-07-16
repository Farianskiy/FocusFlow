using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Sessions;

public sealed record SessionDuration
{
    public const int MinimumMinutes = 1;

    public const int MaximumHours = 12;

    private SessionDuration(TimeSpan value)
    {
        Value = value;
    }

    public TimeSpan Value { get; }

    public static Result<SessionDuration> Create(TimeSpan value)
    {
        if (value < TimeSpan.FromMinutes(MinimumMinutes))
        {
            return Result<SessionDuration>.Failure(
                FocusSessionErrors.DurationIsTooShort);
        }

        if (value > TimeSpan.FromHours(MaximumHours))
        {
            return Result<SessionDuration>.Failure(
                FocusSessionErrors.DurationIsTooLong);
        }

        return Result<SessionDuration>.Success(
            new SessionDuration(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}