namespace GIGXR.Platform.ScenarioBuilder
{
    using Data;
    using GIGXR.GMS.Clients;
    using GIGXR.GMS.Models.Sessions;
    using GIGXR.GMS.Models.Sessions.Requests;
    using Scenarios.Data;
    using Scenarios.GigAssets.Loader;
    using System;
    using UnityEngine;
    using GIGXR.Platform.Core;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Scenarios;
    using Sessions;
    using System.IO;
    using UnityEditor;
    using Scenarios.GigAssets;
    using GIGXR.Platform.Networking;
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.ScenarioBuilder.SessionPlanTools;
    using System.Collections.Generic;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.Managers;
    using System.Linq;
    using GIGXR.Platform.Core.FeatureManagement;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.HMD;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Core.User;
    using GIGXR.Platform.CommonAssetTypes.CommonComponents;

    /// <summary>
    /// A component that allows for running and editing a <c>PresetScenario</c>.
    /// </summary>
    [HelpURL("https://app.clickup.com/8621331/v/dc/8738k-2580/8738k-10560")]
    [RequireComponent(typeof(PresetScenarioBuilderGui))]
    public class PresetScenarioBuilderComponent : GIGXRCore
    {
        [SerializeField]
        [Header("Select a PresetScenario data bundle to load")]
        private PresetScenario presetScenario;

        [SerializeField]
        private GameObject avatarPrefab;

        /// <summary>
        /// If you get a 403, make sure the current test credentials are set to the
        /// same ones used to create the scenario initially. Usually gigxr.test. 
        /// </summary>
        [SerializeField]
        [Header("Set this to false to disable PSB shortcuts.")]
        private bool shortcutsEnabled;

        private UIPlacementData promptPlacementData;

        IAssetTypeLoader assetTypeLoader;

        private IScenarioManager ScenarioManager
            => dependencyProvider.GetDependency<IScenarioManager>();

        private GmsApiClient GmsApiClient
            => dependencyProvider.GetDependency<GmsApiClient>();

        private ProfileManager ProfileManager
            => dependencyProvider.GetDependency<ProfileManager>();

        private ISessionManager SessionManager
            => dependencyProvider.GetDependency<ISessionManager>();

        private AppEventBus EventBus
            => dependencyProvider.GetDependency<AppEventBus>();

        private IFeatureManager FeatureManager
            => dependencyProvider.GetDependency<IFeatureManager>();

        public override IAssetTypeLoader AssetTypeLoader
        {
            get
            {
                if (assetTypeLoader == null)
                {
                    assetTypeLoader = new AddressablesAssetTypeLoader();
                }

                return assetTypeLoader;
            }
        }

        // Not sure if this should go here in Composition Root, but it is similar to the assetTypeLoader.
        // Can be used to load Addressable game objects as needed by asset types. 
        IAddressablesGameObjectLoader addressablesGameObjectLoader;

        public override IAddressablesGameObjectLoader AddressablesGameObjectLoader
        {
            get
            {
                if (addressablesGameObjectLoader == null)
                {
                    addressablesGameObjectLoader = new AddressablesGameObjectLoader();
                }

                return addressablesGameObjectLoader;
            }
        }

        protected override void Awake()
        {
            if (presetScenario == null)
            {
                ExitPlaymode();
            }

            base.Awake();
        }

        protected async void Start()
        {
            try
            {
                var scenario = presetScenario.BuildScenario();
                await ScenarioManager.LoadScenarioAsync(scenario, new CancellationToken());

                SessionManager.AddSessionCapability(new CreatorCapabilities());

                SessionManager.AddSessionCapability
                    (
                        new HostCapabilities
                            (
                                SessionManager,
                                ScenarioManager,
                                dependencyProvider.GetDependency<INetworkManager>(),
                                dependencyProvider.GetDependency<GmsApiClient>(),
                                dependencyProvider.GetDependency<AppEventBus>()
                            )
                    );

                ScenarioManager.AssetManager.AssetContext.SetContext
                    (nameof(ScenarioManager.AssetManager.AssetContext.IsScenarioAuthority), true);

                await ScenarioManager.StopScenarioAsync();

                GenerateUserRepresentations();

                promptPlacementData = new UIPlacementData()
                {
                    ButtonGridLayout = GridLayoutOrder.Vertical,
                    ButtonGridLocalPositionOverride = new Vector3
                        (
                            0.0f,
                            0.0f,
                            -0.009f
                        )
                };
            }
            catch (Exception exception)
            {
                Debug.LogError($"PresetScenario is broken! Exiting due to: {exception.GetType().Name} - {exception.Message}");
                Debug.LogException(exception);

                ExitPlaymode();
            }
        }

        private void GenerateUserRepresentations()
        {
            // TODO Instantiate without referencing PhotonNetwork
            var avatarHeadGO = GameObject.Instantiate(avatarPrefab);
            var avatar = avatarHeadGO.GetComponent<UserAvatar>();

            avatarHeadGO.name = "Avatar Head";

            var followHead = avatarHeadGO.AddComponent<TransformFollow>();
            followHead.positionOffset = avatar.HeadOffset;
            followHead.Follow(Camera.main.transform);
        }

        private CancellationTokenSource promptCurrentScenarioTokenSource;
        private CancellationTokenSource promptPlayModeTokenSource;

        // Copy of the same method name from ScenarioScreen
        public void PromptScenarioPathway(IEnumerable<PathwayData> pathways)
        {
            BringDownPlayModePrompt();

            if (pathways == null ||
                pathways.Count() == 0)
            {
                ScenarioManager.SetPathway(null, true);
            }
            else if (pathways.Count() == 1)
            {
                ScenarioManager.SetPathway(pathways.First(), true);
            }
            // There are two or more choices so actually display the prompt
            else
            {
                if (promptCurrentScenarioTokenSource == null)
                {
                    promptCurrentScenarioTokenSource = new CancellationTokenSource();

                    var allPathwayButtons = new List<ButtonPromptInfo>();

                    // Add a button for each pathway data
                    foreach (PathwayData currentPathway in pathways)
                    {
                        allPathwayButtons.Add
                            (
                                new ButtonPromptInfo()
                                {
                                    buttonText = currentPathway.pathwayDisplayName,
                                    onPressAction = () =>
                                    {
                                        ScenarioManager.SetPathway(currentPathway, true);

                                        promptCurrentScenarioTokenSource.Dispose();
                                        promptCurrentScenarioTokenSource = null;
                                    }
                                }
                            );
                    }

                    promptPlacementData.WindowSize = new Vector2((int)PromptManager.WindowStates.Narrow + 3,
                                                                 (5 * allPathwayButtons.Count) + 15);

                    EventBus.Publish
                        (
                            new ShowCancellablePromptEvent
                                (
                                    promptCurrentScenarioTokenSource.Token,
                                    "Select Pathway",
                                    "", // No main text
                                    allPathwayButtons,
                                    promptPlacementData
                                )
                        );
                }
                else
                {
                    BringDownPathwayPrompt();
                }
            }
        }

        // Copy of
        public void PromptPlayMode()
        {
            BringDownPathwayPrompt();

            if (promptPlayModeTokenSource == null)
            {
                promptPlayModeTokenSource = new CancellationTokenSource();

                var allPathwayButtons = new List<ButtonPromptInfo>();

                // Add a button for each scenario control type
                foreach (ScenarioControlTypes scenarioType in (ScenarioControlTypes[])Enum.GetValues
                    (typeof(ScenarioControlTypes)))
                {
                    allPathwayButtons.Add
                        (
                            new ButtonPromptInfo()
                            {
                                buttonText = scenarioType.ToString(),
                                onPressAction = () =>
                                {
                                    ScenarioManager.SetPlayMode(scenarioType, true);

                                    promptPlayModeTokenSource.Dispose();
                                    promptPlayModeTokenSource = null;
                                }
                            }
                        );
                }

                promptPlacementData.WindowSize = new Vector2((int)PromptManager.WindowStates.Narrow + 3,
                                                                 (5 * allPathwayButtons.Count) + 15);

                EventBus.Publish
                    (
                        new ShowCancellablePromptEvent
                            (
                                promptPlayModeTokenSource.Token,
                                "Select Play Mode",
                                "", // No main text
                                allPathwayButtons,
                                promptPlacementData
                            )
                    );
            }
            else
            {
                BringDownPlayModePrompt();
            }
        }

        private void BringDownPlayModePrompt()
        {
            if (promptPlayModeTokenSource != null)
            {
                promptPlayModeTokenSource.Cancel();
                promptPlayModeTokenSource.Dispose();

                promptPlayModeTokenSource = null;
            }
        }

        private void BringDownPathwayPrompt()
        {
            if (promptCurrentScenarioTokenSource != null)
            {
                promptCurrentScenarioTokenSource.Cancel();
                promptCurrentScenarioTokenSource.Dispose();

                promptCurrentScenarioTokenSource = null;
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Provides keyboard shortcut support. 
        /// </summary>
        private async void Update()
        {
            if (!shortcutsEnabled) return;

            // Play or pause the scenario 
            if ((Input.GetKey(KeyCode.LeftControl) || (Input.GetKey(KeyCode.RightControl))) && (Input.GetKeyDown(KeyCode.P)))
            {
                if (ScenarioManager.ScenarioStatus != ScenarioStatus.Playing)
                {
                    ScenarioManager.PlayScenarioAsync();
                }
                else
                {
                    ScenarioManager.PauseScenarioAsync();
                }
            }
            // Export JSON session plan and show folder containing saved scenario data
            else if ((Input.GetKey(KeyCode.LeftControl) || (Input.GetKey(KeyCode.RightControl))) && Input.GetKeyDown(KeyCode.Backspace))
            {
                ExportScenarioToJson(ScenarioManager.LastSavedScenario.scenarioName + ".json");
            }
            // Stop the Scenario
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                await ScenarioManager.StopScenarioAsync();
            }
            // Go to the next stage 
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                ScenarioManager.StageManager.SwitchToNextStage();
            }
        }

        /// <summary>
        /// Export a single scenario. 
        /// </summary>
        public void ExportScenarioToJson(string exportFileName)
        {
            var path = Path.Combine(Application.persistentDataPath, exportFileName);

            var json = JsonConvert.SerializeObject(ScenarioManager.LastSavedScenario, Formatting.None);

            File.WriteAllText(path, json);
            Debug.Log($"Exported to {path}");

            // Open the export location for easy access
            EditorUtility.RevealInFinder(path);
        }

        /// <summary>
        /// Should be called from Unity UI in the editor.
        /// </summary>
        public async void ListVersionsFromGms()
        {
            await LogInIfNeeded();

            var versions = await GmsApiClient.ClientApps.GetVersionList();

            foreach (var v in versions)
            {
                Debug.Log($"Version: {v}");
            }
        }

        /// <summary>
        /// Should be called from Unity UI in the editor.
        /// </summary>
        public async void ListAppVersionFromGms()
        {
            await LogInIfNeeded();

            var versions = await GmsApiClient.ClientApps.GetAppVersions();

            foreach (var v in versions)
            {
                Debug.Log($"Version: {v}");
            }
        }

        private async UniTask LogInIfNeeded()
        {
            // Login
            if (GmsApiClient.AccountsApi.AuthenticatedAccount == null)
            {
                var testEmail = EditorAuthenticationProfile.GetTestCredentials().Email;
                var testPassword = EditorAuthenticationProfile.GetTestCredentials().Password;

                await GmsApiClient.AccountsApi.LoginWithEmailPassAsync(testEmail, testPassword);
            }
        }

        /// <summary>
        /// Should be called from Unity UI in the editor.
        /// </summary>
        public async void PostUpdatedAppVersionToGms()
        {
            await LogInIfNeeded();

            var updateVersion = await GmsApiClient.ClientApps.CreateClientAppVersionRequest(ProfileManager.appDetailsProfile.appDetailsScriptableObject.VersionString);

            if (updateVersion != null)
            {
                Debug.Log($"Updated {updateVersion.ClientAppId} with {updateVersion.Version}");
            }
            else
            {
                Debug.LogWarning($"Something went wrong when trying to create app version {ProfileManager.appDetailsProfile.appDetailsScriptableObject.VersionString}.");
            }
        }
#endif

        private async void ExitPlaymode()
        {
#if UNITY_EDITOR
            await ScenarioManager.UnloadScenarioAsync();
            EditorApplication.ExitPlaymode();
#endif
        }

        protected override void BuildExtraDependencies()
        {
#if UNITY_WSA_10_0 || UNITY_EDITOR
            ICalibrationManager calibrationManager = new HMDCalibrationManager(this,
                                                                               DependencyProvider.GetDependency<AppEventBus>(),
                                                                               DependencyProvider.GetDependency<IScenarioManager>()?.AssetManager,
                                                                               DependencyProvider.GetDependency<ISessionManager>(),
                                                                               null);

            dependencyProvider.RegisterSingleton(_ => calibrationManager);
#endif
        }

        [ContextMenu("SetAllStageValuesToPersist")]
        private void SetAllStageValuesToPersist()
        {
            SetAllStageValuePersistance(true);
        }

        [ContextMenu("SetAllStageValuesToReset")]
        private void SetAllStageValuesToReset()
        {
            SetAllStageValuePersistance(false);
        }

        private void SetAllStageValuePersistance(bool persist)
        {
            if (!Application.isPlaying || !ScenarioManager.IsScenarioLoaded)
            {
                Debug.LogError("Application not playing or scenario not loaded.");
                return;
            }

            foreach (var asset in ScenarioManager.AssetManager.AllInstantiatedAssets)
            {
                foreach (var property in asset.Value.GameObject.GetComponent<IAssetMediator>().GetAllKnownAssetProperties())
                {
                    property.SetValuePersistance(persist);
                }
            }
        }
    }
}