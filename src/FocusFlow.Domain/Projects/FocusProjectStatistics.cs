using FocusFlow.Domain.Tasks;

namespace FocusFlow.Domain.Projects;

public sealed class FocusProjectStatistics
{
    private FocusProjectStatistics(
        int totalTaskCount,
        int plannedTaskCount,
        int inProgressTaskCount,
        int completedTaskCount,
        int cancelledTaskCount,
        int totalEstimatedPomodoros,
        int totalCompletedPomodoros,
        int remainingEstimatedPomodoros,
        decimal taskCompletionPercentage,
        decimal pomodoroProgressPercentage)
    {
        TotalTaskCount = totalTaskCount;
        PlannedTaskCount = plannedTaskCount;
        InProgressTaskCount = inProgressTaskCount;
        CompletedTaskCount = completedTaskCount;
        CancelledTaskCount = cancelledTaskCount;

        TotalEstimatedPomodoros =
            totalEstimatedPomodoros;

        TotalCompletedPomodoros =
            totalCompletedPomodoros;

        RemainingEstimatedPomodoros =
            remainingEstimatedPomodoros;

        TaskCompletionPercentage =
            taskCompletionPercentage;

        PomodoroProgressPercentage =
            pomodoroProgressPercentage;
    }

    public int TotalTaskCount { get; }

    public int PlannedTaskCount { get; }

    public int InProgressTaskCount { get; }

    public int CompletedTaskCount { get; }

    public int CancelledTaskCount { get; }

    public int ActiveTaskCount =>
        PlannedTaskCount + InProgressTaskCount;

    public int TotalEstimatedPomodoros { get; }

    public int TotalCompletedPomodoros { get; }

    public int RemainingEstimatedPomodoros { get; }

    public decimal TaskCompletionPercentage { get; }

    public decimal PomodoroProgressPercentage { get; }

    public bool IsEmpty => TotalTaskCount == 0;

    public static FocusProjectStatistics Calculate(
        IEnumerable<FocusProjectTaskSnapshot> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        FocusProjectTaskSnapshot[] taskArray =
            tasks.ToArray();

        int plannedTaskCount = taskArray.Count(
            task =>
                task.Status ==
                FocusTaskStatus.Planned);

        int inProgressTaskCount = taskArray.Count(
            task =>
                task.Status ==
                FocusTaskStatus.InProgress);

        int completedTaskCount = taskArray.Count(
            task =>
                task.Status ==
                FocusTaskStatus.Completed);

        int cancelledTaskCount = taskArray.Count(
            task =>
                task.Status ==
                FocusTaskStatus.Cancelled);

        FocusProjectTaskSnapshot[] trackedTasks =
            taskArray
                .Where(
                    task =>
                        task.Status !=
                        FocusTaskStatus.Cancelled)
                .ToArray();

        int totalEstimatedPomodoros =
            trackedTasks.Sum(
                task => task.EstimatedPomodoros);

        int totalCompletedPomodoros =
            trackedTasks.Sum(
                task => task.CompletedPomodoros);

        int remainingEstimatedPomodoros =
            trackedTasks.Sum(
                task => Math.Max(
                    0,
                    task.EstimatedPomodoros -
                    task.CompletedPomodoros));

        int taskCompletionDenominator =
            plannedTaskCount +
            inProgressTaskCount +
            completedTaskCount;

        decimal taskCompletionPercentage =
            taskCompletionDenominator == 0
                ? 0m
                : completedTaskCount *
                  100m /
                  taskCompletionDenominator;

        decimal pomodoroProgressPercentage =
            totalEstimatedPomodoros == 0
                ? 0m
                : Math.Min(
                    100m,
                    totalCompletedPomodoros *
                    100m /
                    totalEstimatedPomodoros);

        return new FocusProjectStatistics(
            totalTaskCount: taskArray.Length,
            plannedTaskCount: plannedTaskCount,
            inProgressTaskCount: inProgressTaskCount,
            completedTaskCount: completedTaskCount,
            cancelledTaskCount: cancelledTaskCount,
            totalEstimatedPomodoros:
                totalEstimatedPomodoros,
            totalCompletedPomodoros:
                totalCompletedPomodoros,
            remainingEstimatedPomodoros:
                remainingEstimatedPomodoros,
            taskCompletionPercentage:
                taskCompletionPercentage,
            pomodoroProgressPercentage:
                pomodoroProgressPercentage);
    }
}