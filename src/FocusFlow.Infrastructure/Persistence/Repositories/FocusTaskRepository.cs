using FocusFlow.Application.Abstractions.Persistence;
using FocusFlow.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Infrastructure.Persistence.Repositories;

internal sealed class FocusTaskRepository
    : IFocusTaskRepository
{
    private readonly FocusFlowDbContext _dbContext;

    public FocusTaskRepository(
        FocusFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<FocusTask?> GetByIdAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext
            .FocusTasks
            .SingleOrDefaultAsync(
                task => task.Id == taskId,
                cancellationToken);
    }
}