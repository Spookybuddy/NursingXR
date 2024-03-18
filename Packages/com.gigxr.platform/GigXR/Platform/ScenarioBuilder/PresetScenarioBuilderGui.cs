namespace GIGXR.Platform.ScenarioBuilder
{
    using Core.DependencyInjection;
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Core.FeatureManagement;
    using GIGXR.Platform.Scenarios.Data;
    using Newtonsoft.Json;
    using Scenarios;
    using System.IO;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// A basic GUI for editing limited Scenario values.
    /// </summary>
    public class PresetScenarioBuilderGui : MonoBehaviour
    {
        private Rect windowRect = new Rect
            (
                20,
                20,
                200,
                50
            );

        private int stageSelectionGridIndex;

        private PresetScenarioBuilderComponent presetScenarioBuilderComponent;
        private IScenarioManager ScenarioManager { get; set; }
        private IFeatureManager FeatureManager { get; set; }

        [InjectDependencies]
        public void Construct(IScenarioManager scenarioManager, IFeatureManager featureManager)
        {
            ScenarioManager = scenarioManager;
            FeatureManager = featureManager;

            ScenarioManager.ScenarioLoaded += ScenarioManager_ScenarioLoaded;
        }

        private void ScenarioManager_ScenarioLoaded(object sender, Scenarios.EventArgs.ScenarioLoadedEventArgs e)
        {
            ScenarioManager.ScenarioLoaded -= ScenarioManager_ScenarioLoaded;

            // Default to the first option in the list per design
            if (ScenarioManager.LastSavedScenario.pathways != null)
                ScenarioManager.SetPathway(ScenarioManager.LastSavedScenario.pathways.FirstOrDefault(), true);
            else
                ScenarioManager.SetPathway(PathwayData.DefaultPathway(), true);
        }

#if UNITY_EDITOR
        private void Awake()
        {
            presetScenarioBuilderComponent = GetComponent<PresetScenarioBuilderComponent>();
        }

        private void OnGUI()
        {
            if (ScenarioManager != null && ScenarioManager.LastSavedScenario != null)
            {
                // The name of the Scenario is the window title.
                var scenarioName = string.IsNullOrWhiteSpace
                    (ScenarioManager.LastSavedScenario.scenarioName)
                    ? "<Untitled>"
                    : ScenarioManager.LastSavedScenario.scenarioName;

                windowRect = GUILayout.Window
                    (
                        nameof(PresetScenarioBuilderGui).GetHashCode(),
                        windowRect,
                        SetupWindow,
                        scenarioName
                    );
            }
        }

        private async void SetupWindow(int windowId)
        {
            GUI.enabled = !ScenarioManager.IsSavingScenario;

            // Provide buttons to switch stages.
            GUILayout.BeginVertical();
            var stages = ScenarioManager.AssetManager.StageManager.Stages.ToList();

            var selectionIndex = stages.FindIndex
                (stage => stage == ScenarioManager.AssetManager.StageManager.CurrentStage);

            var stageLabels = stages.Select
                    (stage => $"{stage.stageTitle} ({stage.stageId.Substring(0, 8)})")
                .ToArray();

            selectionIndex = GUILayout.SelectionGrid
                (
                    selectionIndex,
                    stageLabels,
                    1
                );

            if (ScenarioManager.IsScenarioLoaded &&
                selectionIndex != stageSelectionGridIndex)
            {
                var stage = stages[selectionIndex];
                ScenarioManager.AssetManager.StageManager.SwitchToStage(stage.StageId);
                stageSelectionGridIndex = selectionIndex;
            }

            GUILayout.EndVertical();

            GUILayout.Space(20);

            // Do not allow users to set the pathway while the scenario is loading, saving, or while playing
            GUI.enabled = !ScenarioManager.IsSavingScenario &&
                          (ScenarioManager.ScenarioStatus == ScenarioStatus.Stopped);

            string pathwayName = string.IsNullOrEmpty(ScenarioManager.SelectedPathway?.pathwayDisplayName) ?
                                 "Default" :
                                 ScenarioManager.SelectedPathway.pathwayDisplayName;

            var pathwayButtonText = $"Pathway: {pathwayName}";

            if (GUILayout.Button(pathwayButtonText))
            {
                presetScenarioBuilderComponent.PromptScenarioPathway(ScenarioManager.LastSavedScenario.pathways);

                return;
            }

            // Do not allow users to set the play mode while the scenario is loading, saving, or while playing
            GUI.enabled = !ScenarioManager.IsSavingScenario &&
                          (ScenarioManager.ScenarioStatus == ScenarioStatus.Stopped);

            var playModeButtonText = $"Play Mode: {ScenarioManager.SelectedPlayMode}";

            if (GUILayout.Button(playModeButtonText))
            {
                presetScenarioBuilderComponent.PromptPlayMode();

                return;
            }

            GUILayout.Space(20);

            GUILayout.BeginVertical();

            GUI.enabled = !ScenarioManager.IsSavingScenario &&
                          (ScenarioManager.ScenarioStatus == ScenarioStatus.Loaded ||
                           ScenarioManager.ScenarioStatus == ScenarioStatus.Paused ||
                           ScenarioManager.ScenarioStatus == ScenarioStatus.Stopped);

            if (GUILayout.Button("Play Scenario (P)"))
            {
                ScenarioManager.PlayScenarioAsync();
            }

            GUI.enabled = !ScenarioManager.IsSavingScenario &&
                          (ScenarioManager.ScenarioStatus == ScenarioStatus.Playing);

            if (GUILayout.Button("Pause Scenario (P)"))
            {
                ScenarioManager.PauseScenarioAsync();
            }

            GUI.enabled = !ScenarioManager.IsSavingScenario &&
                          (ScenarioManager.ScenarioStatus == ScenarioStatus.Playing ||
                           ScenarioManager.ScenarioStatus == ScenarioStatus.Paused);

            if (GUILayout.Button("Reset"))
            {
                ScenarioManager.ResetScenario();
            }

            GUI.enabled = !ScenarioManager.IsSavingScenario;

            GUILayout.EndVertical();

            GUI.DragWindow();
        }
#endif
    }
}