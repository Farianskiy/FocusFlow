namespace FocusFlow.Application.Abstractions.Time;

public interface IAppTimeProvider
{
    DateTimeOffset UtcNow { get; }
}