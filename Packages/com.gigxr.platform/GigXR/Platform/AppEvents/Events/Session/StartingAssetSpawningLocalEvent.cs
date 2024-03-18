namespace GIGXR.Platform.AppEvents.Events.Scenarios
{
    public enum JoinTypes
    {
        AdHoc = 0,
        FromSaved = 1,
        FromSessionPlan = 2,
        Joining = 3
    }

    /// <summary>
    /// Event sent out locally when the session has started to load.
    /// </summary>
    public class StartingJoinSessionLocalEvent : BaseScenarioEvent
    {
        public JoinTypes JoinMethod;

        public StartingJoinSessionLocalEvent(JoinTypes joinMethod)
        {
            JoinMethod = joinMethod;
        }
    }
}
