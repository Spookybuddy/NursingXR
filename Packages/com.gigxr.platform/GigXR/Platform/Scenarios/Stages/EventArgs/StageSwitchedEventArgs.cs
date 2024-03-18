namespace GIGXR.Platform.Scenarios.Stages.EventArgs
{
    using System;

    /// <summary>
    /// EventArgs that holds data related to switching into a new stage
    /// </summary>
    public class StageSwitchedEventArgs : EventArgs
    {
        public Guid NewStageID { get; }

        public int PreviousStageTime { get; }

        public StageSwitchedEventArgs(Guid stageID, int previousStageTime)
        {
            NewStageID = stageID;
            PreviousStageTime = previousStageTime;
        }
    }
}