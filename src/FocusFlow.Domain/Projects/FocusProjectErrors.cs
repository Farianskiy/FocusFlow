using FocusFlow.Domain.Common;
using FocusFlow.Domain.Tasks;

namespace FocusFlow.Domain.Projects;

public static class FocusProjectErrors
{
    public static readonly DomainError NameIsRequired = new(
        "FocusProject.Name.Required",
        "Название проекта обязательно.");

    public static readonly DomainError NameIsTooLong = new(
        "FocusProject.Name.TooLong",
        $"Название проекта не может быть длиннее {FocusProjectName.MaximumLength} символов.");

    public static readonly DomainError DescriptionIsTooLong = new(
        "FocusProject.Description.TooLong",
        $"Описание проекта не может быть длиннее {FocusProjectDescription.MaximumLength} символов.");

    public static readonly DomainError ColorIsInvalid = new(
        "FocusProject.Color.Invalid",
        "Цвет проекта должен быть указан в шестизначном HEX-формате, например #6750A4.");

    public static readonly DomainError ArchivedProjectCannotBeModified = new(
        "FocusProject.Archived.CannotModify",
        "Архивный проект нельзя изменять. Сначала восстановите его.");

    public static readonly DomainError StatisticsTaskStatusIsInvalid = new(
        "FocusProject.Statistics.TaskStatus.Invalid",
        "Для статистики задачи необходимо указать корректный статус.");

    public static readonly DomainError StatisticsEstimateIsInvalid = new(
        "FocusProject.Statistics.Estimate.Invalid",
        $"Оценка задачи должна быть от {PomodoroEstimate.MinimumValue} до {PomodoroEstimate.MaximumValue} Pomodoro.");

    public static readonly DomainError StatisticsCompletedPomodorosIsInvalid = new(
        "FocusProject.Statistics.CompletedPomodoros.Invalid",
        "Количество завершенных Pomodoro не может быть отрицательным.");

    public static DomainError CannotComplete(
        FocusProjectStatus status)
    {
        return new DomainError(
            "FocusProject.Complete.InvalidStatus",
            $"Нельзя завершить проект в состоянии {status}.");
    }

    public static DomainError CannotReopen(
        FocusProjectStatus status)
    {
        return new DomainError(
            "FocusProject.Reopen.InvalidStatus",
            $"Нельзя возобновить проект в состоянии {status}.");
    }

    public static DomainError CannotArchive(
        FocusProjectStatus status)
    {
        return new DomainError(
            "FocusProject.Archive.InvalidStatus",
            $"Нельзя архивировать проект в состоянии {status}. Сначала завершите проект.");
    }

    public static DomainError CannotRestore(
        FocusProjectStatus status)
    {
        return new DomainError(
            "FocusProject.Restore.InvalidStatus",
            $"Нельзя восстановить проект в состоянии {status}.");
    }

    public static DomainError ModificationTimeIsInvalid(
        DateTimeOffset modificationTimeUtc,
        DateTimeOffset currentUpdatedAtUtc)
    {
        return new DomainError(
            "FocusProject.ModificationTime.Invalid",
            $"Время изменения {modificationTimeUtc:O} не может быть раньше {currentUpdatedAtUtc:O}.");
    }
}