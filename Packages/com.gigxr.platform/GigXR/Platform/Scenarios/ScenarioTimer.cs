namespace GIGXR.Platform.Scenarios
{
    using EventArgs;
    using GIGXR.Platform.Scenarios.Data;
    using Stages;
    using Stages.EventArgs;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// A serializable struct holding the data related to the ScenarioTimer.
    /// </summary>
    [Serializable]
    public struct ScenarioSyncData
    {
        public int TimeStamp;

        public ScenarioStatus ScenarioStatus;

        public int TotalMillisecondsInSimulation;
        public int TotalMillisecondsInScenario;
        public int TotalMillisecondsInCurrentStage;

        public ScenarioSyncData(int serverTime, ScenarioStatus scenarioStatus, int totalSimulation, int totalScenario, int stageOffset)
        {
            TimeStamp = serverTime;
            ScenarioStatus = scenarioStatus;
            TotalMillisecondsInSimulation = totalSimulation;
            TotalMillisecondsInScenario = totalScenario;
            TotalMillisecondsInCurrentStage = stageOffset;
        }
    }

    /// <summary>
    /// Keeps track of the timers for a Scenario.
    /// </summary>
    public class ScenarioTimer
    {
        private int simulationMillisecondValue = 0;

        // Since entering edit mode will clear the stopwatch, keep track of the offset
        // for when the reset occurs
        private int scenarioOffsetMillisecondValue = 0;

        /// <summary>
        /// How long the user has been playing in the scenario, across all stages
        /// </summary>
        private readonly Stopwatch timeInPlayingScenario = new Stopwatch();

        private readonly IScenarioManager scenarioManager;
        private readonly IStageManager stageManager;

        public ScenarioTimer(IScenarioManager scenarioManager, IStageManager stageManager)
        {
            this.scenarioManager = scenarioManager;
            this.stageManager = stageManager;

            this.scenarioManager.ScenarioLoaded += ScenarioManagerOnScenarioLoaded;
            this.scenarioManager.ScenarioUnloaded += ScenarioManagerOnScenarioUnloaded;
        }

        public TimeSpan TimeInPlayingScenario => new TimeSpan(0, 0, 0, 0, scenarioOffsetMillisecondValue).Add(timeInPlayingScenario.Elapsed);
        public TimeSpan TimeInSimulation => new TimeSpan(0, 0, 0, 0, simulationMillisecondValue).Add(stageManager.ElapsedTimeInStage);

        private void ScenarioManagerOnScenarioLoaded(object sender, ScenarioLoadedEventArgs e)
        {
            ClearTimers();

            this.scenarioManager.ScenarioPlaying += ScenarioManagerOnScenarioPlaying;
            this.scenarioManager.ScenarioPaused += ScenarioManagerOnScenarioPaused;
            // Use ScenarioReset which is called in the same frame as ScenarioStopped but works without PlayEdit Mode
            this.scenarioManager.ScenarioReset += ScenarioManagerOnScenarioReset;
            this.stageManager.StageSwitched += StageManagerOnStageSwitched;
        }

        private void ScenarioManagerOnScenarioUnloaded(object sender, ScenarioUnloadedEventArgs e)
        {
            this.scenarioManager.ScenarioPlaying -= ScenarioManagerOnScenarioPlaying;
            this.scenarioManager.ScenarioPaused -= ScenarioManagerOnScenarioPaused;
            this.scenarioManager.ScenarioReset -= ScenarioManagerOnScenarioReset;
            this.stageManager.StageSwitched -= StageManagerOnStageSwitched;

            ClearTimers();
        }

        private void ClearTimers()
        {
            timeInPlayingScenario.Reset();

            simulationMillisecondValue = 0;
            scenarioOffsetMillisecondValue = 0;

            stageManager.ResetTimer();
        }

        private void ScenarioManagerOnScenarioPlaying(object sender, ScenarioPlayingEventArgs e)
        {
            timeInPlayingScenario.Start();
        }

        private void ScenarioManagerOnScenarioPaused(object sender, ScenarioPausedEventArgs e)
        {
            timeInPlayingScenario.Stop();
        }

        private void ScenarioManagerOnScenarioReset(object sender, ScenarioResetEventArgs e)
        {
            ClearTimers();
        }

        private void StageManagerOnStageSwitched(object sender, StageSwitchedEventArgs e)
        {
            // Adjust the simulation time based on how much time has passed in the previous stage
            AddSimulationTime(e.PreviousStageTime);
        }

        public void SyncScenarioTimer(int totalMillisecondsInSimulation, int totalMillisecondsInScenario)
        {
            SetSimulationTime(totalMillisecondsInSimulation);
            SetScenarioOffset(totalMillisecondsInScenario);
        }

        public void AddSimulationTime(int time)
        {
            simulationMillisecondValue += time;
        }

        public void SetSimulationTime(int time)
        {
            simulationMillisecondValue = time;
        }

        public void SetScenarioOffset(int time)
        {
            scenarioOffsetMillisecondValue = time;

            if (timeInPlayingScenario.IsRunning)
            {
                timeInPlayingScenario.Restart();
            }
            else
            {
                timeInPlayingScenario.Reset();
            }
        }
    }
}