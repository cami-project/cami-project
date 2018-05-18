namespace DSS.Rules.Library
{
    public interface IActivityLog
    {
        bool EventOfTypeHappenedAfter(string owner, ActivityType before, ActivityType after);
        void Log(Activity activity);
    }
}