using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Presets;

public sealed record PomodoroPresetName
{
    public const int MaximumLength = 60;

    private PomodoroPresetName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PomodoroPresetName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<PomodoroPresetName>.Failure(
                PomodoroPresetErrors.NameIsRequired);
        }

        string normalizedValue = Normalize(value);

        if (normalizedValue.Length > MaximumLength)
        {
            return Result<PomodoroPresetName>.Failure(
                PomodoroPresetErrors.NameIsTooLong);
        }

        return Result<PomodoroPresetName>.Success(
            new PomodoroPresetName(normalizedValue));
    }

    public override string ToString()
    {
        return Value;
    }

    private static string Normalize(string value)
    {
        string[] parts = value.Split(
            new[] { ' ', '\t', '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        return string.Join(" ", parts);
    }
}