using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Projects;

public sealed class FocusProject : AggregateRoot
{
    private FocusProject(
        FocusProjectName name,
        FocusProjectDescription description,
        FocusProjectColor color,
        DateTimeOffset createdAtUtc)
    {
        Id = Guid.CreateVersion7();

        Name = name;
        Description = description;
        Color = color;

        Status = FocusProjectStatus.Active;

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public FocusProjectName Name { get; private set; }

    public FocusProjectDescription Description { get; private set; }

    public FocusProjectColor Color { get; private set; }

    public FocusProjectStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public bool CanAcceptTasks =>
        Status == FocusProjectStatus.Active;

    public bool IsCompleted =>
        Status is
            FocusProjectStatus.Completed or
            FocusProjectStatus.Archived;

    public bool IsArchived =>
        Status == FocusProjectStatus.Archived;

    public static Result<FocusProject> Create(
        string? name,
        string? description,
        string? color,
        DateTimeOffset createdAt)
    {
        Result<FocusProjectName> nameResult =
            FocusProjectName.Create(name);

        if (nameResult.IsFailure)
        {
            return Result<FocusProject>.Failure(
                nameResult.Error);
        }

        Result<FocusProjectDescription> descriptionResult =
            FocusProjectDescription.Create(description);

        if (descriptionResult.IsFailure)
        {
            return Result<FocusProject>.Failure(
                descriptionResult.Error);
        }

        Result<FocusProjectColor> colorResult =
            FocusProjectColor.Create(color);

        if (colorResult.IsFailure)
        {
            return Result<FocusProject>.Failure(
                colorResult.Error);
        }

        DateTimeOffset createdAtUtc =
            createdAt.ToUniversalTime();

        var project = new FocusProject(
            nameResult.Value,
            descriptionResult.Value,
            colorResult.Value,
            createdAtUtc);

        return Result<FocusProject>.Success(project);
    }

    public Result Rename(
        string? newName,
        DateTimeOffset renamedAt)
    {
        Result editableResult = EnsureEditable();

        if (editableResult.IsFailure)
            return editableResult;

        Result<FocusProjectName> nameResult =
            FocusProjectName.Create(newName);

        if (nameResult.IsFailure)
        {
            return Result.Failure(
                nameResult.Error);
        }

        DateTimeOffset renamedAtUtc =
            renamedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(renamedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        if (Name == nameResult.Value)
            return Result.Success();

        Name = nameResult.Value;
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

        Result<FocusProjectDescription> descriptionResult =
            FocusProjectDescription.Create(newDescription);

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

    public Result ChangeColor(
        string? newColor,
        DateTimeOffset changedAt)
    {
        Result editableResult = EnsureEditable();

        if (editableResult.IsFailure)
            return editableResult;

        Result<FocusProjectColor> colorResult =
            FocusProjectColor.Create(newColor);

        if (colorResult.IsFailure)
        {
            return Result.Failure(
                colorResult.Error);
        }

        DateTimeOffset changedAtUtc =
            changedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(changedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        if (Color == colorResult.Value)
            return Result.Success();

        Color = colorResult.Value;
        UpdatedAtUtc = changedAtUtc;

        return Result.Success();
    }

    public Result Complete(
        DateTimeOffset completedAt)
    {
        if (Status != FocusProjectStatus.Active)
        {
            return Result.Failure(
                FocusProjectErrors.CannotComplete(
                    Status));
        }

        DateTimeOffset completedAtUtc =
            completedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(completedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        Status = FocusProjectStatus.Completed;

        CompletedAtUtc = completedAtUtc;
        ArchivedAtUtc = null;
        UpdatedAtUtc = completedAtUtc;

        return Result.Success();
    }

    public Result Reopen(
        DateTimeOffset reopenedAt)
    {
        if (Status != FocusProjectStatus.Completed)
        {
            return Result.Failure(
                FocusProjectErrors.CannotReopen(
                    Status));
        }

        DateTimeOffset reopenedAtUtc =
            reopenedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(reopenedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        Status = FocusProjectStatus.Active;

        CompletedAtUtc = null;
        ArchivedAtUtc = null;
        UpdatedAtUtc = reopenedAtUtc;

        return Result.Success();
    }

    public Result Archive(
        DateTimeOffset archivedAt)
    {
        if (Status != FocusProjectStatus.Completed)
        {
            return Result.Failure(
                FocusProjectErrors.CannotArchive(
                    Status));
        }

        DateTimeOffset archivedAtUtc =
            archivedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(archivedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        Status = FocusProjectStatus.Archived;

        ArchivedAtUtc = archivedAtUtc;
        UpdatedAtUtc = archivedAtUtc;

        return Result.Success();
    }

    public Result Restore(
        DateTimeOffset restoredAt)
    {
        if (Status != FocusProjectStatus.Archived)
        {
            return Result.Failure(
                FocusProjectErrors.CannotRestore(
                    Status));
        }

        DateTimeOffset restoredAtUtc =
            restoredAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(restoredAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        Status = FocusProjectStatus.Completed;

        ArchivedAtUtc = null;
        UpdatedAtUtc = restoredAtUtc;

        return Result.Success();
    }

    private Result EnsureEditable()
    {
        if (IsArchived)
        {
            return Result.Failure(
                FocusProjectErrors
                    .ArchivedProjectCannotBeModified);
        }

        return Result.Success();
    }

    private Result ValidateModificationTime(
        DateTimeOffset modificationTimeUtc)
    {
        if (modificationTimeUtc < UpdatedAtUtc)
        {
            return Result.Failure(
                FocusProjectErrors
                    .ModificationTimeIsInvalid(
                        modificationTimeUtc,
                        UpdatedAtUtc));
        }

        return Result.Success();
    }
}