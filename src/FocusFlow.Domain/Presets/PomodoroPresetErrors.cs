using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Presets;

public static class PomodoroPresetErrors
{
    public static readonly DomainError NameIsRequired = new(
        "PomodoroPreset.Name.Required",
        "Название пресета обязательно.");

    public static readonly DomainError NameIsTooLong = new(
        "PomodoroPreset.Name.TooLong",
        $"Название пресета не может быть длиннее {PomodoroPresetName.MaximumLength} символов.");

    public static readonly DomainError FocusDurationIsTooShort = new(
        "PomodoroPreset.FocusDuration.TooShort",
        $"Продолжительность рабочей сессии не может быть меньше {PomodoroCycleSettings.MinimumDurationMinutes} минуты.");

    public static readonly DomainError FocusDurationIsTooLong = new(
        "PomodoroPreset.FocusDuration.TooLong",
        $"Продолжительность рабочей сессии не может превышать {PomodoroCycleSettings.MaximumDurationHours} часов.");

    public static readonly DomainError ShortBreakDurationIsTooShort = new(
        "PomodoroPreset.ShortBreakDuration.TooShort",
        $"Продолжительность короткого перерыва не может быть меньше {PomodoroCycleSettings.MinimumDurationMinutes} минуты.");

    public static readonly DomainError ShortBreakDurationIsTooLong = new(
        "PomodoroPreset.ShortBreakDuration.TooLong",
        $"Продолжительность короткого перерыва не может превышать {PomodoroCycleSettings.MaximumDurationHours} часов.");

    public static readonly DomainError LongBreakDurationIsTooShort = new(
        "PomodoroPreset.LongBreakDuration.TooShort",
        $"Продолжительность длинного перерыва не может быть меньше {PomodoroCycleSettings.MinimumDurationMinutes} минуты.");

    public static readonly DomainError LongBreakDurationIsTooLong = new(
        "PomodoroPreset.LongBreakDuration.TooLong",
        $"Продолжительность длинного перерыва не может превышать {PomodoroCycleSettings.MaximumDurationHours} часов.");

    public static readonly DomainError LongBreakIsShorterThanShortBreak = new(
        "PomodoroPreset.LongBreakDuration.Invalid",
        "Длинный перерыв не может быть короче короткого перерыва.");

    public static readonly DomainError SessionsBeforeLongBreakIsInvalid = new(
        "PomodoroPreset.SessionsBeforeLongBreak.Invalid",
        $"Количество рабочих сессий до длинного перерыва должно быть от {PomodoroCycleSettings.MinimumSessionsBeforeLongBreak} до {PomodoroCycleSettings.MaximumSessionsBeforeLongBreak}.");

    public static readonly DomainError SystemPresetCannotBeModified = new(
        "PomodoroPreset.SystemPreset.CannotModify",
        "Системный пресет нельзя изменять. Создайте его копию.");

    public static readonly DomainError SessionTypeIsRequired = new(
        "PomodoroPreset.SessionType.Required",
        "Для получения продолжительности необходимо указать тип сессии.");

    public static DomainError ModificationTimeIsInvalid(
        DateTimeOffset modificationTimeUtc,
        DateTimeOffset currentUpdatedAtUtc)
    {
        return new DomainError(
            "PomodoroPreset.ModificationTime.Invalid",
            $"Время изменения {modificationTimeUtc:O} не может быть раньше {currentUpdatedAtUtc:O}.");
    }
}