using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Tasks;

public sealed record FocusTaskTitle
{
    public const int MaximumLength = 120;

    private FocusTaskTitle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FocusTaskTitle> Create(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<FocusTaskTitle>.Failure(
                FocusTaskErrors.TitleIsRequired);
        }

        string normalizedValue = Normalize(value);

        if (normalizedValue.Length > MaximumLength)
        {
            return Result<FocusTaskTitle>.Failure(
                FocusTaskErrors.TitleIsTooLong);
        }

        return Result<FocusTaskTitle>.Success(
            new FocusTaskTitle(normalizedValue));
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