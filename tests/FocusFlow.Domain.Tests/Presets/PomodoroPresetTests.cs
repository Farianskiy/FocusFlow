using FluentAssertions;
using FocusFlow.Domain.Presets;
using FocusFlow.Domain.Sessions;

namespace FocusFlow.Domain.Tests.Presets;

public sealed class PomodoroPresetTests
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
    public void CreateUser_WithValidData_ShouldCreatePreset()
    {
        // Act
        var result = PomodoroPreset.CreateUser(
            name: "Классический Pomodoro",
            focusDuration: TimeSpan.FromMinutes(25),
            shortBreakDuration: TimeSpan.FromMinutes(5),
            longBreakDuration: TimeSpan.FromMinutes(20),
            sessionsBeforeLongBreak: 4,
            autoStartBreaks: true,
            autoStartFocusSessions: false,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        PomodoroPreset preset = result.Value;

        preset.Id.Should().NotBe(Guid.Empty);
        preset.Name.Value.Should().Be(
            "Классический Pomodoro");

        preset.Kind.Should().Be(
            PomodoroPresetKind.User);

        preset.IsSystem.Should().BeFalse();

        preset.CycleSettings.FocusDuration.Should().Be(
            TimeSpan.FromMinutes(25));

        preset.CycleSettings.ShortBreakDuration.Should().Be(
            TimeSpan.FromMinutes(5));

        preset.CycleSettings.LongBreakDuration.Should().Be(
            TimeSpan.FromMinutes(20));

        preset.CycleSettings.SessionsBeforeLongBreak
            .Should()
            .Be(4);

        preset.AutoStartBreaks.Should().BeTrue();

        preset.AutoStartFocusSessions
            .Should()
            .BeFalse();

        preset.CreatedAtUtc.Should().Be(CreatedAtUtc);
        preset.UpdatedAtUtc.Should().Be(CreatedAtUtc);
    }

    [Fact]
    public void CreateUser_ShouldNormalizeName()
    {
        // Act
        var result = PomodoroPreset.CreateUser(
            name: "   Глубокая     работа   ",
            focusDuration: TimeSpan.FromMinutes(50),
            shortBreakDuration: TimeSpan.FromMinutes(10),
            longBreakDuration: TimeSpan.FromMinutes(30),
            sessionsBeforeLongBreak: 3,
            autoStartBreaks: false,
            autoStartFocusSessions: false,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Name.Value.Should().Be(
            "Глубокая работа");
    }

    [Fact]
    public void CreateUser_WithEmptyName_ShouldReturnFailure()
    {
        // Act
        var result = PomodoroPreset.CreateUser(
            name: "   ",
            focusDuration: TimeSpan.FromMinutes(25),
            shortBreakDuration: TimeSpan.FromMinutes(5),
            longBreakDuration: TimeSpan.FromMinutes(20),
            sessionsBeforeLongBreak: 4,
            autoStartBreaks: false,
            autoStartFocusSessions: false,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            PomodoroPresetErrors.NameIsRequired);
    }

    [Fact]
    public void CreateUser_WithTooLongName_ShouldReturnFailure()
    {
        // Arrange
        string longName = new(
            'A',
            PomodoroPresetName.MaximumLength + 1);

        // Act
        var result = PomodoroPreset.CreateUser(
            name: longName,
            focusDuration: TimeSpan.FromMinutes(25),
            shortBreakDuration: TimeSpan.FromMinutes(5),
            longBreakDuration: TimeSpan.FromMinutes(20),
            sessionsBeforeLongBreak: 4,
            autoStartBreaks: false,
            autoStartFocusSessions: false,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            PomodoroPresetErrors.NameIsTooLong);
    }

    [Fact]
    public void CreateUser_WithTooShortFocusDuration_ShouldReturnFailure()
    {
        // Act
        var result = PomodoroPreset.CreateUser(
            name: "Неверный пресет",
            focusDuration: TimeSpan.FromSeconds(30),
            shortBreakDuration: TimeSpan.FromMinutes(5),
            longBreakDuration: TimeSpan.FromMinutes(20),
            sessionsBeforeLongBreak: 4,
            autoStartBreaks: false,
            autoStartFocusSessions: false,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            PomodoroPresetErrors.FocusDurationIsTooShort);
    }

    [Fact]
    public void CreateUser_WithLongBreakShorterThanShortBreak_ShouldReturnFailure()
    {
        // Act
        var result = PomodoroPreset.CreateUser(
            name: "Неверный пресет",
            focusDuration: TimeSpan.FromMinutes(25),
            shortBreakDuration: TimeSpan.FromMinutes(10),
            longBreakDuration: TimeSpan.FromMinutes(5),
            sessionsBeforeLongBreak: 4,
            autoStartBreaks: false,
            autoStartFocusSessions: false,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            PomodoroPresetErrors.LongBreakIsShorterThanShortBreak);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void CreateUser_WithInvalidSessionsBeforeLongBreak_ShouldReturnFailure(
        int sessionsBeforeLongBreak)
    {
        // Act
        var result = PomodoroPreset.CreateUser(
            name: "Неверный пресет",
            focusDuration: TimeSpan.FromMinutes(25),
            shortBreakDuration: TimeSpan.FromMinutes(5),
            longBreakDuration: TimeSpan.FromMinutes(20),
            sessionsBeforeLongBreak: sessionsBeforeLongBreak,
            autoStartBreaks: false,
            autoStartFocusSessions: false,
            createdAt: CreatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            PomodoroPresetErrors.SessionsBeforeLongBreakIsInvalid);
    }

    [Fact]
    public void CreateSystem_WithValidData_ShouldCreateSystemPreset()
    {
        // Act
        PomodoroPreset preset = CreateSystemPreset();

        // Assert
        preset.Kind.Should().Be(
            PomodoroPresetKind.System);

        preset.IsSystem.Should().BeTrue();
    }

    [Fact]
    public void Rename_UserPreset_ShouldChangeName()
    {
        // Arrange
        PomodoroPreset preset = CreateUserPreset();

        DateTimeOffset renamedAtUtc =
            CreatedAtUtc.AddMinutes(1);

        // Act
        var result = preset.Rename(
            "Глубокая работа",
            renamedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        preset.Name.Value.Should().Be(
            "Глубокая работа");

        preset.UpdatedAtUtc.Should().Be(
            renamedAtUtc);
    }

    [Fact]
    public void Rename_SystemPreset_ShouldReturnFailure()
    {
        // Arrange
        PomodoroPreset preset = CreateSystemPreset();

        // Act
        var result = preset.Rename(
            "Новое название",
            CreatedAtUtc.AddMinutes(1));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            PomodoroPresetErrors.SystemPresetCannotBeModified);

        preset.Name.Value.Should().Be(
            "Классический Pomodoro");
    }

    [Fact]
    public void ChangeCycleSettings_WithValidData_ShouldUpdateSettings()
    {
        // Arrange
        PomodoroPreset preset = CreateUserPreset();

        DateTimeOffset changedAtUtc =
            CreatedAtUtc.AddMinutes(2);

        // Act
        var result = preset.ChangeCycleSettings(
            focusDuration: TimeSpan.FromMinutes(50),
            shortBreakDuration: TimeSpan.FromMinutes(10),
            longBreakDuration: TimeSpan.FromMinutes(30),
            sessionsBeforeLongBreak: 3,
            changedAt: changedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        preset.CycleSettings.FocusDuration.Should().Be(
            TimeSpan.FromMinutes(50));

        preset.CycleSettings.ShortBreakDuration.Should().Be(
            TimeSpan.FromMinutes(10));

        preset.CycleSettings.LongBreakDuration.Should().Be(
            TimeSpan.FromMinutes(30));

        preset.CycleSettings.SessionsBeforeLongBreak
            .Should()
            .Be(3);

        preset.UpdatedAtUtc.Should().Be(
            changedAtUtc);
    }

    [Fact]
    public void ChangeCycleSettings_WithInvalidData_ShouldNotChangePreset()
    {
        // Arrange
        PomodoroPreset preset = CreateUserPreset();

        PomodoroCycleSettings originalSettings =
            preset.CycleSettings;

        // Act
        var result = preset.ChangeCycleSettings(
            focusDuration: TimeSpan.FromMinutes(25),
            shortBreakDuration: TimeSpan.FromMinutes(20),
            longBreakDuration: TimeSpan.FromMinutes(10),
            sessionsBeforeLongBreak: 4,
            changedAt: CreatedAtUtc.AddMinutes(1));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            PomodoroPresetErrors.LongBreakIsShorterThanShortBreak);

        preset.CycleSettings.Should().Be(
            originalSettings);

        preset.UpdatedAtUtc.Should().Be(
            CreatedAtUtc);
    }

    [Fact]
    public void ChangeAutomation_UserPreset_ShouldUpdateSettings()
    {
        // Arrange
        PomodoroPreset preset = CreateUserPreset();

        DateTimeOffset changedAtUtc =
            CreatedAtUtc.AddMinutes(1);

        // Act
        var result = preset.ChangeAutomation(
            autoStartBreaks: true,
            autoStartFocusSessions: true,
            changedAt: changedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        preset.AutoStartBreaks.Should().BeTrue();

        preset.AutoStartFocusSessions
            .Should()
            .BeTrue();

        preset.UpdatedAtUtc.Should().Be(
            changedAtUtc);
    }

    [Fact]
    public void ChangeAutomation_SystemPreset_ShouldReturnFailure()
    {
        // Arrange
        PomodoroPreset preset = CreateSystemPreset();

        // Act
        var result = preset.ChangeAutomation(
            autoStartBreaks: false,
            autoStartFocusSessions: false,
            changedAt: CreatedAtUtc.AddMinutes(1));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            PomodoroPresetErrors.SystemPresetCannotBeModified);
    }

    [Fact]
    public void CreateCopy_FromSystemPreset_ShouldCreateEditableUserPreset()
    {
        // Arrange
        PomodoroPreset systemPreset =
            CreateSystemPreset();

        // Act
        var result = systemPreset.CreateCopy(
            "Мой Pomodoro",
            CreatedAtUtc.AddMinutes(1));

        // Assert
        result.IsSuccess.Should().BeTrue();

        PomodoroPreset copy = result.Value;

        copy.Id.Should().NotBe(systemPreset.Id);

        copy.Kind.Should().Be(
            PomodoroPresetKind.User);

        copy.IsSystem.Should().BeFalse();

        copy.Name.Value.Should().Be(
            "Мой Pomodoro");

        copy.CycleSettings.Should().Be(
            systemPreset.CycleSettings);
    }

    [Theory]
    [InlineData(FocusSessionType.Focus, 25)]
    [InlineData(FocusSessionType.ShortBreak, 5)]
    [InlineData(FocusSessionType.LongBreak, 20)]
    public void GetDurationFor_ShouldReturnCorrectDuration(
        FocusSessionType sessionType,
        int expectedMinutes)
    {
        // Arrange
        PomodoroPreset preset = CreateUserPreset();

        // Act
        var result = preset.GetDurationFor(sessionType);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().Be(
            TimeSpan.FromMinutes(expectedMinutes));
    }

    [Fact]
    public void GetDurationFor_WithoutSessionType_ShouldReturnFailure()
    {
        // Arrange
        PomodoroPreset preset = CreateUserPreset();

        // Act
        var result = preset.GetDurationFor(
            FocusSessionType.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            PomodoroPresetErrors.SessionTypeIsRequired);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(3, false)]
    [InlineData(4, true)]
    [InlineData(8, true)]
    public void IsLongBreakDue_ShouldReturnExpectedResult(
        int completedFocusSessions,
        bool expectedResult)
    {
        // Arrange
        PomodoroPreset preset = CreateUserPreset();

        // Act
        bool result = preset.IsLongBreakDue(
            completedFocusSessions);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void Rename_WithEarlierModificationTime_ShouldReturnFailure()
    {
        // Arrange
        PomodoroPreset preset = CreateUserPreset();

        // Act
        var result = preset.Rename(
            "Новое название",
            CreatedAtUtc.AddMinutes(-1));

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            "PomodoroPreset.ModificationTime.Invalid");

        preset.Name.Value.Should().Be(
            "Классический Pomodoro");
    }

    private static PomodoroPreset CreateUserPreset()
    {
        var result = PomodoroPreset.CreateUser(
            name: "Классический Pomodoro",
            focusDuration: TimeSpan.FromMinutes(25),
            shortBreakDuration: TimeSpan.FromMinutes(5),
            longBreakDuration: TimeSpan.FromMinutes(20),
            sessionsBeforeLongBreak: 4,
            autoStartBreaks: false,
            autoStartFocusSessions: false,
            createdAt: CreatedAtUtc);

        result.IsSuccess.Should().BeTrue();

        return result.Value;
    }

    private static PomodoroPreset CreateSystemPreset()
    {
        var result = PomodoroPreset.CreateSystem(
            name: "Классический Pomodoro",
            focusDuration: TimeSpan.FromMinutes(25),
            shortBreakDuration: TimeSpan.FromMinutes(5),
            longBreakDuration: TimeSpan.FromMinutes(20),
            sessionsBeforeLongBreak: 4,
            autoStartBreaks: true,
            autoStartFocusSessions: false,
            createdAt: CreatedAtUtc);

        result.IsSuccess.Should().BeTrue();

        return result.Value;
    }
}