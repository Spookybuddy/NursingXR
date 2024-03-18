namespace GIGXR.Platform.Mobile
{
    using GIGXR.GMS.Clients;
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.Core.DependencyValidator;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Mobile.Utilities;
    using GIGXR.Platform.Scenarios;
    using GIGXR.Platform.Sessions;
    using UnityEngine;

    /// <summary>
    /// Mobile Specific Composition Root that sets up mobile-exclusive dependencies.
    /// </summary>
    public class MobileCompositionRoot : CompositionRoot
    {
        #region Editor Set Values

        [SerializeField, RequireDependency]
        private MobileProfileScriptableObject mobileProfile;

        #endregion

        public MobileProfileScriptableObject MobileProfile { get { return mobileProfile; } }

        protected override void BuildExtraDependencies()
        {
            MobileDeviceSettings.Initialize();

            ICalibrationManager _calibrationManager = new MobileCalibrationManager(this,
                                                                                   DependencyProvider.GetDependency<AppEventBus>(), 
                                                                                   DependencyProvider.GetDependency<IScenarioManager>()?.AssetManager,
                                                                                   DependencyProvider.GetDependency<ISessionManager>());

            IFirebaseManager _firebaseManager = new FirebaseManager(DependencyProvider.GetDependency<GmsApiClient>(), 
                                                                    DependencyProvider.GetDependency<AppEventBus>(), 
                                                                    MobileProfile);

            dependencyProvider.RegisterSingleton(_ => _calibrationManager);
            dependencyProvider.RegisterSingleton(_ => _firebaseManager);
        }
    }
}