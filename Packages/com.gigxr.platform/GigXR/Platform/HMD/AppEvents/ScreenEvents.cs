using GIGXR.Platform.Core.EventBus;

namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    using GIGXR.Platform.HMD.UI;
    using GIGXR.Platform.UI;

    /// <summary>
    /// Collection of screen request events that can be sent over the <c>UiEventBus</c>.
    /// </summary>
    public abstract class BaseScreenRequestEvent : IGigEvent<UiEventBus>
    {
        public BaseScreenObject.ScreenType TargetScreen;

        protected BaseScreenRequestEvent
        (
            BaseScreenObject.ScreenType targetScreen
        )
        {
            TargetScreen = targetScreen;
        }
    }
    
    /// <summary>
    /// Used to send updates related to screens.
    /// </summary>
    public abstract class BaseScreenUpdatedEvent : IGigEvent<UiEventBus>
    {
        public BaseScreenObject.ScreenType TargetScreen;

        protected BaseScreenUpdatedEvent
        (
            BaseScreenObject.ScreenType targetScreen
        )
        {
            TargetScreen = targetScreen;
        }
    }
    
    public class SwitchingActiveScreenEvent : BaseScreenRequestEvent
    {
        public readonly BaseScreenObject.ScreenType SenderScreen;

        /// <summary>
        /// Sends a request to switch to a new primary screen in the UI and disable
        /// others in the process. 
        /// </summary>
        /// <param name="targetScreen">The screen to become active.</param>
        /// <param name="senderScreen">The sender screen, usually the former active screen.</param>
        public SwitchingActiveScreenEvent
        (
            BaseScreenObject.ScreenType targetScreen,
            BaseScreenObject.ScreenType senderScreen = BaseScreenObject.ScreenType.None
        ) : base(targetScreen)
        {
            TargetScreen = targetScreen;
            SenderScreen = senderScreen;
        }
    }

    public class TogglingScreenEvent : BaseScreenRequestEvent
    {
        public readonly BaseScreenObject.ScreenType SenderScreen;

        /// <summary>
        /// Sends a request to toggle a particular screen on or off.
        /// </summary>
        /// <param name="targetScreen">The screen to toggle.</param>
        /// <param name="senderScreen">The sender screen.</param>
        public TogglingScreenEvent
        (
            BaseScreenObject.ScreenType targetScreen,
            BaseScreenObject.ScreenType senderScreen
        ) : base(targetScreen)
        {
            SenderScreen = senderScreen;
            TargetScreen = targetScreen;
        }
    }

    public class ReturningScreenToOriginEvent : BaseScreenRequestEvent
    {
        /// <summary>
        /// Sends a request asking for a target screen to return to the origin point.
        /// </summary>
        /// <param name="targetScreen">The screen to return to origin.</param>
        public ReturningScreenToOriginEvent
        (
            BaseScreenObject.ScreenType targetScreen
        ) : base(targetScreen)
        {
            TargetScreen = targetScreen;
        }
    }
    
    public class SettingScreenVisibilityEvent : BaseScreenRequestEvent
    {
        public readonly bool ShouldBeActive;

        /// <summary>
        /// Sends a request to specify the new active state of a screen (on or off). 
        /// </summary>
        /// <param name="targetScreen">The screen to enable or disable.</param>
        /// <param name="shouldBeActive">The desired new screen state.</param>
        public SettingScreenVisibilityEvent
        (
            BaseScreenObject.ScreenType targetScreen,
            bool shouldBeActive
        ) : base(targetScreen)
        {
            TargetScreen = targetScreen;
            ShouldBeActive = shouldBeActive;
        }
    }

    public class SettingActiveSubScreenEvent : BaseScreenRequestEvent
    {
        public readonly SubScreenState SubScreenStateToSwitchTo;

        /// <summary>
        /// Sends a request to enable a subscreen attached to the targetScreen.
        /// </summary>
        /// <param name="targetScreen">The target screen, parent of the subscreen.</param>
        /// <param name="substate">The substate which is used to identify the target subscreen.</param>
        public SettingActiveSubScreenEvent
        (
            BaseScreenObject.ScreenType targetScreen,
            SubScreenState substate
        ) : base(targetScreen)
        {
            TargetScreen = targetScreen;
            SubScreenStateToSwitchTo = substate;
        }
    }

    public class SwitchedActiveSubScreenEvent : BaseScreenUpdatedEvent
    {
        public readonly SubScreenState SubScreenStateToSwitchTo;

        /// <summary>
        /// Sends an update about the new active subscreen attached to
        /// the targetScreen.
        /// </summary>
        /// <param name="targetScreen">The target screen containing the subscreen.</param>
        /// <param name="substate">Refers to the subscreen that was just enabled.</param>
        public SwitchedActiveSubScreenEvent
        (
            BaseScreenObject.ScreenType targetScreen,
            SubScreenState substate
        ) : base(targetScreen)
        {
            TargetScreen = targetScreen;
            SubScreenStateToSwitchTo = substate;
        }
    }
}