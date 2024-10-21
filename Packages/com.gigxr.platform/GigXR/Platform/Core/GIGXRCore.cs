using GIGXR.Platform.AppEvents;
using GIGXR.Platform.Core.Audio;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Core.DependencyValidator;
using GIGXR.Platform.Core.EventBus;
using GIGXR.Platform.Core.FeatureManagement;
using GIGXR.Dictation;
using GIGXR.GMS.Clients;
using GIGXR.Platform.Managers;
using GIGXR.Platform.UI;
using GIGXR.Platform.Interfaces;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Networking.EventBus;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.Loader;
using GIGXR.Platform.Scenarios.Stages;
using GIGXR.Platform.Sessions;
using GIGXR.Platform.Utilities;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GIGXR.Platform.Core
{
    /// <summary>
    /// Interface that helps construct a full GIGXR App
    /// </summary>
    /// /// <remarks>
    /// This is the only place where execution order should be specified.
    /// </remarks>
    [DefaultExecutionOrder(-99)]
    public abstract class GIGXRCore : MonoBehaviour
    {
        #region Editor Set Values

        [SerializeField, RequireDependency] protected ProfileManager profileManager;

        [SerializeField, RequireDependency] protected BasicCalibrationRootComponent calibrationRoot;

        #endregion

        #region Abstract

        public abstract IAssetTypeLoader AssetTypeLoader { get; }

        public abstract IAddressablesGameObjectLoader AddressablesGameObjectLoader { get; }

        protected abstract void BuildExtraDependencies();

        #endregion

        #region ProtectedVariables

        protected IDependencyInjector injector;

        protected readonly DependencyProvider dependencyProvider = new DependencyProvider();

        private EnvironmentDetailsScriptableObject originalTargetEnvironment;

        #endregion

        #region PublicAPI

        // Generally try not to use this. It probably shouldn't be needed.
        //
        // If a MonoBehaviour needs a plain C# class, use the [InjectDependencies] attribute.
        // If a plain C# class needs a C# class, resolve it in Func<> passed when registering.
        // If a plain C# class needs a MonoBehaviour or ScriptableObject, which should be relatively
        //   rare, then add it as a serialized field to this class and pass it into the injection
        //   system.
        public IDependencyProvider DependencyProvider => dependencyProvider;

        public async UniTask SetRuntimeDependencies(IEnumerable<MonoBehaviour> runtimeBehaviors)
        {
            await injector.InjectIntoMonoBehaviours(runtimeBehaviors);
        }

        public async UniTask SetRuntimeDependencies(MonoBehaviour runtimeBehaviors)
        {
            await injector.InjectIntoMonoBehaviours(runtimeBehaviors);
        }

        #endregion

        #region Unity Functions

        protected virtual void Awake()
        {
            originalTargetEnvironment = profileManager.authenticationProfile.TargetEnvironmentalDetails;

            // Inject into MonoBehaviours.
            injector = new DependencyInjector(dependencyProvider);

            dependencyProvider
                // MonoBehaviour and ScriptableObjects needed by the plain C# classes.
                .RegisterSingleton<ProfileManager>(_ => profileManager)
                .RegisterSingleton<ICalibrationRootProvider>(_ => calibrationRoot)

                // Old style registrations where the instances are created elsewhere. Try to avoid
                // doing this. If it is a MonoBehaviour dependency then add it as a serialized
                // field.
                .RegisterSingleton<UnityScheduler>(_ => UnityScheduler.Instance)

                // Generally features should let the DependencyInjection system manage the lifecycle
                // of these C# objects. That is the standard for how dependency injection works in
                // all of the major products. This is so lifecycle management can be handled in one
                // location and also enables other types of lifecycles.
                //
                // For example, DependencyProvider also supports another registration type:
                //
                // .RegisterTransient(_ => new SomeClass());
                //
                // As opposed to the singleton version, this version will return a new instance of
                // that class every time.
                //
                // In the future, it could be possible to have other lifecycles such as scoped to 
                // the lifecycle of a Session, or Asset. This means each Session or Asset would get
                // their own copy of that class which could be useful for certain things.
                //
                .RegisterSingleton<IFeatureManager>
                    (provider => new BasicEnumFeatureManager(profileManager.FeatureFlagsProfile.FeatureFlags))
                .RegisterSingleton<AppEventBus>(_ => new AppEventBus())
                .RegisterSingleton<UiEventBus>(_ => new UiEventBus())
                .RegisterSingleton<GmsApiClient>(BuildGmsApiClient)
                .RegisterSingleton<IScenarioManager>(BuildScenarioManager)
                .RegisterSingleton<IGigAssetManager>(provider => provider.GetDependency<IScenarioManager>()?.AssetManager)
                .RegisterSingleton<IDictationManager>(BuildDictationManager)
                .RegisterSingleton<INetworkManager>(BuildNetworkManager)
                .RegisterSingleton<IAuthenticationManager>(BuildAuthenticationManager)
                .RegisterSingleton<ISessionManager>(BuildSessionManager)
                .RegisterSingleton<IAudioManager>(BuildAudioManager)
                .RegisterSingleton<IBuilderManager>(BuildBuilderManager);

            if(profileManager.injectableScriptableObjects != null)
            {
                foreach (var scriptableObject in profileManager.injectableScriptableObjects)
                {
                    dependencyProvider.RegisterSingleton(_ => scriptableObject, scriptableObject.GetType());
                }
            }

            // Specific hardware such as Mobile or HMD may call this method to inject their own singletons into the dependencyProvider
            BuildExtraDependencies();

            // Since this is the initial injection, do not await so that scripts are constructed during this awake method
            injector.InjectIntoMonoBehaviours();
        }

        protected void OnDestroy()
        {
            // Have to save this for now when working in the Editor
            if (originalTargetEnvironment != null &&
                originalTargetEnvironment != profileManager.authenticationProfile.TargetEnvironmentalDetails)
            {
                profileManager.authenticationProfile.SetTargetGMS(originalTargetEnvironment);
            }

            // Make sure all dependencies are cleaned up at the end, assumes downstream classes clean themselves up
            dependencyProvider.DestroyAllDependencies();
        }

        #endregion

        #region BuildDependencyFunctions

        // The reason these methods take an instance of IDependencyProvider as a parameter instead
        // of using the local private one, is because once the object graph grows very large these
        // can be factored out into separate classes or even groups of dependencies. It could be
        // easier to see the "bounded context" of a group of dependencies if the are defined in
        // separate files by those bounded contexts.
        //
        // The reason this wasn't done originally was because at the time the dependency graph was
        // spider-web like and this wasn't possible to do at the time.
        protected IScenarioManager BuildScenarioManager(IDependencyProvider provider)
        {
            var customScenarioManager = GetComponent<CoreInjectorComponent<IScenarioManager>>();

            if (customScenarioManager != null)
            {
                return customScenarioManager.GetSingleton();
            }
            else
            {
                // The reason these are new-ed up here is because these resources are "owned" by
                // Scenarios. In DDD terms this means Scenario would be an aggregate. The main defining
                // characteristic of an owned resource is "is this ever accessed outside of the scope
                // of a Scenario (or whatever the Aggregate is)?"
                //
                // Assets are not currently used outside of a Scenario, but it could be argued they
                // could be used on their own, so it would be a reasonable refactor to elevate the
                // AssetManager up to a first class dependency.
                var stageManager = new StageManager();
                var assetManager = new GigAssetManager
                (
                    calibrationRoot: provider.GetDependency<ICalibrationRootProvider>(),
                    AssetTypeLoader,
                    AddressablesGameObjectLoader,
                    stageManager,
                    UnityScheduler.Instance,
                    profileManager,
                    runtimeExpeditedFunction: async monoBehaviours =>
                    {
                        await injector.InjectIntoMonoBehavioursMethods(monoBehaviours);
                    },
                    featureManager: provider.GetDependency<IFeatureManager>()
                );

                AddressablesGameObjectLoader.SetManager(assetManager);

                var featureManager = provider.GetDependency<IFeatureManager>();

                var scenarioManager = new ScenarioManager(assetManager, stageManager, featureManager);

                return scenarioManager;
            }
        }

        protected IAuthenticationManager BuildAuthenticationManager(IDependencyProvider provider)
        {
            var customNetworkManager = GetComponent<CoreInjectorComponent<IAuthenticationManager>>();

            if (customNetworkManager != null)
            {
                return customNetworkManager.GetSingleton();
            }
            else
            {
                var networkManager = provider.GetDependency<INetworkManager>();
                var appEventBus = provider.GetDependency<AppEventBus>();
                var gmsApiClient = provider.GetDependency<GmsApiClient>();
                var featureManager = provider.GetDependency<IFeatureManager>();

                return new AuthenticationManager(networkManager, appEventBus, gmsApiClient, profileManager, featureManager);
            }
        }

        protected INetworkManager BuildNetworkManager(IDependencyProvider provider)
        {
            var customNetworkManager = GetComponent<CoreInjectorComponent<INetworkManager>>();

            if (customNetworkManager != null)
            {
                return customNetworkManager.GetSingleton();
            }
            else
            {
                var eventBus = new GigEventBus<NetworkManager>();

                return new NetworkManager
                (
                    profileManager,
                    new ConnectionQualityHandler(profileManager),
                    eventBus,
                    appEventBus: provider.GetDependency<AppEventBus>(),
                    new CustomNetworkEventHandlerThrottleDecorator
                    (
                        new CustomNetworkEventHandler(eventBus),
                        eventInterval: TimeSpan.FromMilliseconds(200),
                        countAllowedPerInterval: 10
                    ),
                    new PhotonEventAdapterHandler(eventBus)
                );
            }
        }

        protected IDictationManager BuildDictationManager(IDependencyProvider provider)
        {
            var customNetworkManager = GetComponent<CoreInjectorComponent<IDictationManager>>();

            if (customNetworkManager != null)
            {
                return customNetworkManager.GetSingleton();
            }
            else
            {
                return new DictationManager(profileManager, featureManager: provider.GetDependency<IFeatureManager>());
            }
        }

        protected GmsApiClient BuildGmsApiClient(IDependencyProvider provider)
        {
            var appEventBus = provider.GetDependency<AppEventBus>();
            var config = new GmsApiClientConfiguration
            (
                productName: Application.productName,
                productVersion: Application.version,
                authenticationProfile: profileManager.authenticationProfile
            );
            var client = new GmsApiClient(appEventBus, config, profileManager);
            return client;
        }

        protected ISessionManager BuildSessionManager(IDependencyProvider provider)
        {
            var customNetworkManager = GetComponent<CoreInjectorComponent<ISessionManager>>();

            if (customNetworkManager != null)
            {
                return customNetworkManager.GetSingleton();
            }
            else
            {
                var scenarioManager = provider.GetDependency<IScenarioManager>();
                var networkManager = provider.GetDependency<INetworkManager>();
                var gmsApiClient = provider.GetDependency<GmsApiClient>();
                var appEventBus = provider.GetDependency<AppEventBus>();
                var featureManager = provider.GetDependency<IFeatureManager>();

                return new SessionManager(scenarioManager, 
                                          networkManager, 
                                          gmsApiClient, 
                                          appEventBus, 
                                          profileManager, 
                                          featureManager, 
                                          provider);
            }
        }

        protected IAudioManager BuildAudioManager(IDependencyProvider provider)
        {
            var customNetworkManager = GetComponent<CoreInjectorComponent<IAudioManager>>();

            if (customNetworkManager != null)
            {
                return customNetworkManager.GetSingleton();
            }
            else
            {
                var ScenarioManager = provider.GetDependency<IScenarioManager>();
                var NetworkManager = provider.GetDependency<INetworkManager>();
                var appEventBus = provider.GetDependency<AppEventBus>();

                return new AudioManager(ScenarioManager, NetworkManager, appEventBus);
            }
        }

        protected IBuilderManager BuildBuilderManager(IDependencyProvider provider)
        {
            var customNetworkManager = GetComponent<CoreInjectorComponent<IBuilderManager>>();

            if (customNetworkManager != null)
            {
                return customNetworkManager.GetSingleton();
            }
            else
            {
                return new BuilderManager(profileManager);
            }
        }
        
        #endregion
    }
}