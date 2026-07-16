namespace FocusFlow.Domain.Common;

public readonly struct Result
{
    private Result(
        bool isSuccess,
        DomainError error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public DomainError Error { get; }

    public static Result Success()
    {
        return new Result(
            true,
            DomainError.None);
    }

    public static Result Failure(DomainError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error == DomainError.None)
        {
            throw new ArgumentException(
                "Для неуспешного результата необходимо указать ошибку.",
                nameof(error));
        }

        return new Result(
            false,
            error);
    }
}

public readonly struct Result<T>
{
    private readonly T? _value;

    private Result(
        T? value,
        bool isSuccess,
        DomainError error)
    {
        _value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public DomainError Error { get; }

    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException(
                    "Нельзя получить Value из неуспешного результата.");
            }

            return _value!;
        }
    }

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<T>(
            value,
            true,
            DomainError.None);
    }

    public static Result<T> Failure(DomainError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error == DomainError.None)
        {
            throw new ArgumentException(
                "Для неуспешного результата необходимо указать ошибку.",
                nameof(error));
        }

        return new Result<T>(
            default,
            false,
            error);
    }
}