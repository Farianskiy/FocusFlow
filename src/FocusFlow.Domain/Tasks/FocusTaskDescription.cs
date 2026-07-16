using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Tasks;

public sealed record FocusTaskDescription
{
    public const int MaximumLength = 2_000;

    private FocusTaskDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool IsEmpty => Value.Length == 0;

    public static Result<FocusTaskDescription> Create(
        string? value)
    {
        string normalizedValue = Normalize(value);

        if (normalizedValue.Length > MaximumLength)
        {
            return Result<FocusTaskDescription>.Failure(
                FocusTaskErrors.DescriptionIsTooLong);
        }

        return Result<FocusTaskDescription>.Success(
            new FocusTaskDescription(normalizedValue));
    }

    public override string ToString()
    {
        return Value;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value
            .Trim()
            .Replace(
                "\r\n",
                "\n",
                StringComparison.Ordinal)
            .Replace('\r', '\n');
    }
}