using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Projects;

public sealed record FocusProjectDescription
{
    public const int MaximumLength = 2_000;

    private FocusProjectDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool IsEmpty => Value.Length == 0;

    public static Result<FocusProjectDescription> Create(
        string? value)
    {
        string normalizedValue = Normalize(value);

        if (normalizedValue.Length > MaximumLength)
        {
            return Result<FocusProjectDescription>.Failure(
                FocusProjectErrors.DescriptionIsTooLong);
        }

        return Result<FocusProjectDescription>.Success(
            new FocusProjectDescription(normalizedValue));
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