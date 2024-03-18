using Cysharp.Threading.Tasks;
using GIGXR.Platform;
using GIGXR.Platform.Core;
using GIGXR.Platform.UI;
using GIGXR.Platform.UI.HMD;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Provides the available concrete builder implementation to dependencies.
/// </summary>
public class BuilderManager : IBuilderManager
{
    public StyleScriptableObject DefaultStyle
    {
        get
        {
            return defaultStyle;
        }
    }

    private StyleScriptableObject defaultStyle;

    public BuilderManager(ProfileManager profileManager)
    {
        defaultStyle = profileManager.StyleProfile.styleScriptableObject;
    }

    private readonly Vector3 _interactionLevel = new Vector3(0f, 0f, -0.001f);

    public Vector3 InteractionLevelOffset { get { return _interactionLevel; } }

    public GameObject BuildScreenObject()
    {
        GameObject root = new GameObject("Screen Object");

        root.AddComponent<ScreenObject>();

        return root;
    }

    public GameObject BuildBackground()
    {
        GameObject root = new GameObject("Screen Background");

        GameObject backgroundImageObject = new GameObject("Background");
        backgroundImageObject.transform.SetParent(root.transform, false);

        var backgroundImageComponent = backgroundImageObject.AddComponent<Image>();

        backgroundImageComponent.raycastTarget = true;
        backgroundImageComponent.material = defaultStyle.screenMaterial;
        backgroundImageComponent.color = defaultStyle.primaryBackgroundColor;

        var rectTransform = backgroundImageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = defaultStyle.backgroundSize;

        backgroundImageObject.AddComponent<NearInteractionTouchableUnityUI>();

        var borderSize = new Vector2(0.0028f, 0.147f);
        var leftBorder = BuildBorder(borderSize);
        leftBorder.transform.SetParent(root.transform, false);
        leftBorder.transform.position -= new Vector3(defaultStyle.backgroundSize.x / 2.0f, 0, -0.0001f);

        var leftGrip = BuildGrip();
        leftGrip.transform.SetParent(root.transform, false);
        // Extra movement is half of the grip's width
        leftGrip.transform.position -= new Vector3(defaultStyle.backgroundSize.x / 2.0f + 0.01f, 0, 0);

        var leftGripManipulator = leftGrip.GetComponent<ObjectManipulator>();
        leftGripManipulator.HostTransform = root.transform;

        var rightBorder = BuildBorder(borderSize);
        rightBorder.transform.SetParent(root.transform, false);
        rightBorder.transform.position += new Vector3(defaultStyle.backgroundSize.x / 2.0f, 0, -0.0001f);

        var rightGrip = BuildGrip();
        rightGrip.transform.SetParent(root.transform, false);
        rightGrip.transform.position += new Vector3(defaultStyle.backgroundSize.x / 2.0f + 0.01f, 0, 0);

        var rightGripManipulator = rightGrip.GetComponent<ObjectManipulator>();
        rightGripManipulator.HostTransform = root.transform;

        return root;
    }

    public GameObject BuildPanel()
    {
        GameObject panelRoot = new GameObject("Panel");

        var panelImageComponent = panelRoot.AddComponent<Image>();

        panelImageComponent.raycastTarget = false;
        panelImageComponent.material = defaultStyle.panelMaterial;
        panelImageComponent.color = defaultStyle.panelColor;

        var rectTransform = panelRoot.GetComponent<RectTransform>();
        rectTransform.sizeDelta = defaultStyle.panelSize;

        return panelRoot;
    }

    public Slate BuildSlate(string title)
    {
        var slateGameObject = GameObject.Instantiate(defaultStyle.slatePrefab);
        slateGameObject.name = $"Slate: {title}";

        var slate = slateGameObject.GetComponent<Slate>();
        slate.Setup(title);

        return slate;
    }

    public ScrollingObjectCollection BuildScrollingPanel(Vector3? clippingSize = null, float? cellHeight = null, bool collectionIgnoreInactive = true)
    {
        var scrollingGameObject = GameObject.Instantiate(defaultStyle.scrollPrefab);
        var scrollingCollection = scrollingGameObject.GetComponent<ScrollingObjectCollection>();
        var gridCollection = scrollingGameObject.GetComponentInChildren<GridObjectCollection>();

        gridCollection.IgnoreInactiveTransforms = collectionIgnoreInactive;

        if (clippingSize.HasValue)
        {
            var boxCollider = scrollingCollection.GetComponent<BoxCollider>();
            boxCollider.size = clippingSize.Value;

            var nearTouchable = scrollingCollection.GetComponent<NearInteractionTouchable>();
            nearTouchable.SetBounds(new Vector2(clippingSize.Value.x, clippingSize.Value.y));

            var clippingBox = scrollingGameObject.GetComponentInChildren<ClippingBox>(true);
            clippingBox.transform.localScale = clippingSize.Value;
        }

        if (cellHeight.HasValue)
        {
            scrollingCollection.CellHeight = cellHeight.Value;
            gridCollection.CellHeight = cellHeight.Value;
        }

        return scrollingCollection;
    }

    public GameObject BuildBorder(Vector2 size)
    {
        GameObject borderImageObject = new GameObject("Border");

        var borderImageComponent = borderImageObject.AddComponent<Image>();
        borderImageComponent.material = defaultStyle.borderMaterial;
        borderImageComponent.raycastTarget = true;

        var rectTransform = borderImageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;

        return borderImageObject;
    }

    public GameObject BuildGrip()
    {
        GameObject gripObject = new GameObject("Grip");

        var borderImageComponent = gripObject.AddComponent<Image>();
        borderImageComponent.material = defaultStyle.gripMaterial;
        borderImageComponent.raycastTarget = true;

        var rectTransform = gripObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = defaultStyle.gripSize;

        var gripCollider = gripObject.AddComponent<BoxCollider>();
        // TODO data driven
        gripCollider.size = new Vector3(0.02f, 0.1f, 0.01f);

        // TODO Move the right screen
        var objectManipulator = gripObject.AddComponent<ObjectManipulator>();

        // Do not allow the grips to scale
        objectManipulator.TwoHandedManipulationType = Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Move |
                                                      Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Rotate;

        var axisConstraint = gripObject.AddComponent<RotationAxisConstraint>();
        axisConstraint.ConstraintOnRotation = Microsoft.MixedReality.Toolkit.Utilities.AxisFlags.XAxis |
                                              Microsoft.MixedReality.Toolkit.Utilities.AxisFlags.ZAxis;

        gripObject.AddComponent<NearInteractionGrabbable>();
        gripObject.AddComponent<ScreenGrip>();

        return gripObject;
    }

    public GameObject BuildImage(Vector2? size = null, Sprite sprite = null, Color? imageColor = null,
        bool isStretched = false, Transform parent = null)
    {
        GameObject imageObject = new GameObject("Image");

        if (parent != null)
        {
            imageObject.transform.SetParent(parent, false);
        }

        var image = imageObject.AddComponent<Image>();

        if(imageColor.HasValue)
        {
            image.color = imageColor.Value;
        }

        if(isStretched)
        {
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.offsetMin = Vector2.zero;
            image.rectTransform.offsetMax = Vector2.zero;
        }
        else if (size.HasValue)
        {
            image.rectTransform.sizeDelta = size.Value;
        }

        if (sprite != null)
        {
            image.sprite = sprite;
        }

        return imageObject;
    }

    public GameObject BuildTextInput(string defaultText, float height, float width)
    {
        throw new System.NotImplementedException();
    }

    public GameObject BuildMRTKButton(string buttonText, Action buttonClick)
    {
        return BuildMRTKButton(buttonClick, buttonText);
    }

    public GameObject BuildButton(UnityAction buttonClick, string buttonText = "", Vector3? buttonSize = null, Color? backgroundColor = null,
                                  float? fontSize = null, TextAlignmentOptions? alignmentOptions = null, Vector4? textMargins = null,
                                  Material? buttonBackground = null)
    {
        GameObject buttonGameObject = new GameObject("Button");

        var button = buttonGameObject.AddComponent<Button>();
        button.onClick.AddListener(buttonClick);

        var image = buttonGameObject.AddComponent<Image>();
        button.image = image;

        if (buttonBackground != null)
        {
            button.image.material = buttonBackground;
        }

        if (!string.IsNullOrEmpty(buttonText))
        {
            var textGo = BuildText(text: buttonText,
                useUGUI: true,
                fontSize: 16.0f,
                anchor: RectTransformExtensions.AnchorPresets.StretchAll);
            textGo.transform.SetParent(buttonGameObject.transform, false);
        }

        return buttonGameObject;
    }

    public Button BuildSlateButton(UnityAction buttonClick, string buttonText = "", Vector2? buttonSize = null, bool useBackground = false, 
                                   float? fontSize = null, TextAlignmentOptions? alignmentOptions = null,  Vector4? textMargins = null, 
                                   Transform parent = null, RectTransformExtensions.PivotPresets? pivot = null, RectTransformExtensions.AnchorPresets? anchor = null,
                                   Vector3 ? anchorOffset = null)
    {
        var buttonGameObject = GameObject.Instantiate(defaultStyle.slateButtonPrefab);

        if (parent != null)
        {
            buttonGameObject.transform.SetParent(parent, false);
        }

        var button = buttonGameObject.GetComponent<Button>();
        var textMesh = buttonGameObject.GetComponentInChildren<TextMeshProUGUI>();
        var rectTransform = buttonGameObject.GetComponent<RectTransform>();
        var backgroundImage = buttonGameObject.GetComponent<Image>();

        backgroundImage.enabled = useBackground;

        button.onClick.AddListener(buttonClick);

        textMesh.text = buttonText;
        textMesh.fontSize = fontSize ?? 14.0f;
        textMesh.alignment = alignmentOptions ?? TextAlignmentOptions.Center;
        textMesh.font = defaultStyle.defaultFont;
        textMesh.margin = textMargins ?? Vector4.zero;

        if(pivot.HasValue)
        {
            rectTransform.SetPivot(pivot.Value);
        }

        if(anchor.HasValue)
        {
            rectTransform.SetAnchor(anchor.Value);
        }

        if(anchorOffset.HasValue)
        {
            rectTransform.anchoredPosition3D = anchorOffset.Value;
        }

        if (buttonSize.HasValue)
        {
            rectTransform.sizeDelta = buttonSize.Value;
        }

        return button;
    }

    public GameObject BuildMRTKButton(Action buttonClick, string buttonText = "", Vector3? buttonSize = null,
                              ButtonIconStyle? buttonIconStyle = null, Vector3? buttonLocation = null,
                              Color? backgroundColor = null, bool showSeeItSayItLabel = false,
                              float? fontSize = null, TextAlignmentOptions? alignmentOptions = null,
                              Vector4? textMargins = null, Material? quadMaterial = null, string quadIconName = null,
                              Vector3? backplateOffset = null)
    {
        var buttonGameObject = GameObject.Instantiate(defaultStyle.buttonPrefab);

        var buttonHelper = buttonGameObject.GetComponent<ButtonConfigHelper>();
        var pressableButton = buttonGameObject.GetComponent<PressableButtonHoloLens2>();
        var nearInteraction = buttonGameObject.GetComponent<NearInteractionTouchable>();
        // TODO Brittle
        var quadBackground = buttonGameObject.transform.Find("BackPlate").gameObject;
        var quadRenderer = quadBackground.GetComponentInChildren<MeshRenderer>();
        var collider = buttonGameObject.GetComponent<BoxCollider>();

        buttonHelper.MainLabelText = buttonText;
        buttonHelper.SeeItSayItLabelEnabled = showSeeItSayItLabel;
        buttonHelper.IconStyle = buttonIconStyle ?? ButtonIconStyle.None;

        if (!string.IsNullOrEmpty(quadIconName))
        {
            // TODO Brittle
            // Due to how things like Scrolling Object Collections work, the GO for the quad renderer can be turned on outside our control so
            // manage the MeshRenderer component directly to on if this button uses the quad icon set. Otherwise, an unwanted icon will be seen
            var quadMeshRenderer = pressableButton.MovingButtonIconText.transform.Find("UIButtonSquareIcon").GetComponent<MeshRenderer>();
            quadMeshRenderer.enabled = true;

            buttonHelper.SetQuadIconByName(quadIconName);
        }

        buttonHelper.OnClick.AddListener(() => buttonClick?.Invoke());

        var buttonSizeValue = buttonSize ?? defaultStyle.buttonSize;

        // Resize collider on the GO
        // TODO Decide about z axis size
        collider.size = buttonSizeValue;

        FixNearInteractionTouchable(nearInteraction, buttonSizeValue);

        // Resize front plate (Go named FrontPlate)
        // TODO We know there is a child called FrontPlate so just grab it for now
        var frontPlate = pressableButton.CompressableButtonVisuals.transform.GetChild(0);
        frontPlate.localScale = buttonSizeValue;
        pressableButton.CompressableButtonVisuals.transform.localPosition = new Vector3(pressableButton.CompressableButtonVisuals.transform.localPosition.x,
                                                                                        pressableButton.CompressableButtonVisuals.transform.localPosition.y,
                                                                                        buttonSizeValue.z);

        // Resize background (Go named Quad)
        quadBackground.transform.localScale = buttonSizeValue;

        // Set color for the background on the quad, the material should have GPU instancing enabled
        var buttonBackgroundColor = backgroundColor ?? defaultStyle.primaryButtonColor;

        var colorPropertyBlock = new MaterialPropertyBlock();

        colorPropertyBlock.SetColor("_Color", buttonBackgroundColor);

        quadRenderer.SetPropertyBlock(colorPropertyBlock);

        quadRenderer.sharedMaterial = quadMaterial != null ? quadMaterial : defaultStyle.buttonBackgroundMaterial;

        var mainTextObject = pressableButton.MovingButtonIconText.GetComponentInChildren<TMP_Text>(true);

        if (buttonLocation.HasValue)
        {
            pressableButton.MovingButtonIconText.transform.localPosition = buttonLocation.Value;
        }
        else
        {
            pressableButton.MovingButtonIconText.transform.localPosition = Vector3.zero;
        }

        if (backplateOffset.HasValue)
        {
            quadBackground.transform.localPosition = backplateOffset.Value;
        }

        mainTextObject.text = buttonText;
        // When creating a default text box vs button, there is a scale difference so the button needs to multiply the default
        // font size by 10 to get it to look correct
        mainTextObject.fontSize = fontSize ?? (defaultStyle.defaultFontSize * 10.0f);
        mainTextObject.alignment = alignmentOptions ?? TextAlignmentOptions.Center;
        mainTextObject.font = defaultStyle.defaultFont;
        mainTextObject.margin = textMargins ?? Vector4.zero;
        mainTextObject.gameObject.transform.localPosition = Vector3.zero; // TODO Might want to add a TextLocationOffset
        mainTextObject.GetComponent<RectTransform>().sizeDelta = buttonSizeValue;

        // HACK As host, when a UI button is generated, if the scenario is stopped, the AutoSimulate may be disabled so when
        // the button generates the collider, it will be at the correct place but be 'unclickable', so refresh it here
        if (!Physics.autoSimulation)
        {
            collider.enabled = false;

            UniTask.Create(async () =>
            {
                await UniTask.NextFrame();

                if (collider != null)
                    collider.enabled = true;
            });
        }

        return buttonGameObject;
    }

    public GameObject BuildText(string text = "")
    {
        return BuildText(text, null);
    }

    public GameObject BuildText(string text = "", Vector2? textBoxSize = null, Vector3? textLocationOffset = null,
                                Color? textColor = null, float? fontSize = null, TextAlignmentOptions? alignmentOptions = null,
                                Vector4? textMargins = null, FontStyles? textStyle = null, TextOverflowModes? textOverflowMode = null,
                                bool useUGUI = true, RectTransformExtensions.AnchorPresets? anchor = null, Transform parent = null,
                                Vector2? preferedSize = null)
    {
        GameObject textGameObject = new GameObject("Text");

        if(parent != null)
        {
            textGameObject.transform.SetParent(parent, false);
        }

        if (useUGUI)
        {
            var textMesh = textGameObject.AddComponent<TextMeshProUGUI>();

            textMesh.text = text;
            textMesh.fontSize = fontSize ?? defaultStyle.defaultFontSize;
            textMesh.alignment = alignmentOptions ?? TextAlignmentOptions.Center;
            textMesh.font = defaultStyle.defaultFont;
            textMesh.color = textColor ?? defaultStyle.primaryTextColor;
            textMesh.fontStyle = textStyle ?? FontStyles.Normal;
            textMesh.margin = textMargins ?? Vector4.zero;
            textMesh.overflowMode = textOverflowMode ?? TextOverflowModes.Overflow;
        }
        else
        {
            var textMesh = textGameObject.AddComponent<TextMeshPro>();

            textMesh.text = text;
            // Multiple by 10 when not using UGUI for scaling reasons
            textMesh.fontSize = fontSize ?? defaultStyle.defaultFontSize * 10.0f;
            textMesh.alignment = alignmentOptions ?? TextAlignmentOptions.Center;
            textMesh.font = defaultStyle.defaultFont;
            textMesh.color = textColor ?? defaultStyle.primaryTextColor;
            textMesh.fontStyle = textStyle ?? FontStyles.Normal;
            textMesh.margin = textMargins ?? Vector4.zero;
            textMesh.overflowMode = textOverflowMode ?? TextOverflowModes.Overflow;
        }

        var rectTransform = textGameObject.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            if (anchor.HasValue)
            {
                rectTransform.SetAnchor(anchor.Value);
            }
            else
            {
                rectTransform.sizeDelta = textBoxSize ?? defaultStyle.textBoxSize;
            }

            if (textLocationOffset.HasValue)
                rectTransform.localPosition += textLocationOffset.Value;

            if(preferedSize.HasValue)
            {
                var layoutElement = textGameObject.AddComponent<LayoutElement>();
                layoutElement.preferredWidth = preferedSize.Value.x;
                layoutElement.preferredHeight = preferedSize.Value.y;
            }
        }

        return textGameObject;
    }

    public GridObjectCollection BuildObjectCollection(LayoutOrder containerLayout, Vector2 cellSize, CollationOrder sortOrder = CollationOrder.ChildOrder,
                                                      bool ignoreInactiveTransforms = true, LayoutAnchor anchor = LayoutAnchor.MiddleCenter)
    {
        GameObject containerRoot = new GameObject($"{containerLayout} Container");

        var gridCollection = containerRoot.AddComponent<GridObjectCollection>();
        gridCollection.Layout = containerLayout;
        gridCollection.SortType = sortOrder;
        gridCollection.IgnoreInactiveTransforms = ignoreInactiveTransforms;
        gridCollection.CellHeight = cellSize.y;
        gridCollection.CellWidth = cellSize.x;
        gridCollection.Anchor = anchor;

        return gridCollection;
    }

    public GridLayoutGroup BuildObjectCollection(Vector2 cellSize, Vector2 spacing, GridLayoutGroup.Corner corner = GridLayoutGroup.Corner.UpperLeft,
        GridLayoutGroup.Constraint constraints = GridLayoutGroup.Constraint.Flexible, int constraintCount = 1)
    {
        GameObject containerRoot = new GameObject($"Grid Layout Group");

        var gridLayout = containerRoot.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = cellSize;
        gridLayout.spacing = spacing;
        gridLayout.startCorner = corner;
        gridLayout.constraint = constraints;
        gridLayout.constraintCount = constraintCount;

        return gridLayout;
    }

    public VerticalLayoutGroup BuildVerticalCollection(float spacing)
    {
        GameObject containerRoot = new GameObject($"Vertical Layout Group");

        var gridLayout = containerRoot.AddComponent<VerticalLayoutGroup>();
        gridLayout.spacing = spacing;

        return gridLayout;
    }

    public HorizontalLayoutGroup BuildHorizontalCollection(float spacing)
    {
        GameObject containerRoot = new GameObject($"Horizontal Layout Group");

        var gridLayout = containerRoot.AddComponent<HorizontalLayoutGroup>();
        gridLayout.spacing = spacing;

        containerRoot.AddComponent<ContentSizeFitter>();

        return gridLayout;
    }

    public GameObject BuildQuad(Vector2? size = null, Material material = null, bool isVisible = true)
    {
        var quadGameObject = new GameObject("Built Quad");
        var meshFilter = quadGameObject.AddComponent<MeshFilter>();
        var meshRenderer = quadGameObject.AddComponent<MeshRenderer>();

        // Easy quad - https://docs.unity3d.com/Manual/Example-CreatingaBillboardPlane.html
        Mesh quad = new Mesh();

        var sizeToUse = size ?? Vector2.one;

        Vector3[] vertices = new Vector3[4]
        {
                new Vector3(0, 0, 0),
                new Vector3(sizeToUse.x, 0, 0),
                new Vector3(0, sizeToUse.y, 0),
                new Vector3(sizeToUse.x, sizeToUse.y, 0)
        };

        quad.vertices = vertices;

        int[] tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        quad.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        quad.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        quad.uv = uv;

        meshFilter.mesh = quad;

        if (material != null)
            meshRenderer.sharedMaterial = material;

        meshRenderer.enabled = isVisible;

        return quadGameObject;
    }

    // Same Utility used in the NearInteractionTouchableInspector
    private void FixNearInteractionTouchable(NearInteractionTouchable t, Vector3 size)
    {
        BoxCollider bc = t.GetComponent<BoxCollider>();
        RectTransform rt = t.GetComponent<RectTransform>();
        if (bc != null)
        {
            // project size to local coordinate system
            Vector2 adjustedSize = new Vector2(
                        Math.Abs(Vector3.Dot(size, t.LocalRight)),
                        Math.Abs(Vector3.Dot(size, t.LocalUp)));

            // Resize helper
            t.SetBounds(adjustedSize);

            // Recenter helper
            t.SetLocalCenter(bc.center + Vector3.Scale(size / 2.0f, t.LocalForward));
        }
        else if (rt != null)
        {
            // Resize Helper
            t.SetBounds(rt.sizeDelta);

            t.SetLocalForward(new Vector3(0, 0, -1));
        }

        // Perpendicular forward/up vectors helpers
        if (!t.AreLocalVectorsOrthogonal)
        {
            t.SetLocalForward(t.LocalForward);
            t.SetLocalUp(t.LocalUp);
        }
    }
}