namespace GIGXR.Platform.Mobile
{
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.AppEvents.Events.Session;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Managers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class MobilePromptScreen : MonoBehaviour, IPromptScreen
    {
        private Dictionary<Button, Action> buttonComponents = new Dictionary<Button, Action>();

        [SerializeField]
        private TextMeshProUGUI promptText;

        [SerializeField]
        private TextMeshProUGUI headerText;

        [SerializeField]
        private GameObject buttonHolder;

        [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private RectTransform backgroundTransform;

        [SerializeField]
        private GameObject mobilePromptButtonPrefab;

        public GameObject SelfGameObject => gameObject;

        public Transform GridTransformHolder
        { 
            get 
            { 
                return buttonHolder.transform; 
            } 
        }

        private AppEventBus EventBus { get; set; }

        public void SetDependencies(AppEventBus eventBus)
        {
            EventBus = eventBus;

            EventBus.Subscribe<LeftSessionEvent>(OnLeftSessionEvent);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<LeftSessionEvent>(OnLeftSessionEvent);
        }

        public void CreateButtons(List<ButtonPromptInfo> newButtons, Vector2? size = null)
        {
            foreach (ButtonPromptInfo currentInfo in newButtons ?? Enumerable.Empty<ButtonPromptInfo>())
            {
                var button = Instantiate(mobilePromptButtonPrefab, buttonHolder.transform);

                var textBox = button.GetComponentInChildren<TextMeshProUGUI>();
                textBox.text = currentInfo.buttonText;

                var buttonComponent = button.GetComponent<Button>();

                buttonComponent.onClick.AddListener(() => ButtonObject_OnClick(buttonComponent));

                buttonComponents.Add(buttonComponent, currentInfo.onPressAction);

                if (size != null && size.HasValue)
                {
                    var rectComponent = button.GetComponentInChildren<RectTransform>();

                    rectComponent.sizeDelta = size.Value;
                }
            }
        }

        public void SetButtonLayout(UIPlacementData transformData)
        {
            switch (transformData.ButtonGridLayout)
            {
                case GridLayoutOrder.Horizontal:
                    // TODO Mobile uses the HorizontalLayoutGroup component
                    break;
                case GridLayoutOrder.Vertical:
                    // TODO Mobile uses the VerticalLayoutGroup component
                    break;
                default:
                    break;
            }
        }

        private void ButtonObject_OnClick(Button button)
        {
            buttonComponents[button]?.Invoke();

            button.onClick.RemoveListener(() => ButtonObject_OnClick(button));

            RemoveScreen();
        }

        public void RemoveScreen()
        {
            if(gameObject != null)
                Destroy(gameObject);
        }

        public void SetWindowSize(int windowWidth, int backgroundHeight)
        {
            // TODO For now, multiple by 20 as the WindowState values work for HMD
            backgroundTransform.sizeDelta = new Vector2(windowWidth * 20.0f, backgroundHeight);
        }

        public void SetHeaderText(string header)
        {
            headerText.text = header;
        }

        public void SetWindowText(string message)
        {
            promptText.text = message;
        }

        public void ShowCancelButton(Action cancelAction)
        {
            cancelButton.gameObject.SetActive(true);

            buttonComponents.Add(cancelButton, cancelAction);

            cancelButton.onClick.AddListener(() => OnCancelSelected(cancelButton));
        }

        private void OnCancelSelected(Button button)
        {
            buttonComponents[button]?.Invoke();

            button.onClick.RemoveListener(() => OnCancelSelected(button));

            RemoveScreen();
        }

        public void PlaySFX()
        {
            // Not needed on mobile as the prompts will appear on the device screen itself and not in app's world space
        }

        public void AdjustGridTransform(UIPlacementData transformData)
        {
            if (transformData != null && transformData.ButtonGridLocalPositionOverride.HasValue)
            {
                buttonHolder.transform.localPosition = transformData.ButtonGridLocalPositionOverride.Value;
            }
        }

        private void OnLeftSessionEvent(LeftSessionEvent @evt)
        {
            // Mobile devices never utilize prompts outside of an active session, so if a prompt gets this event, it's still active
            // while outside of a session and should remove itself
            RemoveScreen();
        }
    }
}