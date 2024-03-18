namespace GIGXR.Platform.Scenarios.Stages
{
    using EventArgs;
    using GIGXR.Platform.Data;
    using System;
    using System.Collections.Generic;
    using Stage = Data.Stage;

    /// <summary>
    /// Responsible for managing the lifecycle of stages.
    /// </summary>
    public interface IStageManager
    {
        event EventHandler<StageCreatedEventArgs> StageCreated;
        event EventHandler<StageDuplicatedEventArgs> StageDuplicated;
        event EventHandler<StageSwitchedEventArgs> EarlyStageSwitched;
        event EventHandler<StageSwitchedEventArgs> StageSwitched;
        event EventHandler<StageSwitchedEventArgs> LateStageSwitched;
        event EventHandler<StageRenamedEventArgs> StageRenamed;
        event EventHandler<StageRemovedEventArgs> StageRemoved;
        event EventHandler<AllStagesRemovedEventArgs> AllStagesRemoved;
        event EventHandler<StagesSwappedEventArgs> StagesSwapped;
        event EventHandler<StagesLoadedEventArgs> StagesLoaded;

        IEnumerable<Stage> Stages { get; }
        Stage CurrentStage { get; }
        int CurrentStageIndex { get; }

        bool ChangesSaved { get; }
        bool IsSwitchingStage { get; }

        Stage GetStageById(Guid stageId);
        Stage CreateStage();
        Stage CreateStageWithID(Guid stageId);
        Stage DuplicateStage(Guid stageId);
        void SwitchToStage(Guid stageId);
        void SwitchToNextStage();
        void RenameStage(Guid stageId, string name);
        void RemoveStage(Guid stageId);
        void RemoveAllStages();
        void SwapStages(Guid stageId1, Guid stageId2);
        void LoadStages(IEnumerable<Stage> stagesToLoad);

        void MarkChangesSaved(bool saved);

        public TimeSpan TimeInPlayingStage { get; }
        public TimeSpan ElapsedTimeInStage { get; }

        void SetStageOffset(int stageOffset);
        void StartTimer();
        void StopTimer();
        void ResetTimer();
    }
}