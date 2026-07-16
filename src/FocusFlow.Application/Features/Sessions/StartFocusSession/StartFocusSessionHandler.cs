using FocusFlow.Application.Abstractions.Persistence;
using FocusFlow.Application.Abstractions.Time;
using FocusFlow.Domain.Common;
using FocusFlow.Domain.Presets;
using FocusFlow.Domain.Sessions;
using FocusFlow.Domain.Tasks;

namespace FocusFlow.Application.Features.Sessions.StartFocusSession;

public sealed class StartFocusSessionHandler
{
    private readonly IFocusTaskRepository _taskRepository;
    private readonly IPomodoroPresetRepository _presetRepository;
    private readonly IFocusSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppTimeProvider _timeProvider;

    public StartFocusSessionHandler(
        IFocusTaskRepository taskRepository,
        IPomodoroPresetRepository presetRepository,
        IFocusSessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        IAppTimeProvider timeProvider)
    {
        _taskRepository = taskRepository;
        _presetRepository = presetRepository;
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<StartFocusSessionResponse>> Handle(
        StartFocusSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.TaskId == Guid.Empty)
        {
            return Result<StartFocusSessionResponse>.Failure(
                StartFocusSessionErrors.TaskIdIsInvalid);
        }

        if (command.PresetId == Guid.Empty)
        {
            return Result<StartFocusSessionResponse>.Failure(
                StartFocusSessionErrors.PresetIdIsInvalid);
        }

        FocusTask? focusTask =
            await _taskRepository.GetByIdAsync(
                command.TaskId,
                cancellationToken);

        if (focusTask is null)
        {
            return Result<StartFocusSessionResponse>.Failure(
                StartFocusSessionErrors.TaskNotFound(
                    command.TaskId));
        }

        if (focusTask.Status is not (
            FocusTaskStatus.Planned or
            FocusTaskStatus.InProgress))
        {
            return Result<StartFocusSessionResponse>.Failure(
                StartFocusSessionErrors.TaskCannotBeFocused(
                    focusTask.Status));
        }

        PomodoroPreset? preset =
            await _presetRepository.GetByIdAsync(
                command.PresetId,
                cancellationToken);

        if (preset is null)
        {
            return Result<StartFocusSessionResponse>.Failure(
                StartFocusSessionErrors.PresetNotFound(
                    command.PresetId));
        }

        bool hasActiveSession =
            await _sessionRepository.HasActiveSessionAsync(
                cancellationToken);

        if (hasActiveSession)
        {
            return Result<StartFocusSessionResponse>.Failure(
                StartFocusSessionErrors
                    .ActiveSessionAlreadyExists);
        }

        Result<TimeSpan> durationResult =
            preset.GetDurationFor(
                FocusSessionType.Focus);

        if (durationResult.IsFailure)
        {
            return Result<StartFocusSessionResponse>.Failure(
                durationResult.Error);
        }

        DateTimeOffset startedAtUtc =
            _timeProvider.UtcNow.ToUniversalTime();

        Result<FocusSession> sessionResult =
            FocusSession.Create(
                type: FocusSessionType.Focus,
                plannedDuration: durationResult.Value,
                startedAt: startedAtUtc,
                taskId: focusTask.Id,
                presetId: preset.Id);

        if (sessionResult.IsFailure)
        {
            return Result<StartFocusSessionResponse>.Failure(
                sessionResult.Error);
        }

        if (focusTask.Status == FocusTaskStatus.Planned)
        {
            Result startTaskResult =
                focusTask.Start(startedAtUtc);

            if (startTaskResult.IsFailure)
            {
                return Result<StartFocusSessionResponse>.Failure(
                    startTaskResult.Error);
            }
        }

        FocusSession session =
            sessionResult.Value;

        _sessionRepository.Add(session);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        DateTimeOffset expectedEndAtUtc =
            session.ExpectedEndAtUtc
            ?? throw new InvalidOperationException(
                "У созданной фокус-сессии отсутствует ожидаемое время завершения.");

        var response = new StartFocusSessionResponse(
            SessionId: session.Id,
            TaskId: focusTask.Id,
            PresetId: preset.Id,
            PlannedDuration:
                session.PlannedDuration.Value,
            StartedAtUtc:
                session.StartedAtUtc,
            ExpectedEndAtUtc:
                expectedEndAtUtc);

        return Result<StartFocusSessionResponse>.Success(
            response);
    }
}