using FocusFlow.Domain.Common;

namespace FocusFlow.Domain.Tasks;

public static class FocusTaskErrors
{
    public static readonly DomainError TitleIsRequired = new(
        "FocusTask.Title.Required",
        "Название задачи обязательно.");

    public static readonly DomainError TitleIsTooLong = new(
        "FocusTask.Title.TooLong",
        $"Название задачи не может быть длиннее {FocusTaskTitle.MaximumLength} символов.");

    public static readonly DomainError DescriptionIsTooLong = new(
        "FocusTask.Description.TooLong",
        $"Описание задачи не может быть длиннее {FocusTaskDescription.MaximumLength} символов.");

    public static readonly DomainError EstimateIsTooSmall = new(
        "FocusTask.Estimate.TooSmall",
        $"Ожидаемое количество Pomodoro не может быть меньше {PomodoroEstimate.MinimumValue}.");

    public static readonly DomainError EstimateIsTooLarge = new(
        "FocusTask.Estimate.TooLarge",
        $"Ожидаемое количество Pomodoro не может превышать {PomodoroEstimate.MaximumValue}.");

    public static readonly DomainError ProjectIdIsInvalid = new(
        "FocusTask.ProjectId.Invalid",
        "Идентификатор проекта не может быть пустым.");

    public static readonly DomainError FocusSessionIdIsInvalid = new(
        "FocusTask.FocusSessionId.Invalid",
        "Идентификатор фокус-сессии не может быть пустым.");

    public static readonly DomainError FocusSessionAlreadyRegistered = new(
        "FocusTask.FocusSession.AlreadyRegistered",
        "Эта фокус-сессия уже зарегистрирована в задаче.");

    public static readonly DomainError FocusSessionIsNotRegistered = new(
        "FocusTask.FocusSession.NotRegistered",
        "Указанная фокус-сессия не зарегистрирована в задаче.");

    public static DomainError CannotModify(FocusTaskStatus status)
    {
        return new DomainError(
            "FocusTask.Modify.InvalidStatus",
            $"Нельзя изменять задачу в состоянии {status}. Сначала возобновите ее.");
    }

    public static DomainError CannotStart(FocusTaskStatus status)
    {
        return new DomainError(
            "FocusTask.Start.InvalidStatus",
            $"Нельзя начать задачу в состоянии {status}.");
    }

    public static DomainError CannotRegisterSession(
        FocusTaskStatus status)
    {
        return new DomainError(
            "FocusTask.RegisterSession.InvalidStatus",
            $"Нельзя зарегистрировать фокус-сессию для задачи в состоянии {status}.");
    }

    public static DomainError CannotRemoveSession(
        FocusTaskStatus status)
    {
        return new DomainError(
            "FocusTask.RemoveSession.InvalidStatus",
            $"Нельзя удалить фокус-сессию из задачи в состоянии {status}.");
    }

    public static DomainError CannotComplete(
        FocusTaskStatus status)
    {
        return new DomainError(
            "FocusTask.Complete.InvalidStatus",
            $"Нельзя завершить задачу в состоянии {status}.");
    }

    public static DomainError CannotCancel(
        FocusTaskStatus status)
    {
        return new DomainError(
            "FocusTask.Cancel.InvalidStatus",
            $"Нельзя отменить задачу в состоянии {status}.");
    }

    public static DomainError CannotReopen(
        FocusTaskStatus status)
    {
        return new DomainError(
            "FocusTask.Reopen.InvalidStatus",
            $"Нельзя возобновить задачу в состоянии {status}.");
    }

    public static DomainError ModificationTimeIsInvalid(
        DateTimeOffset modificationTimeUtc,
        DateTimeOffset currentUpdatedAtUtc)
    {
        return new DomainError(
            "FocusTask.ModificationTime.Invalid",
            $"Время изменения {modificationTimeUtc:O} не может быть раньше {currentUpdatedAtUtc:O}.");
    }
}