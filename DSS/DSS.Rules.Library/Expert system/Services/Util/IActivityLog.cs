namespace DSS.Rules.Library
{
    public enum AssumedState { Null, Awake, Sleeping, Outside }

    public interface IActivityLog
    {
        bool EventOfTypeHappenedAfter(IOwner owner, ActivityType before, ActivityType after);
        bool EventOfTypeHappened(IOwner owner, ActivityType before, int minBack);
        bool DidNotHappenAfter(IOwner owner, ActivityType after, ActivityType didNotHappen);
        int TimeSince(IEvent e, ActivityType type);

        void Log(Activity activity);

        void ChangeAssumedState(IOwner owner, AssumedState state);
        AssumedState GetAssumedState(IOwner owner);
        bool IsAssumedState(IOwner owner, AssumedState state);

    }
}