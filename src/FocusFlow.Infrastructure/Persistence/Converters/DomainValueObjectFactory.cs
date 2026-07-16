using FocusFlow.Domain.Common;
using FocusFlow.Domain.Presets;
using FocusFlow.Domain.Projects;
using FocusFlow.Domain.Sessions;
using FocusFlow.Domain.Tasks;

namespace FocusFlow.Infrastructure.Persistence.Converters;

internal static class DomainValueObjectFactory
{
    public static FocusTaskTitle CreateTaskTitle(
        string value)
    {
        return GetValue(
            FocusTaskTitle.Create(value),
            nameof(FocusTaskTitle));
    }

    public static FocusTaskDescription CreateTaskDescription(
        string value)
    {
        return GetValue(
            FocusTaskDescription.Create(value),
            nameof(FocusTaskDescription));
    }

    public static PomodoroEstimate CreatePomodoroEstimate(
        int value)
    {
        return GetValue(
            PomodoroEstimate.Create(value),
            nameof(PomodoroEstimate));
    }

    public static PomodoroPresetName CreatePresetName(
        string value)
    {
        return GetValue(
            PomodoroPresetName.Create(value),
            nameof(PomodoroPresetName));
    }

    public static SessionDuration CreateSessionDuration(
        long ticks)
    {
        return GetValue(
            SessionDuration.Create(
                TimeSpan.FromTicks(ticks)),
            nameof(SessionDuration));
    }

    public static FocusProjectName CreateProjectName(
        string value)
    {
        return GetValue(
            FocusProjectName.Create(value),
            nameof(FocusProjectName));
    }

    public static FocusProjectDescription CreateProjectDescription(
        string value)
    {
        return GetValue(
            FocusProjectDescription.Create(value),
            nameof(FocusProjectDescription));
    }

    public static FocusProjectColor CreateProjectColor(
        string value)
    {
        return GetValue(
            FocusProjectColor.Create(value),
            nameof(FocusProjectColor));
    }

    public static PomodoroCycleSettings CreateCycleSettings(
        TimeSpan focusDuration,
        TimeSpan shortBreakDuration,
        TimeSpan longBreakDuration,
        int sessionsBeforeLongBreak)
    {
        return GetValue(
            PomodoroCycleSettings.Create(
                focusDuration,
                shortBreakDuration,
                longBreakDuration,
                sessionsBeforeLongBreak),
            nameof(PomodoroCycleSettings));
    }

    private static T GetValue<T>(
        Result<T> result,
        string valueObjectName)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Не удалось восстановить {valueObjectName} из базы данных. " +
                $"Ошибка: {result.Error.Code}. {result.Error.Message}");
        }

        return result.Value;
    }
}