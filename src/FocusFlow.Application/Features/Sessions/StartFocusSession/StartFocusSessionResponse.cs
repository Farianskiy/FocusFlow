namespace FocusFlow.Application.Features.Sessions.StartFocusSession;

public sealed record StartFocusSessionResponse(
    Guid SessionId,
    Guid TaskId,
    Guid PresetId,
    TimeSpan PlannedDuration,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset ExpectedEndAtUtc);