using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Tasks;

public sealed record PomodoroEstimate
{
    public const int MinimumValue = 1;

    public const int MaximumValue = 1_000;

    private PomodoroEstimate(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<PomodoroEstimate> Create(
        int value)
    {
        if (value < MinimumValue)
        {
            return Result<PomodoroEstimate>.Failure(
                FocusTaskErrors.EstimateIsTooSmall);
        }

        if (value > MaximumValue)
        {
            return Result<PomodoroEstimate>.Failure(
                FocusTaskErrors.EstimateIsTooLarge);
        }

        return Result<PomodoroEstimate>.Success(
            new PomodoroEstimate(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}