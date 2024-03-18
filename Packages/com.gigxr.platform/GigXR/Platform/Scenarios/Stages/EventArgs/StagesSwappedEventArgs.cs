namespace GIGXR.Platform.Scenarios.Stages.EventArgs
{
    using System;

    /// <summary>
    /// EventArgs that holds two stage data so they can be swapped in their display list
    /// </summary>
    public class StagesSwappedEventArgs : EventArgs
    {
        public Guid FirstStage { get; }
        public Guid SecondStage { get; }

        public StagesSwappedEventArgs(Guid first, Guid second)
        {
            FirstStage = first;
            SecondStage = second;
        }
    }
}