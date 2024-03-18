namespace GIGXR.Platform.Scenarios.EventArgs
{
    using System;

    public class ScenarioPlayModeSetEventArgs : EventArgs
    {
        public ScenarioControlTypes playMode;

        public bool saveValue;

        public ScenarioPlayModeSetEventArgs(ScenarioControlTypes newPlayMode, bool saveNewValue)
        {
            playMode = newPlayMode;
            saveValue = saveNewValue;
        }
    }
}
