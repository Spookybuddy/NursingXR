namespace GIGXR.Platform.Scenarios.Stages.EventArgs
{
    using System;

    /// <summary>
    /// EventArgs that holds data for a new stage when it is created
    /// </summary>
    public class StageCreatedEventArgs : EventArgs
    {
        public Guid StageID { get; }

        public StageCreatedEventArgs(Guid newID)
        {
            StageID = newID;
        }
    }
}