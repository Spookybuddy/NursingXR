using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Sessions;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GIGXR.Platform.HMD.UI
{
    public class UiInfo
    {

    }

    public class ImageInfo : UiInfo
    {
        public Color color;

        public ImageInfo(Color color)
        {
            this.color = color;
        }
    }

    public class ButtonInfo : UiInfo
    {
        public string buttonText;
        public UnityAction buttonAction;

        public ButtonInfo(string buttonText, UnityAction buttonAction)
        {
            this.buttonText = buttonText;
            this.buttonAction = buttonAction;
        }
    }

    public class TextInfo : UiInfo
    {
        public string text;
        public float? fontSize;
        public TextAlignmentOptions? textAlignment;
        public Color? textColor;

        public TextInfo(string text, float? fontSize = null, TextAlignmentOptions? textAlignment = null, Color? textColor = null)
        {
            this.text = text;
            this.fontSize = fontSize;
            this.textAlignment = textAlignment;
            this.textColor = textColor;
        }
    }

    [RequireComponent(typeof(DebugScreen))]
    public class AssetDebugScreen : MonoBehaviour
    {
        private DebugScreen debugScreen;

        private GridLayoutGroup assetListOptionHolder;

        private Dictionary<Guid, GameObject> createdAssetPanes = new Dictionary<Guid, GameObject>();

        private Dictionary<Guid, Slate> assetSlates = new Dictionary<Guid, Slate>();
        private Dictionary<IAssetTypeComponent, (Slate, OmniUIConnection)> assetTypeDetailSlates = new Dictionary<IAssetTypeComponent, (Slate, OmniUIConnection)>();

        private Slate assetListSlate;

        private Vector2 slateSize = new Vector2(650.0f, 60.0f);

        // An ATC may have multiple property definitions, so there may be multiple GOs to manage
        private Dictionary<string, TextMeshProUGUI> watchedPropertyChangeTextUpdates = new Dictionary<string, TextMeshProUGUI>();

        private ISessionManager SessionManager { get; set; }

        private IBuilderManager UiBuilder { get; set; }

        [InjectDependencies]
        public void ConstructDependencies(ISessionManager sessionManager, IBuilderManager uiBuilder)
        {
            SessionManager = sessionManager;
            UiBuilder = uiBuilder;

            assetListSlate = UiBuilder.BuildSlate("Browse Assets - GigXR Developer Tools");

            assetListOptionHolder = UiBuilder.BuildObjectCollection(slateSize,
                    new Vector2(0.0f, 20.0f),
                    GridLayoutGroup.Corner.UpperLeft,
                    GridLayoutGroup.Constraint.FixedColumnCount,
                    1);

            debugScreen = GetComponent<DebugScreen>();
            // HACK Need these added on the next frame since they happen in the Inject phase
            Invoke(nameof(NextFrame), .5f);

            SessionManager.ScenarioManager.AssetManager.AssetPropertyUpdated += AssetManager_AssetPropertyUpdated;
        }

        private void NextFrame()
        {
            debugScreen.AddDebugButton("Browse Assets", ShowAssetListSlate);

            assetListSlate.AddContent(assetListOptionHolder.gameObject);

            assetListSlate.gameObject.SetActive(false);
        }

        protected void OnDestroy()
        {
            SessionManager.ScenarioManager.AssetManager.AssetPropertyUpdated -= AssetManager_AssetPropertyUpdated;
        }

        private Vector2 textBoxSize = new Vector2(0.08f, 0.01f);

        private string CreatePropertyDetails(IAssetTypeComponent assetType, string propertyName, Guid stageId, bool useInitialValue = true)
        {
            var value = assetType.GetPropertyValueAtStage(stageId, propertyName, useInitialValue);
            string valueString = "";

            if (value.GetType().IsGenericType && value is IEnumerable<object> enumerate)
            {
                int objectCount = 0;

                foreach (var obj in enumerate)
                {
                    objectCount++;

                    // TODO Trailing comma
                    valueString += obj.ToString() + ", ";
                }

                if (objectCount == 0)
                {
                    valueString = "[Empty collection]";
                }
            }
            else
            {
                // TODO Deal with custom classes?
                valueString = value.ToString();
            }

            return valueString;
        }

        private void CreateAssestTypeComponentInformation(IAssetTypeComponent assetType, Transform newParent, Slate parentSlate)
        {
            foreach (var propertyName in assetType.GetAllPropertyDefinitionNames())
            {
                // Toggles displaying the details for a particular asset type component
                void ExpandAssetTypeDetails()
                {
                    CreateOrShowSlateForAssetPropertyDetails(assetType, propertyName, parentSlate);
                }

                var horizontalContainer = UiBuilder.BuildHorizontalCollection(5.0f);
                horizontalContainer.transform.SetParent(newParent, false);

                var assetTypeObject = GameObject.Instantiate(UiBuilder.DefaultStyle.slateTextPrefab);
                assetTypeObject.transform.SetParent(horizontalContainer.transform, false);

                var assetTypeData = assetTypeObject.GetComponent<OmniUIConnection>();
                assetTypeData.Setup("text", new TextInfo(propertyName, 18.0f));
                assetTypeData.SetPreferredSize(new Vector2(slateSize.x * 0.25f, slateSize.y));

                var propertyDetails = CreatePropertyDetails(assetType,
                                                            propertyName,
                                                            SessionManager.ScenarioManager.StageManager.CurrentStage.StageId,
                                                            false);

                var propertyDisplay = GameObject.Instantiate(UiBuilder.DefaultStyle.slateTextWithButtonPrefab);
                propertyDisplay.transform.SetParent(horizontalContainer.transform, false);

                var propertyData = propertyDisplay.GetComponent<OmniUIConnection>();
                propertyData.Setup("button", new ButtonInfo("Expand", ExpandAssetTypeDetails));
                propertyData.Setup("text", new TextInfo(propertyDetails, 18.0f));
                propertyData.SetPreferredSize(new Vector2(slateSize.x * 0.75f, slateSize.y));

                watchedPropertyChangeTextUpdates.Add($"{assetType.AttachedAssetMediator.AssetId}.{propertyName}", propertyData.GetTextObject("text"));
            }
        }

        private void AssetManager_AssetPropertyUpdated(object sender, Scenarios.GigAssets.EventArgs.AssetPropertyChangeEventArgs e)
        {
            var compositeKey = $"{e.AssetId}.{e.AssetPropertyName}";

            if (watchedPropertyChangeTextUpdates.ContainsKey(compositeKey))
            {
                watchedPropertyChangeTextUpdates[compositeKey].text = e.AssetPropertyValue.ToString();
            }
        }

        private void CreateOrShowSlateForAssetDetails(IAssetMediator asset)
        {
            if (assetSlates.ContainsKey(asset.AssetId))
            {
                assetSlates[asset.AssetId].Show(assetListSlate.transform);
            }
            else
            {
                var currentSlate = UiBuilder.BuildSlate("Browse Assets - GigXR Developer Tools");
                currentSlate.gameObject.name += $" {asset.PresetAssetId} Property Details";

                var debugOptionHolder = UiBuilder.BuildObjectCollection(new Vector2(650.0f, 40.0f),
                    new Vector2(0.0f, 5.0f),
                    GridLayoutGroup.Corner.UpperLeft,
                    GridLayoutGroup.Constraint.FixedColumnCount,
                    1);

                currentSlate.AddContent(debugOptionHolder.gameObject);

                // Display each property in the selected asset
                foreach (var assetType in asset.GetAllKnownAssetTypes())
                {
                    CreateAssestTypeComponentInformation(assetType, debugOptionHolder.transform, currentSlate);
                }

                assetSlates.Add(asset.AssetId, currentSlate);

                currentSlate.Show(assetListSlate.transform);
            }
        }

        private void CreateOrShowSlateForAssetPropertyDetails(IAssetTypeComponent assetTypeComponent, string property, Slate parentSlate)
        {
            var expandedDataText = assetTypeComponent.AttachedAssetMediator.SerializeAssetTypeComponent(assetTypeComponent.GetAssetTypeName(),
                                                                                            assetProperty: property,
                                                                                            formatting: Newtonsoft.Json.Formatting.Indented);

            if (assetTypeDetailSlates.ContainsKey(assetTypeComponent))
            {
                assetTypeDetailSlates[assetTypeComponent].Item2.Setup("text", new TextInfo(text: expandedDataText,
                                                         fontSize: 16.0f,
                                                         textAlignment: TextAlignmentOptions.TopLeft,
                                                         textColor: Color.white));

                assetTypeDetailSlates[assetTypeComponent].Item1.ResetScroll();

                assetTypeDetailSlates[assetTypeComponent].Item1.Show(parentSlate.transform);
            }
            else
            {
                var currentSlate = UiBuilder.BuildSlate("Browse Assets - GigXR Developer Tools");
                currentSlate.gameObject.name += $" {nameof(assetTypeComponent)} Property Details";

                // Create a black background to lay on top of the default background per Prod specs
                var background = GameObject.Instantiate(UiBuilder.DefaultStyle.slatePureImagePrefab);
                currentSlate.AddContentAsSibling(background);

                var backgroundConnection = background.GetComponent<OmniUIConnection>();
                backgroundConnection.Setup("image", new ImageInfo(Color.black));

                // Normally the mask bounds extends to the top and bottom of the screen, but they should
                // be confined to the background area here
                currentSlate.AdjustMaskPadding(new Vector4(0f, 20f, 0f, 20f));

                var assetTypeComponentDetailText = GameObject.Instantiate(UiBuilder.DefaultStyle.slatePureTextPrefab);
                currentSlate.AddContent(assetTypeComponentDetailText);

                var assetTypeData = assetTypeComponentDetailText.GetComponent<OmniUIConnection>();

                assetTypeData.Setup("text", new TextInfo(text: expandedDataText, 
                                                         fontSize: 16.0f, 
                                                         textAlignment: TextAlignmentOptions.TopLeft,
                                                         textColor: Color.white));

                assetTypeDetailSlates.Add(assetTypeComponent, (currentSlate, assetTypeData));

                currentSlate.Show(parentSlate.transform);
            }
        }

        private void CreateAssetPane(Guid assetId, Transform newParent)
        {
            if (!createdAssetPanes.ContainsKey(assetId))
            {
                var currentAsset = SessionManager.ScenarioManager.AssetManager.GetById(assetId)?.GetComponent<IAssetMediator>();

                void ShowAssetDetails()
                {
                    CreateOrShowSlateForAssetDetails(currentAsset);
                }

                var assetDisplay = GameObject.Instantiate(UiBuilder.DefaultStyle.slateTextWithButtonPrefab);
                assetDisplay.transform.SetParent(newParent, false);

                var assetData = assetDisplay.GetComponent<OmniUIConnection>();
                assetData.Setup("button", new ButtonInfo("Details", ShowAssetDetails));
                assetData.Setup("text", new TextInfo(currentAsset.PresetAssetId));
                
                createdAssetPanes.Add(assetId, assetDisplay);
            }
        }

        private void ShowAssetListSlate()
        {
            assetListSlate.Show(debugScreen.SlateTransform);

            foreach (var asset in SessionManager.ScenarioManager.AssetManager.InstantiatedAssets)
            {
                CreateAssetPane(asset.Key, assetListOptionHolder.transform);
            }
        }
    }
}