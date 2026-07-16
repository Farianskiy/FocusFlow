using FocusFlow.Domain.Presets;

namespace FocusFlow.Application.Abstractions.Persistence;

public interface IPomodoroPresetRepository
{
    Task<PomodoroPreset?> GetByIdAsync(
        Guid presetId,
        CancellationToken cancellationToken = default);
}