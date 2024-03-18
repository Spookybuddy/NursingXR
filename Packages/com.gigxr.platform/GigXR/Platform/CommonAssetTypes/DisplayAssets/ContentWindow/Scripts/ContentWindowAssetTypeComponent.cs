using GIGXR.Platform.Scenarios.GigAssets;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Rendering;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GIGXR.Platform.CommonAssetTypes.DisplayAssets.ContentWindow.Scripts
{
    using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
    using HMD.UI.Layouts;

    [HelpURL
        ("https://docs.google.com/document/d/1tTsuvwXkLk3dJsm-7AKMo1E28irG5xvWxlmb2sjygMY/edit#")]
    public class ContentWindowAssetTypeComponent : BaseAssetTypeComponent<ContentWindowAssetData>
    {
        /// <summary>
        /// Due to MRTK scroll goofiness, you need to use a background
        /// texture to get the scroll area long enough for chunks of text. 
        /// </summary>
        [SerializeField]
        [Header("Background texture, invisible, used for text scrolling")]
        private Texture scrollSizer;

        [SerializeField]
        private GameObject scrollRoot;

        /// <summary>
        /// Controls scrolling collection. 
        /// </summary>
        private ScrollingObjectCollection scrollView;

        [SerializeField]
        private GameObject scrollContainer;

        [SerializeField]
        private TextMeshPro headerTextReference;

        [Header("Should be Seguisb SDF to support text masking.")]
        [SerializeField]
        private TMP_FontAsset fontAsset;

        private bool isInitialized = false;

        /// <summary>
        /// Used to clean up after the operation ends. 
        /// </summary>
        private AsyncOperationHandle addressablesHandle;

        /// <summary>
        /// Scroll manager.
        /// </summary>
        [SerializeField]
        private ScrollButtonLayout scrollButtonLayout;

        protected void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            addressablesHandle = Addressables.InitializeAsync(true);
            scrollView   = scrollRoot.GetComponent<ScrollingObjectCollection>();

            scrollButtonLayout.ScrollUpButton.OnClick.AddListener(OnScrollUpButtonClicked);
            scrollButtonLayout.ScrollDownButton.OnClick.AddListener(OnScrollDownButtonClicked);

            isInitialized = true;
        }

        private void OnScrollUpButtonClicked()
        {
            scrollView.MoveByTiers(-1);
        }
        
        private void OnScrollDownButtonClicked()
        {
            scrollView.MoveByTiers(1);
        }

        // TODO - Julia move this elsewhere
        void SetPositionAtUserGaze()
        {
            // attach solver handler and radial view, possibly to root where transform stuff is todo 

            // AssetMediator assetMediator = GetComponent<AssetMediator>();
            // SolverHandler solverHandler = objects.displayRoot.GetComponent<SolverHandler>();
            // RadialView radialView = objects.displayRoot.GetComponent<RadialView>();
            // Vector3 gazeOrigin = CoreServices.InputSystem.GazeProvider.GazeOrigin;
            // Vector3 gazeDirection = CoreServices.InputSystem.GazeProvider.GazeDirection;
            // Vector3 spawnPosition = new Vector3();
            // Vector3 displayRootOriginalPosition = objects.displayRoot.transform.position; 
            //
            // // spawnPosition += gazeOrigin;
            // // spawnPosition += gazeDirection;
            //     
            // // var transform = solverHandler.TransformTarget;
            // // Vector3 origin = transform.position;
            // // Vector3 endpoint = transform.position + transform.forward;
            //
            // Vector3 globalGoalPosition = solverHandler.TransformTarget.position + (solverHandler.TransformTarget.forward * 1.5f);
            //
            // // Vector3 localGoalPosition = transform.InverseTransformPoint(solverHandler.TransformTarget.position);
            // // Update the object transform
            // // assetMediator.SetAssetProperty("position", localGoalPosition);
            //
            // globalGoalPosition.y += 0.4f; 
            //
            // this.transform.position = globalGoalPosition;
            //
            // Debug.Log("Gaze Origin: " + gazeOrigin);a
            // Debug.Log("Gaze Direction: " + gazeDirection); 
            // Debug.Log("Goal Position (global version):" + globalGoalPosition);

            // Rotate to face user TODO 
            // Use set asset property and local position TODO 
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        /// <summary>
        /// Used to set display width for the generated material.
        /// </summary>
        private const float displayWidth = 0.64f;

        private IEnumerator DisplayContent(string content)
        {
            isDisplayRoutineRunning = true;

            // by default, texture is set to the generic background image
            Texture texture        = scrollSizer;
            bool    isImageContent = false;

            // If something was loaded previously, release it now, lest we lose track of it
            RemoveCurrentContent();

            // Create the base game object to store the image
            GameObject imageObject = new GameObject { name = "image" };

            // Add components to the image object 
            MeshFilter   meshFilter        = imageObject.AddComponent<MeshFilter>();
            MeshRenderer imageMeshRenderer = imageObject.AddComponent<MeshRenderer>();

            // TODO - just have one quad at beginning?
            // Customize the components on the image object
            GameObject quadObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            meshFilter.mesh = quadObject.GetComponent<MeshFilter>().mesh;
            Destroy(quadObject); // used to be destroyImmediate

            // Identify type of content to display
            if (content.StartsWith("[KEY]"))
            {
                // This must be an addressable key pointing to content
                isImageContent = true;
                var key = content.Replace("[KEY]", "");

                // Load the image using the key 
                addressablesHandle = Addressables.InitializeAsync(true);
                yield return addressablesHandle;

                addressablesHandle = Addressables.LoadAssetAsync<Texture>(key);
                yield return addressablesHandle;

                if (addressablesHandle.Status == AsyncOperationStatus.Failed)
                {
                    Debug.LogError("Could not find an asset using that key.");
                    isDisplayRoutineRunning = false;
                    yield break;
                }

                texture = addressablesHandle.Result as Texture;
                Addressables.Release(addressablesHandle);
            }

            // Create a new material instance and assign the albedo texture from the image key
            imageMeshRenderer.material = new Material(StandardShaderUtility.MrtkStandardShader);
            Material material = imageObject.EnsureComponent<MaterialInstance>().Material;
            material.mainTexture = texture;

            // Move into clipping zone container
            imageObject.transform.SetParent(scrollContainer.transform);

            // Set quad size to fit expected content
            Vector3 localScale       = imageObject.transform.localScale;
            Vector3 newScale         = localScale;
            var     newContentHeight = (displayWidth * (texture.height / (float)texture.width));

            newScale.y                       = newContentHeight;
            newScale.x                       = displayWidth;
            imageObject.transform.localScale = newScale;

            // Set transform
            // TODO - y used to be 0.355f
            imageObject.transform.localPosition = new Vector3
                (
                    0.355f,
                    (-newContentHeight / 2),
                    -0.001f
                );

            // Set rotation
            imageObject.transform.rotation      = Quaternion.identity;
            imageObject.transform.localRotation = Quaternion.identity;

            if (!isImageContent)
            {
                // Hide the image itself
                imageMeshRenderer.enabled = false;

                GameObject textObject = new GameObject { name = "text" };

                textObject.AddComponent<MeshRenderer>();
                TextMeshPro textMeshPro = textObject.AddComponent<TextMeshPro>();

                // Customize the components
                textMeshPro.font = fontAsset;
                textMeshPro.SetText(content);
                textMeshPro.fontSize = 4.5f;

                // Move into clipping zone container
                textObject.transform.SetParent(scrollContainer.transform);

                // Set quad size to fit expected content
                Vector3 textBoxScale = new Vector3
                    (
                        0.05f,
                        0.05f,
                        0.05f
                    );

                textObject.transform.localScale = textBoxScale;

                // Set width and height of container
                RectTransform rect = textObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector3(12.3459f, 32.9851f);

                // Set position
                Vector3 textBoxPosition = new Vector3
                    (
                        0.3568f,
                        -0.8564f,
                        -0.0005f
                    );

                textObject.transform.localPosition = textBoxPosition;
                textObject.transform.rotation      = Quaternion.identity;
                textObject.transform.localRotation = Quaternion.identity;
            }

            scrollView.Reset();
            isDisplayRoutineRunning = false;
            yield return null;
        }

        private void RemoveCurrentContent()
        {
            if (scrollContainer.transform.childCount <= 0)
            {
                return;
            }

            for (int i = scrollContainer.transform.childCount - 1; i >= 0; --i)
            {
                var child = scrollContainer.transform.GetChild(i).gameObject;
                child.transform.SetParent(null);
                Destroy(child);
            }
        }

        private bool isDisplayRoutineRunning;

        protected override void Setup()
        {
            
        }

        protected override void Teardown()
        {
            
        }

        [RegisterPropertyChange(nameof(ContentWindowAssetData.content))]
        private void HandleContentChange(AssetPropertyChangeEventArgs e)
        {
            if ((!isInitialized) ||
                            !GetComponent<IsEnabledAssetTypeComponent>().IsEnabled)
            {
                return;
            }

            string content = assetData.content.runtimeData.Value.text;

            if (!isDisplayRoutineRunning)
                StartCoroutine(DisplayContent(content));
        }

        [RegisterPropertyChange(nameof(ContentWindowAssetData.headerText))]
        private void HandleHeaderTextChange(AssetPropertyChangeEventArgs e)
        {
            headerTextReference.SetText(assetData.headerText.runtimeData.Value);
        }

        public override void SetEditorValues()
        {
            assetData.name.designTimeData.defaultValue        = "Content Window";
            assetData.description.designTimeData.defaultValue = "Displays content.";
            assetData.headerText.designTimeData.defaultValue  = "Content Header";
        }
    }
}