namespace GIGXR.Platform.Scenarios.EventArgs
{
    using GIGXR.Platform.Scenarios.Data;
    using System;

    public class ScenarioUnloadingEventArgs : EventArgs
    {
    }

    public class ScenarioPathwaySetEventArgs : EventArgs
    {
        public PathwayData givenPathway;

        public bool NewValue;

        public ScenarioPathwaySetEventArgs(PathwayData givenPathway, bool newValue)
        {
            this.givenPathway = givenPathway;
            NewValue = newValue;
        }
    }
}