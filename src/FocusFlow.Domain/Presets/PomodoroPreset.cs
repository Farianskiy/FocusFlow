using FocusFlow.Domain.Common;
using FocusFlow.Domain.Sessions;

namespace FocusFlow.Domain.Presets;

public sealed class PomodoroPreset : AggregateRoot
{
    private PomodoroPreset(
        PomodoroPresetName name,
        PomodoroCycleSettings cycleSettings,
        PomodoroPresetKind kind,
        bool autoStartBreaks,
        bool autoStartFocusSessions,
        DateTimeOffset createdAtUtc)
    {
        Id = Guid.CreateVersion7();

        Name = name;
        CycleSettings = cycleSettings;
        Kind = kind;

        AutoStartBreaks = autoStartBreaks;
        AutoStartFocusSessions = autoStartFocusSessions;

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public PomodoroPresetName Name { get; private set; }

    public PomodoroCycleSettings CycleSettings { get; private set; }

    public PomodoroPresetKind Kind { get; private set; }

    public bool AutoStartBreaks { get; private set; }

    public bool AutoStartFocusSessions { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public bool IsSystem => Kind == PomodoroPresetKind.System;

    public static Result<PomodoroPreset> CreateUser(
        string name,
        TimeSpan focusDuration,
        TimeSpan shortBreakDuration,
        TimeSpan longBreakDuration,
        int sessionsBeforeLongBreak,
        bool autoStartBreaks,
        bool autoStartFocusSessions,
        DateTimeOffset createdAt)
    {
        return Create(
            name,
            focusDuration,
            shortBreakDuration,
            longBreakDuration,
            sessionsBeforeLongBreak,
            PomodoroPresetKind.User,
            autoStartBreaks,
            autoStartFocusSessions,
            createdAt);
    }

    public static Result<PomodoroPreset> CreateSystem(
        string name,
        TimeSpan focusDuration,
        TimeSpan shortBreakDuration,
        TimeSpan longBreakDuration,
        int sessionsBeforeLongBreak,
        bool autoStartBreaks,
        bool autoStartFocusSessions,
        DateTimeOffset createdAt)
    {
        return Create(
            name,
            focusDuration,
            shortBreakDuration,
            longBreakDuration,
            sessionsBeforeLongBreak,
            PomodoroPresetKind.System,
            autoStartBreaks,
            autoStartFocusSessions,
            createdAt);
    }

    public Result Rename(
        string newName,
        DateTimeOffset renamedAt)
    {
        Result editableResult = EnsureEditable();

        if (editableResult.IsFailure)
            return editableResult;

        Result<PomodoroPresetName> nameResult =
            PomodoroPresetName.Create(newName);

        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
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

    public Result ChangeCycleSettings(
        TimeSpan focusDuration,
        TimeSpan shortBreakDuration,
        TimeSpan longBreakDuration,
        int sessionsBeforeLongBreak,
        DateTimeOffset changedAt)
    {
        Result editableResult = EnsureEditable();

        if (editableResult.IsFailure)
            return editableResult;

        Result<PomodoroCycleSettings> settingsResult =
            PomodoroCycleSettings.Create(
                focusDuration,
                shortBreakDuration,
                longBreakDuration,
                sessionsBeforeLongBreak);

        if (settingsResult.IsFailure)
        {
            return Result.Failure(
                settingsResult.Error);
        }

        DateTimeOffset changedAtUtc =
            changedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(changedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        if (CycleSettings == settingsResult.Value)
            return Result.Success();

        CycleSettings = settingsResult.Value;
        UpdatedAtUtc = changedAtUtc;

        return Result.Success();
    }

    public Result ChangeAutomation(
        bool autoStartBreaks,
        bool autoStartFocusSessions,
        DateTimeOffset changedAt)
    {
        Result editableResult = EnsureEditable();

        if (editableResult.IsFailure)
            return editableResult;

        DateTimeOffset changedAtUtc =
            changedAt.ToUniversalTime();

        Result timeResult =
            ValidateModificationTime(changedAtUtc);

        if (timeResult.IsFailure)
            return timeResult;

        if (AutoStartBreaks == autoStartBreaks &&
            AutoStartFocusSessions == autoStartFocusSessions)
        {
            return Result.Success();
        }

        AutoStartBreaks = autoStartBreaks;
        AutoStartFocusSessions = autoStartFocusSessions;
        UpdatedAtUtc = changedAtUtc;

        return Result.Success();
    }

    public Result<PomodoroPreset> CreateCopy(
        string copyName,
        DateTimeOffset createdAt)
    {
        return CreateUser(
            copyName,
            CycleSettings.FocusDuration,
            CycleSettings.ShortBreakDuration,
            CycleSettings.LongBreakDuration,
            CycleSettings.SessionsBeforeLongBreak,
            AutoStartBreaks,
            AutoStartFocusSessions,
            createdAt);
    }

    public Result<TimeSpan> GetDurationFor(
        FocusSessionType sessionType)
    {
        return CycleSettings.GetDurationFor(sessionType);
    }

    public bool IsLongBreakDue(
        int completedFocusSessions)
    {
        return CycleSettings.IsLongBreakDue(
            completedFocusSessions);
    }

    private static Result<PomodoroPreset> Create(
        string name,
        TimeSpan focusDuration,
        TimeSpan shortBreakDuration,
        TimeSpan longBreakDuration,
        int sessionsBeforeLongBreak,
        PomodoroPresetKind kind,
        bool autoStartBreaks,
        bool autoStartFocusSessions,
        DateTimeOffset createdAt)
    {
        Result<PomodoroPresetName> nameResult =
            PomodoroPresetName.Create(name);

        if (nameResult.IsFailure)
        {
            return Result<PomodoroPreset>.Failure(
                nameResult.Error);
        }

        Result<PomodoroCycleSettings> settingsResult =
            PomodoroCycleSettings.Create(
                focusDuration,
                shortBreakDuration,
                longBreakDuration,
                sessionsBeforeLongBreak);

        if (settingsResult.IsFailure)
        {
            return Result<PomodoroPreset>.Failure(
                settingsResult.Error);
        }

        DateTimeOffset createdAtUtc =
            createdAt.ToUniversalTime();

        var preset = new PomodoroPreset(
            nameResult.Value,
            settingsResult.Value,
            kind,
            autoStartBreaks,
            autoStartFocusSessions,
            createdAtUtc);

        return Result<PomodoroPreset>.Success(preset);
    }

    private Result EnsureEditable()
    {
        if (IsSystem)
        {
            return Result.Failure(
                PomodoroPresetErrors.SystemPresetCannotBeModified);
        }

        return Result.Success();
    }

    private Result ValidateModificationTime(
        DateTimeOffset modificationTimeUtc)
    {
        if (modificationTimeUtc < UpdatedAtUtc)
        {
            return Result.Failure(
                PomodoroPresetErrors.ModificationTimeIsInvalid(
                    modificationTimeUtc,
                    UpdatedAtUtc));
        }

        return Result.Success();
    }
}