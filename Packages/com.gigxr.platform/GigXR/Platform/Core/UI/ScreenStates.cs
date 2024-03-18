namespace GIGXR.Platform.UI
{
    // public enum ScreenState
    // {
    //     None,
    //     Calibration,
    //     Label,
    //     DownloadManager,
    //     ServerResponse,
    //     // CalibrationPrompt,
    //     Login,
    //     QrLoginScan,
    //     SessionManagement,
    //     SceneManagement,
    //     Webview,
    //     Scan,
    //     Placement,
    //     Session,
    //     InteractableManager,
    //     Connectivity,
    //     Toolbar
    // }

    public enum SubScreenState
    {
        None,
        SessionsList,
        SessionLog,
        Menu,
        /// Could be renamed to AnchorContentMenu as that is more apt now, but will avoid the enum rename 
        /// because prefabs reference this data enum
        CalibrationMenu,
        QRCalibration,
        ManualCalibration,
        InteractablePlacement,
        Generic1,
        Generic2,
        QRLogin,
        ControlsTab,
        AboutScenarioTab
    }
}