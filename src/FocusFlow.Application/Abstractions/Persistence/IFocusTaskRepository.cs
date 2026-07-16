using FocusFlow.Domain.Tasks;

namespace FocusFlow.Application.Abstractions.Persistence;

public interface IFocusTaskRepository
{
    Task<FocusTask?> GetByIdAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);
}