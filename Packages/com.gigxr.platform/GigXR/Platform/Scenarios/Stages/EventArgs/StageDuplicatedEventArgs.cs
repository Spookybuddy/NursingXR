namespace GIGXR.Platform.Scenarios.Stages.EventArgs
{
    using System;

    /// <summary>
    /// EventArgs that holds the new Stage Id when a stage is duplicated
    /// </summary>
    public class StageDuplicatedEventArgs : EventArgs
    {
        public Guid StageDuplicateID { get; }
        
        public Guid NewStageID { get; }

        public StageDuplicatedEventArgs(Guid stageIDToDuplicate, Guid newStageDuplciate)
        {
            StageDuplicateID = stageIDToDuplicate;

            NewStageID = newStageDuplciate;
        }
    }
}