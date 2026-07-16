using FluentAssertions;
using FocusFlow.Application.Abstractions.Persistence;
using FocusFlow.Application.Abstractions.Time;
using FocusFlow.Application.Features.Sessions.StartFocusSession;
using FocusFlow.Domain.Presets;
using FocusFlow.Domain.Sessions;
using FocusFlow.Domain.Tasks;
using NSubstitute;

namespace FocusFlow.Application.Tests.Features.Sessions.StartFocusSession;

public sealed class StartFocusSessionHandlerTests
{
    private static readonly DateTimeOffset CurrentTimeUtc =
        new(
            2026,
            7,
            16,
            10,
            0,
            0,
            TimeSpan.Zero);

    private readonly IFocusTaskRepository
        _taskRepository;

    private readonly IPomodoroPresetRepository
        _presetRepository;

    private readonly IFocusSessionRepository
        _sessionRepository;

    private readonly IUnitOfWork
        _unitOfWork;

    private readonly IAppTimeProvider
        _timeProvider;

    private readonly StartFocusSessionHandler
        _handler;

    public StartFocusSessionHandlerTests()
    {
        _taskRepository =
            Substitute.For<IFocusTaskRepository>();

        _presetRepository =
            Substitute.For<IPomodoroPresetRepository>();

        _sessionRepository =
            Substitute.For<IFocusSessionRepository>();

        _unitOfWork =
            Substitute.For<IUnitOfWork>();

        _timeProvider =
            Substitute.For<IAppTimeProvider>();

        _timeProvider.UtcNow.Returns(
            CurrentTimeUtc);

        _sessionRepository
            .HasActiveSessionAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        _unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _handler = new StartFocusSessionHandler(
            _taskRepository,
            _presetRepository,
            _sessionRepository,
            _unitOfWork,
            _timeProvider);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateSession()
    {
        // Arrange
        FocusTask focusTask =
            CreatePlannedTask();

        PomodoroPreset preset =
            CreatePreset();

        ArrangeTask(focusTask);
        ArrangePreset(preset);

        var command =
            new StartFocusSessionCommand(
                focusTask.Id,
                preset.Id);

        // Act
        var result =
            await _handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.SessionId
            .Should()
            .NotBe(Guid.Empty);

        result.Value.TaskId
            .Should()
            .Be(focusTask.Id);

        result.Value.PresetId
            .Should()
            .Be(preset.Id);

        result.Value.PlannedDuration
            .Should()
            .Be(TimeSpan.FromMinutes(25));

        result.Value.StartedAtUtc
            .Should()
            .Be(CurrentTimeUtc);

        result.Value.ExpectedEndAtUtc
            .Should()
            .Be(CurrentTimeUtc.AddMinutes(25));

        focusTask.Status.Should().Be(
            FocusTaskStatus.InProgress);

        focusTask.StartedAtUtc.Should().Be(
            CurrentTimeUtc);

        _sessionRepository
            .Received(1)
            .Add(
                Arg.Is<FocusSession>(
                    session =>
                        session != null &&
                        session.Id ==
                            result.Value.SessionId &&
                        session.TaskId ==
                            focusTask.Id &&
                        session.PresetId ==
                            preset.Id &&
                        session.Type ==
                            FocusSessionType.Focus &&
                        session.Status ==
                            FocusSessionStatus.Running));

        await _unitOfWork
            .Received(1)
            .SaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTaskAlreadyInProgress_ShouldCreateSession()
    {
        // Arrange
        FocusTask focusTask =
            CreatePlannedTask();

        PomodoroPreset preset =
            CreatePreset();

        DateTimeOffset originalStartedAtUtc =
            CurrentTimeUtc.AddHours(-1);

        var startResult =
            focusTask.Start(
                originalStartedAtUtc);

        startResult.IsSuccess.Should().BeTrue();

        ArrangeTask(focusTask);
        ArrangePreset(preset);

        var command =
            new StartFocusSessionCommand(
                focusTask.Id,
                preset.Id);

        // Act
        var result =
            await _handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        focusTask.Status.Should().Be(
            FocusTaskStatus.InProgress);

        focusTask.StartedAtUtc.Should().Be(
            originalStartedAtUtc);

        _sessionRepository
            .Received(1)
            .Add(
                Arg.Is<FocusSession>(
                    session =>
                        session != null &&
                        session.TaskId ==
                            focusTask.Id &&
                        session.PresetId ==
                            preset.Id));

        await _unitOfWork
            .Received(1)
            .SaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyTaskId_ShouldReturnFailure()
    {
        // Arrange
        PomodoroPreset preset =
            CreatePreset();

        var command =
            new StartFocusSessionCommand(
                Guid.Empty,
                preset.Id);

        // Act
        var result =
            await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            StartFocusSessionErrors.TaskIdIsInvalid);

        await _taskRepository
            .DidNotReceive()
            .GetByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        _sessionRepository
            .DidNotReceive()
            .Add(Arg.Any<FocusSession>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyPresetId_ShouldReturnFailure()
    {
        // Arrange
        FocusTask focusTask =
            CreatePlannedTask();

        var command =
            new StartFocusSessionCommand(
                focusTask.Id,
                Guid.Empty);

        // Act
        var result =
            await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            StartFocusSessionErrors.PresetIdIsInvalid);

        await _taskRepository
            .DidNotReceive()
            .GetByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        _sessionRepository
            .DidNotReceive()
            .Add(Arg.Any<FocusSession>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        Guid missingTaskId =
            Guid.NewGuid();

        PomodoroPreset preset =
            CreatePreset();

        _taskRepository
            .GetByIdAsync(
                missingTaskId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<FocusTask?>(
                    null));

        var command =
            new StartFocusSessionCommand(
                missingTaskId,
                preset.Id);

        // Act
        var result =
            await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "StartFocusSession.Task.NotFound");

        await _presetRepository
            .DidNotReceive()
            .GetByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        _sessionRepository
            .DidNotReceive()
            .Add(Arg.Any<FocusSession>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPresetDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        FocusTask focusTask =
            CreatePlannedTask();

        Guid missingPresetId =
            Guid.NewGuid();

        ArrangeTask(focusTask);

        _presetRepository
            .GetByIdAsync(
                missingPresetId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<PomodoroPreset?>(
                    null));

        var command =
            new StartFocusSessionCommand(
                focusTask.Id,
                missingPresetId);

        // Act
        var result =
            await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "StartFocusSession.Preset.NotFound");

        _sessionRepository
            .DidNotReceive()
            .Add(Arg.Any<FocusSession>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTaskIsCompleted_ShouldReturnFailure()
    {
        // Arrange
        FocusTask focusTask =
            CreatePlannedTask();

        PomodoroPreset preset =
            CreatePreset();

        var completeResult =
            focusTask.Complete(
                CurrentTimeUtc.AddMinutes(-1));

        completeResult.IsSuccess.Should().BeTrue();

        ArrangeTask(focusTask);

        var command =
            new StartFocusSessionCommand(
                focusTask.Id,
                preset.Id);

        // Act
        var result =
            await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "StartFocusSession.Task.InvalidStatus");

        await _presetRepository
            .DidNotReceive()
            .GetByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        _sessionRepository
            .DidNotReceive()
            .Add(Arg.Any<FocusSession>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenActiveSessionExists_ShouldReturnFailure()
    {
        // Arrange
        FocusTask focusTask =
            CreatePlannedTask();

        PomodoroPreset preset =
            CreatePreset();

        ArrangeTask(focusTask);
        ArrangePreset(preset);

        _sessionRepository
            .HasActiveSessionAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var command =
            new StartFocusSessionCommand(
                focusTask.Id,
                preset.Id);

        // Act
        var result =
            await _handler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            StartFocusSessionErrors
                .ActiveSessionAlreadyExists);

        focusTask.Status.Should().Be(
            FocusTaskStatus.Planned);

        _sessionRepository
            .DidNotReceive()
            .Add(Arg.Any<FocusSession>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    private void ArrangeTask(
        FocusTask focusTask)
    {
        _taskRepository
            .GetByIdAsync(
                focusTask.Id,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<FocusTask?>(
                    focusTask));
    }

    private void ArrangePreset(
        PomodoroPreset preset)
    {
        _presetRepository
            .GetByIdAsync(
                preset.Id,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<PomodoroPreset?>(
                    preset));
    }

    private static FocusTask CreatePlannedTask()
    {
        var result = FocusTask.Create(
            title: "Реализовать StartFocusSession",
            description:
                "Создать первый Application-сценарий.",
            estimatedPomodoros: 4,
            createdAt:
                CurrentTimeUtc.AddHours(-2));

        result.IsSuccess.Should().BeTrue();

        return result.Value;
    }

    private static PomodoroPreset CreatePreset()
    {
        var result = PomodoroPreset.CreateUser(
            name: "Классический Pomodoro",
            focusDuration:
                TimeSpan.FromMinutes(25),
            shortBreakDuration:
                TimeSpan.FromMinutes(5),
            longBreakDuration:
                TimeSpan.FromMinutes(20),
            sessionsBeforeLongBreak: 4,
            autoStartBreaks: true,
            autoStartFocusSessions: false,
            createdAt:
                CurrentTimeUtc.AddHours(-2));

        result.IsSuccess.Should().BeTrue();

        return result.Value;
    }
}