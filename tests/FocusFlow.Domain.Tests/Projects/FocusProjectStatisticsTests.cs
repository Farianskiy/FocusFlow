using FluentAssertions;
using FocusFlow.Domain.Projects;
using FocusFlow.Domain.Tasks;

namespace FocusFlow.Domain.Tests.Projects;

public sealed class FocusProjectStatisticsTests
{
    [Fact]
    public void Calculate_WithoutTasks_ShouldReturnEmptyStatistics()
    {
        // Act
        FocusProjectStatistics statistics =
            FocusProjectStatistics.Calculate([]);

        // Assert
        statistics.IsEmpty.Should().BeTrue();

        statistics.TotalTaskCount.Should().Be(0);
        statistics.ActiveTaskCount.Should().Be(0);

        statistics.TotalEstimatedPomodoros
            .Should()
            .Be(0);

        statistics.TotalCompletedPomodoros
            .Should()
            .Be(0);

        statistics.RemainingEstimatedPomodoros
            .Should()
            .Be(0);

        statistics.TaskCompletionPercentage
            .Should()
            .Be(0m);

        statistics.PomodoroProgressPercentage
            .Should()
            .Be(0m);
    }

    [Fact]
    public void Calculate_ShouldAggregateTaskStatuses()
    {
        // Arrange
        FocusProjectTaskSnapshot[] tasks =
        [
            CreateSnapshot(
                FocusTaskStatus.Planned,
                estimate: 4,
                completed: 0),

            CreateSnapshot(
                FocusTaskStatus.InProgress,
                estimate: 4,
                completed: 2),

            CreateSnapshot(
                FocusTaskStatus.Completed,
                estimate: 2,
                completed: 2),

            CreateSnapshot(
                FocusTaskStatus.Cancelled,
                estimate: 10,
                completed: 1)
        ];

        // Act
        FocusProjectStatistics statistics =
            FocusProjectStatistics.Calculate(tasks);

        // Assert
        statistics.TotalTaskCount.Should().Be(4);

        statistics.PlannedTaskCount.Should().Be(1);
        statistics.InProgressTaskCount.Should().Be(1);
        statistics.CompletedTaskCount.Should().Be(1);
        statistics.CancelledTaskCount.Should().Be(1);

        statistics.ActiveTaskCount.Should().Be(2);
        statistics.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Calculate_ShouldCalculatePomodoroProgress()
    {
        // Arrange
        FocusProjectTaskSnapshot[] tasks =
        [
            CreateSnapshot(
                FocusTaskStatus.Planned,
                estimate: 4,
                completed: 0),

            CreateSnapshot(
                FocusTaskStatus.InProgress,
                estimate: 4,
                completed: 2),

            CreateSnapshot(
                FocusTaskStatus.Completed,
                estimate: 2,
                completed: 2)
        ];

        // Act
        FocusProjectStatistics statistics =
            FocusProjectStatistics.Calculate(tasks);

        // Assert
        statistics.TotalEstimatedPomodoros
            .Should()
            .Be(10);

        statistics.TotalCompletedPomodoros
            .Should()
            .Be(4);

        statistics.RemainingEstimatedPomodoros
            .Should()
            .Be(6);

        statistics.PomodoroProgressPercentage
            .Should()
            .Be(40m);

        statistics.TaskCompletionPercentage
            .Should()
            .BeApproximately(
                33.333333333333333333333333333m,
                0.000001m);
    }

    [Fact]
    public void Calculate_ShouldExcludeCancelledTasksFromProgress()
    {
        // Arrange
        FocusProjectTaskSnapshot[] tasks =
        [
            CreateSnapshot(
                FocusTaskStatus.Completed,
                estimate: 2,
                completed: 2),

            CreateSnapshot(
                FocusTaskStatus.Cancelled,
                estimate: 100,
                completed: 1)
        ];

        // Act
        FocusProjectStatistics statistics =
            FocusProjectStatistics.Calculate(tasks);

        // Assert
        statistics.TotalTaskCount.Should().Be(2);
        statistics.CancelledTaskCount.Should().Be(1);

        statistics.TotalEstimatedPomodoros
            .Should()
            .Be(2);

        statistics.TotalCompletedPomodoros
            .Should()
            .Be(2);

        statistics.RemainingEstimatedPomodoros
            .Should()
            .Be(0);

        statistics.TaskCompletionPercentage
            .Should()
            .Be(100m);

        statistics.PomodoroProgressPercentage
            .Should()
            .Be(100m);
    }

    [Fact]
    public void Calculate_WhenEstimateExceeded_ShouldCapProgressAtOneHundred()
    {
        // Arrange
        FocusProjectTaskSnapshot[] tasks =
        [
            CreateSnapshot(
                FocusTaskStatus.Completed,
                estimate: 2,
                completed: 5)
        ];

        // Act
        FocusProjectStatistics statistics =
            FocusProjectStatistics.Calculate(tasks);

        // Assert
        statistics.TotalEstimatedPomodoros
            .Should()
            .Be(2);

        statistics.TotalCompletedPomodoros
            .Should()
            .Be(5);

        statistics.RemainingEstimatedPomodoros
            .Should()
            .Be(0);

        statistics.PomodoroProgressPercentage
            .Should()
            .Be(100m);
    }

    [Fact]
    public void CreateSnapshot_WithNegativeCompletedCount_ShouldReturnFailure()
    {
        // Act
        var result = FocusProjectTaskSnapshot.Create(
            status: FocusTaskStatus.InProgress,
            estimatedPomodoros: 4,
            completedPomodoros: -1);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusProjectErrors
                .StatisticsCompletedPomodorosIsInvalid);
    }

    [Fact]
    public void CreateSnapshot_WithoutStatus_ShouldReturnFailure()
    {
        // Act
        var result = FocusProjectTaskSnapshot.Create(
            status: FocusTaskStatus.None,
            estimatedPomodoros: 4,
            completedPomodoros: 0);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusProjectErrors
                .StatisticsTaskStatusIsInvalid);
    }

    private static FocusProjectTaskSnapshot CreateSnapshot(
        FocusTaskStatus status,
        int estimate,
        int completed)
    {
        var result = FocusProjectTaskSnapshot.Create(
            status,
            estimate,
            completed);

        result.IsSuccess.Should().BeTrue();

        return result.Value;
    }
}