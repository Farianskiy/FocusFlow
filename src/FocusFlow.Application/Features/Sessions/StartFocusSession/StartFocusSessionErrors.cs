using FocusFlow.Domain.Common;
using FocusFlow.Domain.Tasks;

namespace FocusFlow.Application.Features.Sessions.StartFocusSession;

public static class StartFocusSessionErrors
{
    public static readonly DomainError TaskIdIsInvalid = new(
        "StartFocusSession.TaskId.Invalid",
        "Идентификатор задачи не может быть пустым.");

    public static readonly DomainError PresetIdIsInvalid = new(
        "StartFocusSession.PresetId.Invalid",
        "Идентификатор пресета не может быть пустым.");

    public static readonly DomainError ActiveSessionAlreadyExists = new(
        "StartFocusSession.ActiveSession.AlreadyExists",
        "Нельзя запустить новую сессию, пока существует другая активная сессия.");

    public static DomainError TaskNotFound(
        Guid taskId)
    {
        return new DomainError(
            "StartFocusSession.Task.NotFound",
            $"Задача с идентификатором {taskId} не найдена.");
    }

    public static DomainError PresetNotFound(
        Guid presetId)
    {
        return new DomainError(
            "StartFocusSession.Preset.NotFound",
            $"Пресет с идентификатором {presetId} не найден.");
    }

    public static DomainError TaskCannotBeFocused(
        FocusTaskStatus status)
    {
        return new DomainError(
            "StartFocusSession.Task.InvalidStatus",
            $"Нельзя запустить фокус-сессию для задачи в состоянии {status}.");
    }
}