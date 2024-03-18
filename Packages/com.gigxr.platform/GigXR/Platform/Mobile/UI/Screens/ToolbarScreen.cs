using GIGXR.Platform.AppEvents.Events.UI;
using GIGXR.Platform.Mobile.AppEvents.Events.UI;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Managers;
using GIGXR.Platform.Sessions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GIGXR.Platform.AppEvents.Events.Session;
using GIGXR.Platform.Core.User;
using UnityEngine.UI;
using GIGXR.Platform.Mobile.AppEvents.Events.AR;
using GIGXR.Platform.AppEvents.Events.Calibration;

namespace GIGXR.Platform.Mobile.UI
{
    /// <summary>
    ///     The ToolbarScreen is active while in a session.
    ///     It includes the public API for its contained UI
    ///     elements. This includes displaying a participants list,
    ///     and leaving the session.
    ///     
    ///     The toolbar also contains a button to re-scan, which
    ///     calls <see cref="SessionScreen.Rescan"/>
    /// </summary>
    public class ToolbarScreen : BaseScreenObjectMobile
    {
        public override ScreenTypeMobile ScreenType => ScreenTypeMobile.Toolbar;

        // TODO Improvement Could use prefabs to instantiate these, rather than create MonoBehavior hook ups
        [SerializeField]
        private GameObject moreMenuGameObjects;

        [SerializeField]
        private GameObject moreContentGameObjects;

        [SerializeField]
        private GameObject participantListGameObject;

        [SerializeField]
        private Toggle headToggle;

        [SerializeField]
        private Toggle handToggle;

        [SerializeField]
        private Toggle nametagToggle;

        // The height of the 'More Content' that must be scrolled to be visible
        [SerializeField]
        private float screenHeight = 1080f;

        private bool _isMenuOpen = false;

        private bool resetInProgress = false;

        private Vector3 startMenuLocalPosition;

        private ISessionManager SessionManager { get; set; }

        [InjectDependencies]
        public void Construct(ISessionManager sessionManager)
        {
            SessionManager = sessionManager;
        }

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            EventBus.Subscribe<JoinedSessionEvent>(OnJoinedSessionEvent);
            EventBus.Subscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);
            EventBus.Subscribe<StartAnchorRootEvent>(OnStartAnchorRootEvent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventBus.Unsubscribe<JoinedSessionEvent>(OnJoinedSessionEvent);
            EventBus.Unsubscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);
            EventBus.Unsubscribe<StartAnchorRootEvent>(OnStartAnchorRootEvent);
        }

        protected override void Initialize()
        {
            base.Initialize();

            startMenuLocalPosition = moreMenuGameObjects.transform.localPosition;
        }

        protected override void OnSwitchingActiveScreenEvent(SwitchingActiveScreenEventMobile @event)
        {
            ResetToolbar();

            RootScreenObject.SetActive(ScreenTypeMobile.Session == @event.TargetScreen);
        }

        private void ResetToolbar()
        {
            _isMenuOpen = false;

            moreMenuGameObjects.transform.localPosition = startMenuLocalPosition;
            participantListGameObject.transform.localPosition
                = moreContentGameObjects.transform.localPosition + new Vector3(Screen.width, 0, 0);
        }

        // https://gamedevbeginner.com/the-right-way-to-lerp-in-unity-with-examples/#lerp_vector3
        private IEnumerator LerpPosition
        (
            GameObject targetObject,
            Vector3    targetPosition,
            float      duration
        )
        {
            float   time          = 0;
            Vector3 startPosition = targetObject.transform.localPosition;

            while (time < duration)
            {
                targetObject.transform.localPosition = Vector3.Lerp
                    (startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            targetObject.transform.localPosition = targetPosition;
        }

        private void OnArTargetPlacedEvent(ArTargetPlacedEvent @event)
        {
            // When the user resets the anchor, we need to make sure to bring the toolbar back up
            if(resetInProgress)
            {
                RootScreenObject.SetActive(true);
            }

            resetInProgress = false;
        }

        private void OnStartAnchorRootEvent(StartAnchorRootEvent @event)
        {
            resetInProgress = @event.FromReset;
        }

        private void OnJoinedSessionEvent(JoinedSessionEvent @event)
        {
            // Check to see if they are different, isOn only invokes a change if they are different
            if (headToggle.isOn != UserRepresentations.HeadsEnabled)
                headToggle.isOn = UserRepresentations.HeadsEnabled;
            else
                headToggle.GetComponent<ToggleEvents>()?.SetToggleState(headToggle.isOn);

            if(handToggle.isOn != UserRepresentations.HandsEnabled)
                handToggle.isOn = UserRepresentations.HandsEnabled;
            else
                handToggle.GetComponent<ToggleEvents>()?.SetToggleState(handToggle.isOn);

            if (nametagToggle.isOn != UserRepresentations.NameTagsEnabled)
                nametagToggle.isOn = UserRepresentations.NameTagsEnabled;
            else
                nametagToggle.GetComponent<ToggleEvents>()?.SetToggleState(nametagToggle.isOn);
        }

        #region Public API

        /// <summary>
        /// Called via Unity Editor when a user leaves a session.
        /// Prompts the user to leave the session or cancel.
        /// </summary>
        public void PromptLeaveSession()
        {
            var buttonList = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = "Ok",
                    onPressAction = () =>
                                    {
                                        SessionManager.LeaveSessionAsync();
                                    }
                },
                new ButtonPromptInfo()
                {
                    buttonText = "Cancel"
                    // No action
                }
            };

            // TODO Externalize text
            EventBus.Publish
            (
                new ShowPromptEvent
                (
                    "Are you sure you wish to leave the session?",
                    buttonList,
                    PromptManager.WindowStates.Wide
                )
            );
        }

        /// <summary>
        /// Called via Unity Editor when the user hits the "more"
        /// button on the toolbar. Shows the toolbar menu,
        /// including buttons to leave the session, view participants,
        /// or rescan.
        /// </summary>
        public void OpenToolbarMenu()
        {
            if (_isMenuOpen)
            {
                CloseToolbarMenu();

                return;
            }

            _isMenuOpen = true;

            StartCoroutine
            (
                LerpPosition
                (
                    moreMenuGameObjects,
                    new Vector3
                    (
                        startMenuLocalPosition.x,
                        screenHeight,
                        startMenuLocalPosition.z
                    ),
                    1.0f
                )
            );
        }

        /// <summary>
        /// Called via Unity Editor. Hides the toolbar menu.
        /// </summary>
        public void CloseToolbarMenu()
        {
            _isMenuOpen = false;

            CloseParticipantList();

            StartCoroutine(LerpPosition(moreMenuGameObjects, startMenuLocalPosition, 1.0f));
        }

        /// <summary>
        /// Called via Unity Editor in the toolbar menu.
        /// Shows a list of participants in the current session.
        /// </summary>
        public void OpenParticipantList()
        {
            StartCoroutine
            (
                LerpPosition
                (
                    participantListGameObject,
                    new Vector3
                    (
                        moreMenuGameObjects.transform.localPosition
                            .x, // align with the the main menu
                        participantListGameObject.transform.localPosition.y,
                        participantListGameObject.transform.localPosition.z
                    ),
                    1.0f
                )
            );
        }

        /// <summary>
        /// Called via Unity Editor in the toolbar menu.
        /// Hides the session participant list.
        /// </summary>
        public void CloseParticipantList()
        {
            StartCoroutine
            (
                LerpPosition
                (
                    participantListGameObject,
                    new Vector3
                    (
                        moreMenuGameObjects.transform.localPosition.x + Screen.width,
                        participantListGameObject.transform.localPosition.y,
                        participantListGameObject.transform.localPosition.z
                    ),
                    1.0f
                )
            );
        }

        /// <summary>
        /// Called via Unity Editor
        /// </summary>
        public void ToggleAvatarHeads(bool state)
        {
            UserRepresentations.SetAllAvatarHeadState(state);
        }

        /// <summary>
        /// Called via Unity Editor
        /// </summary>
        public void ToggleAvatarHands(bool state)
        {
            UserRepresentations.SetAllAvatarHandState(state);
        }

        /// <summary>
        /// Called via Unity Editor
        /// </summary>
        public void ToggleNameTags(bool state)
        {
            UserRepresentations.SetAllNametagState(state);
        }

        #endregion
    }
}