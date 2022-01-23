namespace Quarter.Core.Events
{
    // Events are effects of something happening.
    //
    // Thoughts:
    // - Should events be persisted next to the aggregate?
    // - Should events be versioned in concrete classes, and having migrate functions going from one to the other?
    // - Should events also be used for ephemeral things (e.g. generated some report) - this way it can be used by the UI
    public interface IEvent
    {
    }
}