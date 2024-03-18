namespace GIGXR.Platform.Scenarios.Stages
{
    using Data;
    using EventArgs;
    using GIGXR.Platform.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Responsible for managing and communicating the changes of all the stages. Keeps the stages in an ordered
    /// list as they are added.
    /// </summary>
    public class StageManager : IStageManager
    {
        private readonly Dictionary<Guid, Stage> stages     = new Dictionary<Guid, Stage>();
        private readonly List<Guid>              stageOrder = new List<Guid>();

        public StageManager()
        {
        }

        public event EventHandler<StageCreatedEventArgs> StageCreated;
        public event EventHandler<StageDuplicatedEventArgs> StageDuplicated;
        public event EventHandler<StageSwitchedEventArgs> EarlyStageSwitched;
        public event EventHandler<StageSwitchedEventArgs> StageSwitched;
        public event EventHandler<StageSwitchedEventArgs> LateStageSwitched;
        public event EventHandler<StageRenamedEventArgs> StageRenamed;
        public event EventHandler<StageRemovedEventArgs> StageRemoved;
        public event EventHandler<AllStagesRemovedEventArgs> AllStagesRemoved;
        public event EventHandler<StagesSwappedEventArgs> StagesSwapped;
        public event EventHandler<StagesLoadedEventArgs> StagesLoaded;

        public IEnumerable<Stage> Stages => stageOrder.Select(stageId => stages[stageId]);

        public Stage CurrentStage { get; private set; }

        public bool ChangesSaved { get; private set; }

        /// <summary>
        /// How long the user has been playing in the current stage
        /// </summary>
        private readonly Stopwatch timeInPlayingStage = new Stopwatch();

        private int stageOffsetMillisecondValue = 0;

        public TimeSpan TimeInPlayingStage => new TimeSpan(0, 0, 0, 0, stageOffsetMillisecondValue).Add(timeInPlayingStage.Elapsed);
        public TimeSpan ElapsedTimeInStage => timeInPlayingStage.Elapsed;

        public void SetStageOffset(int stageOffset)
        {
            stageOffsetMillisecondValue = stageOffset;

            RestartOrReset();
        }

        private void RestartOrReset()
        {
            if(timeInPlayingStage.IsRunning)
            {
                timeInPlayingStage.Restart();
            }
            else
            {
                ResetTimer();
            }
        }
        
        public void StartTimer()
        {
            timeInPlayingStage.Start();
        }

        public void StopTimer()
        {
            timeInPlayingStage.Stop();
        }

        public void ResetTimer()
        {
            timeInPlayingStage.Reset();

            stageOffsetMillisecondValue = 0;
        }

        public Stage GetStageById(Guid stageId)
        {
            if (stages.TryGetValue(stageId, out Stage stage))
            {
                return stage;
            }

            return null;
        }

        public Stage CreateStage()
        {
            var stageId = NewGuid();
            return _CreateStage(stageId);
        }

        public Stage CreateStageWithID(Guid stageId)
        {
            return _CreateStage(stageId);
        }

        private Stage _CreateStage(Guid stageId)
        {
            Stage stage = new Stage(stageId, $"Stage {stageId}");

            stages.Add(stageId, stage);
            stageOrder.Add(stageId);

            StageCreated?.InvokeSafely(this, new StageCreatedEventArgs(stageId));

            return stage;
        }

        public Stage DuplicateStage(Guid stageId)
        {
            Stage stage = GetStageById(stageId);

            if (stage == null)
            {
                return null;
            }

            var   duplicateStageId = NewGuid();
            Stage duplicateStage   = new Stage(stageId, $"{stage.stageTitle} {duplicateStageId}");

            stages.Add(duplicateStageId, duplicateStage);
            stageOrder.Add(duplicateStageId);

            StageDuplicated?.InvokeSafely(this, new StageDuplicatedEventArgs(stageId, duplicateStageId));

            return duplicateStage;
        }

        public bool IsSwitchingStage { get => _isSwitchingStage; }

        private bool _isSwitchingStage = false;
        /// <summary>
        /// Switch to a stage using the stageId. 
        /// </summary>
        /// <param name="stageId"></param>
        public void SwitchToStage(Guid stageId)
        {
            if (CurrentStage?.StageId == stageId)
            {
                return;
            }

            _isSwitchingStage = true;

            if (stages.TryGetValue(stageId, out Stage stage))
            {
                StageSwitchedEventArgs eventArgs = new StageSwitchedEventArgs(stageId, (int)TimeInPlayingStage.TotalMilliseconds);

                EarlyStageSwitched?.InvokeSafely(this, eventArgs);

                CurrentStage = stage;

                StageSwitched?.InvokeSafely(this, eventArgs);

                stageOffsetMillisecondValue = 0;

                if (timeInPlayingStage.IsRunning)
                {
                    timeInPlayingStage.Restart();
                }
                else
                {
                    timeInPlayingStage.Reset();
                }

                LateStageSwitched?.InvokeSafely(this, eventArgs);
            }

            _isSwitchingStage = false;
        }

        /// <summary>
        /// Overload which uses the index instead. 
        /// </summary>
        /// <param name="stageIndex"></param>
        private void SwitchToStage(int stageIndex)
        {
            SwitchToStage(stageOrder[stageIndex]);
        }

        public int CurrentStageIndex => stageOrder.IndexOf(CurrentStage.StageId);

        /// <summary>
        /// Switches to the next stage in the list, if it exists.
        /// If there is no next stage, loops back around to the first stage.
        /// </summary>
        public void SwitchToNextStage()
        {
            int indexOfCurrentStage = stageOrder.IndexOf(CurrentStage.StageId);
            int indexOfNextStage    = indexOfCurrentStage + 1;

            if (indexOfNextStage > (stages.Count - 1))
            {
                indexOfNextStage = 0;
            }

            SwitchToStage(indexOfNextStage);
        }

        public void RenameStage(Guid stageId, string name)
        {
            if (stages.TryGetValue(stageId, out Stage stage))
            {
                stage.stageTitle = name;

                StageRenamed?.InvokeSafely(this, new StageRenamedEventArgs(stageId, name));
            }
        }

        public void RemoveStage(Guid stageId)
        {
            if (stages.TryGetValue(stageId, out Stage stage))
            {
                stages.Remove(stageId);
                stageOrder.Remove(stageId);

                StageRemoved?.InvokeSafely(this, new StageRemovedEventArgs(stageId));
            }
        }

        public void RemoveAllStages()
        {
            stages.Clear();
            stageOrder.Clear();

            AllStagesRemoved?.InvokeSafely(this, new AllStagesRemovedEventArgs());
        }

        public void SwapStages(Guid stageId1, Guid stageId2)
        {
            if (!stages.TryGetValue(stageId1, out Stage _))
            {
                return;
            }

            if (!stages.TryGetValue(stageId2, out Stage _))
            {
                return;
            }

            int stageId1Index = stageOrder.IndexOf(stageId1);
            int stageId2Index = stageOrder.IndexOf(stageId2);

            stageOrder[stageId1Index] = stageId2;
            stageOrder[stageId2Index] = stageId1;

            StagesSwapped?.InvokeSafely(this, new StagesSwappedEventArgs(stageId1, stageId2));
        }

        public void LoadStages(IEnumerable<Stage> stagesToLoad)
        {
            stagesToLoad = stagesToLoad.ToList();

            if (!ValidateStagesToLoad(stagesToLoad))
            {
                UnityEngine.Debug.LogError("Cannot load stages: invalid data");
                return;
            }

            stages.Clear();
            stageOrder.Clear();

            foreach (Stage stage in stagesToLoad)
            {
                stages.Add(stage.StageId, stage);
                stageOrder.Add(stage.StageId);
            }

            StagesLoaded?.InvokeSafely(this, new StagesLoadedEventArgs());
        }

        private bool ValidateStagesToLoad(IEnumerable<Stage> stagesToLoad)
        {
            try
            {
                // Check for duplicate stageIds.
                var stageIds       = stagesToLoad.Select(stage => stage.StageId).ToList();
                var uniqueStageIds = new HashSet<Guid>(stageIds);

                return stageIds.Count == uniqueStageIds.Count;
            }
            catch (FormatException)
            {
                // Invalid Guid.
                return false;
            }
        }

        protected Guid NewGuid() => Guid.NewGuid();

        /// <summary>
        /// Marks whether changes have been saved.
        /// </summary>
        /// <param name="saved"></param>
        public void MarkChangesSaved(bool saved)
        {
            ChangesSaved = saved;
        }
    }
}