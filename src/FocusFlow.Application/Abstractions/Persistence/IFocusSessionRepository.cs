using FocusFlow.Domain.Sessions;

namespace FocusFlow.Application.Abstractions.Persistence;

public interface IFocusSessionRepository
{
    Task<bool> HasActiveSessionAsync(
        CancellationToken cancellationToken = default);

    void Add(FocusSession session);
}