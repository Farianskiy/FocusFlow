using FocusFlow.Domain.Common;
using FocusFlow.Domain.Sessions;

namespace FocusFlow.Domain.Presets;

public sealed record PomodoroCycleSettings
{
    public const int MinimumDurationMinutes = 1;

    public const int MaximumDurationHours = 12;

    public const int MinimumSessionsBeforeLongBreak = 1;

    public const int MaximumSessionsBeforeLongBreak = 12;

    private PomodoroCycleSettings(
        TimeSpan focusDuration,
        TimeSpan shortBreakDuration,
        TimeSpan longBreakDuration,
        int sessionsBeforeLongBreak)
    {
        FocusDuration = focusDuration;
        ShortBreakDuration = shortBreakDuration;
        LongBreakDuration = longBreakDuration;
        SessionsBeforeLongBreak = sessionsBeforeLongBreak;
    }

    public TimeSpan FocusDuration { get; }

    public TimeSpan ShortBreakDuration { get; }

    public TimeSpan LongBreakDuration { get; }

    public int SessionsBeforeLongBreak { get; }

    public static Result<PomodoroCycleSettings> Create(
        TimeSpan focusDuration,
        TimeSpan shortBreakDuration,
        TimeSpan longBreakDuration,
        int sessionsBeforeLongBreak)
    {
        TimeSpan minimumDuration =
            TimeSpan.FromMinutes(MinimumDurationMinutes);

        TimeSpan maximumDuration =
            TimeSpan.FromHours(MaximumDurationHours);

        if (focusDuration < minimumDuration)
        {
            return Result<PomodoroCycleSettings>.Failure(
                PomodoroPresetErrors.FocusDurationIsTooShort);
        }

        if (focusDuration > maximumDuration)
        {
            return Result<PomodoroCycleSettings>.Failure(
                PomodoroPresetErrors.FocusDurationIsTooLong);
        }

        if (shortBreakDuration < minimumDuration)
        {
            return Result<PomodoroCycleSettings>.Failure(
                PomodoroPresetErrors.ShortBreakDurationIsTooShort);
        }

        if (shortBreakDuration > maximumDuration)
        {
            return Result<PomodoroCycleSettings>.Failure(
                PomodoroPresetErrors.ShortBreakDurationIsTooLong);
        }

        if (longBreakDuration < minimumDuration)
        {
            return Result<PomodoroCycleSettings>.Failure(
                PomodoroPresetErrors.LongBreakDurationIsTooShort);
        }

        if (longBreakDuration > maximumDuration)
        {
            return Result<PomodoroCycleSettings>.Failure(
                PomodoroPresetErrors.LongBreakDurationIsTooLong);
        }

        if (longBreakDuration < shortBreakDuration)
        {
            return Result<PomodoroCycleSettings>.Failure(
                PomodoroPresetErrors.LongBreakIsShorterThanShortBreak);
        }

        if (sessionsBeforeLongBreak <
                MinimumSessionsBeforeLongBreak ||
            sessionsBeforeLongBreak >
                MaximumSessionsBeforeLongBreak)
        {
            return Result<PomodoroCycleSettings>.Failure(
                PomodoroPresetErrors.SessionsBeforeLongBreakIsInvalid);
        }

        return Result<PomodoroCycleSettings>.Success(
            new PomodoroCycleSettings(
                focusDuration,
                shortBreakDuration,
                longBreakDuration,
                sessionsBeforeLongBreak));
    }

    public Result<TimeSpan> GetDurationFor(
        FocusSessionType sessionType)
    {
        return sessionType switch
        {
            FocusSessionType.Focus =>
                Result<TimeSpan>.Success(FocusDuration),

            FocusSessionType.ShortBreak =>
                Result<TimeSpan>.Success(ShortBreakDuration),

            FocusSessionType.LongBreak =>
                Result<TimeSpan>.Success(LongBreakDuration),

            _ =>
                Result<TimeSpan>.Failure(
                    PomodoroPresetErrors.SessionTypeIsRequired)
        };
    }

    public bool IsLongBreakDue(
        int completedFocusSessions)
    {
        return completedFocusSessions > 0 &&
               completedFocusSessions %
                   SessionsBeforeLongBreak == 0;
    }
}