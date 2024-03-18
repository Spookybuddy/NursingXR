namespace GIGXR.Platform.HMD.UI
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using GIGXR.Platform.UI;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;

    /// <summary>
    /// Allows a screen to be duplicated with this component.
    /// </summary>
    [RequireComponent(typeof(BaseScreenObject))]
    public class DuplicateScreenComponent : BaseUiObject
    {
        #region EditorSetValues

        [SerializeField]
        private Material backgroundMaterial;

        #endregion

        private static Dictionary<BaseScreenObject.ScreenType, int> screenTypeCount = new Dictionary<BaseScreenObject.ScreenType, int>();

        private GameObject duplicateScreenButton;

        private BaseScreenObject _attachedScreenObject;

        private BaseScreenObject AttachedScreenObject
        {
            get
            {
                if (_attachedScreenObject == null)
                    _attachedScreenObject = GetComponent<BaseScreenObject>();

                return _attachedScreenObject;
            }
        }

        private void DuplicateScreen()
        {
            uiEventBus.Publish(new SetAccessoryElementStateToolbarEvent(false));

            var newScreen = AttachedScreenObject.ScreenObjectFactory(false);

            void CloseScreen()
            {
                Destroy(newScreen);
            }

            var closeScreenButton = UiBuilder.BuildMRTKButton(
                buttonText: "",
                buttonClick: CloseScreen,
                buttonIconStyle: Microsoft.MixedReality.Toolkit.UI.ButtonIconStyle.Quad,
                quadIconName: "IconClose");

            closeScreenButton.name = "Close Screen Button";
            closeScreenButton.transform.SetParent(newScreen.transform, false);

            // HACK Manually positioned this in the scene to get this value. Gross.
            closeScreenButton.transform.localPosition = new Vector3(0.0888f, 0.0883f, 0.0f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if(screenTypeCount.ContainsKey(AttachedScreenObject.ScreenObjectType))
            {
                screenTypeCount[AttachedScreenObject.ScreenObjectType] += 1;
            }
            else
            {
                screenTypeCount[AttachedScreenObject.ScreenObjectType] = 1;
            }
        }

        protected void OnDisable()
        {
            if (screenTypeCount.ContainsKey(AttachedScreenObject.ScreenObjectType))
            {
                screenTypeCount[AttachedScreenObject.ScreenObjectType] -= 1;
            }
        }

        public int GetScreenTypeCount(BaseScreenObject.ScreenType screenType)
        {
            if (screenTypeCount.ContainsKey(screenType))
            {
                return screenTypeCount[screenType];
            }

            return 0;
        }

        protected override void SubscribeToEventBuses()
        {
            duplicateScreenButton = UiBuilder.BuildMRTKButton(
                buttonText: "New",
                buttonClick: DuplicateScreen,
                buttonColor: new Color(0.1792453f, 0.1792453f, 0.1792453f),
                fontSize: 0.006f,
                quadMaterial: backgroundMaterial);

            duplicateScreenButton.name = "New Screen Button";
            duplicateScreenButton.SetActive(false);

            // It's not a good idea to publish events to the event bus in the first frame since the order
            // of events for subscribing to all the events probably isn't complete yet
            _ = SetAccessoryNextFrame(this.GetCancellationTokenOnDestroy());
        }

        private async UniTask SetAccessoryNextFrame(CancellationToken token)
        {
            await UniTask.DelayFrame(1, PlayerLoopTiming.Update, token);

            if (!token.IsCancellationRequested)
            {
                uiEventBus.Publish(new AddAccessoryElementToolbarEvent(AttachedScreenObject.ScreenObjectType, duplicateScreenButton));
            }
        }
    }
}