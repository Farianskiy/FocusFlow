using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Sessions;

public static class FocusSessionErrors
{
    public static readonly DomainError TypeIsRequired = new(
        "FocusSession.Type.Required",
        "Необходимо указать тип сессии.");

    public static readonly DomainError TaskIdIsInvalid = new(
        "FocusSession.TaskId.Invalid",
        "Идентификатор задачи не может быть пустым.");

    public static readonly DomainError PresetIdIsInvalid = new(
        "FocusSession.PresetId.Invalid",
        "Идентификатор пресета не может быть пустым.");

    public static readonly DomainError BreakCannotHaveTask = new(
        "FocusSession.Break.TaskNotAllowed",
        "Задачу можно связать только с рабочей сессией.");

    public static readonly DomainError DurationIsTooShort = new(
        "FocusSession.Duration.TooShort",
        $"Продолжительность сессии не может быть меньше {SessionDuration.MinimumMinutes} минуты.");

    public static readonly DomainError DurationIsTooLong = new(
        "FocusSession.Duration.TooLong",
        $"Продолжительность сессии не может превышать {SessionDuration.MaximumHours} часов.");

    public static readonly DomainError SessionHasExpired = new(
        "FocusSession.Expired",
        "Время сессии уже истекло. Сессию необходимо завершить.");

    public static DomainError CannotPause(FocusSessionStatus status)
    {
        return new DomainError(
            "FocusSession.Pause.InvalidStatus",
            $"Нельзя поставить на паузу сессию в состоянии {status}.");
    }

    public static DomainError CannotResume(FocusSessionStatus status)
    {
        return new DomainError(
            "FocusSession.Resume.InvalidStatus",
            $"Нельзя продолжить сессию в состоянии {status}.");
    }

    public static DomainError CannotComplete(FocusSessionStatus status)
    {
        return new DomainError(
            "FocusSession.Complete.InvalidStatus",
            $"Нельзя завершить сессию в состоянии {status}.");
    }

    public static DomainError CannotCancel(FocusSessionStatus status)
    {
        return new DomainError(
            "FocusSession.Cancel.InvalidStatus",
            $"Нельзя отменить сессию в состоянии {status}.");
    }

    public static DomainError TransitionTimeIsInvalid(
        DateTimeOffset transitionTimeUtc,
        DateTimeOffset minimumTimeUtc)
    {
        return new DomainError(
            "FocusSession.TransitionTime.Invalid",
            $"Время операции {transitionTimeUtc:O} не может быть раньше {minimumTimeUtc:O}.");
    }
}