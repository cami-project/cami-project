namespace DSS.Rules.Library
{
    public interface IActivityLog
    {
        bool EventOfTypeHappenedAfter(string owner, ActivityType before, ActivityType after);
        bool EventOfTypeHappened(string owner, ActivityType before, int minBack);

        void Log(Activity activity);
    }
}