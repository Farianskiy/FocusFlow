namespace FocusFlow.Domain.Sessions;

public enum FocusSessionCompletionReason
{
    None = 0,

    TimerElapsed = 1,

    CompletedManually = 2
}