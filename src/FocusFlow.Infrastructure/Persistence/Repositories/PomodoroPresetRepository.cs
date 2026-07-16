using FocusFlow.Application.Abstractions.Persistence;
using FocusFlow.Domain.Presets;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Infrastructure.Persistence.Repositories;

internal sealed class PomodoroPresetRepository
    : IPomodoroPresetRepository
{
    private readonly FocusFlowDbContext _dbContext;

    public PomodoroPresetRepository(
        FocusFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PomodoroPreset?> GetByIdAsync(
        Guid presetId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext
            .PomodoroPresets
            .SingleOrDefaultAsync(
                preset => preset.Id == presetId,
                cancellationToken);
    }
}