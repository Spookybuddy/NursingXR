namespace GIGXR.Platform.CommonAssetTypes.StageNavigator.Scripts.MainScripts
{
    using GIGXR.Platform.Scenarios.GigAssets;
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
    using GIGXR.Platform.Scenarios.Stages.Data;
    using GIGXR.Platform.UI.Tools;
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.Rendering;
    using Microsoft.MixedReality.Toolkit.UI;
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// AssetTypeComponent that allows the asset to interface with the stage data.
    /// </summary>
    public class StageNavigatorAssetTypeComponent : 
        BaseAssetTypeComponent<StageNavigatorAssetData>,
        ISerializationCallbackReceiver
    {
        [Serializable]
        private struct ObjectReferences
        {
            public GameObject headerText; 
            public GameObject contentText; 
            public GameObject previousStageButton; 
            public GameObject nextStageButton; 
            public GameObject startOverButton;
        }
        [Header("UI object references; don't hit Reset, disabled the save for performance reasons")] 
        [SerializeField] private ObjectReferences objects;
        private ObjectReferences savedObjectReferences;
        
        public delegate void RequestingStageList();
        public event RequestingStageList RequestingStageListEvent;
        
        public delegate void StageNavigatorButtonClicked();
        public event StageNavigatorButtonClicked StageNavigatorButtonClickedEvent;

        // TODO 
        // temp for this build:
        private bool isHost;

        protected override void OnEnable()
        {
            base.OnEnable();

            objects.nextStageButton.GetComponent<Interactable>().OnClick.AddListener(OnClickedNextStageButton);
            objects.previousStageButton.GetComponent<Interactable>().OnClick.AddListener(OnClickedPreviousStageButton);
            objects.startOverButton.GetComponent<Interactable>().OnClick.AddListener(OnClickedStartOverButton);

            OnAwakened += RequestStageInformation;
        }

        private void RequestStageInformation(object sender, EventArgs e)
        {
            isHost          = true;

            RequestingStageListEvent?.Invoke();

            SetButtonState(objects.previousStageButton, false);
            SetButtonState(objects.nextStageButton, true);
            SetButtonState(objects.startOverButton, true);
            
            assetData.stageIndex.runtimeData.UpdateValue(0, AssetPropertyChangeOrigin.Initialization);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            OnAwakened -= RequestStageInformation;
            
            objects.nextStageButton.GetComponent<Interactable>().OnClick.RemoveListener(OnClickedNextStageButton);
            objects.previousStageButton.GetComponent<Interactable>().OnClick.RemoveListener(OnClickedPreviousStageButton);
            objects.startOverButton.GetComponent<Interactable>().OnClick.RemoveListener(OnClickedStartOverButton);
        }
        
        private void DisplayCurrentStageName(string currentStageName)
        {
            string newText = currentStageName;

            // todo 
            string contentText = assetData.contentText.runtimeData.Value;
            if (contentText != "")
            {
                newText += "\n" + contentText;
            }
            
            objects.contentText.GetComponent<TextMeshPro>().SetText(newText);
        }

        void SetStatesForAllButtons()
        {
            int currentStageIndex = assetData.stageIndex.runtimeData.Value;
            int maxStageIndex = assetData.maxStageIndex.runtimeData.Value;

            SetButtonState(objects.nextStageButton, false);
            SetButtonState(objects.previousStageButton, false);
            SetButtonState(objects.startOverButton, true);

            if (currentStageIndex == 0)
            {
            }

            if (maxStageIndex != 0)
            {
            }

            if (currentStageIndex > 0)
            {
                SetButtonState(objects.previousStageButton, true);
            }

            if (currentStageIndex < maxStageIndex)
            {
                SetButtonState(objects.nextStageButton, true);
            }
        }

        void SetButtonState(GameObject button, bool shouldBeActive)
        {
            Transform quadTransform = button.transform.Find("BackPlate/Quad");
            Transform buttonText = button.transform.Find("IconAndText/TextMeshPro");
            
            if (quadTransform != null && buttonText != null)
            {
                Material material = quadTransform.gameObject.EnsureComponent<MaterialInstance>().Material;
                var textMeshProComponent = buttonText.GetComponent<TextMeshPro>();

                Color newButtonColor;
                Color newTextColor;

                button.GetComponent<Collider>()
                    .enabled = shouldBeActive;

                if (shouldBeActive)
                {
                    newButtonColor   = UiTools.hexToColor("183076");
                    newButtonColor.a = 0.5529412f;
                    newTextColor     = UiTools.hexToColor("FFFFFF");
                    newTextColor.a   = 1;
                }
                else
                {
                    newButtonColor   = UiTools.hexToColor("252525");
                    newButtonColor.a = 0.235f;
                    newTextColor     = UiTools.hexToColor("FFFFFF");
                    newTextColor.a   = 0.2352941f;
                }

                material.color             = newButtonColor;
                textMeshProComponent.color = newTextColor;
            }
        }

        protected virtual void OnClickedNextStageButton()
        {
            int nextStageIndex = assetData.stageIndex.runtimeData.Value;
            int maxStageIndex = assetData.maxStageIndex.runtimeData.Value;
            
            if (nextStageIndex + 1 <= maxStageIndex)
            {
                assetData.stageIndex.runtimeData.UpdateValue(nextStageIndex + 1, AssetPropertyChangeOrigin.ValueSet);
            
                StageNavigatorButtonClickedEvent?.Invoke();
            }
            
            SetStatesForAllButtons();
        }

        protected virtual void OnClickedPreviousStageButton()
        {
            int currentStageIndex = assetData.stageIndex.runtimeData.Value;
            int newStageIndex = currentStageIndex - 1; 
            
            if (newStageIndex >= 0)
            {
                assetData.stageIndex.runtimeData.UpdateValue(newStageIndex, AssetPropertyChangeOrigin.ValueSet);
                StageNavigatorButtonClickedEvent?.Invoke();
            }
            
            SetStatesForAllButtons();
        }

        protected virtual void OnClickedStartOverButton()
        {
            assetData.stageIndex.runtimeData.UpdateValue(0, AssetPropertyChangeOrigin.ValueSet);
            StageNavigatorButtonClickedEvent?.Invoke();
            SetStatesForAllButtons();
        }

        protected override void Setup()
        {
            
        }

        protected override void Teardown()
        {
            
        }

        [RegisterPropertyChange(nameof(StageNavigatorAssetData.stageIndex))]
        private void OnStageIndexChanged(AssetPropertyChangeEventArgs e) => OnStageIndexChanged((int)e.AssetPropertyValue);
        private void OnStageIndexChanged(int stageIndex)
        {
            // was scenarioStarted
            if (assetData.stageList.runtimeData.Value.Count > 0)
            {
                SetStatesForAllButtons();
                DisplayCurrentStageName(assetData.stageList.runtimeData.Value[stageIndex].stageTitle);
            }
        }

        [RegisterPropertyChange(nameof(StageNavigatorAssetData.stageList))]
        private void OnStageListChanged(AssetPropertyChangeEventArgs e) => OnStageListChanged((List<Stage>)e.AssetPropertyValue);
        private void OnStageListChanged(List<Stage> stageList)
        {
            int numberOfStages = stageList.Count;
            if (numberOfStages > 0)
            {
                int currentStage = assetData.stageIndex.runtimeData.Value;
                DisplayCurrentStageName(stageList[currentStage].stageTitle);
                assetData.maxStageIndex.runtimeData.UpdateValue(numberOfStages - 1, AssetPropertyChangeOrigin.ValueSet);
            }
        }

        public override void SetEditorValues()
        {
            assetData.name.designTimeData.defaultValue 
                = "Stage Navigator";
            assetData.description.designTimeData.defaultValue 
                = "Allows the user to navigate stages as determined by the flow of the Scenario.";
            assetData.headerText.designTimeData.defaultValue
                = "Header";            
            assetData.contentText.designTimeData.defaultValue
                = "";            
            assetData.leftButtonText.designTimeData.defaultValue
                = "Previous";            
            assetData.rightButtonText.designTimeData.defaultValue
                = "Next";
            assetData.showStages.designTimeData.defaultValue 
                = true;
            assetData.stageIndex.designTimeData.defaultValue
                = 0;
            assetData.stageList.designTimeData.defaultValue
                = new List<Stage>();
            assetData.maxStageIndex.designTimeData.defaultValue
                = 99; 
            
            // objects = savedObjectReferences;
        }

        public void OnBeforeSerialize()
        {
            // savedObjectReferences = objects; 
        }

        public void OnAfterDeserialize()
        {
        }

        protected virtual void OnStageNavigatorButtonClickedEvent()
        {
            StageNavigatorButtonClickedEvent?.Invoke();
        }
    }
}
