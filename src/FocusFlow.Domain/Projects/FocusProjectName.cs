using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Projects;

public sealed record FocusProjectName
{
    public const int MaximumLength = 80;

    private FocusProjectName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FocusProjectName> Create(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<FocusProjectName>.Failure(
                FocusProjectErrors.NameIsRequired);
        }

        string normalizedValue = Normalize(value);

        if (normalizedValue.Length > MaximumLength)
        {
            return Result<FocusProjectName>.Failure(
                FocusProjectErrors.NameIsTooLong);
        }

        return Result<FocusProjectName>.Success(
            new FocusProjectName(normalizedValue));
    }

    public override string ToString()
    {
        return Value;
    }

    private static string Normalize(string value)
    {
        string[] parts = value.Split(
            [' ', '\t', '\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        return string.Join(" ", parts);
    }
}