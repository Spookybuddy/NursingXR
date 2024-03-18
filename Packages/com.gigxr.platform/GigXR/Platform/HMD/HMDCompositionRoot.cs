namespace GIGXR.Platform.HMD
{
    using GIGXR.Platform.Core.DependencyValidator;
    using GIGXR.Platform.HMD.QR;
    using GIGXR.Platform.HMD.Interfaces;
    using UnityEngine;
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Scenarios;
    using GIGXR.Platform.Sessions;

    /// <summary>
    /// HMD Specific Composition Root that allows QR specific MonoBehaviors to be passed into dependencies.
    /// </summary>
    public class HMDCompositionRoot : CompositionRoot
    {
        #region Editor Set Values

        [SerializeField, RequireDependency]
        private QRCodeDecodeControllerForWSA qrCodeDecodeController;

        #endregion

        protected override void BuildExtraDependencies()
        {
            // Make the QR Manager for HMD use
            IQrCodeManager qrCodeManager = new QrCodeManager(qrCodeDecodeController, DependencyProvider.GetDependency<AppEventBus>());

            ICalibrationManager calibrationManager = new HMDCalibrationManager(this,
                                                                               DependencyProvider.GetDependency<AppEventBus>(), 
                                                                               DependencyProvider.GetDependency<IScenarioManager>()?.AssetManager,
                                                                               DependencyProvider.GetDependency<ISessionManager>(),
                                                                               qrCodeManager);

            dependencyProvider.RegisterSingleton(_ => calibrationManager);
            dependencyProvider.RegisterSingleton(_ => qrCodeManager);
        }
    }
}