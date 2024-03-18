using System.Collections.Generic;
using UnityEngine;
using GIGXR.Platform.UI;

namespace GIGXR.Platform.Mobile.UI
{
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Mobile.AppEvents.Events.UI;
    public abstract class BaseScreenObjectMobile : BaseUiObject, IBaseScreen
    {
        public enum ScreenTypeMobile
        {
            None,
            WebView,
            Scan,
            Placement,
            Session,
            ResetScan,
            Toolbar,
            Connectivity,
            ContentMarker
        }

        public abstract ScreenTypeMobile ScreenType { get; }

        public ScreenObject RootScreenObject { get; private set; }

        public Transform RootScreenTransform => RootScreenObject.transform;

        public bool IsVisible 
        { 
            get 
            { 
                return RootScreenObject != null &&
                       RootScreenObject.gameObject.activeInHierarchy; 
            } 
        }

        #region StaticHelpers

        private static readonly Dictionary<ScreenTypeMobile, BaseScreenObjectMobile> KnownScreens =
            new Dictionary<ScreenTypeMobile, BaseScreenObjectMobile>();

        public static BaseScreenObjectMobile GetScreenByType
        (
            ScreenTypeMobile typeOfScreen
        )
        {
            return KnownScreens.ContainsKey(typeOfScreen) ? KnownScreens[typeOfScreen] : null;
        }

        #endregion

        protected virtual void Awake()
        {
            Initialize();
        }

        protected override void SubscribeToEventBuses()
        {
            uiEventBus.Subscribe<SwitchingActiveScreenEventMobile>(OnSwitchingActiveScreenEvent);
        }

        protected virtual void OnSwitchingActiveScreenEvent(SwitchingActiveScreenEventMobile @event)
        {
            RootScreenObject.SetActive(ScreenType == @event.TargetScreen);
        }

        protected virtual void Initialize()
        {
            RootScreenObject = GetComponentInChildren<ScreenObject>(true);

            Debug.Assert
            (
                this.ScreenType != ScreenTypeMobile.None,
                "ERROR: Please set ScreenType in every class derived from BaseScreenObjectMobile."
            );
            Debug.Assert
            (
                RootScreenObject != null,
                "ERROR: RootScreenObject is null for: " + this.ScreenType
            );

            if (KnownScreens.ContainsKey(ScreenType))
            {
                Debug.LogWarning
                    ($"Could not add {ScreenType} as it already exists in known screens.");
            }
            else
            {
                KnownScreens.Add(ScreenType, this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (KnownScreens.ContainsKey(ScreenType))
            {
                KnownScreens.Remove(ScreenType);
            }
        }
    }
}
