namespace GIGXR.Platform.Scenarios.EventArgs
{
    using System;
    using Data;

    public class ScenarioPlayingEventArgs : EventArgs
    {
        public ScenarioStatus StatusBeforePlay { get; }

        public ScenarioPlayingEventArgs(ScenarioStatus statusBeforePlay)
        {
            StatusBeforePlay = statusBeforePlay;
        }
    }
}