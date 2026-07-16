using FocusFlow.Application.Abstractions.Persistence;
using FocusFlow.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Infrastructure.Persistence.Repositories;

internal sealed class FocusSessionRepository
    : IFocusSessionRepository
{
    private readonly FocusFlowDbContext _dbContext;

    public FocusSessionRepository(
        FocusFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> HasActiveSessionAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext
            .FocusSessions
            .AnyAsync(
                session =>
                    session.Status ==
                        FocusSessionStatus.Running ||
                    session.Status ==
                        FocusSessionStatus.Paused,
                cancellationToken);
    }

    public void Add(
        FocusSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        _dbContext.FocusSessions.Add(session);
    }
}