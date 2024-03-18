namespace GIGXR.Platform.HMD.UI
{
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Managers;
    using GIGXR.Platform.UI;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Prompts for HMD
    /// </summary>
    public class PromptScreen : BaseUiObject, IPromptScreen
    {
        [SerializeField] private GameObject promptButtonPrefab;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private GridObjectCollection buttonGrid;

        [SerializeField]
        private AudioSource audioSFX;

        private bool removed = false;

        public GameObject SelfGameObject => gameObject;

        private IBuilderManager BuilderManager;

        [InjectDependencies]
        public void Construct(IBuilderManager builder)
        {
            BuilderManager = builder;
        }

        private AppEventBus EventBus { get; set; }

        public void SetDependencies(AppEventBus eventBus)
        {
            EventBus = eventBus;
        }

        public void SetHeaderText(string header)
        {
            headerText.text = header;
        }

        public void SetWindowText(string message)
        {
            promptText.text = message;
        }

        public void CreateButtons(List<ButtonPromptInfo> newButtons, Vector2? size = null)
        {
            foreach (ButtonPromptInfo currentInfo in newButtons ?? Enumerable.Empty<ButtonPromptInfo>())
            {
                // TODO Prompt button sizes are currently hard-coded, but they are all expected to be the same size at the moment
                var button = BuilderManager.BuildMRTKButton(buttonClick: () =>
                                                        {
                                                            RemoveScreen();

                                                            currentInfo.onPressAction?.Invoke();
                                                        },
                                                        buttonText: currentInfo.buttonText,
                                                        buttonSize: new Vector3(0.04f, 0.025f, 0.01f),
                                                        fontSize: 0.006f);

                button.transform.SetParent(buttonGrid.transform, false);
            }

            buttonGrid.UpdateCollection();
        }

        public void SetButtonLayout(UIPlacementData transformData)
        {
            switch (transformData.ButtonGridLayout)
            {
                case GridLayoutOrder.Horizontal:
                    buttonGrid.Layout = LayoutOrder.Horizontal;
                    //TODO Make data driven buttonGrid.CellWidth
                    break;
                case GridLayoutOrder.Vertical:
                    buttonGrid.Layout = LayoutOrder.Vertical;
                    //TODO Make data driven buttonGrid.CellHeight
                    break;
                default:
                    break;
            }
        }

        public void AdjustGridTransform(UIPlacementData transformData)
        {
            if (transformData != null && transformData.ButtonGridLocalPositionOverride.HasValue)
            {
                buttonGrid.transform.localPosition = transformData.ButtonGridLocalPositionOverride.Value;
            }
        }

        public void SetWindowSize(int windowWidth, int backgroundHeight)
        {
            if (windowWidth <= 0)
            {
                backgroundImage.enabled = false;
            }
            else
            {
                backgroundImage.enabled = true;

                RectTransform j = backgroundImage.GetComponent<RectTransform>();
                j.sizeDelta = new Vector2(windowWidth, backgroundHeight);
            }
        }

        public void RemoveScreen()
        {
            // Prevents this Destroy from being called again in the case of multiple cancellation or remove tokens happening on the screen
            if (!removed)
            {
                removed = true;

                Destroy(transform?.gameObject);
            }
        }

        public void PlaySFX()
        {
            // Only play an SFX if the prompt is not visible in the camera
            if (backgroundImage.rectTransform.CountCornersVisibleFrom(Camera.main) == 0)
            {
                audioSFX?.Play();
            }
        }

        protected void OnDestroy()
        {
            removed = true;
        }

        protected override void SubscribeToEventBuses()
        {
        }
    }
}