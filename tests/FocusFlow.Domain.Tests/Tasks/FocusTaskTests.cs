using FluentAssertions;
using FocusFlow.Domain.Tasks;

namespace FocusFlow.Domain.Tests.Tasks;

public sealed class FocusTaskTests
{
    private static readonly DateTimeOffset CreatedAtUtc =
        new(
            2026,
            7,
            16,
            10,
            0,
            0,
            TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_ShouldCreatePlannedTask()
    {
        // Act
        var result = FocusTask.Create(
            title: "Реализовать FocusTask",
            description: "Добавить доменную модель задачи.",
            estimatedPomodoros: 4,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        FocusTask task = result.Value;

        task.Id.Should().NotBe(Guid.Empty);

        task.Title.Value.Should().Be(
            "Реализовать FocusTask");

        task.Description.Value.Should().Be(
            "Добавить доменную модель задачи.");

        task.Estimate.Value.Should().Be(4);

        task.Status.Should().Be(
            FocusTaskStatus.Planned);

        task.ProjectId.Should().BeNull();

        task.CompletedPomodoroCount.Should().Be(0);

        task.RemainingEstimatedPomodoros
            .Should()
            .Be(4);

        task.ProgressPercentage.Should().Be(0m);
        task.IsEstimateReached.Should().BeFalse();
        task.IsFinished.Should().BeFalse();

        task.CreatedAtUtc.Should().Be(CreatedAtUtc);
        task.UpdatedAtUtc.Should().Be(CreatedAtUtc);
    }

    [Fact]
    public void Create_WithProject_ShouldSaveProjectId()
    {
        // Arrange
        Guid projectId = Guid.NewGuid();

        // Act
        var result = FocusTask.Create(
            title: "Задача проекта",
            description: null,
            estimatedPomodoros: 2,
            createdAt: CreatedAtUtc,
            projectId: projectId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.ProjectId.Should().Be(
            projectId);
    }

    [Fact]
    public void Create_ShouldNormalizeTitle()
    {
        // Act
        var result = FocusTask.Create(
            title: "  Реализовать    импорт   каталога ",
            description: null,
            estimatedPomodoros: 4,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Title.Value.Should().Be(
            "Реализовать импорт каталога");
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldReturnFailure()
    {
        // Act
        var result = FocusTask.Create(
            title: "   ",
            description: null,
            estimatedPomodoros: 4,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusTaskErrors.TitleIsRequired);
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldReturnFailure()
    {
        // Arrange
        string description = new(
            'A',
            FocusTaskDescription.MaximumLength + 1);

        // Act
        var result = FocusTask.Create(
            title: "Задача",
            description: description,
            estimatedPomodoros: 4,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusTaskErrors.DescriptionIsTooLong);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithTooSmallEstimate_ShouldReturnFailure(
        int estimate)
    {
        // Act
        var result = FocusTask.Create(
            title: "Задача",
            description: null,
            estimatedPomodoros: estimate,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusTaskErrors.EstimateIsTooSmall);
    }

    [Fact]
    public void Create_WithTooLargeEstimate_ShouldReturnFailure()
    {
        // Act
        var result = FocusTask.Create(
            title: "Задача",
            description: null,
            estimatedPomodoros:
                PomodoroEstimate.MaximumValue + 1,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusTaskErrors.EstimateIsTooLarge);
    }

    [Fact]
    public void Create_WithEmptyProjectId_ShouldReturnFailure()
    {
        // Act
        var result = FocusTask.Create(
            title: "Задача",
            description: null,
            estimatedPomodoros: 4,
            createdAt: CreatedAtUtc,
            projectId: Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusTaskErrors.ProjectIdIsInvalid);
    }

    [Fact]
    public void Rename_ActiveTask_ShouldChangeTitle()
    {
        // Arrange
        FocusTask task = CreateTask();

        DateTimeOffset renamedAtUtc =
            CreatedAtUtc.AddMinutes(1);

        // Act
        var result = task.Rename(
            "Новое название",
            renamedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.Title.Value.Should().Be(
            "Новое название");

        task.UpdatedAtUtc.Should().Be(
            renamedAtUtc);
    }

    [Fact]
    public void ChangeDescription_ShouldUpdateDescription()
    {
        // Arrange
        FocusTask task = CreateTask();

        DateTimeOffset changedAtUtc =
            CreatedAtUtc.AddMinutes(1);

        // Act
        var result = task.ChangeDescription(
            "Новое подробное описание.",
            changedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.Description.Value.Should().Be(
            "Новое подробное описание.");

        task.UpdatedAtUtc.Should().Be(
            changedAtUtc);
    }

    [Fact]
    public void ChangeEstimate_ShouldUpdateEstimate()
    {
        // Arrange
        FocusTask task = CreateTask();

        // Act
        var result = task.ChangeEstimate(
            estimatedPomodoros: 8,
            changedAt: CreatedAtUtc.AddMinutes(1));

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.Estimate.Value.Should().Be(8);

        task.RemainingEstimatedPomodoros
            .Should()
            .Be(8);
    }

    [Fact]
    public void AssignToProject_ShouldSetProjectId()
    {
        // Arrange
        FocusTask task = CreateTask();
        Guid projectId = Guid.NewGuid();

        // Act
        var result = task.AssignToProject(
            projectId,
            CreatedAtUtc.AddMinutes(1));

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.ProjectId.Should().Be(projectId);
    }

    [Fact]
    public void RemoveFromProject_ShouldClearProjectId()
    {
        // Arrange
        Guid projectId = Guid.NewGuid();

        FocusTask task = CreateTask(
            projectId: projectId);

        // Act
        var result = task.RemoveFromProject(
            CreatedAtUtc.AddMinutes(1));

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.ProjectId.Should().BeNull();
    }

    [Fact]
    public void Start_PlannedTask_ShouldMoveToInProgress()
    {
        // Arrange
        FocusTask task = CreateTask();

        DateTimeOffset startedAtUtc =
            CreatedAtUtc.AddMinutes(1);

        // Act
        var result = task.Start(startedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.Status.Should().Be(
            FocusTaskStatus.InProgress);

        task.StartedAtUtc.Should().Be(
            startedAtUtc);

        task.UpdatedAtUtc.Should().Be(
            startedAtUtc);
    }

    [Fact]
    public void Start_AlreadyStartedTask_ShouldReturnFailure()
    {
        // Arrange
        FocusTask task = CreateTask();

        task.Start(
            CreatedAtUtc.AddMinutes(1));

        // Act
        var result = task.Start(
            CreatedAtUtc.AddMinutes(2));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "FocusTask.Start.InvalidStatus");
    }

    [Fact]
    public void RegisterCompletedFocusSession_ShouldAddProgress()
    {
        // Arrange
        FocusTask task = CreateTask();
        Guid focusSessionId = Guid.NewGuid();

        DateTimeOffset registeredAtUtc =
            CreatedAtUtc.AddMinutes(25);

        // Act
        var result =
            task.RegisterCompletedFocusSession(
                focusSessionId,
                registeredAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.Status.Should().Be(
            FocusTaskStatus.InProgress);

        task.StartedAtUtc.Should().Be(
            registeredAtUtc);

        task.CompletedPomodoroCount
            .Should()
            .Be(1);

        task.CompletedFocusSessionIds
            .Should()
            .Contain(focusSessionId);

        task.RemainingEstimatedPomodoros
            .Should()
            .Be(3);

        task.ProgressPercentage.Should().Be(25m);
    }

    [Fact]
    public void RegisterCompletedFocusSession_Twice_ShouldReturnFailure()
    {
        // Arrange
        FocusTask task = CreateTask();
        Guid focusSessionId = Guid.NewGuid();

        task.RegisterCompletedFocusSession(
            focusSessionId,
            CreatedAtUtc.AddMinutes(25));

        // Act
        var result =
            task.RegisterCompletedFocusSession(
                focusSessionId,
                CreatedAtUtc.AddMinutes(26));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusTaskErrors
                .FocusSessionAlreadyRegistered);

        task.CompletedPomodoroCount
            .Should()
            .Be(1);
    }

    [Fact]
    public void RemoveCompletedFocusSession_ShouldReduceProgress()
    {
        // Arrange
        FocusTask task = CreateTask();
        Guid focusSessionId = Guid.NewGuid();

        task.RegisterCompletedFocusSession(
            focusSessionId,
            CreatedAtUtc.AddMinutes(25));

        // Act
        var result =
            task.RemoveCompletedFocusSession(
                focusSessionId,
                CreatedAtUtc.AddMinutes(26));

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.CompletedPomodoroCount
            .Should()
            .Be(0);

        task.CompletedFocusSessionIds
            .Should()
            .NotContain(focusSessionId);

        task.RemainingEstimatedPomodoros
            .Should()
            .Be(4);
    }

    [Fact]
    public void Progress_WhenEstimateReached_ShouldReturnOneHundredPercent()
    {
        // Arrange
        FocusTask task = CreateTask(
            estimatedPomodoros: 2);

        task.RegisterCompletedFocusSession(
            Guid.NewGuid(),
            CreatedAtUtc.AddMinutes(25));

        task.RegisterCompletedFocusSession(
            Guid.NewGuid(),
            CreatedAtUtc.AddMinutes(50));

        // Assert
        task.CompletedPomodoroCount
            .Should()
            .Be(2);

        task.RemainingEstimatedPomodoros
            .Should()
            .Be(0);

        task.IsEstimateReached.Should().BeTrue();

        task.ProgressPercentage.Should().Be(100m);
    }

    [Fact]
    public void Progress_WhenEstimateExceeded_ShouldStayAtOneHundredPercent()
    {
        // Arrange
        FocusTask task = CreateTask(
            estimatedPomodoros: 1);

        task.RegisterCompletedFocusSession(
            Guid.NewGuid(),
            CreatedAtUtc.AddMinutes(25));

        task.RegisterCompletedFocusSession(
            Guid.NewGuid(),
            CreatedAtUtc.AddMinutes(50));

        // Assert
        task.CompletedPomodoroCount
            .Should()
            .Be(2);

        task.ProgressPercentage.Should().Be(100m);
        task.IsEstimateReached.Should().BeTrue();
    }

    [Fact]
    public void Complete_InProgressTask_ShouldCompleteTask()
    {
        // Arrange
        FocusTask task = CreateTask();

        task.Start(
            CreatedAtUtc.AddMinutes(1));

        DateTimeOffset completedAtUtc =
            CreatedAtUtc.AddHours(2);

        // Act
        var result = task.Complete(
            completedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.Status.Should().Be(
            FocusTaskStatus.Completed);

        task.CompletedAtUtc.Should().Be(
            completedAtUtc);

        task.CancelledAtUtc.Should().BeNull();
        task.IsFinished.Should().BeTrue();
    }

    [Fact]
    public void RegisterSession_CompletedTask_ShouldReturnFailure()
    {
        // Arrange
        FocusTask task = CreateTask();

        task.Complete(
            CreatedAtUtc.AddMinutes(1));

        // Act
        var result =
            task.RegisterCompletedFocusSession(
                Guid.NewGuid(),
                CreatedAtUtc.AddMinutes(2));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "FocusTask.RegisterSession.InvalidStatus");
    }

    [Fact]
    public void Cancel_ActiveTask_ShouldCancelTask()
    {
        // Arrange
        FocusTask task = CreateTask();

        DateTimeOffset cancelledAtUtc =
            CreatedAtUtc.AddMinutes(10);

        // Act
        var result = task.Cancel(
            cancelledAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.Status.Should().Be(
            FocusTaskStatus.Cancelled);

        task.CancelledAtUtc.Should().Be(
            cancelledAtUtc);

        task.CompletedAtUtc.Should().BeNull();
        task.IsFinished.Should().BeTrue();
    }

    [Fact]
    public void Reopen_CompletedTask_ShouldMoveToInProgress()
    {
        // Arrange
        FocusTask task = CreateTask();

        task.Complete(
            CreatedAtUtc.AddMinutes(10));

        DateTimeOffset reopenedAtUtc =
            CreatedAtUtc.AddMinutes(11);

        // Act
        var result = task.Reopen(
            reopenedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        task.Status.Should().Be(
            FocusTaskStatus.InProgress);

        task.CompletedAtUtc.Should().BeNull();
        task.CancelledAtUtc.Should().BeNull();
        task.IsFinished.Should().BeFalse();

        task.UpdatedAtUtc.Should().Be(
            reopenedAtUtc);
    }

    [Fact]
    public void Rename_CompletedTask_ShouldReturnFailure()
    {
        // Arrange
        FocusTask task = CreateTask();

        task.Complete(
            CreatedAtUtc.AddMinutes(1));

        // Act
        var result = task.Rename(
            "Новое название",
            CreatedAtUtc.AddMinutes(2));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "FocusTask.Modify.InvalidStatus");
    }

    [Fact]
    public void Rename_WithEarlierTime_ShouldReturnFailure()
    {
        // Arrange
        FocusTask task = CreateTask();

        // Act
        var result = task.Rename(
            "Новое название",
            CreatedAtUtc.AddMinutes(-1));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "FocusTask.ModificationTime.Invalid");

        task.Title.Value.Should().Be(
            "Реализовать FocusTask");
    }

    private static FocusTask CreateTask(
        Guid? projectId = null,
        int estimatedPomodoros = 4)
    {
        var result = FocusTask.Create(
            title: "Реализовать FocusTask",
            description: "Добавить доменную модель.",
            estimatedPomodoros: estimatedPomodoros,
            createdAt: CreatedAtUtc,
            projectId: projectId);

        result.IsSuccess.Should().BeTrue();

        return result.Value;
    }
}