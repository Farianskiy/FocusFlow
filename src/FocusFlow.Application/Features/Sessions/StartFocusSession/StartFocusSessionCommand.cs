namespace FocusFlow.Application.Features.Sessions.StartFocusSession;

public sealed record StartFocusSessionCommand(
    Guid TaskId,
    Guid PresetId);