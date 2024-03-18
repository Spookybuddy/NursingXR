namespace GIGXR.Platform.Scenarios.Stages.EventArgs
{
    using System;

    /// <summary>
    /// EventArgs that holds data when a stage is deleted from the list of stages
    /// </summary>
    public class StageRemovedEventArgs : EventArgs
    {
        public Guid RemovedStageID { get; }

        public StageRemovedEventArgs(Guid stageIDToRemove)
        {
            RemovedStageID = stageIDToRemove;
        }
    }
}