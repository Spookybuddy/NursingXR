namespace GIGXR.Platform.Scenarios.EventArgs
{
    using System;
    using Data;

    public class ScenarioStatusChangedEventArgs : EventArgs
    {
        public ScenarioStatus OldStatus;
        public ScenarioStatus NewStatus;
    }
}
