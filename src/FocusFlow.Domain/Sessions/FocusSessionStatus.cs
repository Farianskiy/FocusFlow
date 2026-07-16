namespace FocusFlow.Domain.Sessions;

public enum FocusSessionStatus
{
    None = 0,

    Running = 1,

    Paused = 2,

    Completed = 3,

    Cancelled = 4
}