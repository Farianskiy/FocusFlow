using FocusFlow.Application.Abstractions.Time;

namespace FocusFlow.Infrastructure.Time;

public sealed class SystemAppTimeProvider : IAppTimeProvider
{
    public DateTimeOffset UtcNow =>
        DateTimeOffset.UtcNow;
}