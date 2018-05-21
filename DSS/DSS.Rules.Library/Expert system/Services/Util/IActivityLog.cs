namespace DSS.Rules.Library
{
    public enum AssumedState { Null, Awake, Speeping, Outside }

    public interface IActivityLog
    {
        bool EventOfTypeHappenedAfter(string owner, ActivityType before, ActivityType after);
        bool EventOfTypeHappened(string owner, ActivityType before, int minBack);
        bool DidNotHappenAfter(string owner, ActivityType after, ActivityType didNotHappen);

        void Log(Activity activity);


        void ChangeAssumedState(string owner, AssumedState state);
        AssumedState GetAssumedState(string owner);
        bool IsAssumedState(string owner, AssumedState state);

    }
}