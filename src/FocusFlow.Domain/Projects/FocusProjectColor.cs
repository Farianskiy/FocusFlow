using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Projects;

public sealed record FocusProjectColor
{
    public const string DefaultValue = "#6750A4";

    private FocusProjectColor(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FocusProjectColor> Create(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<FocusProjectColor>.Success(
                new FocusProjectColor(DefaultValue));
        }

        string normalizedValue =
            value.Trim().ToUpperInvariant();

        if (normalizedValue[0] != '#')
        {
            normalizedValue = $"#{normalizedValue}";
        }

        if (!IsValidHexColor(normalizedValue))
        {
            return Result<FocusProjectColor>.Failure(
                FocusProjectErrors.ColorIsInvalid);
        }

        return Result<FocusProjectColor>.Success(
            new FocusProjectColor(normalizedValue));
    }

    public override string ToString()
    {
        return Value;
    }

    private static bool IsValidHexColor(
        string value)
    {
        if (value.Length != 7 || value[0] != '#')
            return false;

        return value
            .Skip(1)
            .All(Uri.IsHexDigit);
    }
}