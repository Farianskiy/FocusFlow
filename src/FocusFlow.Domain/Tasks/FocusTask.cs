using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Tasks;

public sealed class FocusTask : AggregateRoot
{
    private readonly HashSet<Guid>
        _completedFocusSessionIds = [];

    private FocusTask(
        FocusTaskTitle title,
        FocusTaskDescription description,
        PomodoroEstimate estimate,
        Guid? projectId,
        DateTimeOffset createdAtUtc)
    {
        Id = Guid.CreateVersion7();

        Title = title;
        Description = description;
        Estimate = estimate;
        ProjectId = projectId;

        Status = FocusTaskStatus.Planned;

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public FocusTaskTitle Title { get; private set; }

    public FocusTaskDescription Description { get; private set; }

    public PomodoroEstimate Estimate { get; private set; }

    public Guid? ProjectId { get; private set; }

    public FocusTaskStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public IReadOnlyCollection<Guid>
        CompletedFocusSessionIds =>
            _completedFocusSessionIds;

    public int CompletedPomodoroCount =>
        _completedFocusSessionIds.Count;

    public int RemainingEstimatedPomodoros =>
        Math.Max(
            0,
            Estimate.Value - CompletedPomodoroCount);

    public bool IsEstimateReached =>
        CompletedPomodoroCount >= Estimate.Value;

    public decimal ProgressPercentage =>
        Math.Min(
            100m,
            CompletedPomodoroCount *
            100m /
            Estimate.Value);

    public bool IsFinished =>
        Status is
            FocusTaskStatus.Completed or
            FocusTaskStatus.Cancelled;

    public static Result<FocusTask> Create(
        string? title,
        string? description,
        int estimatedPomodoros,
        DateTimeOffset createdAt,
        Guid? projectId = null)
    {
        if (projectId == Guid.Empty)
        {
            return Result<FocusTask>.Failure(
                FocusTaskErrors.ProjectIdIsInvalid);
        }

        Result<FocusTaskTitle> titleResult =
            FocusTaskTitle.Create(title);

        if (titleResult.IsFailure)
        {
            return Result<FocusTask>.Failure(
                titleResult.Error);
        }

        Result<FocusTaskDescription> descriptionResult =
            FocusTaskDescription.Create(description);

        if (descriptionResult.IsFailure)
        {
            return Result<FocusTask>.Failure(
                descriptionResult.Error);
        }

        Result<PomodoroEstimate> estimateResult =
            PomodoroEstimate.Create(
                estimatedPomodoros);

        if (estimateResult.IsFailure)
        {
            return Result<FocusTask>.Failure(
                estimateResult.Error);
        }

        DateTimeOffset createdAtUtc =
            createdAt.ToUniversalTime();

        var task = new FocusTask(
            titleResult.Value,
            descriptionResult.Value,
            estimateResult.Value,
            projectId,
            createdAtUtc);

        return Result<FocusTask>.Success(task);
    }

    public Result Rename(
        string? newTitle,
        DateTimeOffset renamedAt)
    {
        Result editableResult = EnsureEditable();

        if (editableResult.IsFailure)
            return editableResult;

        Result<FocusTaskTitle> titleResult =
            FocusTaskTitle.Create(newTitle);

        if (titleResult.IsFailure)
        {
            return Result.Failure(
                titleResult.Error);
        }

        DateTimeOffset renamedAtUtc =
            renamedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(renamedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        if (Title == titleResult.Value)
            return Result.Success();

        Title = titleResult.Value;
        UpdatedAtUtc = renamedAtUtc;

        return Result.Success();
    }

    public Result ChangeDescription(
        string? newDescription,
        DateTimeOffset changedAt)
    {
        Result editableResult = EnsureEditable();

        if (editableResult.IsFailure)
            return editableResult;

        Result<FocusTaskDescription> descriptionResult =
            FocusTaskDescription.Create(newDescription);

        if (descriptionResult.IsFailure)
        {
            return Result.Failure(
                descriptionResult.Error);
        }

        DateTimeOffset changedAtUtc =
            changedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(changedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        if (Description == descriptionResult.Value)
            return Result.Success();

        Description = descriptionResult.Value;
        UpdatedAtUtc = changedAtUtc;

        return Result.Success();
    }

    public Result ChangeEstimate(
        int estimatedPomodoros,
        DateTimeOffset changedAt)
    {
        Result editableResult = EnsureEditable();

        if (editableResult.IsFailure)
            return editableResult;

        Result<PomodoroEstimate> estimateResult =
            PomodoroEstimate.Create(
                estimatedPomodoros);

        if (estimateResult.IsFailure)
        {
            return Result.Failure(
                estimateResult.Error);
        }

        DateTimeOffset changedAtUtc =
            changedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(changedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        if (Estimate == estimateResult.Value)
            return Result.Success();

        Estimate = estimateResult.Value;
        UpdatedAtUtc = changedAtUtc;

        return Result.Success();
    }

    public Result AssignToProject(
        Guid projectId,
        DateTimeOffset assignedAt)
    {
        Result editableResult = EnsureEditable();

        if (editableResult.IsFailure)
            return editableResult;

        if (projectId == Guid.Empty)
        {
            return Result.Failure(
                FocusTaskErrors.ProjectIdIsInvalid);
        }

        DateTimeOffset assignedAtUtc =
            assignedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(assignedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        if (ProjectId == projectId)
            return Result.Success();

        ProjectId = projectId;
        UpdatedAtUtc = assignedAtUtc;

        return Result.Success();
    }

    public Result RemoveFromProject(
        DateTimeOffset removedAt)
    {
        Result editableResult = EnsureEditable();

        if (editableResult.IsFailure)
            return editableResult;

        DateTimeOffset removedAtUtc =
            removedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(removedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        if (!ProjectId.HasValue)
            return Result.Success();

        ProjectId = null;
        UpdatedAtUtc = removedAtUtc;

        return Result.Success();
    }

    public Result Start(DateTimeOffset startedAt)
    {
        if (Status != FocusTaskStatus.Planned)
        {
            return Result.Failure(
                FocusTaskErrors.CannotStart(Status));
        }

        DateTimeOffset startedAtUtc =
            startedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(startedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        Status = FocusTaskStatus.InProgress;
        StartedAtUtc = startedAtUtc;
        UpdatedAtUtc = startedAtUtc;

        return Result.Success();
    }

    public Result RegisterCompletedFocusSession(
        Guid focusSessionId,
        DateTimeOffset registeredAt)
    {
        if (focusSessionId == Guid.Empty)
        {
            return Result.Failure(
                FocusTaskErrors.FocusSessionIdIsInvalid);
        }

        if (Status is not (
            FocusTaskStatus.Planned or
            FocusTaskStatus.InProgress))
        {
            return Result.Failure(
                FocusTaskErrors.CannotRegisterSession(
                    Status));
        }

        if (_completedFocusSessionIds.Contains(
                focusSessionId))
        {
            return Result.Failure(
                FocusTaskErrors
                    .FocusSessionAlreadyRegistered);
        }

        DateTimeOffset registeredAtUtc =
            registeredAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(registeredAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        _completedFocusSessionIds.Add(
            focusSessionId);

        if (Status == FocusTaskStatus.Planned)
        {
            Status = FocusTaskStatus.InProgress;
            StartedAtUtc ??= registeredAtUtc;
        }

        UpdatedAtUtc = registeredAtUtc;

        return Result.Success();
    }

    public Result RemoveCompletedFocusSession(
        Guid focusSessionId,
        DateTimeOffset removedAt)
    {
        if (focusSessionId == Guid.Empty)
        {
            return Result.Failure(
                FocusTaskErrors.FocusSessionIdIsInvalid);
        }

        if (Status is not (
            FocusTaskStatus.Planned or
            FocusTaskStatus.InProgress))
        {
            return Result.Failure(
                FocusTaskErrors.CannotRemoveSession(
                    Status));
        }

        if (!_completedFocusSessionIds.Contains(
                focusSessionId))
        {
            return Result.Failure(
                FocusTaskErrors
                    .FocusSessionIsNotRegistered);
        }

        DateTimeOffset removedAtUtc =
            removedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(removedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        _completedFocusSessionIds.Remove(
            focusSessionId);

        UpdatedAtUtc = removedAtUtc;

        return Result.Success();
    }

    public Result Complete(DateTimeOffset completedAt)
    {
        if (Status is not (
            FocusTaskStatus.Planned or
            FocusTaskStatus.InProgress))
        {
            return Result.Failure(
                FocusTaskErrors.CannotComplete(
                    Status));
        }

        DateTimeOffset completedAtUtc =
            completedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(completedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        Status = FocusTaskStatus.Completed;

        CompletedAtUtc = completedAtUtc;
        CancelledAtUtc = null;
        UpdatedAtUtc = completedAtUtc;

        return Result.Success();
    }

    public Result Cancel(DateTimeOffset cancelledAt)
    {
        if (Status is not (
            FocusTaskStatus.Planned or
            FocusTaskStatus.InProgress))
        {
            return Result.Failure(
                FocusTaskErrors.CannotCancel(Status));
        }

        DateTimeOffset cancelledAtUtc =
            cancelledAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(cancelledAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        Status = FocusTaskStatus.Cancelled;

        CancelledAtUtc = cancelledAtUtc;
        CompletedAtUtc = null;
        UpdatedAtUtc = cancelledAtUtc;

        return Result.Success();
    }

    public Result Reopen(DateTimeOffset reopenedAt)
    {
        if (Status is not (
            FocusTaskStatus.Completed or
            FocusTaskStatus.Cancelled))
        {
            return Result.Failure(
                FocusTaskErrors.CannotReopen(Status));
        }

        DateTimeOffset reopenedAtUtc =
            reopenedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(reopenedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        Status = FocusTaskStatus.InProgress;

        StartedAtUtc ??= reopenedAtUtc;
        CompletedAtUtc = null;
        CancelledAtUtc = null;
        UpdatedAtUtc = reopenedAtUtc;

        return Result.Success();
    }

    private Result EnsureEditable()
    {
        if (IsFinished)
        {
            return Result.Failure(
                FocusTaskErrors.CannotModify(
                    Status));
        }

        return Result.Success();
    }

    private Result ValidateModificationTime(
        DateTimeOffset modificationTimeUtc)
    {
        if (modificationTimeUtc < UpdatedAtUtc)
        {
            return Result.Failure(
                FocusTaskErrors
                    .ModificationTimeIsInvalid(
                        modificationTimeUtc,
                        UpdatedAtUtc));
        }

        return Result.Success();
    }
}