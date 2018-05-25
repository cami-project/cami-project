namespace DSS.Rules.Library
{
    public enum AssumedState { Null, Awake, Sleeping, Outside }

    public interface IActivityLog
    {
        bool EventOfTypeHappenedAfter(string owner, ActivityType before, ActivityType after);
        bool EventOfTypeHappened(string owner, ActivityType before, int minBack);
        bool DidNotHappenAfter(string owner, ActivityType after, ActivityType didNotHappen);
        int TimeSince(IEvent e, ActivityType type);

        void Log(Activity activity);

        void ChangeAssumedState(string owner, AssumedState state);
        AssumedState GetAssumedState(string owner);
        bool IsAssumedState(string owner, AssumedState state);

    }
}