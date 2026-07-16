using FluentAssertions;
using FocusFlow.Domain.Sessions;

namespace FocusFlow.Domain.Tests.Sessions;

public sealed class FocusSessionTests
{
    private static readonly DateTimeOffset StartedAtUtc =
        new(
            2026,
            7,
            15,
            10,
            0,
            0,
            TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_ShouldCreateRunningSession()
    {
        // Arrange
        TimeSpan duration = TimeSpan.FromMinutes(25);

        // Act
        var result = FocusSession.Create(
            FocusSessionType.Focus,
            duration,
            StartedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        FocusSession session = result.Value;

        session.Id.Should().NotBe(Guid.Empty);
        session.Type.Should().Be(FocusSessionType.Focus);
        session.Status.Should().Be(FocusSessionStatus.Running);
        session.PlannedDuration.Value.Should().Be(duration);
        session.RemainingDuration.Should().Be(duration);
        session.StartedAtUtc.Should().Be(StartedAtUtc);
        session.ExpectedEndAtUtc.Should().Be(
            StartedAtUtc.Add(duration));
        session.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithoutType_ShouldReturnFailure()
    {
        // Act
        var result = FocusSession.Create(
            FocusSessionType.None,
            TimeSpan.FromMinutes(25),
            StartedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(
            FocusSessionErrors.TypeIsRequired);
    }

    [Fact]
    public void Create_WithDurationLessThanOneMinute_ShouldReturnFailure()
    {
        // Act
        var result = FocusSession.Create(
            FocusSessionType.Focus,
            TimeSpan.FromSeconds(30),
            StartedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(
            FocusSessionErrors.DurationIsTooShort);
    }

    [Fact]
    public void Create_WithDurationLongerThanTwelveHours_ShouldReturnFailure()
    {
        // Act
        var result = FocusSession.Create(
            FocusSessionType.Focus,
            TimeSpan.FromHours(13),
            StartedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(
            FocusSessionErrors.DurationIsTooLong);
    }

    [Fact]
    public void Create_BreakWithTask_ShouldReturnFailure()
    {
        // Act
        var result = FocusSession.Create(
            FocusSessionType.ShortBreak,
            TimeSpan.FromMinutes(5),
            StartedAtUtc,
            taskId: Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(
            FocusSessionErrors.BreakCannotHaveTask);
    }

    [Fact]
    public void Pause_RunningSession_ShouldSaveRemainingDuration()
    {
        // Arrange
        FocusSession session = CreateSession();

        DateTimeOffset pausedAtUtc =
            StartedAtUtc.AddMinutes(10);

        // Act
        var result = session.Pause(pausedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        session.Status.Should().Be(
            FocusSessionStatus.Paused);

        session.RemainingDuration.Should().Be(
            TimeSpan.FromMinutes(15));

        session.PausedAtUtc.Should().Be(pausedAtUtc);
        session.ExpectedEndAtUtc.Should().BeNull();
    }

    [Fact]
    public void Pause_AfterSessionExpired_ShouldReturnFailure()
    {
        // Arrange
        FocusSession session = CreateSession();

        DateTimeOffset pausedAtUtc =
            StartedAtUtc.AddMinutes(26);

        // Act
        var result = session.Pause(pausedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            FocusSessionErrors.SessionHasExpired);

        session.Status.Should().Be(
            FocusSessionStatus.Running);
    }

    [Fact]
    public void Pause_AlreadyPausedSession_ShouldReturnFailure()
    {
        // Arrange
        FocusSession session = CreateSession();

        session.Pause(
            StartedAtUtc.AddMinutes(5));

        // Act
        var result = session.Pause(
            StartedAtUtc.AddMinutes(6));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "FocusSession.Pause.InvalidStatus");
    }

    [Fact]
    public void Resume_PausedSession_ShouldCalculateNewExpectedEnd()
    {
        // Arrange
        FocusSession session = CreateSession();

        session.Pause(
            StartedAtUtc.AddMinutes(10));

        DateTimeOffset resumedAtUtc =
            StartedAtUtc.AddMinutes(20);

        // Act
        var result = session.Resume(resumedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        session.Status.Should().Be(
            FocusSessionStatus.Running);

        session.ExpectedEndAtUtc.Should().Be(
            resumedAtUtc.AddMinutes(15));

        session.PausedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Resume_RunningSession_ShouldReturnFailure()
    {
        // Arrange
        FocusSession session = CreateSession();

        // Act
        var result = session.Resume(
            StartedAtUtc.AddMinutes(5));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "FocusSession.Resume.InvalidStatus");
    }

    [Fact]
    public void Complete_BeforeTimerElapsed_ShouldMarkSessionAsManual()
    {
        // Arrange
        FocusSession session = CreateSession();

        DateTimeOffset completedAtUtc =
            StartedAtUtc.AddMinutes(10);

        // Act
        var result = session.Complete(completedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        session.Status.Should().Be(
            FocusSessionStatus.Completed);

        session.CompletionReason.Should().Be(
            FocusSessionCompletionReason.CompletedManually);

        session.CompletedAtUtc.Should().Be(completedAtUtc);

        session.ActualDuration.Should().Be(
            TimeSpan.FromMinutes(10));

        session.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Complete_AfterTimerElapsed_ShouldUseExpectedEndTime()
    {
        // Arrange
        FocusSession session = CreateSession();

        DateTimeOffset applicationOpenedAtUtc =
            StartedAtUtc.AddMinutes(40);

        // Act
        var result = session.Complete(
            applicationOpenedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        session.Status.Should().Be(
            FocusSessionStatus.Completed);

        session.CompletionReason.Should().Be(
            FocusSessionCompletionReason.TimerElapsed);

        session.CompletedAtUtc.Should().Be(
            StartedAtUtc.AddMinutes(25));

        session.ActualDuration.Should().Be(
            TimeSpan.FromMinutes(25));
    }

    [Fact]
    public void Complete_PausedSession_ShouldUseElapsedActiveTime()
    {
        // Arrange
        FocusSession session = CreateSession();

        session.Pause(
            StartedAtUtc.AddMinutes(10));

        // Act
        var result = session.Complete(
            StartedAtUtc.AddMinutes(20));

        // Assert
        result.IsSuccess.Should().BeTrue();

        session.Status.Should().Be(
            FocusSessionStatus.Completed);

        session.CompletionReason.Should().Be(
            FocusSessionCompletionReason.CompletedManually);

        session.ActualDuration.Should().Be(
            TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void Cancel_RunningSession_ShouldSaveElapsedDuration()
    {
        // Arrange
        FocusSession session = CreateSession();

        DateTimeOffset cancelledAtUtc =
            StartedAtUtc.AddMinutes(7);

        // Act
        var result = session.Cancel(cancelledAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        session.Status.Should().Be(
            FocusSessionStatus.Cancelled);

        session.CancelledAtUtc.Should().Be(cancelledAtUtc);

        session.ActualDuration.Should().Be(
            TimeSpan.FromMinutes(7));

        session.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Complete_CompletedSession_ShouldReturnFailure()
    {
        // Arrange
        FocusSession session = CreateSession();

        session.Complete(
            StartedAtUtc.AddMinutes(25));

        // Act
        var result = session.Complete(
            StartedAtUtc.AddMinutes(26));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "FocusSession.Complete.InvalidStatus");
    }

    [Fact]
    public void GetRemainingDuration_AfterExpectedEnd_ShouldReturnZero()
    {
        // Arrange
        FocusSession session = CreateSession();

        // Act
        TimeSpan remainingDuration =
            session.GetRemainingDuration(
                StartedAtUtc.AddMinutes(30));

        // Assert
        remainingDuration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void IsExpired_BeforeExpectedEnd_ShouldReturnFalse()
    {
        // Arrange
        FocusSession session = CreateSession();

        // Act
        bool isExpired = session.IsExpired(
            StartedAtUtc.AddMinutes(24));

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_AfterExpectedEnd_ShouldReturnTrue()
    {
        // Arrange
        FocusSession session = CreateSession();

        // Act
        bool isExpired = session.IsExpired(
            StartedAtUtc.AddMinutes(25));

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void Pause_WithTimeBeforeSessionStart_ShouldReturnFailure()
    {
        // Arrange
        FocusSession session = CreateSession();

        // Act
        var result = session.Pause(
            StartedAtUtc.AddMinutes(-1));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "FocusSession.TransitionTime.Invalid");
    }

    private static FocusSession CreateSession()
    {
        var result = FocusSession.Create(
            FocusSessionType.Focus,
            TimeSpan.FromMinutes(25),
            StartedAtUtc);

        result.IsSuccess.Should().BeTrue();

        return result.Value;
    }
}