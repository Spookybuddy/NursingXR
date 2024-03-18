namespace GIGXR.Platform.Scenarios.Stages.EventArgs
{
    using System;

    /// <summary>
    /// EventArgs that holds data when a stage is renamed
    /// </summary>
    public class StageRenamedEventArgs : EventArgs
    {
        public Guid StageID { get; }
        public string NewName { get; private set; }

        public StageRenamedEventArgs(Guid stageID, string newName)
        {
            StageID = stageID;
            NewName = newName;
        }
    }
}