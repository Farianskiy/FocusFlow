using FocusFlow.Domain.Common;
using FocusFlow.Domain.Tasks;

namespace FocusFlow.Domain.Projects;

public sealed record FocusProjectTaskSnapshot
{
    private FocusProjectTaskSnapshot(
        FocusTaskStatus status,
        int estimatedPomodoros,
        int completedPomodoros)
    {
        Status = status;
        EstimatedPomodoros = estimatedPomodoros;
        CompletedPomodoros = completedPomodoros;
    }

    public FocusTaskStatus Status { get; }

    public int EstimatedPomodoros { get; }

    public int CompletedPomodoros { get; }

    public static Result<FocusProjectTaskSnapshot> Create(
        FocusTaskStatus status,
        int estimatedPomodoros,
        int completedPomodoros)
    {
        if (status == FocusTaskStatus.None)
        {
            return Result<FocusProjectTaskSnapshot>.Failure(
                FocusProjectErrors
                    .StatisticsTaskStatusIsInvalid);
        }

        if (estimatedPomodoros <
                PomodoroEstimate.MinimumValue ||
            estimatedPomodoros >
                PomodoroEstimate.MaximumValue)
        {
            return Result<FocusProjectTaskSnapshot>.Failure(
                FocusProjectErrors
                    .StatisticsEstimateIsInvalid);
        }

        if (completedPomodoros < 0)
        {
            return Result<FocusProjectTaskSnapshot>.Failure(
                FocusProjectErrors
                    .StatisticsCompletedPomodorosIsInvalid);
        }

        return Result<FocusProjectTaskSnapshot>.Success(
            new FocusProjectTaskSnapshot(
                status,
                estimatedPomodoros,
                completedPomodoros));
    }

    public static FocusProjectTaskSnapshot From(
        FocusTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return new FocusProjectTaskSnapshot(
            task.Status,
            task.Estimate.Value,
            task.CompletedPomodoroCount);
    }
}