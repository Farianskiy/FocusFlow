using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Sessions;

public sealed class FocusSession : AggregateRoot
{
    private FocusSession(
        Guid? taskId,
        Guid? presetId,
        FocusSessionType type,
        SessionDuration plannedDuration,
        DateTimeOffset startedAtUtc)
    {
        Id = Guid.CreateVersion7();

        TaskId = taskId;
        PresetId = presetId;
        Type = type;
        PlannedDuration = plannedDuration;

        Status = FocusSessionStatus.Running;

        StartedAtUtc = startedAtUtc;
        ExpectedEndAtUtc = startedAtUtc.Add(plannedDuration.Value);

        RemainingDuration = plannedDuration.Value;

        CreatedAtUtc = startedAtUtc;
        UpdatedAtUtc = startedAtUtc;
    }

    public Guid? TaskId { get; private set; }

    public Guid? PresetId { get; private set; }

    public FocusSessionType Type { get; private set; }

    public FocusSessionStatus Status { get; private set; }

    public FocusSessionCompletionReason CompletionReason { get; private set; }

    public SessionDuration PlannedDuration { get; private set; }

    public TimeSpan RemainingDuration { get; private set; }

    public TimeSpan? ActualDuration { get; private set; }

    public DateTimeOffset StartedAtUtc { get; private set; }

    public DateTimeOffset? ExpectedEndAtUtc { get; private set; }

    public DateTimeOffset? PausedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public bool IsActive =>
        Status is FocusSessionStatus.Running or FocusSessionStatus.Paused;

    public static Result<FocusSession> Create(
        FocusSessionType type,
        TimeSpan plannedDuration,
        DateTimeOffset startedAt,
        Guid? taskId = null,
        Guid? presetId = null)
    {
        if (type == FocusSessionType.None)
        {
            return Result<FocusSession>.Failure(
                FocusSessionErrors.TypeIsRequired);
        }

        if (taskId == Guid.Empty)
        {
            return Result<FocusSession>.Failure(
                FocusSessionErrors.TaskIdIsInvalid);
        }

        if (presetId == Guid.Empty)
        {
            return Result<FocusSession>.Failure(
                FocusSessionErrors.PresetIdIsInvalid);
        }

        if (type != FocusSessionType.Focus && taskId.HasValue)
        {
            return Result<FocusSession>.Failure(
                FocusSessionErrors.BreakCannotHaveTask);
        }

        Result<SessionDuration> durationResult =
            SessionDuration.Create(plannedDuration);

        if (durationResult.IsFailure)
        {
            return Result<FocusSession>.Failure(
                durationResult.Error);
        }

        DateTimeOffset startedAtUtc =
            startedAt.ToUniversalTime();

        var session = new FocusSession(
            taskId,
            presetId,
            type,
            durationResult.Value,
            startedAtUtc);

        return Result<FocusSession>.Success(session);
    }

    public Result Pause(DateTimeOffset pausedAt)
    {
        if (Status != FocusSessionStatus.Running)
        {
            return Result.Failure(
                FocusSessionErrors.CannotPause(Status));
        }

        DateTimeOffset pausedAtUtc =
            pausedAt.ToUniversalTime();

        Result timeValidationResult =
            ValidateTransitionTime(pausedAtUtc);

        if (timeValidationResult.IsFailure)
            return timeValidationResult;

        DateTimeOffset expectedEndAtUtc =
            ExpectedEndAtUtc
            ?? throw new InvalidOperationException(
                "У запущенной сессии отсутствует время завершения.");

        if (pausedAtUtc >= expectedEndAtUtc)
        {
            return Result.Failure(
                FocusSessionErrors.SessionHasExpired);
        }

        RemainingDuration =
            expectedEndAtUtc - pausedAtUtc;

        Status = FocusSessionStatus.Paused;
        PausedAtUtc = pausedAtUtc;
        ExpectedEndAtUtc = null;
        UpdatedAtUtc = pausedAtUtc;

        return Result.Success();
    }

    public Result Resume(DateTimeOffset resumedAt)
    {
        if (Status != FocusSessionStatus.Paused)
        {
            return Result.Failure(
                FocusSessionErrors.CannotResume(Status));
        }

        DateTimeOffset resumedAtUtc =
            resumedAt.ToUniversalTime();

        Result timeValidationResult =
            ValidateTransitionTime(resumedAtUtc);

        if (timeValidationResult.IsFailure)
            return timeValidationResult;

        ExpectedEndAtUtc =
            resumedAtUtc.Add(RemainingDuration);

        Status = FocusSessionStatus.Running;
        PausedAtUtc = null;
        UpdatedAtUtc = resumedAtUtc;

        return Result.Success();
    }

    public Result Complete(DateTimeOffset completedAt)
    {
        if (Status is not (
            FocusSessionStatus.Running or
            FocusSessionStatus.Paused))
        {
            return Result.Failure(
                FocusSessionErrors.CannotComplete(Status));
        }

        DateTimeOffset completedAtUtc =
            completedAt.ToUniversalTime();

        Result timeValidationResult =
            ValidateTransitionTime(completedAtUtc);

        if (timeValidationResult.IsFailure)
            return timeValidationResult;

        TimeSpan remainingDuration =
            GetRemainingDuration(completedAtUtc);

        ActualDuration =
            PlannedDuration.Value - remainingDuration;

        DateTimeOffset effectiveCompletionTimeUtc =
            completedAtUtc;

        if (Status == FocusSessionStatus.Running &&
            ExpectedEndAtUtc.HasValue &&
            completedAtUtc >= ExpectedEndAtUtc.Value)
        {
            CompletionReason =
                FocusSessionCompletionReason.TimerElapsed;

            effectiveCompletionTimeUtc =
                ExpectedEndAtUtc.Value;

            ActualDuration =
                PlannedDuration.Value;
        }
        else
        {
            CompletionReason =
                FocusSessionCompletionReason.CompletedManually;
        }

        Status = FocusSessionStatus.Completed;

        CompletedAtUtc = effectiveCompletionTimeUtc;
        CancelledAtUtc = null;
        PausedAtUtc = null;
        ExpectedEndAtUtc = null;
        RemainingDuration = TimeSpan.Zero;
        UpdatedAtUtc = effectiveCompletionTimeUtc;

        return Result.Success();
    }

    public Result Cancel(DateTimeOffset cancelledAt)
    {
        if (Status is not (
            FocusSessionStatus.Running or
            FocusSessionStatus.Paused))
        {
            return Result.Failure(
                FocusSessionErrors.CannotCancel(Status));
        }

        DateTimeOffset cancelledAtUtc =
            cancelledAt.ToUniversalTime();

        Result timeValidationResult =
            ValidateTransitionTime(cancelledAtUtc);

        if (timeValidationResult.IsFailure)
            return timeValidationResult;

        TimeSpan remainingDuration =
            GetRemainingDuration(cancelledAtUtc);

        ActualDuration =
            PlannedDuration.Value - remainingDuration;

        Status = FocusSessionStatus.Cancelled;
        CompletionReason = FocusSessionCompletionReason.None;

        CancelledAtUtc = cancelledAtUtc;
        CompletedAtUtc = null;
        PausedAtUtc = null;
        ExpectedEndAtUtc = null;
        RemainingDuration = TimeSpan.Zero;
        UpdatedAtUtc = cancelledAtUtc;

        return Result.Success();
    }

    public TimeSpan GetRemainingDuration(
        DateTimeOffset currentTime)
    {
        DateTimeOffset currentTimeUtc =
            currentTime.ToUniversalTime();

        if (Status == FocusSessionStatus.Paused)
            return RemainingDuration;

        if (Status != FocusSessionStatus.Running)
            return TimeSpan.Zero;

        DateTimeOffset expectedEndAtUtc =
            ExpectedEndAtUtc
            ?? throw new InvalidOperationException(
                "У запущенной сессии отсутствует время завершения.");

        TimeSpan remainingDuration =
            expectedEndAtUtc - currentTimeUtc;

        return remainingDuration > TimeSpan.Zero
            ? remainingDuration
            : TimeSpan.Zero;
    }

    public bool IsExpired(DateTimeOffset currentTime)
    {
        if (Status != FocusSessionStatus.Running)
            return false;

        DateTimeOffset currentTimeUtc =
            currentTime.ToUniversalTime();

        return ExpectedEndAtUtc.HasValue &&
               currentTimeUtc >= ExpectedEndAtUtc.Value;
    }

    private Result ValidateTransitionTime(
        DateTimeOffset transitionTimeUtc)
    {
        if (transitionTimeUtc < UpdatedAtUtc)
        {
            return Result.Failure(
                FocusSessionErrors.TransitionTimeIsInvalid(
                    transitionTimeUtc,
                    UpdatedAtUtc));
        }

        return Result.Success();
    }
}